using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NXOpen.BlockStyler;
using SnapEx;

public partial class CheckRegionsUI : SnapEx.BaseUI
{
    /// <summary>
    /// 刷新数据
    /// </summary>
    void RefreshData()
    {
        var draftValue = double0.Value;
        var toggle0Dic = dic.Where(u => u.Value < draftValue && u.Value > 0);
        toggle0Dic.ToList().ForEach(u => {
            Snap.NX.Face.Wrap(u.Key).Color = SnapEx.Create.WindowsColor(colorPicker0.ColorIndex);
        });
        var toggle01Dic = dic.Where(u => u.Value >= draftValue && u.Value > 0);
        toggle01Dic.ToList().ForEach(u => {
            Snap.NX.Face.Wrap(u.Key).Color = SnapEx.Create.WindowsColor(colorPicker01.ColorIndex);
        });
        var toggle02Dic = dic.Where(u => u.Value == 0);
        toggle02Dic.ToList().ForEach(u => {
            Snap.NX.Face.Wrap(u.Key).Color = SnapEx.Create.WindowsColor(colorPicker02.ColorIndex);
        });
        var toggle03Dic = dic.Where(u => u.Value == 90);
        toggle03Dic.ToList().ForEach(u => {
            Snap.NX.Face.Wrap(u.Key).Color = SnapEx.Create.WindowsColor(colorPicker03.ColorIndex);
        });
        var toggle04Dic = dic.Where(u => u.Value < 0);
        toggle04Dic.ToList().ForEach(u => {
            Snap.NX.Face.Wrap(u.Key).Color = SnapEx.Create.WindowsColor(colorPicker04.ColorIndex);
        });
        toggle0.Label = string.Format("等高       <{0}             {1}", draftValue, toggle0Dic.Count());

        toggle01.Label = string.Format("平行       >={0}             {1}", draftValue, toggle01Dic.Count());
        toggle02.Label = string.Format("水平       =0             {1}", draftValue, toggle02Dic.Count());
        toggle03.Label = string.Format("垂直       =90             {1}", draftValue, toggle03Dic.Count());
        toggle04.Label = string.Format("倒扣       <0             {1}", draftValue, toggle04Dic.Count());
        toggle05.Label = string.Format("基准                      {1}", draftValue, 0);
    }
    public override void Init()
    {
        var snapSelectCuprum = bodySelect0;
        snapSelectCuprum.SetFilter(Snap.NX.ObjectTypes.Type.Body);
        snapSelectCuprum.AllowMultiple = false;
    }
    public override void DialogShown()
    {
        var bodies = Snap.Globals.WorkPart.Bodies.ToList();
        vector0.Origin = new Snap.Position();
        vector0.Direction = Snap.Orientation.Identity.AxisZ;
        //if (bodies.Count > 0)
        //{
        //    bodySelect0.SelectedObjects = new Snap.NX.NXObject[] { bodies.First() };
        //}
    }

    private Dictionary<NXOpen.Tag, double> dic = new Dictionary<NXOpen.Tag, double>();

    public override void Update(UIBlock block)
    {
        if (bodySelect0.NXOpenBlock == block)
        {
            var body = bodySelect0.SelectedObjects.FirstOrDefault() as Snap.NX.Body;
            if (body != null)
            {
                var faces = body.Faces.ToList();
                faces.ForEach(u =>
                {
                    dic.Add(u.NXOpenTag, u.GetDraftAngle(vector0.Direction));
                });
                RefreshData();
            }
            else
            {
                dic.Clear();
            }
        }
        else if (double0.NXOpenBlock == block)
        {
            RefreshData();
        }
    }
}