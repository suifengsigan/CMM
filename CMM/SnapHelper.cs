﻿using NXOpen.UF;
using Snap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMM
{
    public class Helper
    {
        public static bool AskPointContainment(Snap.Position position, Snap.NX.Face face)
        {
            return Snap.Compute.Distance(position, face) <= SnapEx.Helper.Tolerance;
            var ufSession = NXOpen.UF.UFSession.GetUFSession();
            int pt_status = 0;
            ufSession.Modl.AskPointContainment(position.Array, face.NXOpenTag, out pt_status);
            if (0x1 == pt_status || 0x3 == pt_status)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 是否在相同的象限
        /// </summary>
        public static bool IsSameQuadrant(Snap.Position p1,Snap.Position p2,Snap.Position center)
        {
            var q1 = SnapEx.Helper.GetQuadrantType(p1, center, Snap.Orientation.Identity);
            var q2 = SnapEx.Helper.GetQuadrantType(p2, center, Snap.Orientation.Identity);
            return q1==q2;
        }
        
        public static void ShowMsg(string msg,int type=0)
        {
            if (CSharpProxy.ProxyObject.Instance == null)
            {
                UFSession.GetUFSession().Ui.SetStatus(msg);
            }
            else
            {
                CSharpProxy.ProxyObject.Instance.ShowMsg(msg, type);
            }
        }
        /// <summary>
        /// 获取点到面边的最小距离
        /// </summary>
        public static double GetPointToEdgeMinDistance(Snap.Position pos, List<Snap.NX.Curve> curves)
        {
            var result = double.MaxValue;
            foreach (var item in curves)
            {
                var d = Compute.Distance(pos, item);
                result = System.Math.Min(d, result);
            }
            return result;
        }
        public static Snap.NX.Body ImportPart(string fileName)
        {
            Snap.NX.Body result = null;
            NXOpen.UF.ImportPartModes modes = new NXOpen.UF.ImportPartModes();
            //坐标系
            double[] dest_csys = new double[6];
            //基准点
            double[] dest_point = new double[3];

            dest_csys[0] = 1; //坐标系X轴的矢量
            dest_csys[1] = 0;
            dest_csys[2] = 0;
            dest_csys[3] = 0; //坐标系Y轴的矢量
            dest_csys[4] = 1;
            dest_csys[5] = 0;
            dest_point[0] = 0.0; //基准点【导入到点坐标】
            dest_point[1] = 0.0;
            dest_point[2] = 0.0;

            //导入对象比例缩放
            double scale = 1.0;
            NXOpen.Tag group;
            NXOpen.UF.UFSession.GetUFSession().Part.Import(fileName, ref modes, dest_csys, dest_point, scale, out group);

            foreach (var m in Snap.Globals.WorkPart.Bodies)
            {
                var axisFace = m.Faces.FirstOrDefault(u => u.Name == SnapEx.ConstString.CMM_INSPECTION_AXISPOINT);
                if (axisFace != null)
                {
                    result = m;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// 点是否在面上
        /// </summary>
        public static bool IsPointAtFace(Snap.NX.Face face, Snap.Position p)
        {
            return Snap.Compute.Distance(p, face) < SnapEx.Helper.Tolerance;
        }

        /// <summary>
        /// 干涉检查（1 -> there is interference  2 -> no interference  3 -> touching, that is coincident faces）
        /// </summary>
        public static bool CheckInterference(NXOpen.Tag targetBody, NXOpen.Tag toolBody)
        {
            var result = true;
            var ufSession = NXOpen.UF.UFSession.GetUFSession();
            int[] r = new int[] { 0 };
            ufSession.Modl.CheckInterference(targetBody, 1, new NXOpen.Tag[] { toolBody }, r);
            result = !(r[0] == 3 || r[0] == 2);
            return result;
        }

        /// <summary>
        /// 获取面上所有的测量点
        /// </summary>
        public static List<Snap.Position> GetFacePoints(Snap.NX.Face face, CMMTool.CMMConfig config,List<Snap.NX.Curve> edges, bool isRoundInt=false,double max_facet_size = 1)
        {
            //var mark = Snap.Globals.SetUndoMark(Globals.MarkVisibility.Invisible, "GetFacePointsEx");
            var positions = new List<Snap.Position>();
            try
            {
                positions = SnapEx.Create.GetFacePoints(face, max_facet_size);
                ////所有边上的点都不取
                //var minD = double.MaxValue;
                //var probeDatas = config.ProbeDatas ?? new List<CMMTool.ProbeData>();
                //foreach (var data in probeDatas)
                //{
                //    minD = System.Math.Min(data.D, minD);
                //}
                //positions.ToList().ForEach(p => {
                //    if (Helper.GetPointToEdgeMinDistance(p, edges) < minD)
                //    {
                //        positions.Remove(p);
                //    }
                //});
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Snap.Globals.UndoToMark(mark, null);
            }

            return positions;
        }
    }
}
