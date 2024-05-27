using System;
using Bam.ServiceProxy;
using Bam.Messaging;
using Bam.UserAccounts.Data;

namespace Bam.UserAccounts
{
    public interface IUserManager: ICloneable, IRequiresHttpContext, ISmtpSettingsProvider
    {
        IApplicationNameProvider ApplicationNameProvider { get; set; }
        Func<string, string> GetConfirmationUrlFunction { get; set; }
        Func<string, string> GetPasswordResetUrlFunction { get; set; }
        int PasswordResetTokensExpireInThisManyMinutes { get; set; }

        event EventHandler ConfirmAccountFailed;
        event EventHandler ConfirmAccountSucceeded;
        event EventHandler ForgotPasswordFailed;
        event EventHandler ForgotPasswordSucceeded;
        event EventHandler LoginFailed;
        event EventHandler LoginSucceeded;
        event EventHandler RequestConfirmationEmailFailed;
        event EventHandler RequestConfirmationEmailSucceeded;
        event EventHandler ResetPasswordFailed;
        event EventHandler ResetPasswordSucceeded;
        event EventHandler SignOutFailed;
        event EventHandler SignOutStarted;
        event EventHandler SignOutSucceeded;
        event EventHandler SignUpFailed;
        event EventHandler SignUpSucceeded;

        IServiceProxyResponse ConfirmAccount(string token);
        IServiceProxyResponse ForgotPassword(string emailAddress);
        IUser GetUser(IHttpContext context);
        IServiceProxyResponse IsEmailInUse(string emailAddress);
        IServiceProxyResponse IsUserNameAvailable(string userName);
        IServiceProxyResponse Login(string userName, string passHash);
        IServiceProxyResponse RequestConfirmationEmail(string emailAddress, int accountIndex = 0);
        IServiceProxyResponse ResetPassword(string passHash, string resetToken);
        IServiceProxyResponse SignOut();
        IServiceProxyResponse SignUp(string emailAddress, string userName, string passwordHash, bool sendConfirmationEmail);
    }
}