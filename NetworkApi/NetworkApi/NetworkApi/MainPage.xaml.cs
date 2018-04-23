using System;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace NetworkApi
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
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
        public MainPage()
        {
            InitializeComponent();
            initializeFrostedGlass(GlassHost);
        }
        
        private async void Get(object s, RoutedEventArgs e)
        {
            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();
            var headers = httpClient.DefaultRequestHeaders;

            Uri requestUri = new Uri("https://free-api.heweather.com/s6/weather/now?key=e5daff72968a4a94933166c86eee6fbc&location=" + textBox.Text);
            Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
            string httpResponseBody = "";
            try
            {
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                ConvertToRes(httpResponseBody);
            }
            catch(Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
            
        }
        private void ConvertToRes(String data)
        {
            String[] wea = new string[7];
            int[] count = new int[7];
            count[0] = data.IndexOf("location") + 11;
            count[1] = data.IndexOf("admin_area") + 13;
            count[2] = data.IndexOf("cnty") + 7;
            count[3] = data.IndexOf("cond_txt") + 11;
            count[4] = data.IndexOf("tmp") + 6;
            count[5] = data.IndexOf("wind_dir") + 11;
            count[6] = data.IndexOf("wind_spd") + 11;
            for(int i = 0; i < 7; i++)
            {
                int split = (data.IndexOf(',', count[i]) - 1) < 0 ? (data.IndexOf('}', count[i]) - 1) : (data.IndexOf(',', count[i]) - 1);
                wea[i] = data.Substring(count[i], split - count[i]);
            }
            String res = wea[2] + ", " + wea[1] + ", " + wea[0] + "\n" + wea[3] +
                ", " + wea[4] + "℃, " + wea[5] + wea[6] + "km/h";
            textBlock1.Text = res;
        }
    }
}
