using CryptoWebAuthnDemo.Data;
using CryptoWebAuthnDemo.Data.Migrations;
using CryptoWebAuthnDemo.Database.Models;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace CryptoWebAuthnDemo.Controllers
{
    [Route("Register")]
    public class RegisterConroller: Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly Fido2NetLib.Fido2 _fido2;

        public RegisterConroller(ApplicationDbContext db, Fido2NetLib.Fido2 fido2)
        {
            _db = db;
            _fido2 = fido2;
        }

        [HttpPost("Options")] // POST /register/options
        public async Task<IActionResult> Options([FromBody] CryptoWebAuthnDemo.Models.RegisterRequest request)
        {
            var user = _db.Users.FirstOrDefault(u => u.UserName == request.UserName);
            if (user == null)
            {
                user = new ApplicationUser { UserName = request.UserName };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }

            var fido2 = _fido2; // Injected
            var options = fido2.RequestNewCredential(
                new Fido2User { Name = user.UserName, DisplayName = user.UserName, Id = Encoding.UTF8.GetBytes(user.Id.ToString()) },
                new List<PublicKeyCredentialDescriptor>(), // excludeCredentials
                authenticatorSelection: null,
                attestationPreference: AttestationConveyancePreference.None
            );

            HttpContext.Session.SetString("fido2.options", JsonSerializer.Serialize(options));

            return Ok(options);
        }


        [HttpPost("Complete")]
        public async Task<IActionResult> Complete([FromBody] AuthenticatorAttestationRawResponse attestationResponse)
        {
            var optionsJson = HttpContext.Session.GetString("fido2.options");
            var options = JsonSerializer.Deserialize<CredentialCreateOptions>(optionsJson);

            var result = await _fido2.MakeNewCredentialAsync(attestationResponse, options, (args) => Task.FromResult(true));

            var user = await _db.Users.FindAsync(result.Result.User.Id);
            var credential = new WebAuthnCredential
            {
                UserId = user.Id,
                CredentialId = result.Result.CredentialId,
                PublicKey = result.Result.PublicKey,
                //SignCount = result.Result.Counter
            };

            _db.WebAuthnCredential.Add(credential);
            await _db.SaveChangesAsync();

            return Ok(result);
        }
      

        // По-късно можеш да добавиш и login endpoints:
        // [HttpPost("login/options")]
        // [HttpPost("login/complete")]
    }
}
