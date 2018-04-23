using System;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace NetworkApi
{
    public sealed partial class NavPage : Page
    {
        private void initializeFrostedGlass(UIElement glassHost) //毛玻璃
        {
            Visual hostVisual = ElementCompositionPreview.GetElementVisual(glassHost);
            Compositor compositor = hostVisual.Compositor;
            var backdropBrush = compositor.CreateHostBackdropBrush();
            var glassVisual = compositor.CreateSpriteVisual();
            glassVisual.Brush = backdropBrush;
            ElementCompositionPreview.SetElementChildVisual(glassHost, glassVisual);
            var bindSizeAnimation = compositor.CreateExpressionAnimation("hostVisual.Size");
            bindSizeAnimation.SetReferenceParameter("hostVisual", hostVisual);
            glassVisual.StartAnimation("Size", bindSizeAnimation);
        }
        private void Page1(object s, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage), null);
        }
        private void Page2(object s, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage1), null);
        }
        private void Page3(object s, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage2), null);
        }
        public NavPage()
        {
            InitializeComponent();
            initializeFrostedGlass(GlassHost);
        }

    }
}
