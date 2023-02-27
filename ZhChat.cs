using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using WebSocketSharp;

namespace EasyBot
{


    public class ZhChat
    {


        int times = 0;
        DateTime first;
        DateTime last;
        bool canspeak = true;


        public Action<data_chat> OnMessage { get; set; }

        public Action OnOpen { get; set; }
        public bool IsDebug { get; set; } = true;
        public Action<CloseEventArgs> OnClose { get; set; }
        public Action<ErrorEventArgs> OnError { get; set; }
        public Action<UserInfo> UserAdd { get; set; }

        public List<string> OpList = new List<string> { };
        public List<string> nicks = new List<string>();
        public Action<List<UserInfo>> onlineSet { get; set; }

        public List<ZhChat.UserInfo> users = new List<ZhChat.UserInfo>();
        public UserInfo GetUserInfo(string any)
        {
            foreach (var item in users)
            {
                if (any == item.nick || any == item.trip)
                {
                    return item;
                }
            }
            return new UserInfo();
        }
        /// <summary>
        /// OP功能
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="istrip"></param>
        /// <returns></returns>
        /// 
        public void AddOP(string trip)
        {
            if (!OpList.Contains(trip))
            {
                OpList.Add(trip);
            }
        }
        public void RemoveOP(string trip)
        {
            if (OpList.Contains(trip))
            {
                OpList.Remove(trip);
            }
        }
        public bool IsinOPList(string arg)
        {

            string trip = GetUserInfo(arg).trip;
            return OpList.Contains(trip);


            //return false;
        }


        public string CommandPrefix { get; set; }

        public Dictionary<string, Action<string>> BaseCommand { get; set; }

