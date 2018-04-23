using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using MyList.ViewModels;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.ApplicationModel.Core;
using System.Linq;
using DataAccessLibrary;
using MyList.Models;
using Windows.ApplicationModel;
using System.Collections.Generic;
// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MyList
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage1 : Page
    {
        private string id;
        private StorageFile CurrentPic;
        private StorageFile DefaultPic;
        int mode;
        private void initializeFrostedGlass(UIElement glassHost)
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
        private async void InitializeFile()
        {
            StorageFile imageFile = await Package.Current.InstalledLocation.GetFileAsync("Assets\\IMG_0245.jpg");
            DefaultPic = CurrentPic = imageFile;
        }
        public MainPage1()
        {
            this.InitializeComponent();
            var view = ApplicationView.GetForCurrentView();
            // active
            InitializeFile();
            view.TitleBar.BackgroundColor = Colors.LightGray;
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = false;
            initializeFrostedGlass(GlassHost);
        }
        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            bool suspending = ((App)App.Current).issuspend;
            if (suspending)
            {
                var composite = new ApplicationDataCompositeValue();
                composite["mode"] = mode;
                composite["title"] = textBox.Text;
                composite["des"] = textBox1.Text;
                composite["date"] = datepicker.Date;
                composite["id"] = id;
                ApplicationData.Current.LocalSettings.Values["newpage"] = composite;
                ((App)App.Current).issuspend = false;
                await CurrentPic.CopyAsync(ApplicationData.Current.LocalFolder, "hangpic", NameCollisionOption.ReplaceExisting);
            }
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                if (e.Parameter != null)
                {
                    mode = 1;
                    var item = ListItemViewModels.GetInstance().allItems.FirstOrDefault(j => j.GetId() == e.Parameter as String);
                    id = e.Parameter as String;
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
                    button.Content = "Update";
                    button.Click -= Create;
                    button.Click += Update;
                    textBox.Text = item.title;
                    textBox1.Text = item.description;
                    datepicker.Date = item.date;
                    image.Source = item.Image_Source;
                    DeleteItem.Visibility = Visibility.Visible;
                }
                else
                {
                    DeleteItem.Visibility = Visibility.Collapsed;
                    mode = 0;
                }
                base.OnNavigatedTo(e);
                ApplicationData.Current.LocalSettings.Values.Remove("newpage");
            }
            else
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("newpage"))
                {
                    var composite = ApplicationData.Current.LocalSettings.Values["newpage"] as ApplicationDataCompositeValue;
                    if ((int)composite["mode"] == 1)
                    {
                        mode = 1;
                        button.Content = "Update";
                        button.Click -= Create;
                        button.Click += Update;
                        DeleteItem.Visibility = Visibility.Visible;
                    }
                    else
                        mode = 0;
                    textBox.Text = (string)composite["title"];
                    textBox1.Text = (string)composite["des"];
                    datepicker.Date = (DateTimeOffset)composite["date"];
                    id = (string)composite["id"];
                    StorageFile pic = await ApplicationData.Current.LocalFolder.GetFileAsync("hangpic");
                    CurrentPic = pic;
                    BitmapImage img = await ByteArrayBitmapExtensions.AsBitmapImage(pic);
                    image.Source = img;
                    ApplicationData.Current.LocalSettings.Values.Remove("newpage");
                }
            }
        }

        private async void Update(object sender, RoutedEventArgs e)
        {
            if (textBox.Text == "" || textBox1.Text == "")
            {
                var dialog = new ContentDialog()
                {
                    Title = "错误",
                    Content = "信息不全",
                    PrimaryButtonText = "确定",
                    FullSizeDesired = false,
                };
                await dialog.ShowAsync();
            }
            else if (datepicker.Date < DateTime.Now)
            {
                if (datepicker.Date.Year != DateTime.Now.Year || datepicker.Date.Month != DateTime.Now.Month || datepicker.Date.Day != DateTime.Now.Day)
                {
                    var dialog = new ContentDialog()
                    {
                        Title = "错误",
                        Content = "时间错误",
                        PrimaryButtonText = "确定",
                        FullSizeDesired = false,
                    };
                    await dialog.ShowAsync();
                }
                else
                    CreateDialog(1);
            }
            else
                CreateDialog(1);
        }

        private async void Create(object sender, RoutedEventArgs e)
        {
            if (textBox.Text == "" || textBox1.Text == "")
            {
                var dialog = new ContentDialog()
                {
                    Title = "错误",
                    Content = "信息不全",
                    PrimaryButtonText = "确定",
                    FullSizeDesired = false,
                };
                await dialog.ShowAsync();
            }
            else if (datepicker.Date < DateTime.Now)
            {
                if (datepicker.Date.Year != DateTime.Now.Year || datepicker.Date.Month != DateTime.Now.Month || datepicker.Date.Day != DateTime.Now.Day)
                {
                    var dialog = new ContentDialog()
                    {
                        Title = "错误",
                        Content = "时间错误",
                        PrimaryButtonText = "确定",
                        FullSizeDesired = false,
                    };
                    await dialog.ShowAsync();
                }
                else
                    CreateDialog(0);
            }
            else
                CreateDialog(0);
        }
        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            CreateDialog(2);
        }
        private async void CreateDialog(int mode)
        {
            ListItemViewModels myViewModels = ListItemViewModels.GetInstance();
            ImageSource source = image.Source;
            if (mode == 0)
            {
                ListItem item = new ListItem(textBox.Text, textBox1.Text, source, datepicker.Date, CurrentPic);
                myViewModels.allItems.Add(item);
                MainPage.UpdateTile(textBox.Text, textBox1.Text, datepicker.Date);
                byte[] picbyte = await ByteArrayBitmapExtensions.AsByteArray(CurrentPic == null ? DefaultPic : CurrentPic);
                string res = Convert.ToBase64String(picbyte);
                DataAccess.AddData(item.GetId(), item.title, item.description, item.date.ToString("d"), (item.completed ? 1 : 0), res);
                CurrentPic = null;
            }
            else if (mode == 1)
            {
                byte[] mybytes = await ByteArrayBitmapExtensions.AsByteArray(CurrentPic);
                string res = Convert.ToBase64String(mybytes);
                DataAccess.UpdateData(id, textBox.Text, textBox1.Text, datepicker.Date.ToString("d"), res);
                myViewModels.UpdateTodoItem(id, textBox.Text, textBox1.Text, source, datepicker.Date);
            }
            else
                myViewModels.RemoveTodoItem(id);
            var dialog = new ContentDialog()
            {
                Title = "成功",
                PrimaryButtonText = "确定",
                FullSizeDesired = false,
            };
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.GoBack();
            }
        }
        private void Cancel(object sender, RoutedEventArgs e)
        {
            textBox.Text = "";
            textBox1.Text = "";
            datepicker.Date = DateTime.Now;
        }
        private void Lock(object sender, RoutedEventArgs e)
        {
            scroller.ChangeView(null, 30, null);
        }
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
        }
    }
}
