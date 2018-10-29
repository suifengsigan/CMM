using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    public static class Helper
    {
        /// <summary>
        /// 获取拔模角度
        /// </summary>
        public static double GetDraftAngle(this Snap.NX.Face face, Snap.Vector vector)
        {
            var ufSession = NXOpen.UF.UFSession.GetUFSession();
            double[] param = new double[2], point = new double[3], u1 = new double[3], v1 = new double[3], u2 = new double[3], v2 = new double[3], unitNorm = new double[3], radii = new double[2];
            ufSession.Modl.AskFaceProps(face.NXOpenTag, param, point, u1, v1, u2, v2, unitNorm, radii);
            var angle = Snap.Vector.Angle(unitNorm, vector);
            var draftAngle = 90 - angle;
            return draftAngle;
        }
    }
}
