using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    public class CAMFace
    {
        public NXOpen.Tag FaceTag { get; set; }
        /// <summary>
        /// 拔模角度
        /// </summary>
        public double DraftAngle { get; set; }
    }
}
