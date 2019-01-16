using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Markup;

namespace ChatClient.Extensions
{
    public static class WpfExtensions
    {
        public static T SetDataContext<T>(this T window, object dataContext) where T : Window
        {
            window.DataContext = dataContext;
            return window;
        }

        public static T Initialize<T>(this T window)
            where T : IComponentConnector
        {
            window.InitializeComponent();
            return window;
        }

        public static void Remove<T>(this ObservableCollection<T> collection, Predicate<T> predicate)
        {
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (predicate(collection[i]))
                {
                    collection.RemoveAt(i);
                }
            }
        }
    }
}