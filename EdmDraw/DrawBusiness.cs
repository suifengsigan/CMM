using NXOpen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapEx;

namespace EdmDraw
{
    public enum ViewType 
    {
        EACT_TOP,
        EACT_FRONT,
        EACT_BOTTOM,
        EACT_ISOMETRIC,
        EACT_BOTTOM_FRONT,
        EACT_BOTTOM_ISOMETRIC,
        EACT_ELEFT,
        EACT_ERIGHT,
        EACT_EREAL
    }

    public enum ViewBorderType 
    {
        Top,
        Right
    }

    public class DrawBusiness
    {
        public static NXOpen.Tag CreateVerticalOrddimension(NXOpen.Tag baseView, NXOpen.Tag originObj, NXOpen.Tag marginObj, NXOpen.Tag dimensionObj, double dAngle = 0, Snap.Position? origin = null, double dDistance = 0)
        {
            return CreateOrddimension(1, baseView, originObj, marginObj, dimensionObj,dAngle,dDistance,origin);
        }

        public static NXOpen.Tag CreatePerpendicularOrddimension(NXOpen.Tag baseView, NXOpen.Tag originObj, NXOpen.Tag marginObj, NXOpen.Tag dimensionObj, double dAngle = 0, Snap.Position? origin = null, double dDistance = 0)
        {
            return CreateOrddimension(2, baseView, originObj, marginObj, dimensionObj, dAngle, dDistance,origin);
        }

        /// <summary>
        /// 创建对勾
        /// </summary>
        public static void CreateTick(Snap.Position pos)
        {
            var dis = 3;
            var vector1 = Snap.Vector.Unit(new Snap.Vector(-1, 1, 0));
            var vector2 = Snap.Vector.Unit(new Snap.Vector(1, 1, 0));
            CreateNxObject(() => { return Snap.Create.Line(pos, pos + dis * vector1); }, NXOpen.Tag.Null, false).IsHidden = false;
            CreateNxObject(() => { return Snap.Create.Line(pos, pos + (dis*1.5) * vector2); }, NXOpen.Tag.Null, false).IsHidden = false;
        }

        /// <summary>
        /// 创建C角
        /// </summary>
        public static void CreatePentagon(Snap.Position pos,QuadrantType type,double width=3,double height=2) 
        {
            var temp = width / 2;
            var temp2 = height / 2;
            var points = new List<Snap.Position>();
            var leftUP = new Snap.Position(pos.X - temp, pos.Y + temp);
            var rightUP = new Snap.Position(pos.X + temp, pos.Y + temp);
            var rightDown = new Snap.Position(pos.X + temp, pos.Y - temp);
            var leftDown = new Snap.Position(pos.X - temp, pos.Y - temp);

            switch (type)
            {
                case QuadrantType.First:
                    {
                        points.Add(leftUP);
                        points.Add(new Snap.Position(rightUP.X - temp2, rightUP.Y));
                        points.Add(new Snap.Position(rightUP.X, rightUP.Y - temp2));
                        //points.Add(rightUP);
                        points.Add(rightDown);
                        points.Add(leftDown);
                        break;
                    }
                case QuadrantType.Second:
                    {
                        points.Add(new Snap.Position(leftUP.X, leftUP.Y - temp2));
                        points.Add(new Snap.Position(leftUP.X + temp2, leftUP.Y));
                        //points.Add(leftUP);
                        points.Add(rightUP);
                        points.Add(rightDown);
                        points.Add(leftDown);
                        break;
                    }
                case QuadrantType.Three:
                    {
                        points.Add(leftUP);
                        points.Add(rightUP);
                        points.Add(rightDown);
                        points.Add(new Snap.Position(leftDown.X + temp2, leftDown.Y));
                        points.Add(new Snap.Position(leftDown.X, leftDown.Y + temp2));
                        //points.Add(leftDown);
                        break;
                    }
                case QuadrantType.Four:
                    {
                        points.Add(leftUP);
                        points.Add(rightUP);
                        points.Add(new Snap.Position(rightDown.X, rightDown.Y + temp2));
                        points.Add(new Snap.Position(rightDown.X - temp2, rightDown.Y));
                        //points.Add(rightDown);
                        points.Add(leftDown);
                        break;
                    }
            }

            

            for (int i = 0; i < points.Count; i++)
            {
                var firstPos = points[i];
                var twoPos = points[i];
                if (i == points.Count - 1)
                {
                    twoPos = points[0];
                }
                else
                {
                    twoPos = points[i + 1];
                }

                CreateNxObject(() => { return Snap.Create.Line(firstPos, twoPos); }, NXOpen.Tag.Null, false).IsHidden = false;
            }
        }

