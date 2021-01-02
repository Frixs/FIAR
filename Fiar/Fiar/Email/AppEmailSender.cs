using Ixs.DNA;
using System.Threading.Tasks;

namespace Fiar
{
    /// <summary>
    /// Handles sending emails specific to this project
    /// </summary>
    public static class AppEmailSender
    {
        /// <summary>
        /// Sends the given user a new verify email link
        /// </summary>
        /// <param name="email">The users email</param>
        /// <param name="verificationUrl">The verification URL</param>
        /// <returns></returns>
        public static async Task<SendEmailResponse> SendUserEmailVerificationAsync(string email, string verificationUrl)
        {
            FrameworkDI.Logger.LogDebugSource($"Verify email '{email}' link: {verificationUrl}");

            return await DI.Email.SendAsync(new SendEmailDetails
            {
                FromName = DI.ConfigBox.MessageService_Email_MailFrom_Name,
                FromEmail = DI.ConfigBox.MessageService_Email_MailFrom_Address,
                ToEmail = email,
                Subject = "Verify your email - FIAR",
                Content = $"CVerify your email on this link: {verificationUrl}",
                IsContentHTML = false
            });
        }

        /// <summary>
        /// Sends the given user a new verify password reset link
        /// </summary>
        /// <param name="email">The users email</param>
        /// <param name="verificationUrl">The verification URL</param>
        /// <returns></returns>
        public static async Task<SendEmailResponse> SendUserPasswordResetVerificationAsync(string email, string verificationUrl)
        {
            FrameworkDI.Logger.LogDebugSource($"Verify password reset request: '{email}', link: {verificationUrl}");

            return await DI.Email.SendAsync(new SendEmailDetails
            {
                FromName = DI.ConfigBox.MessageService_Email_MailFrom_Name,
                FromEmail = DI.ConfigBox.MessageService_Email_MailFrom_Address,
                ToEmail = email,
                Subject = "Verify your password reset request - FIAR",
                Content = $"Change your password on this link: {verificationUrl}",
                IsContentHTML = false
            });
        }
    }
}
