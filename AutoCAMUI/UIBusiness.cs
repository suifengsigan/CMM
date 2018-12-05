using NXOpen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Snap;
public partial class EleAutoCamUI:SnapEx.BaseUI
{

    /// <summary>
    /// 模板名称
    /// </summary>
    string templateTypeName;

    public override void DialogShown()
    {
        AutoCAMUI.Helper.ReinitOpt();
    }

    public override void Update(NXOpen.BlockStyler.UIBlock block)
    {
        
    }

    public override void Apply()
    {
        
    }

}