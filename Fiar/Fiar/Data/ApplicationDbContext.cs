using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fiar
{
    /// <summary>
    /// The database representational model for our application.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        #region Public Properties (Game)

        /// <summary>
        /// The games for the application
        /// </summary>
        public DbSet<GameDataModel> Games { get; set; }

        /// <summary>
        /// The game moves for the application
        /// </summary>
        public DbSet<GameMoveDataModel> GameMoves { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor, expectiong database options passed in
        /// </summary>
        /// <param name="options">The databnase context options</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configures the database structure and relationships
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API

            // Configure Users
            // ------------------------------
            //
            // Set up indexed/unique columns
            modelBuilder.Entity<ApplicationUser>().HasIndex(o => o.Nickname);

            #region Games

            // Configure Games
            // ------------------------------
            //
            // Set Id as primary key
            modelBuilder.Entity<GameDataModel>().HasKey(o => o.Id);
            // Set up indexed/unique columns
            modelBuilder.Entity<GameDataModel>().HasIndex(o => o.Result);

            #endregion

            #region Routines

            // Configure GameMoves
            // ------------------------------
            //
            // Set Id as primary key
            modelBuilder.Entity<GameMoveDataModel>().HasKey(o => o.Id);
            // Set Foreign Key
            modelBuilder.Entity<GameMoveDataModel>()
                .HasOne(o => o.Game)
                .WithMany(o => o.Moves)
                .HasForeignKey(o => o.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            // Set up indexed/unique columns
            modelBuilder.Entity<GameMoveDataModel>().HasIndex(o => o.Type);

            #endregion
        }

        #endregion
    }
}
