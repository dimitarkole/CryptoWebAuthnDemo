using Microsoft.AspNetCore.Identity;

namespace CryptoWebAuthnDemo.Database.Models
{
    public class ApplicationUser: IdentityUser
    {
        public int Id { get; set; }
        public List<WebAuthnCredential> Credentials { get; set; } = new();
    }
}
