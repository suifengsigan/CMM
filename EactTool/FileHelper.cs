using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EactTool
{
    /// <summary>
    /// 文件帮助类
    /// </summary>
    public class FileHelper
    {
        const string EACTERROR = "EACTERROR";
        public static string FindFile(string path)
        {
            if (Directory.Exists(path))
            {
                var list = Directory.GetFiles(path).Where(u => Path.GetExtension(u).ToUpper() == ".PRT");
                return list.OrderBy(u => new FileInfo(u).LastWriteTime).FirstOrDefault();
            }
            return string.Empty;
        }

        public static void DeleteFile(string path,string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        public static void WriteErrorFile(string path, string fileName,string errorMsg)
        {
            var name = Path.GetFileName(fileName);
            var nameW = Path.GetFileNameWithoutExtension(fileName);
            var newPath = Path.Combine(path, EACTERROR);
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            File.Copy(fileName, Path.Combine(newPath, name));
            File.WriteAllText(Path.Combine(newPath, nameW + "error.txt"), errorMsg);
            DeleteFile(path, fileName);
        }
    }
}
