/*
	This file was generated and should not be modified directly
*/
// Model is Table
using Bam.Data;

namespace Bam.UserAccounts.Data
{
    public interface IUser
    {
       // AccountCollection AccountsByUserId { get; }
        DateTime? CreationDate { get; set; }
        string Cuid { get; set; }
        string Email { get; set; }
      //  XrefDaoCollection<UserGroup, Group> Groups { get; }
        ulong? Id { get; set; }
        bool? IsApproved { get; set; }
        bool IsAuthenticated { get; }
        bool? IsDeleted { get; set; }
        bool IsLockedOut { get; set; }
        bool IsOnline { get; set; }
        DateTime LastAcitivtyDate { get; set; }
        DateTime LastLockoutDate { get; set; }
        DateTime LastLoginDate { get; set; }
        DateTime LastPasswordChangedDate { get; set; }
       // LockOutCollection LockOutsByUserId { get; }
       // LoginCollection LoginsByUserId { get; }
       // PasswordFailureCollection PasswordFailuresByUserId { get; }
        string PasswordQuestion { get; set; }
       // PasswordQuestionCollection PasswordQuestionsByUserId { get; }
       // PasswordResetCollection PasswordResetsByUserId { get; }
       // PasswordCollection PasswordsByUserId { get; }
       // XrefDaoCollection<UserPermission, Permission> Permissions { get; }
        //XrefDaoCollection<UserRole, Role> Roles { get; }
       // SessionCollection SessionsByUserId { get; }
       // SettingCollection SettingsByUserId { get; }
       // UserGroupCollection UserGroupsByUserId { get; }
        string UserName { get; set; }
        //UserPermissionCollection UserPermissionsByUserId { get; }
        //UserRoleCollection UserRolesByUserId { get; }
        string Uuid { get; set; }

        //void AddLoginRecord(Database db = null);
        bool ChangePasswordQuestionAndAnswer(string password, string newPasswordQuestion, string newPasswordAnswer);
        string GetPassword();
        string GetPassword(string answer);
        IQueryFilter GetUniqueFilter();
        bool IsFirstLogin();
        string ResetPassword();
        string ResetPassword(string answer);
        void SetPassword(string password);
        bool ValidatePassword(string password, bool updateFailures = true);
    }
}