        /// <summary>
        /// 创建表格
        /// </summary>
        public static NXOpen.Tag CreateTabnot(Snap.Position origin,int rowCount,int columnCount,double rowHeight,double columnWidth,EdmConfig edmConfig) 
        {
            var workPart = Snap.Globals.WorkPart.NXOpenPart;
            var theUFSession = NXOpen.UF.UFSession.GetUFSession();
            var tableParam = new NXOpen.UF.UFDraw.TabnotParams();

            //设置单元格默认样式
            NXOpen.UF.UFTabnot.CellPrefs cellPrefs=new NXOpen.UF.UFTabnot.CellPrefs();
            theUFSession.Tabnot.AskDefaultCellPrefs(out cellPrefs);
            cellPrefs.nm_fit_methods = 2;
            cellPrefs.fit_methods[0] = NXOpen.UF.UFTabnot.FitMethod.FitMethodAutoSizeText;//UF_TABNOT_fit_method_auto_size_text;
            cellPrefs.fit_methods[1] = NXOpen.UF.UFTabnot.FitMethod.FitMethodAutoSizeRow; //UF_TABNOT_fit_method_auto_size_row;
            cellPrefs.text_density = 3;
            cellPrefs.zero_display = NXOpen.UF.UFTabnot.ZeroDisplay.ZeroDisplayZero;
            theUFSession.Tabnot.SetDefaultCellPrefs(ref cellPrefs);

            //创建的表格信息
            tableParam.position = new double[3];
            tableParam.position[0] = origin.X;
            tableParam.position[1] = origin.Y;
            tableParam.position[2] = origin.Z;
            tableParam.range_start.row = 1;
            tableParam.range_start.col = 1;
            tableParam.range_end.row = rowCount;
            tableParam.range_end.col = columnCount;

            //标题;
            tableParam.title_cell.row = 0;

            //标题;
            tableParam.title_cell.col = 0;
            tableParam.ug_aspect_ratio = 1.0;
            tableParam.border_type = NXOpen.UF.UFDraw.TabnotBorderType.TabnotBorderTypeSingle;
            tableParam.border_width = 0;

            //标题;
            tableParam.use_title_cell = false;

            //网络线使用;
            tableParam.use_grid_lines = true;
            tableParam.use_vert_grid_lines = true;
            tableParam.use_horiz_grid_lines = true;
            tableParam.use_row_hdr_grid_lines = true;
            tableParam.use_col_hdr_grid_lines = true;
            tableParam.auto_size_cells = false;

            NXOpen.Tag tabularNote = NXOpen.Tag.Null;
            theUFSession.Draw.CreateTabularNote(ref tableParam, out tabularNote);

            //设置列宽
            for (int i = 0; i < columnCount; i++)
            {
                DraftingHelper.SetTabularColumnWidth(i, columnWidth, tabularNote);
            }

            //设置行高
            for (int i = 0; i < rowCount; i++)
            {
                DraftingHelper.SetTabularRowHeight(i, rowHeight, tabularNote);
            }

            var columnInfos = edmConfig.Table.ColumnInfos;
            columnInfos.ForEach(u => {
                DraftingHelper.WriteTabularCell(0, columnInfos.IndexOf(u), u.DisplayName, tabularNote, rowHeight / 2);
            });
            
            return tabularNote;

            //var tabnot=theUFSession.Tabnot;
            //var section_prefs = new NXOpen.UF.UFTabnot.SectionPrefs();
            //theUFSession.Tabnot.AskDefaultSectionPrefs(out section_prefs);
            //section_prefs.border_width = 60;
            //var tabular_note = NXOpen.Tag.Null;
            //theUFSession.Tabnot.Create(ref section_prefs, origin.Array, out tabular_note);

            //var tabel=NXOpen.Utilities.NXObjectManager.Get(tabular_note) as NXOpen.Annotations.Table;
            //var tableSectionBuilder1 = workPart.Annotations.TableSections.CreateTableSectionBuilder(null);
            //tableSectionBuilder1.NumberOfRows = rowCount;
            //tableSectionBuilder1.NumberOfColumns = columnCount;
            //tableSectionBuilder1.ColumnWidth = columnWidth;
            //tableSectionBuilder1.RowHeight = rowHeight;
            //tableSectionBuilder1.Origin.OriginPoint = origin;
            //tableSectionBuilder1.Commit();
            //tableSectionBuilder1.Destroy();
        }



