using System;
using Windows.Data.Xml.Dom;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace NetworkApi
{
    public sealed partial class MainPage2 : Page
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
        public MainPage2()
        {
            InitializeComponent();
            initializeFrostedGlass(GlassHost);
        }
        private async void Get(object s, RoutedEventArgs e)
        {
            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();
            var headers = httpClient.DefaultRequestHeaders;

            Uri requestUri = new Uri("http://api.avatardata.cn/Nba/NomalRace?Key=96b12dbac2094f3ea0f14edbbb83d300&dtype=xml");
            Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
            XmlDocument data = new XmlDocument();
            string httpResponseBody = "";
            try
            {
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                data.LoadXml(httpResponseBody);
                ConvertToRes(data);
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
        }
        private void ConvertToRes(XmlDocument data)
        {
            XmlNodeList list = data.GetElementsByTagName("NbaNomalDetailObj");
            for(uint i = 0; i < list.Length; i++)
            {
                IXmlNode item = list.Item(i);
                string player1 = item.SelectSingleNode("player1").InnerText;
                string player2 = item.SelectSingleNode("player2").InnerText;
                string score = item.SelectSingleNode("score").InnerText;
                string status = item.SelectSingleNode("status").InnerText;
                string time = item.SelectSingleNode("time").InnerText;
                if (status == "1")
                    status = "直播中";
                else if (status == "2")
                    status = "已结束";
                else
                    status = "未开赛";
                string res = time + " " + player1 + score + player2 + " " + status + "\n\n";
                result.Text += res;
            }
        }
    }
}
