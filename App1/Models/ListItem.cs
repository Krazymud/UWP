using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace App1.Models
{
    class ListItem : INotifyPropertyChanged
    {
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private double IconWidth = 80;
        public double width
        {
            get{
                return IconWidth;
            }
            set{
                IconWidth = value;
                NotifyPropertyChanged("width");
            }
        }
        private bool is_checked;
        private string id;
        private string title_;
        private string description_;
        public string title
        {
            get
            {
                return title_;
            }
            set
            {
                title_ = value;
                NotifyPropertyChanged("title");
            }
        }
        public string description
        {
            get
            {
                return description_;
            }
            set
            {
                description_ = value;
                NotifyPropertyChanged("description");
            }
        }
        public bool completed
        {
            get
            {
                return is_checked;
            }
            set
            {
                is_checked = value;
                NotifyPropertyChanged("completed");
            }
        }
        private ImageSource source;
        public ImageSource Image_Source
        {
            get
            {
                return source;
            }
            set
            {
                source = value;
                NotifyPropertyChanged("Image_Source");
            }
        }
        public DateTimeOffset date { get; set; }
        

        public string GetId()
        {
            return id;
        }
        public ListItem(string title, string description, ImageSource source, DateTimeOffset date)
        {
            this.id = Guid.NewGuid().ToString(); // 生成id
            this.title = title;
            this.description = description;
            this.is_checked = false;
            this.Image_Source = source;
            this.date = date;
        }
    }
}
