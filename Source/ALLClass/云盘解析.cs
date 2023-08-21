using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ALLClass
{
    internal class 云盘解析
    {
        /// <summary>
        /// 根据传入的链接调用对应的方法
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        internal static async Task<string> 关键字解析(string link)
        {
            // 通过正则表达式获取链接中的密码
            string password = Regex.Match(link, "(?<=&pwd=).*").Value;
            // 如果链接中包含 "mail.qq"
            if (link.Contains("mail.qq")) { return await QQ邮箱直链解析(link); }
            // 如果链接中包含 "lanzou"
            else if (link.Contains("lanzou"))
            {
                // 获取域名
                string domain = "https://" + new Uri(link).Host;
                // 如果链接以 "&folder" 结尾
                if (link.EndsWith("&folder")) { return await 蓝奏云文件夹解析(domain, link.Replace($"&pwd={password}", "").Replace("&folder", ""), password.Replace("&folder", "")); }
                // 否则，调用 蓝奏云直链解析 函数
                return await 蓝奏云直链解析(domain, link.Replace($"&pwd={password}", ""), password);
            }
            // 如果链接以 "http" 开头
            else if (link.StartsWith("http")) { return link; }
            // 否则，返回 "无法获取正确的链接对象..."
            return "无法获取正确的链接对象...";
        }
        /// <summary>
        /// 根据传入的链接获取重定向后的链接
        /// </summary>
        /// <param name="link">需要被重定向的链接</param>
        /// <returns></returns>
        internal static async Task<string> Get(string link)
        {
            // 创建 HttpWebRequest 对象
            HttpWebRequest request = WebRequest.Create(link) as HttpWebRequest;
            // 设置请求头
            request.Headers.Set("accept-language", "zh-CN,zh;q=0.9");
            // 禁止自动重定向
            request.AllowAutoRedirect = false;
            // 获取响应
            using (WebResponse response = await request.GetResponseAsync())
            {
                // 获取重定向链接
                string redirectLink = response.Headers.Get("Location");
                // 返回重定向链接
                return redirectLink;
            }
        }
        /// <summary>
        /// 获取查询字符串中指定名称的值
        /// </summary>
        /// <param name="url">查询字符串</param>
        /// <param name="name">名称</param>
        /// <returns></returns>
        public static string GetQueryString(string url, string name)
        {
            // 创建一个新的Uri对象
            var uri = new Uri(url);
            // 解析Uri对象的查询字符串
            var query = HttpUtility.ParseQueryString(uri.Query);
            // 返回查询字符串中与name参数匹配的值
            return query[name];
        }
        /// <summary>
        /// 传入蓝奏云页面匹配错误信息
        /// </summary>
        /// <param name="msg">蓝奏云的页面</param>
        /// <param name="m"></param>
        /// <returns></returns>
        private static string Msg(string msg, bool m = false)
        {
            Match match = Regex.Match(msg, "(?<=<div class=\"off\"><div class=\"off0\"><div class=\"off1\"></div></div>).*(?=</div>)");
            if (m) { return match.Success.ToString(); }
            return match.Value;
        }
        /// <summary>
        /// 使用正则匹配长度大于70的指定文本
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private static string RegexAll(string page, string pattern)
        {
            foreach (Match m in Regex.Matches(page, pattern)) { if (m.Length > 70) { return m.Value; } }
            throw new Exception("使用正则匹配特定长度文本失败...");
        }
        /// <summary>
        /// 蓝奏云直链解析获取sign
        /// </summary>
        /// <param name="page">蓝奏云提交Json页面</param>
        /// <returns></returns>
        private static string GetSign(string page)
        {
            // 移除 JavaScript 中的单行注释
            string newpage = Regex.Replace(page, "\\/\\/[^\\n]*", "");
            // 匹配所有var开头的变量 返回长度大于70的值
            foreach (Match m in Regex.Matches(newpage, "var\\s+\\w+\\s*=\\s*'([^']+)'", RegexOptions.Singleline)) { if (m.Success && m.Groups[1].Value.Length > 70) { return m.Groups[1].Value; } }
            throw new Exception("获取Sign失败!");
        }
        /// <summary>
        /// 蓝奏云直链解析
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="Content">蓝奏云分享链接</param>
        /// <param name="password">蓝奏云分享密码</param>
        /// <returns></returns>
        private static async Task<string> 蓝奏云直链解析(string domain, string Content, string password = "")
        {
            // 初始化 result 变量
            string result = "蓝奏云直链解析失败...";
            // 下载 contentUrl 参数指定页面的内容
            string pageContent = await Download.DownloadString(Content);
            // 检查 Msg 方法的返回值
            if (Msg(pageContent, true) != "True")
            {
                string postData;
                // 根据是否提供了密码构造 postData 字符串
                if (string.IsNullOrEmpty(password)) { string sign = GetSign(await Download.DownloadString(domain + RegexAll(pageContent, "(?<=src=\")[^\"]*"))); postData = $"action=downprocess&sign={sign}&ves=1";}
                else { string sign = GetSign(pageContent); postData = $"action=downprocess&sign={sign}&ves=1&p={password}";   }
                if (!string.IsNullOrEmpty(postData))
                {
                    // 上传数据并获取结果
                    result = await Download.UploadData($"{domain}/ajaxm.php", postData, 1, Content);
                    // 反序列化结果
                    dynamic lanzouJson = Json.DeserializeObject(result);
                    // 检查 zt 属性的值
                    if ($"{lanzouJson["zt"]}" == "1") { result = await Get($"{lanzouJson["dom"]}/file/{lanzouJson["url"]}"); }
                    else { result = $"错误：{lanzouJson["inf"]}"; }
                }
            }
            else { result = $"错误：{Msg(pageContent)}"; }
            // 返回 result 的值
            return result;
        }
        /// <summary>
        /// 蓝奏云文件夹解析
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="Content">蓝奏云文件夹分享链接</param>
        /// <param name="password">蓝奏云文件夹分享密码</param>
        /// <returns></returns>
        private static async Task<string> 蓝奏云文件夹解析(string domain, string Content, string password = "")
        {
            // 下载网页内容
            string result;
            string page = await Download.DownloadString(Content);
            // 创建一个集合来存储不需要的字符串
            HashSet<string> trash = new HashSet<string>();
            // 使用正则表达式匹配所有形如 `var variableName =` 的字符串，并将变量名添加到 `trash` 集合中
            foreach (Match m in Regex.Matches(page, "(?<=var )(.*)(?= =)")) { trash.Add(m.Value); }
            // 使用正则表达式匹配形如 `data : {...}` 的字符串，并将其存储在 `result` 变量中
            result = RegexAll(page.Replace("\n", "").Replace("\t", ""), "(?<=data : )[^}]*").Replace("\'", "\"");
            // 删除最后一个逗号并添加一个右花括号
            if (result.EndsWith(",")) { result = result.Substring(0, result.Length - 1); result += "}"; }
            // 在需要时添加密码字段
            if (password != "") { result = result.Substring(0, result.Length - 9); result += $"\"pwd\":\"{password}\"}}"; }
            // 替换掉 `pgs` 字段的值
            if (result.Contains("pgs")) { result = result.Replace("pgs", Regex.Match(page, "(?<=pgs =)(.*)(?=;)").Value); }
            // 遍历 `trash` 集合中的所有字符串
            foreach (string item in trash)
            {
                // 跳过 `pwd` 字段
                if (item.Contains("pwd")) { continue; }
                // 使用正则表达式替换掉其他不需要的字符串
                result = result.Replace(item, $"\"{Regex.Match(page, $"(?<={item} = ')(.*)(?=')").Value}\"");
            }
            // 将 `result` 变量中的 JSON 字符串转换为动态对象
            dynamic postjson = Json.DeserializeObject(result);
            string postdata;
            StringBuilder sb = new StringBuilder();
            // 如果 `password` 为空
            if (password == "")
            {
                // 添加各个字段的值
                sb.Append($"lx={postjson["lx"]}&");
                sb.Append($"fid={postjson["fid"]}&");
                sb.Append($"uid={postjson["uid"]}&");
                sb.Append($"pg={postjson["pg"]}&");
                sb.Append($"rep={postjson["rep"]}&");
                sb.Append($"t={postjson["t"]}&");
                sb.Append($"k={postjson["k"]}&");
                sb.Append($"up={postjson["up"]}&");
                sb.Append($"vip={postjson["vip"]}&");
                sb.Append($"webfoldersign={postjson["webfoldersign"]}");
            }
            else // 如果 `password` 不为空
            {
                // 添加各个字段的值
                sb.Append($"lx={postjson["lx"]}&");
                sb.Append($"fid={postjson["fid"]}&");
                sb.Append($"uid={postjson["uid"]}&");
                sb.Append($"pg={postjson["pg"]}&");
                sb.Append($"rep={postjson["rep"]}&");
                sb.Append($"t={postjson["t"]}&");
                sb.Append($"k={postjson["k"]}&");
                sb.Append($"up={postjson["up"]}&");
                sb.Append($"ls={postjson["ls"]}&");
                sb.Append($"pwd={postjson["pwd"]}");
            }
            // 将 `StringBuilder` 对象转换为字符串
            postdata = sb.ToString();
            // 如果 `postdata` 不为空
            if (postdata != "")
            {
                // 使用 `Web.UploadData` 方法上传数据
                result = await Download.UploadData($"{domain}/filemoreajax.php", postdata, 1, Content);
                // 将返回的 JSON 字符串转换为动态对象
                dynamic lanzouJsonFolder = Json.DeserializeObject(result);
                // 如果返回的状态码为 1
                if ($"{lanzouJsonFolder["zt"]}" == "1")
                {
                    // 创建一个 `StringBuilder` 对象来存储文件信息
                    StringBuilder files = new StringBuilder().Clear();
                    // 遍历所有文件
                    for (int i = 0; i < Convert.ToInt32($"{lanzouJsonFolder["text"].Length}"); i++) { files.Append($"文件名：{lanzouJsonFolder["text"][i]["name_all"]}\n大小：{lanzouJsonFolder["text"][i]["size"]}\n上传时间：{lanzouJsonFolder["text"][i]["time"]}\n链接：{domain}/{Regex.Match(lanzouJsonFolder["text"][i]["id"], "\\w*")}\n\n----------------------------------------------------\n\n"); }
                    // 将 `StringBuilder` 对象转换为字符串并存储在 `result` 变量中
                    result = files.ToString();
                }
                else { result = $"错误：{lanzouJsonFolder["info"]}"; }
            }
            return result;
        }
        /// <summary>
        /// QQ邮箱文件中转站直链解析
        /// </summary>
        /// <param name="Content">分享链接</param>
        /// <returns></returns>
        internal static async Task<string> QQ邮箱直链解析(string Content)
        {
            foreach (Match link in Regex.Matches(await Download.DownloadString(Content), "http[^\"]+")) { if (link.Value.Contains("ftn.qq.com")) { return Regex.Unescape(link.Value).Replace("&eggs", ""); } }
            return "QQ邮箱直链解析失败...";
        }
    }
}