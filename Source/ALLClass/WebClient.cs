using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ALLClass
{
    public class Web : IDisposable
    {
        public WebClient Client { get; private set; }
        public Web(string Referer = "", int is_upload = 0)
        {
            Client = new WebClient{ Proxy = null, Encoding = Encoding.UTF8, };
            Client.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.9.9999.99 Safari/537.36 Edg/99.9.9999.99";
            Client.Headers[HttpRequestHeader.AcceptLanguage] = "zh-CN,zh;q=0.9";
            if (is_upload == 1) { Client.Headers[HttpRequestHeader.Referer] = Referer; Client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded"; }
            else if (is_upload == 2) { Client.Headers[HttpRequestHeader.ContentType] = "application/json"; };
        }
        internal static async Task<string> DownloadString(string url)
        {
            using (Web Web = new Web()) { return await Web.Client.DownloadStringTaskAsync(url); }
        }
        internal static async Task<string> UploadData(string Referer, string address, byte[] postdata, int is_upload = 1)
        {
            using (Web Web = new Web(Referer, is_upload)) { return Encoding.UTF8.GetString(await Web.Client.UploadDataTaskAsync(address, "POST", postdata)); }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Client?.Dispose();
            }
        }
    }
}
