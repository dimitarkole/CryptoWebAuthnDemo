namespace CryptoWebAuthnManager.Services.Data
{
    using CryptoWebAuthnManager.Data;
    using CryptoWebAuthnManager.Data.Common.Repositories;
    using CryptoWebAuthnManager.Data.Models;
    using Fido2NetLib;
    using Fido2NetLib.Objects;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class WebAuthnService : IWebAuthnService
    {
        private readonly IFido2 _fido2;
        private readonly ApplicationDbContext context;

        public WebAuthnService(ApplicationDbContext context, IFido2 fido2)
        {
            this.context = context;
            this._fido2 = fido2;
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
                var exists = context.WebAuthnCredentials
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

            await this.context.WebAuthnCredentials.AddAsync(credential);
            await this.context.SaveChangesAsync();

            return true;
        }
    }
}
