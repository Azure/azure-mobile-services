// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#if NETFX_CORE
using Windows.UI;
using Windows.UI.Xaml.Media;
using System.Collections.Generic;
#endif

#if !NETFX_CORE
using System.Windows.Media;
using System.Collections.Generic;
#endif

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// UI model for a test method.
    /// </summary>
    public class TestDescription : INotifyPropertyChanged
    {
        private static Dictionary<Color, SolidColorBrush> BrushCache = new Dictionary<Color, SolidColorBrush>();
        private static Color DescriptionColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
        private string _name;
        
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private Color _color;

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged();
                OnPropertyChanged("Brush");
            }
        }

        public SolidColorBrush Brush
        {
            get 
            { 
                SolidColorBrush brush;
                if (!BrushCache.TryGetValue(this.Color, out brush))
                {
                    BrushCache[this.Color] = brush = new SolidColorBrush(this.Color);
                }
                return brush;
            }
        }

        public ObservableCollection<string> Details { get; private set; }

        public TestDescription()
        {
            Details = new ObservableCollection<string>();
            Color = DescriptionColor;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string memberName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(memberName));
            }
        }
    }
}
