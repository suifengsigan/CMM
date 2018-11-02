using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapEx;
using NXOpen.Drawings;
using NXOpen;

partial class EdmDrawUI : SnapEx.BaseUI
{
    const double _tolerance = 0.0001;
    List<string> _paramFileList = new List<string>();
    double[] _frontViewMatrix = new double[]{1.000000000,0.000000000,0.000000000,0.000000000,  0.000000000,1.000000000};
    double[] _topViewMatrix = new double[] { 1.000000000,0.000000000,0.000000000,0.000000000,  1.000000000,0.000000000 };
    double[] _bottomViewMatrix = new double[] {-1.000000000,0.000000000,0.000000000,0.000000000,  1.000000000,0.000000000 };
    double[] _bottomFrontViewMatrix = new double[] { -1.000000000,0.000000000,0.000000000,0.000000000,  0.000000000,-1.000000000 };
    double[] _isometricViewMatrix = new double[] { 0.7071067,0.7071067,0.000000000,-0.4082482,  0.4082482,0.8164965 };
    double[] _bottomIsometricViewMatrix = new double[] {0.7071067,-0.7071067,0.000000000,-0.5, -0.5,-0.7 };

    public override void Init()
    {
        var snapSelectSteel = Snap.UI.Block.SelectObject.GetBlock(theDialog, selectSteel.Name);
        snapSelectSteel.AllowMultiple = false;
        snapSelectSteel.SetFilter(Snap.NX.ObjectTypes.Type.Body, Snap.NX.ObjectTypes.SubType.BodySolid);

        var snapSelectCuprum = Snap.UI.Block.SelectObject.GetBlock(theDialog, selectCuprum.Name);
        snapSelectCuprum.AllowMultiple = false;
        snapSelectCuprum.SetFilter(Snap.NX.ObjectTypes.Type.Body, Snap.NX.ObjectTypes.SubType.BodySolid);

        var members = new List<string>();
        var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EdmTemplate");
        if (System.IO.Directory.Exists(path))
        {
            _paramFileList = System.IO.Directory.GetFiles(path).ToList();
            _paramFileList.ForEach(u =>
            {
                members.Add(System.IO.Path.GetFileNameWithoutExtension(u));
            });
        }

        selectTemplate0.Items= members.ToArray();
    }
    public override void Apply()
    {
        Snap.NX.Body selectedObj = selectCuprum.SelectedObjects.FirstOrDefault() as Snap.NX.Body;
        var steel = selectSteel.SelectedObjects.FirstOrDefault() as Snap.NX.Body;
        var workPart = Snap.Globals.WorkPart;
        var templateName = _paramFileList.Where(u => u.Contains(selectTemplate0.SelectedItem)).FirstOrDefault();

        InitModelingView();

        //EdmDraw.DrawBusiness.InitPreferences(3, 2, .8, .1);
        EdmDraw.DrawBusiness.InitPreferences("blockfont", 2, .8, .1);

        CreateDrawingSheet(selectedObj, steel, templateName);
    }

    public void CreateEdmDraw(Snap.NX.Body elecBody,Snap.NX.Body steel) 
    {
        var workPart = Snap.Globals.WorkPart;
        var members = new List<string>();
        var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template");
        if (System.IO.Directory.Exists(path))
        {
            _paramFileList = System.IO.Directory.GetFiles(path).ToList();
            _paramFileList.ForEach(u =>
            {
                members.Add(System.IO.Path.GetFileNameWithoutExtension(u));
            });
        }
        InitModelingView();

        EdmDraw.DrawBusiness.InitPreferences("blockfont", 2, .8, .1);

        CreateDrawingSheet(elecBody, steel,_paramFileList.First());
    }


