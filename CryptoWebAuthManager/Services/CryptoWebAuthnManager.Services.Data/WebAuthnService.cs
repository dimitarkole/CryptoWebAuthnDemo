namespace CryptoWebAuthnManager.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using CryptoWebAuthnManager.Data.Common.Repositories;
    using CryptoWebAuthnManager.Data.Models;
    using Fido2NetLib;
    using Fido2NetLib.Objects;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;

    public class WebAuthnService : IWebAuthnService
    {
        private readonly Fido2 _fido2;
        private readonly IRepository<WebAuthnCredential> credentialsRepo;

        public WebAuthnService(
            IConfiguration config,
            IRepository<WebAuthnCredential> credentialsRepo)
        {
            var fidoConfig = new Fido2Configuration
            {
                ServerDomain = config["Fido:ServerDomain"],
                ServerName = config["Fido:ServerName"],
                Origins = new HashSet<string> { config["Fido:Origin"] }
            };

            this._fido2 = new Fido2(fidoConfig);
            this.credentialsRepo = credentialsRepo;
        }

        public async Task<CredentialCreateOptions> GenerateRegistrationOptionsAsync(string userId, string username)
        {
            var user = new Fido2User
            {
                DisplayName = username,
                Name = username,
                Id = Encoding.UTF8.GetBytes(userId)
            };

            var requestNewCredentialParams = new RequestNewCredentialParams()
            {
                User = user,
            };

            return _fido2.RequestNewCredential(requestNewCredentialParams);
        }

        public async Task<bool> CompleteRegistrationAsync(
            string userId,
            AuthenticatorAttestationRawResponse attestationResponse,
            CredentialCreateOptions originalOptions)
        {
            // Callback за уникалност
            async Task<bool> Callback(IsCredentialIdUniqueToUserParams args, CancellationToken ct)
            {
                var exists = credentialsRepo
                    .All()
                    .Any(c => c.CredentialId.SequenceEqual(args.CredentialId));

                return !exists;
            }

            var success = await _fido2.MakeNewCredentialAsync(
                new MakeNewCredentialParams()
                {
                    AttestationResponse = attestationResponse,
                    OriginalOptions = originalOptions,
                    IsCredentialIdUniqueToUserCallback = Callback,
                },
                CancellationToken.None);

            var credential = new WebAuthnCredential
            {
                UserId = userId,
                CredentialId = success.Id,
                PublicKey = success.PublicKey,
                SignatureCounter = success.SignCount,
                CredType = success.Type.ToString(),
                CreatedOn = DateTime.UtcNow
            };

            await credentialsRepo.AddAsync(credential);
            await credentialsRepo.SaveChangesAsync();

            return true;
        }
    }
}
