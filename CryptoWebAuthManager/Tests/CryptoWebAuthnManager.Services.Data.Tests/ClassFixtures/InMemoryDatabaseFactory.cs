namespace CryptoWebAuthnManager.Services.Data.Tests.ClassFixtures
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using CryptoWebAuthnManager.Data;
    using CryptoWebAuthnManager.Services.Data.Tests.Factories;

    public class InMemoryDatabaseFactory
    {
        public ApplicationDbContext Context { get; private set; }

        public InMemoryDatabaseFactory()
        {
            Context = ApplicationDbContextFactory.CreateInMemoryDatabase();
        }

        public void Dispose() => Context.Dispose();
    }
}
