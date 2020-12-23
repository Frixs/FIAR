using Ixs.DNA;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;

namespace Fiar
{
    /// <summary>
    /// <inheritdoc cref="IConfigBox"/>
    /// </summary>
    public sealed class ConfigBox : IConfigBox
    {
        #region Private Members (Injects)

        private readonly IConfiguration mConfiguration;

        #endregion

        #region Interface Properties

        public SupportedServerDatabaseTechnology DatabaseConnection_Technology { get; private set; }

        public string Jwt_SecretKey => mConfiguration["Jwt:SecretKey"] ?? "";

        public string Jwt_Issuer => mConfiguration["Jwt:Issuer"] ?? "";

        public string Jwt_Audience => mConfiguration["Jwt:Audience"] ?? "";

        public int Jwt_Expires { get; private set; }

        public string MessageService_Email_Host => mConfiguration["MessageService:Email:Host"] ?? "smtp.gmail.com";

        public int MessageService_Email_Port { get; private set; }

        public string MessageService_Email_Credentials_Username => mConfiguration["MessageService:Email:Credentials:Username"] ?? "";

        public string MessageService_Email_Credentials_Password => mConfiguration["MessageService:Email:Credentials:Password"] ?? "";

        public bool MessageService_Email_SSL { get; private set; }

        public string MessageService_Email_MailFrom_Address { get; private set; }

        public string MessageService_Email_MailFrom_Name { get; private set; }

        public int Configuration_WebLoginSessionExpires { get; private set; }

        public string Configuration_DateTimeFormat_Standard => mConfiguration["Configuration:DateTimeFormat:Standard"] ?? "yyyy-MM-dd (HH:mm)";

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ConfigBox(IConfiguration configuration)
        {
            mConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Load();
        }

        #endregion

        #region Interface Methods

        /// <inheritdoc/>
        public void Load()
        {
            Console.WriteLine("=========================================================================================");
            Console.WriteLine("=============================== PARSING CONFIGURATION ... ----------------------------===");
            Console.WriteLine("=========================================================================================");

            try
            {
                SetValue(nameof(DatabaseConnection_Technology),
                    () => DatabaseConnection_Technology = (SupportedServerDatabaseTechnology)Enum.Parse(typeof(SupportedServerDatabaseTechnology), mConfiguration["DatabaseConnection:Technology"] ?? "MSSQL", false)
                    );
                SetValue(nameof(Jwt_SecretKey),
                    () => { if (Jwt_SecretKey.IsNullOrEmpty()) throw new NullReferenceException("Null or empty!"); }
                    );
                SetValue(nameof(Jwt_Issuer),
                    () => { if (Jwt_Issuer.IsNullOrEmpty()) throw new NullReferenceException("Null or empty!"); }
                    );
                SetValue(nameof(Jwt_Audience),
                    () => { if (Jwt_Audience.IsNullOrEmpty()) throw new NullReferenceException("Null or empty!"); }
                    );
                SetValue(nameof(Jwt_Expires),
                    () =>
                    {
                        Jwt_Expires = int.Parse(mConfiguration["Jwt:Expires"]);
                        if (Jwt_Expires < 0) throw new Exception("Cannot be negative!");
                    }
                    );
                SetValue(nameof(MessageService_Email_Host),
                    () => { if (MessageService_Email_Host.IsNullOrEmpty()) throw new NullReferenceException("Null or empty!"); }
                    );
                SetValue(nameof(MessageService_Email_Port),
                    () =>
                    {
                        MessageService_Email_Port = int.Parse(mConfiguration["MessageService:Email:Port"]);
                        if (MessageService_Email_Port < 1) throw new Exception("Cannot be zero or negative!");
                    }
                    );
                SetValue(nameof(MessageService_Email_Credentials_Username),
                    () => { if (MessageService_Email_Credentials_Username == null) throw new NullReferenceException("Null!"); }
                    );
                SetValue(nameof(MessageService_Email_Credentials_Password),
                    () => { if (MessageService_Email_Credentials_Password == null) throw new NullReferenceException("Null!"); }
                    );
                SetValue(nameof(MessageService_Email_SSL),
                    () => MessageService_Email_SSL = bool.Parse(mConfiguration["MessageService:Email:SSL"])
                    );
                SetValue(nameof(MessageService_Email_MailFrom_Address),
                    () =>
                    {
                        MessageService_Email_MailFrom_Address = mConfiguration["MessageService:Email:MailFrom:Address"];
                        if (MessageService_Email_MailFrom_Address.IsNullOrEmpty() || MessageService_Email_MailFrom_Address.Length > 255)
                            throw new Exception("Argument is null, empty or too long!");
                    }
                    );
                SetValue(nameof(MessageService_Email_MailFrom_Name),
                    () =>
                    {
                        MessageService_Email_MailFrom_Name = mConfiguration["MessageService:Email:MailFrom:Address"];
                        if (MessageService_Email_MailFrom_Name.IsNullOrEmpty() || MessageService_Email_MailFrom_Name.Length > 255)
                            throw new Exception("Argument is null, empty or too long!");
                    }
                    );
                SetValue(nameof(Configuration_WebLoginSessionExpires),
                    () =>
                    {
                        Configuration_WebLoginSessionExpires = int.Parse(mConfiguration["Configuration:WebLoginSessionExpires"]);
                        if (Configuration_WebLoginSessionExpires < 0) throw new Exception("Cannot be negative!");
                    }
                    );
                SetValue(nameof(Configuration_DateTimeFormat_Standard),
                    () => { if (Configuration_DateTimeFormat_Standard == null) throw new NullReferenceException("Null or empty!"); }
                    );

                Console.WriteLine("=========================================================================================");
                Console.WriteLine("=========================== CONFIGURATION PARSED SUCCESSFULLY ===========================");
                Console.WriteLine("=========================================================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine("=========================================================================================");
                Console.WriteLine("===-------------------------- FAILED TO PARSE CONFIGURATION --------------------------===");
                Console.WriteLine(ex.Message);
                Console.WriteLine("===--------------------------");
                Console.WriteLine("=========================================================================================");
                throw;
            }
        }

        /// <inheritdoc/>
        public string Dump()
        {
            string sout = "";

            var props = typeof(IConfigBox).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props)
                sout += $"{prop.Name} -> {Environment.NewLine}{prop.GetValue(this).ToString().Replace(Environment.NewLine, @"\n")}{Environment.NewLine}{Environment.NewLine}";

            return sout;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Set configuration value with console output
        /// </summary>
        /// <param name="label">Console label output</param>
        /// <param name="set">Set action</param>
        private void SetValue(string label, Action set)
        {
            Console.WriteLine($"-> {label} ...");
            set();
            Console.WriteLine($"... SET!");
        }

        #endregion
    }
}
