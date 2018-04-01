using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace App1.ViewModels
{
    class ListItemViewModels
    {
        private static ListItemViewModels instance;
        private ListItemViewModels() { }
        public static ListItemViewModels GetInstance()
        {
            if (instance == null)
            {
                instance = new ListItemViewModels();
            }
            return instance;
        }
        //单例模式
        
        public ObservableCollection<Models.ListItem> allItems = new ObservableCollection<Models.ListItem>();
        public void AddToDoItem(string title, string description, ImageSource source, DateTimeOffset date)
        {
            this.allItems.Add(new Models.ListItem(title, description, source, date));
        }

        public void RemoveTodoItem(string id)
        {
            var item = allItems.FirstOrDefault(i => i.GetId() == id);
            if (item != null)
            {
                allItems.Remove(item);
            }
        }

        public void UpdateTodoItem(string id, string title, string description, ImageSource source, DateTimeOffset date)
        {
            var item = allItems.FirstOrDefault(i => i.GetId() == id);
            if(item != null)
            {
                item.title = title;
                item.description = description;
                item.Image_Source = source;
                item.date = date;
            }
        }
    }
}
