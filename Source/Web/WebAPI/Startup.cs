using AuthPermissions;
using AuthPermissions.AspNetCore;
using AuthPermissions.AspNetCore.Services;
using AuthPermissions.SetupCode;
using Common.EnvironmentService;
using Infrastructure.CQRS;
using Infrastructure.Identity;
using Infrastructure.Identity.Types.Overrides;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Infrastructure.Services.TenantApplicationUserManager;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddScoped<IEnvironmentService, ServerEnvironmentService>();
            services.AddScoped<TenantManager>();
            services.AddScoped<TenantApplicationUserManager>();
            services.AddCQRS(GetType().Assembly);

            //services.AddDbContext<ApplicationDbContext>(options =>
            //{
            //    options.UseSqlServer(Configuration["AzureSQLConnection"]);
            //});

            services.AddDbContext<IdentificationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("IdentityDbLocalConnectionString"));
            });

            AuthenticationBuilder authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
                .AddLinkedIn(options =>
                {
                    options.ClientId = "test";
                    options.ClientSecret = "test";
                })
                .AddGitHub(options =>
                {
                    options.ClientId = "test";
                    options.ClientSecret = "test";
                })
                .AddMicrosoftAccount(options =>
                {
                    options.ClientId = "test";
                    options.ClientSecret = "test";
                })
                .AddGoogle(options =>
                {
                    options.ClientId = "test";
                    options.ClientSecret = "test";
                });
            authenticationBuilder.AddExternalCookie().Configure(options =>
            {
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = "ExternalAuthCookie";
            });
            authenticationBuilder.AddApplicationCookie().Configure(options =>
            {
                options.ExpireTimeSpan = new TimeSpan(0, 60, 0);
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = "AuthenticationCookie";
                options.LoginPath = "/Login";
                options.LogoutPath = "/User/Logout";
                options.SlidingExpiration = false;
            });
            var identityService = services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+/ ";
                options.User.RequireUniqueEmail = true;
                options.Stores.MaxLengthForKeys = 128;
            })
                .AddDefaultTokenProviders()
                .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory<ApplicationUser>>()
                .AddEntityFrameworkStores<IdentificationDbContext>();
            identityService.AddSignInManager();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseExceptionHandler("/exceptionHandler");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
