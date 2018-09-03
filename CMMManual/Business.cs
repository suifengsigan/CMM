using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CMM;

public partial class CMMProgramUI:SnapEx.BaseUI
{
    string _probeDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("CMM_INSPECTION", "ProbeData.json"));
    const string _propertyName = "PROPERTYNAME";
    void UFDisp() 
    {
        UFDisp(new List<PointData>());
    }
    void UFDisp(List<PointData> points, int selectedIndex = -1, bool isAddNode = true) 
    {
        if (isAddNode) { DeleteNodes(); }

        var ufSession = NXOpen.UF.UFSession.GetUFSession();
        ufSession.Disp.RegenerateDisplay();
        NXOpen.UF.UFDisp.ConeheadAttrbSTag stConeheadAttrb;
        ufSession.Disp.GetConeheadAttrb(out stConeheadAttrb);
       

        for (int i = points.Count - 1; i >= 0; i--)
        {
            var item = points[i];
            //NXOpen.Tag tag;
            var doublePoint = new double[] { item.Position.X, item.Position.Y, item.Position.Z };
            var doubleVector = new double[] { item.Vector.X, item.Vector.Y, item.Vector.Z };
            //ufSession.Curve.CreatePoint(doublePoint, out tag);
            if (i != selectedIndex)
            {
                stConeheadAttrb.color = 90;
                ufSession.Disp.SetConeheadAttrb(ref stConeheadAttrb);
            }
            else 
            {
                stConeheadAttrb.color = 180;
                ufSession.Disp.SetConeheadAttrb(ref stConeheadAttrb);
            }
            ufSession.Disp.LabeledConehead(NXOpen.UF.UFConstants.UF_DISP_ALL_ACTIVE_VIEWS, doublePoint, doubleVector, 0, string.Format("P{0}", i + 1));
            if (isAddNode) 
            {
                AddNode(i, item);
            }
        }
    }

    #region 树操作

    NXOpen.BlockStyler.Node GetSelectNode() 
    {
        return tree_control0.GetSelectedNodes().FirstOrDefault();
    }

    PointData GetPointData(NXOpen.BlockStyler.Node node) 
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<PointData>(node.GetNodeData().GetString(_propertyName));
    }

    List<PointData> GetPointDatasFromTree(List<NXOpen.BlockStyler.Node> nodes)
    {
        var points = new List<PointData>();
        nodes.ForEach(u =>
        {
            points.Add(GetPointData(u));
        });
        return points;
    }
    List<PointData> GetPointDatasFromTree() 
    {
        return GetPointDatasFromTree(GetNodes());
    }

    void OnSelectcallback(NXOpen.BlockStyler.Tree tree, NXOpen.BlockStyler.Node node, int columnID, bool selected)
    {
        if (selected)
        {
            var selectedNode = GetSelectNode();
            if (selectedNode != null) 
            {
                UFDisp(GetPointDatasFromTree(), GetNodes().IndexOf(selectedNode),false);
            }
           
        }
    }

    void DeleteNodes() 
    {
        GetNodes().ForEach(u => {
            tree_control0.DeleteNode(u);
        });
    }
    List<NXOpen.BlockStyler.Node> GetNodes()
    {
        var nodes = new List<NXOpen.BlockStyler.Node>();
        if (tree_control0.RootNode != null)
        {
            var node = tree_control0.RootNode;
            nodes.Add(node);

            while (node.NextNode != null)
            {
                node = node.NextNode;
                nodes.Add(node);
            }
        }
        return nodes;
    }
    NXOpen.BlockStyler.Node AddNode(int index,PointData point)
    {
        point.PointName = string.Format("P{0}", index + 1);
        var node = tree_control0.CreateNode(point.PointName);
        node.GetNodeData().AddString(_propertyName, Newtonsoft.Json.JsonConvert.SerializeObject(point));
        tree_control0.InsertNode(node, null, null, NXOpen.BlockStyler.Tree.NodeInsertOption.AlwaysLast);
        node.SetColumnDisplayText(1, point.Position.X.ToString("F4"));
        node.SetColumnDisplayText(2, point.Position.Y.ToString("F4"));
        node.SetColumnDisplayText(3, point.Position.Z.ToString("F4"));
        node.SetColumnDisplayText(4, point.Vector.X.ToString());
        node.SetColumnDisplayText(5, point.Vector.Y.ToString());
        node.SetColumnDisplayText(6, point.Vector.Z.ToString());
        node.SetColumnDisplayText(7, point.A.ToString());
        node.SetColumnDisplayText(8, point.B.ToString());
        return node;
    }
    #endregion

    void RefreshUI() 
    {
        //bool isAuto = !toggle0.Value;
        //selectionPoint.Show = !isAuto;
        //btnAutoSelectPoint.Show = isAuto;
        //btnUP.Show = !isAuto;
        //btnDown.Show = !isAuto;
        //btnRemove.Show = !isAuto;
        toggle0.Show = false;
        btnUP.Show = false;
        btnDown.Show = false;
        btnAutoSelectPoint.Show = GetNodes().Count == 0;
        selectionPoint.Show = !btnAutoSelectPoint.Show;
    }

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

    void ComponentHelper(Action action) 
    {
        var body = selectCuprum.GetSelectedObjects().FirstOrDefault() as NXOpen.Body;
        if (body != null)
        {
            var mark = Snap.Globals.SetUndoMark(Snap.Globals.MarkVisibility.Visible, "AutoSelectPoint");
            try
            {
                action();
            }
            catch(Exception ex)
            {
                theUI.NXMessageBox.Show("提示", NXOpen.NXMessageBox.DialogType.Information, ex.Message);
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
        DeleteNodes();
        var body = selectCuprum.GetSelectedObjects().FirstOrDefault() as NXOpen.Body;
        var pointDatas = new List<PointData>();
        ComponentHelper(() =>
        {

        });
        UFDisp(pointDatas);
    }

    public override void Update(NXOpen.BlockStyler.UIBlock block)
    {
        RefreshUI();
        var body = selectCuprum.GetSelectedObjects().FirstOrDefault() as NXOpen.Body;
        if (block == btnAutoSelectPoint) 
        {
            AutoSelectPoint();
            RefreshUI();
        }
        else if (block == toggle0) 
        {
            UFDisp();
            DeleteNodes();
        }
        else if (block == btnDown) 
        {
           var node= GetSelectNode();
           if (node != null) 
           {
               var list = GetNodes();
               var index = list.IndexOf(node) + 1;
               index = index > list.Count-1 ? list.Count - 1 : index;
               list.Remove(node);
               list.Insert(index, node);
               UFDisp(GetPointDatasFromTree(list));
           }
        }
        else if (block == btnUP) 
        {
            var node = GetSelectNode();
            if (node != null)
            {
                var list = GetNodes();
                var index = list.IndexOf(node) - 1;
                index = index < 0 ? 0 : index;
                list.Remove(node);
                list.Insert(index, node);
                UFDisp(GetPointDatasFromTree(list));
            }
        }
        else if (block == btnRemove) 
        {
            var node = GetSelectNode();
            if (node != null)
            {
                var pd = GetPointData(node);
                if (pd != null && pd.PointType == PointType.HeadFace)
                {
                    var list = GetNodes();
                    list.Remove(node);
                    UFDisp(GetPointDatasFromTree(list));
                    theUI.NXMessageBox.Show("提示", NXOpen.NXMessageBox.DialogType.Information, "删除成功！");
                }
                else 
                {
                    theUI.NXMessageBox.Show("提示", NXOpen.NXMessageBox.DialogType.Information, "无法删除该点！");
                }
            }
        }
        else if (block == selectionPoint)
        {
            if (body != null && selectionPoint.GetSelectedObjects().Count() > 0)
            {
                PointData data = null;
                ComponentHelper(() =>
                {
                    //data = new Electrode(body).IsIntervene(selectionPoint.PickPoint, probeData);
                });

                if (data != null&&data.PointType==PointType.HeadFace)
                {
                    var points = GetPointDatasFromTree();
                    points.Add(data);
                    UFDisp(points);
                }
                else
                {
                    theUI.NXMessageBox.Show("提示", NXOpen.NXMessageBox.DialogType.Information, "无法取该点，请重新选点！");
                }
            }

            selectionPoint.SetSelectedObjects(new NXOpen.TaggedObject[] { });

        }
        else if (block == btnExport)
        {
            if (body != null)
            {
                var list = new List<PointData>();
                GetNodes().ForEach(u =>
                {
                    list.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<PointData>(u.GetNodeData().GetString(_propertyName)));
                });
                if (list.Count > 0)
                {
                    //导出
                   
                }
            }
            else 
            {
                //ImportPart(  Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CMM_INSPECTION", enumSelectTool.ValueAsString));
            }
        }
    }

    public override void Init()
    {
        toggle0.Value = false;
        var snapSelectCuprum = Snap.UI.Block.SelectObject.GetBlock(theDialog, selectCuprum.Name);
        snapSelectCuprum.SetFilter(Snap.NX.ObjectTypes.Type.Body, Snap.NX.ObjectTypes.SubType.BodySolid);
        snapSelectCuprum.AllowMultiple = false;

        var snapSelectedPoint = Snap.UI.Block.SelectObject.GetBlock(theDialog, selectionPoint.Name);
        snapSelectedPoint.AllowMultiple = false;
        snapSelectedPoint.SetFilter(Snap.NX.ObjectTypes.Type.Face);

        tree_control0.SetOnSelectHandler(new NXOpen.BlockStyler.Tree.OnSelectCallback(OnSelectcallback));
        
    }
    public override void DialogShown()
    {
        RefreshUI();
        tree_control0.ShowHeader = true;
        tree_control0.InsertColumn(0, "名称", 50);
        tree_control0.InsertColumn(1, "X", 200);
        tree_control0.InsertColumn(2, "Y", 200);
        tree_control0.InsertColumn(3, "Z", 200);
        tree_control0.InsertColumn(4, "I", 200);
        tree_control0.InsertColumn(5, "J", 200);
        tree_control0.InsertColumn(6, "K", 200);
        tree_control0.InsertColumn(7, "A", 200);
        tree_control0.InsertColumn(8, "B", 200);

        tree_control0.SetColumnResizePolicy(1, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);
        tree_control0.SetColumnResizePolicy(2, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);
        tree_control0.SetColumnResizePolicy(3, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);
        tree_control0.SetColumnResizePolicy(4, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);
        tree_control0.SetColumnResizePolicy(5, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);
        tree_control0.SetColumnResizePolicy(6, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);
        tree_control0.SetColumnResizePolicy(7, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);
        tree_control0.SetColumnResizePolicy(8, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);

        tree_control0.SetColumnSortable(0, false);
        tree_control0.SetColumnSortable(1, false);
        tree_control0.SetColumnSortable(2, false);
        tree_control0.SetColumnSortable(3, false);
        tree_control0.SetColumnSortable(4, false);
        tree_control0.SetColumnSortable(5, false);
        tree_control0.SetColumnSortable(6, false);
        tree_control0.SetColumnSortable(7, false);
        tree_control0.SetColumnSortable(8, false);
        

        if (Snap.Globals.WorkPart.Bodies.Count() == 1) 
        {
            selectCuprum.SetSelectedObjects(new NXOpen.TaggedObject[] { Snap.Globals.WorkPart.Bodies.FirstOrDefault() });
        }
    }

    public override void Apply()
    {
        UFDisp();
    }

}