using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fiar
{
    /// <summary>
    /// The database representational model for our application.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        #region Public Properties (Device/Routine)

        /// <summary>
        /// The devices for the application
        /// </summary>
        public DbSet<DeviceDataModel> Devices { get; set; }

        /// <summary>
        /// The (S)MS recipients for a device
        /// </summary>
        public DbSet<DeviceMessageServiceRecipientDataModel> DeviceMessageServiceRecipients { get; set; }

        /// <summary>
        /// The routines for the application
        /// </summary>
        public DbSet<RoutineDataModel> Routines { get; set; }

        /// <summary>
        /// The routine history for the application
        /// </summary>
        public DbSet<RoutineHistoryDataModel> RoutineHistory { get; set; }

        #endregion

        #region Public Properties (SNMP Connector)

        /// <summary>
        /// The SNMP connector for routines as a data connector option
        /// </summary>
        public DbSet<SnmpConnectorDataModel> ConnectorSnmpEssentials { get; set; }

        /// <summary>
        /// Details about OIDs for <see cref="ConnectorSnmpEssentials"/>
        /// </summary>
        public DbSet<SnmpOidDataModel> ConnectorSnmpOids { get; set; }

        #endregion

        #region Public Properties (DBCONN Connector)

        /// <summary>
        /// The DBCONN connector for routines as a data connector option
        /// </summary>
        public DbSet<DbConnConnectorDataModel> ConnectorDbConnEssentials { get; set; }

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
            modelBuilder.Entity<ApplicationUser>().HasIndex(o => o.AlertSuppressed);

            #region Devices

            // Configure Devices
            // ------------------------------
            //
            // Set Id as primary key
            modelBuilder.Entity<DeviceDataModel>().HasKey(o => o.Id);
            // Set up indexed/unique columns
            modelBuilder.Entity<DeviceDataModel>().HasIndex(o => o.Status);
            modelBuilder.Entity<DeviceDataModel>().HasIndex(o => o.StatusLatestNoGreenAt);
            modelBuilder.Entity<DeviceDataModel>().HasIndex(o => o.StatusLatestOkAt);
            modelBuilder.Entity<DeviceDataModel>().HasIndex(o => o.AlertIgnoreTime);
            // Set up limits
            modelBuilder.Entity<DeviceDataModel>().Property(o => o.Ip).HasMaxLength(DeviceDataModel.Ip_MaxLength);
            modelBuilder.Entity<DeviceDataModel>().Property(o => o.Description).HasMaxLength(DeviceDataModel.Description_MaxLength);
            modelBuilder.Entity<DeviceDataModel>().Property(o => o.Note).HasMaxLength(DeviceDataModel.Note_MaxLength);

            #endregion

            #region Device Message Service Recipients

            // Configure Devices
            // ------------------------------
            //
            // Set Id as primary key
            modelBuilder.Entity<DeviceMessageServiceRecipientDataModel>().HasKey(o => o.Id);
            // Set Foreign Key
            modelBuilder.Entity<DeviceMessageServiceRecipientDataModel>()
                .HasOne(o => o.Device)
                .WithMany(o => o.MessageServiceRecipients)
                .HasForeignKey(o => o.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
            // Set up indexed/unique columns
            modelBuilder.Entity<DeviceMessageServiceRecipientDataModel>().HasIndex(o => o.ApplicationUserId);
            modelBuilder.Entity<DeviceMessageServiceRecipientDataModel>().HasIndex(o => o.DeviceId);

            #endregion

            #region Routines

            // Configure Routines
            // ------------------------------
            //
            // Set Id as primary key
            modelBuilder.Entity<RoutineDataModel>().HasKey(o => o.Id);
            // Set Foreign Key
            modelBuilder.Entity<RoutineDataModel>()
                .HasOne(o => o.Device)
                .WithMany(o => o.Routines)
                .HasForeignKey(o => o.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
            // Set up indexed/unique columns
            modelBuilder.Entity<RoutineDataModel>().HasIndex(o => o.Type);
            modelBuilder.Entity<RoutineDataModel>().HasIndex(o => o.IsTemplate);
            modelBuilder.Entity<RoutineDataModel>().HasIndex(o => o.Priority);
            modelBuilder.Entity<RoutineDataModel>().HasIndex(o => o.IsHighlighted);
            modelBuilder.Entity<RoutineDataModel>().HasIndex(o => o.ConfigurationChangedAt);
            // Set up limits
            modelBuilder.Entity<RoutineDataModel>().Property(o => o.TemplateName).HasMaxLength(RoutineDataModel.TemplateName_MaxLength);

            #endregion

            #region Routine History

            // Configure Routine History
            // ------------------------------
            //
            // Set Id as primary key
            modelBuilder.Entity<RoutineHistoryDataModel>().HasKey(o => o.Id);
            // Set Foreign Key
            modelBuilder.Entity<RoutineHistoryDataModel>()
                .HasOne(o => o.Routine)
                .WithMany(o => o.RoutineHistory)
                .HasForeignKey(o => o.RoutineId)
                .OnDelete(DeleteBehavior.Cascade);
            // Set up indexed/unique columns
            modelBuilder.Entity<RoutineHistoryDataModel>().HasIndex(o => o.Status);
            modelBuilder.Entity<RoutineHistoryDataModel>().HasIndex(o => o.RecordedAt);

            #endregion

            #region ConnectorSnmpEssentials

            // Configure Connector: SNMP
            // ------------------------------
            //
            // Set Id as primary key
            modelBuilder.Entity<SnmpConnectorDataModel>().HasKey(o => o.Id);
            // Set Foreign Key
            modelBuilder.Entity<SnmpConnectorDataModel>()
                .HasOne(o => o.Routine)
                .WithOne(o => o.ConnectorSnmp)
                .HasForeignKey<SnmpConnectorDataModel>(o => o.RoutineId)
                .OnDelete(DeleteBehavior.Cascade);
            // Set up limits
            modelBuilder.Entity<SnmpConnectorDataModel>().Property(o => o.Community).HasMaxLength(SnmpConnectorDataModel.Community_MaxLength);

            #endregion

            #region ConnectorSnmpOids

            // Configure Connector: SNMP - Oids
            // ------------------------------
            //
            // Set Id as primary key
            modelBuilder.Entity<SnmpOidDataModel>().HasKey(o => o.Id);
            // Set Foreign Key
            modelBuilder.Entity<SnmpOidDataModel>()
                .HasOne(o => o.Connector)
                .WithMany(o => o.Oids)
                .HasForeignKey(o => o.ConnectorId)
                .OnDelete(DeleteBehavior.Cascade);
            // Set up indexed/unique columns
            modelBuilder.Entity<SnmpOidDataModel>().HasIndex(o => o.OidKey);

            #endregion

            #region ConnectorDbConnEssentials

            // Configure Connector: DBCONN
            // ------------------------------
            //
            // Set Id as primary key
            modelBuilder.Entity<DbConnConnectorDataModel>().HasKey(o => o.Id);
            // Set Foreign Key
            modelBuilder.Entity<DbConnConnectorDataModel>()
                .HasOne(o => o.Routine)
                .WithOne(o => o.ConnectorDbConn)
                .HasForeignKey<DbConnConnectorDataModel>(o => o.RoutineId)
                .OnDelete(DeleteBehavior.Cascade);
            // Set up limits
            modelBuilder.Entity<DbConnConnectorDataModel>().Property(o => o.DatabaseConnectionString).HasMaxLength(DbConnConnectorDataModel.DatabaseConnectionString_MaxLength);
            modelBuilder.Entity<DbConnConnectorDataModel>().Property(o => o.Query).HasMaxLength(DbConnConnectorDataModel.Query_MaxLength);

            #endregion
        }

        #endregion
    }
}
