using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Doto
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public BoolToVisibilityConverter()
        {
            VisibleValue = true;
        }

        public bool VisibleValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool interpreted = System.Convert.ToBoolean(value);
            return interpreted == VisibleValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null || value.GetType() != typeof(Visibility))
            {
                return !VisibleValue;
            }

            Visibility visibility = (Visibility)value;

            return visibility == Visibility.Visible ? VisibleValue : !VisibleValue;

        }
    }
}
