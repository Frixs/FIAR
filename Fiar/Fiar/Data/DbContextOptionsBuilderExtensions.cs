using Ixs.DNA;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Fiar
{
    /// <summary>
    /// Extensions for DbContext builder to leave an option for us to set our options abstractly
    /// </summary>
    public static class DbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// Use series of options intended for this app
        /// </summary>
        /// <typeparam name="TBuilder">Builder specification (generic optionally)</typeparam>
        /// <param name="builder">The builder</param>
        /// <returns>The builder for chaining</returns>
        public static TBuilder UseServerRoomOptions<TBuilder>(this TBuilder builder)
            where TBuilder : DbContextOptionsBuilder
        {
            // Setup connection based on desired DB technology
            switch (DI.ConfigBox.DatabaseConnection_Technology)
            {
                // PostgreSQL
                case SupportedServerDatabaseTechnology.PostgreSQL:
                    builder.UseNpgsql(Framework.Construction.Configuration.GetConnectionString("DefaultConnection"));
                    break;

                // MSSQL
                default:
                    builder.UseSqlServer(Framework.Construction.Configuration.GetConnectionString("DefaultConnection"));
                    break;
            }
            
            // Return builder for chaining
            return builder;
        }
    }
}
