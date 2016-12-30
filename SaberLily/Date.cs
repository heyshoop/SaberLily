using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaberLily
{
    class Date
    {
        public static string webqqUrl;
        public static string QQNum;
        internal static string DicPassword = "";
        internal static string DicServer = "";
    }
    //好友资料
    public class FriendInfo
    {
        public string markname;
        public string nick;
        public string gender;

        public int face;
        public int client_type;
        public int categories;
        public string status;

        public string occupation;   //职业                    
        public string college;
        public string country;
        public string province;
        public string city;
        public string personal;     //简介

        public string homepage;
        public string email;
        public string mobile;
        public string phone;

        public DateTime birthday;
        public int blood;
        public int shengxiao;
        public int vip_info;

        public string Messages = "";
    }
    //群资料
    public class GroupInfo
    {
        public string name;
        public string code;
        public string markname;
        public string memo;
        public int face;
        public string createtime;
        public int level;
        public string owner;
        public GroupManageClass GroupManage = new GroupManageClass();
        public class GroupManageClass
        {
            public string enable;
            public string enableWeather;
            public string enableExchangeRate;
            public string enableStock;
            public string enableStudy;
            public string enableTalk;
            public string enableXHJ;
            public string enableEmoje;
            public string enableCityInfo;
            public string enableWiki;
            public string enableTranslate;
        }
        public Dictionary<string, MenberInfo> MemberList = new Dictionary<string, MenberInfo>();
        public class MenberInfo
        {
            public string nick;
            public string country;
            public string province;
            public string city;
            public string gender;
            public string card;
            public bool isManager;
        }
        public string Messages = "";
    }
    //讨论组
    public class DiscussInfo
    {
        public string name;
        public Dictionary<string, MenberInfo> MemberList = new Dictionary<string, MenberInfo>();
        public class MenberInfo
        {
            public string nick;
            public string status;
            public int client_type;
        }
        public string Messages = "";
    }
}
