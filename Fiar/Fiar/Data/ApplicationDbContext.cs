using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fiar
{
    /// <summary>
    /// The database representational model for our application.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        #region Public Properties (User)

        /// <summary>
        /// The friend user relations
        /// </summary>
        public DbSet<FriendUserRelationDataModel> FriendUserRelations { get; set; }

        /// <summary>
        /// The user requests
        /// </summary>
        public DbSet<UserRequestDataModel> UserRequests { get; set; }

        #endregion

        #region Public Properties (Game)

        /// <summary>
        /// The games for the application
        /// </summary>
        public DbSet<GameDataModel> Games { get; set; }

        /// <summary>
        /// The game moves for the application
        /// </summary>
        public DbSet<GameMoveDataModel> GameMoves { get; set; }

        /// <summary>
        /// The game participants
        /// </summary>
        public DbSet<GameParticipantDataModel> GameParticipants { get; set; }

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

            #region FriendUserRelations

            // Configure FriendUserRelations
            // ------------------------------
            //
            // Set Id as primary key
            modelBuilder.Entity<FriendUserRelationDataModel>().HasKey(o => o.Id);
            // Set Foreign Key (user-side)
            modelBuilder.Entity<FriendUserRelationDataModel>()
                .HasOne(o => o.User)
                .WithMany(o => o.Friends)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // Set Foreign Key (friend-side)
            modelBuilder.Entity<FriendUserRelationDataModel>()
                .HasOne(o => o.FriendUser)
                .WithMany(o => o.BeingFriendOfs)
                .HasForeignKey(o => o.FriendUserId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region UserRequests

            // Configure UserRequests
            // ------------------------------
            //
            // Set Id as primary key
            modelBuilder.Entity<UserRequestDataModel>().HasKey(o => o.Id);
            // Set Foreign Key (user-side)
            modelBuilder.Entity<UserRequestDataModel>()
                .HasOne(o => o.User)
                .WithMany(o => o.Requests)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // Set Foreign Key (related-user-side)
            modelBuilder.Entity<UserRequestDataModel>()
                .HasOne(o => o.RelatedUser)
                .WithMany(o => o.RelatedRequests)
                .HasForeignKey(o => o.RelatedUserId)
                .OnDelete(DeleteBehavior.Cascade);
            // Set up indexed/unique columns
            modelBuilder.Entity<UserRequestDataModel>().HasIndex(o => o.Type);

            #endregion

            #region Games

            // Configure Games
            // ------------------------------
            //
            // Set Id as primary key
            modelBuilder.Entity<GameDataModel>().HasKey(o => o.Id);
            // Set up indexed/unique columns
            modelBuilder.Entity<GameDataModel>().HasIndex(o => o.Result);

            #endregion

            #region GameMoves

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

            #region GameParticipants

            // Configure GameParticipants
            // ------------------------------
            //
            // Set Id as primary key
            modelBuilder.Entity<GameParticipantDataModel>().HasKey(o => o.Id);
            // Set Foreign Key
            modelBuilder.Entity<GameParticipantDataModel>()
                .HasOne(o => o.Game)
                .WithMany(o => o.Participants)
                .HasForeignKey(o => o.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            // Set up indexed/unique columns
            modelBuilder.Entity<GameParticipantDataModel>().HasIndex(o => o.Type);

            #endregion
        }

        #endregion
    }
}
