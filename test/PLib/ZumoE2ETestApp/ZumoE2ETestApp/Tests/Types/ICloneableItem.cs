using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZumoE2ETestApp.Tests.Types
{
    public interface ICloneableItem<T> where T : ICloneableItem<T>
    {
        T Clone();
        object Id { get; set; }
    }
}
