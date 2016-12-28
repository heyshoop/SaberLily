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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SaberLily
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        private static DispatcherTimer timer;
        private static Random rand = new Random();


        public LoginWindow()
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
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Login_QRStatuTimer_Elapsed;
            timer.Start();
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
            }
            catch (Exception) { return false; }
            return true;
        }
        //每5秒检查一遍二维码
        private void Login_QRStatuTimer_Elapsed(object sender, EventArgs e)
        {
            string dat;
            string url = "https://ssl.ptlogin2.qq.com/ptqrlogin?webqq_type=10&remember_uin=1&login2qq=1&aid=501004106 &u1=http%3A%2F%2Fw.qq.com%2Fproxy.html%3Flogin2qq%3D1%26webqq_type%3D10 &ptredirect=0&ptlang=2052&daid=164&from_ui=1&pttype=1&dumy=&fp=loginerroralert &action=0-0-157510&mibao_css=m_webqq&t=1&g=1&js_type=0&js_ver=10143&login_sig=&pt_randsalt=0";
            string referer = "https://ui.ptlogin2.qq.com/cgi-bin/login?daid=164&target=self&style=16&mibao_css=m_webqq&appid=501004106&enable_qlogin=0&no_verifyimg=1 &s_url=http%3A%2F%2Fw.qq.com%2Fproxy.html&f_url=loginerroralert &strong_login=1&login_state=10&t=20131024001";
            dat = HttpClient.Get(url, referer);
            string[] temp = dat.Split('\'');
            Console.WriteLine(temp[1]);
            switch (temp[1])
            {
                case ("65"):
                    login_label.Content = "当前登录状态：二维码失效，请稍后";//二维码失效
                    Login_GetQRCode();
                    break;
                case ("66"):
                    login_label.Content = "当前登录状态：二维码有效，请扫描";//等待扫描
                    break;
                case ("67"):
                    login_label.Content = "当前登录状态：二维码已扫描，请确认";//等待确认
                    break;
                case ("0"):
                    login_label.Content = "当前登录状态：确认成功，请稍候";//已经确认
                    this.DialogResult = true;
                    timer.Stop();
                    break;

                default: break;
            }
        }
    }
}
