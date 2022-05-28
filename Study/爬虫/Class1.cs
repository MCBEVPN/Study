using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Json;

namespace Study.爬虫
{
    internal class Json<T>
    {
        private Json() { }
        /// <summary>
        /// Json写入。
        /// </summary>
        public static string WriteJson(T obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream mstream = new MemoryStream();
            serializer.WriteObject(mstream, obj);
            byte[] Bytes = new byte[mstream.Length];
            mstream.Position = 0;
            mstream.Read(Bytes, 0, (int)mstream.Length);
            mstream.Close();
            mstream.Dispose();
            return Encoding.UTF8.GetString(Bytes);
        }
        /// <summary>
        /// Json读取。
        /// </summary>
        public static T ReadJson(string data)
        {
            MemoryStream mstream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            return (T)serializer.ReadObject(mstream);
        }
    }
    /// <summary>
    /// 网络文件
    /// </summary>
    internal static class WebFile
    {
        /// <summary>
        /// 获取网页中的流
        /// </summary>
        internal static Stream GetStream(string uri, int timeOut = 0, string accept = "*/*", string cookies = "")
        {
            GC.Collect();
            // 声明请求对象，由于new HttpWebRequest()代码被弃用，只能从WebRequest创建请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            // 设置请求标头
            if (timeOut != 0)
            {
                request.Timeout = timeOut;
            }
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.4985.0 Safari/537.36 Edg/102.0.1235.0";
            request.Accept = accept;
            request.KeepAlive = true;
            request.AllowAutoRedirect = true;
            request.Method = "GET";
            if (string.IsNullOrEmpty(cookies)) request.Headers.Add("Cookie", cookies);
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            HttpWebResponse response = null;
            try
            {
                // 声明响应对象
                response = (HttpWebResponse)request.GetResponse();
            }
            catch { }
            // 如果响应报错或者空对象，则返回空对象
            if (response == null)
            {
                return null;
            }
            // 检测状态码是否为OK，是则继续执行，否则返回空对象
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }
            Stream stream = response.GetResponseStream();
            // 返回响应流内容
            return stream;
        }
        /// <summary>
        /// 获取网页内容
        /// </summary>
        internal static string GetHtmlString(Stream stream)
        {
            string vaule = "";
            try
            {
                StreamReader streamReader = new StreamReader(stream);
                vaule = streamReader.ReadToEnd();
                streamReader.Close();
                streamReader.Dispose();
            }
            catch { }
            return vaule;
        }
        /// <summary>
        /// 下载文件
        /// </summary>
        internal static void DownloadFile(string uri, string path)
        {
            // 声明请求对象，由于new HttpWebRequest()代码被弃用，只能从WebRequest创建请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            // 设置请求方法
            request.Method = "GET";
            // 声明响应对象
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // 检测状态码是否为OK，是则继续执行，否则返回
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return;
            }
            // 声明文件流，并指定一个目录创建文件，如果文件已存在则覆盖
            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                // 将响应流复制到fileStream
                response.GetResponseStream().CopyTo(fileStream);
            };
        }
        internal static string DecodeGZipString(Stream stream)
        {
            string vaule = "";
            try
            {
                StreamReader streamReader = new StreamReader(new GZipStream(stream, CompressionMode.Decompress));
                vaule = streamReader.ReadToEnd();
                streamReader.Close();
                streamReader.Dispose();
            }
            catch
            {
                try
                {
                    StreamReader streamReader = new StreamReader(stream);
                    vaule = streamReader.ReadToEnd();
                    streamReader.Close();
                    streamReader.Dispose();
                }
                catch { }
            }
            return vaule;
        }
    }
    internal class WebFileInfo
    {
        public WebFileInfo()
        {
            timer = new System.Windows.Forms.Timer();
        }
        private int lastLength = 0;
        private int nowLength = 0;
        private Thread thread;
        private System.Windows.Forms.Timer timer;
        public void DownloadFile(string uri, string fileName)
        {
            if (string.IsNullOrEmpty(uri))
            {
                IsCompleted = true;
                if (DownloadCompleted != null)
                {
                    DownloadCompleted();
                }
                return;
            }
            thread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    HttpWebRequest webRequest = WebRequest.CreateHttp(uri);
                    webRequest.KeepAlive = true;
                    webRequest.Method = "GET";
                    webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.4985.0 Safari/537.36 Edg/102.0.1235.0";
                    HttpWebResponse httpWebResponse = (HttpWebResponse)webRequest.GetResponse();
                    long totalBytes = httpWebResponse.ContentLength;
                    MaxValue = (int)totalBytes;
                    string newFileName = "";
                    foreach (var item in httpWebResponse.Headers.AllKeys)
                    {
                        foreach (var item2 in httpWebResponse.Headers.GetValues(item))
                        {
                            if (item2.Contains(".exe"))
                            {
                                newFileName = item2.Substring(item2.LastIndexOf('=') + 1);
                                break;
                            }
                        }
                    }
                    Stream stream = httpWebResponse.GetResponseStream();
                    FileStream fileStream = null;
                    StringBuilder stringBuilder = new StringBuilder();
                    if (!string.IsNullOrEmpty(newFileName))
                    {
                        stringBuilder.Append(newFileName);
                    }
                    else
                    {
                        foreach (var item in Path.GetFileName(fileName))
                        {
                            bool b = true;
                            foreach (var item2 in Path.GetInvalidFileNameChars())
                            {
                                if (item == item2)
                                {
                                    b = false;
                                    continue;
                                }
                            }
                            if (b)
                            {
                                stringBuilder.Append(item);
                            }
                        }
                    }
                    FileName = Program.DownloadPath + stringBuilder.ToString().Substring(stringBuilder.ToString().LastIndexOf('=') + 1);
                    try
                    {
                        fileStream = new FileStream(FileName, FileMode.CreateNew);
                    }
                    catch
                    {
                        for (int i = 1; ; i++)
                        {
                            try
                            {
                                FileName = Program.DownloadPath + Path.GetFileNameWithoutExtension(stringBuilder.ToString()) + $" ({i}){Path.GetExtension(FileName)}";
                                fileStream = new FileStream(FileName, FileMode.CreateNew);
                                break;
                            }
                            catch { }
                        }
                    }
                    long totalDownloadedByte = 0;
                    byte[] by = new byte[1024 * 1024];
                    int osize = stream.Read(by, 0, by.Length);

                    timer.Interval = 300;
                    timer.Tick += (object sender, EventArgs e) =>
                    {
                        SpeedText = $"{(nowLength - lastLength) / 1024 * 2.2} KB/S";
                        lastLength = nowLength;
                    };
                    timer.Start();
                    try
                    {
                        while (osize > 0)
                        {
                            if (CanDownload)
                            {
                                try
                                {
                                    Refesh();
                                    totalDownloadedByte += osize;
                                    nowLength = (int)totalDownloadedByte;
                                    fileStream.Write(by, 0, osize);
                                    osize = stream.Read(by, 0, by.Length);
                                    Value = (int)((double)totalDownloadedByte / (double)totalBytes * 100);
                                }
                                catch { }
                            }
                        }
                    }
                    catch
                    {
                        fileStream.Close();
                        stream.Close();
                        fileStream.Dispose();
                        stream.Dispose();
                        IsCompleted = true;
                        webRequest.Abort();
                        if (File.Exists(FileName))
                        {
                            File.Delete(FileName);
                        }
                    }
                    finally
                    {
                        fileStream.Close();
                        stream.Close();
                        fileStream.Dispose();
                        stream.Dispose();
                        IsCompleted = true;
                        webRequest.Abort();
                    }
                }
                catch
                {
                    IsCompleted = true;
                    DownloadCompleted();
                }
                IsCompleted = true;
                DownloadCompleted();
            }));
            thread.Start();
        }
        public void Refesh()
        {
            Application.DoEvents();
            Downloading?.Invoke();
        }
        public void ExitDownload()
        {
            if (thread != null)
            {
                thread.Abort();
            }
        }
        public void StopDownload()
        {
            timer.Stop();
            CanDownload = false;
        }
        public void StartDownload()
        {
            Application.DoEvents();
            timer.Start();
            CanDownload = true;
        }
        public bool CanDownload = true;
        public bool IsCompleted = false;
        public int MaxValue = 0;
        public int Value = 0;
        public string FileName = "";
        public string Text = "";
        public string SpeedText = "0 KB/S";
        public delegate void DownloadingHandle();
        public event DownloadingHandle Downloading;
        public delegate void DownloadCompletedHandle();
        public event DownloadCompletedHandle DownloadCompleted;
    }
}