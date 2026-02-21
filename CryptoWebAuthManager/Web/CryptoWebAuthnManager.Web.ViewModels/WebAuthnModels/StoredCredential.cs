using System;
using System.Collections.Generic;
using System.Text;
using Fido2NetLib;
using System;


namespace CryptoWebAuthnManager.Web.ViewModels.WebAuthnModels
{
    public class StoredCredential
    {
        public string UserId { get; set; }

        public byte[] CredentialId { get; set; }

        public byte[] PublicKey { get; set; }

        public uint SignatureCounter { get; set; }

        public string CredType { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
