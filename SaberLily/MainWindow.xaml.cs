using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace SaberLily
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string vfwebqq, ptwebqq, psessionid, uin, hash;
        public static FriendInfo SelfInfo = new FriendInfo();
        public static Dictionary<string, FriendInfo> FriendList = new Dictionary<string, FriendInfo>();
        public static string[] FriendCategories = new string[100];
        public static Dictionary<string, string> RealQQNum = new Dictionary<string, string>();
        public static Dictionary<string, GroupInfo> GroupList = new Dictionary<string, GroupInfo>();
        public static Dictionary<string, DiscussInfo> DiscussList = new Dictionary<string, DiscussInfo>();
        private static bool Running = true;
        private static int Count103 = 0;

        public MainWindow()
        {
            InitializeComponent();
            Login_Process(Date.webqqUrl);
            
        }
        //扫描二维码后处理函数
        private void Login_Process(string url)
        {
            Login_GetPtwebqq(url);
            Login_GetVfwebqq();
            Login_GetPsessionid();
            Info_FriendList();//获取好友列表
            Info_GroupList();//获取群组列表
            Info_DisscussList();//获取讨论组列表
            Info_SelfInfo();//获取账号信息
            Login_GetOnlineAndRecent_FAKE();
            Task.Run(() => Message_Request());
            Console.WriteLine("当前登录账号为："+Date.QQNum);

        }
        //登录第三步：获取ptwebqq值
        private static void Login_GetPtwebqq(string url)
        {
            string dat = HttpClient.Get(url, "http://s.web2.qq.com/proxy.html?v=20130916001&callback=1&id=1");
            Uri uri = new Uri("http://web2.qq.com/");
            ptwebqq = HttpClient.cookies.GetCookies(uri)["ptwebqq"].Value;
        }
        //登录第四步：获取vfwebqq的值
        private static void Login_GetVfwebqq()
        {
            string url = "http://s.web2.qq.com/api/getvfwebqq?ptwebqq=#{ptwebqq}&clientid=53999199&psessionid=&t=#{t}".Replace("#{ptwebqq}", ptwebqq).Replace("#{t}", AID_TimeStamp());
            string dat = HttpClient.Get(url, "http://s.web2.qq.com/proxy.html?v=20130916001&callback=1&id=1");
            vfwebqq = dat.Split('\"')[7];
        }
        // 登录第五步：获取pessionid
        private static void Login_GetPsessionid()
        {
            string url = "http://d1.web2.qq.com/channel/login2";
            string url1 = "{\"ptwebqq\":\"#{ptwebqq}\",\"clientid\":53999199,\"psessionid\":\"\",\"status\":\"online\"}".Replace("#{ptwebqq}", ptwebqq);
            url1 = "r=" + HttpUtility.UrlEncode(url1);
            string dat = HttpClient.Post(url, url1, "http://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2");
            psessionid = dat.Replace(":", ",").Replace("{", "").Replace("}", "").Replace("\"", "").Split(',')[10];
            Date.QQNum = uin = dat.Replace(":", ",").Replace("{", "").Replace("}", "").Replace("\"", "").Split(',')[14];
            hash = AID_Hash(Date.QQNum, ptwebqq);
        }
        //登录第六步：获取在线成员、近期联系人（仅提交请求，未处理）
        private static void Login_GetOnlineAndRecent_FAKE()
        {
            string url = "http://d1.web2.qq.com/channel/get_online_buddies2?vfwebqq=#{vfwebqq}&clientid=53999199&psessionid=#{psessionid}&t=#{t}".Replace("#{vfwebqq}", vfwebqq).Replace("#{psessionid}", psessionid).Replace("#{t}", AID_TimeStamp());
            HttpClient.Get(url, "http://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2");

            url = "http://d1.web2.qq.com/channel/get_recent_list2";
            string url1 = "{\"vfwebqq\":\"#{vfwebqq}\",\"clientid\":53999199,\"psessionid\":\"#{psessionid}\"}".Replace("#{vfwebqq}", vfwebqq).Replace("#{psessionid}", psessionid);
            string dat = HttpClient.Post(url, "r=" + HttpUtility.UrlEncode(url1), "http://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2");
        }
        //发送poll包，请求消息
        private void Message_Request()
        {
            try
            {
                string url = "http://d1.web2.qq.com/channel/poll2";
                string HeartPackdata = "{\"ptwebqq\":\"#{ptwebqq}\",\"clientid\":53999199,\"psessionid\":\"#{psessionid}\",\"key\":\"\"}";
                HeartPackdata = HeartPackdata.Replace("#{ptwebqq}", ptwebqq).Replace("#{psessionid}", psessionid);
                HeartPackdata = "r=" + HttpUtility.UrlEncode(HeartPackdata);
                HttpClient.Post_Async_Action action = Message_Get;
                HttpClient.Post_Async(url, HeartPackdata, action);
            }
            catch (Exception) { Message_Request(); }
        }
        //接收到消息的回调函数
        private void Message_Get(string data)
        {
            Task.Run(() => Message_Request());
            if (Running)
                Task.Run(() => Message_Process(data));
        }
        //处理收到的消息
        private void Message_Process(string data)
        {
            textBoxLog.Text = data;
            JsonPollMessage poll = (JsonPollMessage)JsonConvert.DeserializeObject(data, typeof(JsonPollMessage));
            if (poll.retcode != 0)
                Message_Process_Error(poll);
            else if (poll.result != null && poll.result.Count > 0)
                for (int i = 0; i < poll.result.Count; i++)
                {
                    switch (poll.result[i].poll_type)
                    {
                        case "kick_message":
                            Running = false;
                            MessageBox.Show(poll.result[i].value.reason);
                            break;
                        case "message":
                            Message_Process_Message(poll.result[i].value);
                            break;
                        case "group_message":
                            Message_Process_GroupMessage(poll.result[i].value);
                            break;
                        case "discu_message":
                            Message_Process_DisscussMessage(poll.result[i].value);
                            break;
                        default:
                            listBoxLog.Items.Add(poll.result[i].poll_type);
                            break;
                    }
                }
        }

        /// <summary>
        /// 根据QQ号和ptwebqq值获取hash值，用于获取好友列表和群列表
        /// </summary>
        /// <param name="QQNum"></param>
        /// <param name="ptwebqq"></param>
        /// <returns></returns>
        private static string AID_Hash(string QQNum, string ptwebqq)
        {
            int[] N = new int[4];
            long QQNum_Long = long.Parse(QQNum);
            for (int T = 0; T < ptwebqq.Length; T++)
            {
                N[T % 4] ^= ptwebqq.ToCharArray()[T];
            }
            string[] U = { "EC", "OK" };
            long[] V = new long[4];
            V[0] = QQNum_Long >> 24 & 255 ^ U[0].ToCharArray()[0];
            V[1] = QQNum_Long >> 16 & 255 ^ U[0].ToCharArray()[1];
            V[2] = QQNum_Long >> 8 & 255 ^ U[1].ToCharArray()[0];
            V[3] = QQNum_Long & 255 ^ U[1].ToCharArray()[1];

            long[] U1 = new long[8];

            for (int T = 0; T < 8; T++)
            {
                U1[T] = T % 2 == 0 ? N[T >> 1] : V[T >> 1];
            }

            string[] N1 = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
            string V1 = "";

            for (int i = 0; i < U1.Length; i++)
            {
                V1 += N1[(int)((U1[i] >> 4) & 15)];
                V1 += N1[(int)(U1[i] & 15)];
            }
            return V1;
        }
        //获取登录账号信息
        internal static void Info_SelfInfo()
        {
            string url = "http://s.web2.qq.com/api/get_self_info2?t=#{t}".Replace("#{t}", AID_TimeStamp());
            string dat = HttpClient.Get(url, "http://s.web2.qq.com/proxy.html?v=20130916001&callback=1&id=1");
            JsonFriendInfModel inf = (JsonFriendInfModel)JsonConvert.DeserializeObject(dat, typeof(JsonFriendInfModel));

            SelfInfo.face = inf.result.face;
            SelfInfo.occupation = inf.result.occupation;
            SelfInfo.phone = inf.result.phone;
            SelfInfo.college = inf.result.college;
            SelfInfo.blood = inf.result.blood;
            SelfInfo.homepage = inf.result.homepage;
            SelfInfo.vip_info = inf.result.vip_info;
            SelfInfo.country = inf.result.country;
            SelfInfo.city = inf.result.city;
            SelfInfo.personal = inf.result.personal;
            SelfInfo.nick = inf.result.nick;
            SelfInfo.shengxiao = inf.result.shengxiao;
            SelfInfo.email = inf.result.email;
            SelfInfo.province = inf.result.province;
            SelfInfo.gender = inf.result.gender;
            if (inf.result.birthday.year != 0 && inf.result.birthday.month != 0 && inf.result.birthday.day != 0)
                SelfInfo.birthday = new DateTime(inf.result.birthday.year, inf.result.birthday.month, inf.result.birthday.day);
        }
        //获取时间戳
        public static string AID_TimeStamp(int type = 1)
        {
            if (type == 1)
            {
                DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                return ((DateTime.UtcNow.Ticks - dt1970.Ticks) / 10000).ToString();
            }
            else if (type == 2)
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                return Convert.ToInt64(ts.TotalSeconds).ToString();
            }
            else return "ERROR";
        }
        //获取好友列表
        internal void Info_FriendList()
        {
            string url = "http://s.web2.qq.com/api/get_user_friends2";
            string sendData = string.Format("r={{\"vfwebqq\":\"{0}\",\"hash\":\"{1}\"}}", vfwebqq, hash);
            string dat = HttpClient.Post(url, sendData, "http://s.web2.qq.com/proxy.html?v=20130916001&callback=1&id=1");

            JsonFriendModel friend = (JsonFriendModel)JsonConvert.DeserializeObject(dat, typeof(JsonFriendModel));
            for (int i = 0; i < friend.result.info.Count; i++)
            {
                if (!FriendList.ContainsKey(friend.result.info[i].uin))
                    FriendList.Add(friend.result.info[i].uin, new FriendInfo());
                FriendList[friend.result.info[i].uin].face = friend.result.info[i].face;
                FriendList[friend.result.info[i].uin].nick = friend.result.info[i].nick;
                Info_FriendInfo(friend.result.info[i].uin);
            }
            for (int i = 0; i < friend.result.friends.Count; i++)
            {
                if (!FriendList.ContainsKey(friend.result.friends[i].uin))
                    FriendList.Add(friend.result.friends[i].uin, new FriendInfo());
                FriendList[friend.result.friends[i].uin].categories = friend.result.friends[i].categories;
            }
            for (int i = 0; i < friend.result.categories.Count; i++)
            {
                FriendCategories[friend.result.categories[i].index] = friend.result.categories[i].name;
            }
            ReNewListBoxFriend();
        }
        //获取好友详细信息
        internal static void Info_FriendInfo(string uin)
        {
            string url = "http://s.web2.qq.com/api/get_friend_info2?tuin=#{uin}&vfwebqq=#{vfwebqq}&clientid=53999199&psessionid=#{psessionid}&t=#{t}".Replace("#{t}", AID_TimeStamp());
            url = url.Replace("#{uin}", uin).Replace("#{vfwebqq}", vfwebqq).Replace("#{psessionid}", psessionid);
            string dat = HttpClient.Get(url, "http://s.web2.qq.com/proxy.html?v=20130916001&callback=1&id=1");
            JsonFriendInfModel inf = (JsonFriendInfModel)JsonConvert.DeserializeObject(dat, typeof(JsonFriendInfModel));
            if (!FriendList.ContainsKey(uin))
                FriendList.Add(uin, new FriendInfo());
            FriendList[uin].face = inf.result.face;
            FriendList[uin].occupation = inf.result.occupation;
            FriendList[uin].phone = inf.result.phone;
            FriendList[uin].college = inf.result.college;
            FriendList[uin].blood = inf.result.blood;
            FriendList[uin].homepage = inf.result.homepage;
            FriendList[uin].vip_info = inf.result.vip_info;
            FriendList[uin].country = inf.result.country;
            FriendList[uin].city = inf.result.city;
            FriendList[uin].personal = inf.result.personal;
            FriendList[uin].nick = inf.result.nick;
            FriendList[uin].shengxiao = inf.result.shengxiao;
            FriendList[uin].email = inf.result.email;
            FriendList[uin].province = inf.result.province;
            FriendList[uin].gender = inf.result.gender;
            if (inf.result.birthday.year != 0 && inf.result.birthday.month != 0 && inf.result.birthday.day != 0)
                FriendList[uin].birthday = new DateTime(inf.result.birthday.year, inf.result.birthday.month, inf.result.birthday.day);
        }
        //更新主界面好友列表
        internal void ReNewListBoxFriend()
        {
            listBoxFriend.Items.Clear();
            foreach (KeyValuePair<string, FriendInfo> FriendList in FriendList)
            {
                listBoxFriend.Items.Add(FriendList.Key + ":" + Info_RealQQ(FriendList.Key) + ":" + FriendList.Value.nick);
            }
        }
        //获取真实QQ号码
        internal static string Info_RealQQ(string uin)
        {
            if (RealQQNum.ContainsKey(uin))
                return RealQQNum[uin];

            string url = "http://s.web2.qq.com/api/get_friend_uin2?tuin=#{uin}&type=1&vfwebqq=#{vfwebqq}&t=#{t}".Replace("#{uin}", uin).Replace("#{vfwebqq}", vfwebqq).Replace("#{t}", AID_TimeStamp());
            string dat = HttpClient.Get(url);
            string temp = dat.Split('\"')[10].Split(',')[0].Replace(":", "");
            if (temp != "" && !RealQQNum.ContainsKey(uin))
            {
                RealQQNum.Add(uin, temp);
                return temp;
            }
            else return "";
        }
        //获取群列表并保存
        internal void Info_GroupList()
        {
            string url = "http://s.web2.qq.com/api/get_group_name_list_mask2";
            string sendData = string.Format("r={{\"vfwebqq\":\"{0}\",\"hash\":\"{1}\"}}", vfwebqq, hash);
            string dat = HttpClient.Post(url, sendData, "http://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2");

            JsonGroupModel group = (JsonGroupModel)JsonConvert.DeserializeObject(dat, typeof(JsonGroupModel));
            for (int i = 0; i < group.result.gnamelist.Count; i++)
            {
                if (!GroupList.ContainsKey(group.result.gnamelist[i].gid))
                    GroupList.Add(group.result.gnamelist[i].gid, new GroupInfo());
                GroupList[group.result.gnamelist[i].gid].name = group.result.gnamelist[i].name;
                GroupList[group.result.gnamelist[i].gid].code = group.result.gnamelist[i].code;
                Info_GroupInfo(group.result.gnamelist[i].gid);
                GetGroupSetting(group.result.gnamelist[i].gid);
            }
            ReNewListBoxGroup();
        }
        //获取群详细信息
        internal static void Info_GroupInfo(string gid)
        {
            if (!GroupList.ContainsKey(gid))
                return;
            string gcode = GroupList[gid].code;
            string url = "http://s.web2.qq.com/api/get_group_info_ext2?gcode=#{group_code}&vfwebqq=#{vfwebqq}&t=#{t}".Replace("#{group_code}", gcode).Replace("#{vfwebqq}", vfwebqq).Replace("#{t}", AID_TimeStamp());
            string dat = HttpClient.Get(url, "http://s.web2.qq.com/proxy.html?v=20130916001&callback=1&id=1");
            JsonGroupInfoModel groupInfo = (JsonGroupInfoModel)JsonConvert.DeserializeObject(dat, typeof(JsonGroupInfoModel));
            GroupList[gid].name = groupInfo.result.ginfo.name;
            GroupList[gid].createtime = groupInfo.result.ginfo.createtime;
            GroupList[gid].face = groupInfo.result.ginfo.face;
            GroupList[gid].owner = groupInfo.result.ginfo.owner;
            GroupList[gid].memo = groupInfo.result.ginfo.memo;
            GroupList[gid].markname = groupInfo.result.ginfo.markname;
            GroupList[gid].level = groupInfo.result.ginfo.level;
            for (int i = 0; i < groupInfo.result.minfo.Count; i++)
            {
                if (!GroupList[gid].MemberList.ContainsKey(groupInfo.result.minfo[i].uin))
                    GroupList[gid].MemberList.Add(groupInfo.result.minfo[i].uin, new GroupInfo.MenberInfo());
                GroupList[gid].MemberList[groupInfo.result.minfo[i].uin].city = groupInfo.result.minfo[i].city;
                GroupList[gid].MemberList[groupInfo.result.minfo[i].uin].province = groupInfo.result.minfo[i].province;
                GroupList[gid].MemberList[groupInfo.result.minfo[i].uin].country = groupInfo.result.minfo[i].country;
                GroupList[gid].MemberList[groupInfo.result.minfo[i].uin].gender = groupInfo.result.minfo[i].gender;
                GroupList[gid].MemberList[groupInfo.result.minfo[i].uin].nick = groupInfo.result.minfo[i].nick;
            }
            if (groupInfo.result.cards != null)
                for (int i = 0; i < groupInfo.result.cards.Count; i++)
                {
                    if (!GroupList[gid].MemberList.ContainsKey(groupInfo.result.cards[i].muin))
                        GroupList[gid].MemberList.Add(groupInfo.result.cards[i].muin, new GroupInfo.MenberInfo());
                    GroupList[gid].MemberList[groupInfo.result.cards[i].muin].card = groupInfo.result.cards[i].card;
                }
            for (int i = 0; i < groupInfo.result.ginfo.members.Count; i++)
                if (groupInfo.result.ginfo.members[i].mflag % 2 == 1)
                    GroupList[gid].MemberList[groupInfo.result.ginfo.members[i].muin].isManager = true;
                else GroupList[gid].MemberList[groupInfo.result.ginfo.members[i].muin].isManager = false;
        }
        //获取指定群的信息
        internal void GetGroupSetting(string gid)
        {
            string url = Date.DicServer + "groupmanage.php?password=" + Date.DicPassword + "&action=get&gno=" + AID_GroupKey(gid);
            string temp = HttpClient.Get(url);
            JsonGroupManageModel GroupManageInfo = (JsonGroupManageModel)JsonConvert.DeserializeObject(temp, typeof(JsonGroupManageModel));
            if (GroupManageInfo.statu.Equals("success"))
            {
                GroupList[gid].GroupManage.enable = GroupManageInfo.enable;
                GroupList[gid].GroupManage.enableXHJ = GroupManageInfo.enablexhj;
                GroupList[gid].GroupManage.enableWeather = GroupManageInfo.enableWeather;
                GroupList[gid].GroupManage.enableTalk = GroupManageInfo.enabletalk;
                GroupList[gid].GroupManage.enableStudy = GroupManageInfo.enableStudy;
                GroupList[gid].GroupManage.enableStock = GroupManageInfo.enableStock;
                GroupList[gid].GroupManage.enableExchangeRate = GroupManageInfo.enableExchangeRate;
                GroupList[gid].GroupManage.enableEmoje = GroupManageInfo.enableEmoje;
                GroupList[gid].GroupManage.enableCityInfo = GroupManageInfo.enableCityInfo;
                GroupList[gid].GroupManage.enableWiki = GroupManageInfo.enableWiki;
                GroupList[gid].GroupManage.enableTranslate = GroupManageInfo.enableTranslate;

                if (GroupList[gid].GroupManage.enable.Equals(""))
                    GroupList[gid].GroupManage.enable = "true";
                if (GroupList[gid].GroupManage.enableXHJ.Equals(""))
                    GroupList[gid].GroupManage.enableXHJ = "true";
                if (GroupList[gid].GroupManage.enableWeather.Equals(""))
                    GroupList[gid].GroupManage.enableWeather = "true";
                if (GroupList[gid].GroupManage.enableTalk.Equals(""))
                    GroupList[gid].GroupManage.enableTalk = "true";
                if (GroupList[gid].GroupManage.enableStudy.Equals(""))
                    GroupList[gid].GroupManage.enableStudy = "true";
                if (GroupList[gid].GroupManage.enableStock.Equals(""))
                    GroupList[gid].GroupManage.enableStock = "true";
                if (GroupList[gid].GroupManage.enableExchangeRate.Equals(""))
                    GroupList[gid].GroupManage.enableExchangeRate = "true";
                if (GroupList[gid].GroupManage.enableEmoje.Equals(""))
                    GroupList[gid].GroupManage.enableEmoje = "true";
                if (GroupList[gid].GroupManage.enableCityInfo.Equals(""))
                    GroupList[gid].GroupManage.enableCityInfo = "true";
                if (GroupList[gid].GroupManage.enableWiki.Equals(""))
                    GroupList[gid].GroupManage.enableWiki = "true";
                if (GroupList[gid].GroupManage.enableTranslate.Equals(""))
                    GroupList[gid].GroupManage.enableTranslate = "true";
            }
        }
        //生成由群主QQ和群创建时间构成的群标识码
        internal string AID_GroupKey(string gid)
        {
            if (!GroupList.ContainsKey(gid))
                Info_GroupList();
            if (GroupList.ContainsKey(gid))
                return Info_RealQQ(GroupList[gid].owner) + ":" + GroupList[gid].createtime;
            else return "FAIL";
        }
        //更新主界面的QQ群列表
        internal void ReNewListBoxGroup()
        {
            listBoxGroup.Items.Clear();
            foreach (KeyValuePair<string, GroupInfo> GroupList in GroupList)
            {
                listBoxGroup.Items.Add(GroupList.Key + "::" + GroupList.Value.name);
            }
        }
        //获取讨论组并保存
        internal void Info_DisscussList()
        {
            string url = "http://s.web2.qq.com/api/get_discus_list?clientid=53999199&psessionid=#{psessionid}&vfwebqq=#{vfwebqq}&t=#{t}".Replace("#{psessionid}", psessionid).Replace("#{vfwebqq}", vfwebqq).Replace("#{t}", AID_TimeStamp());
            string dat = HttpClient.Get(url, "http://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2");
            JsonDisscussModel disscuss = (JsonDisscussModel)JsonConvert.DeserializeObject(dat, typeof(JsonDisscussModel));
            for (int i = 0; i < disscuss.result.dnamelist.Count; i++)
            {
                if (!DiscussList.ContainsKey(disscuss.result.dnamelist[i].did))
                    DiscussList.Add(disscuss.result.dnamelist[i].did, new DiscussInfo());
                DiscussList[disscuss.result.dnamelist[i].did].name = disscuss.result.dnamelist[i].name;
                Info_DisscussInfo(disscuss.result.dnamelist[i].did);
            }
            ReNewListBoxDiscuss();
        }
        //获取讨论组详细信息
        internal void Info_DisscussInfo(string did)
        {
            string url = "http://d1.web2.qq.com/channel/get_discu_info?did=#{discuss_id}&psessionid=#{psessionid}&vfwebqq=#{vfwebqq}&clientid=53999199&t=#{t}".Replace("#{t}", AID_TimeStamp());
            url = url.Replace("#{discuss_id}", did).Replace("#{psessionid}", psessionid).Replace("#{vfwebqq}", vfwebqq);
            string dat = HttpClient.Get(url, "http://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2");
            JsonDisscussInfoModel inf = (JsonDisscussInfoModel)JsonConvert.DeserializeObject(dat, typeof(JsonDisscussInfoModel));

            for (int i = 0; i < inf.result.mem_info.Count; i++)
            {
                if (!DiscussList[did].MemberList.ContainsKey(inf.result.mem_info[i].uin))
                    DiscussList[did].MemberList.Add(inf.result.mem_info[i].uin, new DiscussInfo.MenberInfo());
                DiscussList[did].MemberList[inf.result.mem_info[i].uin].nick = inf.result.mem_info[i].nick;
            }
            for (int i = 0; i < inf.result.mem_status.Count; i++)
            {
                if (!DiscussList[did].MemberList.ContainsKey(inf.result.mem_status[i].uin))
                    DiscussList[did].MemberList.Add(inf.result.mem_status[i].uin, new DiscussInfo.MenberInfo());
                DiscussList[did].MemberList[inf.result.mem_status[i].uin].status = inf.result.mem_status[i].status;
                DiscussList[did].MemberList[inf.result.mem_status[i].uin].client_type = inf.result.mem_status[i].client_type;
            }
        }
        //更新讨论组列表
        internal void ReNewListBoxDiscuss()
        {
            listBoxDiscuss.Items.Clear();
            foreach (KeyValuePair<string, DiscussInfo> DiscussList in DiscussList)
            {
                listBoxDiscuss.Items.Add(DiscussList.Key + ":" + DiscussList.Value.name);
            }
        }
        //错误信息处理
        
        private void Message_Process_Error(JsonPollMessage poll)
        {
            int TempCount103 = Count103;
            Count103 = 0;
            if (poll.retcode == 102)
            {
                return;
            }
            else if (poll.retcode == 103)
            {
                listBoxLog.Items.Insert(0, "retcode:103");
                Count103 = TempCount103 + 1;
                if (Count103 > 20)
                {
                    Running = false;
                    MessageBox.Show("retcode:" + poll.retcode);
                }
                return;
            }
            else if (poll.retcode == 116)
            {
                listBoxLog.Items.Insert(0, "retcode:" + poll.retcode + poll.p);
                ptwebqq = poll.p;
                return;
            }
            else if (poll.retcode == 108 || poll.retcode == 114)
            {
                listBoxLog.Items.Insert(0, "retcode:" + poll.retcode);
                Running = false;
                MessageBox.Show("retcode:" + poll.retcode);
                return;
            }
            else if (poll.retcode == 120 || poll.retcode == 121)
            {
                listBoxLog.Items.Insert(0, "retcode:" + poll.retcode);
                listBoxLog.Items.Insert(0, poll.t);
                Running = false;
                MessageBox.Show("retcode:" + poll.retcode);
                return;
            }
            else if (poll.retcode == 100006 || poll.retcode == 100003)
            {
                listBoxLog.Items.Insert(0, "retcode:" + poll.retcode);
                Running = false;
                MessageBox.Show("retcode:" + poll.retcode);
                return;
            }
            listBoxLog.Items.Insert(0, "retcode:" + poll.retcode);
        }
        //私聊消息处理
        private void Message_Process_Message(JsonPollMessage.paramResult.paramValue value)
        {
            string message = Message_Process_GetMessageText(value.content);
            string nick = "未知";
            if (!FriendList.ContainsKey(value.from_uin))
                Info_FriendList();
            if (FriendList.ContainsKey(value.from_uin))
                nick = FriendList[value.from_uin].nick;
            AddAndReNewTextBoxFriendChat(value.from_uin, (nick + "  " + Info_RealQQ(value.from_uin) + Environment.NewLine + message), false);
            AnswerMessage(value.from_uin, message, 0);
        }
        //处理poll包中的消息数组
        private static string Message_Process_GetMessageText(List<object> content)
        {
            string message = "";
            for (int i = 1; i < content.Count; i++)
            {
                if (content[i].ToString().Contains("[\"cface\","))
                    continue;
                else if (content[i].ToString().Contains("\"face\","))
                    message += ("{..[face" + content[i].ToString().Replace("\"face\",", "").Replace("]", "").Replace("[", "").Replace(" ", "").Replace("\r", "").Replace("\n", "") + "]..}");
                else
                    message += content[i].ToString();
            }
            message = message.Replace("\\\\n", Environment.NewLine).Replace("＆", "&");
            return message;
        }

        internal void AddAndReNewTextBoxFriendChat(string uin, string str = "", bool ChangeCurrentUin = false)
        {
            FriendList[uin].Messages += str + Environment.NewLine;
            if (ChangeCurrentUin || (listBoxFriend.SelectedItem != null && uin.Equals(listBoxFriend.SelectedItem.ToString().Split(':')[0])))
                textBoxFriendChat.Text = FriendList[uin].Messages;
        }


    }
}
