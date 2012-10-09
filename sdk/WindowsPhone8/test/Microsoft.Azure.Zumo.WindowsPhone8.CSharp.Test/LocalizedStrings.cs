using Microsoft.Azure.Zumo.WindowsPhone8.CSharp.Test.Resources;

namespace Microsoft.Azure.Zumo.WindowsPhone8.CSharp.Test
{
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        private static AppResources _localizedResources = new AppResources();

        public AppResources LocalizedResources { get { return _localizedResources; } }
    }
}