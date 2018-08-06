using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMMProgram;
using System.IO;

public partial class CMMProgramUIBusiness
{
    string _probeDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMM_INSPECTION", "ProbeData.json");

    List<ProbeData> _probeDatas = new List<ProbeData>();
    const string _propertyName = "PROPERTYNAME";

    /// <summary>
    /// 寻找隐藏图层
    /// </summary>
    int FindLayer(Snap.NX.Body body) 
    {
        int r = 1;
        for (int i = 1; i <= 256; i++) 
        {
            if (i != body.Layer && i != Snap.Globals.WorkLayer) 
            {
                r = i;
                break;
            }
        }
        return r;
    }

    void ComponentHelper(Action<ProbeData> action,Snap.NX.Body body,string probeName) 
    {
        if (body != null && !string.IsNullOrEmpty(probeName))
        {

            var mark = Snap.Globals.SetUndoMark(Snap.Globals.MarkVisibility.Visible, "AutoSelectPoint");
            try
            {

                var probeData = _probeDatas.FirstOrDefault(u => u.ProbeName == probeName);
                var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMM_INSPECTION", probeData.ProbeName);

                probeData.Body = ImportPart(fileName);
                if (probeData.Body != null)
                {
                    action(probeData);
                }
            }
            catch (Exception ex)
            {
                NXOpen.UI.GetUI().NXMessageBox.Show("提示", NXOpen.NXMessageBox.DialogType.Information, ex.Message);
            }
            Snap.Globals.UndoToMark(mark, null);
        }
    }

    Snap.NX.Body ImportPart(string fileName) 
    {
        Snap.NX.Body result = null;
        NXOpen.UF.ImportPartModes modes = new NXOpen.UF.ImportPartModes();
        //坐标系
        double[] dest_csys = new double[6];
        //基准点
        double[] dest_point = new double[3];

        dest_csys[0] = 1; //坐标系X轴的矢量
        dest_csys[1] = 0;
        dest_csys[2] = 0;
        dest_csys[3] = 0; //坐标系Y轴的矢量
        dest_csys[4] = 1;
        dest_csys[5] = 0;
        dest_point[0] = 0.0; //基准点【导入到点坐标】
        dest_point[1] = 0.0;
        dest_point[2] = 0.0;

        //导入对象比例缩放
        double scale = 1.0; 
        NXOpen.Tag group;
        NXOpen.UF.UFSession.GetUFSession().Part.Import(fileName, ref modes, dest_csys, dest_point, scale, out group);

        foreach (var m in Snap.Globals.WorkPart.Bodies)
        {
            var axisFace = m.Faces.FirstOrDefault(u => u.Name == SnapEx.ConstString.CMM_INSPECTION_AXISPOINT);
            if (axisFace != null)
            {
                result = m;
                break;
            }
        }
      
        return result;
    }


    void AutoSelectPoint() 
    {
    }

}