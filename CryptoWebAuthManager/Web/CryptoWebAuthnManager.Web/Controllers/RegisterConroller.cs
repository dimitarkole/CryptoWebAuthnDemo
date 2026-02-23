using CryptoWebAuthnManager.Data;
using CryptoWebAuthnManager.Data.Migrations;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CryptoWebAuthnManager.Web.ViewModels.WebAuthnModels;

namespace CryptoWebAuthnManager.Controllers
{
    [Route("Register")]
    public class RegisterConroller: Controller
    {
        [HttpGet("Test")]
        public async Task<IActionResult> Test()
        {
            var result = true;
            return Ok(result);
        }

        [HttpPost("TestPost")]
        public async Task<IActionResult> TestPost()
        {
            var result = true;
            return Ok(result);
        }
    }
}
