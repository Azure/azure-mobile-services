namespace Microsoft.WindowsAzure.MobileServices
{
    public class CurrentPlatform : IPlatform
    {
        /// <summary>
        /// You must call this method from your application in order to ensure
        /// that this platform specific assembly is included in your app.
        /// </summary>
        public static void Init()
        {
        }

        IApplicationStorage IPlatform.ApplicationStorage
        {
            get { return ApplicationStorage.Instance; }
        }

        IPlatformInformation IPlatform.PlatformInformation
        {
            get { return PlatformInformation.Instance; }
        }

        IExpressionUtility IPlatform.ExpressionUtility
        {
            get { return ExpressionUtility.Instance; }
        }
    }
}