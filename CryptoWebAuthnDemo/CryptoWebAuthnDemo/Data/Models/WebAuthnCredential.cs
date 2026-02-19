namespace CryptoWebAuthnDemo.Database.Models
{
    public class WebAuthnCredential
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public byte[] CredentialId { get; set; }
        public byte[] PublicKey { get; set; }
        public uint SignatureCounter { get; set; }
    }
}
