using System.Reflection;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Test;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MicrosoftWindowsAzureMobileiOSTest
{
	[Register("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		UIWindow window;

	    public static TestHarness Harness { get; private set; }

	    static AppDelegate()
	    {
            CurrentPlatform.Init();

	        Harness = new TestHarness();
            Harness.LoadTestAssembly(typeof(MobileServiceSerializerTests).GetTypeInfo().Assembly);
            Harness.LoadTestAssembly(typeof(PushFunctional).GetTypeInfo().Assembly);
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