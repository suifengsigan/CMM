using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMMProgram
{
    public enum PointType 
    {
        UNKOWN=0,
        HorizontalDatumFace,
        HeadFace,
        VerticalDatumFace
    }
    public class PointData
    {
        public string PointName { get; set; }
        public Snap.Position Position { get; set; }
        public Snap.Vector Vector { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        /// <summary>
        /// 测这个点测针的名称
        /// </summary>
        public string Arrow { get; set; }
        /// <summary>
        /// 点的类型，2为基准台侧壁的点，1为基准面的点，3为电极头部的点
        /// </summary>
        public PointType PointType { get; set; }
        /// <summary>
        /// 点的上公差，电极分为整体公差和点公差，电极整体公差就是所有的点都采用一个公差，点公差就是每个点都有自己独立的公差，这个属于人工输入内容，外部输入；
        /// </summary>
        public double Uptol { get; set; }
        /// <summary>
        /// 点的下公差，电极分为整体公差和点公差，电极整体公差就是所有的点都采用一个公差，点公差就是每个点都有自己独立的公差，这个属于人工输入内容，外部输入；
        /// </summary>
        public double Downtol { get; set; }

    }
}
