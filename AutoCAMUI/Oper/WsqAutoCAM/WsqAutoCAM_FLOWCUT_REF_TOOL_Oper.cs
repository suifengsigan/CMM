using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 曲面清角
    /// </summary>
    public class WsqAutoCAM_FLOWCUT_REF_TOOL_Oper:WsqAutoCAM_Oper
    {
        public WsqAutoCAM_FLOWCUT_REF_TOOL_Oper()
        {
            TmplateOper = E_TmplateOper.FLOWCUT_REF_TOOL;
        }

        protected override void AutoSet(CAMElectrode ele)
        {
            SetMillArea(ele);
        }

        public override void SetReferenceCutter(CAMCutter cutter)
        {
            if (OperIsValid)
            {
                NX.Entry.GetInstance().SetFlowCutRefTool(OperTag, cutter.CutterTag);
            }
            
        }

        public override void SetCutDepth(double depth)
        {
            if (OperIsValid)
            {
                NX.Entry.GetInstance().SetFlowCutStepOver(OperTag, depth);
            }
        }

        public void SetMillArea(CAMElectrode ele)
        {
            var tags = new List<NXOpen.Tag>();
            ele.GentleFaces.ForEach(u => {
                tags.Add(u.FaceTag);
            });
            tags = tags.Distinct().ToList();
            Helper.SetCamgeom(NXOpen.UF.CamGeomType.CamCutArea, OperTag, tags);
        }

        /// <summary>
        /// 设置加工区域
        /// </summary>
        /// <param name="ele">电极</param>
        public void SetMillArea(ElecManage.Electrode ele)
        {
            var faces = ele.ElecHeadFaces.Where(u => u.ObjectSubType != Snap.NX.ObjectTypes.SubType.FacePlane).ToList();
            Helper.SetCamgeom(NXOpen.UF.CamGeomType.CamCutArea, OperTag, Enumerable.Select(faces, u => u.NXOpenTag).ToList());
        }
    }
}
