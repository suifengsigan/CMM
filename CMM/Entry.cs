using ElecManage;
using NXOpen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CMM
{
    public class Entry
    {
        public static void Main()
        {

        }

        public static void InitUG()
        {
            Helper.ShowMsg("UG正在初始化...");
            var session=NXOpen.Session.GetSession();
            Helper.ShowMsg("UG完成初始化");
        }

        public static string[] Verification()
        {
            Helper.ShowMsg("正在验证加密锁...");
            var result = new string[] { "1",""};
            string msg = string.Empty;
            if (!License.Entry.Instance.Verification(EactConfig.ConfigData.GetInstance().LicenseType, out msg))
            {
                result[0] = "2";
                result[1] = msg;
            }
            Helper.ShowMsg("验证加密锁完成");
            return result;
        }

        /// <summary>
        /// 自动选点
        /// </summary>
        public static void AutoSelPoint()
        {
            Helper.ShowMsg("正在匹配图档...");
            var cmmConfig = CMMTool.CMMConfig.GetInstance();
            if (cmmConfig.IsUploadDataBase)
            {
                try
                {
                    CMMBusiness.InitDatabase();
                    DataAccess.BOM.IsConnect();
                }
                catch (Exception ex)
                {
                    Helper.ShowMsg("数据库连接异常：" + ex.Message);
                    return;
                }
            }
            var ConfigData = EactConfig.ConfigData.GetInstance();
            EactTool.FileHelper.InitFileMode(cmmConfig.IsAutoCmmFtpDir ? 1 : 0, ConfigData.FTP.Address, "", ConfigData.FTP.User, ConfigData.FTP.Pass, false);
            var path = cmmConfig.AutoCmmDir;
            var fileName = EactTool.FileHelper.FindFile(path);
            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    AutoSelPoint(fileName);
                    EactTool.FileHelper.DeleteFile(path, fileName);
                }
                catch (Exception ex)
                {
                    Helper.ShowMsg("自动取点异常:" + ex.Message, 1);
                    EactTool.FileHelper.WriteErrorFile(path, fileName, ex.Message);
                }
            }
        }

        /// <summary>
        /// 导入探针数据
        /// </summary>
        public static CMMTool.CMMConfig ImportProbePart()
        {
            var config = CMMTool.CMMConfig.GetInstance();
            foreach (var item in config.ProbeDatas)
            {
                foreach (var ab in item.GetABList())
                {
                    var fileName = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMM_INSPECTION"), string.Format("{0}A{1}B{2}.prt", item.ProbeName, ab.A, ab.B));
                    Helper.ImportPart(fileName);
                }
            }
            return config;
        }

        /// <summary>
        /// 自动选点
        /// </summary>
        public static void AutoSelPoint(string filename)
        {
            //导入探针
            var part = NXOpen.Session.GetSession().Parts.Work;
            if (part != null)
            {
                Snap.NX.Part.Wrap(part.Tag).Close(true, true);
            }

            Snap.NX.Part snapPart = Snap.NX.Part.OpenPart(filename);
            var name = Path.GetFileNameWithoutExtension(filename);
            Snap.Globals.WorkPart = snapPart;
            try
            {
                var body = snapPart.Bodies.FirstOrDefault();
                var config = ImportProbePart();
                Helper.ShowMsg(string.Format("{0}开始取点...", name));
                var list = CMMBusiness.AutoSelPoint(body, config);
                Helper.ShowMsg(string.Format("{0}取点完成", name));
            }
            catch (Exception ex)
            {
                Helper.ShowMsg(string.Format("{0}取点错误【{1}】", name, ex.Message));
                Console.WriteLine("AutoSelPoint错误:{0}", ex.Message);
                throw ex;
            }
            finally
            {
                snapPart.Close(true, true);
            }
        }

        static bool IsInit = false;

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            Helper.ShowMsg("初始化配置数据...");
            if (!IsInit)
            { 
                CMMTool.Business.InitConfig();
            }
            IsInit = true;
            Helper.ShowMsg("完成初始化配置数据");
        }
    }
}
