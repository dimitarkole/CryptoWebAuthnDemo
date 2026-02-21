//using CryptoWebAuthnManager.Services.Data;
//using CryptoWebAuthnManager.Services.Messaging;
//using CryptoWebAuthnManager.Web.Tests.Mocks;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Caching.Distributed;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.FileProviders;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

//namespace CryptoWebAuthnManager.Web.Tests
//{
//    public class TestStartup : StartUp
//    {
//        private const string configurationFileName = "appsettings.Tests.json";

//        public TestStartup(IConfiguration configuration) : base(configuration)
//        {
//            Configuration = new ConfigurationBuilder()
//               .AddJsonFile(configurationFileName)
//               .Build();
//        }

//        public void ConfigureTestServices(IServiceCollection services)
//        {
//            base.ConfigureServices(services);
//            services.AddTransient<IEmailSender, NullMessageSender>();
//            services.AddTransient<IWebAuthnService, WebAuthnService>();
//        }
//    }
//}
