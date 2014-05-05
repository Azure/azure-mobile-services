using System;

using MonoTouch.Foundation;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class ApplicationStorage : IApplicationStorage
    {
        /// <summary>
        /// A singleton instance of the <see cref="ApplicationStorage"/>.
        /// </summary>
        private static readonly IApplicationStorage instance = new ApplicationStorage();

        /// <summary>
        /// A singleton instance of the <see cref="ApplicationStorage"/>.
        /// </summary>
        internal static IApplicationStorage Instance
        {
            get
            {
                return instance;
            }
        }

        public bool TryReadSetting (string name, out object value)
        {
            if (String.IsNullOrWhiteSpace (name))
                throw new ArgumentException (Resources.IApplicationStorage_NullOrWhitespaceSettingName, "name");

            value = null;

            var defaults = NSUserDefaults.StandardUserDefaults;
            string svalue = defaults.StringForKey (name);
            if (svalue == null)
                return false;

            try {
                int sepIndex = svalue.IndexOf (":");
                string valueStr = svalue.Substring (sepIndex + 1);
                TypeCode type = (TypeCode) Enum.Parse (typeof (TypeCode), svalue.Substring (0, sepIndex));
                value = Convert.ChangeType (valueStr, type);
            } catch (Exception) {
                return false;
            }

            return true;
        }

        public void WriteSetting (string name, object value)
        {
            if (String.IsNullOrWhiteSpace (name))
                throw new ArgumentException (Resources.IApplicationStorage_NullOrWhitespaceSettingName, "name");

            var defaults = NSUserDefaults.StandardUserDefaults;
            if (value == null)
            {
                defaults.SetString (null, name);
                return;
            }

            string svalue;

            TypeCode type = Type.GetTypeCode (value.GetType());
            if (type == TypeCode.Object || type == TypeCode.DBNull)
                throw new ArgumentException ("Settings of type " + type + " are not supported");
            else
                svalue = value.ToString();

            defaults.SetString (type + ":" + svalue, name);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}