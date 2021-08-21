using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Net;
using System.IO;
using System;
using Newtonsoft.Json.Linq;

namespace eggcation
{
    public static class Utility
    {
        public static string displayName = String.Empty;
        public static string userId = String.Empty;
        public static string roomName = String.Empty;

        public static string request_server(JObject req, string method)
        {
            string url = "http://34.64.85.29:8080/";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url + method);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(req.ToString());
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string characterSet = httpResponse.CharacterSet;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream(), System.Text.Encoding.UTF8, true))
            {
                var result = streamReader.ReadToEnd();
                Debug.Log(result);
                return result;
            }
        }
    }
}
