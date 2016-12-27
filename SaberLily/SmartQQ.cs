using SaberLily;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SaberLily
{
    public class SmartQQ:MainWindow
    {
        private static System.Timers.Timer Login_QRStatuTimer = new System.Timers.Timer();
        private static Random rand = new Random();

        public void Login(Button login_button)
        {
            Console.WriteLine("进入登录按钮");
            Login_GetQRCode();
            login_button.IsEnabled = false;
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
                Console.WriteLine(res);
                //MainWindow.login_image.Source = BitmapFrame.Create(res.GetResponseStream(), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
            catch (Exception) { return false; }
            return true;
        }
    }
}
