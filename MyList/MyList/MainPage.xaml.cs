using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using MyList.ViewModels;
using MyList.Models;
using System.ComponentModel;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.ApplicationModel.Core;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using Windows.UI.ViewManagement;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.ApplicationModel.DataTransfer;
using DataAccessLibrary;
using Windows.ApplicationModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MyList
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    internal static class ByteArrayBitmapExtensions
    {
        public static async Task<byte[]> AsByteArray(this StorageFile file)
        {
            IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
            var reader = new DataReader(fileStream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)fileStream.Size);

            byte[] pixels = new byte[fileStream.Size];

            reader.ReadBytes(pixels);

            return pixels;
        }
        public static async Task<BitmapImage> AsBitmapImage(this StorageFile file)
        {
            var stream = await file.OpenAsync(FileAccessMode.Read);
            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(stream);
            return bitmapImage;
        }
        public static BitmapImage AsBitmapImage(this byte[] byteArray)
        {
            if (byteArray != null)
            {
                using (var stream = new InMemoryRandomAccessStream())
                {
                    stream.WriteAsync(byteArray.AsBuffer()).GetResults(); // I made this one synchronous on the UI thread; this is not a best practice.
                    var image = new BitmapImage();
                    stream.Seek(0);
                    image.SetSource(stream);
                    return image;
                }
            }

            return null;
        }
    }


    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private string id;
        private int mode = 0;
        ListItemViewModels myViewModels = ListItemViewModels.GetInstance();
        public event PropertyChangedEventHandler PropertyChanged;
        private ListItem NowItem;
        private StorageFile CurrentPic;
        private StorageFile DefaultPic;
        //左边部分
        public static void UpdateTile(string title, string des, DateTimeOffset date)
        {
            string sdate = date.ToString("D");
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(System.IO.File.ReadAllText("Tile.xml"));
            XmlNodeList textElements = xmlDoc.GetElementsByTagName("text");
            foreach (var text in textElements)
            {
                if (text.InnerText.Equals("title"))
                {
                    text.InnerText = title;
                }
                else if (text.InnerText.Equals("des"))
                {
                    text.InnerText = des;
                }
                else if (text.InnerText.Equals("date"))
                {
                    text.InnerText = sdate;
                }
            }
            var notification = new TileNotification(xmlDoc);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
        } //更新磁贴
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        } //绑定
        private void ShareItem(object sender, RoutedEventArgs e)  //分享
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            var originalSource = e.OriginalSource as MenuFlyoutItem;
            ListItem data = (ListItem)originalSource.DataContext;
            NowItem = data;
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }
        private async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = NowItem.title;
            request.Data.SetText(NowItem.description + "\n" + NowItem.date.ToString("D"));
            DataRequestDeferral deferral = request.GetDeferral();
            List<string> data = DataAccess.GetData();
            byte[] thispic = null;
            int i = 0;
            while (i < data.Count)
            {
                if (data[i] == NowItem.GetId())
                {
                    thispic = Convert.FromBase64String(data[i + 5]);
                    break;
                }
                i += 6;
            }
            StorageFile picfile = await ApplicationData.Current.LocalFolder.CreateFileAsync("tempic", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(picfile, thispic);
            request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(picfile));
            deferral.Complete();
        }
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
        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            bool suspending = ((App)App.Current).issuspend;
            if (suspending)
            {
                var composite = new ApplicationDataCompositeValue();
                for (int i = 0; i < ListItemViewModels.GetInstance().allItems.Count(); i++)
                {
                    var list = ListItemViewModels.GetInstance().allItems;

                    DataAccess.UpdateData(list[i].GetId(), (list[i].completed == true ? 1 : 0));
                    composite["ischecked" + i] = ListItemViewModels.GetInstance().allItems[i].completed;
                }
                composite["title"] = textBox.Text;
                composite["des"] = textBox1.Text;
                composite["date"] = datepicker.Date;
                composite["mode"] = mode;
                composite["id"] = id;
                ApplicationData.Current.LocalSettings.Values["newpage"] = composite;
                ((App)App.Current).issuspend = false;
                await CurrentPic.CopyAsync(ApplicationData.Current.LocalFolder, "hangpic", NameCollisionOption.ReplaceExisting);
            }
        } //挂起操作
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                ApplicationData.Current.LocalSettings.Values.Remove("newpage");
            }
            else
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("newpage"))
                {
                    var composite = ApplicationData.Current.LocalSettings.Values["newpage"] as ApplicationDataCompositeValue;
                    for (int i = 0; i < ListItemViewModels.GetInstance().allItems.Count(); i++)
                    {
                        ListItemViewModels.GetInstance().allItems[i].completed = (bool)composite["ischecked" + i];
                    }
                    if ((int)composite["mode"] == 1)
                    {
                        button.Content = "Update";
                        button.Click -= Create;
                        button.Click += Update;
                        DeleteItem.Visibility = Visibility.Visible;
                    }
                    textBox.Text = (string)composite["title"];
                    textBox1.Text = (string)composite["des"];
                    id = (string)composite["id"];
                    datepicker.Date = (DateTimeOffset)composite["date"];
                    StorageFile pic = await ApplicationData.Current.LocalFolder.GetFileAsync("hangpic");
                    BitmapImage img = await ByteArrayBitmapExtensions.AsBitmapImage(pic);
                    image.Source = img;
                    ApplicationData.Current.LocalSettings.Values.Remove("newpage");
                    list.SelectedItem = ListItemViewModels.GetInstance().allItems.FirstOrDefault(j => j.GetId() == id);
                }
            }
        }
        private async void InitializeFile()
        {
            StorageFile imageFile = await Package.Current.InstalledLocation.GetFileAsync("Assets\\IMG_0245.jpg");
            DefaultPic = CurrentPic = imageFile;
        }
        private void LoadItem(List<string> entries)
        {
            int i = 0;
            while (i < entries.Count)
            {
                ListItem item = ListItemViewModels.GetInstance().allItems.FirstOrDefault(j => j.GetId() == entries[i]);
                if (item == null)
                {
                    byte[] picbytes = Convert.FromBase64String(entries[i + 5]);
                    BitmapImage img = ByteArrayBitmapExtensions.AsBitmapImage(picbytes);
                    var dateres = DateTime.Parse(entries[i + 3]);
                    ListItemViewModels.GetInstance().AddToDoItem(entries[i + 1],
                        entries[i + 2], img, dateres, null, entries[i]);
                    ListItem temp = ListItemViewModels.GetInstance().allItems.FirstOrDefault(j => j.GetId() == entries[i]);
                    temp.completed = (entries[i + 4] == "1" ? true : false);
                    UpdateTile(entries[i + 1], entries[i + 2], DateTime.Parse(entries[i + 3]));
                }
                else
                {
                    item.title = entries[i + 1];
                    item.description = entries[i + 2];
                    item.date = DateTime.Parse(entries[i + 3]);
                    item.Image_Source = ByteArrayBitmapExtensions.AsBitmapImage(Convert.FromBase64String(entries[i + 5]));
                }
                i += 6;
            }
        }
        public MainPage()
        {
            this.InitializeComponent();
            DataAccess.InitializeDatabase();
            InitializeFile();
            LoadItem(DataAccess.GetData());
            var view = ApplicationView.GetForCurrentView();
            // active
            view.TitleBar.BackgroundColor = Colors.LightGray;
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = false;
            initializeFrostedGlass(GlassHost);
            this.SizeChanged += trigger;
        }
        private void trigger(object sender, SizeChangedEventArgs e)
        {
            ListItemViewModels viewmodel = ListItemViewModels.GetInstance();
            if (Frame.ActualWidth < 600)
            {
                foreach (var item in viewmodel.allItems)
                {
                    item.width = 0;
                }
            }
            else
            {
                foreach (var item in viewmodel.allItems)
                {
                    item.width = 80;
                }
            }
        } //visualstate
        private async void ListItem_ItemClicked(object sender, ItemClickEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            if (frame.ActualWidth < 800)
                this.Frame.Navigate(typeof(MainPage1), (e.ClickedItem as ListItem).GetId());
            else
            {
                mode = 1;
                ListItem item = e.ClickedItem as ListItem;
                id = item.GetId();
                button.Content = "Update";
                button.Click -= Create;
                button.Click += Update;
                textBox.Text = item.title;
                textBox1.Text = item.description;
                datepicker.Date = item.date;
                image.Source = item.Image_Source;
                DeleteItem.Visibility = Visibility.Visible;
                List<string> data = DataAccess.GetData();
                byte[] thispic = null;
                int i = 0;
                while (i < data.Count)
                {
                    if (data[i] == id)
                    {
                        thispic = Convert.FromBase64String(data[i + 5]);
                        break;
                    }
                    i += 6;
                }
                StorageFile picfile = await ApplicationData.Current.LocalFolder.CreateFileAsync("tempic", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBytesAsync(picfile, thispic);
                CurrentPic = picfile;
            }
        } //item点击事件
        private async void ErrorDialog(int mode)
        {
            string message;
            if (mode == 0)
                message = "信息不全";
            else
                message = "时间错误";
            var dialog = new ContentDialog()
            {
                Title = "错误",
                Content = message,
                PrimaryButtonText = "确定",
                FullSizeDesired = false,
            };
            await dialog.ShowAsync();
        } //创建更新不成功
        private void Update(object sender, RoutedEventArgs e)
        {
            if (textBox.Text == "" || textBox1.Text == "")
            {
                ErrorDialog(0);
            }
            else if (datepicker.Date < DateTime.Now)
            {
                if (datepicker.Date.Year != DateTime.Now.Year || datepicker.Date.Month != DateTime.Now.Month || datepicker.Date.Day != DateTime.Now.Day)
                {
                    ErrorDialog(1);
                }
                else
                    CreateDialog(1);
            }
            else
                CreateDialog(1);
        } //更新item
        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            CreateDialog(2);
        } //删除item
        private void Lock(object sender, RoutedEventArgs e)
        {
            scroller.ChangeView(null, 30, null);
        }
        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame frame = Window.Current.Content as Frame;
            if (frame.ActualWidth < 800)
                this.Frame.Navigate(typeof(MainPage1), null);
        } //创建item跳转
        private async void ItemSearch(object sender, RoutedEventArgs e)
        {
            List<string> res = DataAccess.search(search.Text);
            if (res.Count != 0)
            {
                StringBuilder mystr = new StringBuilder();
                int i = 0;
                while (i < res.Count)
                {
                    mystr.Append(res[i]);
                    mystr.Append(" ");
                    mystr.Append(res[i + 1]);
                    mystr.Append(" ");
                    mystr.Append(res[i + 2]);
                    mystr.Append(" ");
                    if (res[i + 3] == "1")
                        mystr.Append("√");
                    else
                        mystr.Append("×");
                    mystr.Append("\n");
                    i += 4;
                }
                var dialog = new ContentDialog()
                {
                    Title = "结果",
                    Content = mystr,
                    CloseButtonText = "确定"
                };
                await dialog.ShowAsync();
            }
            else
            {
                var dialog = new ContentDialog()
                {
                    Title = "结果",
                    Content = "未找到内容!",
                    CloseButtonText = "确定"
                };
                await dialog.ShowAsync();
            }
        } //查询item
        //右边部分
        private void Create(object sender, RoutedEventArgs e)
        {
            if (textBox.Text == "" || textBox1.Text == "")
            {
                ErrorDialog(0);
            }
            else if (datepicker.Date < DateTime.Now)
            {
                if (datepicker.Date.Year != DateTime.Now.Year || datepicker.Date.Month != DateTime.Now.Month || datepicker.Date.Day != DateTime.Now.Day)
                {
                    ErrorDialog(1);
                }
                else
                    CreateDialog(0);
            }
            else
                CreateDialog(0);
        }  //创建item
        private async void CreateDialog(int mode) 
        {
            ListItemViewModels myViewModels = ListItemViewModels.GetInstance();
            ImageSource source = image.Source;
            if (mode == 0)
            {
                ListItem item = new ListItem(textBox.Text, textBox1.Text, source, datepicker.Date, CurrentPic);
                myViewModels.allItems.Add(item);
                UpdateTile(textBox.Text, textBox1.Text, datepicker.Date);
                byte[] picbyte = await ByteArrayBitmapExtensions.AsByteArray(CurrentPic == null ? DefaultPic : CurrentPic);
                string res = Convert.ToBase64String(picbyte);
                DataAccess.AddData(item.GetId(), item.title, item.description, item.date.ToString("d"), (item.completed ? 1 : 0), res);
                CurrentPic = DefaultPic;
            }
            else if (mode == 1)
            {
                myViewModels.UpdateTodoItem(id, textBox.Text, textBox1.Text, source, datepicker.Date, CurrentPic);
                byte[] picbyte = await ByteArrayBitmapExtensions.AsByteArray(CurrentPic == null ? DefaultPic : CurrentPic);
                string res = Convert.ToBase64String(picbyte);
                DataAccess.UpdateData(id, textBox.Text, textBox1.Text, datepicker.Date.ToString("d"), 0, res);
                CurrentPic = DefaultPic;
            }
            else
            {
                myViewModels.RemoveTodoItem(id);
                DataAccess.DeleteData(id);
            }
            var dialog = new ContentDialog()
            {
                Title = "成功",
                PrimaryButtonText = "确定",
                FullSizeDesired = false,
            };
            await dialog.ShowAsync();
            this.id = null;
            button.Content = "Create";
            if (mode == 1 || mode == 2)
            {
                button.Click -= Update;
                button.Click += Create;
            }
            textBox.Text = "";
            textBox1.Text = "";
            datepicker.Date = DateTime.Now;
            image.Source = new BitmapImage(new Uri("ms-appx:///Assets/IMG_0245.JPG"));
            DeleteItem.Visibility = Visibility.Collapsed;
            list.SelectedItem = null;
            mode = 0;
        }  //管理创建更新删除
        private void Cancel(object sender, RoutedEventArgs e)
        {
            textBox.Text = "";
            textBox1.Text = "";
            datepicker.Date = DateTime.Now;
        }  //清空创建
        private async void Select(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            StorageFile file = await openPicker.PickSingleFileAsync();
            CurrentPic = file;
            if (file != null)
            {
                SoftwareBitmap softwareBitmap;
                IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                    softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                {
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }
                var source = new SoftwareBitmapSource();
                await source.SetBitmapAsync(softwareBitmap);
                image.Source = source;
            }
        }  //更改图片
    }
}
