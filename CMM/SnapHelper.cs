using NXOpen.UF;
using Snap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMM
{
    public class Helper
    {
        public static void ShowMsg(string msg)
        {
            CSharpProxy.ProxyObject.Instance.ShowMsg(msg);
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
        public static List<Snap.Position> GetFacePoints(Snap.NX.Face face, double max_facet_size = 1)
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
                var edges = face.EdgeCurves.ToList();
                #endregion

                //所有边上的点都不取
                positions.ToList().ForEach(p => {
                    if (Helper.GetPointToEdgeMinDistance(p, edges) < SnapEx.Helper.Tolerance)
                    {
                        positions.Remove(p);
                    }
                });
                

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Snap.Globals.UndoToMark(mark, null);
            }

            return positions;
        }
    }
}
