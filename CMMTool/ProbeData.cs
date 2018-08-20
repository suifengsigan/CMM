using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SnapEx;

namespace CMMTool
{
    public class ProbeData
    {
        /// <summary>
        /// 名称
        /// </summary>
        [DisplayName("探针名称")]
        public string ProbeName { get; set; }
        /// <summary>
        /// 探球直径
        /// </summary>
        public double D = 0;
        /// <summary>
        /// 测杆直径
        /// </summary>
        public double d = 0;
        /// <summary>
        /// 测头工作长度
        /// </summary>
        public double L = 0;
        public double D1 = 0;
        public double L1 = 0;
        public double L2 = 0;
        public double D2 = 0;
        public double D3 = 0;
          /// <summary>
        /// 探针角度
        /// </summary>
        public List<AB> ABList = new List<AB>();
        public List<AB> GetABList()
        {
            var list = ABList ?? new List<AB>();
            if (list.Where(u => u.A == 0 && u.B == 0).Count() == 0)
            {
                list.Add(new AB { });
            }
            return list;
        }

        /// <summary>
        /// 获取球心
        /// </summary>
        public Snap.Position GetCentreOfSphere(ProbeData.AB ab)
        {
            var sphere=GetBody(ab);
            Snap.NX.Face.Sphere sphereFace = sphere.Faces.FirstOrDefault(u => u.IsHasAttr(Business.EACTPROBESPHEREFACE)) as Snap.NX.Face.Sphere;
            return sphereFace.Geometry.Center;
        }


        /// <summary>
        /// 获取体
        /// </summary>
        public Snap.NX.Body GetBody(ProbeData.AB ab)
        {
            Snap.NX.Body result = null;
            result = Snap.Globals.WorkPart.Bodies.FirstOrDefault(u => u.Name == string.Format("{0}A{1}B{2}", this.ProbeName, ab.A, ab.B));
            return result;
        }
        public class AB
        {
            public double A { get; set; }
            public double B { get; set; }
        }
    }
}
