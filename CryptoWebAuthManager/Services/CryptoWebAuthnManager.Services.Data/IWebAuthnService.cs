namespace CryptoWebAuthnManager.Services.Data
{
    using CryptoWebAuthnManager.Web.ViewModels.WebAuthnModels;
    using Fido2NetLib;
    using Fido2NetLib.Objects;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public interface IWebAuthnService
    {
        Task<CredentialCreateOptions> GenerateRegistrationOptionsAsync(string userId, string username);
        Task<bool> CompleteRegistrationAsync(string userId, AuthenticatorAttestationRawResponse attestationResponse, CredentialCreateOptions originalOptions);
        AssertionOptions GetCredentialsForUser(string userId);
        Task<AssertionServiceResult> GetCredentialById(AuthenticatorAssertionRawResponse assertionResponse, string challenge);
    }
}
