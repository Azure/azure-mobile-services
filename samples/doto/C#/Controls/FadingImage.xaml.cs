using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Doto.Controls
{
    /// <summary>
    /// Simple user control to allow a data bound image to fade between images when changed
    /// </summary>
    public sealed partial class FadingImage : UserControl
    {
        public FadingImage()
        {
            this.InitializeComponent();
        }

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(FadingImage), new PropertyMetadata(default(ImageSource), SourceChanged));

        private static void SourceChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            FadingImage instance = (FadingImage)source;
            instance.ImageA.Source = (ImageSource)args.OldValue;
            instance.ImageB.Source = (ImageSource)args.NewValue;
            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 1;
            da.Duration = new Duration(TimeSpan.FromMilliseconds(instance.TransitionMilliseconds));
            Storyboard sb = new Storyboard();
            Storyboard.SetTarget(da, instance.ImageB);
            Storyboard.SetTargetProperty(da, "Opacity");
            sb.Children.Add(da);
            sb.Begin();
        }

        public int TransitionMilliseconds
        {
            get { return (int)GetValue(TransitionMillisecondsProperty); }
            set { SetValue(TransitionMillisecondsProperty, value); }
        }

        public static readonly DependencyProperty TransitionMillisecondsProperty =
            DependencyProperty.Register("TransitionMilliseconds", typeof(int), typeof(FadingImage), new PropertyMetadata(2000));
    }
}