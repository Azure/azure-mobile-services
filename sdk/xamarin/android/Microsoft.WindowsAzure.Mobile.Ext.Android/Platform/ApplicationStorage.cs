using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Preferences;

namespace Microsoft.WindowsAzure.MobileServices
{
    class ApplicationStorage : IApplicationStorage
    {
         /// <summary>
        /// A singleton instance of the <see cref="ApplicationStorage"/>.
        /// </summary>
        private static IApplicationStorage instance = new ApplicationStorage();

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

            using (ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (Application.Context))
            {
                string svalue = prefs.GetString (name, null);
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
        }

        public void WriteSetting (string name, object value)
        {
            if (String.IsNullOrWhiteSpace (name))
                throw new ArgumentException (Resources.IApplicationStorage_NullOrWhitespaceSettingName, "name");

            using (ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (Application.Context))
            using (ISharedPreferencesEditor editor = prefs.Edit())
            {
                string svalue = null;
                if (value != null) {
                    TypeCode type = Type.GetTypeCode (value.GetType());
                    if (type == TypeCode.Object || type == TypeCode.DBNull)
                        throw new ArgumentException ("Settings of type " + type + " are not supported");
                    else
                        svalue = value.ToString();

                    svalue = String.Format ("{0}:{1}", type, svalue);
                }

                editor.PutString (name, svalue);
                editor.Commit();
            }
        }
    }
}