﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace MangaCrawlerLib
{
    internal static class HTTPUtils
    {
        internal static HtmlAgilityPack.HtmlDocument Submit(string a_url, Dictionary<string, string> a_parameters)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(a_url);
            request.Method = "POST";

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
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                return doc;
            }
        }
    }
}
