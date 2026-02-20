namespace CryptoWebAuthnManager.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using Fido2NetLib;

    public interface IWebAuthnService
    {
        Task<CredentialCreateOptions> GenerateRegistrationOptionsAsync(string userId, string username);
        Task<bool> CompleteRegistrationAsync(string userId, AuthenticatorAttestationRawResponse attestationResponse, CredentialCreateOptions originalOptions);
    }
}
