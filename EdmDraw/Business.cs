using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapEx;
using NXOpen.Drawings;
using NXOpen;
using ElecManage;
using Snap.NX;

partial class EdmDrawUI : SnapEx.BaseUI,CommonInterface.IEDM
{
    EactConfig.ConfigData _ConfigData = EactConfig.ConfigData.GetInstance();
    public override void DialogShown()
    {
        selectTemplate0.Show = false;
        txtDrfLayer.Show = false;
    }
    public override void Init()
    {
        var snapSelectSteel = Snap.UI.Block.SelectObject.GetBlock(theDialog, selectSteel.Name);
        snapSelectSteel.AllowMultiple = false;
        snapSelectSteel.SetFilter(Snap.NX.ObjectTypes.Type.Body, Snap.NX.ObjectTypes.SubType.BodySolid);

        var snapSelectCuprum = Snap.UI.Block.SelectObject.GetBlock(theDialog, selectCuprum.Name);
        snapSelectCuprum.AllowMultiple = false;
        snapSelectCuprum.SetFilter(Snap.NX.ObjectTypes.Type.Body, Snap.NX.ObjectTypes.SubType.BodySolid);
    }
    public override void Apply()
    {
        Snap.NX.Body selectedObj = selectCuprum.SelectedObjects.FirstOrDefault() as Snap.NX.Body;
        var steel = selectSteel.SelectedObjects.FirstOrDefault() as Snap.NX.Body;
        var workPart = Snap.Globals.WorkPart;
        CreateDrawingSheet(selectedObj, steel);
    }

    void CreateDrawingSheet(Snap.NX.Body selectedObj, Snap.NX.Body steel)
    {
        //获取电极信息
        var positionings = new List<ElecManage.PositioningInfo>();
        Snap.Globals.WorkPart.Bodies.Where(u => selectedObj.Layer == u.Layer && selectedObj.Name == u.Name).ToList().ForEach(u => {
            var info = ElecManage.Electrode.GetElectrode(u);
            if (info != null)
            {
                var positioning = new ElecManage.PositioningInfo();
                positioning.Electrode = info;
                var pos = info.GetElecBasePos();
                pos = Snap.NX.CoordinateSystem.MapAcsToWcs(pos);
                positioning.X = Math.Round(pos.X, 4);
                positioning.Y = Math.Round(pos.Y, 4);
                positioning.Z = Math.Round(pos.Z, 4);
                positioning.QuadrantType = info.GetQuadrantType();
                positionings.Add(positioning);
            }
        });

        if (positionings.Count <= 0)
        {
            throw new Exception("无法识别该电极！");
        }

        positionings = positionings.OrderBy(u => u.C).ToList();

        foreach (var item in positionings)
        {
            item.N = string.Format("C{0}", positionings.IndexOf(item) + 1);
        }

        CreateDrawingSheet(positionings, steel,false);
    }

