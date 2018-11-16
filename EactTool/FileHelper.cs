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
        private static int _mode = 0;
        private static string _FtpServerIP;
        private static string _FtpRemotePath;
        private static string _FtpUserID;
        private static string _FtpPassword;
        private static bool _SSL;
        public static void InitFileMode(int mode, string FtpServerIP, string FtpRemotePath, string FtpUserID, string FtpPassword, bool SSL,
            string Eact_FTPPATH = "/Eact_AutoCMM", 
            string tempPath = @"Temp\Eact_AutoCMM",
            string errorPath = @"Temp\Eact_AutoCMM_Error")
        {
            _mode = mode;
            _FtpServerIP = FtpServerIP;
            _FtpRemotePath = FtpRemotePath;
            _FtpUserID = FtpUserID;
            _FtpPassword = FtpPassword;
            _SSL = SSL;
            _Eact_FTPPATH = Eact_FTPPATH;
            _tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tempPath);
            _errorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, errorPath);
        }

        public static string FindFile(string path)
        {
            switch (_mode)
            {
                case 1:
                    {
                        return FindFtpFile();
                    }
                default:
                    {
                        return FindFileEx(path);
                    }
            }
        }

        public static void DeleteFile(string path, string fileName)
        {
            switch (_mode)
            {
                case 1:
                    {
                        DeleteFtpFile(fileName);
                        break;
                    }
                default:
                    {
                        DeleteFileEx(path, fileName);
                        break;
                    }
            }
        }

        public static void WriteErrorFile(string path, string fileName, string errorMsg)
        {
            switch (_mode)
            {
                case 1:
                    {
                        var name = Path.GetFileName(fileName);
                        path = _errorPath;
                        var nameW = Path.GetFileNameWithoutExtension(fileName);
                        var newPath = path;
                        if (!Directory.Exists(newPath))
                        {
                            Directory.CreateDirectory(newPath);
                        }
                        var newFileName = Path.Combine(newPath, name);
                        if (!File.Exists(newFileName))
                        {
                            File.Copy(fileName, newFileName);
                            File.WriteAllText(Path.Combine(newPath, nameW + "error.txt"), errorMsg);
                        }
                        DeleteFile(path, fileName);
                        break;
                    }
                default:
                    {
                        WriteErrorFileEx(path, fileName, errorMsg);
                        break;
                    }
            }
        }


        public static string FindFileEx(string path)
        {
           
            if (Directory.Exists(path))
            {
                var list = Directory.GetFiles(path).Where(u => Path.GetExtension(u).ToUpper() == ".PRT");
                return list.OrderBy(u => new FileInfo(u).LastWriteTime).FirstOrDefault();
            }
            return string.Empty;
        }

        public static void DeleteFileEx(string path,string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        public static void WriteErrorFileEx(string path, string fileName, string errorMsg)
        {
            var name = Path.GetFileName(fileName);
            var nameW = Path.GetFileNameWithoutExtension(fileName);
            var newPath = Path.Combine(path, EACTERROR);
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            var newFileName = Path.Combine(newPath, name);
            if (!File.Exists(newFileName))
            {
                File.Copy(fileName, newFileName);
                File.WriteAllText(Path.Combine(newPath, nameW + "error.txt"), errorMsg);
            }
            DeleteFile(path, fileName);
        }

        private static string _Eact_FTPPATH = "/Eact_AutoCMM";
        private static string _tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Temp\Eact_AutoCMM");
        private static string _errorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Temp\Eact_AutoCMM_Error");
        public static string FindFtpFile()
        {
            var ftp = FlieFTP.Entry.GetFtp(_FtpServerIP, _FtpRemotePath, _FtpUserID, _FtpPassword, _SSL);
            if (ftp.DirectoryExist(_Eact_FTPPATH))
            {
                var files = ftp.GetFileList(_Eact_FTPPATH);
                var result = files.FirstOrDefault();
                if (!string.IsNullOrEmpty(result))
                {
                    if (Directory.Exists(_tempPath))
                    {
                        Directory.Delete(_tempPath, true);
                    }
                    Directory.CreateDirectory(_tempPath);
                    //上传至临时路径
                    ftp.DownloadFile(_tempPath, _Eact_FTPPATH, result);
                    result = Path.Combine(_tempPath, result);

                }
                return result;
            }
            return string.Empty;
        }

        public static void DeleteFtpFile(string fileName)
        {
            var ftp = FlieFTP.Entry.GetFtp(_FtpServerIP, _FtpRemotePath, _FtpUserID, _FtpPassword, _SSL);
            //删除Ftp文件
            if (ftp.DirectoryExist(_Eact_FTPPATH))
            {
                var ftpFile = string.Format("{0}/{1}", _Eact_FTPPATH, Path.GetFileName(fileName));
                ftp.Delete(ftpFile);
            }

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
    }
}
