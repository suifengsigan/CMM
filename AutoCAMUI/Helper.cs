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
            ufSession.UF.SetVariable("EACT_AUTOCAM_ELE_TEMPLATEPRT_DIR", curTemplatePrtDir);
            var eact_cam_general_optFile = System.IO.Path.Combine(curTemplateSetDir, "eact_cam_general.opt");
            if (System.IO.File.Exists(optFile))
            {
                var optFileInfo = System.IO.File.ReadAllText(optFile);
                StringBuilder str = new StringBuilder();
                str.Append(optFileInfo);
                System.IO.Directory.GetFiles(curTemplatePrtDir).ToList().ForEach(u => {
                    string tempV = "${EACT_AUTOCAM_ELE_TEMPLATEPRT_DIR}" + System.IO.Path.GetFileName(u);
                    if (!optFileInfo.Contains(tempV))
                    {
                        str.AppendLine();
                        str.AppendLine(tempV);
                    }
                });

                System.IO.File.WriteAllText(eact_cam_general_optFile, str.ToString());
            }

            ufSession.Cam.ReinitOpt(eact_cam_general_optFile);
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

        public static void AutoCAM(ElecManage.Electrode ele)
        {
            var camConfig = new CAMConfig();
            var body = ele.ElecBody;
            var basePos = ele.GetElecBasePos();
            var eleInfo = ele.GetElectrodeInfo();
            var bodyBox = body.AcsToWcsBox3d(new Snap.Orientation(-ele.BaseFace.GetFaceDirection()));
            var autoBlankOffset = new double[] { 2, 2, 2, 2, 2, 0 };
            var safeDistance = 10;

            //分析面
            var faces = ele.ElecBody.Faces;
            double judgeValue = 15;
            var camFaces = new List<CAMFace>();
            ele.ElecHeadFaces.ForEach(u => {
                camFaces.Add(new CAMFace { FaceTag = u.NXOpenTag, DraftAngle = u.GetDraftAngle() });
            });

            //基准面
            var allBaseFaces = faces.Where(u => camFaces.FirstOrDefault(m => m.FaceTag == u.NXOpenTag) == null).ToList();
            //垂直面
            var verticalFaces = camFaces.Where(u => u.DraftAngle == 0 && u.GetSnapFace().ObjectSubType == Snap.NX.ObjectTypes.SubType.FacePlane).ToList();
            //水平面
            var horizontalFaces = camFaces.Where(u => u.DraftAngle == 90 && u.GetSnapFace().ObjectSubType == Snap.NX.ObjectTypes.SubType.FacePlane).ToList();
            //平缓面（等高面）
            var gentleFaces = camFaces.Where(u =>
            (u.DraftAngle >= judgeValue && u.DraftAngle < 90)
            ||
            (u.DraftAngle == 90 && u.GetSnapFace().ObjectSubType != Snap.NX.ObjectTypes.SubType.FacePlane)
            ).ToList();
            //陡峭面
            var steepFaces = camFaces.Where(u =>
            (u.DraftAngle < judgeValue && u.DraftAngle > 0)
            ||
            (u.DraftAngle == 0 && u.GetSnapFace().ObjectSubType != Snap.NX.ObjectTypes.SubType.FacePlane)
            ).ToList();
            //倒扣面
            var buttonedFaces = camFaces.Where(u => u.DraftAngle < 0).ToList();
            //非平面
            var nonPlanefaces = ele.ElecHeadFaces.Where(u => u.ObjectSubType != Snap.NX.ObjectTypes.SubType.FacePlane).ToList();

            //设置基准面颜色
            allBaseFaces.ForEach(u => {
                CAMFace.SetColor(camConfig.BaseFaceColor,u.NXOpenTag);
            });
            //设置垂直面颜色
            verticalFaces.ForEach(u => {
                u.SetColor(camConfig.VerticalPlaneColor);
            });
            //设置水平面颜色
            horizontalFaces.ForEach(u => {
                u.SetColor(camConfig.HorizontalPlaneColor);
            });
            //设置平缓面颜色
            gentleFaces.ForEach(u => {
                u.SetColor(camConfig.GentlePlaneColor);
            });
            //设置陡峭面颜色
            steepFaces.ForEach(u => {
                u.SetColor(camConfig.CurveSurfaceColor);
            });
            //倒扣面
            buttonedFaces.ForEach(u => {
                u.SetColor(camConfig.ButtonedFaceColor);
            });

            //判断加工方案
            var camScheme = E_CamScheme.SIMPLE;
            if (gentleFaces.Count > 0 || steepFaces.Count > 0)
            {
                camScheme = E_CamScheme.FIRST;
            }

            //几何组根节点
            NXOpen.Tag geometryGroupRootTag;

            //程序组根节点
            NXOpen.Tag orderGroupRootTag;

            //刀具组根节点
            NXOpen.Tag cutterGroupRootTag;

            //方法组根节点
            NXOpen.Tag methodGroupRootTag;

            //几何体组
            NXOpen.Tag workGeometryGroupTag;


            //TODO 初始化对象
            NXOpen.Tag setup_tag;
            ufSession.Setup.AskSetup(out setup_tag);
            ufSession.Setup.AskGeomRoot(setup_tag, out geometryGroupRootTag);
            ufSession.Setup.AskProgramRoot(setup_tag, out orderGroupRootTag);
            ufSession.Setup.AskMctRoot(setup_tag, out cutterGroupRootTag);
            ufSession.Setup.AskMthdRoot(setup_tag, out methodGroupRootTag);

            //TODO 创建坐标系和几何体
            //加工坐标系
            NXOpen.Tag workMcsGroupTag;
            ufSession.Ncgeom.Create(AUTOCAM_TYPE.mill_planar, AUTOCAM_SUBTYPE.MCS, out workMcsGroupTag);
            ufSession.Obj.SetName(workMcsGroupTag, AUTOCAM_ROOTNAME.GEOM_EACT);
            ufSession.Ncgroup.AcceptMember(geometryGroupRootTag, workMcsGroupTag);

            //TODO 设置安全平面
            var normal = new Snap.Vector(0, 0, 1);
            var origin = new Snap.Position((bodyBox.MinX+bodyBox.MaxX)/2, (bodyBox.MinY + bodyBox.MaxY) / 2, bodyBox.MaxZ + safeDistance);
            ufSession.Cam.SetClearPlaneData(workMcsGroupTag, origin.Array, normal.Array);

            //TODO 创建几何体
            ufSession.Ncgeom.Create(AUTOCAM_TYPE.mill_planar, AUTOCAM_SUBTYPE.WORKPIECE, out workGeometryGroupTag);
            ufSession.Obj.SetName(workGeometryGroupTag, AUTOCAM_ROOTNAME.WORKPIECE_EACT);
            ufSession.Ncgroup.AcceptMember(workMcsGroupTag, workGeometryGroupTag);

            //TODO 创建程序
            NXOpen.Tag programGroupTag;
            ufSession.Ncprog.Create(AUTOCAM_TYPE.mill_planar, AUTOCAM_SUBTYPE.PROGRAM, out programGroupTag);
            ufSession.Obj.SetName(programGroupTag, eleInfo.Elec_Name);
            ufSession.Ncgroup.AcceptMember(orderGroupRootTag, programGroupTag);

            //TODO 添加Body作为工作几何体
            SetCamgeom(NXOpen.UF.CamGeomType.CamPart, workGeometryGroupTag, new List<NXOpen.Tag> { body.NXOpenTag });

            //TODO 设置毛坯为自动块
            ufSession.Cam.SetAutoBlank(workGeometryGroupTag, NXOpen.UF.UFCam.BlankGeomType.AutoBlockType, autoBlankOffset);

            //TODO 创建刀具
            var cutters = new List<CAMCutter>();
            //TODO 通过配置创建刀具
            var D10_R = new CAMCutter();
            D10_R.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            D10_R.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            D10_R.CutterName = "D10_R";
            D10_R.TL_DIAMETER = 10;
            D10_R.TL_COR1_RAD = 0;
            D10_R.TL_HEIGHT = 70;
            D10_R.TL_FLUTE_LN = 45;
            D10_R.TL_NUMBER = 3;
            D10_R.Speed = 5000;
            D10_R.FeedRate = 3000;
            cutters.Add(D10_R);

            var D4_R = new CAMCutter();
            D4_R.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            D4_R.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            D4_R.CutterName = "D4_R";
            D4_R.TL_DIAMETER = 4;
            D4_R.TL_COR1_RAD = 0;
            D4_R.TL_HEIGHT = 50;
            D4_R.TL_FLUTE_LN = 30;
            D4_R.TL_NUMBER = 6;
            D4_R.Speed = 6500;
            D4_R.FeedRate = 1500;
            cutters.Add(D4_R);

            var D10 = new CAMCutter();
            D10.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            D10.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            D10.CutterName = "D10";
            D10.TL_DIAMETER = 10;
            D10.TL_COR1_RAD = 0;
            D10.TL_HEIGHT = 70;
            D10.TL_FLUTE_LN = 50;
            D10.TL_NUMBER = 9;
            D10.Speed = 5500;
            D10.FeedRate = 3000;
            cutters.Add(D10);

            var D8R05 = new CAMCutter();
            D8R05.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            D8R05.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            D8R05.CutterName = "D8R0.5";
            D8R05.TL_DIAMETER = 8;
            D8R05.TL_COR1_RAD = 0.5;
            D8R05.TL_HEIGHT = 70;
            D8R05.TL_FLUTE_LN = 50;
            D8R05.TL_NUMBER = 19;
            D8R05.Speed = 6000;
            D8R05.FeedRate = 3000;
            cutters.Add(D8R05);

            var D03 = new CAMCutter();
            D03.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            D03.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            D03.CutterName = "D0.3";
            D03.TL_DIAMETER = 0.3;
            D03.TL_COR1_RAD = 0;
            D03.TL_HEIGHT = 50;
            D03.TL_FLUTE_LN = 3;
            D03.TL_NUMBER = 21;
            D03.Speed = 8000;
            D03.FeedRate = 500;
            cutters.Add(D03);

            var R2 = new CAMCutter();
            R2.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            R2.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            R2.CutterName = "R2";
            R2.TL_DIAMETER = 4;
            R2.TL_COR1_RAD = 2;
            R2.TL_HEIGHT = 50;
            R2.TL_FLUTE_LN = 30;
            R2.TL_NUMBER = 24;
            R2.Speed = 7000;
            R2.FeedRate = 2000;
            cutters.Add(R2);

            var R1 = new CAMCutter();
            R1.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            R1.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            R1.CutterName = "R1";
            R1.TL_DIAMETER = 2;
            R1.TL_COR1_RAD = 1;
            R1.TL_HEIGHT = 50;
            R1.TL_FLUTE_LN = 8;
            R1.TL_NUMBER = 26;
            R1.Speed = 8000;
            R1.FeedRate = 1000;
            cutters.Add(R1);

            var D05 = new CAMCutter();
            D05.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            D05.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            D05.CutterName = "D0.5";
            D05.TL_DIAMETER = 0.5;
            D05.TL_COR1_RAD = 0;
            D05.TL_HEIGHT = 50;
            D05.TL_FLUTE_LN = 3;
            D05.TL_NUMBER = 18;
            D05.Speed = 10000;
            D05.FeedRate = 400;
            cutters.Add(D05);

            var KEZI = new CAMCutter();
            KEZI.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            KEZI.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            KEZI.CutterName = "KEZI";
            KEZI.TL_DIAMETER = 0.6;
            KEZI.TL_COR1_RAD = 0.3;
            KEZI.TL_HEIGHT = 70;
            KEZI.TL_FLUTE_LN = 20;
            KEZI.TL_NUMBER = 20;
            KEZI.Speed = 7000;
            KEZI.FeedRate = 500;
            cutters.Add(KEZI);

            CreateCutter(cutters, cutterGroupRootTag);


            var camOpers = new List<AutoCAMUI.CAMOper>();
            //杀顶
            var FACE_MILLING_TOP_0 = new WsqAutoCAM_FACE_MILLING_TOP_Oper();
            FACE_MILLING_TOP_0.CreateOper(workGeometryGroupTag, programGroupTag, methodGroupRootTag, D10_R);
            FACE_MILLING_TOP_0.SetCutDepth(0.3);
            FACE_MILLING_TOP_0.SetBoundary(ele);
            camOpers.Add(FACE_MILLING_TOP_0);

            //电极头部开粗（开粗）
            var CAM_CAVITY_MILL_C_0 = new WsqAutoCAM_CAVITY_MILL_C_Oper();
            CAM_CAVITY_MILL_C_0.CreateOper(workGeometryGroupTag, programGroupTag, methodGroupRootTag, D10_R);
            CAM_CAVITY_MILL_C_0.SetCutDepth(0.3);
            CAM_CAVITY_MILL_C_0.SetCutLevels(ele.BaseFace.NXOpenTag);
            CAM_CAVITY_MILL_C_0.SetRegionStartPoints(ele);
            camOpers.Add(CAM_CAVITY_MILL_C_0);

            //基准台开粗（2d开粗基准）
            var CAVITY_PLANAR_MILL_0 = new WsqAutoCAM_CAVITY_PLANAR_MILL_Oper();
            CAVITY_PLANAR_MILL_0.CreateOper(workGeometryGroupTag, programGroupTag, methodGroupRootTag, D10_R);
            CAVITY_PLANAR_MILL_0.SetCutDepth(0.3);
            CAVITY_PLANAR_MILL_0.SetBoundaryAndCutFloor(ele);
            camOpers.Add(CAVITY_PLANAR_MILL_0);

            switch (camScheme)
            {
                case E_CamScheme.FIRST:
                    {
                        //残料开粗
                        var CAVITY_MILL_REF_0 = new WsqAutoCAM_CAVITY_MILL_REF_Oper();
                        CAVITY_MILL_REF_0.CreateOper(workGeometryGroupTag, programGroupTag, methodGroupRootTag, D4_R);
                        CAVITY_MILL_REF_0.SetReferenceCutter(D10_R);
                        CAVITY_MILL_REF_0.SetCutDepth(0.15);
                        camOpers.Add(CAVITY_MILL_REF_0);
                        break;
                    }
            }

            //基准平面
            var FACE_MILLING_BASE_0 = new WsqAutoCAM_FACE_MILLING_BASE_Oper();
            FACE_MILLING_BASE_0.CreateOper(workGeometryGroupTag, programGroupTag, methodGroupRootTag, D10);
            FACE_MILLING_BASE_0.SetCutDepth(0.3);
            FACE_MILLING_BASE_0.SetBoundary(ele);
            camOpers.Add(FACE_MILLING_BASE_0);

            //平面
            var FACE_MILLING_0 = new WsqAutoCAM_FACE_MILLING_Oper();
            FACE_MILLING_0.CreateOper(workGeometryGroupTag, programGroupTag, methodGroupRootTag, D10);
            FACE_MILLING_0.SetCutDepth(0.1);
            FACE_MILLING_0.SetBoundary(horizontalFaces);
            camOpers.Add(FACE_MILLING_0);

            //基准侧面
            var PLANAR_MILL_BASE_0 = new WsqAutoCAM_PLANAR_MILL_BASE_Oper();
            PLANAR_MILL_BASE_0.CreateOper(workGeometryGroupTag, programGroupTag, methodGroupRootTag, D10);
            PLANAR_MILL_BASE_0.SetCutDepth(20);
            PLANAR_MILL_BASE_0.SetBoundaryAndCutFloor(ele);
            camOpers.Add(PLANAR_MILL_BASE_0);

            //直身位
            var PLANAR_MILL_0 = new WsqAutoCAM_PLANAR_MILL_Oper();
            PLANAR_MILL_0.CreateOper(workGeometryGroupTag, programGroupTag, methodGroupRootTag, D10);
            PLANAR_MILL_0.SetCutDepth(20);
            PLANAR_MILL_0.SetBoundaryAndCutFloor(ele);
            camOpers.Add(PLANAR_MILL_0);

            switch (camScheme)
            {
                case E_CamScheme.FIRST:
                    {
                        //等高角度
                        var ZLEVEL_PROFILE_STEEP_0 = new WsqAutoCAM_ZLEVEL_PROFILE_STEEP_Oper();
                        ZLEVEL_PROFILE_STEEP_0.CreateOper(workGeometryGroupTag, programGroupTag, methodGroupRootTag, D8R05);
                        ZLEVEL_PROFILE_STEEP_0.SetMillArea(ele);
                        ZLEVEL_PROFILE_STEEP_0.SetCutDepth(0.15);
                        camOpers.Add(ZLEVEL_PROFILE_STEEP_0);

                        //等高清角
                        var ZLEVEL_CORNER_0 = new WsqAutoCAM_ZLEVEL_CORNER_Oper();
                        ZLEVEL_CORNER_0.CreateOper(workGeometryGroupTag, programGroupTag, methodGroupRootTag, D03);
                        ZLEVEL_CORNER_0.SetMillArea(ele);
                        ZLEVEL_CORNER_0.SetCutDepth(0.06);
                        camOpers.Add(ZLEVEL_CORNER_0);

                        //曲面角度
                        var CONTOUR_AREA_NON_STEEP_0 = new WsqAutoCAM_CONTOUR_AREA_NON_STEEP_Oper();
                        CONTOUR_AREA_NON_STEEP_0.CreateOper(workGeometryGroupTag, programGroupTag, methodGroupRootTag, R2);
                        CONTOUR_AREA_NON_STEEP_0.SetMillArea(ele);
                        CONTOUR_AREA_NON_STEEP_0.SetCutDepth(0.1);
                        camOpers.Add(CONTOUR_AREA_NON_STEEP_0);

                        //曲面清角
                        var FLOWCUT_REF_TOOL_0 = new WsqAutoCAM_FLOWCUT_REF_TOOL_Oper();
                        FLOWCUT_REF_TOOL_0.CreateOper(workGeometryGroupTag, programGroupTag, methodGroupRootTag, R1);
                        FLOWCUT_REF_TOOL_0.SetMillArea(ele);
                        FLOWCUT_REF_TOOL_0.SetCutDepth(0.04);
                        camOpers.Add(FLOWCUT_REF_TOOL_0);

                        //平面清角
                        var FACE_MILLING_CORNER_0 = new WsqAutoCAM_FACE_MILLING_CORNER_Oper();
                        FACE_MILLING_CORNER_0.CreateOper(workGeometryGroupTag, programGroupTag, methodGroupRootTag, D05);
                        FACE_MILLING_CORNER_0.SetMillArea(Enumerable.Select(horizontalFaces,u=>u.FaceTag).ToList());
                        FACE_MILLING_CORNER_0.SetCutDepth(0.06);
                        camOpers.Add(FACE_MILLING_CORNER_0);

                        break;
                    }
                case E_CamScheme.SIMPLE:
                    {
                        break;
                    }
            }

            //刻字
            var CONTOUR_TEXT_Oper_0 = new WsqAutoCAM_CONTOUR_TEXT_Oper();
            CONTOUR_TEXT_Oper_0.CreateOper(workGeometryGroupTag, programGroupTag, methodGroupRootTag, KEZI);
            CONTOUR_TEXT_Oper_0.SetText(ele.ElecBody.Name,ele);
            camOpers.Add(CONTOUR_TEXT_Oper_0);

            #region old


            ////曲面角度
            //var camOper6 = new AutoCAMUI.CAMOper();
            //camOper6.AUTOCAM_TYPE = ELECTRODETEMPLATETYPENAME;
            //camOper6.AUTOCAM_SUBTYPE = "CONTOUR_AREA_NON_STEEP";
            //var cutter6 = new CAMCutter();
            //cutter6.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            //cutter6.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            //cutter6.CutterName = "R2";
            //cutter6.TL_DIAMETER = 4;
            //cutter6.TL_COR1_RAD = 2;
            //cutter6.TL_HEIGHT = 50;
            //cutter6.TL_FLUTE_LN = 30;
            //CreateCutter(new List<CAMCutter> { cutter6 }, cutterGroupRootTag);
            //cutters.Add(cutter6);
            //camOper6.CAMCutter = cutter6;
            //camOper6.WorkGeometryGroup = workGeometryGroupTag;
            //camOper6.ProgramGroup = programGroupTag;
            //camOper6.MethodGroupRoot = methodGroupRootTag;
            //camOper6.CreateOper();
            //SetPartStockAndFloorStock(camOper6.OperTag, 0, 0);
            //SetMillArea(NXOpen.UF.CamGeomType.CamCutArea, camOper6.OperTag, Enumerable.Select(ele.ElecHeadFaces, u => u.NXOpenTag).ToList());
            ////ufSession.Param.SetDoubleValue(camOper6.OperTag, NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_GLOBAL_CUT_DEPTH, 0.1);
            //ufSession.Obj.SetName(camOper6.OperTag, camOper6.AUTOCAM_SUBTYPE + "_0");
            //camOpers.Add(camOper6);

            ////曲面清角
            //var camOper7 = new AutoCAMUI.CAMOper();
            //camOper7.AUTOCAM_TYPE = ELECTRODETEMPLATETYPENAME;
            //camOper7.AUTOCAM_SUBTYPE = "FLOWCUT_REF_TOOL";
            //var cutter7 = new CAMCutter();
            //cutter7.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            //cutter7.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            //cutter7.CutterName = "R1";
            //cutter7.TL_DIAMETER = 2;
            //cutter7.TL_COR1_RAD = 1;
            //cutter7.TL_HEIGHT = 50;
            //cutter7.TL_FLUTE_LN = 12;
            //CreateCutter(new List<CAMCutter> { cutter7 }, cutterGroupRootTag);
            //cutters.Add(cutter7);
            //camOper7.CAMCutter = cutter7;
            //camOper7.WorkGeometryGroup = workGeometryGroupTag;
            //camOper7.ProgramGroup = programGroupTag;
            //camOper7.MethodGroupRoot = methodGroupRootTag;
            //camOper7.CreateOper();
            //SetPartStockAndFloorStock(camOper7.OperTag, 0, 0);
            //SetMillArea(NXOpen.UF.CamGeomType.CamCutArea, camOper7.OperTag, Enumerable.Select(nonPlanefaces, u => u.NXOpenTag).ToList());
            ////ufSession.Param.SetDoubleValue(camOper7.OperTag, NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_GLOBAL_CUT_DEPTH, 0.04);
            //ufSession.Obj.SetName(camOper7.OperTag, camOper7.AUTOCAM_SUBTYPE + "_0");
            //camOpers.Add(camOper7);

            ////等宽精铣曲面
            //var camOper2 = new AutoCAMUI.CAMOper();
            //camOper2.AUTOCAM_TYPE = ELECTRODETEMPLATETYPENAME;
            //camOper2.AUTOCAM_SUBTYPE = "CONTOUR_AREA";
            //var cutter2 = new CAMCutter();
            //cutter2.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            //cutter2.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            //cutter2.CutterName = "R3";
            //cutter2.TL_DIAMETER = 6;
            //cutter2.TL_COR1_RAD = 3;
            //cutter2.TL_HEIGHT = 50;
            //cutter2.TL_FLUTE_LN = 35;
            //CreateCutter(new List<CAMCutter> { cutter2 }, cutterGroupRootTag);
            //cutters.Add(cutter2);
            //camOper2.CAMCutter = cutter2;
            //camOper2.WorkGeometryGroup = workGeometryGroupTag;
            //camOper2.ProgramGroup = programGroupTag;
            //camOper2.MethodGroupRoot = methodGroupRootTag;
            //camOper2.CreateOper();
            //ufSession.Param.SetIntValue(camOper2.OperTag, NXOpen.UF.UFConstants.UF_PARAM_STEPOVER_TYPE, 1);
            //ufSession.Param.SetDoubleValue(camOper2.OperTag, NXOpen.UF.UFConstants.UF_PARAM_STEPOVER_DIST, 0.1);
            //var tempfaces = ele.ElecHeadFaces.Where(u => u.ObjectSubType != Snap.NX.ObjectTypes.SubType.FacePlane).ToList();
            //SetMillArea(NXOpen.UF.CamGeomType.CamCutArea, camOper2.OperTag, Enumerable.Select(tempfaces, u => u.NXOpenTag).ToList());
            //camOpers.Add(camOper2);

            #endregion


            Snap.InfoWindow.Clear();
            camOpers.ForEach(u => {
                var exMsg = PathGenerate(u.OperTag);
                if (!string.IsNullOrEmpty(exMsg))
                {
                    Snap.InfoWindow.WriteLine(exMsg);
                }
            });
           
            camOpers.ForEach(u => {
                string name;
                ufSession.Obj.AskName(u.OperTag, out name);
                Snap.InfoWindow.WriteLine(string.Format("{0}:{1}", name, IsPathGouged(u.OperTag) ? "过切" : "未过切"));
            });

            //获取后处理器列表
            string[] names;
            int count;
            ufSession.Cam.OptAskPostNames(out count, out names);
            var postName = "铜电极-自动换刀";
            var extension = "nc";

            var path = System.AppDomain.CurrentDomain.BaseDirectory;
            path = System.IO.Path.Combine(path, "Temp");
            path = System.IO.Path.Combine(path, "EACTCNCFILE");
            if (System.IO.Directory.Exists(path))
            {
                System.IO.Directory.Delete(path);
            }

            System.IO.Directory.CreateDirectory(path);

            camOpers.ForEach(u => {
                string name;
                ufSession.Obj.AskName(u.OperTag, out name);
                //TODO 判断是否有无刀路
                try
                {
                    //生成nc程式
                    ufSession.Setup.GenerateProgram(
                        Snap.Globals.WorkPart.NXOpenPart.CAMSetup.Tag,
                       u.OperTag
                        , postName
                        , System.IO.Path.Combine(path, string.Format(@"{0}_{1}_{2}.{3}", ele.ElecBody.Name, name, camOpers.IndexOf(u), extension))
                        , NXOpen.UF.UFSetup.OutputUnits.OutputUnitsOutputDefined
                        );
                }
                catch (Exception ex)
                {
                    Snap.InfoWindow.WriteLine(string.Format("{0}:{1}", name, ex.Message));
                }
            });

            //生成nc程式
            ufSession.Setup.GenerateProgram(
                Snap.Globals.WorkPart.NXOpenPart.CAMSetup.Tag,
                programGroupTag
                , postName
                , System.IO.Path.Combine(path, string.Format(@"{0}.{1}", ele.ElecBody.Name, extension))
                , NXOpen.UF.UFSetup.OutputUnits.OutputUnitsOutputDefined
                );
        }

        public enum EACT_FeedUnit
        {
            FeedNone,
            FeedPerMinute,
            FeedPerRevolution
        }

        public struct EACT_Feedrate
        {
            public EACT_FeedUnit unit;
            public double value;
            public short color;
        }

        [System.Runtime.InteropServices.DllImport("libufun.dll", EntryPoint = "UF_PARAM_ask_subobj_ptr_value", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        internal static extern int _AskFeedRate(Tag param_tag, int param_index, out EACT_Feedrate value);

        [System.Runtime.InteropServices.DllImport("libufun.dll", EntryPoint = "UF_PARAM_set_subobj_ptr_value", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        internal static extern int _SetFeedRate(Tag param_tag, int param_index, EACT_Feedrate value);

        /// <summary>
        /// 获取进给率
        /// </summary>
        public static EACT_Feedrate AskFeedRate(NXOpen.Tag operTag, int param_index)
        {
            EACT_Feedrate result;
            NXOpen.Utilities.JAM.StartUFCall();
            int errorCode = _AskFeedRate(operTag, param_index, out result);
            NXOpen.Utilities.JAM.EndUFCall();
            return result;
        }

        /// <summary>
        /// 设置进给参数
        /// </summary>
        public static void SetCutterFeed(NXOpen.Tag operTag, int param_index, double value)
        {
            var feedRate = AskFeedRate(operTag, param_index);
            NXOpen.Utilities.JAM.StartUFCall();
            feedRate.value = value;
            int errorCode = _SetFeedRate(operTag, param_index, feedRate);
            NXOpen.Utilities.JAM.EndUFCall();
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
            SetCutterFeed(oper, NXOpen.UF.UFConstants.UF_PARAM_FEED_ENGAGE, engage);
            SetCutterFeed(oper, NXOpen.UF.UFConstants.UF_PARAM_FEED_FIRST_CUT, first_cut);
            SetCutterFeed(oper, NXOpen.UF.UFConstants.UF_PARAM_FEED_TRAVERSAL, traversal);
            SetCutterFeed(oper, NXOpen.UF.UFConstants.UF_PARAM_FEED_RETRACT, retract);
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
        public static void SetCutLevels(NXOpen.Tag operTag,NXOpen.Tag faceTag , int levelsPosition = 1)
        {
            double zLevels = Snap.NX.Face.Wrap(faceTag).BoxEx().MaxZ;
            var cut_levels = new NXOpen.UF.UFCutLevels.CutLevelsStruct();
            ufSession.CutLevels.SetRangeType(operTag, NXOpen.UF.ParamClvRangeType.ParamClvRangeUserDefined, out cut_levels);
            var cut_levels_ptr_addr = new NXOpen.UF.UFCutLevels.CutLevelsStruct[] { cut_levels };
            //ufSession.CutLevels.Load(operTag, out cut_levels_ptr_addr);
            foreach (var item in cut_levels_ptr_addr)
            {
                int num_levels = item.num_levels;
                var num = 0;
                switch (levelsPosition)
                {
                    case 1: //BottomLevel
                        {
                            num = num_levels - 1;
                            break;
                        }
                    default: //TopLevel
                        {
                            break;
                        }
                }
                ufSession.CutLevels.EditLevelUsingZ(operTag, num, zLevels, item.cut_levels[num].local_cut_depth, out cut_levels);
            }

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
                NXOpen.Tag cutterTag;
                ufSession.Cutter.Create(item.AUTOCAM_TYPE, item.AUTOCAM_SUBTYPE, out cutterTag);
                ufSession.Ncgroup.AcceptMember(cutterGroupRootTag, cutterTag);
                ufSession.Obj.SetName(cutterTag, item.CutterName);
                ufSession.Param.SetDoubleValue(cutterTag, NXOpen.UF.UFConstants.UF_PARAM_TL_DIAMETER, item.TL_DIAMETER);
                ufSession.Param.SetDoubleValue(cutterTag, NXOpen.UF.UFConstants.UF_PARAM_TL_COR1_RAD, item.TL_COR1_RAD);
                ufSession.Param.SetDoubleValue(cutterTag, NXOpen.UF.UFConstants.UF_PARAM_TL_HEIGHT, item.TL_HEIGHT);
                ufSession.Param.SetDoubleValue(cutterTag, NXOpen.UF.UFConstants.UF_PARAM_TL_FLUTE_LN,item.TL_FLUTE_LN);
                ufSession.Param.SetIntValue(cutterTag, NXOpen.UF.UFConstants.UF_PARAM_TL_NUMBER, item.TL_NUMBER);
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
                    isHas = item.Where(u => Snap.Compute.Distance(edge, Snap.NX.NXObject.Wrap(u)) <= SnapEx.Helper.Tolerance).Count() > 0;
                    if (isHas)
                    {
                        item.Add(edge.NXOpenTag);
                        break;
                    }
                }
                if (!isHas)
                {
                    var tmpEdges = edges.Where(u => Snap.Compute.Distance(edge, u) <= SnapEx.Helper.Tolerance);
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
