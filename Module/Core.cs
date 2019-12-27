using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using sysAct = System.Action;

namespace CQGuguBot
{
    public partial class MainWindow : Window
    {
        #region Websocket实现
        WebSocket4Net.WebSocket websocket;
        private void WebsocketIn()
        {
            websocket.Opened += async (s, j) =>
            {
                await Task.Run(new sysAct(() => Output("\n服务已启动")));
            };
            websocket.Error += async (s, j) =>
            {
                await Task.Run(new sysAct(() => Output("\nError:" + j.Exception.ToString())));
            };
            websocket.Closed += async (s, j) =>
            {
                await Task.Run(new sysAct(() => Output("\n关闭链接")));
            };
            websocket.MessageReceived += async (s, j) =>
            {
                await Task.Run(new sysAct(() =>
                {
                    Output("\n收到数据\n" + j.Message);
                    MsgParsing(j.Message);

                }));
            };
            websocket.Open();
        }
        #endregion


        #region 消息解析实现

        public long[] wlsetu_u;
        public long[] wlsetu_g;
        public long[] wlsetu_d;
        public long loginAccont;
        public string regSetu;
        public string regSetu1;
        public string regImageSearch;
        public bool Is_setu_On;
        public bool Is_imageSearch_On;
        /// <summary>
        /// 消息解析类
        /// </summary>
        /// <param name="msg"></param>
        public void MsgParsing(string msg)
        {


            S2J s2j = JsonConvert.DeserializeObject<S2J>(msg);
            string msgtype = null;
            long id = 0;

            if (s2j.message_type == "private")
            {
                msgtype = "send_private_msg";
                id = s2j.user_id;
            }
            else if (s2j.message_type == "group")
            {
                msgtype = "send_group_msg";
                id = s2j.group_id;
            }
            else if (s2j.message_type == "discuss")
            {
                msgtype = "send_discuss_msg";
                id = s2j.discuss_id;
            }

            if (s2j.message == null)
            {


            }


            if (s2j.message != null)//开始套娃
            {
                try
                {
                    //搜图
                    string regImageSearch = @"^(\[CQ:at,qq=" + loginAccont.ToString() + @"\]\s\[CQ:image)";
                    if (Regex.IsMatch(s2j.message, regImageSearch) && Is_imageSearch_On)
                    {

                    }
                }
                catch (Exception)
                {


                }

                //setu
                if (((Regex.IsMatch(s2j.message, regSetu) || Regex.IsMatch(s2j.message, regSetu1))) && (Is_setu_On))
                {
                    if (wlsetu_g.Length == 0 || wlsetu_u.Length == 0 || wlsetu_d.Length == 0 || wlsetu_g.Contains(id) || wlsetu_u.Contains(id) || wlsetu_d.Contains(id))
                    {
                        SendSetu(s2j, msgtype, id);
                    }
                    else
                    {
                        MsgSend(msgtype, "不被允许的操作", id);
                    }
                }
            }
        }

