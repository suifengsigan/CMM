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
            TmplateOper = E_TmplateOper.CAVITY_MILL_G;
        }

        protected override void AutoSet(CAMElectrode ele)
        {
            _SetPartStockAndFloorStock(ele.CamConfig.CAVITYPartStock, ele.CamConfig.CAVITYFloorStock);
            _SetCutLevels(ele);
            _SetRegionStartPoints(ele);
        }
      
    }
}
