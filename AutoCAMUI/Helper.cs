using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    public static class Helper
    {
        private static NXOpen.UF.UFSession ufSession = NXOpen.UF.UFSession.GetUFSession();
        /// <summary>
        /// 获取拔模角度
        /// </summary>
        public static double GetDraftAngle(this Snap.NX.Face face, Snap.Vector vector)
        {
            double[] param = new double[2], point = new double[3], u1 = new double[3], v1 = new double[3], u2 = new double[3], v2 = new double[3], unitNorm = new double[3], radii = new double[2];
            ufSession.Modl.AskFaceProps(face.NXOpenTag, param, point, u1, v1, u2, v2, unitNorm, radii);
            var angle = Snap.Vector.Angle(unitNorm, vector);
            var draftAngle = 90 - angle;
            return draftAngle;
        }
        public static void AutoCAM(Snap.NX.Body body)
        {
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
            var origin = new Snap.Position();
            ufSession.Cam.SetClearPlaneData(workMcsGroupTag, origin.Array, normal.Array);

            //TODO 创建几何体
            ufSession.Ncgeom.Create(AUTOCAM_TYPE.mill_planar, AUTOCAM_SUBTYPE.WORKPIECE, out workGeometryGroupTag);
            ufSession.Obj.SetName(workGeometryGroupTag, AUTOCAM_ROOTNAME.WORKPIECE_EACT);
            ufSession.Ncgroup.AcceptMember(workMcsGroupTag, workGeometryGroupTag);

            //TODO 创建程序
            NXOpen.Tag programGroupTag;
            ufSession.Ncprog.Create(AUTOCAM_TYPE.mill_planar, AUTOCAM_SUBTYPE.PROGRAM, out programGroupTag);
            ufSession.Obj.SetName(programGroupTag, AUTOCAM_ROOTNAME.PROGRAM_EACT);
            ufSession.Ncgroup.AcceptMember(orderGroupRootTag, programGroupTag);
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
    }
}
