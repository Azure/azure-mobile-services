using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

#if NETFX_CORE
using Windows.UI.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif
#if WINDOWS_PHONE
using System.Windows.Interactivity;
using System.Windows.Controls;
using System.Windows;
#endif

namespace Todo.Behavior
{
    public class CheckedBehavior : Behavior<CheckBox>
    {
        public static readonly DependencyProperty CommandProperty
            = DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(CheckedBehavior),
                new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)this.GetValue(CommandProperty); }
            set { this.SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty
            = DependencyProperty.Register(
                "CommandParameter",
                typeof(object),
                typeof(CheckedBehavior),
                new PropertyMetadata(null));

        public object CommandParameter
        {
            get { return this.GetValue(CommandParameterProperty); }
            set { this.SetValue(CommandParameterProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Checked += AssociatedObject_Checked;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.Checked -= AssociatedObject_Checked;
        }

        void AssociatedObject_Checked(object sender, RoutedEventArgs e)
        {
            if (this.Command != null)
            {
                this.Command.Execute(this.CommandParameter);
            }
        } 
    }
}
