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
        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="colorIndex">UG颜色ID</param>
        public void SetColor(int colorIndex)
        {
            SetColor(colorIndex, FaceTag);
        }
        public Snap.NX.Face GetSnapFace()
        {
            if (FaceTag != NXOpen.Tag.Null)
            {
                return Snap.NX.Face.Wrap(FaceTag);
            }
            return null;
        }
        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="colorIndex">UG颜色ID</param>
        public static void SetColor(int colorIndex,NXOpen.Tag faceTag)
        {
            if (faceTag != NXOpen.Tag.Null)
            {
                Snap.NX.Face.Wrap(faceTag).Color = SnapEx.Create.WindowsColor(colorIndex);
            }
        }
    }
}
