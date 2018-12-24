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

            //陡峭面（等高面）
            var steepFaces = camFaces.Where(u => u.DraftAngle < judgeValue && u.DraftAngle > 0).ToList();
            //非平面
            var nonPlanefaces = ele.ElecHeadFaces.Where(u => u.ObjectSubType != Snap.NX.ObjectTypes.SubType.FacePlane).ToList();
            //垂直面
            var verticalFaces = camFaces.Where(u => u.DraftAngle ==90).ToList();

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
            SetMillArea(NXOpen.UF.CamGeomType.CamPart, workGeometryGroupTag, new List<NXOpen.Tag> { body.NXOpenTag });

            //TODO 设置毛坯为自动块
            ufSession.Cam.SetAutoBlank(workGeometryGroupTag, NXOpen.UF.UFCam.BlankGeomType.AutoBlockType, autoBlankOffset);

            //TODO 创建刀具
            var cutters = new List<CAMCutter>();
            var camOpers = new List<AutoCAMUI.CAMOper>();

            //电极头部开粗
            var camOper = new AutoCAMUI.CAMOper();
            camOper.AUTOCAM_TYPE = ELECTRODETEMPLATETYPENAME;
            camOper.AUTOCAM_SUBTYPE = "CAVITY_MILL_C";
            var cutter = new CAMCutter();
            cutter.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            cutter.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL ;
            cutter.CutterName = "D10_R";
            cutter.TL_DIAMETER = 10;
            cutter.TL_COR1_RAD = 0;
            cutter.TL_HEIGHT = 70;
            cutter.TL_FLUTE_LN = 45;
            CreateCutter(new List<CAMCutter> { cutter }, cutterGroupRootTag);
            cutters.Add(cutter);
            camOper.CAMCutter = cutter;
            camOper.WorkGeometryGroup = workGeometryGroupTag;
            camOper.ProgramGroup = programGroupTag;
            camOper.MethodGroupRoot = methodGroupRootTag;
            camOper.CreateOper();
            ufSession.Param.SetDoubleValue(camOper.OperTag, NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_GLOBAL_CUT_DEPTH, 0.3);
            SetCutLevels(camOper.OperTag, ele.BaseFace.NXOpenTag);
            ufSession.Obj.SetName(camOper.OperTag, camOper.AUTOCAM_SUBTYPE + "_0");
            camOpers.Add(camOper);


            //基准台开粗
            var camOper0 = new AutoCAMUI.CAMOper();
            camOper0.AUTOCAM_TYPE = ELECTRODETEMPLATETYPENAME;
            camOper0.AUTOCAM_SUBTYPE = "CAVITY_MILL_C";
            camOper0.CAMCutter = cutter;
            camOper0.WorkGeometryGroup = workGeometryGroupTag;
            camOper0.ProgramGroup = programGroupTag;
            camOper0.MethodGroupRoot = methodGroupRootTag;
            camOper0.CreateOper();
            ufSession.Param.SetDoubleValue(camOper0.OperTag, NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_GLOBAL_CUT_DEPTH, 0.3);
            SetCutLevels(camOper0.OperTag, ele.BaseFace.NXOpenTag, 0);
            ufSession.Obj.SetName(camOper0.OperTag, camOper.AUTOCAM_SUBTYPE + "_1");
            camOpers.Add(camOper0);

            //残料开粗
            var camOper01 = new AutoCAMUI.CAMOper();
            camOper01.AUTOCAM_TYPE = ELECTRODETEMPLATETYPENAME;
            camOper01.AUTOCAM_SUBTYPE = "CAVITY_MILL_REF";
            var cutter01 = new CAMCutter();
            cutter01.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            cutter01.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            cutter01.CutterName = "D2";
            cutter01.TL_DIAMETER = 2;
            cutter01.TL_COR1_RAD = 0;
            cutter01.TL_HEIGHT = 50;
            cutter01.TL_FLUTE_LN = 12;
            CreateCutter(new List<CAMCutter> { cutter01 }, cutterGroupRootTag);
            cutters.Add(cutter01);
            camOper01.CAMCutter = cutter01;
            camOper01.WorkGeometryGroup = workGeometryGroupTag;
            camOper01.ProgramGroup = programGroupTag;
            camOper01.MethodGroupRoot = methodGroupRootTag;
            camOper01.CreateOper();
            //TODO 设置参考刀具
            SetReferenceCutter(camOper01.OperTag, cutter.CutterTag);
            ufSession.Param.SetDoubleValue(camOper01.OperTag, NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_GLOBAL_CUT_DEPTH, 0.04);
            SetCutLevels(camOper01.OperTag, ele.BaseFace.NXOpenTag);
            ufSession.Obj.SetName(camOper01.OperTag, camOper01.AUTOCAM_SUBTYPE + "_1");
            camOpers.Add(camOper01);

            //基准平面
            var camOper1 = new AutoCAMUI.CAMOper();
            camOper1.AUTOCAM_TYPE = ELECTRODETEMPLATETYPENAME;
            camOper1.AUTOCAM_SUBTYPE = "FACE_MILLING_BASE";
            var cutter3 = new CAMCutter();
            cutter3.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            cutter3.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            cutter3.CutterName = "D10";
            cutter3.TL_DIAMETER = 10;
            cutter3.TL_COR1_RAD = 0;
            cutter3.TL_HEIGHT = 70;
            cutter3.TL_FLUTE_LN = 50;
            CreateCutter(new List<CAMCutter> { cutter3 }, cutterGroupRootTag);
            cutters.Add(cutter3);
            camOper1.CAMCutter = cutter3;
            camOper1.WorkGeometryGroup = workGeometryGroupTag;
            camOper1.ProgramGroup = programGroupTag;
            camOper1.MethodGroupRoot = methodGroupRootTag;
            camOper1.CreateOper();
            SetBoundaryByFace(ele.BaseFace.NXOpenTag, NXOpen.UF.CamGeomType.CamBlank, camOper1.OperTag, NXOpen.UF.CamMaterialSide.CamMaterialSideInLeft);
            camOpers.Add(camOper1);

            //平面
            var camOper9 = new AutoCAMUI.CAMOper();
            camOper9.AUTOCAM_TYPE = ELECTRODETEMPLATETYPENAME;
            camOper9.AUTOCAM_SUBTYPE = "FACE_MILLING";
            var cutter9 = new CAMCutter();
            cutter9.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            cutter9.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            cutter9.CutterName = "D0.5";
            cutter9.TL_DIAMETER = 0.5;
            cutter9.TL_COR1_RAD = 0;
            cutter9.TL_HEIGHT = 50;
            cutter9.TL_FLUTE_LN = 4;
            CreateCutter(new List<CAMCutter> { cutter9 }, cutterGroupRootTag);
            cutters.Add(cutter9);
            camOper9.CAMCutter = cutter9;
            camOper9.WorkGeometryGroup = workGeometryGroupTag;
            camOper9.ProgramGroup = programGroupTag;
            camOper9.MethodGroupRoot = methodGroupRootTag;
            camOper9.CreateOper();
            //SetPartStockAndFloorStock(camOper8.OperTag, 0, 0);
            verticalFaces.ForEach(u => {
                SetBoundaryByFace(u.FaceTag, NXOpen.UF.CamGeomType.CamBlank, camOper9.OperTag, NXOpen.UF.CamMaterialSide.CamMaterialSideInLeft);
            });
            ufSession.Param.SetDoubleValue(camOper9.OperTag, NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_DEPTH_PER_CUT, 0.1);
            ufSession.Obj.SetName(camOper9.OperTag, camOper9.AUTOCAM_SUBTYPE + "_0");
            camOpers.Add(camOper9);

            //基准侧面
            var camOper3 = new AutoCAMUI.CAMOper();
            camOper3.AUTOCAM_TYPE = ELECTRODETEMPLATETYPENAME;
            camOper3.AUTOCAM_SUBTYPE = "PLANAR_MILL_BASE";
            camOper3.CAMCutter = cutter3;
            camOper3.WorkGeometryGroup = workGeometryGroupTag;
            camOper3.ProgramGroup = programGroupTag;
            camOper3.MethodGroupRoot = methodGroupRootTag;
            camOper3.CreateOper();
            //SetPartStockAndFloorStock(camOper3.OperTag, 0, 0);
            SetBoundary(ele.BaseFace.GetCenterPointEx(), ele.BaseFace.NXOpenTag, NXOpen.UF.CamGeomType.CamPart, camOper3.OperTag, NXOpen.UF.CamMaterialSide.CamMaterialSideInLeft);
            SetCutFloor(camOper3.OperTag, ele.TopFace.GetCenterPointEx());
            //ufSession.Param.SetIntValue(camOper3.OperTag, NXOpen.UF.UFConstants.UF_PARAM_CUT_METHOD, 5);//UF_PARAM_cut_method_profile
            //ufSession.Param.SetDoubleValue(camOper3.OperTag, NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_MAX_DEPTH, 0.3);
            ufSession.Obj.SetName(camOper3.OperTag, camOper3.AUTOCAM_SUBTYPE + "_0");
            camOpers.Add(camOper3);

            //等高角度
            var camOper4 = new AutoCAMUI.CAMOper();
            camOper4.AUTOCAM_TYPE = ELECTRODETEMPLATETYPENAME;
            camOper4.AUTOCAM_SUBTYPE = "ZLEVEL_PROFILE_STEEP";
            var cutter4 = new CAMCutter();
            cutter4.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            cutter4.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            cutter4.CutterName = "D8R0.5";
            cutter4.TL_DIAMETER = 8;
            cutter4.TL_COR1_RAD = 0.5;
            cutter4.TL_HEIGHT = 70;
            cutter4.TL_FLUTE_LN = 50;
            CreateCutter(new List<CAMCutter> { cutter4 }, cutterGroupRootTag);
            cutters.Add(cutter4);
            camOper4.CAMCutter = cutter4;
            camOper4.WorkGeometryGroup = workGeometryGroupTag;
            camOper4.ProgramGroup = programGroupTag;
            camOper4.MethodGroupRoot = methodGroupRootTag;
            camOper4.CreateOper();
            SetPartStockAndFloorStock(camOper4.OperTag, 0, 0);
            SetMillArea(NXOpen.UF.CamGeomType.CamCutArea, camOper4.OperTag, Enumerable.Select(ele.ElecHeadFaces, u => u.NXOpenTag).ToList());
            ufSession.Param.SetDoubleValue(camOper4.OperTag, NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_GLOBAL_CUT_DEPTH, 0.15);
            ufSession.Obj.SetName(camOper4.OperTag, camOper4.AUTOCAM_SUBTYPE + "_0");
            camOpers.Add(camOper4);

            //等高清角
            var camOper5 = new AutoCAMUI.CAMOper();
            camOper5.AUTOCAM_TYPE = ELECTRODETEMPLATETYPENAME;
            camOper5.AUTOCAM_SUBTYPE = "ZLEVEL_CORNER";
            var cutter5 = new CAMCutter();
            cutter5.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            cutter5.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            cutter5.CutterName = "D0.3";
            cutter5.TL_DIAMETER = 0.3;
            cutter5.TL_COR1_RAD = 0;
            cutter5.TL_HEIGHT = 50;
            cutter5.TL_FLUTE_LN = 30;
            CreateCutter(new List<CAMCutter> { cutter5 }, cutterGroupRootTag);
            cutters.Add(cutter5);
            camOper5.CAMCutter = cutter5;
            camOper5.WorkGeometryGroup = workGeometryGroupTag;
            camOper5.ProgramGroup = programGroupTag;
            camOper5.MethodGroupRoot = methodGroupRootTag;
            camOper5.CreateOper();
            SetPartStockAndFloorStock(camOper5.OperTag, 0, 0);
            SetMillArea(NXOpen.UF.CamGeomType.CamCutArea, camOper5.OperTag, Enumerable.Select(ele.ElecHeadFaces, u => u.NXOpenTag).ToList());
            ufSession.Param.SetDoubleValue(camOper5.OperTag, NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_GLOBAL_CUT_DEPTH, 0.06);
            ufSession.Obj.SetName(camOper5.OperTag, camOper5.AUTOCAM_SUBTYPE + "_0");
            camOpers.Add(camOper5);

            //曲面角度
            var camOper6 = new AutoCAMUI.CAMOper();
            camOper6.AUTOCAM_TYPE = ELECTRODETEMPLATETYPENAME;
            camOper6.AUTOCAM_SUBTYPE = "CONTOUR_AREA_NON_STEEP";
            var cutter6 = new CAMCutter();
            cutter6.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            cutter6.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            cutter6.CutterName = "R2";
            cutter6.TL_DIAMETER = 4;
            cutter6.TL_COR1_RAD = 2;
            cutter6.TL_HEIGHT = 50;
            cutter6.TL_FLUTE_LN = 30;
            CreateCutter(new List<CAMCutter> { cutter6 }, cutterGroupRootTag);
            cutters.Add(cutter6);
            camOper6.CAMCutter = cutter6;
            camOper6.WorkGeometryGroup = workGeometryGroupTag;
            camOper6.ProgramGroup = programGroupTag;
            camOper6.MethodGroupRoot = methodGroupRootTag;
            camOper6.CreateOper();
            SetPartStockAndFloorStock(camOper6.OperTag, 0, 0);
            SetMillArea(NXOpen.UF.CamGeomType.CamCutArea, camOper6.OperTag, Enumerable.Select(ele.ElecHeadFaces, u => u.NXOpenTag).ToList());
            //ufSession.Param.SetDoubleValue(camOper6.OperTag, NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_GLOBAL_CUT_DEPTH, 0.1);
            ufSession.Obj.SetName(camOper6.OperTag, camOper6.AUTOCAM_SUBTYPE + "_0");
            camOpers.Add(camOper6);

            //曲面清角
            var camOper7 = new AutoCAMUI.CAMOper();
            camOper7.AUTOCAM_TYPE = ELECTRODETEMPLATETYPENAME;
            camOper7.AUTOCAM_SUBTYPE = "FLOWCUT_REF_TOOL";
            var cutter7 = new CAMCutter();
            cutter7.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            cutter7.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            cutter7.CutterName = "R1";
            cutter7.TL_DIAMETER = 2;
            cutter7.TL_COR1_RAD = 1;
            cutter7.TL_HEIGHT = 50;
            cutter7.TL_FLUTE_LN = 12;
            CreateCutter(new List<CAMCutter> { cutter7 }, cutterGroupRootTag);
            cutters.Add(cutter7);
            camOper7.CAMCutter = cutter7;
            camOper7.WorkGeometryGroup = workGeometryGroupTag;
            camOper7.ProgramGroup = programGroupTag;
            camOper7.MethodGroupRoot = methodGroupRootTag;
            camOper7.CreateOper();
            SetPartStockAndFloorStock(camOper7.OperTag, 0, 0);
            SetMillArea(NXOpen.UF.CamGeomType.CamCutArea, camOper7.OperTag, Enumerable.Select(nonPlanefaces, u => u.NXOpenTag).ToList());
            //ufSession.Param.SetDoubleValue(camOper7.OperTag, NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_GLOBAL_CUT_DEPTH, 0.04);
            ufSession.Obj.SetName(camOper7.OperTag, camOper7.AUTOCAM_SUBTYPE + "_0");
            camOpers.Add(camOper7);

            //平面清角
            var camOper8 = new AutoCAMUI.CAMOper();
            camOper8.AUTOCAM_TYPE = ELECTRODETEMPLATETYPENAME;
            camOper8.AUTOCAM_SUBTYPE = "FACE_MILLING_CORNER";
            var cutter8 = new CAMCutter();
            cutter8.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            cutter8.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            cutter8.CutterName = "D0.5";
            cutter8.TL_DIAMETER = 0.5;
            cutter8.TL_COR1_RAD = 0;
            cutter8.TL_HEIGHT = 50;
            cutter8.TL_FLUTE_LN = 4;
            CreateCutter(new List<CAMCutter> { cutter8 }, cutterGroupRootTag);
            cutters.Add(cutter8);
            camOper8.CAMCutter = cutter8;
            camOper8.WorkGeometryGroup = workGeometryGroupTag;
            camOper8.ProgramGroup = programGroupTag;
            camOper8.MethodGroupRoot = methodGroupRootTag;
            camOper8.CreateOper();
            //SetPartStockAndFloorStock(camOper8.OperTag, 0, 0);
            SetMillArea(NXOpen.UF.CamGeomType.CamCutArea, camOper8.OperTag, Enumerable.Select(verticalFaces, u => u.FaceTag).ToList());
            //ufSession.Param.SetDoubleValue(camOper7.OperTag, NXOpen.UF.UFConstants.UF_PARAM_CUTLEV_GLOBAL_CUT_DEPTH, 0.04);
            ufSession.Obj.SetName(camOper8.OperTag, camOper8.AUTOCAM_SUBTYPE + "_0");
            camOpers.Add(camOper8);

            //等宽精铣曲面
            var camOper2 = new AutoCAMUI.CAMOper();
            camOper2.AUTOCAM_TYPE = ELECTRODETEMPLATETYPENAME;
            camOper2.AUTOCAM_SUBTYPE = "CONTOUR_AREA";
            var cutter2 = new CAMCutter();
            cutter2.AUTOCAM_TYPE = AUTOCAM_TYPE.mill_planar;
            cutter2.AUTOCAM_SUBTYPE = AUTOCAM_SUBTYPE.MILL;
            cutter2.CutterName = "R3";
            cutter2.TL_DIAMETER = 6;
            cutter2.TL_COR1_RAD = 3;
            cutter2.TL_HEIGHT = 50;
            cutter2.TL_FLUTE_LN = 35;
            CreateCutter(new List<CAMCutter> { cutter2 }, cutterGroupRootTag);
            cutters.Add(cutter2);
            camOper2.CAMCutter = cutter2;
            camOper2.WorkGeometryGroup = workGeometryGroupTag;
            camOper2.ProgramGroup = programGroupTag;
            camOper2.MethodGroupRoot = methodGroupRootTag;
            camOper2.CreateOper();
            ufSession.Param.SetIntValue(camOper2.OperTag, NXOpen.UF.UFConstants.UF_PARAM_STEPOVER_TYPE, 1);
            ufSession.Param.SetDoubleValue(camOper2.OperTag, NXOpen.UF.UFConstants.UF_PARAM_STEPOVER_DIST, 0.1);
            var tempfaces = ele.ElecHeadFaces.Where(u => u.ObjectSubType != Snap.NX.ObjectTypes.SubType.FacePlane).ToList();
            SetMillArea(NXOpen.UF.CamGeomType.CamCutArea, camOper2.OperTag, Enumerable.Select(tempfaces, u => u.NXOpenTag).ToList());
            camOpers.Add(camOper2);


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
        }

        [System.Runtime.InteropServices.DllImport("libufun.dll", EntryPoint = "UF_PARAM_ask_subobj_ptr_value", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        internal static extern int _AskFeedRate(Tag param_tag, int param_index, out int value);

        [System.Runtime.InteropServices.DllImport("libufun.dll", EntryPoint = "UF_PARAM_set_subobj_ptr_value", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        internal static extern int _SetFeedRate(Tag param_tag, int param_index, int value);

        /// <summary>
        /// 获取进给率
        /// </summary>
        public static int AskFeedRate(NXOpen.Tag operTag, int param_index)
        {
            int result;
            _AskFeedRate(operTag, param_index, out result);
            return result;
        }

        /// <summary>
        /// 设置进给率
        /// </summary>
        public static void SetFeedRate(NXOpen.Tag operTag, int param_index, int value)
        {
            _SetFeedRate(operTag, param_index, value);
        }


        /// <summary>
        /// 设置主轴转速
        /// </summary>
        public static void SetSpeedValue(NXOpen.Tag operTag,double speedValue)
        {
            ufSession.Param.SetDoubleValue(operTag, NXOpen.UF.UFConstants.UF_PARAM_SPINDLE_RPM, speedValue);
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
                item.CutterTag = cutterTag;
                result.Add(item);
            }
            return result;
        }
        
        /// <summary>
        /// 设置区域
        /// </summary>
        public static void SetMillArea(NXOpen.UF.CamGeomType camGeomType, NXOpen.Tag operTag, List<NXOpen.Tag> cutAreaGeometryTags)
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
}
