using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ChatLiveClient.Commands;
using ChatLiveClient.Services.NetworkService;
using WPF_ClientTcp;

namespace ChatLiveClient.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Properties

        public TcpClient TcpClient { get; set; }


        private string clientMessage;

        public string ClientMessage
        {
            get { return clientMessage; }
            set { clientMessage = value; OnPropertyChanged(); }
        }


        private string clientName;

        public string ClientName
        {
            get { return clientName; }
            set { clientName = value; OnPropertyChanged(); }
        }


        private string displayName;

        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; OnPropertyChanged(); }
        }


        public BinaryReader BinaryReader { get; set; }

        public BinaryWriter BinaryWriter { get; set; }

        public bool IsConnected { get; set; }


        private string connectContent;

        public string ConnectContent
        {
            get { return connectContent; }
            set { connectContent = value; OnPropertyChanged(); }
        }

        public StackPanel MessagePanel { get; set; }

        #endregion

        ///---------------------------------------------------------------

        #region Commands
        public RelayCommand ConnectCommand { get; set; }
        public RelayCommand SendCommand { get; set; }
        public RelayCommand DisconnectCommand { get; set; }


        #endregion

        public DispatcherTimer messageReceiverTimer { get; set; }




        public async void ReceiveMessage()
        {
            messageReceiverTimer.Stop();
            await Task.Run(() =>
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {

                    try
                    {
                        var stream = TcpClient.GetStream();
                        BinaryReader = new BinaryReader(stream);
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                });
            });
        }



        public MainViewModel()
        {
            ConnectContent = "Connect";
            TcpClient = new TcpClient();
            var ip = IPAddress.Parse(IPService.GetLocalIPAddress());
            var port = 27001;
            var endPoint = new IPEndPoint(ip, port);

            messageReceiverTimer = new DispatcherTimer();
            messageReceiverTimer.Interval = TimeSpan.FromSeconds(3);
            messageReceiverTimer.Tick += MessageReceiverTimer_Tick;
            ConnectCommand = new RelayCommand((c) =>
            {
                Task.Run(() =>
                {
                    try
                    {
                        TcpClient.Connect(endPoint);
                        var stream = TcpClient.GetStream();
                        BinaryWriter = new BinaryWriter(stream);
                        BinaryWriter.Write(ClientName);
                        IsConnected = true;
                        messageReceiverTimer.Start();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("You are already connected or connection was not sucessfull");
                        ClientName = "";
                    }
                });
            }, (a) =>
            {
                if (!IsConnected)
                {
                    if (ClientName != null)
                    {
                        if (ClientName.Length != 0)
                        {
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                else if (IsConnected)
                    return false;
                return false;
            });



            SendCommand = new RelayCommand((c) =>
            {
                if (TcpClient.Connected)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                    {
                        var stream = TcpClient.GetStream();
                        BinaryWriter = new BinaryWriter(stream);
                        BinaryWriter.Write(ClientMessage);
                    });


                }
            });
        }

        private void MessageReceiverTimer_Tick(object sender, EventArgs e)
        {
            ReceiveMessage();
        }
    }
}
