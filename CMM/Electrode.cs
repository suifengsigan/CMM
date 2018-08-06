using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapEx;
using NXOpen.UF;
using Snap;

namespace CMMProgram
{
    /// <summary>
    /// 电极信息
    /// </summary>
    public class Electrode
    {
        Snap.NX.Body _body;
        List<Snap.NX.Face> _faces = new List<Snap.NX.Face>();
        /// <summary>
        /// 坐标系
        /// </summary>
        Snap.Orientation _orientation = new Snap.Orientation(new Snap.Vector(0, 0, 1));
        /// <summary>
        /// 公差
        /// </summary>
        const double _tolerance = 0.0001;
        /// <summary>
        /// 水平基准面
        /// </summary>
        Snap.NX.Face _horizontalDatumFace { get; set; }
        /// <summary>
        /// 基准底面
        /// </summary>
        Snap.NX.Face _datumBottomFace { get; set; }

        public Electrode(Snap.NX.Body body)
        {
            _body = body;
            _faces = _body.Faces.ToList();
            _horizontalDatumFace = GetHorizontalDatumFace();
            if (_horizontalDatumFace == null) throw new Exception("该实体无法取点！");
        }

        /// <summary>
        /// 电极的Partname名称
        /// </summary>
        public string PartName { get { return _body.Name; } }

        /// <summary>
        /// 电极的模具名称
        /// </summary>
        public string MouldName
        {
            get
            {
                var partName = PartName;
                var strs=partName.Split('-').ToList();
                strs.Remove(strs.LastOrDefault());
                return string.Join("-", strs);
            }
        }

        /// <summary>
        /// 电极的大小
        /// </summary>
        public Position CuprumSize
        {
            get
            {
                var size = new Position();
                size.X = System.Math.Abs(_body.Box.MaxX - _body.Box.MinX);
                size.Y = System.Math.Abs(_body.Box.MaxY - _body.Box.MinY);
                size.Z = System.Math.Abs(_body.Box.MaxZ - _body.Box.MinZ);
                return size;
            }
        }

        /// <summary>
        /// 基准台底面到基准台顶面的高度
        /// </summary>
        public double BasestationH
        {
            get
            {
                if (_datumBottomFace != null && _horizontalDatumFace != null) 
                {
                    return Snap.Compute.Distance(_datumBottomFace, _horizontalDatumFace);
                }
                return 0;
            }
        }

        /// <summary>
        /// 获取取点信息
        /// </summary>
        public GetPointInfo GetPointInfo(List<PointData> points) 
        {
            var info = new GetPointInfo();
            info.basestationh = BasestationH;
            var cuprumSize=CuprumSize;
            info.sizex = cuprumSize.X;
            info.sizey = cuprumSize.Y;
            info.sizez = cuprumSize.Z;
            info.headh = System.Math.Abs(info.sizez - info.basestationh);
            info.partname = PartName;
            info.mouldname = MouldName;

            var trans = Snap.Geom.Transform.CreateTranslation();
            if (_horizontalDatumFace != null) 
            {
                var boxUV=_horizontalDatumFace.BoxUV;
                var midPoint = _horizontalDatumFace.Position((boxUV.MaxU + boxUV.MinU) / 2, (boxUV.MaxV + boxUV.MinV) / 2);
                trans = Snap.Geom.Transform.CreateTranslation(new Position() - midPoint);
            }

            //TODO 电极取点的象限角

            points.ForEach(u =>
            {
                var pointInfo = new CMMProgram.GetPointInfo.PointInfo();
                u.Position= u.Position.Copy(trans);
                pointInfo.pointname = u.PointName;
                pointInfo.arrow = u.Arrow;
                pointInfo.TIP = string.Format("A{0}B{0}", u.A, u.B);
                pointInfo.a = u.A;
                pointInfo.b = u.B;
                pointInfo.type = (int)u.PointType;
                pointInfo.x = u.Position.X;
                pointInfo.y = u.Position.Y;
                pointInfo.z = u.Position.Z;
                pointInfo.i = u.Vector.X;
                pointInfo.j = u.Vector.Y;
                pointInfo.k = u.Vector.Z;
                info.pointlist.Add(pointInfo);
            });

            return info;
        }

