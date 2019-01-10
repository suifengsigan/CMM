using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NXOpen;
using SnapEx;

namespace AutoCAMUI
{
    /// <summary>
    /// 电极头部开粗(石墨开粗)
    /// </summary>
    public class WsqAutoCAM_CAVITY_MILL_G_Oper : WsqAutoCAM_Oper
    {
        public WsqAutoCAM_CAVITY_MILL_G_Oper()
        {
            AUTOCAM_SUBTYPE = "CAVITY_MILL_G";
        }

        /// <summary>
        /// 设置切削层
        /// </summary>
        public void SetCutLevels(Tag faceTag, levelsPosition levelsPosition = levelsPosition.BottomLevel)
        {
            _SetCutLevels(faceTag, levelsPosition);
        }

        /// <summary>
        /// 设置非切削移动 区域起点
        /// </summary>
        public void SetRegionStartPoints(ElecManage.Electrode electrode)
        {
            var baseFace = electrode.BaseFace;
            var result = baseFace.GetCenterPointEx();
            var box = electrode.ElecBody.Box;
            var info = electrode.GetElectrodeInfo();
            result.X = System.Math.Abs(box.MaxX - box.MinX);
            result.Z = System.Math.Abs(info.HEADPULLUPH);
            Helper.SetRegionStartPoints(OperTag, result);
        }
    }
}