        /// <summary>
        /// 创建ID符号
        /// </summary>
        public static NXOpen.Tag CreateIdSymbol(string text,Snap.Position origin,Snap.Position pos,NXOpen.Tag view,NXOpen.Tag objectTag)
        {

            var theUFSession = NXOpen.UF.UFSession.GetUFSession();
            var dd = new NXOpen.UF.UFDrf.SymbolPreferences();
            theUFSession.Drf.AskSymbolPreferences(ref dd);
            dd.id_symbol_size = 6;
            theUFSession.Drf.SetSymbolPreferences(ref dd);

            NXOpen.UF.UFDrf.Object object1 = new NXOpen.UF.UFDrf.Object();

            object1.object_tag =objectTag;
            object1.object_view_tag = view;
            object1.object_assoc_type = NXOpen.UF.UFDrf.AssocType.EndPoint;
            object1.object_assoc_modifier = NXOpen.UF.UFConstants.UF_DRF_first_end_point;
            
            var result=NXOpen.Tag.Null;
            theUFSession.Drf.CreateIdSymbol(NXOpen.UF.UFDrf.IdSymbolType.SymCircle, text, "",
                origin.Array, NXOpen.UF.UFDrf.LeaderMode.WithLeader, NXOpen.UF.UFDrf.LeaderAttachType.LeaderAttachObject,
                ref object1,
                pos.Array,
                out result
                );

            return result;
        }

        /// <summary>
        /// 创建注释
        /// </summary>
        public static NXOpen.Tag CreateNode(string text, Snap.Position origin)
        {
            var theUFSession = NXOpen.UF.UFSession.GetUFSession();
            var result = NXOpen.Tag.Null;
            theUFSession.Drf.CreateNote(1, new string[] { text }, origin.Array, 0, out result);
            return result;
        }

        /// <summary>
        /// 创建坐标尺寸
        /// </summary>
        static NXOpen.Tag CreateOrddimension(int type, NXOpen.Tag baseView, NXOpen.Tag originObj, NXOpen.Tag marginObj, NXOpen.Tag dimensionObj, double dAngle = 0, double dDistance = 50, Snap.Position? origin = null) 
        {
            var theUFSession = NXOpen.UF.UFSession.GetUFSession();
            //创建坐标标注的原点
            NXOpen.UF.UFDrf.Object object1 = new NXOpen.UF.UFDrf.Object();

            object1.object_tag = originObj;
            object1.object_view_tag = baseView;
            object1.object_assoc_type = NXOpen.UF.UFDrf.AssocType.EndPoint;
            object1.object_assoc_modifier = NXOpen.UF.UFConstants.UF_DRF_first_end_point;

            var orginTag = NXOpen.Tag.Null;
            theUFSession.Drf.CreateOrdorigin(ref object1, 1, 1, 2, "test", out orginTag);

            //创建坐标标注的尺寸边缘(留边)
            NXOpen.UF.UFDrf.Object object2 = new NXOpen.UF.UFDrf.Object();
            object2.object_tag = marginObj;
            object2.object_view_tag = baseView;
            object2.object_assoc_type = NXOpen.UF.UFDrf.AssocType.EndPoint;
            object2.object_assoc_modifier = NXOpen.UF.UFConstants.UF_DRF_first_end_point;

            var marginTag = NXOpen.Tag.Null;
            theUFSession.Drf.CreateOrdmargin(type, orginTag, ref object2, null, null, 0, out marginTag);


            NXOpen.UF.UFDrf.Object object3 = new NXOpen.UF.UFDrf.Object();
            object3.object_tag = dimensionObj;
            object3.object_view_tag = baseView;
            object3.object_assoc_type = NXOpen.UF.UFDrf.AssocType.EndPoint;
            object3.object_assoc_modifier = NXOpen.UF.UFConstants.UF_DRF_first_end_point;

            NXOpen.UF.UFDrf.Text text = new NXOpen.UF.UFDrf.Text();
            double[] origin3d = (origin ?? Snap.Position.Origin).Array;
            NXOpen.Tag result;
            theUFSession.Drf.CreateOrddimension(marginTag, type, ref object3, dAngle, dDistance, ref text, 2, origin3d, out result);

            return result;
        }

