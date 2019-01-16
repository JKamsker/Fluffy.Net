using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Input;
using ChatClient.Extensions;
using ChatClient.Models;
using ChatClient.Utilities;
using ChatSharedComps.Login;
using Fluffy.Net;

namespace ChatClient.ViewModels
{
    public class ConnectionViewModel : BaseViewModel
    {
        public static ConnectionViewModel Default
            => new ConnectionViewModel
            {
                Servers = new ObservableCollection<EndpointModel>
                {
                    new EndpointModel
                    {
                        Address = IPAddress.Parse("127.0.0.1"),
                        Port = 8001,
                        Description = "LocalHost"
                    },
                    new EndpointModel
                    {
                        Address = IPAddress.Parse("37.59.53.54"),
                        Port = 8001,
                        Description = "TestServer"
                    }
                }
            };

        private string _name;
        private string _password;
        private EndpointModel _selectedEndpoint;
        private volatile bool _connecting = false;

        public ObservableCollection<EndpointModel> Servers { get; private set; }

        public EndpointModel SelectedEndpoint
        {
            get => _selectedEndpoint ?? (_selectedEndpoint = Servers?.FirstOrDefault());
            set
            {
                if (Equals(value, _selectedEndpoint))
                {
                    return;
                }

                _selectedEndpoint = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name)
                {
                    return;
                }

                _name = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (value == _password)
                {
                    return;
                }

                _password = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConnectCommand
        {
            get => new RelayCommand(x => OnConnectClick(x), x => !_connecting);
        }

        private async void OnConnectClick(object callingWindow)
        {
            if (string.IsNullOrEmpty(Name))
            {
                MessageBox.Show("Name cannot be null");
                return;
            }

            if (SelectedEndpoint == null)
            {
                MessageBox.Show("No endpoint selected");
                return;
            }

            _connecting = true;

            var client = new FluffyClient(SelectedEndpoint.Address, SelectedEndpoint.Port);
            await client.ConnectAsync();
            client.Connection.OnDisposing += (sender, info) =>
            {
                MessageBox.Show("Server Shutdown");
                Environment.Exit(0);
            };

            var response = await client.Connection.Sender.Send<LoginResponse>(new LoginRequest
            {
                Name = _name,
                Password = _password
            });
            _connecting = false;
            if (response.Success)
            {
                var window = new MainWindow
                {
                    DataContext = new MainViewModel(client, response.ChatUsers)
                    {
                        Title = Name,
                    }
                }.Initialize();
                window.Show();
                if (callingWindow is Window wnd)
                {
                    wnd.Close();
                }
            }
        }
    }
}