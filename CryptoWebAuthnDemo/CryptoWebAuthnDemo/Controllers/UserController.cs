using Microsoft.AspNetCore.Mvc;

namespace CryptoWebAuthnDemo.Controllers
{
    public class UserController: Controller
    {
        public IActionResult Register()
        {
            return this.View();
        }

        public IActionResult Login() => View();
    }
}
