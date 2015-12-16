using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// When applied to a target (e.g. assembly, class, member, etc.), instructs the Xamarin linker to preserve that target
    /// in the linking process result.
    /// </summary>
    [AttributeUsage(System.AttributeTargets.All)]
    public sealed class PreserveAttribute : System.Attribute
    {
        public bool AllMembers;
        public bool Conditional;
    }
}
