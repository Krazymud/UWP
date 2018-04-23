using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml.Media;
namespace MyList.ViewModels
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
        public void AddToDoItem(string title, string description, ImageSource source, DateTimeOffset date, StorageFile pic = null, string id = null)
        {
            this.allItems.Add(new Models.ListItem(title, description, source, date, pic, id));
        }

        public void RemoveTodoItem(string id)
        {
            var item = allItems.FirstOrDefault(i => i.GetId() == id);
            if (item != null)
            {
                allItems.Remove(item);
            }
        }

        public void UpdateTodoItem(string id, string title, string description, ImageSource source, DateTimeOffset date, StorageFile pic = null)
        {
            var item = allItems.FirstOrDefault(i => i.GetId() == id);
            if (item != null)
            {
                item.title = title;
                item.description = description;
                item.Image_Source = source;
                item.date = date;
                item.PicFile = pic;
            }
        }
    }
}
