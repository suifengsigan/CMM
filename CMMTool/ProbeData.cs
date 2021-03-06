﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SnapEx;

namespace CMMTool
{
    public class ProbeData
    {
        /// <summary>
        /// 名称
        /// </summary>
        [DisplayName("探针名称")]
        public string ProbeName { get; set; }
        /// <summary>
        /// 探球直径
        /// </summary>
        public double D = 0;
        /// <summary>
        /// 测杆直径
        /// </summary>
        public double d = 0;
        /// <summary>
        /// 测头工作长度
        /// </summary>
        public double L = 0;
        public double D1 = 0;
        public double L1 = 0;
        public double L2 = 0;
        public double D2 = 0;
        public double D3 = 0;
        /// <summary>
        /// 加长杆数据
        /// </summary>
        public List<ExtensionBarData> ExtensionBarDataList = new List<ExtensionBarData>();
          /// <summary>
          /// 探针角度
          /// </summary>
        public List<AB> ABList = new List<AB>();

        public List<AB> GetABList(Snap.Position p, Snap.Vector pV)
        {
            var list =  GetABList();
            list = Business.OrderProbeDataAB(list,p,pV);
            var tempList = list.Where(u => u.A == 0 && u.B == 0).ToList();
            tempList.ForEach(u => {
                list.Remove(u);
            });
            tempList.ForEach(u => {
                list.Insert(0, u);
            });
            return list;
        }
        public List<AB> GetABList()
        {
            var list = ABList ?? new List<AB>();
            if (list.Where(u => u.A == 0 && u.B == 0).Count() == 0)
            {
                list.Insert(0, new AB { });
            }
            return list;
        }

        

        /// <summary>
        /// 获取球心
        /// </summary>
        public Snap.Position GetCentreOfSphere(ProbeData.AB ab)
        {
            var sphere=GetBody(ab);
            Snap.NX.Face.Sphere sphereFace = sphere.Faces.FirstOrDefault(u => u.IsHasAttr(Business.EACTPROBESPHEREFACE)) as Snap.NX.Face.Sphere;
            return sphereFace.Geometry.Center;
        }


        /// <summary>
        /// 获取体
        /// </summary>
        public Snap.NX.Body GetBody(ProbeData.AB ab)
        {
            Snap.NX.Body result = null;
            result = Snap.Globals.WorkPart.Bodies.FirstOrDefault(u => u.IsHasAttr(SnapEx.ConstString.CMM_INSPECTION_SPHERE)&&u.Name == string.Format("{0}A{1}B{2}", this.ProbeName, ab.A, ab.B));
            return result;
        }
        

        /// <summary>
        /// 干涉点检查
        /// </summary>
        public bool CheckInspectionPath(ProbeData.AB ab,List<Snap.Position> inspectionPath, params Snap.NX.NXObject[] inspectionBodies)
        {
            //var mark = Snap.Globals.SetUndoMark(Snap.Globals.MarkVisibility.Invisible, "CheckInspectionPath");
            var reuslt = false;
            try
            {
                if (inspectionBodies.Count() <= 0 && inspectionPath.Count >= 2)
                    return reuslt;
                var inspectionPoints = GetInspectionPoints(ab);
                var trans = new List<Snap.Geom.Transform>();
                inspectionPath.ForEach(u => {
                    var tran = Snap.Geom.Transform.CreateTranslation(u - Snap.Position.Origin);
                    trans.Add(tran);
                });

                foreach (var item in inspectionPoints)
                {
                    var listP = new List<Snap.Position>();
                    foreach (var tranItem in trans)
                    {
                        listP.Add(item.Copy(tranItem));
                    }
                    //碰撞检测
                    for (int i = 0; i < listP.Count - 1; i++)
                    {
                        reuslt = SnapEx.Create.Intersect(inspectionBodies, listP[i], listP[i + 1]);
                        if (reuslt)
                        {
                            break;
                        }
                    }

                    if (reuslt)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Snap.Globals.UndoToMark(mark, null);
            }
            return reuslt;
        }

        /// <summary>
        /// 获取干涉数据
        /// </summary>
        public List<Snap.Position> GetInspectionPoints(ProbeData.AB ab)
        {
            var result = new List<Snap.Position>();
            var sphere = GetBody(ab);
            if (ab != null)
            {
                var str = sphere.GetAttrValue(Business.EACT_PROBEINSPECTIONPOINT);
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Snap.Position>>(str) ?? new List<Snap.Position>();
            }
            return result;
        }

        public class AB
        {
            public double A { get; set; }
            public double B { get; set; }
        }

        /// <summary>
        /// 加长杆数据
        /// </summary>
        public class ExtensionBarData
        {
            [DisplayName("高度")]
            public double Height { get; set; }
            [DisplayName("底面直径")]
            public double D1 { get; set; }
            [DisplayName("顶面直径")]
            public double D2 { get; set; }
        }

        /// <summary>
        /// 是否基准面测针
        /// </summary>
        public bool IsBaseFaceProbe = false;
    }
}