    void CreateDrawingSheet(Snap.NX.Body selectedObj, Snap.NX.Body steel, string templateName) 
    {
        var workPart = Snap.Globals.WorkPart;
        var dsName = selectedObj.Name;
        var list = new List<NXOpen.TaggedObject>();
        list.Add(steel);
        list.Add(selectedObj);

        workPart.NXOpenPart.DrawingSheets.ToArray().Where(u => u.Name == dsName).ToList().ForEach(u =>
        {
            Snap.NX.NXObject.Delete(u);
        });

        //新建图纸页
        var ds = SnapEx.Create.DrawingSheet(selectedObj.Name, templateName);
        EdmDraw.DrawBusiness.SetDrawSheetLayer(ds, 254);

        //获取电极信息
        var electrode = ElecManage.Electrode.GetElectrode(selectedObj);
        if (electrode == null)
        {
            throw new Exception("无法识别该电极！");
        }
        electrode.InitAllFace();
        var topFace = electrode.TopFace;
        var baseFace = electrode.BaseFace;
        //BASE_SIDE
        var baseSideFaces = electrode.BaseSideFaces;
        //ELECTRODE_FACE
        var electrodeFaces = electrode.ElecHeadFaces;

        var edmConfig = EdmDraw.UCEdmConfig.GetInstance();
        var draftViewLocations = edmConfig.DraftViewLocations ?? new List<EdmDraw.EdmConfig.DraftViewLocation>();
        foreach (var item in draftViewLocations)
        {
            var viewType = (EdmDraw.ViewType)Enum.Parse(typeof(EdmDraw.ViewType), item.ViewType);
            switch (viewType)
            {
                case EdmDraw.ViewType.EACT_TOP:
                    {
                        CreateEACT_TOPView(
                            ds,
                            list, 
                            new Snap.Position(item.LocationX,item.LocationY),
                            new Snap.Position(item.SizeX,item.SizeY)
                            );
                    }
                    break;
                case EdmDraw.ViewType.EACT_FRONT:
                    {
                        CreateEACT_FRONTView(
                            ds,
                            list,
                            new Snap.Position(item.LocationX, item.LocationY),
                            new Snap.Position(item.SizeX, item.SizeY)
                            );
                    }
                    break;
                case EdmDraw.ViewType.EACT_BOTTOM_FRONT:
                    {
                        CreateEACT_BOTTOM_FRONTView(
                            ds,
                            new List<TaggedObject> { selectedObj },
                            new Snap.Position(item.LocationX, item.LocationY),
                            new Snap.Position(item.SizeX, item.SizeY)
                            );
                    }
                    break;
                case EdmDraw.ViewType.EACT_BOTTOM:
                    {
                        CreateEACT_BOTTOMView(
                            ds,
                            new List<TaggedObject> { selectedObj },
                            new Snap.Position(item.LocationX, item.LocationY),
                            new Snap.Position(item.SizeX, item.SizeY)
                            );
                    }
                    break;
                case EdmDraw.ViewType.EACT_BOTTOM_ISOMETRIC:
                    {
                        CreateEACT_BOTTOM_ISOMETRICView(
                            ds,
                            new List<TaggedObject> { selectedObj },
                            new Snap.Position(item.LocationX, item.LocationY),
                            new Snap.Position(item.SizeX, item.SizeY)
                            );
                    }
                    break;
                case EdmDraw.ViewType.EACT_ISOMETRIC:
                    {
                        CreateEACT_ISOMETRICView(
                            ds,
                            new List<TaggedObject> { steel },
                            new Snap.Position(item.LocationX, item.LocationY),
                            new Snap.Position(item.SizeX, item.SizeY)
                            );
                    }
                    break;
            }
        }

        CreateTable(edmConfig, new List<ElecManage.Electrode> { electrode });
    }

    /// <summary>
    /// 创建表格
    /// </summary>
    void CreateTable(EdmDraw.EdmConfig edmConfig,List<ElecManage.Electrode> elecs)
    {
        //创建表格
        var tableInfo = edmConfig.Table;
        var columnInfos = tableInfo.ColumnInfos;
        EdmDraw.DrawBusiness.CreateTabnot(
            new Snap.Position(tableInfo.locationX, tableInfo.locationY),
            elecs.Count + 1,
            tableInfo.ColumnInfos.Count,
            tableInfo.RowHeight,
            tableInfo.ColumnWidth
            );

        foreach (var item in elecs)
        {
            var elecIndex = elecs.IndexOf(item)+1;
            foreach (var columnInfo in columnInfos)
            {
                var index = columnInfos.IndexOf(columnInfo);
                if (columnInfo.Ex == "1")
                {
                    EdmDraw.DrawBusiness.CreatePentagon(
                        new Snap.Position(tableInfo.locationX+((index* tableInfo.ColumnWidth)+ tableInfo.ColumnWidth/2), tableInfo.locationY-((elecIndex * tableInfo.RowHeight) + tableInfo.RowHeight / 2))
                        ,item.GetQuadrantType()
                        , tableInfo.ColumnWidth * 2 / 3
                        , tableInfo.RowHeight * 2 / 3
                        );
                }
            }
        }
    }

