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
        public const string EACTPROBESPHEREFACE = "EACTPROBESPHEREFACE";
        /// <summary>
        /// 删除模型
        /// </summary>
        public static void DeleteProbe()
        {
            var temp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMM_INSPECTION");
            var files = Directory.GetFiles(temp);
            foreach (var file in files)
            {
                if (file.Contains(".prt"))
                {
                    File.Delete(file);
                }
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
                    var filePath = Path.Combine(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config"), "blank.prt");
                    basePart = Snap.NX.Part.OpenPart(filePath);
                    Snap.Globals.WorkPart = basePart;
                }

                foreach (var ab in data.GetABList())
                {
                    var mark = Globals.SetUndoMark(Globals.MarkVisibility.Invisible, "CreateProbe");
                    //创建探球
                    var vec = Snap.Orientation.Identity.AxisZ;
                    var sphere = Snap.Create.Sphere(new Position(), data.D).Body;
                    sphere.Faces.ToList().ForEach(u =>
                    {
                        u.SetStringAttribute(EACTPROBESPHEREFACE, "1");
                    });
                    //连接部分值
                    var tmpConnectValue = 2;
                    var tmpConnectHeight = 3;
                    //创建加长杆
                    var lengtheningRodMaxPosition = new Position(0, 0, data.L - (data.D / 2) - tmpConnectHeight);
                    var lengtheningRod = Snap.Create.Cylinder(new Position(), lengtheningRodMaxPosition, data.d).Body;
                    //创建连接部分
                    var connect = Snap.Create.Cone(lengtheningRodMaxPosition, vec, new Number[] { data.d, data.d + tmpConnectValue }, tmpConnectHeight).Body;
                    //创建基座
                    var startPedestal = lengtheningRodMaxPosition + new Position(0, 0, tmpConnectHeight);
                    var firstPedestal = Snap.Create.Cylinder(startPedestal, startPedestal + new Position(0, 0, data.L2), data.D3).Body;
                    var twoPedestalPosition = startPedestal + new Position(0, 0, data.L2);
                    var twoPedestal = Snap.Create.Sphere(twoPedestalPosition, data.D1).Body;
                    var threePedestalPosition = twoPedestalPosition;
                    var threePedestal = Snap.Create.Cylinder(threePedestalPosition, threePedestalPosition + new Position(0, 0, data.L1), data.D2).Body;

                    //AB旋转
                    var requireAbBodies = new List<Snap.NX.Body> { sphere, lengtheningRod, connect, firstPedestal };
                    var trans=Snap.Geom.Transform.CreateRotation(twoPedestalPosition, Snap.Orientation.Identity.AxisX, ab.A);
                    trans = Snap.Geom.Transform.Composition(trans, Snap.Geom.Transform.CreateRotation(twoPedestalPosition, Snap.Orientation.Identity.AxisZ, ab.B));
                    foreach (var rBody in requireAbBodies)
                    {
                        rBody.Move(trans);
                    }

                    var r = Snap.Create.Unite(sphere, lengtheningRod, connect, firstPedestal, twoPedestal, threePedestal);
                    r.Orphan();
                    sphere.Name = string.Format("{0}A{1}B{2}", data.ProbeName, ab.A, ab.B);
                    var fileName = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMM_INSPECTION"), sphere.Name);
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
                }

                if (basePart != null)
                {
                    basePart.Close(true, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("CreateProbe错误:{0}", ex.Message));
                result = false;
            }
            return result;
        }
    }
}
