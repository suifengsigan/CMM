using CMMTool;
using NXOpen.UF;
using Snap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapEx;

namespace CMM
{
    /// <summary>
    /// 自动取点业务
    /// </summary>
    public class CMMBusiness
    {
        /// <summary>
        /// 循环变量值
        /// </summary>
        static int LoopVarValue = 10;

        /// <summary>
        /// 自动取点
        /// </summary>
        public static List<PointData> AutoSelPoint(Snap.NX.Body body,CMMConfig config)
        {
            var electrode = ElecManage.Electrode.GetElectrode(body);
            if (electrode == null)
            {
                throw new Exception("无法识别该电极！");
            }
            electrode.InitAllFace();
            var positions = new List<PointData>();
            var tempPositions = new List<PointData>();
            tempPositions = GetHorizontalDatumFacePositions(electrode, config);
            if (tempPositions.Count < 4)
            {
                throw new Exception("基准面取点异常！");
            }

            //根据象限排序
            positions.AddRange(OrderPointDatas(tempPositions));

            tempPositions = GetVerticalDatumFacesPositions(electrode, config);
            if (tempPositions.Count < 8)
            {
                throw new Exception("侧面取点异常！");
            }
            positions.AddRange(tempPositions);


            tempPositions = GetElectrodeHeadFacePositions(electrode, config);
            positions.AddRange(tempPositions);

            return positions;
        }