        public static void SetShowLayers(List<Snap.NX.NXObject> objs) 
        {
            var list = new List<int>();
            objs.ForEach(u=>{
                list.Add(u.Layer);
            });
            SetShowLayers(list.Distinct().ToList());
        }

        public static void SetShowLayers(List<int> showLayers,int workLayer=230)
        {
            var ufSession = NXOpen.UF.UFSession.GetUFSession();
            SetLayerState(workLayer, Snap.Globals.LayerState.WorkLayer);
            foreach(var item in GetAllLayer())
            {
                if (item.Key == workLayer) { }
                else if (showLayers.Where(u => u == item.Key).Count() > 0)
                {
                    SetLayerState(item.Key, Snap.Globals.LayerState.Selectable);
                }
                else 
                {
                    SetLayerState(item.Key, Snap.Globals.LayerState.Hidden);
                }
            }
        }

        private static void SetLayerState(int layer, Snap.Globals.LayerState state) 
        {
            if (Snap.Globals.LayerStates[layer] != state) 
            {
                Snap.Globals.LayerStates[layer] = state;
            }
        }

        public static void SetAllLayer(Dictionary<int, Snap.Globals.LayerState> dic) 
        {
            foreach (var item in dic)
            {
                Snap.Globals.LayerStates[item.Key] = item.Value;
            }
        }

        public static Dictionary<int, Snap.Globals.LayerState> GetAllLayer() 
        {
            var dic = new Dictionary<int, Snap.Globals.LayerState>();
            var layerStates = Snap.Globals.LayerStates;
            for (int i = 1; i <= 256; i++) 
            {

                dic.Add(i, layerStates[i]);
            }

            return dic;
        }

        public static void SetDrawSheetLayer(NXOpen.Drawings.DrawingSheet ds,int newLayer) 
        {
            var temp = NXOpen.Tag.Null;
            var ufSession = NXOpen.UF.UFSession.GetUFSession();
            var templist = new List<NXOpen.Tag>();
            do
            {
                ufSession.View.CycleObjects(ds.View.Tag, NXOpen.UF.UFView.CycleObjectsEnum.VisibleObjects, ref temp);
                if (temp != NXOpen.Tag.Null)
                {
                    templist.Add(temp);
                }
            } while (temp != NXOpen.Tag.Null);

            Snap.NX.NXObject.MoveToLayer(newLayer, Enumerable.Select(templist, u => Snap.NX.NXObject.Wrap(u)).ToArray());
            var state = Snap.Globals.LayerStates[newLayer];
            if (state == Snap.Globals.LayerState.Hidden) 
            {
                Snap.Globals.LayerStates[newLayer] = Snap.Globals.LayerState.Visible;
            }
        }

