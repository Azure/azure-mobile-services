using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;

namespace iOSsample
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Foundation.Register("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		public override UIWindow Window {get; set;}
	}
}

