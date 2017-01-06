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
        private static bool Running = true;
        private static int Count103 = 0;
        Saber saber = new Saber();

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

            Date.SelfInfo.face = inf.result.face;
            Date.SelfInfo.occupation = inf.result.occupation;
            Date.SelfInfo.phone = inf.result.phone;
            Date.SelfInfo.college = inf.result.college;
            Date.SelfInfo.blood = inf.result.blood;
            Date.SelfInfo.homepage = inf.result.homepage;
            Date.SelfInfo.vip_info = inf.result.vip_info;
            Date.SelfInfo.country = inf.result.country;
            Date.SelfInfo.city = inf.result.city;
            Date.SelfInfo.personal = inf.result.personal;
            Date.SelfInfo.nick = inf.result.nick;
            Date.SelfInfo.shengxiao = inf.result.shengxiao;
            Date.SelfInfo.email = inf.result.email;
            Date.SelfInfo.province = inf.result.province;
            Date.SelfInfo.gender = inf.result.gender;
            if (inf.result.birthday.year != 0 && inf.result.birthday.month != 0 && inf.result.birthday.day != 0)
                Date.SelfInfo.birthday = new DateTime(inf.result.birthday.year, inf.result.birthday.month, inf.result.birthday.day);
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
                if (!Date.FriendList.ContainsKey(friend.result.info[i].uin))
                    Date.FriendList.Add(friend.result.info[i].uin, new FriendInfo());
                Date.FriendList[friend.result.info[i].uin].face = friend.result.info[i].face;
                Date.FriendList[friend.result.info[i].uin].nick = friend.result.info[i].nick;
                Info_FriendInfo(friend.result.info[i].uin);
            }
            for (int i = 0; i < friend.result.friends.Count; i++)
            {
                if (!Date.FriendList.ContainsKey(friend.result.friends[i].uin))
                    Date.FriendList.Add(friend.result.friends[i].uin, new FriendInfo());
                Date.FriendList[friend.result.friends[i].uin].categories = friend.result.friends[i].categories;
            }
            for (int i = 0; i < friend.result.categories.Count; i++)
            {
                Date.FriendCategories[friend.result.categories[i].index] = friend.result.categories[i].name;
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
            if (!Date.FriendList.ContainsKey(uin))
                Date.FriendList.Add(uin, new FriendInfo());
            Date.FriendList[uin].face = inf.result.face;
            Date.FriendList[uin].occupation = inf.result.occupation;
            Date.FriendList[uin].phone = inf.result.phone;
            Date.FriendList[uin].college = inf.result.college;
            Date.FriendList[uin].blood = inf.result.blood;
            Date.FriendList[uin].homepage = inf.result.homepage;
            Date.FriendList[uin].vip_info = inf.result.vip_info;
            Date.FriendList[uin].country = inf.result.country;
            Date.FriendList[uin].city = inf.result.city;
            Date.FriendList[uin].personal = inf.result.personal;
            Date.FriendList[uin].nick = inf.result.nick;
            Date.FriendList[uin].shengxiao = inf.result.shengxiao;
            Date.FriendList[uin].email = inf.result.email;
            Date.FriendList[uin].province = inf.result.province;
            Date.FriendList[uin].gender = inf.result.gender;
            if (inf.result.birthday.year != 0 && inf.result.birthday.month != 0 && inf.result.birthday.day != 0)
                Date.FriendList[uin].birthday = new DateTime(inf.result.birthday.year, inf.result.birthday.month, inf.result.birthday.day);
        }
        //更新主界面好友列表
        internal void ReNewListBoxFriend()
        {
            listBoxFriend.Items.Clear();
            foreach (KeyValuePair<string, FriendInfo> FriendList in Date.FriendList)
            {
                listBoxFriend.Items.Add(FriendList.Key + ":" + Info_RealQQ(FriendList.Key) + ":" + FriendList.Value.nick);
            }
        }
        //获取真实QQ号码
        internal static string Info_RealQQ(string uin)
        {
            if (Date.RealQQNum.ContainsKey(uin))
                return Date.RealQQNum[uin];

            string url = "http://s.web2.qq.com/api/get_friend_uin2?tuin=#{uin}&type=1&vfwebqq=#{vfwebqq}&t=#{t}".Replace("#{uin}", uin).Replace("#{vfwebqq}", vfwebqq).Replace("#{t}", AID_TimeStamp());
            string dat = HttpClient.Get(url);
            string temp = dat.Split('\"')[10].Split(',')[0].Replace(":", "");
            if (temp != "" && !Date.RealQQNum.ContainsKey(uin))
            {
                Date.RealQQNum.Add(uin, temp);
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
                if (!Date.GroupList.ContainsKey(group.result.gnamelist[i].gid))
                    Date.GroupList.Add(group.result.gnamelist[i].gid, new GroupInfo());
                Date.GroupList[group.result.gnamelist[i].gid].name = group.result.gnamelist[i].name;
                Date.GroupList[group.result.gnamelist[i].gid].code = group.result.gnamelist[i].code;
                Info_GroupInfo(group.result.gnamelist[i].gid);
                GetGroupSetting(group.result.gnamelist[i].gid);
            }
            ReNewListBoxGroup();
        }
        //获取群详细信息
        internal static void Info_GroupInfo(string gid)
        {
            if (!Date.GroupList.ContainsKey(gid))
                return;
            string gcode = Date.GroupList[gid].code;
            string url = "http://s.web2.qq.com/api/get_group_info_ext2?gcode=#{group_code}&vfwebqq=#{vfwebqq}&t=#{t}".Replace("#{group_code}", gcode).Replace("#{vfwebqq}", vfwebqq).Replace("#{t}", AID_TimeStamp());
            string dat = HttpClient.Get(url, "http://s.web2.qq.com/proxy.html?v=20130916001&callback=1&id=1");
            JsonGroupInfoModel groupInfo = (JsonGroupInfoModel)JsonConvert.DeserializeObject(dat, typeof(JsonGroupInfoModel));
            Date.GroupList[gid].name = groupInfo.result.ginfo.name;
            Date.GroupList[gid].createtime = groupInfo.result.ginfo.createtime;
            Date.GroupList[gid].face = groupInfo.result.ginfo.face;
            Date.GroupList[gid].owner = groupInfo.result.ginfo.owner;
            Date.GroupList[gid].memo = groupInfo.result.ginfo.memo;
            Date.GroupList[gid].markname = groupInfo.result.ginfo.markname;
            Date.GroupList[gid].level = groupInfo.result.ginfo.level;
            for (int i = 0; i < groupInfo.result.minfo.Count; i++)
            {
                if (!Date.GroupList[gid].MemberList.ContainsKey(groupInfo.result.minfo[i].uin))
                    Date.GroupList[gid].MemberList.Add(groupInfo.result.minfo[i].uin, new GroupInfo.MenberInfo());
                Date.GroupList[gid].MemberList[groupInfo.result.minfo[i].uin].city = groupInfo.result.minfo[i].city;
                Date.GroupList[gid].MemberList[groupInfo.result.minfo[i].uin].province = groupInfo.result.minfo[i].province;
                Date.GroupList[gid].MemberList[groupInfo.result.minfo[i].uin].country = groupInfo.result.minfo[i].country;
                Date.GroupList[gid].MemberList[groupInfo.result.minfo[i].uin].gender = groupInfo.result.minfo[i].gender;
                Date.GroupList[gid].MemberList[groupInfo.result.minfo[i].uin].nick = groupInfo.result.minfo[i].nick;
            }
            if (groupInfo.result.cards != null)
                for (int i = 0; i < groupInfo.result.cards.Count; i++)
                {
                    if (!Date.GroupList[gid].MemberList.ContainsKey(groupInfo.result.cards[i].muin))
                        Date.GroupList[gid].MemberList.Add(groupInfo.result.cards[i].muin, new GroupInfo.MenberInfo());
                    Date.GroupList[gid].MemberList[groupInfo.result.cards[i].muin].card = groupInfo.result.cards[i].card;
                }
            for (int i = 0; i < groupInfo.result.ginfo.members.Count; i++)
                if (groupInfo.result.ginfo.members[i].mflag % 2 == 1)
                    Date.GroupList[gid].MemberList[groupInfo.result.ginfo.members[i].muin].isManager = true;
                else Date.GroupList[gid].MemberList[groupInfo.result.ginfo.members[i].muin].isManager = false;
        }
        //获取指定群的信息
        internal void GetGroupSetting(string gid)
        {
            string url = Date.DicServer + "groupmanage.php?password=" + Date.DicPassword + "&action=get&gno=" + AID_GroupKey(gid);
            string temp = HttpClient.Get(url);
            JsonGroupManageModel GroupManageInfo = (JsonGroupManageModel)JsonConvert.DeserializeObject(temp, typeof(JsonGroupManageModel));
            if (GroupManageInfo.statu.Equals("success"))
            {
                Date.GroupList[gid].GroupManage.enable = GroupManageInfo.enable;
                Date.GroupList[gid].GroupManage.enableXHJ = GroupManageInfo.enablexhj;
                Date.GroupList[gid].GroupManage.enableWeather = GroupManageInfo.enableWeather;
                Date.GroupList[gid].GroupManage.enableTalk = GroupManageInfo.enabletalk;
                Date.GroupList[gid].GroupManage.enableStudy = GroupManageInfo.enableStudy;
                Date.GroupList[gid].GroupManage.enableStock = GroupManageInfo.enableStock;
                Date.GroupList[gid].GroupManage.enableExchangeRate = GroupManageInfo.enableExchangeRate;
                Date.GroupList[gid].GroupManage.enableEmoje = GroupManageInfo.enableEmoje;
                Date.GroupList[gid].GroupManage.enableCityInfo = GroupManageInfo.enableCityInfo;
                Date.GroupList[gid].GroupManage.enableWiki = GroupManageInfo.enableWiki;
                Date.GroupList[gid].GroupManage.enableTranslate = GroupManageInfo.enableTranslate;

                if (Date.GroupList[gid].GroupManage.enable.Equals(""))
                    Date.GroupList[gid].GroupManage.enable = "true";
                if (Date.GroupList[gid].GroupManage.enableXHJ.Equals(""))
                    Date.GroupList[gid].GroupManage.enableXHJ = "true";
                if (Date.GroupList[gid].GroupManage.enableWeather.Equals(""))
                    Date.GroupList[gid].GroupManage.enableWeather = "true";
                if (Date.GroupList[gid].GroupManage.enableTalk.Equals(""))
                    Date.GroupList[gid].GroupManage.enableTalk = "true";
                if (Date.GroupList[gid].GroupManage.enableStudy.Equals(""))
                    Date.GroupList[gid].GroupManage.enableStudy = "true";
                if (Date.GroupList[gid].GroupManage.enableStock.Equals(""))
                    Date.GroupList[gid].GroupManage.enableStock = "true";
                if (Date.GroupList[gid].GroupManage.enableExchangeRate.Equals(""))
                    Date.GroupList[gid].GroupManage.enableExchangeRate = "true";
                if (Date.GroupList[gid].GroupManage.enableEmoje.Equals(""))
                    Date.GroupList[gid].GroupManage.enableEmoje = "true";
                if (Date.GroupList[gid].GroupManage.enableCityInfo.Equals(""))
                    Date.GroupList[gid].GroupManage.enableCityInfo = "true";
                if (Date.GroupList[gid].GroupManage.enableWiki.Equals(""))
                    Date.GroupList[gid].GroupManage.enableWiki = "true";
                if (Date.GroupList[gid].GroupManage.enableTranslate.Equals(""))
                    Date.GroupList[gid].GroupManage.enableTranslate = "true";
            }
        }
        //生成由群主QQ和群创建时间构成的群标识码
        internal string AID_GroupKey(string gid)
        {
            if (!Date.GroupList.ContainsKey(gid))
                Info_GroupList();
            if (Date.GroupList.ContainsKey(gid))
                return Info_RealQQ(Date.GroupList[gid].owner) + ":" + Date.GroupList[gid].createtime;
            else return "FAIL";
        }
        //更新主界面的QQ群列表
        internal void ReNewListBoxGroup()
        {
            listBoxGroup.Items.Clear();
            foreach (KeyValuePair<string, GroupInfo> GroupList in Date.GroupList)
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
                if (!Date.DiscussList.ContainsKey(disscuss.result.dnamelist[i].did))
                    Date.DiscussList.Add(disscuss.result.dnamelist[i].did, new DiscussInfo());
                Date.DiscussList[disscuss.result.dnamelist[i].did].name = disscuss.result.dnamelist[i].name;
                Info_DisscussInfo(disscuss.result.dnamelist[i].did);
            }
            ReNewListBoxDiscuss();
        }
        //获取讨论组详细信息
        internal static void Info_DisscussInfo(string did)
        {
            string url = "http://d1.web2.qq.com/channel/get_discu_info?did=#{discuss_id}&psessionid=#{psessionid}&vfwebqq=#{vfwebqq}&clientid=53999199&t=#{t}".Replace("#{t}", AID_TimeStamp());
            url = url.Replace("#{discuss_id}", did).Replace("#{psessionid}", psessionid).Replace("#{vfwebqq}", vfwebqq);
            string dat = HttpClient.Get(url, "http://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2");
            JsonDisscussInfoModel inf = (JsonDisscussInfoModel)JsonConvert.DeserializeObject(dat, typeof(JsonDisscussInfoModel));

            for (int i = 0; i < inf.result.mem_info.Count; i++)
            {
                if (!Date.DiscussList[did].MemberList.ContainsKey(inf.result.mem_info[i].uin))
                    Date.DiscussList[did].MemberList.Add(inf.result.mem_info[i].uin, new DiscussInfo.MenberInfo());
                Date.DiscussList[did].MemberList[inf.result.mem_info[i].uin].nick = inf.result.mem_info[i].nick;
            }
            for (int i = 0; i < inf.result.mem_status.Count; i++)
            {
                if (!Date.DiscussList[did].MemberList.ContainsKey(inf.result.mem_status[i].uin))
                    Date.DiscussList[did].MemberList.Add(inf.result.mem_status[i].uin, new DiscussInfo.MenberInfo());
                Date.DiscussList[did].MemberList[inf.result.mem_status[i].uin].status = inf.result.mem_status[i].status;
                Date.DiscussList[did].MemberList[inf.result.mem_status[i].uin].client_type = inf.result.mem_status[i].client_type;
            }
        }
        //更新讨论组列表
        internal void ReNewListBoxDiscuss()
        {
            listBoxDiscuss.Items.Clear();
            foreach (KeyValuePair<string, DiscussInfo> DiscussList in Date.DiscussList)
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
            if (!Date.FriendList.ContainsKey(value.from_uin))
                Info_FriendList();
            if (Date.FriendList.ContainsKey(value.from_uin))
                nick = Date.FriendList[value.from_uin].nick;
            AddAndReNewTextBoxFriendChat(value.from_uin, (nick + "  " + Info_RealQQ(value.from_uin) + Environment.NewLine + message), false);
            saber.AnswerMessage(value.from_uin, message, 0);
        }
        //群聊消息处理
        private void Message_Process_GroupMessage(JsonPollMessage.paramResult.paramValue value)
        {
            string message = Message_Process_GetMessageText(value.content);
            string gid = value.from_uin;
            string gno = AID_GroupKey(gid);
            if (gno.Equals("FAIL"))
                return;
            string nick = "未知";
            if (Date.GroupList[gid].MemberList.ContainsKey(value.send_uin))
                nick = Date.GroupList[gid].MemberList[value.send_uin].nick;
            if (Info_RealQQ(value.send_uin).Equals("1000000"))
                nick = "系统消息";
            AddAndReNewTextBoxGroupChat(value.from_uin, (Date.GroupList[gid].name + "   " + nick + "  " + Info_RealQQ(value.send_uin) + Environment.NewLine + message), false);
            saber.AnswerGroupMessage(gid, message, value.send_uin, gno);
        }
        //讨论组消息处理
        private void Message_Process_DisscussMessage(JsonPollMessage.paramResult.paramValue value)
        {
            string message = Message_Process_GetMessageText(value.content);
            string DName = "讨论组";
            string SenderNick = "未知";
            if (!Date.DiscussList.ContainsKey(value.did))
                Info_DisscussList();
            if (Date.DiscussList.ContainsKey(value.did))
            {
                DName += Date.DiscussList[value.did].name;
                if (Date.DiscussList[value.did].MemberList.ContainsKey(value.send_uin))
                    SenderNick = Date.DiscussList[value.did].MemberList[value.send_uin].nick;
            }
            else DName = "未知讨论组";
            if (Info_RealQQ(value.send_uin).Equals("1000000"))
                SenderNick = "系统消息";
            AddAndReNewTextBoxDiscussChat(value.from_uin, (DName + "   " + SenderNick + "  " + Info_RealQQ(value.send_uin) + Environment.NewLine + message), false);
            saber.AnswerMessage(value.did, message, 2);
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
            Date.FriendList[uin].Messages += str + Environment.NewLine;
            if (ChangeCurrentUin || (listBoxFriend.SelectedItem != null && uin.Equals(listBoxFriend.SelectedItem.ToString().Split(':')[0])))
                textBoxFriendChat.Text = Date.FriendList[uin].Messages;
        }
        internal void AddAndReNewTextBoxGroupChat(string gid, string str = "", bool ChangeCurrentGid = false)
        {
            Date.GroupList[gid].Messages += str + Environment.NewLine;
            if (ChangeCurrentGid || (listBoxGroup.SelectedItem != null && gid.Equals(listBoxGroup.SelectedItem.ToString().Split(':')[0])))
                textBoxGroupChat.Text = Date.GroupList[gid].Messages;
        }
        internal void AddAndReNewTextBoxDiscussChat(string did, string str = "", bool ChangeCurrentDid = false)
        {
            Date.DiscussList[did].Messages += str + Environment.NewLine;
            if (ChangeCurrentDid || (listBoxDiscuss.SelectedItem != null && did.Equals(listBoxDiscuss.SelectedItem.ToString().Split(':')[0])))
                textBoxDiscussChat.Text = Date.DiscussList[did].Messages;
        }
        //发送消息
        public bool Message_Send(int type, string id, string messageToSend, bool auto = true)
        {
            if (auto)
            {
                if (type == 0)
                    AddAndReNewTextBoxFriendChat(id, ("自动回复：" + Environment.NewLine + messageToSend));
                else if (type == 1)
                    AddAndReNewTextBoxGroupChat(id, ("自动回复：" + Environment.NewLine + messageToSend));
                else if (type == 2)
                    AddAndReNewTextBoxDiscussChat(id, ("自动回复：" + Environment.NewLine + messageToSend));
            }
            listBoxLog.Items.Add(type + ":" + id + ":" + messageToSend);
            if (messageToSend.Equals("") || id.Equals(""))
                return false;

            string[] tmp = messageToSend.Split("{}".ToCharArray());
            messageToSend = "";
            for (int i = 0; i < tmp.Length; i++)
                if (!tmp[i].Trim().StartsWith("..[face") || !tmp[i].Trim().EndsWith("].."))
                    messageToSend += "\\\"" + tmp[i] + "\\\",";
                else
                    messageToSend += tmp[i].Replace("..[face", "[\\\"face\\\",").Replace("]..", "],");
            messageToSend = messageToSend.Remove(messageToSend.LastIndexOf(','));
            messageToSend = messageToSend.Replace("\r\n", "\n").Replace("\n\r", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
            try
            {
                string to_groupuin_did, url;
                switch (type)
                {
                    case 0:
                        to_groupuin_did = "to";
                        url = "http://d1.web2.qq.com/channel/send_buddy_msg2";
                        break;
                    case 1:
                        to_groupuin_did = "group_uin";
                        url = "http://d1.web2.qq.com/channel/send_qun_msg2";
                        break;
                    case 2:
                        to_groupuin_did = "did";
                        url = "http://d1.web2.qq.com/channel/send_discu_msg2";
                        break;
                    default:
                        return false;
                }
                string postData = "{\"#{type}\":#{id},\"content\":\"[#{msg},[\\\"font\\\",{\\\"name\\\":\\\"宋体\\\",\\\"size\\\":10,\\\"style\\\":[0,0,0],\\\"color\\\":\\\"000000\\\"}]]\",\"face\":#{face},\"clientid\":53999199,\"msg_id\":#{msg_id},\"psessionid\":\"#{psessionid}\"}";
                postData = "r=" + HttpUtility.UrlEncode(postData.Replace("#{type}", to_groupuin_did).Replace("#{id}", id).Replace("#{msg}", messageToSend).Replace("#{face}", Date.SelfInfo.face.ToString()).Replace("#{msg_id}", Date.rand.Next(10000000, 99999999).ToString()).Replace("#{psessionid}", psessionid));

                string dat = HttpClient.Post(url, postData, "http://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2");

                return dat.Equals("{\"errCode\":0,\"msg\":\"send ok\"}");
            }
            catch (Exception)
            {
                return false;
            }
        }


    }
}
