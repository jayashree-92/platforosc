using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Xml.Linq;

namespace Web.Util
{
    public static class StaticFileUrlModifier
    {
        private static readonly Dictionary<string, string> StaticFileNamesMap = new Dictionary<string, string>();
        private static readonly object Locker = new object();
        
        public static string GetModifiedStaticFileUrl(this string file)
        {
            if (StaticFileNamesMap.ContainsKey(file))
                return StaticFileNamesMap[file];

            var newUrl = GetNewStaticFileUrl(file);

            lock (Locker)
            {
                if (!StaticFileNamesMap.ContainsKey(file))
                    StaticFileNamesMap.Add(file, newUrl);
            }

            return newUrl;
        }

        private const string AppendCacheFileNamesWithDateFormat = "yyyyMMddHHmmssff";

        private static string GetNewStaticFileUrl(string file)
        {
            if (!IsUrlReWritten()) return file;

            var sysPath = HostingEnvironment.MapPath(file);

            if (string.IsNullOrWhiteSpace(sysPath))
                return file;

            var fileInfo = new FileInfo(sysPath);
            if (!fileInfo.Exists)
                return file;

            var fileParts = file.Split('/');
            var fileNameWithOutExt = fileInfo.GetNameWithoutExtension();
            var fileExtension = fileInfo.Extension;
            var lastWriteTime = fileInfo.LastWriteTime.ToString(AppendCacheFileNamesWithDateFormat);

            fileParts[fileParts.Length - 1] = $"{fileNameWithOutExt}.{lastWriteTime}{fileExtension}";

            var newUrl = string.Join("/", fileParts);
            newUrl = newUrl.Replace("~", string.Empty);

            return newUrl;
        }
        private static string GetNameWithoutExtension(this FileInfo fileInfo)
        {
            return fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
        }

        private static bool? _mIsUrlReWritten;
        private static bool IsUrlReWritten()
        {
            if (_mIsUrlReWritten.HasValue)
                return _mIsUrlReWritten.Value;

            var webConfig = WebConfigurationManager.OpenWebConfiguration("~");
            var cs = webConfig.GetSection("system.webServer");
            if (cs == null) return false;

            var xml = XDocument.Load(new StringReader(cs.SectionInformation.GetRawXml()));
            var rules = from c in xml.Descendants("rule") select c;

            _mIsUrlReWritten = rules.Select(rule => rule.Attribute("name").Value).Any(name => name == "Remove Modified Time from HTML JS CSS files");

            return _mIsUrlReWritten.Value;
        }
    }
}