    void CreateEACT_TOPView(NXOpen.Drawings.DrawingSheet ds, List<NXOpen.TaggedObject> selections, Snap.Position pos, Snap.Position size)
    {
        var view = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_TOP).Tag, selections, pos, size);
    }

    void CreateEACT_FRONTView(NXOpen.Drawings.DrawingSheet ds, List<NXOpen.TaggedObject> selections, Snap.Position pos, Snap.Position size)
    {
        var view = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_FRONT).Tag, selections, pos, size);
    }

    void CreateEACT_BOTTOM_FRONTView(NXOpen.Drawings.DrawingSheet ds, List<NXOpen.TaggedObject> selections, Snap.Position pos, Snap.Position size)
    {
        var view = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_BOTTOM_FRONT).Tag, selections, pos, size);
    }

    void CreateEACT_BOTTOMView(NXOpen.Drawings.DrawingSheet ds, List<NXOpen.TaggedObject> selections, Snap.Position pos, Snap.Position size)
    {
        var view = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_BOTTOM).Tag, selections, pos, size);
    }

    void CreateEACT_BOTTOM_ISOMETRICView(NXOpen.Drawings.DrawingSheet ds, List<NXOpen.TaggedObject> selections, Snap.Position pos, Snap.Position size)
    {
        var view = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_BOTTOM_ISOMETRIC).Tag, selections, pos, size);
    }

    void CreateEACT_ISOMETRICView(NXOpen.Drawings.DrawingSheet ds, List<NXOpen.TaggedObject> selections, Snap.Position pos, Snap.Position size)
    {
        var view = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_ISOMETRIC).Tag, selections, pos, size);
    }

    ModelingView GetModelingView(EdmDraw.ViewType viewType) 
    {
        var name=Enum.GetName(viewType.GetType(),viewType);
        var result= Snap.Globals.WorkPart.NXOpenPart.ModelingViews.ToArray().FirstOrDefault(u => u.Name == name);
        return result;
    }
    
    void InitModelingView() 
    {
        SnapEx.Create.ApplicationSwitchRequest(SnapEx.ApplicationType.MODELING);
        EdmDraw.DrawBusiness.CreateCamera(EdmDraw.ViewType.EACT_BOTTOM, _bottomViewMatrix);
        EdmDraw.DrawBusiness.CreateCamera(EdmDraw.ViewType.EACT_TOP, _topViewMatrix);
        EdmDraw.DrawBusiness.CreateCamera(EdmDraw.ViewType.EACT_FRONT, _frontViewMatrix);
        EdmDraw.DrawBusiness.CreateCamera(EdmDraw.ViewType.EACT_BOTTOM_FRONT, _bottomFrontViewMatrix);
        EdmDraw.DrawBusiness.CreateCamera(EdmDraw.ViewType.EACT_BOTTOM_ISOMETRIC, _bottomIsometricViewMatrix);
        EdmDraw.DrawBusiness.CreateCamera(EdmDraw.ViewType.EACT_ISOMETRIC, _isometricViewMatrix);
    }

    public void Test()
    {
        //var topView = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_TOP).Tag, list, new Snap.Position(56, 72), new Snap.Position(90, 90));
        //var frontView = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_FRONT).Tag, list, new Snap.Position(56, 155), new Snap.Position(90, 40));
        //var bottomFrontView = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_BOTTOM_FRONT).Tag, new List<TaggedObject> { selectedObj }, new Snap.Position(154, 155), new Snap.Position(40, 40));
        //var bottomView = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_BOTTOM).Tag, new List<TaggedObject> { selectedObj }, new Snap.Position(154, 100), new Snap.Position(60, 60));
        //var bottomIsometricView = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_BOTTOM_ISOMETRIC).Tag, new List<TaggedObject> { selectedObj }, new Snap.Position(154, 50), new Snap.Position(60, 60));
        //var isometricView = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_ISOMETRIC).Tag, new List<NXOpen.TaggedObject> { steel }, new Snap.Position(220, 58), new Snap.Position(60, 60));
        //var originPoint = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(Snap.Globals.Wcs.Origin); }, frontView.Tag);
        //var elecBasePoint = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(electrode.GetElecBasePos()); }, frontView.Tag);


        //var bottomFrontViewBorderPoints = EdmDraw.DrawBusiness.GetBorderPoint(bottomFrontView, selectedObj);

        //var bottomFrontViewTopPoint = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(bottomFrontViewBorderPoints[3]); }, bottomFrontView.Tag);
        //var bottomFrontViewBottomPoint = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(bottomFrontViewBorderPoints[2]); }, bottomFrontView.Tag);

        //var bottomViewBorderPoints = EdmDraw.DrawBusiness.GetBorderPoint(bottomView, selectedObj);

        //var yPlusSideFace = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(bottomViewBorderPoints[1]); }, bottomFrontView.Tag);
        //var yMinusSideFace = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(bottomViewBorderPoints[0]); }, bottomFrontView.Tag);
        //var xPlusSideFace = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Line(bottomViewBorderPoints[0], bottomViewBorderPoints[1]); }, bottomFrontView.Tag);
        //var xMinusSideFace = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Line(bottomViewBorderPoints[2], bottomViewBorderPoints[3]); }, bottomFrontView.Tag);

        //var bottomViewElecHeadBorderPoints = EdmDraw.DrawBusiness.GetBorderPoint(bottomView, electrodeFaces);

        //var yPlusElectrodeEdge = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(bottomViewElecHeadBorderPoints[1]); }, bottomFrontView.Tag);
        //var yMinusElectrodeEdge = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(bottomViewElecHeadBorderPoints[0]); }, bottomFrontView.Tag);
        //var xPlusElectrodeEdge = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Line(bottomViewElecHeadBorderPoints[0], bottomViewElecHeadBorderPoints[1]); }, bottomFrontView.Tag);
        //var xMinusElectrodeEdge = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Line(bottomViewElecHeadBorderPoints[2], bottomViewElecHeadBorderPoints[3]); }, bottomFrontView.Tag);


        ////电极前视图
        //EdmDraw.DrawBusiness.CreateVerticalDim(
        //    bottomFrontView.Tag,
        //    bottomFrontViewTopPoint.NXOpenTag,
        //    bottomFrontViewBottomPoint.NXOpenTag,
        //    new Snap.Position(bottomFrontView.GetDrawingReferencePoint().X + (EdmDraw.DrawBusiness.GetBorderSize(bottomFrontView.Tag).X / 2), bottomFrontView.GetDrawingReferencePoint().Y));

        //EdmDraw.DrawBusiness.CreateVerticalDim(bottomFrontView.Tag, baseFace.NXOpenTag, bottomFrontViewBottomPoint.NXOpenTag,
        //    new Snap.Position(bottomFrontView.GetDrawingReferencePoint().X - (EdmDraw.DrawBusiness.GetBorderSize(bottomFrontView.Tag).X / 2), bottomFrontView.GetDrawingReferencePoint().Y));

        ////电极仰视图
        //EdmDraw.DrawBusiness.CreateVerticalDim(bottomView.Tag, yPlusSideFace.NXOpenTag, yMinusSideFace.NXOpenTag,
        //    new Snap.Position(bottomView.GetDrawingReferencePoint().X + (EdmDraw.DrawBusiness.GetBorderSize(bottomView.Tag).X / 2), bottomView.GetDrawingReferencePoint().Y, 0));
        //EdmDraw.DrawBusiness.CreatePerpendicularDim(bottomView.Tag, xPlusSideFace.NXOpenTag, xMinusSideFace.NXOpenTag,
        //    new Snap.Position(bottomView.GetDrawingReferencePoint().X, bottomView.GetDrawingReferencePoint().Y + (EdmDraw.DrawBusiness.GetBorderSize(bottomView.Tag).Y / 2), 0));

        //EdmDraw.DrawBusiness.CreateVerticalDim(bottomView.Tag, yPlusElectrodeEdge.NXOpenTag, yMinusElectrodeEdge.NXOpenTag,
        //    new Snap.Position(bottomView.GetDrawingReferencePoint().X - (EdmDraw.DrawBusiness.GetBorderSize(bottomView.Tag).X / 2), bottomView.GetDrawingReferencePoint().Y, 0));
        //EdmDraw.DrawBusiness.CreatePerpendicularDim(bottomView.Tag, xPlusElectrodeEdge.NXOpenTag, xMinusElectrodeEdge.NXOpenTag,
        //   new Snap.Position(bottomView.GetDrawingReferencePoint().X, bottomView.GetDrawingReferencePoint().Y - (EdmDraw.DrawBusiness.GetBorderSize(bottomView.Tag).Y / 2), 0));

        //var frontViewTopMargin = EdmDraw.DrawBusiness.GetViewBorder(EdmDraw.ViewBorderType.Right, frontView);
        //var tempMap = new double[] { 0, 0 };
        //var ufSession = NXOpen.UF.UFSession.GetUFSession();
        //ufSession.View.MapModelToDrawing(frontView.Tag, elecBasePoint.Position.Array, tempMap);
        //var basePointMTD = tempMap.ToArray();
        //ufSession.View.MapModelToDrawing(frontView.Tag, originPoint.Position.Array, tempMap);
        //var originPointMTD = tempMap.ToArray();
        //var distance = Snap.Compute.Distance(new Snap.Position(tempMap.First(), tempMap.Last()), frontViewTopMargin);
        //EdmDraw.DrawBusiness.CreatePerpendicularOrddimension(
        //    frontView.Tag,
        //    originPoint.NXOpenTag,
        //    frontViewTopMargin.NXOpenTag,
        //    originPoint.NXOpenTag
        //    );
        ////TODO 坐标尺寸位置问题
        //var configData = 8;
        //Snap.Vector v = new Snap.Vector(distance, 0);
        //Snap.Vector v1 = new Snap.Vector(distance, configData);
        //var angle = Snap.Vector.Angle(v, v1);
        //Snap.Position? origin = null;
        //if (basePointMTD.Last() > originPointMTD.Last())
        //{
        //    var line = frontViewTopMargin as Snap.NX.Line;
        //    origin = new Snap.Position(line.StartPoint.X, originPointMTD.Last() + (configData * 2));
        //}
        //var frontViewOrddimension = EdmDraw.DrawBusiness.CreatePerpendicularOrddimension(
        //    frontView.Tag,
        //    originPoint.NXOpenTag,
        //    frontViewTopMargin.NXOpenTag,
        //    elecBasePoint.NXOpenTag,
        //    angle,
        //    origin
        //    );

        //EdmDraw.DrawBusiness.SetToleranceType(frontViewOrddimension);



        //var topViewRightMargin = EdmDraw.DrawBusiness.GetViewBorder(EdmDraw.ViewBorderType.Right, topView);
        //var topViewTopMargin = EdmDraw.DrawBusiness.GetViewBorder(EdmDraw.ViewBorderType.Top, topView);

        //var topViewRightElecBasePoint = EdmDraw.DrawBusiness.CreatePerpendicularOrddimension(
        //    topView.Tag,
        //    originPoint.NXOpenTag,
        //    topViewRightMargin.NXOpenTag,
        //    elecBasePoint.NXOpenTag
        //    );

        //EdmDraw.DrawBusiness.SetToleranceType(topViewRightElecBasePoint);

        //EdmDraw.DrawBusiness.CreatePerpendicularOrddimension(
        //    topView.Tag,
        //    originPoint.NXOpenTag,
        //    topViewRightMargin.NXOpenTag,
        //    originPoint.NXOpenTag
        //    );

        //var topViewTopElecBasePoint = EdmDraw.DrawBusiness.CreateVerticalOrddimension(
        //    topView.Tag,
        //    originPoint.NXOpenTag,
        //    topViewTopMargin.NXOpenTag,
        //    elecBasePoint.NXOpenTag
        //    );

        //EdmDraw.DrawBusiness.SetToleranceType(topViewTopElecBasePoint);

        //EdmDraw.DrawBusiness.CreateVerticalOrddimension(
        //    topView.Tag,
        //    originPoint.NXOpenTag,
        //    topViewTopMargin.NXOpenTag,
        //    originPoint.NXOpenTag
        //    );

        //EdmDraw.DrawBusiness.GetBorderPoint(topView, steel).ForEach(u =>
        //{
        //    var tempU = EdmDraw.DrawBusiness.CreateNxObject<Snap.NX.Point>(() => { return Snap.Create.Point(u); }, topView.Tag);
        //    EdmDraw.DrawBusiness.CreateVerticalOrddimension(
        //    topView.Tag,
        //    originPoint.NXOpenTag,
        //    topViewTopMargin.NXOpenTag,
        //    tempU.NXOpenTag
        //    );

        //    EdmDraw.DrawBusiness.CreatePerpendicularOrddimension(
        //    topView.Tag,
        //    originPoint.NXOpenTag,
        //    topViewRightMargin.NXOpenTag,
        //    tempU.NXOpenTag
        //    );
        //});

        //var borderSize = topView.GetBorderSize();
        //var refPoint = topView.GetDrawingReferencePoint();

        //EdmDraw.DrawBusiness.CreateIdSymbol("C1", new Snap.Position(refPoint.X - (borderSize.X / 2), refPoint.Y), new Snap.Position(), topView.Tag, elecBasePoint.NXOpenTag);

        ////注释
        //EdmDraw.DrawBusiness.CreateNode(selectedObj.Name, new Snap.Position(35, 19));

        ////表格
        //EdmDraw.DrawBusiness.CreateTabnot(new Snap.Position(216, ds.Height - 1.5), 2, 4, 15, 15);

        //EdmDraw.DrawBusiness.CreatePentagon(new Snap.Position(223, ds.Height - 1.5 - 2), QuadrantType.Four);
    }

}