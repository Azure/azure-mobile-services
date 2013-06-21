// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using ZumoE2ETestApp.Framework;

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
                    using (var stream = await Util.OpenAppSettingsForRead(SavedAppsFileName))
                    {
                        DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(SavedAppInfo));
                        savedAppInfo = (SavedAppInfo)dcjs.ReadObject(stream);
                    }
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
                using (var stream = await Util.OpenAppSettingsForWrite(SavedAppsFileName))
                {
                    DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(SavedAppInfo));
                    dcjs.WriteObject(stream, appInfo);
                }
            }
            catch (Exception e)
            {
                ex = e;
            }

            if (ex != null)
            {
                string errorText = string.Format("{0}: {1}", ex.GetType().FullName, ex.Message);
                await Util.MessageBox(errorText, "Error saving app info");
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
