using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapEx;

namespace AutoCAMUI
{
    /// <summary>
    /// 电极分析类（加工用）
    /// </summary>
    public class CAMElectrode
    {
        /// <summary>
        /// 电极
        /// </summary>
        public ElecManage.Electrode Electrode { get; private set; }
        public E_CamScheme CamScheme { get; set; }
        /// <summary>
        /// 倒扣面
        /// </summary>
        public List<CAMFace> ButtonedFaces { get; private set; }
        /// <summary>
        /// 基准台面
        /// </summary>
        public List<Snap.NX.Face> AllBaseFaces { get; private set; }
        /// <summary>
        /// 垂直面
        /// </summary>
        public List<CAMFace> VerticalFaces { get; private set; }
        /// <summary>
        /// 水平面
        /// </summary>
        public List<CAMFace> HorizontalFaces { get; private set; }
        public void Init(ElecManage.Electrode ele,CNCConfig.CAMConfig camConfig)
        {
            Electrode = ele;

            var body = ele.ElecBody;
            var basePos = ele.GetElecBasePos();
            var eleInfo = ele.GetElectrodeInfo();
            var bodyBox = body.AcsToWcsBox3d(new Snap.Orientation(-ele.BaseFace.GetFaceDirection()));
            var autoBlankOffset = new double[] { 2, 2, 2, 2, 2, 0 };

            //分析面
            var faces = ele.ElecBody.Faces;
            double judgeValue = 15;
            var camFaces = new List<CAMFace>();
            ele.ElecHeadFaces.ForEach(u => {
                camFaces.Add(new CAMFace { FaceTag = u.NXOpenTag, DraftAngle = u.GetDraftAngle() });
            });

            //基准面
            AllBaseFaces = faces.Where(u => camFaces.FirstOrDefault(m => m.FaceTag == u.NXOpenTag) == null).ToList();
            //垂直面
            VerticalFaces = camFaces.Where(u => u.DraftAngle == 0 && u.GetSnapFace().ObjectSubType == Snap.NX.ObjectTypes.SubType.FacePlane).ToList();
            //水平面
            HorizontalFaces = camFaces.Where(u => u.DraftAngle == 90 && u.GetSnapFace().ObjectSubType == Snap.NX.ObjectTypes.SubType.FacePlane).ToList();
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
            ButtonedFaces = camFaces.Where(u => u.DraftAngle < 0).ToList();
            //非平面
            var nonPlanefaces = ele.ElecHeadFaces.Where(u => u.ObjectSubType != Snap.NX.ObjectTypes.SubType.FacePlane).ToList();

            //设置基准面颜色
            AllBaseFaces.ForEach(u => {
                CAMFace.SetColor(camConfig.BaseFaceColor, u.NXOpenTag);
            });
            //设置垂直面颜色
            VerticalFaces.ForEach(u => {
                u.SetColor(camConfig.VerticalPlaneColor);
            });
            //设置水平面颜色
            HorizontalFaces.ForEach(u => {
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
            ButtonedFaces.ForEach(u => {
                u.SetColor(camConfig.ButtonedFaceColor);
            });

            //分析方案
            CamScheme = E_CamScheme.SIMPLE;
        }
    }
}
