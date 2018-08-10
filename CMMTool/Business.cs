using Snap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapEx;
using System.IO;

namespace CMMTool
{
    public class Business
    {
        /// <summary>
        /// 删除模型
        /// </summary>
        public static void DeleteProbe(ProbeData data)
        {
            var temp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMM_INSPECTION");
            var fileName = Path.Combine(temp, data.ProbeName);
            if (File.Exists(fileName + ".prt"))
            {
                File.Delete(fileName + ".prt");
            }
        }

        /// <summary>
        /// 创建探针模型
        /// </summary>
        public static bool CreateProbe(ProbeData data)
        {
            bool result = true;
            //var theUI = NXOpen.UI.GetUI();
            //var mark = Globals.SetUndoMark(Globals.MarkVisibility.Visible, "IsIntervene");
            //try
            //{
            //    Snap.NX.CoordinateSystem wcs = Globals.WorkPart.NXOpenPart.WCS.CoordinateSystem;
            //    var vector = wcs.AxisZ;
            //    var position = new Snap.Position(0, 0, data.SphereRadius);
            //    //创建探球
            //    var body = Snap.Create.Sphere(position, data.SphereRadius * 2).Body;
            //    body.IsHidden = true;
            //    body.Faces.ToList().ForEach(u =>
            //    {
            //        u.Name = SnapEx.ConstString.CMM_INSPECTION_SPHERE;
            //    });
            //    //创建测针
            //    var body1 = Snap.Create.Cylinder(position, position + (data.ArrowLength * vector), data.ArrowRadius * 2).Body;
            //    body1.IsHidden = true;
            //    position = position + (data.ArrowLength * vector);
            //    //创建加长杆
            //    var body2 = Snap.Create.Cylinder(position, position + (data.ExtensionBarLength * vector), data.ExtensionBarRadius * 2).Body;
            //    body2.IsHidden = true;
            //    //创建测头
            //    position = position + (data.ExtensionBarLength * vector);
            //    var body3 = Snap.Create.Cylinder(position, position + (data.HeadLength * vector), data.HeadRadius * 2).Body;
            //    body3.IsHidden = true;
            //    body3.Faces.ToList().ForEach(u =>
            //    {
            //        if (SnapEx.Helper.Equals(u.GetFaceDirection(), vector) && u.ObjectSubType == Snap.NX.ObjectTypes.SubType.FacePlane)
            //        {
            //            u.Name = SnapEx.ConstString.CMM_INSPECTION_AXISPOINT;
            //        }
            //    });
            //    var r = Snap.Create.Unite(body, body1, body2, body3);
            //    r.Orphan();

            //    body.Name = data.ProbeName;

            //    var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMM_INSPECTION", data.ProbeName);
            //    if (File.Exists(fileName + ".prt"))
            //    {
            //        File.Delete(fileName + ".prt");
            //    }
            //    var dir = Path.GetDirectoryName(fileName);
            //    if (!Directory.Exists(dir))
            //    {
            //        Directory.CreateDirectory(dir);
            //    }
            //    SnapEx.Create.ExtractBody(new List<NXOpen.Body> { body }, fileName, false, true);
            //}
            //catch (Exception ex)
            //{
            //    theUI.NXMessageBox.Show("提示", NXOpen.NXMessageBox.DialogType.Information, ex.Message);
            //    result = false;
            //}
            //Globals.UndoToMark(mark, null);
            return result;
        }
    }
}