        /// <summary>
        /// 创建基本视图
        /// </summary>
        public static NXOpen.Drawings.BaseView CreateBaseView(NXOpen.Drawings.DrawingSheet ds, NXOpen.Tag modelViewTag, List<NXOpen.TaggedObject> selections, Snap.Position pos, Snap.Position size)
        {
            var sList = Enumerable.Select(selections, u => Snap.NX.NXObject.Wrap(u.Tag).Layer).ToList();
            SetShowLayers(sList, 254);
            sList.Add(254);
            var workPart = NXOpen.Session.GetSession().Parts.Work;
            Snap.NX.Part snapWorkPart = workPart;
            var theUFSession = NXOpen.UF.UFSession.GetUFSession();
            NXOpen.UF.UFDraw.ViewInfo view_info;
            theUFSession.Draw.InitializeViewInfo(out view_info);
            double[] dwg_point = { pos.X, pos.Y };
            Tag draw_view_tag;
            theUFSession.Draw.ImportView(ds.Tag, modelViewTag, dwg_point, ref view_info, out draw_view_tag);
            string viewName;
            theUFSession.Obj.AskName(draw_view_tag, out viewName);

            var allObj = new List<NXOpen.NXObject>();
            allObj.AddRange(Enumerable.Select(workPart.Bodies.ToArray().Where(u => sList.Contains(u.Layer)),m=>m as NXObject));
            allObj.AddRange(Enumerable.Select(workPart.Points.ToArray().Where(u => sList.Contains(u.Layer)), m => m as NXObject));
            allObj.AddRange(Enumerable.Select(workPart.Lines.ToArray().Where(u => sList.Contains(u.Layer)), m => m as NXObject));
            allObj.AddRange(Enumerable.Select(workPart.Curves.ToArray().Where(u => sList.Contains(u.Layer)), m => m as NXObject));
            allObj.AddRange(Enumerable.Select(workPart.Sketches.ToArray().Where(u => sList.Contains(u.Layer)), m => m as NXObject));
            allObj.AddRange(Enumerable.Select(workPart.Axes.ToArray().Where(u => sList.Contains(u.Layer)), m => m as NXObject));
            allObj.AddRange(Enumerable.Select(workPart.Datums.ToArray().Where(u => sList.Contains(u.Layer)), m => m as NXObject));

            allObj.Distinct().ToList().ForEach(u =>
            {
                if (selections.Where(m => u.Tag == m.Tag).Count() <= 0)
                {
                    SnapEx.Ex.UC6400(viewName, u.Tag);
                }
            });
            theUFSession.Draw.DefineBoundByObjects(draw_view_tag, selections.Count, Enumerable.Select(selections, u => u.Tag).ToArray());

            theUFSession.Draw.UpdateOneView(ds.Tag, draw_view_tag);

            var borderSize = GetBorderSize(draw_view_tag);
            theUFSession.Draw.SetViewScale(draw_view_tag, new double[] { size.X / borderSize.X, size.Y / borderSize.Y }.Min());
            theUFSession.Draw.MoveView(draw_view_tag, dwg_point);
            theUFSession.Draw.UpdateOneView(ds.Tag, draw_view_tag);
            
            return NXOpen.Utilities.NXObjectManager.Get(draw_view_tag) as NXOpen.Drawings.BaseView;
        }


        /// <summary>
        ///  获取边界尺寸
        /// </summary>
        public static NXOpen.Point2d GetBorderSize(NXOpen.Tag tag)
        {
            var view_borders = new double[4];
            NXOpen.UF.UFSession.GetUFSession().Draw.AskViewBorders(tag, view_borders);
            var size = new NXOpen.Point2d();
            size.Y = Math.Abs(view_borders[3] - view_borders[1]);
            size.X = Math.Abs(view_borders[2] - view_borders[0]);
            return size;
        }

        /// <summary>
        /// 创建水平尺寸
        /// </summary>
        public static NXOpen.Tag CreateVerticalDim(NXOpen.Tag draw_view_tag, NXOpen.Tag o1, NXOpen.Tag o2, Snap.Position origin)
        {
            var theUFSession = NXOpen.UF.UFSession.GetUFSession();
            NXOpen.UF.UFDrf.Object object1 = new NXOpen.UF.UFDrf.Object();

            object1.object_tag = o1;
            object1.object_view_tag = draw_view_tag;
            object1.object_assoc_type = NXOpen.UF.UFDrf.AssocType.EndPoint;
            object1.object_assoc_modifier = NXOpen.UF.UFConstants.UF_DRF_first_end_point;

            NXOpen.UF.UFDrf.Object object2 = new NXOpen.UF.UFDrf.Object();
            object2.object_tag = o2;
            object2.object_view_tag = draw_view_tag;
            object2.object_assoc_type = NXOpen.UF.UFDrf.AssocType.EndPoint;
            object2.object_assoc_modifier = NXOpen.UF.UFConstants.UF_DRF_first_end_point;

            NXOpen.UF.UFDrf.Text text = new NXOpen.UF.UFDrf.Text();

            NXOpen.Tag result;
            theUFSession.Drf.CreateVerticalDim(ref object1, ref object2, ref text, new double[] { origin.X, origin.Y, 0 }, out result);

            var environmentData = new DraftingEnvironmentData();
            theUFSession.Drf.AskObjectPreferences(result, environmentData.mpi, environmentData.mpr, out environmentData.radiusValue, out environmentData.diameterValue);
            environmentData.mpi[9] = 3;
            theUFSession.Drf.SetObjectPreferences(result, environmentData.mpi, environmentData.mpr, environmentData.radiusValue, environmentData.diameterValue);
            return result;
        }


