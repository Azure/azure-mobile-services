using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZumoE2ETestApp.Framework
{
    public class ZumoTestGlobals
    {
        public const string RoundTripTableName = "w8RoundTripTable";
        public const string MoviesTableName = "w8Movies";
        public const string PushTestTableName = "w8PushTest";

        private static ZumoTestGlobals instance = new ZumoTestGlobals();

        public MobileServiceClient Client { get; private set; }

        private ZumoTestGlobals() { }

        public static ZumoTestGlobals Instance
        {
            get { return instance; }
        }

        public void InitializeClient(string appUrl, string appKey)
        {
            bool needsUpdate = this.Client == null ||
                (this.Client.ApplicationUri.ToString() != appUrl) ||
                (this.Client.ApplicationKey != appKey);

            if (needsUpdate)
            {
                if (string.IsNullOrEmpty(appUrl) || string.IsNullOrEmpty(appKey))
                {
                    throw new ArgumentException("Please enter valid application URL and key.");
                }

                this.Client = new MobileServiceClient(appUrl, appKey);
            }
        }
    }
}
