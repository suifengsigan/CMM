using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 残料开粗
    /// </summary>
    public class WsqAutoCAM_CAVITY_MILL_REF_Oper:WsqAutoCAM_Oper
    {
        public WsqAutoCAM_CAVITY_MILL_REF_Oper()
        {
            TmplateOper = E_TmplateOper. CAVITY_MILL_REF;
        }

        /// <summary>
        /// 设置参考刀具
        /// </summary>
        /// <param name="cutter">参考刀具</param>
        public void SetReferenceCutter(CAMCutter cutter)
        {
            _SetReferenceCutter(cutter);
        }

        /// <summary>
        /// 设置加工层
        /// </summary>
        /// <param name="ele">电极</param>
        public void SetCutLevels(ElecManage.Electrode ele)
        {
            _SetCutLevels(ele.BaseFace.NXOpenTag);
        }

    }
}
