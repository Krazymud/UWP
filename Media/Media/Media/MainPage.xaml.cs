using System;
using System.ComponentModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Media
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>

    public sealed partial class MainPage : Page
    {

        MediaPlayer media;
        TimeSpan _duration;
        MediaTimelineController _mediaTimelineController;
        public MainPage()
        {
            InitializeComponent();
            media = new MediaPlayer();
            var mediaSource = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/song.mp3"));
            mediaSource.OpenOperationCompleted += MediaSource_OpenOperationCompleted;
            media.Volume = 50;
            _mediaTimelineController = new MediaTimelineController();
            _mediaTimelineController.PositionChanged += _mediaTimelineController_PositionChanged;
            media.CommandManager.IsEnabled = false;
            media.TimelineController = _mediaTimelineController;
            time.Text = "00:00:00";
            timeLeft.Text = "00:00:00";
            media.Source = mediaSource;
            player.SetMediaPlayer(media);
            volumeSlider.Value = 50;
        }
        private async void _mediaTimelineController_PositionChanged(MediaTimelineController sender, object args)
        {
            if (_duration != TimeSpan.Zero)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    playPos.Value = sender.Position.TotalSeconds;
                    TimeSpan left = _duration.Subtract(sender.Position);
                    time.Text = sender.Position.ToString(@"hh\:mm\:ss");
                    timeLeft.Text = left.ToString(@"hh\:mm\:ss");
                    TimeSpan temp = new TimeSpan(0);
                    if (playPos.Value == playPos.Maximum)
                    {
                        playPos.Value = 0;
                        media.TimelineController.Position = TimeSpan.FromSeconds(playPos.Value);
                        _mediaTimelineController.Pause();
                        time.Text = "00:00:00";
                        timeLeft.Text = _duration.ToString(@"hh\:mm\:ss");
                        VisualStateManager.GoToState(this, "Normal", false);
                        myStoryboard.Stop();
                    }
                });
            }
        }
        private async void MediaSource_OpenOperationCompleted(MediaSource sender, MediaSourceOpenOperationCompletedEventArgs args)
        {
            _duration = sender.Duration.GetValueOrDefault();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                playPos.Minimum = 0;
                playPos.Maximum = _duration.TotalSeconds;
                playPos.StepFrequency = 0.2;

            });
        }
        public void PosChange(object s, RoutedEventArgs e)
        {
            media.TimelineController.Position = TimeSpan.FromSeconds(playPos.Value);
        }
        public void VolumeChanged(object s, RoutedEventArgs e)
        {
            media.Volume = volumeSlider.Value / 100;
        }
        public void FullScreen(object s, RoutedEventArgs e)
        {
            player.IsFullWindow = (player.IsFullWindow == true) ? false : true;
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
            }
            else
            {
                view.TryEnterFullScreenMode();
            }
        }
        public void Play(object s, RoutedEventArgs e)
        {
            _mediaTimelineController.Resume();
            if (myStoryboard.GetCurrentState() == Windows.UI.Xaml.Media.Animation.ClockState.Stopped)
                myStoryboard.Begin();
            else
                myStoryboard.Resume();
            state.Text = "Now Playing...";
            play.Icon = new SymbolIcon(Symbol.Pause);
            play.Click -= Play;
            play.Click += Pause;
        }
        public void Pause(object s, RoutedEventArgs e)
        {
            _mediaTimelineController.Pause();
            myStoryboard.Pause();
            state.Text = "Now Pausing...";
            play.Icon = new SymbolIcon(Symbol.Play);
            play.Click -= Pause;
            play.Click += Play;
        }
        public void Stop(object s, RoutedEventArgs e)
        {
            playPos.Value = 0;
            media.TimelineController.Position = TimeSpan.FromSeconds(playPos.Value);
            _mediaTimelineController.Pause();
            time.Text = "00:00:00";
            timeLeft.Text = _duration.ToString(@"hh\:mm\:ss");
            VisualStateManager.GoToState(this, "Normal", false);
            myStoryboard.Stop();
            state.Text = "Stop";
            play.Icon = new SymbolIcon(Symbol.Play);
            play.Click -= Pause;
            play.Click += Play;
        }
        private async void Select(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".mkv");
            openPicker.FileTypeFilter.Add(".avi");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                if(file.FileType == ".mp3")
                {
                    zoom.IsEnabled = false;
                }
                else
                {
                    zoom.IsEnabled = true;
                }
                StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 600);
                if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(thumbnail);
                    SongPic.ImageSource = bitmapImage;
                }
                else
                {
                    BitmapImage temp = new BitmapImage(new Uri("ms-appx:///Assets/Vinyl.png"));
                    SongPic.ImageSource = temp;
                }
                MusicProperties musicProperties = await file.Properties.GetMusicPropertiesAsync();
                song.Text = musicProperties.Title + " - " + musicProperties.Artist;
                var mediaSource = MediaSource.CreateFromStorageFile(file);
                mediaSource.OpenOperationCompleted += MediaSource_OpenOperationCompleted;
                media.Source = mediaSource;
                playPos.Value = 0;
                media.TimelineController.Position = TimeSpan.FromSeconds(playPos.Value);
                time.Text = "00:00:00";
                timeLeft.Text = "00:00:00";
            }
        }
        private void OnFileOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;

            if (e.DragUIOverride != null)
            {
                e.DragUIOverride.Caption = "Add file";
                e.DragUIOverride.IsContentVisible = true;
            }
            
        }
        private async void OnFileDrop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var item = await e.DataView.GetStorageItemsAsync();
                var file = item[0] as StorageFile;
                if (file != null)
                {
                    if (file.FileType != ".mp3" && file.FileType != ".mp4" && file.FileType != ".mkv"
                        && file.FileType != ".avi") return;
                    if (file.FileType == ".mp3")
                    {
                        zoom.IsEnabled = false;
                    }
                    else
                    {
                        zoom.IsEnabled = true;
                    }
                    StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 600);
                    if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(thumbnail);
                        SongPic.ImageSource = bitmapImage;
                    }
                    else
                    {
                        BitmapImage temp = new BitmapImage(new Uri("ms-appx:///Assets/Vinyl.png"));
                        SongPic.ImageSource = temp;
                    }
                    MusicProperties musicProperties = await file.Properties.GetMusicPropertiesAsync();
                    song.Text = musicProperties.Title + " - " + musicProperties.Artist;
                    var mediaSource = MediaSource.CreateFromStorageFile(file);
                    mediaSource.OpenOperationCompleted += MediaSource_OpenOperationCompleted;
                    media.Source = mediaSource;
                    playPos.Value = 0;
                    media.TimelineController.Position = TimeSpan.FromSeconds(playPos.Value);
                    time.Text = "00:00:00";
                    timeLeft.Text = "00:00:00";
                }
            }
        }
    }
}
