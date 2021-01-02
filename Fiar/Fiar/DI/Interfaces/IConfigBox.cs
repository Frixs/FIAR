using Microsoft.Extensions.Configuration;

namespace Fiar
{
    /// <summary>
    /// Configuration interface to follow <see cref="IConfiguration"/> configuration (file)
    /// </summary>
    public interface IConfigBox
    {
        SupportedServerDatabaseTechnology DatabaseConnection_Technology { get; }
        string Jwt_SecretKey { get; }
        string Jwt_Issuer { get; }
        string Jwt_Audience { get; }
        int Jwt_Expires { get; }
        string MessageService_Email_Host { get; }
        int MessageService_Email_Port { get; }
        string MessageService_Email_Credentials_Username { get; }
        string MessageService_Email_Credentials_Password { get; }
        bool MessageService_Email_SSL { get; }
        string MessageService_Email_MailFrom_Address { get; }
        string MessageService_Email_MailFrom_Name { get; }
        int Configuration_WebLoginSessionExpires { get; }
        string Configuration_DateTimeFormat_Standard { get; }
        bool Configuration_VerifyEmailRequired { get; }

        /// <summary>
        /// Load configuration
        /// </summary>
        void Load();

        /// <summary>
        /// Dump the configuration
        /// </summary>
        string Dump();
    }
}
