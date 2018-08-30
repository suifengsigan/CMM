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
        public const string EACT_PROBEINSPECTIONPOINT = "EACT_PROBEINSPECTIONPOINT";
        /// <summary>
        /// 删除模型
        /// </summary>
        public static void DeleteProbe()
        {
            var temp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMM_INSPECTION");
            if (Directory.Exists(temp))
            {
                var files = Directory.GetFiles(temp);
                foreach (var file in files)
                {
                    if (Path.GetExtension(file).ToUpper() == ".PRT")
                    {
                        File.Delete(file);
                    }
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

                var inPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMM_INSPECTION");

                //创建路径
                if (!Directory.Exists(inPath))
                {
                    Directory.CreateDirectory(inPath);
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
                    var fileName = Path.Combine(inPath, sphere.Name);
                    if (File.Exists(fileName + ".prt"))
                    {
                        File.Delete(fileName + ".prt");
                    }
                    var dir = Path.GetDirectoryName(fileName);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    //创建干涉检查数据
                    var faces = sphere.Faces;
                    var points = new List<Position>();
                    points.Add(Position.Origin);
                    faces.ToList().ForEach(u =>
                    {
                        var faceBox = u.Box;
                        var centerPoint = new Snap.Position((faceBox.MaxX + faceBox.MinX) / 2, (faceBox.MaxY + faceBox.MinY) / 2, (faceBox.MaxZ + faceBox.MinZ) / 2);
                        points.Add(centerPoint);
                        if (!u.IsHasAttr(EACTPROBESPHEREFACE))
                        {
                            u.Edges.ToList().ForEach(e => {
                                points.Add(e.StartPoint);
                                points.Add(e.EndPoint);
                            });
                        }
                    });
                    points = points.Distinct().ToList();
                    var str = Newtonsoft.Json.JsonConvert.SerializeObject(points);
                    sphere.SetStringAttribute(EACT_PROBEINSPECTIONPOINT, str);
                    var exObject = new List<NXOpen.NXObject> { sphere };
                    Snap.Globals.WorkPart.Lines.ToList().ForEach(u => { exObject.Add(u.NXOpenDisplayableObject); });
                    SnapEx.Create.ExtractObject(exObject, fileName, false, true);
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
