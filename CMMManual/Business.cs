﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CMM;
using SnapEx;

public partial class CMMProgramUI:SnapEx.BaseUI
{
    const string _propertyName = "PROPERTYNAME";
    CMMTool.CMMConfig _config = CMMTool.CMMConfig.GetInstance();
    ElecManage.Electrode _electrode = null;
    void UFDisp() 
    {
        UFDisp(new List<PointData>());
    }

    /// <summary>
    /// 设置探针显示
    /// </summary>
    public static void SetProbeShow(PointData data,CMMTool.CMMConfig config)
    {
        SetProbeHide();

        var probe = config.ProbeDatas.FirstOrDefault(u => u.ProbeName == data.Arrow);
        if (probe != null)
        {
            var ab = probe.GetABList().FirstOrDefault(u => u.A == data.A && u.B == data.B);
            if (ab != null)
            {
                var body = probe.GetBody(ab);
                //移动
                var centreOfSphere = probe.GetCentreOfSphere(ab);
                //退点
                var sRetreatPosition = data.Position.Copy(Snap.Geom.Transform.CreateTranslation((probe.D / 2) * data.Vector));
                var trans = Snap.Geom.Transform.CreateTranslation(sRetreatPosition - centreOfSphere);
                body.Move(trans);
                body.IsHidden = false;
            }
        }
    }

    /// <summary>
    /// 设置探针隐藏
    /// </summary>
    public static void SetProbeHide()
    {
        var bodies = Snap.Globals.WorkPart.Bodies.ToList().Where(u => u.IsHasAttr(SnapEx.ConstString.CMM_INSPECTION_SPHERE)).ToList();
        bodies.ForEach(u => u.IsHidden = true);
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
                var datas = GetPointDatasFromTree();
                var index = GetNodes().IndexOf(selectedNode);
                UFDisp(datas, index, false);
                if (index >= 0 && datas.Count > index)
                {
                    //显示探针
                    SetProbeShow(datas.ElementAt(index), _config);
                }
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
        node.SetColumnDisplayText(1, point.Arrow);
        node.SetColumnDisplayText(2, point.Position.X.ToString("F4"));
        node.SetColumnDisplayText(3, point.Position.Y.ToString("F4"));
        node.SetColumnDisplayText(4, point.Position.Z.ToString("F4"));
        node.SetColumnDisplayText(5, point.Vector.X.ToString("F4"));
        node.SetColumnDisplayText(6, point.Vector.Y.ToString("F4"));
        node.SetColumnDisplayText(7, point.Vector.Z.ToString("F4"));
        node.SetColumnDisplayText(8, point.A.ToString());
        node.SetColumnDisplayText(9, point.B.ToString());
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
        enumSelectTool.Show = false;
        btnAutoSelectPoint.Show = GetNodes().Count == 0;
        selectionPoint.Show = !btnAutoSelectPoint.Show;
        SetProbeHide();
    }
    

    void ComponentHelper(Action action) 
    {
        var body = selectCuprum.SelectedObjects.FirstOrDefault() as Snap.NX.Body;
        if (body != null)
        {
            var mark = Snap.Globals.SetUndoMark(Snap.Globals.MarkVisibility.Invisible, "ComponentHelper");
            try
            {
                action();
            }
            catch(Exception ex)
            {
                theUI.NXMessageBox.Show("提示", NXOpen.NXMessageBox.DialogType.Information, ex.Message);
                Snap.Globals.UndoToMark(mark, null);
            }
        }
    }
    


    void AutoSelectPoint() 
    {
        DeleteNodes();
        var body = selectCuprum.SelectedObjects.FirstOrDefault() as Snap.NX.Body;
        var pointDatas = new List<PointData>();
        var mark = Snap.Globals.SetUndoMark(Snap.Globals.MarkVisibility.Invisible, "ComponentHelperAutoSelectPoint");
        try
        {
            pointDatas = CMMBusiness.AutoSelPoint(_electrode, _config, false);
        }
        catch (Exception ex)
        {
            pointDatas = new List<PointData>();
            selectCuprum.SelectedObjects = new Snap.NX.NXObject[] { };
            _electrode = null;
            NXOpen.UF.UFSession.GetUFSession().Ui.DisplayMessage(ex.Message, 1);
        }

        Snap.Globals.UndoToMark(mark, "ComponentHelperAutoSelectPoint");
        UFDisp(pointDatas);
    }

