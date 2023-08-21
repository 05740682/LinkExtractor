using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ALLClass
{
    internal class Download
    {
        public static WebHeaderCollection RequestHeaders = new WebHeaderCollection()
        {
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.9.9999.99 Safari/537.36 Edg/99.9.9999.99" },
            { "Accept-Language", "zh-CN,zh;q=0.9" },
        };

        internal static async Task<string> UploadData(string url, string data, int uploadcode = 0, string Referer = "")
        {
            using (WebClient WebClient1 = new WebClient { Proxy = null, Encoding = Encoding.UTF8 })
            {
                WebClient1.Headers = RequestHeaders;
                if (uploadcode == 1) { WebClient1.Headers[HttpRequestHeader.Referer] = Referer; WebClient1.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded"; }
                else if (uploadcode == 2) { WebClient1.Headers[HttpRequestHeader.Referer] = Referer; WebClient1.Headers[HttpRequestHeader.ContentType] = "application/json"; };
                return Encoding.UTF8.GetString(await WebClient1.UploadDataTaskAsync(url, Encoding.UTF8.GetBytes(data)));
            }
        }

        internal static async Task<string> DownloadString(string url)
        {
            using (WebClient WebClient1 = new WebClient { Proxy = null, Encoding = Encoding.UTF8 })
            {
                WebClient1.Headers = RequestHeaders;
                return await WebClient1.DownloadStringTaskAsync(url);
            }
        }

        internal delegate void ProgressChangedDelegate(int progress, long bytesReceived, long totalBytesToReceive);
        internal static async Task<bool> DownloadFile(string url, string filePath, ProgressChangedDelegate progressChanged)
        {
            bool result = false;
            using (WebClient WebClient1 = new WebClient { Proxy = null , Encoding = Encoding.UTF8 })
            {
                WebClient1.Headers = RequestHeaders;
                int progress = 0;
                long bytesReceived = 0;
                long totalBytesToReceive = 0;
                var timer = new System.Timers.Timer(1000);
                timer.Elapsed += (s, e) =>
                {
                    progressChanged?.Invoke(progress, bytesReceived, totalBytesToReceive);
                };
                timer.Start();
                WebClient1.DownloadProgressChanged += (s, e) =>
                {
                    progress = e.ProgressPercentage;
                    bytesReceived = e.BytesReceived;
                    totalBytesToReceive = e.TotalBytesToReceive;
                };
                WebClient1.DownloadFileCompleted += (s, e) =>
                {
                    timer.Stop();
                    if (e.Error == null) { result = true; progressChanged?.Invoke(100, totalBytesToReceive, totalBytesToReceive); }
                };
                await WebClient1.DownloadFileTaskAsync(new Uri(url), filePath);
            }
            return result;
        }

        private static readonly string[] SizeSuffixes = { " B", " KB", " MB", " GB", " TB", " PB", " EB" };

        /// <summary>
        /// 将字节数转换为带单位的字符串。
        /// </summary>
        /// <param name="byteCount">要转换的字节数。</param>
        /// <returns>带单位的字符串。</returns>
        internal static string FormatByteSize(long byteCount)
        {
            // 如果字节数为 0，则直接返回 "0 B"
            if (byteCount == 0) { return $"0{SizeSuffixes[0]}"; }

            // 计算绝对值
            long absoluteByteCount = Math.Abs(byteCount);

            // 计算合适的单位
            int sizeSuffixIndex = Convert.ToInt32(Math.Floor(Math.Log(absoluteByteCount) / Math.Log(1024)));

            // 将字节数转换为对应单位下的数值
            double sizeValue = Math.Round(absoluteByteCount / Math.Pow(1024, sizeSuffixIndex), 2);

            // 创建一个 NumberFormatInfo 对象
            NumberFormatInfo formatInfo = new NumberFormatInfo();

            // 判断 sizeValue 变量是否为整数
            bool isInteger = sizeValue == (int)sizeValue;

            // 设置小数位数
            formatInfo.NumberDecimalDigits = isInteger ? 0 : 2;

            // 返回转换后的数值和单位
            return $"{Math.Sign(byteCount) * sizeValue:0.##}{SizeSuffixes[sizeSuffixIndex]}";
        }

    }
}
