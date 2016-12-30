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

        public MainWindow()
        {
            InitializeComponent();
            Login_Process(Date.webqqUrl);
            
        }
        //扫描二维码后处理函数
        private static void Login_Process(string url)
        {
            Login_GetPtwebqq(url);
            Login_GetVfwebqq();
            Login_GetPsessionid();
            Info_FriendList();
            Info_GroupList();
            Info_DisscussList();
            Info_SelfInfo();//获取账号信息
            Login_GetOnlineAndRecent_FAKE();
            Task.Run(() => Message_Request());

            m = "当前登录状态：QQ " + QQNum + "已登录";
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
        internal static void Info_FriendList()
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
        internal static void ReNewListBoxFriend()
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
    }
}
