using Ixs.DNA;
using Ixs.DNA.AspNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                options.UseFiarOptions();
            });
            // Add function to create transient DB context
            services.AddTransient((provider) => new Func<ApplicationDbContext>(() => new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>().UseFiarOptions().Options
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
                options.AddPolicy(PolicyNames.PlayerLevel,
                    builder => builder.RequireRole(Policies.Dict[PolicyNames.PlayerLevel]));
                options.AddPolicy(PolicyNames.AdministratorLevel,
                    builder => builder.RequireRole(Policies.Dict[PolicyNames.AdministratorLevel]));
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
                options.Cookie.Name = "Fiar.AuthCookieAspNetCore";

                // Redirect to /login
                options.LoginPath = WebRoutes.Login;

                // Change cookie timeout to expire
                options.ExpireTimeSpan = TimeSpan.FromSeconds(DI.ConfigBox.Configuration_WebLoginSessionExpires);
                options.Cookie.SameSite = SameSiteMode.None;
                options.SlidingExpiration = true;
            });

            // Add caching
            services.AddMemoryCache();

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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor)
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

            // Dev-only
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Production-only
            else
            {
                app.UseExceptionHandler(WebRoutes.Error500);
                // TODO:LATER: ... HSTS
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                // In production, tell the browsers (via the HSTS header) to only try and access our site via HTTPS, not HTTP
                // Explanation what HSTS does https://youtu.be/fsIkYMpBO6s?t=761
                app.UseHsts();
            }

            // Use web-sockets
            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(60),
                ReceiveBufferSize = 1024 * 4
            };
            //webSocketOptions.AllowedOrigins.Add("https://client.com");
            //webSocketOptions.AllowedOrigins.Add("https://www.client.com");
            app.UseWebSockets(webSocketOptions);

            // Middleware
            app.Use(async (context, next) =>
            {
                // Check the context for the web-socket request...
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                        using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                        {
                            await HandleWebSocket(context, webSocket);
                        }
                    else
                        context.Response.StatusCode = 400;
                }
                // Otherwise go in the standard way...
                else
                {
                    await next();
                }

                // Handle online users cache
                HandleOnlineUsersCache(httpContextAccessor, memoryCache);

                // Define error page redirects
                // 400
                if (context.Response.StatusCode == 400 && !context.Response.HasStarted)
                {
                    //Re-execute the request so the user gets the error page
                    string originalPath = context.Request.Path.Value;
                    context.Items["originalPath"] = originalPath;
                    context.Request.Path = WebRoutes.Error400;
                    await next();
                }
                // 401
                if (context.Response.StatusCode == 401 && !context.Response.HasStarted)
                {
                    //Re-execute the request so the user gets the error page
                    string originalPath = context.Request.Path.Value;
                    context.Items["originalPath"] = originalPath;
                    context.Request.Path = WebRoutes.Error401;
                    await next();
                }
                // 403
                else if (context.Response.StatusCode == 403 && !context.Response.HasStarted)
                {
                    //Re-execute the request so the user gets the error page
                    string originalPath = context.Request.Path.Value;
                    context.Items["originalPath"] = originalPath;
                    context.Request.Path = WebRoutes.Error403;
                    await next();
                }
                // 404
                else if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
                {
                    //Re-execute the request so the user gets the error page
                    string originalPath = context.Request.Path.Value;
                    context.Items["originalPath"] = originalPath;
                    context.Request.Path = WebRoutes.Error404;
                    await next();
                }
            });

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

        #region Helpers

        /// <summary>
        /// WebSocket handler
        /// </summary>
        private async Task HandleWebSocket(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        /// <summary>
        /// Handle the cache on end request
        /// </summary>
        private void HandleOnlineUsersCache(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            Dictionary<string, DateTime> loggedInUsers;
            if (!memoryCache.TryGetValue(MemoryCacheIdentifiers.LoggedInUsers, out loggedInUsers))
                loggedInUsers = new Dictionary<string, DateTime>();
            
            if (httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                var userName = httpContextAccessor.HttpContext.User.Identity.Name;
                if (loggedInUsers != null)
                {
                    loggedInUsers[userName] = DateTime.Now;
                    memoryCache.Set(MemoryCacheIdentifiers.LoggedInUsers, loggedInUsers);
                }
            }

            if (loggedInUsers != null)
            {
                foreach (var item in loggedInUsers.ToList())
                {
                    if (item.Value < DateTime.Now.AddMinutes(-5))
                    {
                        loggedInUsers.Remove(item.Key);
                    }
                }
                memoryCache.Set(MemoryCacheIdentifiers.LoggedInUsers, loggedInUsers);
            }
        }

        #endregion
    }
}
