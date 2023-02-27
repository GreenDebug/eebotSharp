using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyBot;
using json_ = Newtonsoft.Json.JsonConvert;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.IO;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using RestSharp;
using Newtonsoft.Json;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;

namespace eebot_CSharp
{
    public class Program
    {
        static string StartPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        static string admin = "114514";
        static List<string> ads = new List<string>();
        static ObservableCollection<KeyValuePair<string, string>> Board = new ObservableCollection<KeyValuePair<string, string>>();
        static ObservableCollection<KeyValuePair<string, string>> CustomQuesion = new ObservableCollection<KeyValuePair<string, string>>();
        static ObservableCollection<KeyValuePair<string, string>> CustomHello = new ObservableCollection<KeyValuePair<string, string>>();
        static Dictionary<string, object> gameobjs = new Dictionary<string, object>();
        static V8ScriptEngine engine = new V8ScriptEngine();



        static bool addGameObj(string id, object o)
        {
            if (!gameobjs.ContainsKey(id))
            {
                gameobjs.Add(id, o);
                return true;
            }
            return false;

        }
        static bool delGameObj(string id)
        {
            if (gameobjs.ContainsKey(id))
            {
                gameobjs.Remove(id);
                return true;
            }
            return false;
        }
        static class Morse
        {
            /// <summary>
            　　　　　　/// 组织mores集合
            　　　　　　///我使用了键值对，Dictionary， HashTable都是可以的
            　　　　　　/// </summary>
            static Dictionary<char, String> morseCode = new Dictionary<char, String>()
            {
                {'a' , ".-"},{'b' , "-..."},{'c' , "-.-."}, //alpha
                {'d' , "-.."},{'e' , "."},{'f' , "..-."},
                {'g' , "--."},{'h' , "...."},{'i' , ".."},
                {'j' , ".---"},{'k' , "-.-"},{'l' , ".-.."},
                {'m' , "--"},{'n' , "-."},{'o' , "---"},
                {'p' , ".--."},{'q' , "--.-"},{'r' , ".-."},
                {'s' , "..."},{'t' , "-"},{'u' , "..-"},
                {'v' , "...-"},{'w' , ".--"},{'x' , "-..-"},
                {'y' , "-.--"},{'z' , "--.."},
                //Numbers 
                {'0' , "-----"},{'1' , ".----"},{'2' , "..----"},{'3' , "...--"},
                {'4' , "....-"},{'5' , "....."},{'6' , "-...."},{'7' , "--..."},
                {'8' , "---.."},{'9' , "----."},
            };
            /// <summary>
            /// 加密
            /// </summary>
            /// <param name="value">命令</param>
            /// <returns>密文</returns>
            public static string WordsTransferToMoresCodes(string value)
            {
                string values = "";
                if (value.Length < 0)
                {
                    return values;
                }

                foreach (char words in value.ToCharArray()) //拆分字符串为字节数组
                {
                    foreach (var dic in morseCode)
                    {
                        if (dic.Key == words)
                        {
                            values += dic.Value + "|"; //"|"为标识码
                        }
                    }
                }

                return values;
            }
            /// <summary>
            /// 解密
            /// </summary>
            /// <param name="code">密文</param>
            /// <returns>命令</returns>
            public static string MoresCodeTransferToWords(string code)
            {
                string keys = "";
                if (code.Length < 0)
                {
                    return keys;
                }

                foreach (string codes in code.Split('|')) //拆分密文
                {
                    foreach (var dic in morseCode) //遍历mores集合
                    {
                        if (dic.Value == codes)
                        {
                            keys += dic.Key;
                        }
                    }
                }

                return keys;
            }
        }
        class Games
        {
            public struct Grass
            {
                public string text { get; set; }
                public bool isComplete { get; set; }
                public string author { get; set; }
            }

        }
        static List<string> folers = new List<string> {
        $"{StartPath}/[record]",//0
        $"{StartPath}/[record]/[board]",//1
        $"{StartPath}/[record]/[quesion]",//2
        $"{StartPath}/[record]/[words]"//3
        };

        static Dictionary<string, OnMessage> hooks = new Dictionary<string, OnMessage>();

        static Dictionary<string, TimeSpan> UserInitTime = new Dictionary<string, TimeSpan>();

        static Dictionary<string, int> userTjxx = new Dictionary<string, int>();

        delegate void OnMessage(ZhChat.data_chat arg);

        static event OnMessage hook;
        public static string YouDao(string q, string from, string to)
        {
            string result = "";
            string url = "http://fanyi.youdao.com/translate_o?smartresult=dict&smartresult=rule/";
            string u = "fanyideskweb";
            string c = "Y2FYu%TNSbMCxc3t2u^XT";
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            long millis = (long)ts.TotalMilliseconds;
            string curtime = Convert.ToString(millis);
            Random rd = new Random();
            string f = curtime + rd.Next(0, 9);
            string signStr = u + q + f + c;
            string sign = GetMd5Str_32(signStr);
            Dictionary<String, String> dic = new Dictionary<String, String>();
            dic.Add("i", q);
            dic.Add("from", from);
            dic.Add("to", to);
            dic.Add("smartresult", "dict");
            dic.Add("client", "fanyideskweb");
            dic.Add("salt", f);
            dic.Add("sign", sign);
            dic.Add("lts", curtime);
            dic.Add("bv", GetMd5Str_32("5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.82 Safari/537.36"));
            dic.Add("doctype", "json");
            dic.Add("version", "2.1");
            dic.Add("keyfrom", "fanyi.web");
            dic.Add("action", "FY_BY_REALTlME");
            //dic.Add("typoResult", "false");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            req.Referer = "http://fanyi.youdao.com/";
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.82 Safari/537.36";
            req.Headers.Add("Cookie", "OUTFOX_SEARCH_USER_ID=-2030520936@111.204.187.35; OUTFOX_SEARCH_USER_ID_NCOO=798307585.9506682; UM_distinctid=17c2157768a25e-087647b7cf38e8-581e311d-1fa400-17c2157768b8ac; P_INFO=15711476666|1632647789|1|youdao_zhiyun2018|00&99|null&null&null#bej&null#10#0|&0||15711476666; JSESSIONID=aaafZvxuue5Qk5_d9fLWx; ___rl__test__cookies=" + curtime);
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dic)
            {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(reader.ReadToEnd());
                if (jo.Value<string>("errorCode").Equals("0"))
                {
                    var tgtarray = jo.SelectToken("translateResult").First().Values<string>("tgt").ToArray();
                    result = string.Join("", tgtarray);
                }
            }
            return result;
        }

        public static string GetMd5Str_32(string encryptString)
        {
            byte[] result = Encoding.UTF8.GetBytes(encryptString);
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            string encryptResult = BitConverter.ToString(output).Replace("-", "");
            return encryptResult;
        }

