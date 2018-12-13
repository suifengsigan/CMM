﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 加工刀具类
    /// </summary>
    public class CAMCutter
    {
        public string AUTOCAM_TYPE { get; set; }
        public string AUTOCAM_SUBTYPE { get; set; }
        /// <summary>
        /// 刀具标签
        /// </summary>
        public NXOpen.Tag CutterTag { get; set; }
        /// <summary>
        /// 刀具名称
        /// </summary>
        public string CutterName { get; set; }
        /// <summary>
        /// 刀具直径(D 直径)
        /// </summary>
        public double TL_DIAMETER { get; set; }
        /// <summary>
        /// 刀具半径（R1 下半径）
        /// </summary>
        public double TL_COR1_RAD { get; set; }
        /// <summary>
        /// 刀具的首下长(L 长度)
        /// </summary>
        public double TL_HEIGHT { get; set; }
        /// <summary>
        /// 刀刃长度(FL 刀刃长度)
        /// </summary>
        public double TL_FLUTE_LN { get; set; }
    }
}
