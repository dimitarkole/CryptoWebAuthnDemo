namespace CryptoWebAuthnManager.Services.Data.Tests
{
    using CryptoWebAuthnManager.Data;
    using CryptoWebAuthnManager.Data.Common.Repositories;
    using CryptoWebAuthnManager.Data.Models;
    using Fido2NetLib;
    using Fido2NetLib.Objects;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class WebAuthnServiceTests : BaseTestClass
    {
        private readonly Mock<Fido2> _fido2Mock;
        private readonly WebAuthnService _service;

        public WebAuthnServiceTests()
        {
            _fido2Mock = new Mock<Fido2>();
            _service = new WebAuthnService(context, _fido2Mock.Object);
        }

        [Fact]
        public async Task GenerateRegistrationOptionsAsync_Should_Create_Correct_User_And_Call_Fido()
        {
            var userId = "123";
            var username = "dimitar";
            var expectedOptions = CreateValidCredentialCreateOptions(userId, username);

            _fido2Mock
                .Setup(x => x.RequestNewCredential(It.IsAny<RequestNewCredentialParams>()))
                .Returns(expectedOptions);

            // Act
            var result = await _service.GenerateRegistrationOptionsAsync(userId, username);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedOptions, result);

            _fido2Mock.Verify(x => x.RequestNewCredential(
                It.Is<RequestNewCredentialParams>(p =>
                    p.User.Name == username &&
                    p.User.DisplayName == username &&
                    p.User.Id.SequenceEqual(Encoding.UTF8.GetBytes(userId))
                )), Times.Once);
        }

        [Fact]
        public async Task CompleteRegistrationAsync_Should_Save_Credential_And_Return_True()
        {
            // Arrange
            var userId = "123";
            var username = "dimitar";
            var originalOptions = CreateValidCredentialCreateOptions(userId, username);

            var attestationResponse = new AuthenticatorAttestationRawResponse();

            var credentialId = new byte[] { 10, 20, 30 };
            var publicKey = new byte[] { 1, 2, 3 };

            var fidoResult = new RegisteredPublicKeyCredential()
            {
                Id = credentialId,
                PublicKey = publicKey,
                SignCount = 5,
                Type = PublicKeyCredentialType.PublicKey
            };

            _fido2Mock
                .Setup(x => x.MakeNewCredentialAsync(
                    It.IsAny<MakeNewCredentialParams>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(fidoResult);

            // Act
            var result = await _service.CompleteRegistrationAsync(
                userId,
                attestationResponse,
                originalOptions);

            // Assert
            Assert.True(result);

            var savedCredential = context.WebAuthnCredentials.FirstOrDefault();

            Assert.NotNull(savedCredential);
            Assert.Equal(userId, savedCredential.UserId);
            Assert.True(savedCredential.CredentialId.SequenceEqual(credentialId));
            Assert.True(savedCredential.PublicKey.SequenceEqual(publicKey));
            Assert.Equal(5u, savedCredential.SignatureCounter);

            _fido2Mock.Verify(x => x.MakeNewCredentialAsync(
                It.IsAny<MakeNewCredentialParams>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private CredentialCreateOptions CreateValidCredentialCreateOptions(string userId, string username)
        {
            return new CredentialCreateOptions
            {
                User = new Fido2User()
                {
                    Id = Encoding.UTF8.GetBytes(userId),
                    Name = username,
                    DisplayName = username,
                },
                Rp = new PublicKeyCredentialRpEntity("test", "localhost"),
                PubKeyCredParams = new List<PubKeyCredParam>
                {
                    new PubKeyCredParam(COSE.Algorithm.ES256,PublicKeyCredentialType.PublicKey)
                },
                Challenge = new byte[] { 1, 2, 3 }
            };
        }
    }
}
