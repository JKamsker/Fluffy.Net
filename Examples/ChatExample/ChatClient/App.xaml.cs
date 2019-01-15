using System;
using System.Windows;
using ChatClient.Extensions;
using ChatClient.ViewModels;
using ChatClient.Views;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            var application = new App();
            application.InitializeComponent();

            var wnd = new ConnectionView()
                .SetDataContext(ConnectionViewModel.Default)
                .Initialize();

            application.Run(wnd);
        }
    }
}