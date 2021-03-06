﻿using Snap;
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
        /// 初始化配置
        /// </summary>
        public static void InitConfig()
        {
            //初始化探针数据
            var config = CMMTool.CMMConfig.GetInstance();
            CMMTool.Business.DeleteProbe();
            foreach (var item in config.ProbeDatas.ToList())
            {
                CMMTool.Business.CreateProbe(item);
            }
        }

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

        public static List<ProbeData.AB> OrderProbeDataAB(List<ProbeData.AB> abs, Snap.Position p, Snap.Vector pV)
        {
            var axisZ = Snap.Orientation.Identity.AxisZ;
            Func<ProbeData.AB, double> getAngleAction = (ab) => {
                var angle = Snap.Vector.Angle(pV, axisZ.Copy(GetTrans(p, ab)));
                return angle;
            };
            var result = abs.OrderBy(u => getAngleAction(u)).ToList();
            return result;
        }

        private static Snap.Geom.Transform GetTrans(Snap.Position pos, ProbeData.AB ab)
        {
            var trans = Snap.Geom.Transform.CreateRotation(pos, -Snap.Orientation.Identity.AxisX, ab.A);
            trans = Snap.Geom.Transform.Composition(trans, Snap.Geom.Transform.CreateRotation(pos, -Snap.Orientation.Identity.AxisZ, -ab.B));
            return trans;
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
                    var requireAbBodies = new List<Snap.NX.Body>();
                    var requireUnite = new List<Snap.NX.Body>();
                    var mark = Globals.SetUndoMark(Globals.MarkVisibility.Invisible, "CreateProbe");
                    //创建探球
                    var vec = Snap.Orientation.Identity.AxisZ;
                    var sphere = Snap.Create.Sphere(new Position(), data.D).Body;
                    sphere.Faces.ToList().ForEach(u =>
                    {
                        u.SetStringAttribute(EACTPROBESPHEREFACE, "1");
                    });
                    //创建加长杆
                    var lengtheningRodMaxPosition = new Position(0, 0, data.L);
                    var lengtheningRod = Snap.Create.Cylinder(new Position(), lengtheningRodMaxPosition, data.d).Body;
                   

                    Action<double,double,double> action = (h,ed,ed2) => 
                    {
                        if (h > 0 && ed > 0)
                        {
                            if (ed2 <= 0)
                            {
                                ed2 = ed;
                            }
                            //创建加长杆1
                            Snap.NX.Body connect1;
                            if (ed != ed2)
                            {
                                connect1 = Snap.Create.Cone(lengtheningRodMaxPosition, vec, new Number[] { ed, ed2 }, h).Body;
                            }
                            else
                            {
                                connect1 = Snap.Create.Cylinder(lengtheningRodMaxPosition, lengtheningRodMaxPosition + new Position(0, 0, h), ed).Body;
                            }
                           
                            lengtheningRodMaxPosition = lengtheningRodMaxPosition + new Position(0, 0, h);
                            requireAbBodies.AddRange(new List<Snap.NX.Body> { connect1 });
                            requireUnite.AddRange(new List<Snap.NX.Body> { connect1 });
                        }
                    };

                    data.ExtensionBarDataList.ForEach(u => {
                        action(u.Height, u.D1, u.D2);
                    });

                    //创建基座
                    var startPedestal = lengtheningRodMaxPosition;
                    var firstPedestal = Snap.Create.Cylinder(startPedestal, startPedestal + new Position(0, 0, data.L2), data.D3).Body;
                    var twoPedestalPosition = startPedestal + new Position(0, 0, data.L2);
                    var twoPedestal = Snap.Create.Sphere(twoPedestalPosition, data.D1).Body;
                    var threePedestalPosition = twoPedestalPosition;
                    var threePedestal = Snap.Create.Cylinder(threePedestalPosition, threePedestalPosition + new Position(0, 0, data.L1), data.D2).Body;

                    //AB旋转
                    requireAbBodies.AddRange(new List<Snap.NX.Body> { sphere, lengtheningRod, firstPedestal });
                    requireUnite.AddRange( new List<Snap.NX.Body> { lengtheningRod, firstPedestal, twoPedestal, threePedestal });
                    var trans = GetTrans(twoPedestalPosition, ab);
                    foreach (var rBody in requireAbBodies)
                    {
                        rBody.Move(trans);
                    }

                    var r = Snap.Create.Unite(sphere, requireUnite.ToArray());
                    r.Orphan();
                    sphere.Move(Snap.Geom.Transform.CreateTranslation(Snap.Position.Origin-Snap.Position.Origin.Copy(trans)));
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
                    faces.ToList().ForEach(u =>
                    {
                        var faceBox = u.Box;
                        var centerPoint = new Snap.Position((faceBox.MaxX + faceBox.MinX) / 2, (faceBox.MaxY + faceBox.MinY) / 2, (faceBox.MaxZ + faceBox.MinZ) / 2);
                        points.Add(centerPoint);
                        if (!u.IsHasAttr(EACTPROBESPHEREFACE))
                        {
                            u.Edges.ToList().ForEach(e =>
                            {
                                points.Add(e.StartPoint);
                                points.Add(e.EndPoint);
                            });
                        }
                        else
                        {
                            Snap.NX.Face.Sphere sphereFace = u as Snap.NX.Face.Sphere;
                            points.Add(sphereFace.Geometry.Center);
                        }
                    });
                    points = points.Distinct().ToList();
                    var str = Newtonsoft.Json.JsonConvert.SerializeObject(points);
                    sphere.SetStringAttribute(EACT_PROBEINSPECTIONPOINT, str);
                    var exObject = new List<NXOpen.NXObject> { sphere };
                    exObject.ForEach(u => {
                        Snap.NX.NXObject uO = u;
                        uO.IsHidden = true;
                        uO.SetStringAttribute(SnapEx.ConstString.CMM_INSPECTION_SPHERE, "1");
                    });
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