        /// <summary>
        /// 创建垂直尺寸
        /// </summary>
        public static NXOpen.Tag CreatePerpendicularDim(NXOpen.Tag draw_view_tag, NXOpen.Tag o1, NXOpen.Tag o2, Snap.Position origin)
        {
            var theUFSession = NXOpen.UF.UFSession.GetUFSession();
            NXOpen.UF.UFDrf.Object object1 = new NXOpen.UF.UFDrf.Object();

            object1.object_tag = o1;
            object1.object_view_tag = draw_view_tag;
            object1.object_assoc_type = NXOpen.UF.UFDrf.AssocType.EndPoint;
            object1.object_assoc_modifier = NXOpen.UF.UFConstants.UF_DRF_first_end_point;

            NXOpen.UF.UFDrf.Object object2 = new NXOpen.UF.UFDrf.Object();
            object2.object_tag = o2;
            object2.object_view_tag = draw_view_tag;
            object2.object_assoc_type = NXOpen.UF.UFDrf.AssocType.EndPoint;
            object2.object_assoc_modifier = NXOpen.UF.UFConstants.UF_DRF_first_end_point;

            NXOpen.UF.UFDrf.Text text = new NXOpen.UF.UFDrf.Text();

            NXOpen.Tag result;
            theUFSession.Drf.CreatePerpendicularDim(ref object1, ref object2, ref text, new double[] { origin.X, origin.Y, 0 }, out result);
            var environmentData = new DraftingEnvironmentData();
            theUFSession.Drf.AskObjectPreferences(result, environmentData.mpi, environmentData.mpr, out environmentData.radiusValue, out environmentData.diameterValue);
            environmentData.mpi[9] = 3;
            theUFSession.Drf.SetObjectPreferences(result, environmentData.mpi, environmentData.mpr, environmentData.radiusValue, environmentData.diameterValue);
            return result;
        }

        /// <summary>
        /// 创建摄像机
        /// </summary>
        public static ModelingView CreateCamera(ViewType viewType,double[] martrix) 
        {
            var viewName = Enum.GetName(viewType.GetType(), viewType);
            Session theSession = Session.GetSession();
            Part workPart = theSession.Parts.Work;
            Part displayPart = theSession.Parts.Display;

            var modelingView = workPart.ModelingViews.ToArray().FirstOrDefault(u => u.Name == viewName);

            if (modelingView != null)
            {
                Snap.NX.NXObject.Wrap(modelingView.Tag).Delete();
                modelingView = null;
            }

            if (modelingView == null) 
            {
                var ufSession = NXOpen.UF.UFSession.GetUFSession();
                var or = new Snap.Orientation(
                        new Snap.Vector(martrix[0], martrix[1], martrix[2])
                        , new Snap.Vector(martrix[3], martrix[4], martrix[5]));
                var ds = new List<double>();
                ds.AddRange(or.AxisX.Array);
                ds.AddRange(or.AxisY.Array);
                SnapEx.Ex.UC6434("", 4, NXOpen.Tag.Null, ds.ToArray());
                //ufSession.View.SetViewMatrix("", 4, NXOpen.Tag.Null, martrix);

                #region createCamera Code
                NXOpen.Tag workView;
                ufSession.View.AskWorkView(out workView);
                string workViewName;
                ufSession.Obj.AskName(workView, out workViewName);

                SnapEx.Ex.UC6450(workViewName, viewName, 0, 0);
                SnapEx.Ex.UC6449(workViewName);

                //NXOpen.Display.Camera nullDisplay_Camera = null;
                //NXOpen.Display.CameraBuilder cameraBuilder1;
                //cameraBuilder1 = workPart.Cameras.CreateCameraBuilder(nullDisplay_Camera);
                //cameraBuilder1.Commit();
                //cameraBuilder1.CameraName = viewName;
                //cameraBuilder1.CameraNameChar = viewName;
                //cameraBuilder1.Commit();
                //cameraBuilder1.Destroy();
                #endregion
            }

            modelingView = workPart.ModelingViews.ToArray().FirstOrDefault(u => u.Name == viewName);

            return modelingView;
        }

