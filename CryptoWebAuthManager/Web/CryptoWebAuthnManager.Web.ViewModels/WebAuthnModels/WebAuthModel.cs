using Fido2NetLib;
using Fido2NetLib.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace CryptoWebAuthnManager.Web.ViewModels.WebAuthnModels
{
    public class WebAuthnAuthenticatorAttestationRawResponse
    {
        public string Id { get; init; }

        public byte[] RawId { get; init; }

        public WebAuthnAttestationResponse Response { get; init; }

        public AuthenticationExtensionsClientOutputs Extensions
        {
            get => ClientExtensionResults;
            set => ClientExtensionResults = value;
        }

        public AuthenticationExtensionsClientOutputs ClientExtensionResults { get; set; }

        public sealed class WebAuthnAttestationResponse
        {
            public required byte[] AttestationObject { get; init; }
            public required byte[] ClientDataJson { get; init; }

            public AuthenticatorTransport[] Transports { get; init; }
        }
    }
}