    public void CreateDrawingSheet(List<PositioningInfo> positionings, Snap.NX.Body steel)
    {
        CreateDrawingSheet(positionings, steel,true);
    }
    public void CreateDrawingSheet(List<PositioningInfo> positionings, Snap.NX.Body steel,bool isDeleteDs)
    {
        var edmConfig = EdmDraw.UCEdmConfig.GetInstance();
        var templateName = edmConfig.GetEdmTemplate();
        if (string.IsNullOrEmpty(templateName))
        {
            return;
        }
        ElecManage.Electrode electrode = positionings.FirstOrDefault().Electrode;
        var selectedObj = electrode.ElecBody;
        electrode.InitAllFace();
        InitModelingView(edmConfig);
        EdmDraw.DrawBusiness.InitPreferences(edmConfig);
        var workPart = Snap.Globals.WorkPart;
        var dsName = selectedObj.Name;
        var list = new List<NXOpen.TaggedObject>();
        list.Add(steel);
        list.Add(selectedObj);

        workPart.NXOpenPart.DrawingSheets.ToArray().Where(u => u.Name == dsName).ToList().ForEach(u =>
        {
            Snap.NX.NXObject.Delete(u);
        });

        //图纸显示
        EdmDraw.DrawBusiness.SetShowLayers(new List<int> { 1 }, edmConfig.EdmDrfLayer);

        //新建图纸页
        var ds = SnapEx.Create.DrawingSheet(selectedObj.Name, templateName);
        EdmDraw.DrawBusiness.SetDrawSheetLayer(ds, edmConfig.EdmDrfLayer);
        
        var draftViewLocations = edmConfig.DraftViewLocations ?? new List<EdmDraw.EdmConfig.DraftViewLocation>();
        EdmDraw.EdmConfig.DraftViewLocation ViewTypeEACT_TOP = null;
        foreach (var item in draftViewLocations)
        {
            var viewType = EdmDraw.DrawBusiness.GetEumnViewType( item.ViewType);
            switch (viewType)
            {
                case EdmDraw.ViewType.EACT_TOP:
                    {
                        ViewTypeEACT_TOP = item;
                    }
                    break;
                case EdmDraw.ViewType.EACT_FRONT:
                    {
                        CreateEACT_FRONTView(
                            ds,
                            list,
                            new Snap.Position(item.LocationX, item.LocationY),
                            new Snap.Position(item.SizeX, item.SizeY),
                            electrode,
                            edmConfig
                            );
                    }
                    break;
                case EdmDraw.ViewType.EACT_BOTTOM_FRONT:
                    {
                        CreateEACT_BOTTOM_FRONTView(
                            ds,
                            new List<TaggedObject> { selectedObj },
                            new Snap.Position(item.LocationX, item.LocationY),
                            new Snap.Position(item.SizeX, item.SizeY),
                            electrode,
                            edmConfig
                            );
                    }
                    break;
                case EdmDraw.ViewType.EACT_BOTTOM:
                    {
                        CreateEACT_BOTTOMView(
                            ds,
                            new List<TaggedObject> { selectedObj },
                            new Snap.Position(item.LocationX, item.LocationY),
                            new Snap.Position(item.SizeX, item.SizeY),
                            electrode, edmConfig
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
                            , edmConfig
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
                            , edmConfig
                            );
                    }
                    break;
            }
        }

        CreateNodeInfo(electrode, edmConfig);

        var ps = new List<List<PositioningInfo>>();
        if (edmConfig.PageCount <= 0)
        {
            ps.Add(positionings);
        }
        else
        {
            var ceiling = Math.Ceiling((double)(positionings.Count * 1.0 / edmConfig.PageCount));
            var tempV = positionings.Count % edmConfig.PageCount;
            for (int i = 0; i < ceiling; i++)
            {
                ps.Add(positionings.Skip(i * edmConfig.PageCount).Take(
                    i == ceiling - 1 && tempV != 0 ? tempV : edmConfig.PageCount
                    ).ToList());
            }
        }

        foreach (var item in ps)
        {
            var pdfName = ds.Name;
            if (ps.Count > 1)
            {
                pdfName += "_" + (ps.IndexOf(item) + 1);
            }
            var deleteObj = new List<NXOpen.Tag>();
            if (ViewTypeEACT_TOP != null)
            {
                var topView = CreateEACT_TOPView(
                                ds,
                                steel,
                                new Snap.Position(ViewTypeEACT_TOP.LocationX, ViewTypeEACT_TOP.LocationY),
                                new Snap.Position(ViewTypeEACT_TOP.SizeX, ViewTypeEACT_TOP.SizeY),
                                item,
                                edmConfig
                                );

                deleteObj.Add(topView.Tag);
            }

            deleteObj.AddRange(CreateTable(edmConfig, item));

            var result = EdmDraw.Helper.ExportPDF(ds, pdfName);
            var info = electrode.GetElectrodeInfo();
            CommonInterface.FtpHelper.FtpUpload("EDM", new ElecManage.MouldInfo { MODEL_NUMBER = string.IsNullOrEmpty(info.EACT_MODELNO)? "UNKOWN_MODELNO" : info.EACT_MODELNO }, result, info.Elec_Name, _ConfigData);

            if (ps.Count > 1)
            {
                deleteObj.ForEach(u => {
                    Snap.NX.NXObject.Wrap(u).Delete();
                });
            }  
        }

        if (isDeleteDs)
        {
            Snap.NX.NXObject.Wrap(ds.Tag).Delete();
        }


    }