        public static Snap.NX.NXObject GetViewBorder(EdmDraw.ViewBorderType borderType, NXOpen.Drawings.DraftingView view)
        {
            Snap.NX.NXObject result = null;
            Snap.Position viewCenterPoint = view.GetDrawingReferencePoint();
            var viewBorderSize = view.GetBorderSize();
            switch (borderType)
            {
                case EdmDraw.ViewBorderType.Right:
                    {
                        Snap.Position xLeft = viewCenterPoint;
                        xLeft.X += viewBorderSize.X / 2;
                        xLeft.Y += viewBorderSize.Y / 2;
                        Snap.Position xRight = viewCenterPoint;
                        xRight.X += viewBorderSize.X / 2;
                        xRight.Y -= viewBorderSize.Y / 2;
                        result = CreateNxObject<Snap.NX.Line>(() => { return Snap.Create.Line(xLeft, xRight); }, view.Tag, false);
                        break;
                    }
                case EdmDraw.ViewBorderType.Top:
                    {
                        Snap.Position xLeft = viewCenterPoint;
                        xLeft.X -= viewBorderSize.X / 2;
                        xLeft.Y += viewBorderSize.Y / 2;
                        Snap.Position xRight = viewCenterPoint;
                        xRight.X += viewBorderSize.X / 2;
                        xRight.Y += viewBorderSize.Y / 2;
                        result = CreateNxObject<Snap.NX.Line>(() => { return Snap.Create.Line(xLeft, xRight); }, view.Tag, false);
                        break;
                    }
            }
            return result;
        }

       public static List<Snap.Position> GetBorderPoint(NXOpen.Drawings.DraftingView view, List<Snap.NX.Face> faces)
        {
            var list = new List<Snap.Position>();
            faces.ForEach(u =>
            {
                list.AddRange(u.Box.Corners.Distinct());
            });
            return GetBorderPoint(view, list);
        }

        public static List<Snap.Position> GetBorderPoint(NXOpen.Drawings.DraftingView view, Snap.NX.Body steel)
        {
            var box = steel.Box;
            var list = new List<Snap.Position>();
            var xs = new List<double> { box.MinX, box.MaxX };
            var ys = new List<double> { box.MinY, box.MaxY };
            var zs = new List<double> { box.MinZ, box.MaxZ };
            xs.ForEach(x =>
            {
                ys.ForEach(y =>
                {
                    zs.ForEach(z =>
                    {
                        list.Add(new Snap.Position(x, y, z));
                    });
                });
            });

            return GetBorderPoint(view, list);
        }


        static List<Snap.Position> GetBorderPoint(NXOpen.Drawings.DraftingView view, List<Snap.Position> listPos)
        {
            var result = new List<Snap.Position>();
            var ufSession = NXOpen.UF.UFSession.GetUFSession();
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            listPos.ForEach(u =>
            {
                var tempMap = new double[] { 0, 0 };
                ufSession.View.MapModelToDrawing(view.Tag, u.Array, tempMap);
                minX = Math.Min(tempMap[0], minX);
                maxX = Math.Max(tempMap[0], maxX);
                minY = Math.Min(tempMap[1], minY);
                maxY = Math.Max(tempMap[1], maxY);
            });

            var xs = new List<double> { minX, maxX };
            var ys = new List<double> { minY, maxY };
            xs.ForEach(x =>
            {
                ys.ForEach(y =>
                {
                    var model_pt = new double[] { 0, 0, 0 };
                    ufSession.View.MapDrawingToModel(view.Tag, new double[] { x, y }, model_pt);
                    result.Add(new Snap.Position(model_pt));
                });
            });
            return result;
        }



