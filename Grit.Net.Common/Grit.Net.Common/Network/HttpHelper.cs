﻿using Grit.Net.Common.Log;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Grit.Net.Common.Network
{
    public class HttpHelper
    {
        public static bool PostWebRequest(string postUrl, string paramData, out string ret)
        {
            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(paramData); //转化
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                webReq.Method = "POST";
                webReq.ContentType = "application/x-www-form-urlencoded";

                webReq.ContentLength = byteArray.Length;
                Stream newStream = webReq.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);//写入参数

                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                ret = sr.ReadToEnd();
                sr.Close();
                sr.Dispose();
                response.Close();
                response.Dispose();
                newStream.Close();
                newStream.Dispose();
                return true;
            }
            catch (WebException ex)
            {
                LogManager.Write(ex.Message);
                ret = ex.Message;
            }
            catch (IOException ex)
            {
                LogManager.Write(ex.Message);
                ret = ex.Message;
            }
            catch (Exception ex)
            {
                LogManager.Write(ex.Message);
                ret = ex.Message;
            }
            return false;
        }

        /// <summary>
        /// 模拟Post表单上传文件
        /// </summary>
        /// <param name="url">上传地址</param>
        /// <param name="timeOut">超时时间，单位(S)</param>
        /// <param name="fileKeyName">表单名称，参数名</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="stringDict">参数</param>
        /// <returns></returns>
        public static bool HttpPostData(string url, int timeOut, string fileKeyName,
                            string filePath, NameValueCollection stringDict, out string ret)
        {
            MemoryStream memStream = new MemoryStream();
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                // 边界符
                string boundary = "---------------" + DateTime.Now.Ticks.ToString("x");
                // 边界符
                byte[] beginBoundary = Encoding.ASCII.GetBytes("--" + boundary + "\r\n");
                FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                // 最后的结束符
                byte[] endBoundary = Encoding.ASCII.GetBytes("--" + boundary + "--\r\n");

                // 设置属性
                webRequest.Method = "POST";
                webRequest.Timeout = timeOut * 1000;
                webRequest.ContentType = "multipart/form-data; boundary=" + boundary;

                // 写入文件
                const string filePartHeader =
                    "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
                     "Content-Type: application/octet-stream\r\n\r\n";
                string header = string.Format(filePartHeader, fileKeyName, filePath);
                byte[] headerbytes = Encoding.UTF8.GetBytes(header);

                memStream.Write(beginBoundary, 0, beginBoundary.Length);
                memStream.Write(headerbytes, 0, headerbytes.Length);

                byte[] buffer = new byte[1024];
                int bytesRead; // =0

                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    memStream.Write(buffer, 0, bytesRead);
                }

                // 写入字符串的Key
                string stringKeyHeader = "\r\n--" + boundary +
                                       "\r\nContent-Disposition: form-data; name=\"{0}\"" +
                                       "\r\n\r\n{1}\r\n";

                foreach (byte[] formitembytes in from string key in stringDict.Keys
                                                 select string.Format(stringKeyHeader, key, stringDict[key])
                                                     into formitem
                                                 select Encoding.UTF8.GetBytes(formitem))
                {
                    memStream.Write(formitembytes, 0, formitembytes.Length);
                }

                // 写入最后的结束边界符
                memStream.Write(endBoundary, 0, endBoundary.Length);

                webRequest.ContentLength = memStream.Length;

                Stream requestStream = webRequest.GetRequestStream();

                memStream.Position = 0;
                byte[] tempBuffer = new byte[memStream.Length];
                memStream.Read(tempBuffer, 0, tempBuffer.Length);
                memStream.Close();

                requestStream.Write(tempBuffer, 0, tempBuffer.Length);
                requestStream.Close();

                WebResponse httpWebResponse = (HttpWebResponse)webRequest.GetResponse();

                using (var httpStreamReader = new StreamReader(httpWebResponse.GetResponseStream(),
                                                                Encoding.GetEncoding("utf-8")))
                {
                    ret = httpStreamReader.ReadToEnd();
                }

                fileStream.Close();
                httpWebResponse.Close();
                webRequest.Abort();
            }
            catch (WebException ex)
            {
                LogManager.Write(ex.Message);
                ret = ex.Message;
            }
            catch (IOException ex)
            {
                LogManager.Write(ex.Message);
                ret = ex.Message;
            }
            catch (Exception ex)
            {
                LogManager.Write(ex.Message);
                ret = ex.Message;
            }
            return false;
        }
    }
}