    /// <summary>
    /// 创建注释
    /// </summary>
    void CreateNodeInfo(ElecManage.Electrode elec, EdmDraw.EdmConfig edmConfig)
    {
        var properties = edmConfig.PropertyInfos ?? new List<EdmDraw.EdmConfig.PropertyInfo>();
        var propertiesDic = new Dictionary<string, List<EdmDraw.EdmConfig.PropertyInfo>>();

        properties.ForEach(u => {
            var displayName = u.DisplayName;
            var dValue = string.Empty;
            if (displayName.Contains("-"))
            {
                var splits = displayName.Split('-');
                displayName = splits.FirstOrDefault();
                dValue = splits.LastOrDefault();
            }
            if (!propertiesDic.ContainsKey(displayName))
            {
                propertiesDic.Add(displayName, new List<EdmDraw.EdmConfig.PropertyInfo> { u });
            }
            else
            {
                propertiesDic[displayName].Add(u);
            }
        });

        var elecInfo = elec.GetElectrodeInfo();
        foreach (var item in propertiesDic)
        {
            var displayName = item.Key;
            var pValue = EdmDraw.Helper.GetPropertyValue(elecInfo, displayName) ?? string.Empty;
            var u = item.Value.FirstOrDefault(p => (p.DisplayName.Contains(displayName)&&p.DisplayName.Contains(pValue.ToString()))|| p.DisplayName.Contains(displayName));
            if (u.Ex == "1")
            {
                EdmDraw.DrawBusiness.CreateTick(new Snap.Position(u.LocationX, u.LocationY));
            }
            else
            {
                EdmDraw.DrawBusiness.CreateNode(pValue.ToString(), new Snap.Position(u.LocationX, u.LocationY));
            }
        }
    }

    /// <summary>
    /// 创建表格
    /// </summary>
    List<NXOpen.Tag> CreateTable(EdmDraw.EdmConfig edmConfig,List<ElecManage.PositioningInfo> elecs)
    {
        var result = new List<NXOpen.Tag>();
        //创建表格
        var tableInfo = edmConfig.Table;
        var columnInfos = tableInfo.ColumnInfos;
        var tabularNote = EdmDraw.DrawBusiness.CreateTabnot(
            new Snap.Position(tableInfo.locationX, tableInfo.locationY),
            elecs.Count + 1,
            tableInfo.ColumnInfos.Count,
            tableInfo.RowHeight,
            tableInfo.ColumnWidth,
            edmConfig
            );

        result.Add(tabularNote);

        foreach (var item in elecs)
        {
            var elecIndex = elecs.IndexOf(item)+1;
            foreach (var columnInfo in columnInfos)
            {
                var index = columnInfos.IndexOf(columnInfo);
                if (columnInfo.Ex == "1")
                {
                    var lines=EdmDraw.DrawBusiness.CreatePentagon(
                        new Snap.Position(tableInfo.locationX + ((index * tableInfo.ColumnWidth) + tableInfo.ColumnWidth / 2), tableInfo.locationY - ((elecIndex * tableInfo.RowHeight) + tableInfo.RowHeight / 2))
                        , item.QuadrantType
                        , tableInfo.ColumnWidth * 2 / 3
                        , tableInfo.RowHeight * 2 / 3
                        );
                    result.AddRange(lines);
                }
                else
                {
                    EdmDraw.DraftingHelper.WriteTabularCell(elecIndex, index, EdmDraw.Helper.GetPropertyValue(item, columnInfo.DisplayName).ToString(), tabularNote, tableInfo.RowHeight / 2);
                }
            }
        }

        EdmDraw.DraftingHelper.UpdateTabularNote(tabularNote);
        return result;
    }