        public static void SetToleranceType(NXOpen.Tag tag)
        {
            var ufSession = NXOpen.UF.UFSession.GetUFSession();
            var eData = new DraftingEnvironmentData();
            ufSession.Drf.AskObjectPreferences(tag, eData.mpi, eData.mpr, out eData.radiusValue, out eData.diameterValue);
            //设置公差类型 首选项→尺寸→公差→类型
            eData.mpi[6] = 8;
            ufSession.Drf.SetObjectPreferences(tag, eData.mpi, eData.mpr, eData.radiusValue, eData.diameterValue);
            //Session theSession = Session.GetSession();
            //Part workPart = theSession.Parts.Work;
            //Part displayPart = theSession.Parts.Display;
            //var obj = NXOpen.Utilities.NXObjectManager.Get(tag) as NXOpen.Annotations.OrdinateDimension;
            //if (obj != null)
            //{
            //    var ordinateDimensionBuilder1 = workPart.Dimensions.CreateOrdinateDimensionBuilder(obj);
            //    //尺寸样式
            //    ordinateDimensionBuilder1.Style.DimensionStyle.ToleranceType = NXOpen.Annotations.ToleranceType.Basic;

            //    ordinateDimensionBuilder1.Commit();
            //    ordinateDimensionBuilder1.Destroy();
            //}
        }

        public static T CreateNxObject<T>(Func<T> action, NXOpen.Tag viewTag, bool isExpandView = true) where T : Snap.NX.NXObject
        {
            var ufSession = NXOpen.UF.UFSession.GetUFSession();
            if (isExpandView)
            {
                ufSession.View.ExpandView(viewTag);
            }

            var result = action();
            result.IsHidden = true;

            if (isExpandView)
            {
                ufSession.View.UnexpandWorkView();
            }
            return result;
        }

        /// <summary>
        /// 制图首选项初始化
        /// </summary>
        public static void InitPreferences(EdmConfig edmConfig) 
        {
            var ufSession = NXOpen.UF.UFSession.GetUFSession();
            int[] mpi=new int[100];
            double[] mpr=new double[70];
            string radius_value=string.Empty;
            string diameter_value=string.Empty;
            ufSession.Drf.AskPreferences(mpi,mpr,out radius_value,out diameter_value);
            var textStyle = new Snap.NX.TextStyle();
            textStyle.SetFont(edmConfig.TextMpi88, Snap.NX.TextStyle.FontType.NX);
            textStyle.AlignmentPosition = Snap.NX.TextStyle.AlignmentPositions.MidCenter;

            var dimensionStyle = new Snap.NX.TextStyle();
            dimensionStyle.SetFont(edmConfig.DimensionMpi85, Snap.NX.TextStyle.FontType.NX);
            dimensionStyle.AlignmentPosition = Snap.NX.TextStyle.AlignmentPositions.TopCenter;

            //文字对齐位置 首选项→公共→文字→对齐位置
            mpi[30] = (int)textStyle.AlignmentPosition;
            //文字样式 首选项→公共→文字→文本参数→字体(将字体设置为blockfont)
            mpi[88] = textStyle.FontIndex;
            //文字样式 首选项→公共→文字→文本参数→设置字宽(粗细)
            mpi[89] = edmConfig.TextMpi89;

            //字大小
            mpr[44] = edmConfig.TextMpr44;
            //文本长宽比
            mpr[45] = edmConfig.TextMpr45;
            //字体间距
            mpr[46] = edmConfig.TextMpr46;

            //尺寸小数位数 首选项→尺寸→文本→单位→小数位数
            mpi[3] = edmConfig.DimensionMpi3;

            //设置公差类型 首选项→尺寸→公差→类型
            //mpi[6] = edmConfig.DimensionMpi3;

            //设置尺寸文本方向 首选项→尺寸→文本→方向和位置→方向
            mpi[9] = (int)dimensionStyle.AlignmentPosition; ;

            //小数点样式 首选项→尺寸→文本→单位→小数分隔符
            //mpi[16] = 1;

            //首选项→尺寸→文本→附加文件→设置字体
            mpi[85] = dimensionStyle.FontIndex;
            //首选项→尺寸→文本→附加文件→设置字宽(粗细)
            mpi[86] = edmConfig.DimensionMpi86;
            
            //尺寸字大小
            mpr[32] = edmConfig.DimensionMpr32;
            //尺寸长宽比
            mpr[33] = edmConfig.DimensionMpr33;
            //尺寸间距
            mpr[34] = edmConfig.DimensionMpr34;

            //首选项→尺寸→文本→单位→显示后置零(默认显示尾零)
            mpi[90] = edmConfig.DimensionMpi90;

            ufSession.Drf.SetPreferences(mpi, mpr, radius_value, diameter_value);  
        }
    }
}