        public static int getlevel(ZhChat.data_chat arg)
        {
            var level = 0;
            //0 用户
            //1 管理
            //2 机器人所有者

            if (arg.level >= 999999)
            {
                level = 1;
            }

            else if (arg.trip == admin)
            {
                level = 2;
            }

            return level;
        }

        static void AddHook(string id, OnMessage o)
        {
            if (!hooks.ContainsKey(id))
            {
                hooks.Add(id, o);
                hook += o;
            }
        }

        static void BotStart()
        {
            Console.WriteLine($"[Started] {DateTime.Now.ToString()}");

        }
        public class codes
        {
            public class code
            {
                private ZhChat chat { get; set; }
                public ZhChat.UserInfo[] Onlines { get => chat.users.ToArray(); }
                public string[] Nicks { get => chat.nicks.ToArray(); }
                public code(ZhChat zc)
                {
                    chat = zc;
                }
                public void send(object s)
                {
                    chat.SendMsg(s.ToString());
                }

            }
            public static bool OnlineCode(string code, ZhChat zc)
            {
                try
                {


                    engine.Execute(code);


                    return true;
                }
                catch (Exception e)
                {
                    zc.SendMsg(e.Message);
                    return false;
                }
            }
            public static string OnlineCodeEval(string code, ZhChat zc)
            {
                try
                {

                    return engine.Evaluate(code).ToString();

                }
                catch (Exception e)
                {
                    return $"{e.Message}";
                }
            }
        }

