namespace CryptoWebAuthnManager.Web.Controllers
{
    using CryptoWebAuthnManager.Services.Data;
    using CryptoWebAuthnManager.Web.ViewModels.WebAuthnModels;
    using Fido2NetLib;
    using Fido2NetLib.Objects;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity.Data;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [Route("WebAuthn")]
    public class WebAuthnController : BaseController
    {
        private readonly IWebAuthnService _webAuthnService;

        public WebAuthnController(IWebAuthnService webAuthnService)
        {
            _webAuthnService = webAuthnService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return this.View();
        }
        /// <summary>
        /// Генерира registration options (challenge, rp, user, pubKeyCredParams и т.н.)
        /// Извиква се от клиента преди navigator.credentials.create()
        /// </summary>

        [HttpGet("RegisterOptions")]
        public async Task<IActionResult> RegisterOptions([FromQuery] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username is required.");

            var options = await _webAuthnService.GenerateRegistrationOptionsAsync(username, username);

            // Запазваме challenge/options в session
            HttpContext.Session.SetString("fido2.register.options", options.ToJson());

            // Връщаме правилен JSON за браузъра (camelCase)
            return Content(options.ToJson(), "application/json");
        }

        ///// <summary>
        ///// Приема attestation response от клиента и финализира регистрацията
        ///// Извиква се след navigator.credentials.create()
        ///// </summary>
        [HttpPost("RegisterComplete")]
        public async Task<IActionResult> RegisterComplete([FromBody] AuthenticatorAttestationRawResponse attestationResponse)
        {
            var jsonOptions = HttpContext.Session
                .GetString("fido2.register.options");

            if (jsonOptions == null)
                return BadRequest("Registration session expired.");

            var originalOptions = CredentialCreateOptions
                .FromJson(jsonOptions);

            var userId = Encoding.UTF8.GetString(originalOptions.User.Id);
            var result = await _webAuthnService
                .CompleteRegistrationAsync(
                    userId,
                    attestationResponse,
                    originalOptions);

            if (!result)
                return BadRequest("Registration failed.");

            return Ok(new { Success = true });
        }

        [HttpPost("LoginOptions")]
        public IActionResult LoginOptions([FromBody] LoginRequestModel model)
        {
            var options = _webAuthnService.GetCredentialsForUser(model.Username);

            // Записваме ЦЯЛОТО options
            HttpContext.Session.SetString(
                "fido2.assertion.options",
                options.ToJson()
            );

            return Json(options);
        }

        // 2️⃣ Проверка на отговора от клиента
        [HttpGet("LoginComplete")]
        public async Task<IActionResult> LoginComplete([FromBody] AuthenticatorAssertionRawResponse assertionResponse)
        {
            var optionsJson = HttpContext.Session
                .GetString("fido2.assertion.options");

            if (optionsJson == null)
                return BadRequest("Assertion session expired.");

            try
            {
                var res = await _webAuthnService
                    .GetCredentialById(assertionResponse, optionsJson);

                if (res == null)
                    return BadRequest("Unknown credential.");

                HttpContext.Session.SetString("user", res.UserId);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest($"Assertion failed: {ex.Message}");
            }
        }
    }
}
