using CMMTool;
using NXOpen.UF;
using Snap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMM
{
    /// <summary>
    /// 自动取点业务
    /// </summary>
    public class CMMBusiness
    {
        /// <summary>
        /// 自动取点
        /// </summary>
        public static string AutoSelPoint(Snap.NX.Body body)
        {
            return string.Empty;
        }

        /// <summary>
        /// 获取检测点(使用增量干涉)
        /// </summary>
        public void GetCheckPoint()
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