        /// <summary>
        /// 自动选点
        /// </summary>
        public List<PointData> AutoSelectPointOnFace(ProbeData data) 
        {
            var positions = new List<PointData>();

            var tempPositions = new List<PointData>();
            tempPositions = GetHorizontalDatumFacePositions(data);
            if (tempPositions.Count < 4) 
            {
                throw new Exception("基准面取点异常！");
            }
            //根据象限排序
            positions.AddRange(OrderPointDatas(tempPositions));

            tempPositions = GetVerticalDatumFacesPositions(data);
            if (tempPositions.Count < 8)
            {
                throw new Exception("侧面取点异常！");
            }
            positions.AddRange(tempPositions);


            tempPositions = GetElectrodeHeadFacePositions(data);
            positions.AddRange(tempPositions);
           
           
            
            return positions;
        }

        List<PointData> OrderPointDatas(List<PointData> tempPositions) 
        {
            var positions = new List<PointData>();
            var predicates = new List<Func<PointData, bool>>();
            predicates.Add(u => u.Position.X < 0 && u.Position.Y < 0);
            predicates.Add(u => u.Position.X > 0 && u.Position.Y < 0);
            predicates.Add(u => u.Position.X > 0 && u.Position.Y > 0);
            foreach (var i in predicates)
            {
                var thirdQuadrant = tempPositions.Where(i).ToList();
                positions.AddRange(thirdQuadrant);
                thirdQuadrant.ForEach(u =>
                {
                    tempPositions.Remove(u);
                });
            }
            positions.AddRange(tempPositions);
            return positions;
        }

        /// <summary>
        /// 获取基准底面
        /// </summary>
        Snap.NX.Face GetDatumBottomFace() 
        {
            var faces = new List<Snap.NX.Face>();
            foreach (var item in _faces)
            {
                if (item.NXOpenFace.SolidFaceType == NXOpen.Face.FaceType.Planar)
                {
                    var vector = item.GetFaceDirection();
                    if (SnapEx.Helper.Equals(-_orientation.AxisZ,vector))
                    {
                        faces.Add(item);
                    }
                }
            }
            return faces.OrderByDescending(u => Snap.Compute.Perimeter(u)).FirstOrDefault();
        }

        /// <summary>
        /// 获取水平基准面
        /// </summary>
        Snap.NX.Face GetHorizontalDatumFace() 
        {
            _datumBottomFace = GetDatumBottomFace();
            if (_datumBottomFace == null) return null;
            var faces = new List<Snap.NX.Face>();
            foreach (var item in _faces)
            {
                if (item.NXOpenFace.SolidFaceType == NXOpen.Face.FaceType.Planar)
                {
                    var vector = item.GetFaceDirection();
                    if (vector.Equals(_orientation.AxisZ))
                    {
                        faces.Add(item);
                    }
                }
            }
            return faces.OrderBy(u => Snap.Compute.Distance(_datumBottomFace,u)).FirstOrDefault();
        }

        List<PointData> GetVerticalDatumFacesPositions(ProbeData data)
        {
            var result = new List<PointData>();
            var faces=GetVerticalDatumFaces();
            foreach (var face in faces)
            {
                var positions = GetFacePoints(face,data);
                var body = data.Body;
                var faceDirection = face.GetFaceDirection();
                var faceOrientation = new Orientation(faceDirection);
                var faceMidPoint = face.Position((face.BoxUV.MaxU + face.BoxUV.MinU) / 2, (face.BoxUV.MaxV + face.BoxUV.MinV) / 2);

                var tempP = face.Box.MaxXYZ;
                var ps = positions.Where(u => System.Math.Abs(u.Z - face.Box.MaxZ) < data.VerticalValue).OrderBy(u => Snap.Position.Distance(tempP, u)).ToList();

                //对称点
                while (ps.Count > 0) 
                {
                    var item = ps.First();
                    var trans = Snap.Geom.Transform.CreateReflection(new Snap.Geom.Surface.Plane(faceMidPoint, faceOrientation.AxisY));
                    var symmetryPoint = item.Copy(trans);
                    if (!SnapEx.Helper.Equals(item, symmetryPoint, _tolerance) && positions.Where(u => SnapEx.Helper.Equals(u, symmetryPoint, _tolerance)).Count() > 0)
                    {
                        var p1 = IsIntervene(symmetryPoint, faceDirection, data);
                        var p2 = IsIntervene(item, faceDirection, data);
                        if (p1 != null && p2 != null)
                        {
                            p1.PointType = PointType.VerticalDatumFace;
                            p2.PointType = PointType.VerticalDatumFace;
                            if (SnapEx.Helper.Equals(faceDirection, -_orientation.AxisX))
                            {
                                var orderPoints = OrderPointDatas(new List<PointData> { p1, p2 });
                                result.Add(orderPoints.Last());
                                result.Add(orderPoints.First());
                            }
                            else 
                            {
                                result.AddRange(OrderPointDatas(new List<PointData> { p1, p2 }));
                            }
                            break;
                        }
                    }
                    ps.Remove(item);
                    positions.RemoveAll(u => SnapEx.Helper.Equals(u, symmetryPoint, _tolerance));
                }
            }

            return result;
        }

