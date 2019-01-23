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

        protected override void AutoSet(CAMElectrode ele)
        {
            SetCutLevels(ele);
        }

        /// <summary>
        /// 设置加工层
        /// </summary>
        /// <param name="ele">电极</param>
        public void SetCutLevels(CAMElectrode ele)
        {
            if (OperIsValid)
            {
                _SetCutLevels(ele);
            }
        }

    }
}
