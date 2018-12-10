using NXOpen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Snap;
public partial class EleAutoCamUI:SnapEx.BaseUI
{
    public override void DialogShown()
    {
        AutoCAMUI.Helper.InitCAMSession("WsqAutoCAM");
    }

    public override void Update(NXOpen.BlockStyler.UIBlock block)
    {
        
    }

    public override void Apply()
    {
        var body = Snap.Globals.WorkPart.Bodies.FirstOrDefault();
        var ele = ElecManage.Electrode.GetElectrode(body);
        if (ele != null)
        {
            ele.InitAllFace();
            AutoCAMUI.Helper.AutoCAM(ele);
        }
    }

}