        /// <summary>
        /// 获取头部面的检测点
        /// </summary>
        static List<PointData> GetElectrodeHeadFacePositions(ElecManage.Electrode elec, CMMConfig config)
        {
            var result = new List<PointData>();
            var faces = elec.ElecHeadFaces;
            foreach (var face in faces)
            {
                var positions = GetFacePoints(face);
                var faceMidPoint = face.Position((face.BoxUV.MaxU + face.BoxUV.MinU) / 2, (face.BoxUV.MaxV + face.BoxUV.MinV) / 2);
                var ps = positions.OrderBy(u => Snap.Position.Distance(faceMidPoint, u)).ToList();
                for (var i = 0; i < LoopVarValue; i++)
                {
                    if (ps.Count > i)
                    {
                        var item = ps[i];
                        var p1 = IsIntervene(item, config, PointType.HeadFace);
                        if (p1 != null)
                        {
                            result.Add(p1);
                            break;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取基准面的检测点
        /// </summary>
        static List<PointData> GetHorizontalDatumFacePositions(ElecManage.Electrode elec, CMMConfig config)
        {
            var result = new List<PointData>();
            var face = elec.BaseFace;
            var positions = GetFacePoints(face);
            var vector = face.GetFaceDirection();
            //边界点
            var p1 = face.Position(face.BoxUV.MinU, face.BoxUV.MinV);
            var p2 = face.Position(face.BoxUV.MinU, face.BoxUV.MaxV);
            var p3 = face.Position(face.BoxUV.MaxU, face.BoxUV.MinV);
            var p4 = face.Position(face.BoxUV.MaxU, face.BoxUV.MaxV);

            var tempPs = new List<Snap.Position> { p1, p2, p3, p4 };
            foreach (var tempP in tempPs)
            {
                var ps = positions.OrderBy(u => Snap.Position.Distance(tempP, u));
                foreach (var item in ps)
                {
                    var interveneP = IsIntervene(item,config,PointType.HorizontalDatumFace);
                    if (interveneP == null)
                    {
                        positions.Remove(item);
                    }
                    else
                    {
                        positions.Remove(item);
                        interveneP.PointType = PointType.HorizontalDatumFace;
                        result.Add(interveneP);
                        break;
                    }
                }
            }
            return result;
        }

        
        /// <summary>
        /// 获取侧面的检测点
        /// </summary>
        static List<PointData> GetVerticalDatumFacesPositions(ElecManage.Electrode elec, CMMConfig config)
        {
            var result = new List<PointData>();
            //获取所有的侧面
            var faces = elec.BaseSideFaces;
            //获取Z值
            var minZ = elec.BaseFace.Box.MinZ;
            minZ -= config.SideFaceGetPointValue;
            foreach (var face in faces)
            {
                
            }
            return result;
        }

        static List<PointData> OrderPointDatas(List<PointData> tempPositions)
        {
            var positions = new List<PointData>();
            var predicates = new List<Func<PointData, bool>>();
            predicates.Add(u => u.Position.X < 0 && u.Position.Y < 0);
            predicates.Add(u => u.Position.X > 0 && u.Position.Y < 0);
            predicates.Add(u => u.Position.X > 0 && u.Position.Y > 0);
            foreach (var i in predicates)
            {
                var thirdQuadrant = tempPositions.Where(i).ToList();
                positions.AddRange(thirdQuadrant);
                thirdQuadrant.ForEach(u =>
                {
                    tempPositions.Remove(u);
                });
            }
            positions.AddRange(tempPositions);
            return positions;
        }

        static PointData IsIntervene(Snap.Position p, CMMConfig config, PointType pointType = PointType.UNKOWN)
        {
            PointData result = null;
            return result;
        }

        /// <summary>
        /// 获取干涉点(使用增量干涉)
        /// </summary>
        static void GetCheckPoint(ElecManage.Electrode elec, ProbeData data)
        {
            //创建射线检测
        }

        /// <summary>
        /// 干涉检查（1 -> there is interference  2 -> no interference  3 -> touching, that is coincident faces）
        /// </summary>
        static bool CheckInterference(NXOpen.Tag targetBody,NXOpen.Tag toolBody)
        {
            var result = true;
            var ufSession = NXOpen.UF.UFSession.GetUFSession();
            int[] r = new int[] { 0 };
            ufSession.Modl.CheckInterference(targetBody, 1, new NXOpen.Tag[] { toolBody }, r);
            result = r[0] != 3;
            return result;
        }

        /// <summary>
        /// 点是否在面上
        /// </summary>
        static bool IsPointAtFace(Snap.NX.Face face, Snap.Position p)
        {
            return Snap.Compute.Distance(p, face) < SnapEx.Helper.Tolerance;
        }

        /// <summary>
        /// 是否小于探针半径
        /// </summary>
        static bool IsLessthanProbeR(List<Snap.NX.Curve> curves, Snap.Position p, ProbeData probe)
        {
            foreach (var item in curves)
            {
                var d = Compute.Distance(p, item);
                if (d <= probe.D/2)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取面上所有的测量点
        /// </summary>
        static List<Snap.Position> GetFacePoints(Snap.NX.Face face, double max_facet_size = 1)
        {
            var mark = Snap.Globals.SetUndoMark(Globals.MarkVisibility.Visible, "GetFacePoints");
            var positions = new List<Snap.Position>();
            try
            {
                #region old code
                var parameters = new UFFacet.Parameters();

                var ufSession = NXOpen.UF.UFSession.GetUFSession();
                var facet = ufSession.Facet;
                facet.AskDefaultParameters(out parameters);
                parameters.max_facet_edges = 3;
                parameters.specify_max_facet_size = true;
                parameters.max_facet_size = 1;

                NXOpen.Tag facet_model = NXOpen.Tag.Null;
                facet.FacetSolid(face.NXOpenTag, ref parameters, out facet_model);

                if (facet_model == NXOpen.Tag.Null) return positions;
                NXOpen.Tag solid = NXOpen.Tag.Null;
                facet.AskSolidOfModel(facet_model, out solid);
                if (solid != face.NXOpenTag) return positions;

                int facet_id = NXOpen.UF.UFConstants.UF_FACET_NULL_FACET_ID;
                bool isWhile = true;
                while (isWhile)
                {
                    facet.CycleFacets(facet_model, ref facet_id);
                    if (facet_id != NXOpen.UF.UFConstants.UF_FACET_NULL_FACET_ID)
                    {
                        int num_vertices = 0;
                        facet.AskNumVertsInFacet(facet_model, facet_id, out num_vertices);
                        if (num_vertices == 3)
                        {
                            var vertices = new double[num_vertices, 3];
                            facet.AskVerticesOfFacet(facet_model, facet_id, out num_vertices, vertices);
                            for (int i = 0; i < num_vertices; i++)
                            {
                                int pt_status = 0;
                                var position = new Snap.Position(vertices[i, 0], vertices[i, 1], vertices[i, 2]);
                                ufSession.Modl.AskPointContainment(position.Array, face.NXOpenTag, out pt_status);
                                if (0x1 == pt_status || 0x3 == pt_status)
                                {
                                    positions.Add(position);
                                }
                            }
                        }
                    }
                    else
                    {
                        isWhile = false;
                    }
                }

                ufSession.Obj.DeleteObject(facet_model);
                positions = positions.Distinct().ToList();
                #endregion
               
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Snap.Globals.UndoToMark(mark, null);
            }

            return positions;
        }
    }
}