    public override void Update(NXOpen.BlockStyler.UIBlock block)
    {
        RefreshUI();
        var body = selectCuprum.SelectedObjects.FirstOrDefault() as Snap.NX.Body;
        if (block == btnAutoSelectPoint.NXOpenBlock)
        {
            if (body != null)
            {
                AutoSelectPoint();
                RefreshUI();
            }
        }
        else if (block == selectCuprum.NXOpenBlock)
        {
            _electrode = null;
            if (body != null)
            {
                var elec=ElecManage.Electrode.GetElectrode(body);
                if (elec != null)
                {
                    elec.InitAllFace();
                    _electrode = elec;
                }
                else
                {
                    selectCuprum.SelectedObjects = new Snap.NX.NXObject[] { };
                    NXOpen.UF.UFSession.GetUFSession().Ui.DisplayMessage("该电极无法识别",1);
                }
            }
        }
        else if (block == toggle0.NXOpenBlock)
        {
            UFDisp();
            DeleteNodes();
        }
        else if (block == btnDown.NXOpenBlock)
        {
            var node = GetSelectNode();
            if (node != null)
            {
                var list = GetNodes();
                var index = list.IndexOf(node) + 1;
                index = index > list.Count - 1 ? list.Count - 1 : index;
                list.Remove(node);
                list.Insert(index, node);
                UFDisp(GetPointDatasFromTree(list));
            }
        }
        else if (block == btnUP.NXOpenBlock)
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
        else if (block == btnRemove.NXOpenBlock)
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
        else if (block == selectionPoint.NXOpenBlock)
        {
            if (body != null && selectionPoint.SelectedObjects.Count() > 0)
            {
                PointData data = null;
                ComponentHelper(() =>
                {
                    data = CMMBusiness.IsInterveneBySelPoint(_electrode, selectionPoint.PickPoint, _config);
                });

                if (data != null)
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

            selectionPoint.SelectedObjects = new Snap.NX.NXObject[] { };

        }
        else if (block == btnExport.NXOpenBlock)
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
                    CMMBusiness.WriteCMMFileByPointData(_electrode, list,_config);
                    NXOpen.UF.UFSession.GetUFSession().Ui.DisplayMessage("导出成功", 1);
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
        snapSelectCuprum.SetFilter(Snap.NX.ObjectTypes.Type.Body);
        snapSelectCuprum.AllowMultiple = false;

        var snapSelectedPoint = Snap.UI.Block.SelectObject.GetBlock(theDialog, selectionPoint.Name);
        snapSelectedPoint.AllowMultiple = false;
        snapSelectedPoint.SetFilter(Snap.NX.ObjectTypes.Type.Face);

        tree_control0.SetOnSelectHandler(new NXOpen.BlockStyler.Tree.OnSelectCallback(OnSelectcallback));
        _electrode = null;
    }
    public override void DialogShown()
    {
        RefreshUI();
        //tree_control0.ShowHeader = true;
        tree_control0.InsertColumn(0, "名称", 50);
        tree_control0.InsertColumn(1, "探针", 200);
        tree_control0.InsertColumn(2, "X", 200);
        tree_control0.InsertColumn(3, "Y", 200);
        tree_control0.InsertColumn(4, "Z", 200);
        tree_control0.InsertColumn(5, "I", 200);
        tree_control0.InsertColumn(6, "J", 200);
        tree_control0.InsertColumn(7, "K", 200);
        tree_control0.InsertColumn(8, "A", 100);
        tree_control0.InsertColumn(9, "B", 100);

        tree_control0.SetColumnResizePolicy(1, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);
        tree_control0.SetColumnResizePolicy(2, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);
        tree_control0.SetColumnResizePolicy(3, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);
        tree_control0.SetColumnResizePolicy(4, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);
        tree_control0.SetColumnResizePolicy(5, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);
        tree_control0.SetColumnResizePolicy(6, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);
        tree_control0.SetColumnResizePolicy(7, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);
        tree_control0.SetColumnResizePolicy(8, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);
        tree_control0.SetColumnResizePolicy(9, NXOpen.BlockStyler.Tree.ColumnResizePolicy.ResizeWithTree);

        tree_control0.SetColumnSortable(0, false);
        tree_control0.SetColumnSortable(1, false);
        tree_control0.SetColumnSortable(2, false);
        tree_control0.SetColumnSortable(3, false);
        tree_control0.SetColumnSortable(4, false);
        tree_control0.SetColumnSortable(5, false);
        tree_control0.SetColumnSortable(6, false);
        tree_control0.SetColumnSortable(7, false);
        tree_control0.SetColumnSortable(8, false);
        tree_control0.SetColumnSortable(9, false);


        if (Snap.Globals.WorkPart.Bodies.Count() == 1)
        {
            selectCuprum.SelectedObjects = new Snap.NX.NXObject[] { Snap.Globals.WorkPart.Bodies.FirstOrDefault() };
        }
    }

    public override void Apply()
    {
        UFDisp();
    }

}