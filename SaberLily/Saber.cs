using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//扩展工具类
namespace SaberLily
{
    class Saber
    {
        //系统配置相关
        internal static string MasterQQ = "";
        internal static string DicPassword = "";
        internal static string DicServer = "";
        internal static bool NoDicPassword = false;
        public static string[] Badwords;
        MainWindow mainWindow = new MainWindow();

        //答复
        private string[] Answer(string message, string uin, string gid = "", string gno = "")
        {
            string qunnum = gno;
            if (qunnum.Equals(""))
                qunnum = "NULL";
            string[] MessageToSend = new string[20];

            if (message.Equals(""))
                return MessageToSend;
            string QQNum = MainWindow.Info_RealQQ(uin);
            for (int i = 0; i < 20; i++)
                MessageToSend[i] = "";
            bool MsgSendFlag = false;
            if (message.Equals("报工") )
            {
                MessageToSend[0] = "今天你报工了吗？";
                return MessageToSend;
            }
            if (message.StartsWith("天气"))
            {
                bool DisableFlag = false;
                if (!gid.Equals(""))
                {
                    if (Date.GroupList[gid].GroupManage.enableWeather == null)
                        mainWindow.GetGroupSetting(gid);
                    if (Date.GroupList[gid].GroupManage.enableWeather.Equals("false"))
                        DisableFlag = true;
                }
                if (!DisableFlag)
                {
                    bool WeatherFlag = true;
                    string[] tmp = message.Split('&');
                    if ((!tmp[0].Equals("天气")) || (tmp.Length != 2 && tmp.Length != 3))
                    {
                        WeatherFlag = false;
                    }
                    if (WeatherFlag)
                    {
                        if (tmp.Length == 2)
                            MessageToSend[0] = GetInfo.GetWeather(tmp[1], "");
                        else
                            MessageToSend[0] = GetInfo.GetWeather(tmp[1], tmp[2]);

                        string url = DicServer + "log.php";
                        string postdata = "password=" + HttpUtility.UrlEncode(DicPassword) + "&qqnum=" + HttpUtility.UrlEncode(QQNum) + "&qunnum=" + HttpUtility.UrlEncode(qunnum) + "&action=weather&p1=" + HttpUtility.UrlEncode(tmp[1]) + "&p2=NULL&p3=NULL&p4=NULL";
                        HttpClient.Post(url, postdata);
                        return MessageToSend;
                    }
                }
            }
            if (message.StartsWith("翻译"))
            {
                bool DisableFlag = false;
                if (!gid.Equals(""))
                {
                    if (Date.GroupList[gid].GroupManage.enableTranslate == null)
                        mainWindow.GetGroupSetting(gid);
                    if (Date.GroupList[gid].GroupManage.enableTranslate.Equals("false"))
                        DisableFlag = true;
                }
                if (!DisableFlag)
                {
                    bool TranslateFlag = true;
                    string[] tmp = message.Split('&');
                    if ((!tmp[0].Equals("翻译")) || tmp.Length != 2)
                    {
                        TranslateFlag = false;
                    }
                    if (TranslateFlag)
                    {
                        MessageToSend[0] = GetInfo.GetTranslate(tmp[1]);

                        string url = DicServer + "log.php";
                        string postdata = "password=" + HttpUtility.UrlEncode(DicPassword) + "&qqnum=" + HttpUtility.UrlEncode(QQNum) + "&qunnum=" + HttpUtility.UrlEncode(qunnum) + "&action=translate&p1=" + HttpUtility.UrlEncode(tmp[1]) + "&p2=NULL&p3=NULL&p4=NULL";
                        HttpClient.Post(url, postdata);
                        return MessageToSend;
                    }
                }
            }
            if (message.StartsWith("学习") || message.StartsWith("特权学习"))
            {
                bool DisableFlag = false;
                if (!gid.Equals(""))
                {
                    if (SmartQQ.GroupList[gid].GroupManage.enableStudy == null)
                        GetGroupSetting(gid);
                    if (SmartQQ.GroupList[gid].GroupManage.enableStudy.Equals("false"))
                        DisableFlag = true;
                }
                if (!DisableFlag)
                {
                    bool StudyFlag = true;
                    bool SuperStudy = false;
                    string[] tmp = message.Split('^');
                    if ((!tmp[0].Equals("学习")) || tmp.Length != 3)
                    {
                        if ((tmp[0].Equals("特权学习")) && tmp.Length == 3)
                        {
                            SuperStudy = true;
                        }
                        tmp = message.Split('&');
                        if ((!tmp[0].Equals("学习")) || tmp.Length != 3)
                        {
                            StudyFlag = false;
                            if ((tmp[0].Equals("特权学习")) && tmp.Length == 3)
                            {
                                SuperStudy = true;
                            }
                        }
                    }
                    if (tmp.Length != 3 || tmp[1].Replace(" ", "").Equals("") || tmp[2].Replace(" ", "").Equals(""))
                    {
                        StudyFlag = false;
                        SuperStudy = false;
                    }
                    if (SuperStudy)
                    {
                        string result = "";
                        result = AIStudy(tmp[1], tmp[2], QQNum, gno, true);
                        MessageToSend[0] = GetStudyFlagInfo(result, QQNum, tmp[1], tmp[2]);
                        return MessageToSend;
                    }
                    if (StudyFlag)
                    {
                        string result = "";
                        for (int i = 0; i < Badwords.Length; i++)
                            if (tmp[1].Contains(Badwords[i]) || tmp[2].Contains(Badwords[i]))
                            {
                                result = "ForbiddenWord";
                                break;
                            }
                        if (result.Equals(""))
                            result = AIStudy(tmp[1], tmp[2], QQNum, gno, false);
                        MessageToSend[0] = GetStudyFlagInfo(result, QQNum, tmp[1], tmp[2]);

                        string url = DicServer + "log.php";
                        string postdata = "password=" + HttpUtility.UrlEncode(DicPassword) + "&qqnum=" + HttpUtility.UrlEncode(QQNum) + "&qunnum=" + HttpUtility.UrlEncode(qunnum) + "&action=study&p1=" + HttpUtility.UrlEncode(tmp[1]) + "&p2=" + HttpUtility.UrlEncode(tmp[2]) + "&p3=NULL&p4=NULL";
                        HTTP.Post(url, postdata);
                        return MessageToSend;
                    }
                }
            }
            bool DisableTalkFlag = false;
            if (!gid.Equals(""))
            {
                if (SmartQQ.GroupList[gid].GroupManage.enableTalk == null)
                    GetGroupSetting(gid);
                if (SmartQQ.GroupList[gid].GroupManage.enableTalk.Equals("false"))
                    DisableTalkFlag = true;
            }
            if (!DisableTalkFlag)
            {

                MessageToSend[0] = AIGet(message, QQNum, gno);
                if (!MessageToSend[0].Equals(""))
                {
                    string url = DicServer + "log.php";
                    string postdata = "password=" + HttpUtility.UrlEncode(DicPassword) + "&qqnum=" + HttpUtility.UrlEncode(QQNum) + "&qunnum=" + HttpUtility.UrlEncode(qunnum) + "&action=talk&p1=" + HttpUtility.UrlEncode(message) + "&p2=NULL&p3=NULL&p4=NULL";
                    HTTP.Post(url, postdata);
                    return MessageToSend;
                }
                string[] tmp1 = message.Split("@#$(),，.。:：;^&；“”～~！!#（）%？?》《、· \r\n\"".ToCharArray());
                int j = 0;
                bool RepeatFlag = false;
                for (int i = 0; i < tmp1.Length && i < 10; i++)
                {
                    if (tmp1[i].Equals(message))
                        continue;
                    for (int k = 0; k < i; k++)
                        if (tmp1[k].Equals(tmp1[i]))
                            RepeatFlag = true;
                    if (RepeatFlag)
                    {
                        RepeatFlag = false;
                        continue;
                    }
                    if (!tmp1[i].Equals(""))
                    {
                        MessageToSend[j] = AIGet(tmp1[i], QQNum, gno);
                        j++;
                        MsgSendFlag = true;
                    }
                }
                if (!MsgSendFlag)
                {
                    string[] tmp2 = message.Split("@#$(),，.。:：;^&；“”～~！!#（）%？?》《、· \r\n\"啊喔是的么吧呀恩嗯了呢很吗".ToCharArray());
                    j = 0;
                    RepeatFlag = false;
                    for (int i = 0; i < tmp2.Length && i < 10; i++)
                    {
                        if (tmp2[i].Equals(message))
                            continue;
                        for (int k = 0; k < i; k++)
                            if (tmp2[k].Equals(tmp2[i]))
                                RepeatFlag = true;
                        for (int k = 0; k < tmp1.Length; k++)
                            if (tmp1[k].Equals(tmp2[i]))
                                RepeatFlag = true;

                        if (RepeatFlag)
                        {
                            RepeatFlag = false;
                            continue;
                        }
                        if (!tmp2[i].Equals(""))
                        {
                            MessageToSend[j] = AIGet(tmp2[i], QQNum, gno);
                            j++;
                            MsgSendFlag = true;
                        }
                    }
                }

                if (!MsgSendFlag)
                {
                    bool DisableFlag = false;
                    if (!gid.Equals(""))
                    {
                        if (SmartQQ.GroupList[gid].GroupManage.enableXHJ == null)
                            GetGroupSetting(gid);
                        if (SmartQQ.GroupList[gid].GroupManage.enableXHJ.Equals("false"))
                            DisableFlag = true;
                    }
                    if (!DisableFlag)
                    {
                        string XiaoHuangJiMsg = GetInfo.GetTuLin(message, QQNum);
                        if (XiaoHuangJiMsg.Length > 1)
                        {
                            for (int i = 0; i < Badwords.Length; i++)
                                if (XiaoHuangJiMsg.Contains(Badwords[i]))
                                    return null;
                            MessageToSend[0] = "隔壁图灵机器人说：" + XiaoHuangJiMsg;

                        }
                        return MessageToSend;
                    }
                }
                if (!MessageToSend[0].Equals(""))
                {
                    string url = DicServer + "log.php";
                    string postdata = "password=" + HttpUtility.UrlEncode(DicPassword) + "&qqnum=" + HttpUtility.UrlEncode(QQNum) + "&qunnum=" + HttpUtility.UrlEncode(qunnum) + "&action=talk&p1=" + HttpUtility.UrlEncode(message) + "&p2=NULL&p3=NULL&p4=NULL";
                    HTTP.Post(url, postdata);
                }
                return MessageToSend;
            }
            return MessageToSend;
        }


