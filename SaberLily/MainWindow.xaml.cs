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
        private static System.Timers.Timer Login_QRStatuTimer = new System.Timers.Timer();
        private static Random rand = new Random();



        public MainWindow()
        {
            InitializeComponent();
        }
        //点击获取二维码
        private void login_button_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        public void Login()
        {
            Login_GetQRCode();
            login_button.IsEnabled = false;
            Login_QRStatuTimer.AutoReset = true;
            Login_QRStatuTimer.Elapsed += Login_QRStatuTimer_Elapsed;
            Login_QRStatuTimer.Interval = 5000;
            Login_QRStatuTimer.Start();
        }

        //获取登录二维码
        public bool Login_GetQRCode()
        {
            login_button.IsEnabled = true;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://ssl.ptlogin2.qq.com/ptqrshow?appid=501004106&e=0&l=M&s=5&d=72&v=4&t=#{t}".Replace("#{t}", rand.NextDouble().ToString()));
                req.CookieContainer = HttpClient.cookies;
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                login_image.Source = BitmapFrame.Create(res.GetResponseStream(), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                Console.WriteLine(login_image.Source);
            }
            catch (Exception) { return false; }
            return true;
        }
        //每秒检查一遍二维码
        private void Login_QRStatuTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Login_GetQRStatu();
        }
        //检查二维码状态
        private void Login_GetQRStatu()
        {
            string dat;
            string url = "https://ssl.ptlogin2.qq.com/ptqrlogin?webqq_type=10&remember_uin=1&login2qq=1&aid=501004106 &u1=http%3A%2F%2Fw.qq.com%2Fproxy.html%3Flogin2qq%3D1%26webqq_type%3D10 &ptredirect=0&ptlang=2052&daid=164&from_ui=1&pttype=1&dumy=&fp=loginerroralert &action=0-0-157510&mibao_css=m_webqq&t=1&g=1&js_type=0&js_ver=10143&login_sig=&pt_randsalt=0";
            string referer = "https://ui.ptlogin2.qq.com/cgi-bin/login?daid=164&target=self&style=16&mibao_css=m_webqq&appid=501004106&enable_qlogin=0&no_verifyimg=1 &s_url=http%3A%2F%2Fw.qq.com%2Fproxy.html&f_url=loginerroralert &strong_login=1&login_state=10&t=20131024001";
            dat = HttpClient.Get(url, referer);
            string[] temp = dat.Split('\'');
            Console.WriteLine(temp[1]);
            switch (temp[1])
            {
                case ("65"):                                            //二维码失效
                    login_label.Content = "当前登录状态：二维码失效，请稍后";
                    Login_GetQRCode();
                    break;
                case ("66"):                                            //等待扫描
                    login_label.Content = "当前登录状态：二维码有效，请扫描";
                    break;
                case ("67"):                                            //等待确认
                    login_label.Content = "当前登录状态：二维码已扫描，请确认";
                    break;
                case ("0"):                                             //已经确认
                    login_label.Content = "当前登录状态：确认成功，请稍候";
                    //Login_Process(temp[5]);
                    break;

                default: break;
            }

        }
    }
}
