using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using HtmlAgilityPack;

namespace MangaCrawlerLib
{
    internal static class HTTPUtils
    {
        internal static string UserAgent = "Mozilla/5.0 (Windows NT 6.0; WOW64; rv:10.0) Gecko/20100101 Firefox/10.0";

        internal static HtmlDocument Submit(string a_url, Dictionary<string, string> a_parameters)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(a_url);
            request.Method = "POST";
            request.UserAgent = UserAgent;
                
            string parameters = "";
            foreach (KeyValuePair<string, string> _Parameter in a_parameters)
                parameters = parameters + (parameters != "" ? "&" : "") + string.Format("{0}={1}", _Parameter.Key, _Parameter.Value);

            byte[] byteArray = Encoding.UTF8.GetBytes(parameters);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string html = reader.ReadToEnd();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                return doc;
            }
        }
    }
}
