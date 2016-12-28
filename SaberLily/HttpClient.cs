using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
//Http工具类
namespace SaberLily
{
    public class HttpClient
    {
        //网络通信相关
        public static CookieContainer cookies = new CookieContainer();
        static CookieCollection CookieCollection = new CookieCollection();
        static CookieContainer CookieContainer = new CookieContainer();

        public static string Get(string url, string referer = "http://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2", int timeout = 100000, Encoding encode = null, bool NoProxy = false)
        {
            string dat;
            HttpWebResponse res = null;
            HttpWebRequest req;
            try
            {
                req = (HttpWebRequest)WebRequest.Create(url);
                req.CookieContainer = cookies;
                req.AllowAutoRedirect = false;
                req.Timeout = timeout;
                req.Referer = referer;
                if (NoProxy)
                    req.Proxy = null;
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0;%20WOW64; rv:47.0) Gecko/20100101 Firefox/47.0";
                res = (HttpWebResponse)req.GetResponse();

                cookies.Add(res.Cookies);
            }
            catch (Exception)
            {
                return "";
            }
            StreamReader reader;

            reader = new StreamReader(res.GetResponseStream(), encode == null ? Encoding.UTF8 : encode);
            dat = reader.ReadToEnd();

            res.Close();
            req.Abort();
            return dat;
        }

        internal static string Post(string url, string data, string Referer = "http://d1.web2.qq.com/proxy.html?v=20151105001&callback=1&id=2", int timeout = 100000, Encoding encode = null)
        {
            string dat = "";
            HttpWebRequest req;
            try
            {
                req = WebRequest.Create(url) as HttpWebRequest;
                req.CookieContainer = cookies;
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";
                req.Proxy = null;
                req.Timeout = timeout;
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0;%20WOW64; rv:47.0) Gecko/20100101 Firefox/47.0";
                req.ProtocolVersion = HttpVersion.Version10;
                req.Referer = Referer;

                byte[] mybyte = Encoding.Default.GetBytes(data);
                req.ContentLength = mybyte.Length;

                Stream stream = req.GetRequestStream();
                stream.Write(mybyte, 0, mybyte.Length);


                HttpWebResponse res = req.GetResponse() as HttpWebResponse;

                cookies.Add(res.Cookies);
                stream.Close();

                StreamReader SR = new StreamReader(res.GetResponseStream(), encode == null ? Encoding.UTF8 : encode);
                dat = SR.ReadToEnd();
                res.Close();
                req.Abort();
            }
            catch (Exception)
            {
                return "";
            }
            return dat;
        }
    }
}