        /// <summary>
        /// 获取基准台所有的侧面
        /// </summary>
        List<Snap.NX.Face> GetVerticalDatumFaces() 
        {
            var faces = new List<Snap.NX.Face>();
            if (_horizontalDatumFace != null)
            {
                var tempFaces = _faces.Where(u => !u.NXOpenTag.Equals(_horizontalDatumFace.NXOpenTag)
                        && Equals(_horizontalDatumFace.Box.MinZ, u.Box.MaxZ )).ToList();
                faces.AddRange(tempFaces.Where(u=>SnapEx.Helper.Equals(u.GetFaceDirection(), -_orientation.AxisY)));
                faces.AddRange(tempFaces.Where(u=>SnapEx.Helper.Equals(u.GetFaceDirection(), _orientation.AxisX)));
                faces.AddRange(tempFaces.Where(u=>SnapEx.Helper.Equals(u.GetFaceDirection(), _orientation.AxisY)));
                faces.AddRange(tempFaces.Where(u=>SnapEx.Helper.Equals(u.GetFaceDirection(), -_orientation.AxisX)));
            }
            return faces;
        }

        List<PointData> GetElectrodeHeadFacePositions(ProbeData probeData)
        {
            var result = new List<PointData>();
            GetElectrodeHeadFace().ForEach(face => {
                var positions = GetFacePoints(face,probeData);
                positions = positions.Distinct().ToList();

                var vector = face.GetFaceDirection();

                var faceMidPoint = face.Position((face.BoxUV.MaxU + face.BoxUV.MinU) / 2, (face.BoxUV.MaxV + face.BoxUV.MinV) / 2);
                var ps = positions.OrderBy(u => Snap.Position.Distance(faceMidPoint, u)).ToList();
                var tempI = 10;
                for (var i = 0; i < tempI; i++)
                {
                    if (ps.Count > i)
                    {
                        var item = ps[i];
                        var p1 = IsIntervene(item, vector, probeData);
                        if (p1 != null)
                        {
                            p1.PointType = PointType.HeadFace;
                            result.Add(p1);
                            break;
                        }
                    }
                }
            });
            return result;
        }

        /// <summary>
        /// 电极头部面
        /// </summary>
        List<Snap.NX.Face> GetElectrodeHeadFace() 
        {
            var faces = new List<Snap.NX.Face>();
            foreach (var face in _faces) 
            {
                if (face.Box.MaxZ - _horizontalDatumFace.Box.MaxZ > _tolerance) //头部面
                {
                    faces.Add(face);
                    
                }
            }
            faces = faces.OrderByDescending(u => u.Box.MinZ).ToList();
            return faces;
        }


        bool Equals(double d1,double d2) 
        {
            var temp=System.Math.Abs(d1 - d2);
            return  temp< _tolerance;
        }

