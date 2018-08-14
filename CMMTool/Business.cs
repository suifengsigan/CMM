using Snap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapEx;
using System.IO;

namespace CMMTool
{
    public class Business
    {
        /// <summary>
        /// 删除模型
        /// </summary>
        public static void DeleteProbe(ProbeData data)
        {
            var temp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMM_INSPECTION");
            var fileName = Path.Combine(temp, data.ProbeName);
            if (File.Exists(fileName + ".prt"))
            {
                File.Delete(fileName + ".prt");
            }
        }

        /// <summary>
        /// 创建探针模型
        /// </summary>
        public static bool CreateProbe(ProbeData data)
        {
            bool result = true;
            try
            {
                Snap.NX.Part basePart = null;
                if (NXOpen.Session.GetSession().Parts.Work == null)
                {
                    var filePath = Path.Combine(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "CMM_INSPECTION"), "blank.prt");
                    basePart = Snap.NX.Part.OpenPart(filePath);
                    Snap.Globals.WorkPart = basePart;
                }

                var mark = Globals.SetUndoMark(Globals.MarkVisibility.Invisible, "CreateProbe");
                //创建探球
                var vec = Snap.Orientation.Identity.AxisZ;
                var sphere = Snap.Create.Sphere(new Position(), data.D).Body;
                //连接部分值
                var tmpConnectValue = 2;
                var tmpConnectHeight = 3;
                //创建加长杆
                var lengtheningRodMaxPosition = new Position(0, 0, data.L - (data.D / 2) - tmpConnectHeight);
                var lengtheningRod = Snap.Create.Cylinder(new Position(), lengtheningRodMaxPosition, data.d).Body;
                //创建连接部分
                var connect = Snap.Create.Cone(lengtheningRodMaxPosition, vec, new Number[] { data.d, data.d + tmpConnectValue }, tmpConnectHeight).Body;
                //创建基座
                var startPedestal = lengtheningRodMaxPosition + new Position(0,0, tmpConnectHeight);
                var firstPedestal=Snap.Create.Cylinder(startPedestal, startPedestal + new Position(0, 0, data.L2), data.D3).Body;
                var twoPedestalPosition = startPedestal + new Position(0, 0, data.L2);
                var twoPedestal = Snap.Create.Sphere(twoPedestalPosition, data.D1).Body;
                var threePedestalPosition = twoPedestalPosition;
                var threePedestal = Snap.Create.Cylinder(threePedestalPosition, threePedestalPosition + new Position(0, 0, data.L1), data.D2).Body;
                var r = Snap.Create.Unite(sphere, lengtheningRod, connect, firstPedestal, twoPedestal, threePedestal);
                r.Orphan();
                sphere.Name = data.ProbeName;
                var fileName = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMM_INSPECTION"), data.ProbeName);
                if (File.Exists(fileName + ".prt"))
                {
                    File.Delete(fileName + ".prt");
                }
                var dir = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                SnapEx.Create.ExtractBody(new List<NXOpen.Body> { sphere }, fileName, false, true);
                Globals.UndoToMark(mark, null);
                if (basePart != null)
                {
                    basePart.Close(true, true);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                result = false;
            }
            return result;
        }
    }
}
