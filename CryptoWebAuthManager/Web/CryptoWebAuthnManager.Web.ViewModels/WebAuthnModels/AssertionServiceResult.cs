using Fido2NetLib.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoWebAuthnManager.Web.ViewModels.WebAuthnModels
{
    public class AssertionServiceResult
    {
        public VerifyAssertionResult Result { get; set; }
        public string UserId { get; set; }
    }
}