        void SendSetu(S2J s2j, string msgtype, long id)
        {
            string urlSetu = @"https://api.lolicon.app/setu/?r18=2";
            try
            {
                string s = GetResponseString(CreateGetHttpResponse(urlSetu));
                Debug.WriteLine(s);
                JObject o = JObject.Parse(s);
                string url = (string)o["data"][0]["url"];
                string msgat = @"[CQ:at,qq=" + s2j.user_id + "]";
                string msginfo = @"[PID:" + (string)o["data"][0]["pid"] + "][URL:" + url + "]";
                if (msgtype == "send_private_msg")
                {
                    MsgSend(msgtype, msginfo, id);
                }
                else
                {
                    MsgSend(msgtype, msgat + msginfo, id);
                }
                MsgSend(msgtype, "[CQ:image,file=" + url + "]", id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }


        public void MsgSend(string type, string msg, long id)
        {
            JObject j2s = null;
            if (type == "send_private_msg")
            {
                j2s = JObject.FromObject(new
                {
                    action = type,
                    @params = new
                    {
                        user_id = id,
                        message = msg
                    }
                });
            }
            else if (type == "send_group_msg")
            {
                j2s = JObject.FromObject(new
                {
                    action = type,
                    @params = new
                    {
                        group_id = id,
                        message = msg
                    }
                });
            }
            else if (type == "send_discuss_msg")
            {
                j2s = JObject.FromObject(new
                {
                    action = type,
                    @params = new
                    {
                        discuss_id = id,
                        message = msg
                    }
                });
            }

            websocket.Send(j2s.ToString());
            Debug.WriteLine(j2s.ToString());
        }

        public void MsgDel(long id)
        {
            JObject j = JObject.FromObject(new
            {
                action = "delete_msg",
                @params = new
                {
                    message_id = id
                }
            });
            Thread.Sleep(120000);
            websocket.Send(j.ToString());
            Debug.WriteLine(j.ToString());
        }
        #region 配置文件读取
        public void ConfigLoad()
        {
            try
            {
                JObject o = JObject.Parse(File.ReadAllText(@"./config/Config.json"));
                regSetu = (string)o["setu"]["reg"];
                regSetu1 = (string)o["setu"]["reg1"];
                loginAccont = (long)o["main"]["loginAccont"];
                Is_setu_On = (bool)o["main"]["Is_setu_On"];
                Is_imageSearch_On = (bool)o["main"]["Is_imageSearch_On"];
                serverAddress = (string)o["main"]["serverAddress"];
                Output("配置加载完成");
            }
            catch (Exception ex)
            {
                Output("配置加载失败");
                Output(ex.Message);
            }

        }




        #endregion


        #region 白名单配置读取
        public void WhiteListLoad()
        {
            try
            {
                Dictionary<string, Dictionary<string, int[]>> aa = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int[]>>>(File.ReadAllText(@"./config/WhiteList.json"));
                wlsetu_u = new long[aa["setu"]["user_id"].Length];
                for (int i = 0; i < aa["setu"]["user_id"].Length; i++)
                {
                    wlsetu_u[i] = Convert.ToInt32(aa["setu"]["user_id"].GetValue(i));
                }
                wlsetu_g = new long[aa["setu"]["group_id"].Length];
                for (int i = 0; i < aa["setu"]["group_id"].Length; i++)
                {
                    wlsetu_g[i] = Convert.ToInt32(aa["setu"]["group_id"].GetValue(i));
                }
                wlsetu_d = new long[aa["setu"]["discuss_id"].Length];
                for (int i = 0; i < aa["setu"]["discuss_id"].Length; i++)
                {
                    wlsetu_d[i] = Convert.ToInt32(aa["setu"]["discuss_id"].GetValue(i));
                }
                Output("白名单加载完成");
            }
            catch (Exception ex)
            {
                Output("白名单加载失败");
                Output(ex.Message);
            }

        }

        #endregion



        #region S2J,J2S


        public class S2J
        {
            public long group_id { get; set; }
            public long message_id { get; set; }
            public long user_id { get; set; }
            public long discuss_id { get; set; }
            public string message_type { get; set; }
            public string post_type { get; set; }
            public int times { get; set; }
            public string message { get; set; }
            public int retcode { get; set; }
            public string status { get; set; }
        }
        //public class J2S
        //{
        //    public string action { get; set; }
        //    public Params @params { get; set; }
        //}
        //public class Params
        //{
        //    public int user_id { get; set; }
        //    public int group_id { get; set; }
        //    public int discuss_id { get; set; }
        //    public string message { get; set; }
        //}

        #endregion
        #endregion


        #region HttpGet方法


        public string HttpGet(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36";
            request.Timeout = 30000;
            request.CookieContainer = new CookieContainer();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        /// <summary> 
        /// 创建GET方式的HTTP请求 
        /// </summary> 
        //public static HttpWebResponse CreateGetHttpResponse(string url, int timeout, string userAgent, CookieCollection cookies)
        public static HttpWebResponse CreateGetHttpResponse(string url)
        {
            HttpWebRequest request = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                //对服务端证书进行有效性校验（非第三方权威机构颁发的证书，如自己生成的，不进行验证，这里返回true）
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;    //http版本，默认是1.1,这里设置为1.0
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "GET";

            //设置代理UserAgent和超时
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36";
            request.Timeout = 30000;
            request.CookieContainer = new CookieContainer();

            return request.GetResponse() as HttpWebResponse;
        }
        /// <summary>
        /// 获取请求的数据
        /// </summary>
        public static string GetResponseString(HttpWebResponse webresponse)
        {
            using (Stream s = webresponse.GetResponseStream())
            {
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                return reader.ReadToEnd();

            }
        }

        /// <summary>
        /// 验证证书
        /// </summary>
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;
            return false;
        }
        #endregion
    }
}

