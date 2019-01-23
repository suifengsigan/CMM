using NXOpen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapEx;

namespace AutoCAMUI
{
    public static class Helper
    {
        private static NXOpen.UF.UFSession ufSession = NXOpen.UF.UFSession.GetUFSession();
        /// <summary>
        /// 电极编程模板
        /// </summary>
        private static string ELECTRODETEMPLATETYPENAME = "EACT_AUTOCAM";

        /// <summary>
        /// 初始化CAM会话
        /// </summary>
        public static void InitCAMSession(string templateTypeName = "EACT_AUTOCAM")
        {
            ELECTRODETEMPLATETYPENAME = templateTypeName;
            Session theSession = Session.GetSession();
            Part workPart = theSession.Parts.Work;
            if (!theSession.IsCamSessionInitialized())
            {
                //进入CAM模块
                theSession.CreateCamSession();
            }

            var cAMSetup1 = workPart.CreateCamSetup(AUTOCAM_TYPE.mill_planar);

            ReinitOpt();
        }

        /// <summary>
        /// 加载模板
        /// </summary>
        public static void ReinitOpt()
        {
            var ufSession = NXOpen.UF.UFSession.GetUFSession();
            var curBaseDir = new System.IO.DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory).Parent.FullName;
            var curTemplatePrtDir = System.IO.Path.Combine(curBaseDir
                , @"MACH\resource\template_part\metric\");
            var curTemplateSetDir = System.IO.Path.Combine(curBaseDir
                , @"MACH\resource\template_set\");
            string UGII_CAM_TEMPLATE_SET_DIR = string.Empty;
            ufSession.UF.TranslateVariable("UGII_CAM_TEMPLATE_SET_DIR", out UGII_CAM_TEMPLATE_SET_DIR);
            var optFile = System.IO.Path.Combine(UGII_CAM_TEMPLATE_SET_DIR, "cam_general.opt");
            //ufSession.UF.SetVariable("EACT_AUTOCAM_ELE_TEMPLATEPRT_DIR", curTemplatePrtDir);
            var eact_cam_general_optFile = System.IO.Path.Combine(curTemplateSetDir, "eact_cam_general.opt");
            if (System.IO.File.Exists(optFile))
            {
                var optFileInfo = System.IO.File.ReadAllText(optFile);
                StringBuilder str = new StringBuilder();
                str.Append(optFileInfo);
                System.IO.Directory.GetFiles(curTemplatePrtDir).ToList().ForEach(u => {
                    string tempV = "${UGII_CAM_TEMPLATE_SET_DIR}" + System.IO.Path.GetFileName(u);
                    if (!optFileInfo.Contains(tempV))
                    {
                        str.AppendLine();
                        str.AppendLine(tempV);
                        System.IO.File.Copy(u, System.IO.Path.Combine(System.IO.Path.GetDirectoryName(optFile), System.IO.Path.GetFileName(u)),true);
                    }

                });

                if (optFileInfo != str.ToString())
                {
                    System.IO.File.WriteAllText(optFile, str.ToString());
                }
            }

            //ufSession.Cam.ReinitOpt(eact_cam_general_optFile);
        }

        /// <summary>
        /// 过切检查
        /// </summary>
        public static bool IsPathGouged(NXOpen.Tag oper)
        {
            var result = true;
            ufSession.Oper.IsPathGouged(oper, out result);
            return result;
        }

        //public static void AutoCAM(ElecManage.Electrode ele)
        //{
        //    camOpers.ForEach(u => {
        //        if (u.OperIsValid)
        //        {
        //            string name;
        //            ufSession.Obj.AskName(u.OperTag, out name);
        //            ShowInfoWindow(string.Format("{0}:{1}", name, u.IsPathGouged() ? "过切" : "未过切"));
        //        }
        //    });

        //    //获取后处理器列表
        //    string[] names;
        //    int count;
        //    ufSession.Cam.OptAskPostNames(out count, out names);
        //    var postName = "铜电极-自动换刀";
        //    var extension = "nc";

        //    var path = System.AppDomain.CurrentDomain.BaseDirectory;
        //    path = System.IO.Path.Combine(path, "Temp");
        //    path = System.IO.Path.Combine(path, "EACTCNCFILE");
        //    if (System.IO.Directory.Exists(path))
        //    {
        //        System.IO.Directory.Delete(path,true);
        //    }

        //    System.IO.Directory.CreateDirectory(path);

        //    camOpers.ForEach(u => {
        //        u.GenerateProgram(postName, path, extension);
        //    });

        //    //生成nc程式
        //    ufSession.Setup.GenerateProgram(
        //        Snap.Globals.WorkPart.NXOpenPart.CAMSetup.Tag,
        //        programGroupTag
        //        , postName
        //        , System.IO.Path.Combine(path, string.Format(@"{0}.{1}", ele.ElecBody.Name, extension))
        //        , NXOpen.UF.UFSetup.OutputUnits.OutputUnitsOutputDefined
        //        );
        //}

        /// <summary>
        /// 设置Z偏置（骗刀Z）
        /// </summary>
        public static void SetZoffset(NXOpen.Tag operTag, double value)
        {
            Session theSession = Session.GetSession();
            Part workPart = theSession.Parts.Work;
            Part displayPart = theSession.Parts.Display;
            var oper = NXOpen.Utilities.NXObjectManager.Get(operTag) as NXOpen.CAM.Operation;
            var zLevelMillingBuilder1 = workPart.CAMSetup.CAMOperationCollection.CreatePlanarMillingBuilder(oper);
            zLevelMillingBuilder1.ToolChangeSetting.Zoffset.Value = value;
            NXObject nXObject1;
            nXObject1 = zLevelMillingBuilder1.Commit();
            zLevelMillingBuilder1.Destroy();
        }

        /// <summary>
        /// 设置横越(移刀)
        /// </summary>
        public static void SetFeedTraversal(NXOpen.Tag operTag, double value)
        {
            var oper = NXOpen.Utilities.NXObjectManager.Get(operTag) as NXOpen.CAM.Operation;
            var feedsBuilder1 = NXOpen.Session.GetSession().Parts.Work.CAMSetup.CreateFeedsBuilder(new NXOpen.CAM.CAMObject[] { oper });
            feedsBuilder1.FeedsBuilder.FeedTraversalBuilder.Value = value;
            feedsBuilder1.Commit();
            feedsBuilder1.Destroy();
        }

        /// <summary>
        /// 进给参数设置
        /// </summary>
        /// <param name="oper">工序</param>
        /// <param name="engage"></param>
        /// <param name="first_cut">第一刀切削</param>
        /// <param name="traversal">横越</param>
        /// <param name="retract">退刀</param>
        public static void SetFeedParam(NXOpen.Tag oper,double engage, double first_cut, double traversal, double retract)
        {
            SnapEx.EactUF.SetFeedRate(oper, NXOpen.UF.UFConstants.UF_PARAM_FEED_ENGAGE, engage);
            SnapEx.EactUF.SetFeedRate(oper, NXOpen.UF.UFConstants.UF_PARAM_FEED_FIRST_CUT, first_cut);
            SnapEx.EactUF.SetFeedRate(oper, NXOpen.UF.UFConstants.UF_PARAM_FEED_TRAVERSAL, traversal);
            SnapEx.EactUF.SetFeedRate(oper, NXOpen.UF.UFConstants.UF_PARAM_FEED_RETRACT, retract);
        }

        /// <summary>
        /// 指定文本几何体
        /// </summary>
        public static void SetCamText(NXOpen.Tag oper, List<Tag> tags)
        {
            ufSession.Camtext.AppendItems(oper, tags.Count, tags.ToArray());
        }

        /// <summary>
        /// 创建智能曲线
        /// </summary>
        public static NXOpen.Tag Create_SO_Curve(Snap.Position pos,Snap.Position pos1)
        {
            NXOpen.Tag result;
            var workPart = NXOpen.Session.GetSession().Parts.Work;
            var pointLst = new List<NXOpen.Tag>();
            pointLst.Add(Create_SO_Point(pos));
            pointLst.Add(Create_SO_Point(pos1));
            ufSession.So.CreateLineTwoPoints(workPart.Tag, NXOpen.UF.UFSo.UpdateOption.UpdateAfterModeling,pointLst.ToArray(),out result);
            ufSession.So.SetVisibilityOption(result, NXOpen.UF.UFSo.VisibilityOption.Invisible);
            return result;
        }

        /// <summary>
        /// 创建智能点
        /// </summary>
        public static NXOpen.Tag Create_SO_Point(Snap.Position pos)
        {
            NXOpen.Tag result;
            var scalarDoubleLst = new List<NXOpen.Tag>();
            NXOpen.Tag scalarDouble;
            var workPart = NXOpen.Session.GetSession().Parts.Work;
            ufSession.So.CreateScalarDouble(workPart.Tag, NXOpen.UF.UFSo.UpdateOption.UpdateAfterModeling, pos.X, out scalarDouble);
            scalarDoubleLst.Add(scalarDouble);
            ufSession.So.CreateScalarDouble(workPart.Tag, NXOpen.UF.UFSo.UpdateOption.UpdateAfterModeling, pos.Y, out scalarDouble);
            scalarDoubleLst.Add(scalarDouble);
            ufSession.So.CreateScalarDouble(workPart.Tag, NXOpen.UF.UFSo.UpdateOption.UpdateAfterModeling, pos.Z, out scalarDouble);
            scalarDoubleLst.Add(scalarDouble);
            ufSession.So.CreatePoint3Scalars(workPart.Tag, NXOpen.UF.UFSo.UpdateOption.UpdateAfterModeling, scalarDoubleLst.ToArray(), out result);
            return result;
        }

        /// <summary>
        /// 设置非切削移动区域起点
        /// </summary>
        public static void SetRegionStartPoints(NXOpen.Tag operTag, Snap.Position pos)
        {
            var oper = NXOpen.Utilities.NXObjectManager.Get(operTag) as NXOpen.CAM.Operation;
            var workPart = NXOpen.Session.GetSession().Parts.Work;
            var cavityMillingBuilder1 = workPart.CAMSetup.CAMOperationCollection.CreatePlanarMillingBuilder(oper);
            var point = workPart.Points.CreatePoint(pos);
            cavityMillingBuilder1.NonCuttingBuilder.SetRegionStartPoints(new Point[] { point });
            cavityMillingBuilder1.Commit();
            cavityMillingBuilder1.Destroy();
        }

        /// <summary>
        /// 设置进给率
        /// </summary>
        public static void SetFeedRate(NXOpen.Tag operTag, double value)
        {
            var oper = NXOpen.Utilities.NXObjectManager.Get(operTag) as NXOpen.CAM.Operation;
            var feedsBuilder1 = NXOpen.Session.GetSession().Parts.Work.CAMSetup.CreateFeedsBuilder(new NXOpen.CAM.CAMObject[] { oper });
            feedsBuilder1.FeedsBuilder.FeedCutBuilder.Value = value;
            feedsBuilder1.Commit();
            feedsBuilder1.Destroy();
        }


        /// <summary>
        /// 设置主轴转速
        /// </summary>
        public static void SetSpeedValue(NXOpen.Tag operTag,double speedValue,int param_index= NXOpen.UF.UFConstants.UF_PARAM_SPINDLE_RPM)
        {
            ufSession.Param.SetDoubleValue(operTag, param_index, speedValue);
        }

        /// <summary>
        /// 设置加工底面
        /// </summary>
        public static void SetCutFloor(NXOpen.Tag operTag, NXOpen.Tag faceTag)
        {
            SetCutFloor(operTag, new Snap.Position(), faceTag);
        }

        /// <summary>
        /// 设置加工底面
        /// </summary>
        public static void SetCutFloor(NXOpen.Tag operTag, Snap.Position origin, NXOpen.Tag faceTag = NXOpen.Tag.Null)
        {
            NXOpen.Tag result;
            if (NXOpen.Tag.Null == faceTag)
            {
                var identity = Snap.Orientation.Identity;
                NXOpen.Tag xforms;
                ufSession.So.CreateXformDoubles(Snap.Globals.WorkPart.NXOpenTag, NXOpen.UF.UFSo.UpdateOption.UpdateWithinModeling, origin.Array, identity.AxisX.Array, identity.AxisY.Array, 1, out xforms);
                result = xforms;
            }
            else
            {
                result = faceTag;
            }

            ufSession.Param.SetTagValue(operTag, NXOpen.UF.UFConstants.UF_PARAM_FLOOR, result);
        }

        /// <summary>
        /// 设置检查几何体
        /// </summary>
        public static void SetCheckGeometry(NXOpen.Tag operTag, NXOpen.Tag faceTag)
        {
            ufSession.Param.SetTagValue(operTag, NXOpen.UF.UFConstants.UF_PARAM_STOCK_CHECK, faceTag);
        }

        /// <summary>
        /// 设置部件余量和底部余量
        /// </summary>
        public static void SetPartStockAndFloorStock(NXOpen.Tag operTag, double sideStock, double floorStock)
        {
            ufSession.Param.SetDoubleValue(operTag, NXOpen.UF.UFConstants.UF_PARAM_STOCK_PART, sideStock);
            if (Math.Abs(sideStock - floorStock) < SnapEx.Helper.Tolerance)
            {
                ufSession.Param.SetIntValue(operTag, NXOpen.UF.UFConstants.UF_PARAM_STOCK_PART_USE, 1);
            }
            else
            {
                ufSession.Param.SetIntValue(operTag, NXOpen.UF.UFConstants.UF_PARAM_STOCK_PART_USE, 0);
                ufSession.Param.SetDoubleValue(operTag, NXOpen.UF.UFConstants.UF_PARAM_STOCK_FLOOR, floorStock);
            }
        }

        /// <summary>
        /// //通过Z轴偏置值设置加工层
        /// </summary>
        public static void SetCutLevels(NXOpen.Tag operTag, double zLevels, int levelsPosition = 1)
        {
            SetCutLevels(operTag, new List<double> { zLevels }, levelsPosition);
        }

        /// <summary>
        /// //通过Z轴偏置值设置加工层
        /// </summary>
        public static void SetCutLevels(NXOpen.Tag operTag, List<double> zLevels, int levelsPosition = 1)
        {
            NXOpen.UF.UFCutLevels.CutLevelsStruct cut_levels;
            ufSession.CutLevels.SetRangeType(operTag, NXOpen.UF.ParamClvRangeType.ParamClvRangeUserDefined, out cut_levels);
            int num = 0;
            switch (levelsPosition)
            {
                case 0:
                    {
                        NXOpen.UF.UFCutLevels.CutLevelsStruct tmpCut_levels;
                        ufSession.CutLevels.EditLevelUsingZ(operTag, num, zLevels.First(), cut_levels.cut_levels[num].local_cut_depth, out tmpCut_levels);
                        break;
                    }
                default:
                    {
                        num = cut_levels.num_levels - 1;
                        for (int i = 0; i < zLevels.Count; i++)
                        {
                            NXOpen.UF.UFCutLevels.CutLevelsStruct tmpCut_levels;

                            if (i == 0)
                            {
                                ufSession.CutLevels.EditLevelUsingZ(operTag, num, zLevels[i], cut_levels.cut_levels[num].local_cut_depth, out tmpCut_levels);
                            }
                            else
                            {
                                var tmpZ = zLevels[i];
                                ufSession.CutLevels.AddLevelsUsingZ(operTag, num + i, ref tmpZ, cut_levels.cut_levels[num].local_cut_depth, out tmpCut_levels);
                            }
                        }
                        break;
                    }
            }
          
           
        }

        /// <summary>
        /// //通过Z轴偏置值设置加工层
        /// </summary>
        public static void SetCutLevels(NXOpen.Tag operTag,NXOpen.Tag faceTag , int levelsPosition = 1)
        {
            double zLevels = Snap.NX.Face.Wrap(faceTag).GetCenterPointEx().Z;
            SetCutLevels(operTag, zLevels, levelsPosition);
        }

        /// <summary>
        /// 获取刀具
        /// </summary>
        public static NXOpen.Tag GetCutter(string cutterName, NXOpen.Tag cutterGroupRootTag)
        {
            NXOpen.Tag result = NXOpen.Tag.Null;
            ufSession.Ncgroup.AskObjectOfName(cutterGroupRootTag, cutterName, out result);
            return result;
        }

        /// <summary>
        /// 创建刀具
        /// </summary>
        /// <returns></returns>
        public static List<CAMCutter> CreateCutter(List<CAMCutter> cutters, NXOpen.Tag cutterGroupRootTag)
        {
            var result = new List<CAMCutter>();
            foreach (var item in cutters)
            {
                NXOpen.Tag cutterTag = NXOpen.Tag.Null;
                cutterTag = GetCutter(item.CutterName, cutterGroupRootTag);
                if (cutterTag == NXOpen.Tag.Null)
                {
                    ufSession.Cutter.Create(item.AUTOCAM_TYPE, item.AUTOCAM_SUBTYPE, out cutterTag);
                    ufSession.Ncgroup.AcceptMember(cutterGroupRootTag, cutterTag);
                    ufSession.Obj.SetName(cutterTag, item.CutterName);
                }
                ufSession.Param.SetDoubleValue(cutterTag, NXOpen.UF.UFConstants.UF_PARAM_TL_DIAMETER, item.TL_DIAMETER);
                ufSession.Param.SetDoubleValue(cutterTag, NXOpen.UF.UFConstants.UF_PARAM_TL_COR1_RAD, item.TL_COR1_RAD);
                ufSession.Param.SetDoubleValue(cutterTag, NXOpen.UF.UFConstants.UF_PARAM_TL_HEIGHT, item.TL_HEIGHT);
                ufSession.Param.SetDoubleValue(cutterTag, NXOpen.UF.UFConstants.UF_PARAM_TL_FLUTE_LN,item.TL_FLUTE_LN);
                ufSession.Param.SetIntValue(cutterTag, NXOpen.UF.UFConstants.UF_PARAM_TL_NUMBER, item.TL_NUMBER);
                ufSession.Param.SetIntValue(cutterTag, NXOpen.UF.UFConstants.UF_PARAM_TL_ADJ_REG, item.TL_ADJ_REG);
                item.CutterTag = cutterTag;
                result.Add(item);
            }
            return result;
        }
        
        /// <summary>
        /// 设置几何体
        /// </summary>
        public static void SetCamgeom(NXOpen.UF.CamGeomType camGeomType, NXOpen.Tag operTag, List<NXOpen.Tag> cutAreaGeometryTags)
        {
            if (cutAreaGeometryTags.Count <= 0) return;
            var appDatas = new List<NXOpen.UF.UFCamgeom.AppData>();
            cutAreaGeometryTags.ForEach(u =>
            {
                var appData = new NXOpen.UF.UFCamgeom.AppData();
                appData.has_stock = 0;
                appData.has_cut_stock = 0;
                appData.has_tolerances = 0;
                appData.has_feedrate = 0;
                appData.has_offset = 0;
                appData.has_avoidance_type = 0;
                appData.offset = 0.1;
                appDatas.Add(appData);
            });
            ufSession.Camgeom.DeleteGeometry(operTag, camGeomType);
            ufSession.Camgeom.AppendItems(operTag, camGeomType, cutAreaGeometryTags.Count, cutAreaGeometryTags.ToArray(), appDatas.ToArray());
        }

        /// <summary>
        /// 路径生成
        /// </summary>
        public static string PathGenerate(NXOpen.Tag operTag)
        {
            string generated = string.Empty;
            var mark = Snap.Globals.SetUndoMark(Snap.Globals.MarkVisibility.Invisible, "PathGenerate");
            try
            {
                PathGenerate(new List<NXOpen.Tag> { operTag });
            }
            catch(Exception ex)
            {
                ufSession.Obj.AskName(operTag, out generated);
                generated = string.Format("{0}:{1}", generated, ex.Message);
            }
            Snap.Globals.DeleteUndoMark(mark, "PathGenerate");
            return generated;
        }

        /// <summary>
        /// 路径生成
        /// </summary>
        public static void PathGenerate(List<NXOpen.Tag> operTags)
        {
            bool generated;
            operTags.ForEach(u =>
            {
                ufSession.Param.Generate(u, out generated);
            });
        }

        /// <summary>
        /// 显示刀具路径
        /// </summary>
        public static void ShowCutterPath(NXOpen.Tag operTag)
        {
            ufSession.Param.ReplayPath(operTag);
        }

        /// <summary>
        /// 设置参考刀具
        /// </summary>
        public static void SetReferenceCutter(NXOpen.Tag operTag,NXOpen.Tag refCutterTag)
        {
            ufSession.Oper.SetRefCutter(operTag, refCutterTag);
        }


        /// <summary>
        /// 显示编辑的对话框
        /// </summary>
        public static void ShowEditDialog(NXOpen.Tag tag, Action action = null)
        {
            int dialogResponse;
            ufSession.UiParam.EditObject(tag, out dialogResponse);
            if (dialogResponse == NXOpen.UF.UFConstants.UF_UI_APPLY || dialogResponse == NXOpen.UF.UFConstants.UF_UI_OK)
            {
                if (action != null)
                {
                    action();
                }
            }
        }

        /// <summary>
        /// 设置边界
        /// </summary>
        public static void SetBoundary(Snap.Position origin, NXOpen.Tag faceTag, NXOpen.UF.CamGeomType camGeomType, NXOpen.Tag operTag, NXOpen.UF.CamMaterialSide materialSide)
        {
            SetBoundaryByFace(faceTag, camGeomType, operTag, materialSide);
            IntPtr[] Boundaries;
            int count;
            ufSession.Cambnd.AskBoundaries(operTag, camGeomType, out count, out Boundaries);
            for (int i = 0; i < count; i++)
            {
                NXOpen.Matrix3x3 identity = Snap.Orientation.Identity;
                ufSession.Cambnd.SetBoundaryPlane(Boundaries[i], origin.Array, new double[]{
                    identity.Xx, identity.Xy, identity.Xz
                    , identity.Yx, identity.Yy, identity.Yz
                    , identity.Zx, identity.Zy, identity.Zz });
            }   
        }

        /// <summary>
        /// 获取轮廓线
        /// </summary>
        public static void GetOutlineCurve(
            Snap.NX.Face face
            , out List<NXOpen.Tag> peripheral
            , out List<List<NXOpen.Tag>> innerCircumference
            )
        {
            var tolerance = 0.1;
            Tag[] tagArray;
            ufSession.Modl.AskFaceEdges(face.NXOpenTag, out tagArray);
            var edges = Enumerable.Select(tagArray, u => Snap.NX.Edge.Wrap(u)).ToList();
            peripheral = new List<Tag>();
            innerCircumference = new List<List<Tag>>();
            while (edges.Count > 0)
            {
                var edge = edges.FirstOrDefault();
                bool isHas = false;
                foreach (var item in innerCircumference)
                {
                    isHas = item.Where(u => Snap.Compute.Distance(edge, Snap.NX.NXObject.Wrap(u)) <= tolerance).Count() > 0;
                    if (isHas)
                    {
                        item.Add(edge.NXOpenTag);
                        break;
                    }
                }
                if (!isHas)
                {
                    var tmpEdges = edges.Where(u => Snap.Compute.Distance(edge, u) <= tolerance);
                    tmpEdges.ToList().ForEach(u => {
                        edges.Remove(u);
                        edges.Insert(0, u);
                    });
                    innerCircumference.Add(new List<Tag> { edge.NXOpenTag });
                }
                edges.Remove(edge);
            }

            //排序
            Func<List<Tag>, List<Tag>> func = (t) =>
            {
                var result = new List<Tag>();
                //var tags = t.ToList();
                //while (tags.Count > 0)
                //{
                //    var edge = tags.FirstOrDefault();
                //    var tmpEdges = tags.Take(1).Where(u => Snap.Compute.Distance(Snap.NX.NXObject.Wrap(edge), Snap.NX.NXObject.Wrap(u)) <= SnapEx.Helper.Tolerance);
                //    tmpEdges.ToList().ForEach(u =>
                //    {
                //        tags.Remove(u);
                //        tags.Insert(0, u);
                //    });
                //    result.Add(edge);
                //    tags.Remove(edge);
                //}
                tagArray.ToList().ForEach(u =>
                {
                    if (t.Where(m => m == u).Count() > 0)
                    {
                        result.Add(u);
                    }
                });
                return result;
            };

            for (int i = 0; i < innerCircumference.Count; i++)
            {
                var result = func(innerCircumference[i]);
                innerCircumference[i] = result;
            }

            peripheral = innerCircumference.OrderByDescending(u => Snap.Compute.Distance(face.GetCenterPointEx(), Snap.NX.NXObject.Wrap(u[0]))).FirstOrDefault();
            innerCircumference.Remove(peripheral);
        }


        /// <summary>
        /// 删除边界
        /// </summary>
        public static void DeleteBoundaries(NXOpen.Tag operTag, NXOpen.UF.CamGeomType camGeomType)
        {
            ufSession.Cambnd.DeleteBoundaries(operTag, camGeomType);
        }

        /// <summary>
        /// 修剪边界
        /// </summary>
        public static void SetBoundaryByCurves(List<NXOpen.Tag> curves,NXOpen.UF.CamGeomType camGeomType, NXOpen.Tag operTag, NXOpen.UF.CamMaterialSide materialSide)
        {
            var boundary_data = new NXOpen.UF.UFCambnd.BoundaryData();
            boundary_data.boundary_type = NXOpen.UF.CamBoundaryType.CamBoundaryTypeClosed;
            boundary_data.plane_type = 1;
            boundary_data.origin = new double[] { 0, 0, 0 };
            boundary_data.matrix = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            boundary_data.material_side = materialSide;
            boundary_data.ignore_holes = 0;
            boundary_data.ignore_islands = 0;
            boundary_data.ignore_chamfers = 0;
            boundary_data.app_data = null;

            var appDatas = new List<NXOpen.UF.UFCambnd.AppData>();
            curves.ForEach(u => {
                var app_data_list = new NXOpen.UF.UFCambnd.AppData();
                /* Set flags. */
                app_data_list.has_stock = 1;
                app_data_list.has_tolerances = 0;
                app_data_list.has_feedrate = 0;
                app_data_list.has_blank_distance = 0;
                app_data_list.has_tool_position = 0;

                /* Set values. */
                app_data_list.stock = 0.1;
                app_data_list.tolerances = new double[] { 0.1, 0.1 };
                app_data_list.feedrate_unit = NXOpen.UF.CamFeedrateUnit.CamFeedrateUnitPerMinute;
                app_data_list.feedrate_value = 0.1;
                app_data_list.blank_distance = 0.0;
                app_data_list.tool_position = NXOpen.UF.CamToolPosition.CamToolPositionTanto;
                appDatas.Add(app_data_list);
            });
            ufSession.Cambnd.AppendBndFromCurve(operTag, camGeomType, curves.Count, curves.ToArray(), ref boundary_data, appDatas.ToArray());
        }

        /// <summary>
        /// 设置面边界
        /// </summary>
        public static void SetBoundaryByFace(NXOpen.Tag faceTag,NXOpen.UF.CamGeomType camGeomType, NXOpen.Tag operTag, NXOpen.UF.CamMaterialSide materialSide)
        {
            var boundary_data = new NXOpen.UF.UFCambnd.BoundaryData();
            boundary_data.boundary_type = NXOpen.UF.CamBoundaryType.CamBoundaryTypeClosed;
            boundary_data.plane_type = 1;
            boundary_data.material_side = materialSide;
            boundary_data.ignore_holes = 0;
            boundary_data.ignore_islands = 0;
            boundary_data.ignore_chamfers = 0;
            boundary_data.app_data = new NXOpen.UF.UFCambnd.AppData[] { };
            ufSession.Cambnd.AppendBndFromFace(operTag, camGeomType, faceTag, ref boundary_data);
        }

        /// <summary>
        /// 显示信息窗体
        /// </summary>
        public static void ShowInfoWindow(string msg, bool isClear = false, int type = 0)
        {
            if (CSharpProxy.ProxyObject.Instance == null)
            {
                if (isClear)
                {
                    Snap.InfoWindow.Clear();
                }
                else
                {
                    Snap.InfoWindow.WriteLine(msg);
                }
            }
            else
            {
                CSharpProxy.ProxyObject.Instance.ShowMsg(msg, type);
            }
        }

        public static void ShowMsg(string msg, int type = 0)
        {
            if (CSharpProxy.ProxyObject.Instance == null)
            {
                NXOpen.UF.UFSession.GetUFSession().Ui.SetStatus(msg);
            }
            else
            {
                CSharpProxy.ProxyObject.Instance.ShowMsg(msg, type);
            }
        }
    }

    public class AUTOCAM_TYPE
    {
        public const string mill_planar = "mill_planar";
    }

    public class AUTOCAM_ROOTNAME
    {
        public const string GEOM_EACT = "GEOM_EACT";
        public const string WORKPIECE_EACT = "WORKPIECE_EACT";
        public const string PROGRAM_EACT = "PROGRAM_EACT";
    }

    public class AUTOCAM_SUBTYPE
    {
        public const string MCS = "MCS";
        public const string WORKPIECE = "WORKPIECE";
        public const string PROGRAM = "PROGRAM";
        public const string MILL = "MILL";
    }

    /// <summary>
    /// 加工方案
    /// </summary>
    public enum E_CamScheme
    {
        /// <summary>
        /// 简易方案
        /// </summary>
        SIMPLE,
        FIRST
    }
}