        public List<PointData> GetHorizontalDatumFacePositions(ProbeData probeData)
        {
            var result = new List<PointData>();
            var face = _horizontalDatumFace;
            var positions = GetFacePoints(face,probeData);
            positions=positions.Distinct().ToList();

            var vector = face.GetFaceDirection();

            //干涉检查
            var p1 = face.Position(face.BoxUV.MinU, face.BoxUV.MinV);
            var p2 = face.Position(face.BoxUV.MinU, face.BoxUV.MaxV);
            var p3 = face.Position(face.BoxUV.MaxU, face.BoxUV.MinV);
            var p4 = face.Position(face.BoxUV.MaxU, face.BoxUV.MaxV);

            var tempPs = new List<Snap.Position> { p1, p2, p3, p4 };
            for (int i = 0; i < tempPs.Count; i++)
            {
                var tempP = tempPs[i];
                var ps=positions.OrderBy(u => Snap.Position.Distance(tempP, u));
                foreach (var item in ps)
                {
                    var interveneP = IsIntervene(item, vector,probeData);
                    if (interveneP == null)
                    {
                        positions.Remove(item);
                    }
                    else
                    {
                        positions.Remove(item);
                        interveneP.PointType = PointType.HorizontalDatumFace;
                        result.Add(interveneP);
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 点是否在面上
        /// </summary>
        bool IsPointAtFace(Snap.NX.Face face, Snap.Position p) 
        {
            return Snap.Compute.Distance(p, face) < _tolerance;
        }

        public PointData IsIntervene(Snap.Position p,ProbeData data)
        {
            PointData result = null;

            PointType pointType=PointType.UNKOWN;
            Snap.NX.Face face=null;
            
            //基准顶面
            if (_horizontalDatumFace != null)
            {
                if (IsPointAtFace(_horizontalDatumFace,p)) 
                {
                    face=_horizontalDatumFace;
                    pointType = PointType.HorizontalDatumFace;
                }
            }

            if (pointType == PointType.UNKOWN)
            {
                foreach (var u in GetVerticalDatumFaces()) 
                {
                    if (IsPointAtFace(u, p))
                    {
                        face=u;
                        pointType = PointType.VerticalDatumFace;
                        break;
                    }
                }
            }

            if (pointType == PointType.UNKOWN) 
            {
                foreach (var u in GetElectrodeHeadFace())
                {
                    if (IsPointAtFace(u, p))
                    {
                        face=u;
                        pointType = PointType.HeadFace;
                        break;
                    }
                }
            }

            if (pointType != PointType.UNKOWN) 
            {
                result = IsIntervene(p, face.GetFaceDirection(), data);
                if (result != null) { result.PointType = pointType; }
            }

            
            return result;
        }

        /// <summary>
        /// 检测是否有干涉
        /// </summary>
        PointData IsIntervene(Snap.Position p,Snap.Vector vector,ProbeData probeData) 
        {
            PointData result = null;
            var probe = probeData.Body;
            var axisFace = probe.Faces.FirstOrDefault(u => u.Name == SnapEx.ConstString.CMM_INSPECTION_AXISPOINT);
            if (axisFace != null)
            {
                foreach (var ab in probeData.ABList) 
                {
                    var mark = Globals.SetUndoMark(Globals.MarkVisibility.Visible, "IsIntervene");
                    var tempAxisPoint = axisFace.Position((axisFace.BoxUV.MinU + axisFace.BoxUV.MaxU) / 2, (axisFace.BoxUV.MinV + axisFace.BoxUV.MaxV) / 2);
                    var origin = new Snap.Position((probe.Box.MinX + probe.Box.MaxX) / 2, (probe.Box.MinY + probe.Box.MaxY) / 2, probe.Box.MaxZ);
                    var axisPoint = origin;
                    if (SnapEx.Helper.Equals(tempAxisPoint, origin, _tolerance))
                    {
                        origin.Z = probe.Box.MinZ + probeData.SphereRadius;
                        axisPoint.Z = probe.Box.MaxZ;
                    }
                    else
                    {
                        axisPoint.Z = probe.Box.MinZ;
                        origin.Z = probe.Box.MaxZ - probeData.SphereRadius;
                    }
                    //A角
                    origin.Move(Snap.Geom.Transform.CreateRotation(axisPoint, _orientation.AxisX, ab.A));
                    probe.Move(Snap.Geom.Transform.CreateRotation(axisPoint, _orientation.AxisX, ab.A));
                    //B角
                    origin.Move(Snap.Geom.Transform.CreateRotation(axisPoint, _orientation.AxisZ, ab.B));
                    probe.Move(Snap.Geom.Transform.CreateRotation(axisPoint, _orientation.AxisZ, ab.B));

                    var tempP = p + (probeData.SphereRadius * vector);
                    probe.Move(Snap.Geom.Transform.CreateTranslation(tempP - origin));

                    var r = SnapEx.Create.SimpleInterference(probe, _body);
                    Globals.UndoToMark(mark, null);
                    if (r == NXOpen.GeometricAnalysis.SimpleInterference.Result.OnlyEdgesOrFacesInterfere)
                    {
                        result = new PointData() { Vector = vector, Position = p, A = ab.A, B = ab.B,Arrow=probeData.ProbeName};
                        break;
                    } 
                }
            }
            
           
           

            return result;
        }

        /// <summary>
        /// 是否小于探针半径
        /// </summary>
        static bool IsLessthanProbeR(List<Snap.NX.Curve> curves, Snap.Position p,ProbeData probe)
        {
            foreach (var item in curves)
            {
                var d = Compute.Distance(p, item);
                if (d <= probe.SphereRadius)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否是边上的点
        /// </summary>
       static bool IsPointOnEdge(Snap.NX.Face face,Snap.Position p) 
        {
            foreach (var item in face.EdgeCurves) 
            {
                var d = Compute.Distance(p, item);
                if (d < _tolerance) 
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 创建探针
        /// </summary>
        Snap.NX.Body CreateProbe(ProbeData data)
        {
            Snap.NX.CoordinateSystem wcs = Globals.WorkPart.NXOpenPart.WCS.CoordinateSystem;
            var vector = wcs.AxisZ;
            var position = new Snap.Position(0, 0, data.SphereRadius);
            //创建探球
            var body = Snap.Create.Sphere(position, data.SphereRadius * 2).Body;
            //创建测针
            var body1 = Snap.Create.Cylinder(position, position + (data.ArrowLength * vector), data.ArrowRadius * 2).Body;
            body1.IsHidden = true;
            position = position + (data.ArrowLength * vector);
            //创建加长杆
            var body2 = Snap.Create.Cylinder(position, position + (data.ExtensionBarLength * vector), data.ExtensionBarRadius * 2).Body;
            body2.IsHidden = true;
            //创建测头
            position = position + (data.ExtensionBarLength * vector);
            var body3 = Snap.Create.Cylinder(position, position + (data.HeadLength * vector), data.HeadRadius * 2).Body;
            body3.IsHidden = true;
            var r=Snap.Create.Unite(body, body1, body2, body3);
            r.Orphan();
           
            return body;
        }

        /// <summary>
        /// 创建箭头
        /// </summary>
        public static Snap.NX.Body CreateArrows(Snap.Position origin, Snap.Vector vector)
        {
            return Snap.Create.DatumAxis(origin, vector).Body;
            //var cylinder = Snap.Create.Cylinder(origin, vector, 40, 0.5);
            //origin = origin + (40 * vector);

            //var baseDiameter = 2.5;
            //var topDiameter = 0.0;
            //var cone = Snap.Create.Cone(origin, vector, new Number[] { baseDiameter, topDiameter }, 5);
            //var body = cone.Body;
            //var unite = Snap.Create.Unite(body, cylinder.Body);
            //unite.Orphan();
            //body.Color = System.Drawing.Color.Black;
            //return body;
        }


        static List<Snap.Position> GetFacePoints(Snap.NX.Face face, ProbeData data, double max_facet_size=1)
        {
            var mark = Snap.Globals.SetUndoMark(Globals.MarkVisibility.Visible, "GetFacePoints");
            var positions = new List<Snap.Position>();
            #region old code
            var parameters = new UFFacet.Parameters();

            var ufSession = NXOpen.UF.UFSession.GetUFSession();
            var facet = ufSession.Facet;
            facet.AskDefaultParameters(out parameters);
            parameters.max_facet_edges = 3;
            parameters.specify_max_facet_size = true;
            parameters.max_facet_size = 1;

            NXOpen.Tag facet_model = NXOpen.Tag.Null;
            facet.FacetSolid(face.NXOpenTag, ref parameters, out facet_model);

            if (facet_model == NXOpen.Tag.Null) return positions;
            NXOpen.Tag solid = NXOpen.Tag.Null;
            facet.AskSolidOfModel(facet_model, out solid);
            if (solid != face.NXOpenTag) return positions;

            int facet_id = NXOpen.UF.UFConstants.UF_FACET_NULL_FACET_ID;
            bool isWhile = true;
            while (isWhile)
            {
                facet.CycleFacets(facet_model, ref facet_id);
                if (facet_id != NXOpen.UF.UFConstants.UF_FACET_NULL_FACET_ID)
                {
                    int num_vertices = 0;
                    facet.AskNumVertsInFacet(facet_model, facet_id, out num_vertices);
                    if (num_vertices == 3)
                    {
                        var vertices = new double[num_vertices, 3];
                        facet.AskVerticesOfFacet(facet_model, facet_id, out num_vertices, vertices);
                        for (int i = 0; i < num_vertices; i++)
                        {
                            int pt_status = 0;
                            var position = new Snap.Position(vertices[i, 0], vertices[i, 1], vertices[i, 2]);
                            ufSession.Modl.AskPointContainment(position.Array, face.NXOpenTag, out pt_status);
                            if (0x1 == pt_status || 0x3 == pt_status)
                            {
                                positions.Add(position);
                            }
                        }
                    }
                }
                else
                {
                    isWhile = false;
                }
            }

            ufSession.Obj.DeleteObject(facet_model);
            positions=positions.Distinct().ToList();
            #endregion

            var edges = face.EdgeCurves.ToList();
            //过滤小于探球半径的点
            foreach (var item in positions.ToList())
            {
                if (IsLessthanProbeR(edges, item, data))
                {
                    positions.Remove(item);
                }
            }
            Snap.Globals.UndoToMark(mark, null);

            return positions;
        }
    }
}

