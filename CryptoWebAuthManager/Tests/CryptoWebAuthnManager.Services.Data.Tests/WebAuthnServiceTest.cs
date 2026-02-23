namespace CryptoWebAuthnManager.Services.Data.Tests
{
    using CryptoWebAuthnManager.Data;
    using CryptoWebAuthnManager.Data.Common.Repositories;
    using CryptoWebAuthnManager.Data.Models;
    using Fido2NetLib;
    using Fido2NetLib.Objects;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using System;
    using System.Buffers.Text;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class WebAuthnServiceTests : BaseTestClass
    {
        [Fact]
        public async Task GenerateRegistrationOptionsAsync_Should_Return_Valid_Options()
        {
            // Arrange
            var service = CreateService();
            var userId = "12345";
            var username = "dimitar";

            // Act
            var result = await service.GenerateRegistrationOptionsAsync(userId, username);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.User);
            Assert.Equal(username, result.User.Name);
            Assert.Equal(username, result.User.DisplayName);

            Assert.True(result.User.Id.Length >= 16);

            Assert.NotNull(result.Challenge);
            Assert.NotEmpty(result.Challenge);

            Assert.Contains(result.PubKeyCredParams,
                x => x.Alg == COSE.Algorithm.ES256);

            Assert.Contains(result.PubKeyCredParams,
                x => x.Alg == COSE.Algorithm.RS256);
        }

        [Fact]
        public async Task CompleteRegistrationAsync_Should_Throw_With_Invalid_Attestation()
        {
            // Arrange
            var service = CreateService();

            var userId = "1234567890123456";
            var username = "dimitar";

            var originalOptions = await service
                .GenerateRegistrationOptionsAsync(userId, username);

            var attestationResponse = new AuthenticatorAttestationRawResponse
            {
                RawId = new byte[] { 1, 2, 3, 4 },
                Id = "AQIDBA",
                Type = PublicKeyCredentialType.PublicKey
            };

            // Act & Assert
            await Assert.ThrowsAsync<Fido2VerificationException>(async () =>
            {
                await service.CompleteRegistrationAsync(
                    userId,
                    attestationResponse,
                    originalOptions);
            });

            Assert.Empty(context.WebAuthnCredentials);
        }

        [Fact]
        public void GetCredentialsForUser_Should_Return_AssertionOptions_With_AllowCredentials()
        {
            // Arrange
            var service = CreateService();
            var userId = "123";

            context.WebAuthnCredentials.Add(new WebAuthnCredential
            {
                UserId = userId,
                CredentialId = new byte[] { 1, 2, 3 },
                PublicKey = new byte[] { 10, 20, 30 },
                SignatureCounter = 0,
                CredType = "public-key",
                CreatedOn = DateTime.UtcNow
            });

            context.SaveChanges();

            // Act
            var options = service.GetCredentialsForUser(userId);

            // Assert
            Assert.NotNull(options);
            Assert.NotNull(options.AllowCredentials);
            Assert.Single(options.AllowCredentials);

            var descriptor = options.AllowCredentials.First();
            Assert.Equal(new byte[] { 1, 2, 3 }, descriptor.Id);
        }

        [Fact]
        public async Task GetCredentialById_Should_Return_Null_When_Credential_Not_Found()
        {
            // Arrange
            var service = CreateService();

            var assertionResponse = new AuthenticatorAssertionRawResponse
            {
                RawId = new byte[] { 1, 2, 3 }
            };

            // Act
            var result = await service.GetCredentialById(assertionResponse, "{}");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCredentialById_Should_Throw_When_Challenge_Is_Null()
        {
            // Arrange
            var service = CreateService();

            var credentialId = new byte[] { 1, 2, 3 };

            context.WebAuthnCredentials.Add(new WebAuthnCredential
            {
                UserId = "123",
                CredentialId = credentialId,
                PublicKey = new byte[] { 10, 20 },
                SignatureCounter = 1,
                CredType = "public-key",
                CreatedOn = DateTime.UtcNow
            });

            context.SaveChanges();

            var assertionResponse = new AuthenticatorAssertionRawResponse
            {
                RawId = credentialId
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.GetCredentialById(assertionResponse, null));

            Assert.Equal("Assertion session expired", ex.Message);
        }

        [Fact]
        public async Task GetCredentialById_Should_Not_Update_Counter_When_Assertion_Fails()
        {
            // Arrange
            var service = CreateService();
            var credentialId = new byte[] { 1, 2, 3 };
            context.WebAuthnCredentials.Add(new WebAuthnCredential
            {
                UserId = "123",
                CredentialId = credentialId,
                PublicKey = new byte[] { 10, 20 },
                SignatureCounter = 1,
                CredType = "public-key",
                CreatedOn = DateTime.UtcNow
            });

            context.SaveChanges();

            var assertionResponse = new AuthenticatorAssertionRawResponse
            {
                RawId = credentialId
            };

            var fakeChallenge = "{}";
            try
            {
                await service.GetCredentialById(assertionResponse, fakeChallenge);
            }
            catch
            {
            }

            // Assert – counter не трябва да се е променил
            var credential = context.WebAuthnCredentials.First();
            Assert.Equal(1L, credential.SignatureCounter);
        }

        private WebAuthnService CreateService()
        {
            var fido2Config = new Fido2Configuration
            {
                ServerDomain = "localhost",
                ServerName = "Test Server",
                Origins = new HashSet<string> { "https://localhost:44320" },
            };

            var fido2 = new Fido2(fido2Config);

            return new WebAuthnService(context, fido2);
        }
    }
}