        public Dictionary<string, Action<string>> _BaseCommand()
        {
            return /*Dictionary<string, Action<string>> baseCommand =*/ new Dictionary<string, Action<string>> {
                {"onlineSet",((k)=>{
                    JObject jb =JObject.Parse(k);
                  List<ZhChat.UserInfo>tmp=JsonConvert.DeserializeObject<List<ZhChat.UserInfo>>(jb["users"].ToString());
                    var New=from a in tmp
                            where !a.nick.Contains("挂机")
                            select a;
                    users=New.ToList();
                    var Nicks=New.Select(d=>d.nick);
                    nicks=Nicks.ToList();
                    onlineSet(users);
                     if (!IsDebug)
                        {
                            Console.WriteLine($"用户列表:{string.Join(",",nicks)}\r\n");
                        }
                }) },
                {"onlineAdd",((k)=>{
              var u=JsonConvert.DeserializeObject<ZhChat.UserInfo>(k);
                    if (users.Where(f => f.nick == k).Count()<=0)
                    {
                        users.Add(u);
                        if (UserAdd!= null){
                            UserAdd(u);
                        }
                        if (!IsDebug)
                        {
                            Console.WriteLine($"用户加入:{u.nick} [{u.trip}]");
                        }
                    }
                    if (!nicks.Contains(u.nick))nicks.Add(u.nick);

                }) },
                {"onlineRemove",((k)=>{
                JObject jb =JObject.Parse(k);
                          var g=users.Where(a=>a.nick==jb["nick"].ToString());
                   if(g.Count()==1){

                        if (!IsDebug)
                        {
                            Console.WriteLine($"用户离开:{g.First().nick} [{g.First().trip}]");
                        }
                    users.Remove(g.First());
                    // 

                    }
                }) },

            };
        }
        public static string ws_url = "wss://ws.zhangsoft.cf/";
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="_username">用户名</param>
        /// <param name="_password">密码</param>
        /// <param name="_token">密钥</param>
        /// <param name="_channel">频道</param>
        /// <param name="autoJoin">自动加入</param>
        public ZhChat(string _username, string _password, string _token, string _channel, string commandprefix, string _ws_url = "wss://xq.kzw.ink/ws", bool autoInit = true)
        {
            username = _username;
            password = _password;
            token = _token;
            CommandPrefix = commandprefix;
            channel = _channel;
            ws_url = _ws_url;
            if (autoInit)
                Init();


        }
        public ZhChat()
        {

        }
        public void Restart()
        {
            wss.Close();
            wss.Connect();
        }
        public void Disconnect()
        {
            wss.Close();
        }
        /// <summary>
        /// 使用config初始化
        /// </summary>
        /// <param name="bt">配置</param>
        public ZhChat(botConfig bt, bool autoinit = false)
        {
            username = bt.username;
            password = bt.password;
            channel = bt.channel;
            token = bt.token;
            header = bt.header;
            CommandPrefix = bt.CommandPreix;

            if (autoinit)
                Init();


        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            BaseCommand = _BaseCommand();

            wss = new WebSocket(ws_url);
            wss.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            wss.OnMessage += new EventHandler<MessageEventArgs>((a, b) =>
            {
                string json = b.Data;
                if (IsDebug)
                    Console.WriteLine(b.Data);
                JObject jb = JObject.Parse(json);
                if (jb.ContainsKey("cmd"))
                {

                    string cmd = jb["cmd"].ToString();
                    if (BaseCommand.ContainsKey(jb["cmd"].ToString()))
                    {
                        BaseCommand[jb["cmd"].ToString()](json);
                    }
                    if (cmd == "chat")
                    {


                        ZhChat.data_chat dc = JsonConvert.DeserializeObject<ZhChat.data_chat>(json);
                        if (OnMessage != null) OnMessage(dc);


                    }
                    if (cmd == "warn")
                    {

                        Console.WriteLine($"服务器向客户端报告错误:{jb["text"].ToString()}");

                    }
                    if (cmd == "info")
                    {
                        if (jb.ContainsKey("text"))
                        {
                            if (!IsDebug)
                            {
                                Console.WriteLine($"[系统信息]:{jb["text"]}");

                            }
                        }
                        if (jb.ContainsKey("type"))
                        {
                            string type = jb["type"].ToString();
                            if (type == "whisper" && jb.ContainsKey("from"))
                            {

                                string nick = jb["from"].ToString();
                                string trip = jb["trip"].ToString();
                                string msg = jb["msg"].ToString();
                                long level = GetUserInfo(nick).level;
                                //  bool isbot = jb["isbot"].ToObject<bool>();
                                data_chat dc = new data_chat
                                {
                                    isWhisper = true,
                                    nick = nick,
                                    text = msg,
                                    trip = trip,
                                    level = level

                                };

                                if (OnMessage != null) OnMessage(dc);




                            }
                        }
                    }
                }



            });
            wss.OnOpen += new EventHandler((o, e) =>
            {
                new Thread(() =>
                {
                    while (true)
                    {

                        Thread.Sleep(60 * 1000); wss.Send("{\"cmd\":\"ping\"}");
                    }
                }).Start();
                wss.Send(GetJoin(username, password, channel, token));

                if (OnOpen != null) OnOpen();


            });
            wss.OnError += new EventHandler<ErrorEventArgs>((a, b) =>
            {
                if (OnError != null) OnError(b);

            });
            wss.OnClose += new EventHandler<CloseEventArgs>((a, b) =>
            {
                try
                {
                    wss.Connect();
                }
                catch { }
                if (OnClose != null) OnClose(b);

            });
            try
            {
                wss.Connect();
            }
            catch (Exception e)
            {
                Console.WriteLine("连接失败。原因:" + e.Message + "\r\n请尝试重新连接。");
            }
        }
        /// <summary>
        /// 加入
        /// </summary>
        /// <param name="_channel">若频道未空则必填</param>
        public void Join(string _channel = "")
        {
            if (wss == null) { Init(); return; }
            if (string.IsNullOrEmpty(channel))
            {
                if (string.IsNullOrEmpty(_channel))
                {
                    throw new Exception("频道未设置。");
                }
                else
                {
                    channel = _channel;
                    wss.Send(GetJoin(username, password, channel, token));
                }
            }
            else
            {
                wss.Send(GetJoin(username, password, channel, token));
            }
        }
        /// <summary>
        /// websocket
        /// </summary>
        public WebSocket wss { get; set; }
        /// <summary>
        /// 头像地址
        /// </summary>
        public string header { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string password { get; set; }
        /// <summary>
        /// 密钥
        /// </summary>
        public string token { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// 频道
        /// </summary>
        public string channel { get; set; }
        public static data_chat ParseChat(string json)
        {
            return JsonConvert.DeserializeObject<data_chat>(json);
        }

        public void ChangeName(string uname)
        {
            username = uname;
            wss.Send(this.GetSendMessage($"/nick {uname}"));
        }

        public void MoveTo(string newChannel)
        {
            channel = newChannel;
            wss.Close();
            wss.Connect();
        }

        /// <summary>
        /// 获取加入的json字符串
        /// </summary>
        /// <param name="uname">用户名</param>
        /// <param name="pwd">密码</param>
        /// <param name="channel">频道</param>
        /// <param name="token">密钥</param>
        /// <returns></returns>
        public string GetJoin(string uname, string pwd, string channel, string token)
        {
            return JsonConvert.SerializeObject(new data_join
            {
                channel = channel,
                nick = uname,
                password = pwd,
                cmd = "join",
                show = "0",
                head = header,
                token = token,
                // client_key = "easybot_js"
            });
        }
        /// <summary>
        /// 获取聊天的json
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string GetSendMessage(string text)
        {
            return JsonConvert.SerializeObject(new data_chat
            {
                cmd = "chat",
                nick = username,
                head = header,
                show = "0",
                text = text
            });
        }
        /// <summary>
        /// 发送消息(带ws)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ws"></param>
        public void GetSendMessage(string text, WebSocket ws)
        {
            ws.Send(JsonConvert.SerializeObject(new data_chat
            {
                cmd = "chat",
                nick = username,
                head = header,
                show = "0",
                text = text
            }));
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="text"></param>
        public void SendMsg(string text)
        {
            if (canspeak)
                wss.Send(JsonConvert.SerializeObject(new data_chat
                {
                    cmd = "chat",
                    nick = username,
                    head = header,
                    show = "0",
                    text = text
                }));
        }
        public void SendMsgTo(string text, string name)
        {
            if (canspeak)
                wss.Send(JsonConvert.SerializeObject(new data_chat
                {
                    cmd = "chat",
                    nick = username,
                    head = header,
                    show = "0",
                    text = $"/w {name} {text}"
                }));
        }
        public void SendMsg2(string text, string _header)
        {
            if (canspeak)
                wss.Send(JsonConvert.SerializeObject(new data_chat
                {
                    cmd = "chat",
                    nick = username,
                    head = _header,
                    show = "0",
                    text = text
                }));
        }
        /// <summary>
        /// 发送消息(私信)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="uname"></param>
        public void SendMsg(string text, string uname)
        {
            if (canspeak)
                wss.Send(JsonConvert.SerializeObject(new data_chat
                {
                    cmd = "chat",
                    nick = username,
                    head = header,
                    show = "0",
                    text = $"/w {uname} {text}"
                }));
        }
        /// <summary>
        /// 将消息通过ws发送
        /// </summary>
        /// <param name="text">消息</param>
        /// <param name="uname">用户名</param>
        /// <param name="ws">ws连接</param>
        public void GetSendMessage(string text, string uname, WebSocket ws)
        {
            ws.Send(JsonConvert.SerializeObject(new data_chat
            {
                cmd = "chat",
                nick = username,
                head = header,
                show = "0",
                text = $"/w {uname} {text}"
            }));
        }
        /// <summary>
        /// 设置头像
        /// </summary>
        /// <param name="Header"></param>
        public void SetHeader(string Header)
        {
            header = Header;
        }
        /// <summary>
        /// 设置密钥
        /// </summary>
        /// <param name="Token"></param>
        public void SetToken(string Token)
        {
            token = Token;
        }
        /// <summary>
        /// 
        /// </summary>
        public class data_chat
        {
            public string cmd { get; set; }
            public string head { get; set; }
            public string nick { get; set; }
            public string show { get; set; }
            public string text { get; set; }
            public string trip { get; set; }
            public string mod { get; set; }
            public long level { get; set; }
            public bool isWhisper { get; set; }
            public int xc { get; set; }
        }
        public class UserInfo
        {

            public string nick { get; set; }
            public string trip { get; set; }
            public string utype { get; set; }
            public string hash { get; set; }
            public string userid { get; set; }
            public long level { get; set; }
            public string isbot { get; set; }
            public bool mod { get; set; }
            public long time { get; set; }
        }

        public class PeopleInfo
        {
            public string cmd { get; set; }
            public List<string> nicks { get; set; }
            public List<UserInfo> users { get; set; }


        }
        public class data_join
        {
            public string channel { get; set; }
            public string cmd { get; set; }
            public string head { get; set; }
            public string nick { get; set; }
            public string password { get; set; }
            public string show { get; set; }
            public string token { get; set; }
            public string client_key { get; set; }
        }

        public class get_old_msg
        {
            public string cmd { get; set; }
            public string num { get; set; }
        }


    }
    public class botConfig
    {
        public string token { get; set; }
        public string channel { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string CommandPreix { get; set; }
        public string header { get; set; }

    }
}


