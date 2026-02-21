namespace CryptoWebAuthnManager.Services.Data
{
    using CryptoWebAuthnManager.Data;
    using CryptoWebAuthnManager.Data.Models;
    using CryptoWebAuthnManager.Web.ViewModels.WebAuthnModels;
    using Fido2NetLib;
    using Fido2NetLib.Objects;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using static Fido2NetLib.AuthenticatorAssertionRawResponse;

    public class WebAuthnService : IWebAuthnService
    {
        private readonly Fido2 _fido2;
        private readonly ApplicationDbContext context;

        public WebAuthnService(ApplicationDbContext context, Fido2 fido2)
        {
            this.context = context;
            this._fido2 = fido2;
        }

        public async Task<CredentialCreateOptions> GenerateRegistrationOptionsAsync(string userId, string username)
        {
            // 1️⃣ Генерираме user.Id с минимум 16 байта
            byte[] userIdBytes = Encoding.UTF8.GetBytes(userId);
            if (userIdBytes.Length < 16)
            {
                // padding, ако е по-малко от 16
                var padded = new byte[16];
                Array.Copy(userIdBytes, padded, userIdBytes.Length);
                userIdBytes = padded;
            }

            var user = new Fido2User
            {
                DisplayName = username,
                Name = username,
                Id = userIdBytes
            };

            // 2️⃣ Параметри за нов credential
            var requestNewCredentialParams = new RequestNewCredentialParams
            {
                User = user,
                AuthenticatorSelection = new AuthenticatorSelection
                {
                    RequireResidentKey = false,
                    UserVerification = UserVerificationRequirement.Preferred
                },
                AttestationPreference = AttestationConveyancePreference.None,
                PubKeyCredParams = new List<PubKeyCredParam>
                {
                    new PubKeyCredParam(COSE.Algorithm.ES256,PublicKeyCredentialType.PublicKey),
                    new PubKeyCredParam(COSE.Algorithm.RS256,PublicKeyCredentialType.PublicKey),
                },
            };

            // 3️⃣ Създаваме CredentialCreateOptions
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

        public AssertionOptions GetCredentialsForUser(string userId)
        {
            var userCredentials = context.WebAuthnCredentials
                .Where(c => c.UserId == userId)
                .Select(c => new StoredCredential
                {
                    UserId = c.UserId,
                    CredentialId = c.CredentialId,
                    PublicKey = c.PublicKey,
                    SignatureCounter = c.SignatureCounter,
                    CredType = c.CredType,
                    CreatedOn = c.CreatedOn
                })
                .ToList();

            var pubKey = new List<PublicKeyCredentialDescriptor>();
            var getAssertionOptionsParams = new GetAssertionOptionsParams()
            {
               AllowedCredentials = userCredentials
                   .Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))
                   .ToList(),
               UserVerification = UserVerificationRequirement.Preferred,
            };

            var options = _fido2.GetAssertionOptions(getAssertionOptionsParams);

            return options;
        }

        public async Task<AssertionServiceResult> GetCredentialById(AuthenticatorAssertionRawResponse assertionResponse, string challenge)
        {
            var credentialId = assertionResponse.RawId;
            var c = context.WebAuthnCredentials
                .FirstOrDefault(x => x.CredentialId.SequenceEqual(credentialId));

            if (c == null)
                return null;

            if (challenge == null)
                throw new Exception("Assertion session expired");

            var originalOptions = AssertionOptions.FromJson(challenge);

            var makeAssertionParams = new MakeAssertionParams
            {
                AssertionResponse = assertionResponse,
                StoredPublicKey = c.PublicKey,
                StoredSignatureCounter = c.SignatureCounter,
                OriginalOptions = originalOptions,
                IsUserHandleOwnerOfCredentialIdCallback = async (args, cancellationToken) =>
                {
                    // args.UserHandle -> byte[]
                    // args.CredentialId -> byte[]

                    var userHandle = Encoding.UTF8.GetString(args.UserHandle);

                    var credential = context.WebAuthnCredentials
                        .FirstOrDefault(x =>
                            x.UserId == userHandle &&
                            x.CredentialId.SequenceEqual(args.CredentialId));

                    return await Task.FromResult(credential != null);
                }
            };

            // 4️⃣ извършваме проверката
            var result = await _fido2.MakeAssertionAsync(makeAssertionParams);

            // 5️⃣ update counter (МНОГО ВАЖНО)
            c.SignatureCounter = result.SignCount;
            context.SaveChanges();

            return new AssertionServiceResult()
            {
                Result = result,
                UserId = c.UserId
            };
        }
    }
}
