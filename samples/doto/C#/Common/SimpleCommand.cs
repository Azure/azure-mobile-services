using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doto
{
    /// <summary>
    /// A simple, UI-technology-agnostic representation of a simple command for 
    /// use in Doto's dialogs and Settings
    /// </summary>
    public class SimpleCommand
    {
        public SimpleCommand()
        {
        }

        public SimpleCommand(string text, Action action)
        {
            Text = text;
            Action = action;
        }

        public string Text { get; set; }
        public Action Action { get; set; }
    }
}
