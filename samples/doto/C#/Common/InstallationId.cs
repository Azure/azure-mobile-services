using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Doto
{
    /// <summary>
    /// Generates a persistant unique identifier for this installation that is persisted in
    /// local storage. This is used by Doto to manage channelUrls for push notifications
    /// </summary>
    public static class InstallationId
    {
        private static string _fileName = "installation-id.dat";
        private static string _value = null;
        private static object _lock = new object();

        public static async Task<string> GetInstallationId()
        {
            if (_value != null)
            {
                return _value;
            }

            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(_fileName, CreationCollisionOption.OpenIfExists);
            _value = await FileIO.ReadTextAsync(file);
            if (string.IsNullOrWhiteSpace(_value))
            {
                _value = Guid.NewGuid().ToString();
                await FileIO.WriteTextAsync(file, _value);
            }

            return _value;
        }
    }
}
