using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMMProgram
{
    /// <summary>
    /// 取点信息
    /// </summary>
    public class GetPointInfo
    {
        public List<PointInfo> pointlist = new List<PointInfo>();
        public class PointInfo
        {
            /// <summary>
            /// 点的名称
            /// </summary>
            public string pointname { get; set; }
            /// <summary>
            /// 测这个点测针的名称
            /// </summary>
            public string arrow { get; set; }
            /// <summary>
            /// 测针旋转角度，A是测针的摆动角度，B是测座的旋转角度
            /// </summary>
            public string TIP { get; set; }
            /// <summary>
            /// 逼近回退距离，这个目前没有使用
            /// </summary>
            public double retract { get; set; }
            /// <summary>
            /// 点的上公差，电极分为整体公差和点公差，电极整体公差就是所有的点都采用一个公差，点公差就是每个点都有自己独立的公差，这个属于人工输入内容，外部输入；
            /// </summary>
            public double uptol { get; set; }
            /// <summary>
            /// 点的下公差，电极分为整体公差和点公差，电极整体公差就是所有的点都采用一个公差，点公差就是每个点都有自己独立的公差，这个属于人工输入内容，外部输入；
            /// </summary>
            public double downtol { get; set; }
            /// <summary>
            /// 点的X坐标
            /// </summary>
            public double x { get; set; }
            /// <summary>
            /// 点的Y坐标
            /// </summary>
            public double y { get; set; }
            /// <summary>
            /// 点的Z坐标
            /// </summary>
            public double z { get; set; }
            /// <summary>
            /// 点的法向I
            /// </summary>
            public double i { get; set; }
            /// <summary>
            /// 点的法向J
            /// </summary>
            public double j { get; set; }
            /// <summary>
            /// 点的法向K
            /// </summary>
            public double k { get; set; }
            /// <summary>
            /// 对应与TIP项目中A
            /// </summary>
            public double a { get; set; }
            /// <summary>
            /// 对应与TIP项目中B
            /// </summary>
            public double b { get; set; }
            /// <summary>
            /// 点的类型，2为基准台侧壁的点，1为基准面的点，3为电极头部的点
            /// </summary>
            public int type { get; set; }
            /// <summary>
            ///  点所在的坐标系象限
            /// </summary>
            public int angle { get; set; }

        }
        public GetPointInfo()
        {
            unit = "MM";
        }

        /// <summary>
        /// 当前取点用的单位距离，目前没有使用，可以考虑去掉
        /// </summary>
        public string unit { get; set; }
        /// <summary>
        ///  电极的模具名称，这个是从Prt档的文件名进行分解得来的
        /// </summary>
        public string mouldname { get; set; }
        /// <summary>
        ///  电极的Partname名称，与数据库 cuprum表的Partname字段进行对应 
        /// </summary>
        public string partname { get; set; }
        /// <summary>
        /// 电极的X大小，已基准台底面中心为原点，创建一个包络体，包含电极头部
        /// </summary>
        public double sizex { get; set; }
        /// <summary>
        /// 电极的Y大小，已基准台底面中心为原点，创建一个包络体，包含电极头部
        /// </summary>
        public double sizey { get; set; }
        /// <summary>
        /// 电极的Z大小，已基准台底面中心为原点，创建一个包络体，包含电极头部
        /// </summary>
        public double sizez { get; set; }
        /// <summary>
        /// 基准台底面到基准台顶面的高度
        /// </summary>
        public double basestationh { get; set; }
        /// <summary>
        /// 电极头部的高度
        /// </summary>
        public double headh { get; set; }
        /// <summary>
        /// 电极取点的象限角
        /// </summary>
        public string cornor { get; set; }
    }
}
