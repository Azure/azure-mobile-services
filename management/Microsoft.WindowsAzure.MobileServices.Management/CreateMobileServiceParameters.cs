using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Management
{
    public class CreateMobileServiceParameters
    {
        static readonly Regex[] passwordVerifiers = {
            new Regex("[A-Z]"),
            new Regex("[a-z]"),
            new Regex("[0-9]"),
            new Regex(@"[_\~\`\!\@\#\$\%\^\&\*\(\)\-\+\=\[\{\]\}\|\\\:\;\""\'\<\,\>\.\?\/]")
        };

        static readonly Regex restrictedUsername = new Regex(
                "^(admin|administrator|sa|root|dbmanager|loginmanager|dbo|guest|information_schema"
                + "|sys|db_accessadmin|db_backupoperator|db_datareader|db_datawriter|db_ddladmin|db_denydatareader|"
                + "db_denydatawriter|db_owner|db_securityadmin|public)$", RegexOptions.IgnoreCase);

        static readonly Regex validUsername = new Regex(
                @"^[^\s\t\r#@\u0000-\u001F\""<>\|\:\*\?\\\/\;\,\%\=\&\$\+\d][^\s\t\r\u0000-\u001F\""\<\>\|\:\*\?\\\/\#\&\;\,\%\=]+$");

        /// <summary>
        /// Required. The name of the Mobile Service
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Optional. The location of the Mobile Service. If not specified, defaults to default location for the subscription.
        /// </summary>
        public string ServiceLocation { get; set; }

        /// <summary>
        /// Required. The SQL administrator username. 
        /// </summary>
        public string SqlAdminUsername { get; set; }

        /// <summary>
        /// Required. The SQL administrator password.
        /// </summary>
        public SecureString SqlAdminPassword { get; set; }

        /// <summary>
        /// Optional. Name of the existing SQL server to use.
        /// </summary>
        public string ExistingSqlServer { get; set; }

        /// <summary>
        /// Optional. Name of the existing SQL databse to use. If specified, ExistingSqlServer must also be specified.
        /// </summary>
        public string ExistingSqlDatabase { get; set; }

        /// <summary>
        /// Optional. Location of the SQL server to create. If not specified, defaults to ServiceLocation.
        /// </summary>
        public string SqlServerLocation { get; set; }

        public CreateMobileServiceParameters()
        {
            // empty
        }

        /// <summary>
        /// Validates the SQL administrator password meets strength requirements.
        /// </summary>
        /// <param name="sqlPassword">The SQL administrator password</param>
        /// <param name="sqlUsername">The SQL administrator username</param>
        /// <returns></returns>
        static bool IsSqlPasswordValid(SecureString sqlPassword, string sqlUsername)
        {
            if (sqlPassword == null)
            {
                throw new ArgumentNullException("sqlPassword");
            }

            if (sqlUsername == null)
            {
                throw new ArgumentNullException("sqlUsername");
            }

            string unsecuredPassword = ConvertToUnsecureString(sqlPassword);
            int matches = 0;
            foreach (Regex regex in passwordVerifiers)
            {
                if (regex.IsMatch(unsecuredPassword, 0))
                {
                    matches++;
                }
            }

            return sqlPassword.Length >= 8 && !unsecuredPassword.Contains(sqlUsername) && matches >= 3;
        }

        static bool IsSqlUsernameValid(string sqlUsername)
        {
            if (sqlUsername == null)
            {
                throw new ArgumentNullException("sqlUsername");
            }

            if (sqlUsername.Length > 117)
            {
                return false;
            }

            if (restrictedUsername.IsMatch(sqlUsername))
            {
                return false;
            }

            if (!validUsername.IsMatch(sqlUsername))
            {
                return false;
            }

            return true;
        }

        public static void EnsureSqlCredentialStrength(SecureString sqlPassword, string sqlUsername)
        {
            if (!IsSqlPasswordValid(sqlPassword, sqlUsername))
            {
                throw new ArgumentOutOfRangeException(Resources.WeakSqlPassword);
            }
        }

        public static void EnsureSqlUsernameValid(string sqlUsername)
        {
            if (!IsSqlUsernameValid(sqlUsername))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidSqlUsername);
            }
        }

        public void Validate()
        {
            if (!string.IsNullOrEmpty(this.ExistingSqlDatabase) && string.IsNullOrEmpty(this.ExistingSqlServer))
            {
                throw new InvalidOperationException(Resources.ExisingSqlDbRequiresExistingSqlServer);
            }

            if (string.IsNullOrEmpty(this.ServiceName))
            {
                throw new InvalidOperationException(Resources.MissingServiceName);
            }

            if (string.IsNullOrEmpty(this.SqlAdminUsername))
            {
                throw new InvalidOperationException(Resources.MissingSqlAdminUsername);
            }

            if (this.SqlAdminPassword == null || this.SqlAdminPassword.Length == 0)
            {
                throw new InvalidOperationException(Resources.MissingSqlAdminPassword);
            }

            if (!IsSqlUsernameValid(this.SqlAdminUsername))
            {
                throw new InvalidOperationException(Resources.InvalidSqlUsername);
            }

            if (!IsSqlPasswordValid(this.SqlAdminPassword, this.SqlAdminUsername))
            {
                throw new InvalidOperationException(Resources.WeakSqlAdminPassword);
            }
        }

        internal static string ConvertToUnsecureString(SecureString securePassword)
        {
            if (securePassword == null)
            {
                throw new ArgumentNullException("securePassword");
            }

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

    }
}

