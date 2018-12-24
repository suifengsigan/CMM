using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NXOpen;

namespace AutoCAMUI
{
    public class WsqAutoCAM_CAVITY_MILL_C_Oper : CAMOper
    {
        public WsqAutoCAM_CAVITY_MILL_C_Oper()
        {
            AUTOCAM_TYPE = "WsqAutoCAM";
            AUTOCAM_SUBTYPE = "CAVITY_MILL_C";
        }

        /// <summary>
        /// 设置切削层
        /// </summary>
        public void SetCutLevels(Tag faceTag, levelsPosition levelsPosition = levelsPosition.BottomLevel)
        {
            _SetCutLevels(faceTag, levelsPosition);
        }
    }
}
