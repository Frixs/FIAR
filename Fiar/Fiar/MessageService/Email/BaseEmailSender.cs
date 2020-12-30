using Ixs.DNA;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Fiar
{
    /// <summary>
    /// Sends emails using 3rd party service
    /// </summary>
    /// <remarks>
    ///     Sender is intended to be transient
    /// </remarks>
    public class BaseEmailSender : IEmailSender
    {
        #region Private Members (Injects)

        /// <summary>
        /// Injection - <inheritdoc cref="ILogger"/>
        /// </summary>
        protected readonly ILogger mLogger;

        /// <summary>
        /// Injection - <inheritdoc cref="IConfigBox"/>
        /// </summary>
        protected readonly IConfigBox mConfigBox;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseEmailSender(ILogger logger, IConfigBox configBox)
        {
            mConfigBox = configBox ?? throw new ArgumentNullException(nameof(configBox));
            mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Interface Methods

        /// <inheritdoc/>
        public async Task<SendEmailResponse> SendAsync(SendEmailDetails details)
        {
            // Check for mail details
            if (details == null)
            {
                // Log it
                mLogger.LogErrorSource("Mail details are not specified!");
                return new SendEmailResponse() { Errors = new List<string>() { "Mail details are not specified!" } };
            }

            // List of errors
            SendEmailResponse response = new SendEmailResponse();

            // Use the method to send an email
            response.Errors = await SendEmailViaCustomServerAsync(details);

            // Return response based on response
            return response;
        }

        /// <inheritdoc/>
        public async  Task<SendEmailResponse> SendBulkAsync(SendEmailDetails[] details)
        {
            // Check for mail details
            if (details == null || details.Length == 0)
            {
                // Log it
                mLogger.LogErrorSource("Mail details are not specified!");
                return new SendEmailResponse() { Errors = new List<string>() { "Mail details are not specified!" } };
            }

            // List of errors
            SendEmailResponse response = new SendEmailResponse { Errors = new List<string>() };

            foreach (var item in details)
            {
                var res = await SendAsync(item);
                if (res.Errors != null)
                    response.Errors.AddRange(res.Errors);
            }

            return response;
        }

        #endregion

        #region Private Send Methods

        /// <summary>
        /// Send message via custom server defined in the server configuration file
        /// </summary>
        /// <param name="details">Mail details</param>
        /// <returns>List of errors or empty one on successful send</returns>
        private async Task<List<string>> SendEmailViaCustomServerAsync(SendEmailDetails details)
        {
            SmtpClient client = null;
            MailMessage mail = null;
            List<string> response = new List<string>();

            try
            {
                // Specify the SMTP server
                client = new SmtpClient(mConfigBox.MessageService_Email_Host);

                // Specify the message
                mail = new MailMessage();
                // From
                mail.From = new MailAddress(details.FromEmail, details.FromName);
                // To
                mail.To.Add(details.ToEmail);
                // Subject
                mail.Subject = details.Subject;
                // Body
                mail.Body = details.Content;
                mail.BodyEncoding = Encoding.UTF8;
                mail.IsBodyHtml = details.IsContentHTML;

                // Specify details for the client
                client.Port = mConfigBox.MessageService_Email_Port;
                client.Credentials = new System.Net.NetworkCredential(mConfigBox.MessageService_Email_Credentials_Username, mConfigBox.MessageService_Email_Credentials_Password);
                client.EnableSsl = mConfigBox.MessageService_Email_SSL;

                // Send the mail
                await client.SendMailAsync(mail);

                // Log it
                mLogger.LogInformationSource($"Mail has been sent from '{mail.From}' to '{mail.To}'.");
            }
            catch (SmtpException ex)
            {
                response.Add(ex.Message);
                // Log it
                mLogger.LogErrorSource($"The connection to the SMTP server failed, authorization failed or the request timed out! {ex.Message}");
            }
            catch (Exception ex)
            {
                response.Add(ex.Message);
                // Log it
                mLogger.LogErrorSource($"Something went wrong during senting mail! {ex.Message}");
            }
            finally
            {
                if (client != null)
                    client.Dispose();

                if (mail != null)
                    mail.Dispose();
            }

            // Return response
            return response;
        }

        #endregion
    }
}
