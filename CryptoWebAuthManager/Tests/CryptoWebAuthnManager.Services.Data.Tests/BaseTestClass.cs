namespace CryptoWebAuthnManager.Services.Data.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using CryptoWebAuthnManager.Data;
    using CryptoWebAuthnManager.Services.Data.Tests.ClassFixtures;
    using CryptoWebAuthnManager.Services.Data.Tests.Factories;
    using Xunit;

    public class BaseTestClass : IClassFixture<MappingsProvider>
    {
        protected readonly ApplicationDbContext context;

        public BaseTestClass()
        {
            this.context = ApplicationDbContextFactory.CreateInMemoryDatabase();
        }
    }
}
