namespace CryptoWebAuthnManager.Web
{
    using CryptoWebAuthnManager.Data;
    using CryptoWebAuthnManager.Data.Common;
    using CryptoWebAuthnManager.Data.Common.Repositories;
    using CryptoWebAuthnManager.Data.Models;
    using CryptoWebAuthnManager.Data.Repositories;
    using CryptoWebAuthnManager.Data.Seeding;
    using CryptoWebAuthnManager.Services.Data;
    using CryptoWebAuthnManager.Services.Mapping;
    using CryptoWebAuthnManager.Services.Messaging;
    using CryptoWebAuthnManager.Web.ViewModels;
    using Fido2NetLib;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder.Services, builder.Configuration);
            var app = builder.Build();
            Configure(app);
            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.CommandTimeout(180))
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

            services.AddDefaultIdentity<ApplicationUser>(IdentityOptionsProvider.GetIdentityOptions)
                .AddRoles<ApplicationRole>().AddEntityFrameworkStores<ApplicationDbContext>();

            services.Configure<CookiePolicyOptions>(
                options =>
                {
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                });

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages();
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddSingleton(configuration);

            // WebOptimizer (bundling and minification)
            services.AddWebOptimizer(pipeline =>
            {
                pipeline.AddCssBundle("/css/site.min.css", "css/site.css");
                pipeline.AddJavaScriptBundle("/js/site.min.js", "js/site.js");
            });
            services.AddControllersWithViews().AddNewtonsoftJson();

            //services.AddFido2(options =>
            //{
            //    options.ServerDomain = configuration["fido2:serverDomain"];
            //    options.ServerName = "Okta WebAuthn Demo";
            //    options.Origins = new HashSet<string> { configuration["fido2:origin"] };
            //    options.TimestampDriftTolerance = configuration.GetValue<int>("fido2:timestampDriftTolerance");
            //});

            // Data repositories
            services.AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IDbQueryRunner, DbQueryRunner>();
            services.AddSingleton(new Fido2(new Fido2Configuration
            {
                ServerDomain = "localhost",
                ServerName = "Demo WebAuthn API",
                Origins = new HashSet<string> { "https://localhost:44320" }
            }));
            // Application services
            services.AddTransient<IEmailSender, NullMessageSender>();
            services.AddTransient<IWebAuthnService, WebAuthnService>();
        }

        private static void Configure(WebApplication app)
        {
            // Seed data on application startup
            using (var serviceScope = app.Services.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
                new ApplicationDbContextSeeder().SeedAsync(dbContext, serviceScope.ServiceProvider).GetAwaiter().GetResult();
            }

            AutoMapperConfig.RegisterMappings(typeof(ErrorViewModel).GetTypeInfo().Assembly);

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.MapControllers();                // нужно за Route атрибутите

            app.UseHttpsRedirection();
            app.UseWebOptimizer();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute("areaRoute", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();
        }
    }
}
