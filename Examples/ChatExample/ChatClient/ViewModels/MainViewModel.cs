using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ChatClient.Extensions;
using ChatClient.Utilities;
using ChatSharedComps;
using ChatSharedComps.Messaging;
using Fluffy.Net;

namespace ChatClient.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly FluffyClient _client;
        private readonly List<ChatUser> _chatUsers;
        private string _messages;
        private string _title;
        private string _message;

        public ObservableCollection<ChatUser> Users { get; }

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Just cause I`m lazy
        /// </summary>
        public string Messages
        {
            get => _messages;
            set
            {
                if (value == _messages) return;
                _messages = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                if (value == _message) return;
                _message = value;
                OnPropertyChanged();
            }
        }

        public ICommand SendCommand => new RelayCommand(OnSendClicked);

        public MainViewModel(FluffyClient client, List<ChatUser> chatUsers)
        {
            _client = client;
            Users = new ObservableCollection<ChatUser>(chatUsers);

            _client.Connection.PacketHandler.On<Message>().Do(x => MessageReceived(x));
            _client.Connection.PacketHandler.On<ChatUserJoined>().Do(x => ChatUserJoined(x));
            _client.Connection.PacketHandler.On<ChatUserLeft>().Do(x => ChatUserLeft(x));
            //ChatUserJoined
        }

        private void ChatUserJoined(ChatUserJoined x)
        {
            DispatchService.Invoke(() => Users.Add(x));
        }

        private void ChatUserLeft(ChatUserLeft user)
        {
            DispatchService.Invoke(() => Users.Remove(x => x?.Identifier == user.Identifier));
        }

        private void MessageReceived(Message obj)
        {
            var sender = Users.FirstOrDefault(x => x.Identifier == obj.Sender)?.UserName ?? "Unknown";

            Messages += $"[{DateTime.Now:t}] {sender}: {obj.Text}\n";
        }

        private async void OnSendClicked(object obj)
        {
            var message = new MessageRequest
            {
                MessageType = MessageType.BroadCast,
                Message = new Message
                {
                    Sender = Guid.Empty,
                    Text = Message
                }
            };
            var result = await _client.Connection.Sender.Send<BaseResponse>(message);

            if (!result)
            {
                MessageBox.Show("An error occurred while sending the message");
                return;
            }
            MessageReceived(message.Message);
            Message = string.Empty;
        }
    }
}