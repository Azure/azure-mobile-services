using System.Reflection;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore.Test.UnitTests;
using Microsoft.WindowsAzure.MobileServices.Test;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Foundation;
using UIKit;

namespace Microsoft.WindowsAzure.Mobile.SQLiteStore.iOS.Test
{
	[Register("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		UIWindow window;

	    public static TestHarness Harness { get; private set; }

	    static AppDelegate()
	    {
            CurrentPlatform.Init();
            SQLitePCL.CurrentPlatform.Init();

	        Harness = new TestHarness();
            Harness.LoadTestAssembly(typeof(SQLiteStoreTests).Assembly);
	    }

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds);
		    window.RootViewController = new UINavigationController (new LoginViewController());
			window.MakeKeyAndVisible();

			return true;
		}
	}
}