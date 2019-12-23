using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using sysAct = System.Action;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Security;
using System.IO;
using System.Security.Cryptography.X509Certificates;

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

        public int[] wlsetu_u;
        public int[] wlsetu_g;
        public int[] wlsetu_d;
        /// <summary>
        /// 消息解析类
        /// </summary>
        /// <param name="msg"></param>
        public void MsgParsing(string msg)
        {
            S2J s2j = JsonConvert.DeserializeObject<S2J>(msg);
            string msgtype=null;
            int id=0;
            string regSetu = @"^咕咕(车?([色瑟]图)?(setu)?){1,}(来?([gG][kK][dD])?)$";
            string regSetu1 = @"^咕咕来点(车?([色瑟]图)?(setu)?(好康的)?){1,}";
            string urlSetu = @"https://api.lolicon.app/setu/?r18=2";
            Regex regSetu00 = new Regex(@"^咕咕(车?([色瑟]图)?(setu)?){1,}(来?([gG][kK][dD])?)$");
            Regex regSetu01 = new Regex(@"^咕咕来点(车?([色瑟]图)?(setu)?(好康的)?){1,}");

            if (s2j.message_type=="private")
            {
                msgtype = "send_private_msg";
                id = s2j.user_id;
            }
            else if (s2j.message_type=="group")
            {
                msgtype = "send_group_msg";
                id = s2j.group_id;
            }
            else if (s2j.message_type=="discuss")
            {
                msgtype = "send_discuss_msg";
                id = s2j.discuss_id;
            }
            

            if (s2j.message != null)//开始套娃
            {
                if ((Regex.IsMatch(s2j.message, regSetu)||Regex.IsMatch(s2j.message, regSetu1)))
                {
                    if (wlsetu_g.Length == 0 || wlsetu_u.Length == 0 || wlsetu_d.Length == 0 || wlsetu_g.Contains(id) || wlsetu_u.Contains(id) || wlsetu_d.Contains(id))
                    {
                        string s = GetResponseString(CreateGetHttpResponse(urlSetu));
                        JObject o = JObject.Parse(s);
                        string url = (string)o["data"][0]["url"];
                        string msgat = @"[CQ:at,qq=" + s2j.user_id + "]";
                        string msginfo = @"[PID:" + (string)o["data"][0]["pid"] + "][URL:" + url + "]";
                        if (msgtype == "send_private_msg")
                        {
                            MsgSend(msgtype,msginfo, id);
                        }
                        else
                        {
                            MsgSend(msgtype, msgat + msginfo, id);
                        }
                        MsgSend(msgtype, "[CQ:image,file=" + url + "]", id);
                    }
                    else
                    {
                        MsgSend(msgtype, "不被允许的操作", id);
                    }
                }
                else if (true)
                {

                }
            }
        }
        public void MsgSend(string type,string msg,int id)
        {
            JObject j2s = null;
            if (type == "send_private_msg")
            {
                j2s = JObject.FromObject(new
                {
                    action = type,
                    @params = new
                    {
                        user_id=id,
                        message=msg
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

        #region 白名单实现

        void WLSetu_u()
        {
            Dictionary<string, Dictionary<string, int[]>> aa = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int[]>>>(File.ReadAllText(@"./config/WhiteList.json"));
            wlsetu_u = new int[aa["setu"]["user_id"].Length];
            for (int i = 0; i < aa["setu"]["user_id"].Length; i++)
            {
                wlsetu_u[i]=Convert.ToInt32(aa["setu"]["user_id"].GetValue(i));
            }
        }
        void WLSetu_g()
        {
            Dictionary<string, Dictionary<string, int[]>> aa = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int[]>>>(File.ReadAllText(@"./config/WhiteList.json"));
            wlsetu_g = new int[aa["setu"]["group_id"].Length];
            for (int i = 0; i < aa["setu"]["group_id"].Length; i++)
            {
                wlsetu_g[i] = Convert.ToInt32(aa["setu"]["group_id"].GetValue(i));
            }
        }
        void WLSetu_d()
        {
            Dictionary<string, Dictionary<string, int[]>> aa = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int[]>>>(File.ReadAllText(@"./config/WhiteList.json"));
            wlsetu_d = new int[aa["setu"]["discuss_id"].Length];
            for (int i = 0; i < aa["setu"]["discuss_id"].Length; i++)
            {
                wlsetu_d[i] = Convert.ToInt32(aa["setu"]["discuss_id"].GetValue(i));
            }
        }
        public void WhiteListLoad()
        {
            WLSetu_u();
            WLSetu_g();
            WLSetu_d();
        }

        #endregion

        #region S2J,J2S


        public class S2J
        {
            public int group_id { get; set; }
            public int message_id { get; set; }
            public int user_id { get; set; }
            public int discuss_id { get; set; }
            public string message_type { get; set; }
            public string post_type { get; set; }
            public int times { get; set; }
            public string message { get; set; }
        }
        public class J2S
        {
            public string action { get; set; }
            public Params @params { get; set; }
        }
        public class Params
        {
            public int user_id { get; set; }
            public int group_id { get; set; }
            public int discuss_id { get; set; }
            public string message { get; set; }
        }

        #endregion
        #endregion


        #region HttpGet方法


        public string HttpGet(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "application/json";

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
            //request.UserAgent = userAgent;
            //request.Timeout = timeout;
            //if (cookies != null)
            //{
            //    request.CookieContainer = new CookieContainer();
            //    request.CookieContainer.Add(cookies);
            //}
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

