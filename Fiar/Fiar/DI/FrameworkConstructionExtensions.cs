using Microsoft.Extensions.DependencyInjection;

namespace Fiar
{
    /// <summary>
    /// Extension methods for the <see cref="FrameworkConstruction"/>
    /// </summary>
    public static class FrameworkConstructionExtensions
    {
        /// <summary>
        /// Injects the base project into the services
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddProjectBaseModules(this IServiceCollection services)
        {
            // Bind email sender
            services.AddTransient<IEmailSender, BaseEmailSender>();

            // Return collection for chaining
            return services;
        }
    }
}
