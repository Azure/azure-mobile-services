﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
#if !NET45
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
#endif
#if !NETFX_CORE
using System.Windows;
#endif

namespace ZumoE2ETestApp.UIElements
{
    internal class AppInfoRepository
    {
        const string SavedAppsFileName = "AppInfo.txt";

        public static AppInfoRepository Instance = new AppInfoRepository();
        private SavedAppInfo savedAppInfo = null;

        private AppInfoRepository() { }

        public async Task<SavedAppInfo> GetSavedAppInfo()
        {
            if (savedAppInfo == null)
            {
                try
                {
#if NET45
                    using (var stream = await Task.FromResult(File.OpenRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SavedAppsFileName"))))
                    {
                        DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(SavedAppInfo));
                        savedAppInfo = (SavedAppInfo)dcjs.ReadObject(stream);
                    }
#else
                    var file = await ApplicationData.Current.LocalFolder.GetFileAsync(SavedAppsFileName);
                    using (var stream = await file.OpenStreamForReadAsync())
                    {
                        DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(SavedAppInfo));
                        savedAppInfo = (SavedAppInfo)dcjs.ReadObject(stream);
                    }
#endif
                }
                catch (Exception)
                {
                    // Cannot retrieve saved info
                    return new SavedAppInfo();
                }
            }

            return savedAppInfo;
        }

        public async Task SaveAppInfo(SavedAppInfo appInfo)
        {
            Exception ex = null;
            this.savedAppInfo = appInfo;
            try
            {
#if NET45
                using (var stream = await Task.FromResult(File.OpenWrite(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SavedAppsFileName"))))
                {
                    DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(SavedAppInfo));
                    dcjs.WriteObject(stream, appInfo);
                }
#else
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(SavedAppsFileName, CreationCollisionOption.ReplaceExisting);
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(SavedAppInfo));
                    dcjs.WriteObject(stream, appInfo);
                }
#endif
            }
            catch (Exception e)
            {
                ex = e;
            }

            if (ex != null)
            {
                string errorText = string.Format("{0}: {1}", ex.GetType().FullName, ex.Message);
#if NETFX_CORE
                await new MessageDialog(errorText, "Error saving app info").ShowAsync();
#else
                MessageBox.Show(errorText, "Error saving app info", MessageBoxButton.OK);
#endif
            }
        }
    }

    public class MobileServiceInfo
    {
        public string AppUrl { get; set; }
        public string AppKey { get; set; }
    }

    public class SavedAppInfo
    {
        public List<MobileServiceInfo> MobileServices;
        public MobileServiceInfo LastService;
        public string LastUploadUrl { get; set; }

        public SavedAppInfo()
        {
            this.MobileServices = new List<MobileServiceInfo>();
            this.LastService = new MobileServiceInfo { AppKey = "", AppUrl = "" };
            this.LastUploadUrl = "";
        }
    }
}
