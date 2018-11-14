﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoCAMUI
{
    /// <summary>
    /// 加工工序类
    /// </summary>
    public class CAMOper
    {
        public string AUTOCAM_TYPE { get; set; }
        public string AUTOCAM_SUBTYPE { get; set; }
        public NXOpen.Tag CAMCutter { get; set; }
        public NXOpen.Tag WorkGeometryGroup { get; set; }
        public NXOpen.Tag ProgramGroup { get; set; }
        public NXOpen.Tag MethodGroupRoot { get; set; }

        /// <summary>
        /// 创建工序
        /// </summary>
        public void CreateOper()
        {
            //TODO 创建工序
            NXOpen.Tag operTag;
            var ufSession = NXOpen.UF.UFSession.GetUFSession();
            ufSession.Oper.Create(AUTOCAM_TYPE, AUTOCAM_SUBTYPE, out operTag);
            ufSession.Ncgroup.AcceptMember(WorkGeometryGroup, operTag);
            ufSession.Ncgroup.AcceptMember(ProgramGroup, operTag);
            ufSession.Ncgroup.AcceptMember(MethodGroupRoot, operTag);
            ufSession.Ncgroup.AcceptMember(CAMCutter, operTag);
        }
    }
}
