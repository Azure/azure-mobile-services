using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.ViewModels
{
    public class NetworkInformationDelegate
    {
        private Func<bool> getter;
        private Action<bool> setter;

        public NetworkInformationDelegate(Func<bool> getter, Action<bool> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public bool IsOnline
        {
            get { return getter(); }
            set { this.setter(value); }
        }
    }
}
