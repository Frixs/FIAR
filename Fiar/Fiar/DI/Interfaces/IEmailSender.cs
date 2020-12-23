using System.Threading.Tasks;

namespace Fiar
{
    /// <summary>
    /// A service that handles sending emails on behalf of the called
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an email message with the given information
        /// </summary>
        /// <param name="details">The details about the email send</param>
        /// <returns></returns>
        Task<SendEmailResponse> SendAsync(SendEmailDetails details);

        /// <summary>
        /// Sends an bulk email message with the given information
        /// </summary>
        /// <param name="details">The details about the email send</param>
        /// <returns></returns>
        Task<SendEmailResponse> SendBulkAsync(SendEmailDetails[] details);
    }
}
