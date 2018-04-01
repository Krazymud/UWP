using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using App1.Models;
using App1.ViewModels;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.ApplicationModel.Core;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace App1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage1 : Page
    {
        private string id;
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
        public MainPage1()
        {
            this.InitializeComponent();
            var view = ApplicationView.GetForCurrentView();
            // active
            view.TitleBar.BackgroundColor = Colors.LightGray;
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = false;
            initializeFrostedGlass(GlassHost);
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                ListItem item = e.Parameter as ListItem;
                this.id = item.GetId();
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
                DeleteItem.Visibility = Visibility.Collapsed;
            base.OnNavigatedTo(e);
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
                myViewModels.AddToDoItem(textBox.Text, textBox1.Text, source, datepicker.Date);
            else if(mode == 1)
                myViewModels.UpdateTodoItem(id, textBox.Text, textBox1.Text, source, datepicker.Date);
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
            if(file != null)
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