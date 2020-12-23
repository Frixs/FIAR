using Ixs.DNA;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Fiar
{
    /// <summary>
    /// A shorthand access class to get DI services with nice clean short code
    /// </summary>
    public static class DI
    {
        /// <summary>
        /// The scoped instance of the <see cref="ApplicationDbContext"/>
        /// </summary>
        public static ApplicationDbContext ApplicationDbContext => Framework.Provider.GetService<ApplicationDbContext>();

        /// <summary>
        /// Function to create transient DB context
        /// </summary>
        public static Func<ApplicationDbContext> CreateDbContext => Framework.Service<Func<ApplicationDbContext>>();

        /// <summary>
        /// Scope Factory reference to be able to create a scope to access root provider (for DB context e.g.)
        /// </summary>
        public static IServiceScopeFactory ScopeFactory => Framework.Provider.GetService<IServiceScopeFactory>();

        /// <summary>
        /// The singleton instance of the <see cref="IConfigBox"/>
        /// </summary>
        public static IConfigBox ConfigBox => Framework.Service<IConfigBox>();

        /// <summary>
        /// The transient of the <see cref="IEmailSender"/>
        /// </summary>
        public static IEmailSender Email => Framework.Service<IEmailSender>();
    }
}
