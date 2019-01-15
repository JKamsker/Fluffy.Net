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
    }
}