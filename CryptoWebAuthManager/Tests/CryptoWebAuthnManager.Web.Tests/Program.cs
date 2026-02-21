//namespace CryptoWebAuthnManager.Web
//{
//    using Antlr.Runtime;
//    using CryptoWebAuthnManager.Data;
//    using CryptoWebAuthnManager.Data.Common;
//    using CryptoWebAuthnManager.Data.Common.Repositories;
//    using CryptoWebAuthnManager.Data.Models;
//    using CryptoWebAuthnManager.Data.Repositories;
//    using CryptoWebAuthnManager.Data.Seeding;
//    using CryptoWebAuthnManager.Services.Data;
//    using CryptoWebAuthnManager.Services.Mapping;
//    using CryptoWebAuthnManager.Services.Messaging;
//    using CryptoWebAuthnManager.Web;
//    using CryptoWebAuthnManager.Web.ViewModels;
//    using Microsoft.AspNetCore.Builder;
//    using Microsoft.AspNetCore.Hosting;
//    using Microsoft.AspNetCore.Http;
//    using Microsoft.AspNetCore.Mvc;
//    using Microsoft.EntityFrameworkCore;
//    using Microsoft.EntityFrameworkCore.Diagnostics;
//    using Microsoft.Extensions.Configuration;
//    using Microsoft.Extensions.DependencyInjection;
//    using Microsoft.Extensions.Hosting;
//    using Microsoft.Extensions.Logging;
//    using System;
//    using System.Diagnostics;
//    using System.IO;
//    using System.Reflection;
//    using System.Threading.Tasks;

//    public static class Program
//    {
//        public static void main(string[] args)
//        {
//            Console.WriteLine($"{typeof(Program).Namespace} ({string.Join(" ", args)}) starts working...");
//            var serviceCollection = new ServiceCollection();
//            ConfigureServices(serviceCollection);
//            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider(true);

//            // Seed data on application startup
//            using (var serviceScope = serviceProvider.CreateScope())
//            {
//                var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                dbContext.Database.Migrate();
//                new ApplicationDbContextSeeder().SeedAsync(dbContext, serviceScope.ServiceProvider).GetAwaiter().GetResult();
//            }
//        }

//        private static void ConfigureServices(ServiceCollection services)
//        {
//            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
//                .AddJsonFile("appsettings.json", false, true)
//                .AddEnvironmentVariables()
//                .Build();

//            services.AddSingleton<IConfiguration>(configuration);

//            services.AddDbContext<ApplicationDbContext>(
//                options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
//                    .UseLoggerFactory(new LoggerFactory()));

//            services.AddDefaultIdentity<ApplicationUser>(IdentityOptionsProvider.GetIdentityOptions)
//                .AddRoles<ApplicationRole>().AddEntityFrameworkStores<ApplicationDbContext>();

//            services.AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>));
//            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
//            services.AddScoped<IDbQueryRunner, DbQueryRunner>();

//            // Application services
//            services.AddTransient<IEmailSender, NullMessageSender>();
//            services.AddTransient<IWebAuthnService, WebAuthnService>();
//        }
//    }
//}
