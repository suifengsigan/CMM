using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    public abstract class AutoCamBusiness
    {
        /// <summary>
        /// 自动编程
        /// </summary>
        public static void AutoCAM()
        {
            Helper.ShowMsg("正在匹配图档...");
            var ConfigData = EactConfig.ConfigData.GetInstance();
            EactTool.FileHelper.InitFileMode(true ? 1 : 0, ConfigData.FTP.Address, "", ConfigData.FTP.User, ConfigData.FTP.Pass, false, "/Eact_AutoCNC", @"Temp\Eact_AutoCNC", @"Temp\Eact_AutoCNC_Error");
            var path = string.Empty;
            var fileName = EactTool.FileHelper.FindFile(path);
            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    AutoCAM(fileName);
                    EactTool.FileHelper.DeleteFile(path, fileName);
                }
                catch (Exception ex)
                {
                    Helper.ShowMsg("自动编程异常:" + ex.Message, 1);
                    EactTool.FileHelper.WriteErrorFile(path, fileName, ex.Message);
                }
            }
        }

        /// <summary>
        /// 自动编程
        /// </summary>
        static void AutoCAM(string filename)
        {
            
            var part = NXOpen.Session.GetSession().Parts.Work;
            if (part != null)
            {
                Snap.NX.Part.Wrap(part.Tag).Close(true, true);
            }
            
            Snap.NX.Part snapPart = Snap.NX.Part.OpenPart(filename);
            AutoCAMUI.Helper.InitCAMSession("WsqAutoCAM");
            var name = Path.GetFileNameWithoutExtension(filename);
            Snap.Globals.WorkPart = snapPart;
            try
            {
                var body = snapPart.Bodies.FirstOrDefault();
                Helper.ShowMsg(string.Format("{0}开始自动编程...", name));
                var ele = ElecManage.Electrode.GetElectrode(body);
                if (ele != null)
                {
                    ele.InitAllFace();
                    AutoCAMUI.Helper.AutoCAM(ele);
                    Helper.ShowMsg(string.Format("{0}自动编程完成", name));
                }
            }
            catch (Exception ex)
            {
                Helper.ShowMsg(string.Format("{0}自动编程错误【{1}】", name, ex.Message));
                Console.WriteLine("自动编程错误:{0}", ex.Message);
                throw ex;
            }
            finally
            {
                snapPart.Close(true, true);
            }
        }


        /// <summary>
        /// 自动编程
        /// </summary>
        /// <param name="ele"></param>
        /// <param name="camConfig"></param>
        static void AutoCam(CAMElectrode ele, CNCConfig.CAMConfig camConfig)
        {

        }
    }
}