        private static string GroupManage(string message, string uin, string gid, string gno)
        {
            string adminuin = "";
            string MessageToSend = "";
            if (message.StartsWith("群管理"))
            {
                SmartQQ.Info_GroupInfo(gid);
                if (!gid.Equals(""))
                {
                    adminuin = "";
                    if (SmartQQ.GroupList.ContainsKey(gid))
                        adminuin = SmartQQ.GroupList[gid].owner;
                }

                bool GroupManageFlag = true;
                string[] tmp = message.Split('&');
                tmp[1] = tmp[1].Replace("\r", "").Replace("\n", "").Replace(" ", "");
                if ((!tmp[0].Equals("群管理")) || tmp.Length != 2)
                {
                    GroupManageFlag = false;
                }
                if (GroupManageFlag)
                {
                    bool HaveRight = false;
                    if (uin.Equals(adminuin) || SmartQQ.Info_RealQQ(uin).Equals(MasterQQ))
                        HaveRight = true;
                    else if (SmartQQ.GroupList[gid].MemberList[uin].isManager)
                        HaveRight = true;
                    else HaveRight = false;
                    if (SmartQQ.GroupList[gid].GroupManage.enable == null)
                    {
                        GetGroupSetting(gid);
                    }
                    if (tmp.Length != 2 || tmp[1] == null)
                        return "";
                    if ((HaveRight || SmartQQ.GroupList[gid].GroupManage.enable.Equals("true")) && (tmp[1].Equals("查询状态") || tmp[1].Equals("状态")))
                    {
                        MessageToSend = "机器人启动：" + SmartQQ.GroupList[gid].GroupManage.enable + Environment.NewLine;
                        MessageToSend += "汇率查询启动：" + SmartQQ.GroupList[gid].GroupManage.enableExchangeRate + Environment.NewLine;
                        MessageToSend += "百科查询启动：" + SmartQQ.GroupList[gid].GroupManage.enableWiki + Environment.NewLine;
                        MessageToSend += "天气查询启动：" + SmartQQ.GroupList[gid].GroupManage.enableWeather + Environment.NewLine;
                        MessageToSend += "城市信息查询启动：" + SmartQQ.GroupList[gid].GroupManage.enableCityInfo + Environment.NewLine;
                        MessageToSend += "学习启动：" + SmartQQ.GroupList[gid].GroupManage.enableStudy + Environment.NewLine;
                        MessageToSend += "行情查询启动：" + SmartQQ.GroupList[gid].GroupManage.enableStock + Environment.NewLine;
                        MessageToSend += "翻译启动：" + SmartQQ.GroupList[gid].GroupManage.enableTranslate + Environment.NewLine;
                        MessageToSend += "闲聊启动：" + SmartQQ.GroupList[gid].GroupManage.enableTalk + Environment.NewLine;
                        MessageToSend += "表情启动：" + SmartQQ.GroupList[gid].GroupManage.enableEmoje + Environment.NewLine;
                        MessageToSend += "小黄鸡启动：" + SmartQQ.GroupList[gid].GroupManage.enableXHJ;
                        return MessageToSend;
                    }
                    if (HaveRight == false)
                    {
                        MessageToSend = "账号" + SmartQQ.Info_RealQQ(uin) + "不是群管理，无权进行此操作";
                        return MessageToSend;
                    }
                    else
                    {
                        tmp[1] = tmp[1].Replace("开启", "启动");
                        tmp[1] = tmp[1].Replace("开起", "启动");
                        if (tmp[1].Equals("启动机器人"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enable.Equals("true"))
                            {
                                MessageToSend = "当前机器人已启动";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enable", "true");

                                MessageToSend = "机器人启动成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("关闭机器人"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enable.Equals("false"))
                            {
                                MessageToSend = "当前机器人已关闭";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enable", "false");

                                MessageToSend = "机器人关闭成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("启动城市信息查询"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableCityInfo.Equals("true"))
                            {
                                MessageToSend = "当前城市信息查询已启动";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enableCityInfo", "true");

                                MessageToSend = "城市信息查询启动成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("关闭城市信息查询"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableCityInfo.Equals("false"))
                            {
                                MessageToSend = "当前城市信息查询已关闭";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enableCityInfo", "false");

                                MessageToSend = "城市信息查询关闭成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("启动天气查询"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableWeather.Equals("true"))
                            {
                                MessageToSend = "当前天气查询已启动";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enableWeather", "true");

                                MessageToSend = "天气查询启动成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("关闭天气查询"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableWeather.Equals("false"))
                            {
                                MessageToSend = "当前天气查询已关闭";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enableWeather", "false");

                                MessageToSend = "天气查询关闭成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("启动百科查询"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableWiki.Equals("true"))
                            {
                                MessageToSend = "当前百科查询已启动";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enableWiki", "true");

                                MessageToSend = "百科查询启动成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("关闭百科查询"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableWiki.Equals("false"))
                            {
                                MessageToSend = "当前百科查询已关闭";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enableWiki", "false");

                                MessageToSend = "百科查询关闭成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("启动汇率查询"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableExchangeRate.Equals("true"))
                            {
                                MessageToSend = "当前汇率查询已启动";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enableExchangeRate", "true");

                                MessageToSend = "汇率查询启动成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("关闭汇率查询"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableExchangeRate.Equals("false"))
                            {
                                MessageToSend = "当前汇率查询已关闭";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enableExchangeRate", "false");

                                MessageToSend = "汇率查询关闭成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("启动行情查询"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableStock.Equals("true"))
                            {
                                MessageToSend = "当前行情查询已启动";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enableStock", "true");

                                MessageToSend = "行情查询启动成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("关闭行情查询"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableStock.Equals("false"))
                            {
                                MessageToSend = "当前行情查询已关闭";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enableStock", "false");

                                MessageToSend = "行情查询关闭成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("启动聊天") || tmp[1].Equals("启动闲聊"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableTalk.Equals("true"))
                            {
                                MessageToSend = "当前聊天已启动";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enabletalk", "true");

                                MessageToSend = "聊天启动成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("关闭聊天") || tmp[1].Equals("关闭闲聊"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableTalk.Equals("false"))
                            {
                                MessageToSend = "当前聊天已关闭";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enabletalk", "false");

                                MessageToSend = "聊天关闭成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("启动学习"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableStudy.Equals("true"))
                            {
                                MessageToSend = "当前学习已启动";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enableStudy", "true");

                                MessageToSend = "学习启动成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("关闭学习"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableStudy.Equals("false"))
                            {
                                MessageToSend = "当前学习已关闭";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enableStudy", "false");

                                MessageToSend = "学习关闭成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("启动小黄鸡"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableXHJ.Equals("true"))
                            {
                                MessageToSend = "当前小黄鸡已启动";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enablexhj", "true");

                                MessageToSend = "小黄鸡启动成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("关闭小黄鸡"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableXHJ.Equals("false"))
                            {
                                MessageToSend = "当前小黄鸡已关闭";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enablexhj", "false");

                                MessageToSend = "小黄鸡关闭成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("启动表情"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableEmoje.Equals("true"))
                            {
                                MessageToSend = "当前表情已启动";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enableEmoje", "true");

                                MessageToSend = "表情启动成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("关闭表情"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableEmoje.Equals("false"))
                            {
                                MessageToSend = "当前表情已关闭";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enableEmoje", "false");

                                MessageToSend = "表情关闭成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("启动翻译"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableTranslate.Equals("true"))
                            {
                                MessageToSend = "当前翻译已启动";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enableTranslate", "true");

                                MessageToSend = "翻译启动成功";
                                return MessageToSend;
                            }
                        }
                        else if (tmp[1].Equals("关闭翻译"))
                        {
                            if (SmartQQ.GroupList[gid].GroupManage.enableTranslate.Equals("false"))
                            {
                                MessageToSend = "当前翻译已关闭";
                                return MessageToSend;
                            }
                            else
                            {
                                SetGroupSetting(gid, "enableTranslate", "false");

                                MessageToSend = "翻译关闭成功";
                                return MessageToSend;
                            }
                        }
                        else
                        {
                            MessageToSend = "没有这条指令。";
                            return MessageToSend;
                        }
                    }
                }
            }
            return MessageToSend;
        }

        //收到私聊和讨论组消息时调用的回复函数
        public static void AnswerMessage(string uin, string message, int type = 0)
        {
            string[] MessageToSendArray = Answer(message, uin);
            string MessageToSend = "";
            for (int i = 0; i < 10; i++)
            {
                if (MessageToSendArray[i] != null && !MessageToSendArray[i].Equals(""))
                {
                    if (!MessageToSend.Equals(""))
                        MessageToSend += Environment.NewLine;
                    MessageToSend += MessageToSendArray[i];
                    MessageToSendArray[i] = "";
                }
            }
            if (!MessageToSend.Equals(""))
                MainWindow.Message_Send(type, uin, MessageToSend);
            else if (type == 0)
            {
                string SenderName = "";
                string Gender = "";
                if (!Date.FriendList.ContainsKey(uin))
                    MainWindow.Info_FriendList();
                if (Date.FriendList.ContainsKey(uin))
                {
                    SenderName = Date.FriendList[uin].nick;
                    Gender = Date.FriendList[uin].gender;
                }
                if (Gender == "female")
                    Gender = "切嗣 ";
                else if (Gender == "male")
                    Gender = "士郎 ";
                MainWindow.Message_Send(0, uin, SenderName + Gender + "什么意思？请告诉我怎么做" + Environment.NewLine + "格式 学习&问题&Saber的回复");
            }
        }
        //向服务器提交AI学习请求
        public static string AIStudy(string source, string aim, string QQNum, string QunNum = "", bool superstudy = false)
        {
            MainWindow.listBoxLog.Items.Insert(0, "学习 " + source + " " + aim);
            string url;
            if (NoDicPassword)
                url = DicServer + "AddTalkRequest.php";
            else
                url = DicServer + "addtalk.php";
            string postdata = "password=" + HttpUtility.UrlEncode(DicPassword) + "&source=" + HttpUtility.UrlEncode(source) + "&aim=" + HttpUtility.UrlEncode(aim) + "&qqnum=" + HttpUtility.UrlEncode(QQNum) + "&qunnum=" + HttpUtility.UrlEncode(QunNum);
            if (superstudy)
                postdata = postdata + "&superstudy=true";
            else postdata = postdata + "&superstudy=false";

            string MsgGet = HttpClient.Post(url, postdata);
            return MsgGet;
        }
    }
}
