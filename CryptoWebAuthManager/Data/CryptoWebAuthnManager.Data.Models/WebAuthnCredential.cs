using CryptoWebAuthnManager.Data.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoWebAuthnManager.Data.Models
{
    public class WebAuthnCredential : BaseDeletableModel<string>
    {
        public WebAuthnCredential()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public string UserId { get; set; }

        public byte[] CredentialId { get; set; }

        public byte[] PublicKey { get; set; }

        public uint SignatureCounter { get; set; }

        public string CredType { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}