        /// <summary>
        /// Get提交
        /// </summary>
        /// <param name="url">提交地址</param>
        /// <returns></returns>
        public static string HttpGet(string url)
        {
            try
            {
                var client = new RestClient(url);
                var request = new RestRequest()
                    ;
                request.Method = Method.Get;
                request.Timeout = 5000;

                request.AddHeader("content-type", "text/html; charset=utf-8");
                request.AddHeader("content-encoding", "gzip");
                RestResponse response = client.Execute(request);
                return response.Content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }



        static void Creat()
        {
            folers.ForEach((s) =>
            {
                if (!Directory.Exists(s))
                {
                    Directory.CreateDirectory(s);
                }
            });
        }

        static string[] GetArgs(ZhChat.data_chat d)
        {
            return d.text.Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToArray();
        }

        static void Main(string[] args)
        {
            Creat();

            bool can_scj = true, scj = false,
                can_zxh = true, zxh = false,
                can_fhl = true, fhl = false,
                can_cyjl = true, cyjl = false,
                isgame = false;
            conf exts = new conf
            {
                adtime = 60 * 60 * 1000,
                custom_str = ""
            };
            bool isdzh = false;
            if (!File.Exists(StartPath + "/[record]/conf.json"))
            {
                File.WriteAllText(StartPath + "/[record]/conf.json", json.dumps(exts));
            }
            else
            {
                exts = json.parse<conf>(File.ReadAllText(StartPath + "/[record]/conf.json"));

            }

            ZhChat.ws_url = "wss://chat.zhangsoft.cf/ws";
            ZhChat zc = new ZhChat(
               new botConfig
               {
                   channel = "chat",
                   password = "40331145",
                   token = "114514",
                   username = "csbot",

               }
               ); ;
            engine.Script.console = new
            {
                log = new Action<string>(zc.SendMsg)
            };
            engine.Script.bot = new codes.code(zc);
            #region 定义

            bool iscolor = false;
            var defind = new
            {
                /*聊天室列表*/
                ws_hc = "wss://hack.chat/chat-ws",//hc
                ws_zhc = "wss://ws.zhangsoft.cf",
                ws_cc = "ws://xq.kzw.ink:6060",//十字街
                ws_xc = "wss://xq.kzw.ink/ws",//xc
                ws_xcc = "wss://ws.crosst.chat:35197",//xcc
                ws_tc = "ws://chat.thz.cool:6060",//tanchat
                ws_tcc = "wss://chat.thz.cool/chat-ws",//
                ws_sc = "wss://sprchatport.run.goorm.io",//drxw
                ws_zc = "wss://zzchat.cf/wss"//zzchzt
                ,
                /*常用频道*/
                ma = "main",
                ts = "test",
                xq = "xq102210",
                ca = "chat",
                cn = "chinese",
                pu = "public",
                lo = "lounge",
                yc = "your-channel",
                ycl = "your-channel1",
                ccc = "公共聊天室",
                bot = "bot",
                CommandPie = "@"
                ,
                psstr = $@"PS:
Mod表示只有管理员能用
True表示游戏可用但未开启
False则是游戏被管理员禁止或在进行
·
你的{zc.username}努力工作中
使用精简模式吗？
在「聊天框」里发送：@{zc.username} 睡
·
不会使用？
试试在下面「聊天框」里发送上面的命令吧

·
{exts.custom_str}",

                customHelp = @".
·
hc指令：
https://tieba.baidu.com/p/6833224084
（by.Lithium, 666）
·
hc wiki：
https://www.zzchat.cf/wiki/
（by.zzChumo, 13）
·
全面入坑hc：
https://littlebees.herokuapp.com/fun/hc/
（by.DarkT）
·
非官方hc wiki：
https://hcwiki.github.io/
（by.4n0n4me）
·
hc同人作品集：
https://note.ms/HCworks111
（by.tctd）
·
XChat用户手册
https://zzchumo.github.io/XChat-doc/
（by.zzChumo）
·
XChat文字攻略
https://paperee.tk/XChat.html
（by.ee）
·
聊天室：
hack.chat（hc）：https://hack.chat/
十字街（cc）：https://crosst.chat/
XChat（xc）：https://xq.kzw.ink/（墙裂推荐）
Tchat（tc）：https://chat.thz.cool/
冬日小屋（sc）：https://chat.spr233.eu.org/
zzChat（zc）：http://zzchat.cf/
星聊：https://im.uerr.cn/（真·新站点）
·
整合版：
XClient：https://zzchumo.github.io/XClient/ch/index.html
zzChat：https://zzchat.thz.cool/
·
这是有关聊天室（大部分是hc）的链接 欢迎投稿"
                ,
                todayboard = $"{StartPath}/[record]/[board]/{DateTime.Now.ToString("yyyy_MM_dd")}.json",
                board = $"{StartPath}/[record]/[board]/",
                quesion = $"{StartPath}/[record]/[quesion]/quesion.json",
                Methods = new Dictionary<string, ChatCommand>(),
                CustomWord = folers[3] + "/word.json"




            };
            #endregion
            if (!File.Exists(StartPath + "/[record]/ads.txt"))
            {
                File.WriteAllText(StartPath + "/[record]/ads.txt", "示例广告.\n");
            }
            else
            {
                ads = File.ReadAllText(StartPath + "/[record]/ads.txt").Split('\n').ToList();
            }
            if (!File.Exists(defind.board))
            {
                File.WriteAllText(defind.todayboard, "");
            }
            else
            {
                try
                {
                    Board = json.parse<ObservableCollection<KeyValuePair<string, string>>>(File.ReadAllText(defind.board));
                }
                catch
                {
                    Board = new ObservableCollection<KeyValuePair<string, string>>();
                }
            }
            if (!File.Exists(defind.quesion))
            {
                File.WriteAllText(defind.quesion, "[]");
            }
            else
            {
                try
                {
                    CustomQuesion = json.parse<ObservableCollection<KeyValuePair<string, string>>>(File.ReadAllText(defind.quesion));
                }
                catch
                {
                    CustomQuesion = new ObservableCollection<KeyValuePair<string, string>>();
                }
            }
            Func<string, bool> isnull = (s) =>
               {
                   if (string.IsNullOrEmpty(s))
                   {
                       zc.SendMsg("文本不能为空哦");
                       return true;
                   }
                   return false;
               };
            if (!File.Exists(defind.CustomWord))
            {
                File.WriteAllText(defind.CustomWord, "[]");
            }
            else
            {
                try
                {
                    CustomHello = json.parse<ObservableCollection<KeyValuePair<string, string>>>(File.ReadAllText(defind.CustomWord));
                }
                catch
                {
                    CustomHello = new ObservableCollection<KeyValuePair<string, string>>();
                }
            }



            Board.CollectionChanged += (p, a) => File.WriteAllText(defind.board, json.dumps(Board));
            CustomQuesion.CollectionChanged += (p, a) => File.WriteAllText(defind.quesion, json.dumps(CustomQuesion));
            CustomHello.CollectionChanged += (p, a) => File.WriteAllText(defind.CustomWord, json.dumps(CustomHello));
            defind.Methods.Add("order", new ChatCommand
            {
                help = "bot功能列表",
                method = ChatCommand.level.Base,
                isshow = false,
                action = (s) =>
                {
                    string Base = "基本:\n",
                            Methods = "功能:\n",
                            Random = "随机：\n",
                            Super = "高级:\n",
                            Conver = "转换:\n",
                            Get = "查询:\n",
                            Parse = "解析:\n",
                            Control = "控制:\n",
                            Game = "游戏:\n";
                    defind.Methods.ToList().ForEach((l) =>
                    {
                        if (l.Value.isshow)
                            switch (l.Value.method)
                            {
                                case ChatCommand.level.Base:

                                    Base += $"{l.Key}：{l.Value.help}\n";
                                    break;
                                case ChatCommand.level.Random:
                                    Random += $"{l.Key}：{l.Value.help}\n"; ;
                                    break;
                                case ChatCommand.level.Super:
                                    Super += $"{l.Key}：{l.Value.help}\n"; ;
                                    break;
                                case ChatCommand.level.Conver:
                                    Conver += $"{l.Key}：{l.Value.help}\n"; ;
                                    break;
                                case ChatCommand.level.Get:
                                    Get += $"{l.Key}：{l.Value.help}\n"; ;
                                    break;
                                case ChatCommand.level.Parse:
                                    Parse += $"{l.Key}：{l.Value.help}\n"; ;
                                    break;
                                case ChatCommand.level.Control:
                                    Control += $"{l.Key}：{l.Value.help}\n"; ;
                                    break;
                                case ChatCommand.level.Game:
                                    Game += $"{l.Key}：{l.Value.help} \n"; ;
                                    break;
                                case ChatCommand.level.Method:
                                    Methods += $"{l.Key}：{l.Value.help} \n"; ;
                                    break;

                            }
                    });
                    string res = $".\r\n{Base}\r\n{Random}\r\n{Methods}\r\n{Super}\r\n{Conver}\r\n{Get}\r\n{Parse}\r\n{Control}\r\n{Game}\r\n";
                    zc.SendMsg(res, s.nick);
                }
            });

            Action<string, string, Action<ZhChat.data_chat>> addbaseCmd = (p, a, c) =>
            {
                defind.Methods.Add(p, new ChatCommand
                {
                    action = c,
                    isshow = true,
                    help = a,
                    method = ChatCommand.level.Base
                });
            };
            Action<string, string, Action<ZhChat.data_chat>> addhideCmd = (p, a, c) =>
            {
                defind.Methods.Add(p, new ChatCommand
                {
                    action = c,
                    isshow = false,
                    help = a,
                    method = ChatCommand.level.Base
                });
            };
            Action<string, string, Action<ZhChat.data_chat>> addSuperCmd = (p, a, c) =>
            {
                defind.Methods.Add(p, new ChatCommand
                {
                    action = c,
                    isshow = true,
                    help = a,
                    method = ChatCommand.level.Super
                });
            };
            Action<string, string, Action<ZhChat.data_chat>> addRandomCmd = (p, a, c) =>
            {
                defind.Methods.Add(p, new ChatCommand
                {
                    action = c,
                    isshow = true,
                    help = a,
                    method = ChatCommand.level.Random
                });
            };
            Action<string, string, Action<ZhChat.data_chat>> addGetCmd = (p, a, c) =>
            {
                defind.Methods.Add(p, new ChatCommand
                {
                    action = c,
                    isshow = true,
                    help = a,
                    method = ChatCommand.level.Get
                });
            };
            Action<string, string, Action<ZhChat.data_chat>> addConverCmd = (p, a, c) =>
            {
                defind.Methods.Add(p, new ChatCommand
                {
                    action = c,
                    help = a,
                    isshow = true,
                    method = ChatCommand.level.Conver
                });
            };
            Action<string, string, Action<ZhChat.data_chat>> addParseCmd = (p, a, c) =>
            {
                defind.Methods.Add(p, new ChatCommand
                {
                    action = c,
                    help = a,
                    isshow = true,
                    method = ChatCommand.level.Parse
                });
            };
            Action<string, string, Action<ZhChat.data_chat>> addControlCmd = (p, a, c) =>
            {
                defind.Methods.Add(p, new ChatCommand
                {
                    action = c,
                    help = a,
                    isshow = true,
                    method = ChatCommand.level.Control
                });
            };
            Action<string, string, Action<ZhChat.data_chat>> addGameCmd = (p, a, c) =>
            {
                defind.Methods.Add(p, new ChatCommand
                {
                    action = c,
                    help = a,
                    isshow = true,
                    method = ChatCommand.level.Game
                });
            };
            Action<string, string, Action<ZhChat.data_chat>> addMethodCmd = (p, a, c) =>
            {
                defind.Methods.Add(p, new ChatCommand
                {
                    action = c,
                    help = a,
                    isshow = true,
                    method = ChatCommand.level.Method
                });
            };

            addhideCmd("sts", "bot数据", (s) =>
            {
                string str = ".\r\n统计信息:\r\n";
                int max = 0;

                var c = userTjxx.ToList();

                var sortResult1 = from pair in c orderby pair.Value descending select pair; //以字典Value值逆序排序
                c = sortResult1.ToList();
                c.ForEach(p => max += p.Value);
                int index = 0;

                c.ForEach((u) =>
                {

                    if (u.Value > 0)
                    {
                        index++;
                        //计算比率
                        double A = u.Value;
                        double B = max;
                        var t = ((A / B) * 100).ToString("0.00");
                        str += $"{index}.{u.Key}: {u.Value}({t}%) \r\n";
                    }
                    /* else
                         str += $"{index}.{u.Key}: {u.Value}({0}%) \r\n";*/
                });
                zc.SendMsg(str, s.nick);
            }); addhideCmd("统计消息", "bot数据", (s) => defind.Methods["sts"].action(s)); addbaseCmd(" 统计｜sts", "bot数据", (s) => { });
            addhideCmd("功能", "bot功能列表", (s) =>
            {
                defind.Methods["order"].action(s);
            }); addbaseCmd(" 功能｜order", "bot功能", (s) => { });
            addhideCmd("help", "聊天室说明", (s) =>
            {
                zc.SendMsg(defind.customHelp, s.nick);
            }); addhideCmd("帮助", "聊天室说明", (s) => defind.Methods["help"].action(s)); addbaseCmd("帮助｜help", "聊天室说明", (s) => { });
            addhideCmd("eebot", "bot的下载地址", (s) =>
            {
                zc.SendMsg(@"·
eebot_ver1（Light’foolishbird）：
https://drive.noire.cc/s/lNdOu1
·
eebot_ver2（new）：
https://drive.noire.cc/s/WppwhQ
·
eebot_3.0（当前版本）：
https://paperee.guru/eebot_3.0.zip
·
密码：纸片君ee
当前版本：ver.3.0
欢迎使用 bot长期更新中", s.nick);
            }); addhideCmd("开源", "bot的下载地址", (s) => defind.Methods["eebot"].action(s)); addbaseCmd("开源｜eebot", "bot的下载地址", (s) => { });
            addhideCmd("web", "bot收集到的网址", (s) =>
            {

            }); addhideCmd("网站", "bot收集到的网址", (s) => defind.Methods["web"].action(s)); addbaseCmd("网站｜web", "bot收集到的网址", (s) => { });
            addhideCmd("bot", "所有bot列表", (s) =>
            {
                zc.SendMsg(@"·
目前hc的yc出现过所有的bot（时间顺序）：
1.fucco，fuck（carborane）
2.bo_od｜bo_od_Dont_kick_ME，刷屏bot，轰炸姬（MuRongPIG）
3.emankciN_s_bot，Collecter（emankicN）
4.junco（ji11）
5.foolishbird｜fOoLishbIrD（Light）
6.Wcy_Bot（__thomas_wcy）
7.eebot｜e1bot｜11bot，eebot_test（ee）
8.zzChumo｜zzbot，zzBot_Lite，Chumo_Chan（zzChumo）
9.SprinkleBot，dx_xb（Sprinkle）
10.notBot｜teapot｜茶壶（DarkT）
11.fakebo_od（realNickname）
12.dotbot（4n0n4me）
13.ZhangBot，ZhangKiller，ZhangProtector，ZhangModHelper｜ZhangHelper，ZhangLockBot，ZhangTouPiaoBot，ZhangHelper｜ZhangHelperS，ZhangProtectPlus，ZhangTurner，ZhangLogger，ZhangChinese（Mr_Zhang）
14.DeteleBot｜wine_Corpse｜wine_Corpsebot｜QiuLingYanBot，FreeDotBotWith4n0n4me（detele）
15.XBot（liguiyu）
16.ModBot（R）
17.Dogebot，KittenBot（Maggie）
18.SuMx_bot｜SuMx_Msg_bot（kilo）
19.MyBot（test）
20.HBot｜ABot｜CBot（Hack4r）
21.AfK_Bot（DPG）
22.do_ob（Henrize）
23.xiao_li（terry）
·
欢迎补充！
也欢迎更多bot编写者写出更多bot", s.nick);
            }); addhideCmd("机器人", "所有bot列表", (s) => defind.Methods["bot"].action(s)); addbaseCmd("机器人｜bot", "所有bot列表", (s) => { });
            addhideCmd("re", "所有自设的问题", (s) =>
            {
                zc.SendMsg($@".
可@{zc.username}的选项:
{string.Join('\n', new Func<string[]>(() =>
                {
                    var res = new List<string>();
                    foreach (var item in CustomQuesion)
                    {
                        res.Add(item.Key);
                    }
                    return res.ToArray();
                })())}
", s.nick);
            }); addhideCmd("自定义", "所有自设的问题", (s) => defind.Methods["re"].action(s)); addbaseCmd("自定义|re", "所有自设的问题", (s) => { });
            addhideCmd("board", "有揭示板的日期", (s) =>
            {
                try
                {
                    var has = Directory.GetFiles(folers[1]);
                    var res = "";
                    foreach (var item in has)
                    {
                        var str = Path.GetFileName(item).Replace(".json", "").Split('_');
                        res += $"{str[0]}.{str[1]}.{str[2]}\r\n";
                    }
                    zc.SendMsg($".\r\n有揭示板的日期：{res}", s.nick);
                }
                catch { }
            }); addhideCmd("揭示板", "有揭示板的日期", (s) => defind.Methods["board"].action(s)); addbaseCmd("揭示板|board", "有揭示板的日期", (s) => { });
            addMethodCmd("CA", "写入今天的揭示板", (s) =>
            {

                File.AppendAllText(defind.todayboard, $"[{DateTime.Now}][{s.trip}][{s.nick}]:\r\n{s.text}\r\n");
                zc.SendMsg($"已成功把{s.nick}的话写入公开揭示板");
            });
            addMethodCmd("B", "查看揭示板", (s) =>
            {
                var arg = GetArgs(s);
                if (arg.Length <= 0)
                {
                    try
                    {
                        zc.SendMsg($".\r\n今天的揭示板:\r\n{File.ReadAllText(defind.todayboard)}", s.nick);
                    }
                    catch
                    {
                        zc.SendMsg("今天好像还没有揭示板呢~");
                    }
                }
                else if (arg.Length > 0)
                {
                    try
                    {
                        var tmp = DateTime.Parse(arg[0]);
                        zc.SendMsg($".\r\n揭示板:\r\n{File.ReadAllText(defind.board + "/" + tmp.ToString("yyyy_MM_dd") + ".json")}", s.nick);
                    }
                    catch
                    {
                        zc.SendMsg("日期转换失败或找不到揭示板");
                    }
                }
            });
            addMethodCmd("Z", "自定义回答，格式:Z [问题] [答案]", (s) =>
            {
                try
                {
                    var arg = GetArgs(s);
                    if (arg.Length >= 2)
                    {
                        if (!CustomQuesion.Any(s => s.Key == arg[0]))
                        {
                            CustomQuesion.Add(KeyValuePair.Create<string, string>(arg[0], arg[1]));
                            zc.SendMsg($"好的 {zc.username} 记住要这么回答了");
                        }
                        else
                        {
                            CustomQuesion.Remove(CustomQuesion.ToList().Find(s => s.Key == arg[0]));
                            CustomQuesion.Add(KeyValuePair.Create<string, string>(arg[0], arg[1]));
                            zc.SendMsg($"已重新设置这个问题的回答");
                        }
                    }
                    else
                    {
                        zc.SendMsg($"格式:Z [问题] [答案]");
                    }
                }
                catch
                {
                }
            });
            addMethodCmd("E", "自定义欢迎语，格式:E [欢迎语]", (s) =>
            {
                if (!CustomHello.Any(j => j.Key == s.nick))
                {
                    CustomHello.Add(KeyValuePair.Create<string, string>(s.nick, s.text));
                    zc.SendMsg("设置成功 将在下次进入聊天室时生效");
                }
                else if (string.IsNullOrEmpty(s.text))
                {
                    CustomHello.Remove(CustomHello.ToList().Find(j => j.Key == s.nick));
                    zc.SendMsg($"已清空对{s.nick}的欢迎语");
                }
                else
                {

                    CustomHello.Remove(CustomHello.ToList().Find(j => j.Key == s.nick));
                    CustomHello.Add(KeyValuePair.Create<string, string>(s.nick, s.text));
                    zc.SendMsg("设置成功 将在下次进入聊天室时生效");
                }

            });
            addConverCmd("TU", "倒着说话:TU [文本]", (s) =>
            {
                if (!isnull(s.text))
                    zc.SendMsg(string.Join("", s.text.Reverse()));
            });
            addConverCmd("MIX", "打乱文本:MIX [文本]", (s) =>
            {
                if (!isnull(s.text))
                    zc.SendMsg(string.Join("", s.text.OrderBy(s => Guid.NewGuid())));
            });
            addConverCmd("T", "字母大写:T [文本]", (s) =>
            {
                if (!isnull(s.text))
                    zc.SendMsg(s.text.ToUpper());
            });
            addConverCmd("MS", "摩斯电码：MS [文本]", (s) =>
            {
                if (!isnull(s.text))
                    zc.SendMsg(Morse.WordsTransferToMoresCodes(s.text));

            });
            addConverCmd("F", "有道翻译:F [文本]", (s) =>
            {
                try
                {
                    if (!isnull(s.text))
                        zc.SendMsg(YouDao(s.text, "auto", "en"));
                }
                catch
                {
                    zc.SendMsg("api出错");
                }
            });
            addGetCmd("IP", "IP详细消息:IP [ip地址]", (s) =>
            {
                if (!isnull(s.text))
                {
                    try
                    {
                        var client = new RestClient("https://api.vvhan.com/api/getIpInfo");
                        var request = new RestRequest()
                            ;
                        request.Method = Method.Get;
                        request.Timeout = 5000;
                        request.AddJsonBody(new { ip = s.text });
                        request.AddHeader("content-type", "text/html; charset=utf-8");
                        request.AddHeader("content-encoding", "gzip");
                        RestResponse response = client.Execute(request);
                        JObject se = JObject.Parse(response.Content);
                        if (se["success"].ToObject<bool>())
                        {
                            JToken a = se["info"];
                            string b = "·\n地址：" + se["ip"] + "\n所属：" + a["lsp"];

                            if (a["prov"].ToString() == "")
                            {
                                zc.SendMsg(b);
                            }
                            else if (a["city"].ToString() == "")
                            {
                                zc.SendMsg(b + "\n·\n国家：" + a["country"] + "\n省份：" + a["prov"]);
                            }
                            else
                            {
                                zc.SendMsg(b + "\n·\n国家：" + a["country"] + "\n省份：" + a["prov"] + "\n城市：" + a["city"]);
                            }
                        }
                        else zc.SendMsg("请求失败 请检查IP地址");
                    }
                    catch
                    {
                        zc.SendMsg("api请求出错");
                    }
                }
            });
            addGetCmd("ICP", "查询icp备案:ICP [域名]", (s) =>
            {
                if (!isnull(s.text))
                {
                    try
                    {
                        JObject se = JObject.Parse(HttpGet($"https://api.oick.cn/icp/api.php?url={s.text}"));
                        if (se["code"].ToObject<int>() == 200)
                        {
                            if (se["name"].ToString() == "-")
                            {
                                zc.SendMsg("API出错或未有此域名ICP备案记录");
                                return;
                            }
                            string a = "·\n" + se["site_name"] + "（" + se["site_index"] + "）",

                                  b = "\n·\n单位：" + se["name"] + "\n类型：" + se["nature"],

                                  c = "\n·\n备案信息：" + se["icp"] + "\n备案时间：" + se["site_time"];
                            zc.SendMsg(a + b + c);


                        }
                        else if (se["code"].ToObject<int>() == 201)
                            zc.SendMsg("API出错或未有此域名ICP备案记录");
                        else zc.SendMsg("获取失败 请检查域名");
                    }
                    catch
                    {
                        zc.SendMsg("api出错");
                    }
                }
            });
            addGetCmd("QQ", "QQ号呢称:QQ [QQ号]", (s) =>
            {
                if (!isnull(s.text))
                {
                    try
                    {
                        var client = new RestClient("http://api.btstu.cn/qqxt/api.php");
                        var request = new RestRequest()
                            ;
                        request.Method = Method.Get;
                        request.Timeout = 5000;
                        request.AddJsonBody(new { qq = s.text });
                        request.AddHeader("content-type", "text/html; charset=utf-8");
                        request.AddHeader("content-encoding", "gzip");
                        RestResponse response = client.Execute(request);
                        JObject se = JObject.Parse(response.Content);
                        if (se["code"].ToObject<int>() == 1 && se["name"].ToString() != "")
                            zc.SendMsg("·\n" + se["name"] + "\n" + s.text);
                        else
                            zc.SendMsg("查询失败 请检查QQ号");


                    }
                    catch
                    {
                        zc.SendMsg("api出错");
                    }
                }
            });
            addGetCmd("CITY", "城市消息:CITY [城市]", (s) =>
            {
                if (!isnull(s.text))
                {
                    try
                    {
                        var client = new RestClient("https://api.muxiaoguo.cn/api/tianqi");
                        var request = new RestRequest()
                            ;
                        request.Method = Method.Get;
                        request.Timeout = 5000;
                        request.AddJsonBody(new { city = s.text, type = 1, api_key = "9410e360a940cb5e" });
                        request.AddHeader("content-type", "text/html; charset=utf-8");
                        request.AddHeader("content-encoding", "gzip");
                        RestResponse response = client.Execute(request);
                        JObject se = JObject.Parse(response.Content);
                        JToken a = se["data"];
                        if (se["code"].ToObject<int>() == 200)
                        {
                            string b = "·\n城市：" + s.text + "（" + a["weather"] + "）",
                                                c = "\n温度：" + a["temp"] + "°C\n湿度：" + a["SD"],
                                                d = "\n·\n风向：" + a["WD"] + "\n风级：" + a["WS"] + "\n风速：" + a["wse"],
                                                e = "\n·\n更新时间：" + a["time"];
                            zc.SendMsg(b + c + d + e);
                        }
                        else
                            zc.SendMsg("查询失败 请检查城市");



                    }
                    catch
                    {
                        zc.SendMsg("api出错");
                    }
                }
            });
            addGetCmd("DU", "百度百科:DU [文本]", (s) =>
            {
                if (!isnull(s.text))
                {
                    try
                    {
                        var client = new RestClient("https://api.muxiaoguo.cn/api/Baike");
                        var request = new RestRequest()
                            ;
                        request.Method = Method.Get;
                        request.Timeout = 5000;
                        request.AddJsonBody(new { type = "Baidu", word = s.text, api_key = "cb28182953ac4557" });
                        request.AddHeader("content-type", "text/html; charset=utf-8");
                        request.AddHeader("content-encoding", "gzip");
                        RestResponse response = client.Execute(request);
                        JObject se = JObject.Parse(response.Content);
                        JToken a = se["data"];

                        if (se["code"].ToObject<int>() == 200)
                        {
                            string b = "·\n查询：" + s.text + "\n·\n百度：" + a["content"];
                            zc.SendMsg(b.Replace("。", "。\n").
                            Replace("\"", "").
                            Replace("(", "（").
                            Replace(")", "）").
                            Replace("“", "「").
                            Replace("”", "」"));
                        }
                        else zc.SendMsg("查询失败 无此百科");
                    }
                    catch
                    {
                        zc.SendMsg("api出错");
                    }
                }
            });
            addGetCmd("PT", "手机号详细消息:PT [手机号]", (s) =>
            {
                if (!isnull(s.text))
                {
                    try
                    {
                        var client = new RestClient(" http://api.botwl.cn/api/sjhgsd");
                        var request = new RestRequest()
                            ;
                        request.Method = Method.Get;
                        request.Timeout = 5000;
                        request.AddJsonBody(new { sjh = s.text });
                        request.AddHeader("content-type", "text/html; charset=utf-8");
                        request.AddHeader("content-encoding", "gzip");
                        RestResponse response = client.Execute(request);
                        JObject se = JObject.Parse(response.Content);

                        if (se["code"].ToObject<int>() == 1)
                        {
                            zc.SendMsg("·\n手机号：" + se["手机号"] + "\n·\n归属地：" + se["归属地"] + "\n卡类型：" + se["卡类型"]);
                        }
                        else zc.SendMsg("获取失败 请检查手机号");

                    }
                    catch
                    {
                        zc.SendMsg("api出错");
                    }
                }
            });
            addGetCmd("L", "垃圾分类:L [分类]", (s) =>
            {
                if (!isnull(s.text))
                {
                    if (new string[] { zc.username, s.nick }.Contains(s.text))
                    {
                        zc.SendMsg($"咳咳 {s.nick}礼貌吗");
                        return;
                    }
                    if (zc.users.Any(p => p.nick == s.text))
                    {
                        zc.SendMsg($"@{s.text} 觉得自己是垃圾吗 （");
                        return;
                    }
                    try
                    {
                        var client = new RestClient($"https://api.vvhan.com/api/la.ji?lj={s.text}");
                        var request = new RestRequest()
                            ;
                        request.Method = Method.Get;
                        request.Timeout = 5000;
                        request.AddJsonBody(new { lj = s.text });
                        request.AddHeader("content-type", "text/html; charset=utf-8");
                        request.AddHeader("content-encoding", "gzip");
                        RestResponse response = client.Execute(request);
                        JObject se = JObject.Parse(response.Content);
                        if (se["sort"].ToString() != "俺也不知道是什么垃圾~")
                            zc.SendMsg(se["sort"].ToString());
                        else
                            zc.SendMsg("所以这是什么垃圾");
                    }
                    catch
                    {
                        zc.SendMsg("api出错");
                    }
                }
            });
            addParseCmd("BI", "解析b站视频:BI [av/bv]", (s) =>
            {
                string id_str = "id=";

                if (s.text.Contains("av"))
                {
                    s.text = s.text.Replace("av", "");
                    id_str = "id=";
                }
                else if (s.text.Contains("BV"))
                {

                    id_str = "bid=";
                }
                else
                {
                    zc.SendMsg("不合法的av/bv号");
                    return;
                }
                if (!isnull(s.text))
                {
                    try
                    {
                        var client = new RestClient($"https://api.kaaass.net/biliapi/video/info?{id_str}{s.text}");
                        var request = new RestRequest()
                            ;
                        request.Method = Method.Get;
                        request.Timeout = 5000;

                        request.AddHeader("content-type", "text/html; charset=utf-8");
                        request.AddHeader("content-encoding", "gzip");
                        RestResponse response = client.Execute(request);
                        JObject se = JObject.Parse(response.Content);
                        if (se["status"].ToString() == "OK")
                        {
                            JToken data = se["data"];
                            string info = $@"
# UP主:{data["author"]}
标题:{data["title"]}
简介:{data["description"]}
播放量:{data["play"]}
评论数:{data["review"]}
收藏量:{data["favorites"]}
地址:https://www.bilibili.com/video/{s.text}
";
                            zc.SendMsg(info);
                        }
                    }
                    catch
                    {
                        zc.SendMsg("api出错");
                    }
                }
            });
            addRandomCmd("r", "随机", (s) =>
            {
                if (zc.users.Any(k => k.nick == s.text))
                {
                    zc.SendMsg($"@{s.text} 被r了");
                    return;
                }
                var arg = GetArgs(s);
                if (arg.Length >= 1)
                {
                    try
                    {
                        int.TryParse(arg[0], out int re);
                        if (re <= 0)
                        {
                            zc.SendMsg(re.ToString().Replace("-", ""));

                        }
                        else if (arg.Length >= 2)
                        {
                            int.TryParse(arg[1], out int re2);
                            zc.SendMsg(new Random().Next(re, re2).ToString());

                        }
                        else zc.SendMsg(new Random().Next(0, re).ToString());

                    }
                    catch
                    {
                        zc.SendMsg(new Random().Next(0, 1000).ToString());
                    }
                    return;

                }
                zc.SendMsg(new Random().Next(0, 1000).ToString());


            });
            addGameCmd("生草机", $"({can_scj})", (s) =>
            {
                if (isgame)
                {
                    zc.SendMsg("已经有游戏在进行了");
                    return;
                }
                addGameObj("scj_list", new Dictionary<string, Games.Grass>());
                scj = true;
                isgame = true;
                zc.SendMsg("请用1报数");
            });
            addGameCmd("真心话", $"({can_zxh})", (s) => { });
            addGameCmd("飞花令", $"({can_fhl})", (s) => { });
            addGameCmd("词语接龙", $"({can_cyjl})", (s) => { });
            addControlCmd($"@{zc.username}", $"此命令有二级帮助，请使用:@{zc.username} help", (s) =>
            {
                var arg = GetArgs(s);
                if (arg.Length >= 1)
                {
                    if (arg[0] == "help")
                    {
                        zc.SendMsg(@$".
@{zc.username} 文本：与bot聊天
@{zc.username} 重启：重启bot（Mod）
@{zc.username} 退出：让bot退出（Mod）
@{zc.username} 打招呼开｜关：是否自动打招呼
@{zc.username} 打广告开｜关：是否定时发友链

@{zc.username} 睡｜醒：是否让bot休眠
@{zc.username} （不）变成彩虹：是否让bot有多种颜色
@{zc.username} 昵称（新昵称）：改bot昵称
@{zc.username} 上次看到（nick）：某人最后出现的时间
@{zc.username} 在线时长（nick）：某人在线的时间
（只允许查询在线列表中的用户）", s.nick);
                    }
                    else if (arg[0] == "重启")
                    {
                        if (getlevel(s) > 0)
                        {
                            zc.wss.Close();
                            zc.wss.Connect();
                        }
                    }
                    else if (arg[0] == "退出")
                    {
                        if (getlevel(s) > 0)
                        {
                            Environment.Exit(114514);
                        }
                    }
                    else if (arg[0] is "打招呼" or "打招呼开" or "打招呼关")
                    {
                        try
                        {
                            if (getlevel(s) > 0)
                            {
                                isdzh = arg[0].Contains('开');
                                zc.SendMsg($"打招呼开关:{isdzh}");
                            }
                        }
                        catch (Exception e)
                        {
                            zc.SendMsg($"异常:{e.Message}");
                        }
                    }
                    else if (arg[0] is "睡" or "醒")
                    {

                        zc.SendMsg($"/me 已经{arg[0]}了");
                    }
                    else if (arg[0] is "变成彩虹" or "不变成彩虹")
                    {
                        iscolor = (arg[0].Contains('不'));
                        zc.SendMsg($"彩虹开关:{iscolor}");

                    }
                    else if (arg[0] == "呢称")
                    {
                        if (getlevel(s) > 0)
                        {
                            try
                            {
                                zc.SendMsg($"/nick {arg[1]}");
                            }
                            catch { };
                        }
                    }
                    else if (arg[0] == "上次看到")
                    {
                        try
                        {
                            if (UserInitTime.Any(s => s.Key == arg[1]))
                            {
                                var tmp = UserInitTime.ToList().Find(p => p.Key == arg[1]).Value;
                                zc.SendMsg($"上次看到{arg[1]}在: {tmp.Hours}时{tmp.Minutes}分{tmp.Seconds}秒");
                            }
                            else
                            {
                                zc.SendMsg("找不到");
                            }
                        }
                        catch { zc.SendMsg($"缺少参数：[呢称]"); }
                    }
                    else if (arg[0] == "在线时长")
                    {
                        try
                        {
                            if (args.Length >= 2)
                            {
                                if (UserInitTime.Any(s => s.Key == arg[1]))
                                {
                                    var tmp = new TimeSpan(DateTime.Now.Ticks) - UserInitTime.ToList().Find(p => p.Key == arg[1]).Value;
                                    zc.SendMsg($"{arg[1]}在线时长: {tmp.Hours}时{tmp.Minutes}分{tmp.Seconds}秒");
                                }
                                else
                                {
                                    zc.SendMsg("找不到");
                                }
                            }
                            else zc.SendMsg("缺少参数:[呢称]");
                        }
                        catch
                        {
                        }
                    }
                    else if (CustomQuesion.Any(s => s.Key == arg[0]))
                    {
                        zc.SendMsg(CustomQuesion.ToList().Find(o => o.Key == arg[0]).Value);
                    }
                }

            });
            addSuperCmd("js", "在线运行js:js [任意js代码](无返回)", (s) =>
            {
                if (codes.OnlineCode(s.text, zc))
                {
                    zc.SendMsg("代码执行成功", s.nick);
                }
                else zc.SendMsg("代码执行失败", s.nick);
            });
            addSuperCmd("eval", "在线运行js:eval [任意js代码](有返回)", (s) =>
            {
                zc.SendMsg(codes.OnlineCodeEval(s.text, zc));
            });
            zc.IsDebug = false;
            AddHook("tjxx", (o) =>
            {
                if (userTjxx.ContainsKey(o.nick))
                {
                    userTjxx[o.nick]++;
                }
                else
                {
                    userTjxx.Add(o.nick, 1);
                }
            });
            AddHook("color", (o) =>
            {
                if (iscolor)
                {
                    var tmp = Color.FromArgb(random.randrange(0, 255), random.randrange(0, 255), random.randrange(0, 255));
                    zc.SendMsg($"/color #{tmp.R:X2}{tmp.G:X2}{tmp.B:X2}");
                }
            });
            AddHook("game", (o) =>
            {
                if (isgame)
                {
                    if (scj)
                    {
                        Games.Grass completeing = new Games.Grass();
                        if (o.text == "1")
                        {
                            var playing = gameobjs.ContainsKey("scj_play");
                            if (playing)
                            {
                                return;
                            }
                            var list = (Dictionary<string, Games.Grass>)gameobjs["scj_list"];
                            if (list.ContainsKey(o.nick))
                            {
                                zc.SendMsg($"{o.nick}已经报过数了 现在有{list.Count}人");
                            }
                            else
                            {
                                list.Add(o.nick, new Games.Grass { author = o.nick, text = "" });
                                zc.SendMsg($"{o.nick}成功加入 现在有{list.Count}人");
                            }

                        }
                        if (o.text == "开始游戏")
                        {
                            var list = (Dictionary<string, Games.Grass>)gameobjs["scj_list"];
                            var playing = gameobjs.ContainsKey("scj_play");
                            if (list.Count() >= 3 && !playing)
                            {
                                addGameObj("scj_play", true);
                                zc.SendMsg("游戏开始");
                                list.Keys.ToList().ForEach(kun =>
                                {
                                    addGameObj($"scj_{kun}_sc_ok", false);//添加一个一个变量判断生草是否完毕
                                });
                                zc.SendMsg($"{new Func<string>(() => { string s = ""; list.Keys.ToList().ForEach(p => s += "@" + p + " "); return s; })()}\n注意查看私聊 由于是按顺序来 部分人的私聊通知还在等待中-");
                                new Thread(() =>
                                {

                                    for (int p = 0; p < list.Count; p++)
                                    {

                                        var item = new { Key = list.Keys.ToList()[p], Value = list[list.Keys.ToList()[p]] };
                                        if (p == 0)
                                            zc.SendMsg($"/w {item.Key} {item.Key}是第一个 使用/w {zc.username} 输入一个人物名");
                                        else zc.SendMsg($"/w {item.Key} 轮到你开始了 使用/w {zc.username} 输入一个地点/时间点");
                                        completeing = item.Value;


                                        string nick = o.nick;
                                        for (int i = 0; i < 59; i++)
                                        {

                                            Thread.Sleep(1000);
                                            if ((bool)gameobjs[$"scj_{item.Key}_sc_ok"])
                                            {
                                                var list2 = (Dictionary<string, Games.Grass>)gameobjs["scj_list"];
                                                if (!list2[item.Key].isComplete)
                                                {
                                                    //如果超时了自动填写
                                                    var tmp = list2[item.Key];

                                                    tmp.author = $"替{item.Key}写的{zc.username}";
                                                    tmp.text = cxk.RandomSinge();
                                                    tmp.isComplete = true;
                                                    list2[nick] = tmp;
                                                    gameobjs["scj_list"] = list;
                                                    zc.SendMsg($"@{item.Key} 你超时了,下一位");

                                                }
                                                break;
                                            }

                                        }


                                    }

                                    string res = @"·
详细信息：
";
                                    string jz = @"";
                                    list.Values.ToList().ForEach(S => { jz += S.text; res += $"{S.text}({S.author})\r\n"; });
                                    zc.SendMsg($"[{DateTime.Now.ToString()}]\r\n" + jz);
                                    res += $@"
·
本次参与人数：{list.Count}";
                                    zc.SendMsg(res);
                                    scj = false;
                                    isgame = false;
                                    gameobjs.Clear();
                                }).Start();
                            }
                            else if (playing)
                            {
                                zc.SendMsg("游戏已经开始了,等下一轮罢");
                            }
                            else
                            {
                                zc.SendMsg("人数不足 至少要有3个人");
                            }
                        }
                        if (o.isWhisper)
                        {
                            var list = (Dictionary<string, Games.Grass>)gameobjs["scj_list"];
                            if (list.ContainsKey(o.nick))
                            {
                                var tmp = list[o.nick];
                                tmp.text = o.text;
                                tmp.isComplete = true;
                                list[o.nick] = tmp;
                                gameobjs["scj_list"] = list;
                                gameobjs["scj_" + o.nick + "_sc_ok"] = true;
                            }
                        }
                        if (o.text == "信息")
                        {
                            var list = (Dictionary<string, Games.Grass>)gameobjs["scj_list"];
                            if (list.Count <= 2)
                            {
                                zc.SendMsg("生草机报数中...");
                            }
                            else
                            {
                                string res = "已完成:";
                                list.ToList().ForEach(info =>
                                {
                                    if (info.Value.isComplete)
                                    {
                                        res += $"{info.Value.author} ";
                                    }
                                });
                                zc.SendMsg(res);
                            }
                        }
                        if (o.text == "结束游戏")
                        {
                            isgame = false;
                            scj = false;
                            gameobjs.Clear();
                            zc.SendMsg("游戏已结束");
                        }
                    }
                }

            });
            /*
             if (!isnull(s.text))
                {
                    try
                    {
                        var client = new RestClient("");
                        var request = new RestRequest()
                            ;
                        request.Method = Method.Get;
                        request.Timeout = 5000;
                        request.AddJsonBody(new { ip = s.text });
                        request.AddHeader("content-type", "text/html; charset=utf-8");
                        request.AddHeader("content-encoding", "gzip");
                        RestResponse response = client.Execute(request);
                        JObject se = JObject.Parse(response.Content);
                    }
                    catch
                    {
                        zc.SendMsg("api出错");
                    }
                }
             */
            zc.onlineSet = (j) =>
            j.ForEach((u) =>
            {
                // Console.WriteLine(j.Count);
                if (!userTjxx.ContainsKey(u.nick))
                {
                    userTjxx.Add(u.nick, 0);
                }

            });
            zc.UserAdd = (u) =>
            {
                if (isdzh)
                {
                    zc.SendMsg($"hey yo {u.nick}");
                }
                if (!userTjxx.ContainsKey(u.nick))
                {
                    userTjxx.Add(u.nick, 0);
                }
                if (!UserInitTime.ContainsKey(u.nick))
                {
                    UserInitTime.Add(u.nick, new TimeSpan(DateTime.Now.Ticks));
                }

                if (CustomHello.Any(j => j.Key == u.nick))
                {
                    try
                    {
                        zc.SendMsg(CustomHello.ToList().Find(j => j.Key == u.nick).Value);
                    }
                    catch { }
                }
            };
            zc.OnMessage = (a) =>
            {
                var arg = a.text.Split(' ');
                if (arg.Length >= 1)
                    foreach (var item in defind.Methods)
                    {
                        if (defind.CommandPie + item.Key == arg[0])
                        {

                            a.text = string.Join(" ", arg.Skip(1).Take(arg.Length - 1).Where(s => !string.IsNullOrEmpty(s)));

                            defind.Methods[item.Key].action(a);
                            break;
                        }
                        else if (arg[0] == "@" + zc.username)
                        {
                            a.text = string.Join(" ", arg.Skip(1).Take(arg.Length - 1).Where(s => !string.IsNullOrEmpty(s)));
                            defind.Methods["@" + zc.username].action(a);
                            break;
                        }
                    }

                try
                {
                    hook(a);
                }
                catch (Exception e)
                {
                    zc.SendMsg($"程序异常:{e.Message}");
                }
            };
            zc.Init();
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(60 * 60 * 1000);
                    if (ads.Count > 0)
                    {
                        zc.SendMsg(ads[new Random().Next(0, ads.Count - 1)]);
                    }


                }
            }).Start();
            BotStart();

        }

        static void RemoveHook(string id)
        {
            if (hooks.ContainsKey(id))
            {
                hook -= hooks[id];
                hooks.Remove(id);
            }
        }

        static class json
        {
            public static string dumps(object o)
            {
                return json_.SerializeObject(o);
            }
            public static JObject Json(string s)
            {
                return JObject.Parse(s);
            }

            public static T parse<T>(string s)
            {
                return json_.DeserializeObject<T>(s);
            }
        }

        static class random
        {
            static int seed = 114514;
            public static T choice<T>(T[] t)
            {
                return t.OrderBy(u => Guid.NewGuid()).First();
            }
            public static double Random()
            {
                return new Random(seed).NextDouble();
            }

            public static int randrange(int stop, int start = 0, int step = 1)
            {
                return new Random(seed).Next(stop, start) + step;
            }
            public static void Seed(int newseed)
            {
                seed = newseed;
            }
            public static T[] shuffle<T>(T[] t)
            {
                return t.OrderBy(s => Guid.NewGuid()).ToArray<T>();
            }
        }

        class ChatCommand
        {
            public enum level
            {
                Base,
                Method,
                Random,
                Super,
                Conver,
                Get,
                Parse,
                Control,
                Game
            }

            public Action<ZhChat.data_chat> action { get; set; }
            public string customCmd { get; set; }
            public string help { get; set; }
            public bool isshow { get; set; }
            public level method { get; set; }
            public override string ToString()
            {
                return help;
            }
        }
        class conf
        {
            public int adtime { get; set; }
            public string custom_str { get; set; }
        }
    }
}
