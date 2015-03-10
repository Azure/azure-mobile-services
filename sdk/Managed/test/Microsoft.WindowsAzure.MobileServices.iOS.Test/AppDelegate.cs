using System.Reflection;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Test;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Foundation;
using UIKit;
using CoreGraphics;

namespace MicrosoftWindowsAzureMobileiOSTest
{
	[Foundation.Register("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		UIWindow window;

	    public static TestHarness Harness { get; private set; }

	    static AppDelegate()
	    {
            CurrentPlatform.Init();

	        Harness = new TestHarness();
            Harness.LoadTestAssembly(typeof(MobileServiceSerializerTests).GetTypeInfo().Assembly);
	    }

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow ((CGRect)UIScreen.MainScreen.Bounds);
		    window.RootViewController = new UINavigationController (new LoginViewController());
			window.MakeKeyAndVisible();

			return true;
		}
	}
}