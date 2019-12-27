using Newtonsoft.Json.Linq;

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

using sysAct = System.Action;

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
        }

        string serverAddress;
        /// <summary>
        /// 消息输出到文本框
        /// </summary>
        /// <param name="msg"></param>
        void Output(string msg)
        {
            OutputField.Dispatcher.Invoke(new sysAct(() =>
            {
                OutputField.AppendText("\n" + msg);
                OutputField.ScrollToEnd();
            }));
        }


        private void ConnectedButton_Click(object sender, RoutedEventArgs e)
        {
            Output("Starting Connected...");
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
                MsgSend(InPutField_msgType.Text, InPutField_msg.Text, long.Parse(InPutField_id.Text));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }

        //默认白名单文件
        void DefaultWhiteList()
        {
            JObject defWL = JObject.FromObject(new
            {
                setu = new
                {
                    group_id = new JArray { },
                    user_id = new JArray { },
                    discuss_id = new JArray { }
                },
            });
            File.WriteAllText(@"./config/WhiteList.json", defWL.ToString());

        }
        //默认配置文件
        void DefaultConfig()
        {
            JObject defCfg = JObject.FromObject(new
            {
                main = new
                {
                    loginAccont = 0,
                    serverAddress = @"ws://localhost:6700",
                    Is_setu_On = true,
                    Is_imageSearch_On = true,
                },
                setu = new
                {
                    reg = @"^咕咕(车?([色瑟]图)?(setu)?){1,}(来?([gG][kK][dD])?)$",
                    reg1 = @"^咕咕来点(车?([色瑟]图)?(setu)?(好康的)?){1,}",
                    regpic = "",
                    regpic1 = "",
                },
                imageSearch = new
                {

                }
            });
            File.WriteAllText(@"./config/Config.json", defCfg.ToString());
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAll();

        }
        void LoadAll()
        {
            DirectoryInfo data = new DirectoryInfo(@"./data");
            DirectoryInfo cfg = new DirectoryInfo(@"./config");
            if (data.Exists)
            {

            }
            else
            {
                //Directory.CreateDirectory(@"./data");
            }

            if (cfg.Exists)
            {
                IsCfgFileReady();
                IsWLFileReady();
            }
            else
            {
                Directory.CreateDirectory(@"./config");
                IsCfgFileReady();
                IsWLFileReady();
            }
        }
        //白名单准备就绪
        void IsWLFileReady()
        {
            FileInfo wl = new FileInfo(@"./config/WhiteList.json");

            if (wl.Exists)
            {
                WhiteListLoad();
            }
            else
            {
                DefaultWhiteList();
                WhiteListLoad();
            }
        }
        //配置文件准备就绪
        void IsCfgFileReady()
        {
            FileInfo c = new FileInfo(@"./config/Config.json");
            if (c.Exists)
            {
                ConfigLoad();
            }
            else
            {
                DefaultConfig();
                ConfigLoad();
            }
        }


        private void ReloadConfig_Btn_Click(object sender, RoutedEventArgs e)
        {
            Output("开始重载配置文件···");
            ConfigLoad();
            Output("重载结束");
        }

        private void ReloadWhiteList_Btn_Click(object sender, RoutedEventArgs e)
        {
            Output("开始重载白名单文件···");
            WhiteListLoad();
            Output("重载结束");
        }

        private void ServerAddressField_Loaded(object sender, RoutedEventArgs e)
        {
            ServerAddressField.Text = serverAddress;
        }
    }

}
