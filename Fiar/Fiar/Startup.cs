using Ixs.DNA;
using Ixs.DNA.AspNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Fiar
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add project base modules
            services.AddProjectBaseModules();

            // Add (default) scoped ApplicationDbContext to DI
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Use the settings we set
                options.UseServerRoomOptions();
            });
            // Add function to create transient DB context
            services.AddTransient((provider) => new Func<ApplicationDbContext>(() => new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>().UseServerRoomOptions().Options
                )));

            // Register Hosted Services here
            // ...

            // AddIdentity adds cookies based authentication
            // Adds scoped classes for things like UserManager, SignInManager, PasswordHashers etc.
            // NOTE: Automatically adds the validated user from a cookies to the HttpContext.User
            // https://github.com/aspnet/Identity/blob/85f8a49aef68bf9763cd9854ce1dd4a26a7c5d3c/src/Identity/IdentityServiceCollectionExtensions.cs
            services.AddIdentity<ApplicationUser, IdentityRole>()
                // Adds UserStore and RoleStore from this context
                // That are consumed by the UserManager and RoleManager
                // https://youtu.be/WQywatfis6s?t=1499
                // https://github.com/aspnet/Identity/blob/master/src/EF/IdentityEntityFrameworkBuilderExtensions.cs
                .AddEntityFrameworkStores<ApplicationDbContext>()
                // Adds a provider that generates unique keys and hashes for things like
                // forgot passwords links, phone number verification codes etc.
                .AddDefaultTokenProviders();

            // Add JWT Authentication for API clients
            services.AddAuthentication()
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    //options.Cookie.SecurePolicy = Framework.Construction.Environment.IsDevelopment ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = DI.ConfigBox.Jwt_Issuer,
                        ValidAudience = DI.ConfigBox.Jwt_Audience,

                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(DI.ConfigBox.Jwt_SecretKey))
                    };
                });

            // Add authorization policy
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyNames.ReadOnlyLevel,
                    builder => builder.RequireRole(Policies.Dict[PolicyNames.ReadOnlyLevel]));
                options.AddPolicy(PolicyNames.ControlLevel,
                    builder => builder.RequireRole(Policies.Dict[PolicyNames.ControlLevel]));
                options.AddPolicy(PolicyNames.AdminLevel,
                    builder => builder.RequireRole(Policies.Dict[PolicyNames.AdminLevel]));
            });

            // Change policy
            services.Configure<IdentityOptions>(options =>
            {
                // Password policy
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = UserDataModel.Password_MinLength;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                // Make sure users have unique emails
                options.User.RequireUniqueEmail = true;
            });

            // Add proper cookie request to follow GDPR
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
                options.HttpOnly = HttpOnlyPolicy.None;
                //options.Secure = Framework.Construction.Environment.IsDevelopment ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
                options.Secure = CookieSecurePolicy.None;
            });

            // Alter application cookie info
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "ServerRoom.AuthCookieAspNetCore";

                // Redirect to /login
                options.LoginPath = WebRoutes.Login;

                // Change cookie timeout to expire
                options.ExpireTimeSpan = TimeSpan.FromSeconds(DI.ConfigBox.Configuration_WebLoginSessionExpires);
                options.Cookie.SameSite = SameSiteMode.None;
                options.SlidingExpiration = true;
            });

            var mvc = services.AddMvc()
                // State we are a minimum compatability of...
                // Excplanation at https://youtu.be/fsIkYMpBO6s?t=578
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_3_0);

#if (DEBUG)
            // Add runtime compilation to razor templates - ONLY IN DEBUG
            // Reference to nuget packgage is also set conditionaly - only in debug
            mvc.AddRazorRuntimeCompilation();
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Use base path
            app.UsePathBase(Framework.Construction.Configuration["PathBase"]);

            // Use DNA Framework
            app.UseDnaFramework();

            // Force non-essential cookies to only store
            // if the user has consented
            app.UseCookiePolicy();

            // Setup Identity
            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error/500");
                // TODO:LATER: ... HSTS
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // In production, tell the browsers (via the HSTS header) to only try and access our site via HTTPS, not HTTP
                // Explanation what HSTS does https://youtu.be/fsIkYMpBO6s?t=761
                app.UseHsts();
            }

            // Redirect all calls from HTTP to HTTPS
            app.UseHttpsRedirection();

            // Serve static files
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // Default route
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            // Make sure we have the database.
            serviceProvider.GetService<ApplicationDbContext>().Database.EnsureCreated();
        }
    }
}