    BaseView CreateEACT_TOPView(NXOpen.Drawings.DrawingSheet ds, Snap.NX.Body steel, Snap.Position pos, Snap.Position size, List<ElecManage.PositioningInfo> positionings, EdmDraw.EdmConfig edmConfig)
    {
        var ufSession = NXOpen.UF.UFSession.GetUFSession();
        var selections = new List<TaggedObject>();
        selections.Add(steel);

        positionings.ForEach(p => {
            selections.Add(p.Electrode.ElecBody);
        });

        var topView = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_TOP).Tag, selections, pos, size,edmConfig);
        var topViewRightMargin = EdmDraw.DrawBusiness.GetViewBorder(EdmDraw.ViewBorderType.Right, topView) as Snap.NX.Line;
        var topViewTopMargin = EdmDraw.DrawBusiness.GetViewBorder(EdmDraw.ViewBorderType.Top, topView) as Snap.NX.Line;
        var topViewLeftMargin = EdmDraw.DrawBusiness.GetViewBorder(EdmDraw.ViewBorderType.Left, topView) as Snap.NX.Line;
        var topViewBottomMargin = EdmDraw.DrawBusiness.GetViewBorder(EdmDraw.ViewBorderType.Bottom, topView) as Snap.NX.Line;
        var originPoint = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(Snap.Globals.Wcs.Origin); }, topView.Tag);

        var drawBorderPoints = EdmDraw.DrawBusiness.GetDrawBorderPoint(topView, steel);
        var originPointMTD = EdmDraw.DrawBusiness.MapModelToDrawing(topView.Tag, originPoint.Position);

        var listY = new List<double>();
        var listX = new List<double>();
        var temPoints = new List<Snap.Position>();
        temPoints.Add(originPointMTD);
        temPoints.AddRange(drawBorderPoints);
        temPoints.OrderByDescending(u => u.Y).ToList().ForEach(u => {
            listY.Add(u.Y);
        });
        temPoints.OrderBy(u => u.X).ToList().ForEach(u => {
            listX.Add(u.X);
        });
        listY = listY.Distinct().ToList();
        listX = listX.Distinct().ToList();

        listX.ForEach(u =>
        {
            var tempModel = new double[] { 0, 0, 0 };
            ufSession.View.MapDrawingToModel(topView.Tag, new double[] { u, listY.Min() }, tempModel);
            var tempU = EdmDraw.DrawBusiness.CreateNxObject<Snap.NX.Point>(() => { return Snap.Create.Point(tempModel); }, topView.Tag);
            EdmDraw.DrawBusiness.CreateVerticalOrddimension(
            topView.Tag,
            originPoint.NXOpenTag,
            topViewBottomMargin.NXOpenTag,
            tempU.NXOpenTag
            );

        });

        listY.ForEach(u =>
        {
            var tempModel = new double[] { 0, 0, 0 };
            ufSession.View.MapDrawingToModel(topView.Tag, new double[] { listX.Min(), u }, tempModel);
            var tempU = EdmDraw.DrawBusiness.CreateNxObject<Snap.NX.Point>(() => { return Snap.Create.Point(tempModel); }, topView.Tag);
            EdmDraw.DrawBusiness.CreatePerpendicularOrddimension(
            topView.Tag,
            originPoint.NXOpenTag,
            topViewLeftMargin.NXOpenTag,
            tempU.NXOpenTag
            );
        });

        var tempDic = new Dictionary<ElecManage.PositioningInfo, Snap.NX.Point>();
        var tempMTDDic = new Dictionary<ElecManage.PositioningInfo,Snap.Position>();
        positionings.ForEach(p => {
            var electrode = p.Electrode;
            var elecBasePoint = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(p.X, p.Y, p.Z); }, topView.Tag);
            tempDic.Add(p, elecBasePoint);
        });

        positionings.ForEach(p => {
            var positioningMTD = EdmDraw.DrawBusiness.MapModelToDrawing(topView.Tag, tempDic[p].Position.Array);
            tempMTDDic.Add(p, positioningMTD);
        });

        var psY = positionings.OrderByDescending(p => tempMTDDic[p].Y).ToList();
        double tempY = psY.Count > 0 ? tempMTDDic[psY.First()].Y : 0;

        psY.ForEach(p => {
            var elecBasePoint = tempDic[p];
            var elecBasePointMTD = tempMTDDic[p];
            var line = topViewTopMargin as Snap.NX.Line;

            if (elecBasePointMTD.Y < tempY || Math.Abs(elecBasePointMTD.Y -(edmConfig.DimensionMpr32 * 2 + tempY))<=SnapEx.Helper.Tolerance)
            {
                tempY = elecBasePointMTD.Y;
            }
            Snap.Position origin = new Snap.Position(line.StartPoint.X, tempY);
            var minP = new Snap.Position( line.StartPoint.X, elecBasePointMTD.Y);
            var angle = Snap.Vector.Angle(elecBasePointMTD - minP, elecBasePointMTD - origin);

            var topViewRightElecBasePoint = EdmDraw.DrawBusiness.CreatePerpendicularOrddimension(
                topView.Tag,
                originPoint.NXOpenTag,
                topViewRightMargin.NXOpenTag,
                elecBasePoint.NXOpenTag
                , angle
                , origin
                );

            EdmDraw.DrawBusiness.SetToleranceType(topViewRightElecBasePoint);
            tempY -= edmConfig.DimensionMpr32 * 2;
        });

        var psX = positionings.OrderByDescending(p => tempMTDDic[p].X).ToList();
        double tempX = psX.Count > 0 ? tempMTDDic[psX.First()].X : 0;
        psX.ForEach(p =>
        {
            var index = positionings.IndexOf(p);
            var elecBasePoint = tempDic[p];
            var elecBasePointMTD = tempMTDDic[p];
            var line = topViewTopMargin as Snap.NX.Line;
            var distance = Snap.Compute.Distance(elecBasePointMTD, topViewTopMargin);
            if (elecBasePointMTD.X < tempX || Math.Abs(elecBasePointMTD.X - (edmConfig.DimensionMpr32 * 2 + tempX))<=SnapEx.Helper.Tolerance)
            {
                tempX = elecBasePointMTD.X;
            }
            Snap.Position origin = new Snap.Position(tempX, line.StartPoint.Y);
            var minP = new Snap.Position(elecBasePointMTD.X,line.StartPoint.Y);
            var angle = Snap.Vector.Angle(elecBasePointMTD - minP, elecBasePointMTD - origin);

            var topViewTopElecBasePoint = EdmDraw.DrawBusiness.CreateVerticalOrddimension(
                topView.Tag,
                originPoint.NXOpenTag,
                topViewTopMargin.NXOpenTag,
                elecBasePoint.NXOpenTag
                , angle
                ,origin
                );
            EdmDraw.DrawBusiness.SetToleranceType(topViewTopElecBasePoint);

            tempX -= edmConfig.DimensionMpr32 * 2;
        });

        //positionings.ForEach(p => {
        //    var elecBasePoint = tempDic[p];
        //    var borderSize = topView.GetBorderSize();
        //    var refPoint = topView.GetDrawingReferencePoint();

        //    EdmDraw.DrawBusiness.CreateIdSymbol(p.N, new Snap.Position(refPoint.X - (borderSize.X / 2), refPoint.Y), new Snap.Position(), topView.Tag, elecBasePoint.NXOpenTag);
        //});
        return topView;
    }

    void CreateEACT_FRONTView(NXOpen.Drawings.DrawingSheet ds, List<NXOpen.TaggedObject> selections, Snap.Position pos, Snap.Position size, ElecManage.Electrode electrode,EdmDraw.EdmConfig edmConfig)
    {
        var frontView = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_FRONT).Tag, selections, pos, size,edmConfig);
        var frontViewRightMargin = EdmDraw.DrawBusiness.GetViewBorder(EdmDraw.ViewBorderType.Right, frontView);
        var frontViewLeftMargin = EdmDraw.DrawBusiness.GetViewBorder(EdmDraw.ViewBorderType.Left, frontView);


        var ufSession = NXOpen.UF.UFSession.GetUFSession();
        var originPoint = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(Snap.Globals.Wcs.Origin); }, frontView.Tag);
        var elecBasePoint = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(electrode.GetElecBasePos()); }, frontView.Tag);

        var drawBorderPoints = EdmDraw.DrawBusiness.GetDrawBorderPoint(frontView, Snap.NX.Body.Wrap(selections.First().Tag));
        var listY = Enumerable.Select(drawBorderPoints, u => u.Y).ToList();
        var listX = Enumerable.Select(drawBorderPoints, u => u.X).ToList();
        listY.Add(EdmDraw.DrawBusiness.MapModelToDrawing(frontView.Tag, originPoint.Position).Y);
        listY.Distinct().ToList().ForEach(u => {
            var tempModel = new double[] { 0, 0, 0 };
            ufSession.View.MapDrawingToModel(frontView.Tag, new double[] { listX.Min(), u }, tempModel);
            var tempU = EdmDraw.DrawBusiness.CreateNxObject<Snap.NX.Point>(() => { return Snap.Create.Point(tempModel); }, frontView.Tag);
            EdmDraw.DrawBusiness.CreatePerpendicularOrddimension(
                frontView.Tag,
                originPoint.NXOpenTag,
                frontViewLeftMargin.NXOpenTag,
                tempU.NXOpenTag
                );
        });

        var frontViewOrddimension = EdmDraw.DrawBusiness.CreatePerpendicularOrddimension(
            frontView.Tag,
            originPoint.NXOpenTag,
            frontViewRightMargin.NXOpenTag,
            elecBasePoint.NXOpenTag
            );

        EdmDraw.DrawBusiness.SetToleranceType(frontViewOrddimension);
    }

    void CreateEACT_BOTTOM_FRONTView(NXOpen.Drawings.DrawingSheet ds, List<NXOpen.TaggedObject> selections, Snap.Position pos, Snap.Position size, ElecManage.Electrode electrode,EdmDraw.EdmConfig edmConfig)
    {
        var bottomFrontView = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_BOTTOM_FRONT).Tag, selections, pos, size, edmConfig);
        var baseFace = electrode.BaseFace;
        var bottomFrontViewBorderPoints = EdmDraw.DrawBusiness.GetBorderPoint(bottomFrontView, electrode.ElecBody);

        var bottomFrontViewTopPoint = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(bottomFrontViewBorderPoints[3]); }, bottomFrontView.Tag);
        var bottomFrontViewBottomPoint = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(bottomFrontViewBorderPoints[2]); }, bottomFrontView.Tag);
        //电极前视图
        EdmDraw.DrawBusiness.CreateVerticalDim(
            bottomFrontView.Tag,
            bottomFrontViewTopPoint.NXOpenTag,
            bottomFrontViewBottomPoint.NXOpenTag,
            new Snap.Position(bottomFrontView.GetDrawingReferencePoint().X + (EdmDraw.DrawBusiness.GetBorderSize(bottomFrontView.Tag).X / 2), bottomFrontView.GetDrawingReferencePoint().Y));

        EdmDraw.DrawBusiness.CreateVerticalDim(bottomFrontView.Tag, baseFace.NXOpenTag, bottomFrontViewBottomPoint.NXOpenTag,
            new Snap.Position(bottomFrontView.GetDrawingReferencePoint().X - (EdmDraw.DrawBusiness.GetBorderSize(bottomFrontView.Tag).X / 2), bottomFrontView.GetDrawingReferencePoint().Y));
    }

    void CreateEACT_BOTTOMView(NXOpen.Drawings.DrawingSheet ds, List<NXOpen.TaggedObject> selections, Snap.Position pos, Snap.Position size,ElecManage.Electrode electrode,EdmDraw.EdmConfig edmConfig)
    {
        var bottomView = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_BOTTOM).Tag, selections, pos, size,edmConfig);
        var bottomViewBorderPoints = EdmDraw.DrawBusiness.GetBorderPoint(bottomView, electrode.ElecBody);

        var yPlusSideFace = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(bottomViewBorderPoints[1]); }, bottomView.Tag);
        var yMinusSideFace = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(bottomViewBorderPoints[0]); }, bottomView.Tag);
        var xPlusSideFace = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Line(bottomViewBorderPoints[0], bottomViewBorderPoints[1]); }, bottomView.Tag);
        var xMinusSideFace = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Line(bottomViewBorderPoints[2], bottomViewBorderPoints[3]); }, bottomView.Tag);
        //电极仰视图
        EdmDraw.DrawBusiness.CreateVerticalDim(bottomView.Tag, yPlusSideFace.NXOpenTag, yMinusSideFace.NXOpenTag,
            new Snap.Position(bottomView.GetDrawingReferencePoint().X + (EdmDraw.DrawBusiness.GetBorderSize(bottomView.Tag).X / 2), bottomView.GetDrawingReferencePoint().Y, 0));
        EdmDraw.DrawBusiness.CreatePerpendicularDim(bottomView.Tag, xPlusSideFace.NXOpenTag, xMinusSideFace.NXOpenTag,
            new Snap.Position(bottomView.GetDrawingReferencePoint().X, bottomView.GetDrawingReferencePoint().Y + (EdmDraw.DrawBusiness.GetBorderSize(bottomView.Tag).Y / 2), 0));

        var bottomViewElecHeadBorderPoints = EdmDraw.DrawBusiness.GetBorderPoint(bottomView, electrode.ElecHeadFaces);

        var yPlusElectrodeEdge = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(bottomViewElecHeadBorderPoints[1]); }, bottomView.Tag);
        var yMinusElectrodeEdge = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Point(bottomViewElecHeadBorderPoints[0]); }, bottomView.Tag);
        var xPlusElectrodeEdge = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Line(bottomViewElecHeadBorderPoints[0], bottomViewElecHeadBorderPoints[1]); }, bottomView.Tag);
        var xMinusElectrodeEdge = EdmDraw.DrawBusiness.CreateNxObject(() => { return Snap.Create.Line(bottomViewElecHeadBorderPoints[2], bottomViewElecHeadBorderPoints[3]); }, bottomView.Tag);

        EdmDraw.DrawBusiness.CreateVerticalDim(bottomView.Tag, yPlusElectrodeEdge.NXOpenTag, yMinusElectrodeEdge.NXOpenTag,
            new Snap.Position(bottomView.GetDrawingReferencePoint().X - (EdmDraw.DrawBusiness.GetBorderSize(bottomView.Tag).X / 2), bottomView.GetDrawingReferencePoint().Y, 0));
        EdmDraw.DrawBusiness.CreatePerpendicularDim(bottomView.Tag, xPlusElectrodeEdge.NXOpenTag, xMinusElectrodeEdge.NXOpenTag,
           new Snap.Position(bottomView.GetDrawingReferencePoint().X, bottomView.GetDrawingReferencePoint().Y - (EdmDraw.DrawBusiness.GetBorderSize(bottomView.Tag).Y / 2), 0));

    }

    void CreateEACT_BOTTOM_ISOMETRICView(NXOpen.Drawings.DrawingSheet ds, List<NXOpen.TaggedObject> selections, Snap.Position pos, Snap.Position size, EdmDraw.EdmConfig edmConfig)
    {
        var view = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_BOTTOM_ISOMETRIC).Tag, selections, pos, size,edmConfig);
    }

    void CreateEACT_ISOMETRICView(NXOpen.Drawings.DrawingSheet ds, List<NXOpen.TaggedObject> selections, Snap.Position pos, Snap.Position size, EdmDraw.EdmConfig edmConfig)
    {
        var view = EdmDraw.DrawBusiness.CreateBaseView(ds, GetModelingView(EdmDraw.ViewType.EACT_ISOMETRIC).Tag, selections, pos, size,edmConfig);
    }

    ModelingView GetModelingView(EdmDraw.ViewType viewType) 
    {
        var name=Enum.GetName(viewType.GetType(),viewType);
        var result= Snap.Globals.WorkPart.NXOpenPart.ModelingViews.ToArray().FirstOrDefault(u => u.Name == name);
        return result;
    }

    void InitModelingView(EdmDraw.EdmConfig edmConfig)
    {
        SnapEx.Create.ApplicationSwitchRequest(SnapEx.ApplicationType.MODELING);
        var draftViewLocations = edmConfig.DraftViewLocations ?? new List<EdmDraw.EdmConfig.DraftViewLocation>();
        foreach (var item in draftViewLocations)
        {
            var viewType = EdmDraw.DrawBusiness.GetEumnViewType(item.ViewType);
            EdmDraw.DrawBusiness.CreateCamera(viewType, new double[] { item.Xx, item.Xy, item.Xz, item.Yx, item.Yy, item.Yz });
        }
    }
}
