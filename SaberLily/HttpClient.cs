﻿using System;
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
    }
}