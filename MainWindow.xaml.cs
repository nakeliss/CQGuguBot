using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Net.Http;
using SuperSocket.ClientEngine;
using WebSocket4Net;
using System.Threading;
using Newtonsoft.Json;
using sysAct = System.Action;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.IO;

namespace CQGuguBot
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            ServerAddressField.Text = $"ws://localhost:6700";
        }
 
        /// <summary>
        /// 消息输出到文本框
        /// </summary>
        /// <param name="msg"></param>
        void Output(string msg)
        {
            this.OutputField.Dispatcher.Invoke(new sysAct(() =>
            {
                this.OutputField.AppendText(msg);
                this.OutputField.ScrollToEnd();
            }));
        }


        private void ConnectedButton_Click(object sender, RoutedEventArgs e)
        {
            Output("\nStarting Connected...");
            ConnectedButton.IsEnabled = false;
            DisconnectedButton.IsEnabled = true;
            websocket = new WebSocket4Net.WebSocket(ServerAddressField.Text);
            WebsocketIn();
        }

        private void DisconnectedButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectedButton.IsEnabled = true;
            DisconnectedButton.IsEnabled = false;
            websocket.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MsgSend(InPutField_msgType.Text, InPutField_msg.Text,int.Parse(InPutField_id.Text) );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }


        public void DefaultWhiteList()
        {
            JObject defWL = JObject.FromObject(new
            {
                setu = new
                {
                    group_id = new JArray { },
                    user_id=new JArray { },
                    discuss_id=new JArray { }
                },
            }) ;
            File.WriteAllText(@"./config/WhiteList.json", defWL.ToString());

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Window_Loaded");
            DirectoryInfo data = new DirectoryInfo(@"./data");
            DirectoryInfo cfg = new DirectoryInfo(@"./config");
            if (data.Exists)
            {
                
            }
            else
            {
                Directory.CreateDirectory(@"./data");
            }

            if (cfg.Exists)
            {
                FileInfo f = new FileInfo(@"./config/WhiteList.json");
                if (f.Exists)
                {
                    WhiteListLoad();
                }
                else
                {
                    DefaultWhiteList();
                    WhiteListLoad();
                }
            }
            else
            {
                Directory.CreateDirectory(@"./config");
            }
            
        }


    }

}
