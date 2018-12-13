using CMMTool;
using NXOpen.UF;
using Snap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapEx;
using System.IO;

namespace CMM
{
    /// <summary>
    /// 自动取点业务
    /// </summary>
    public class CMMBusiness
    {
        static string _cmmFilePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"Temp\EACTCMMFILE");
        static EactConfig.ConfigData _EactConfigData = EactConfig.ConfigData.GetInstance();
        /// <summary>
        /// 循环变量值
        /// </summary>
        static int LoopVarValue = 30;

        /// <summary>
        /// 上传CMM文件
        /// </summary>
        public static void WriteCMMFileByPointData(ElecManage.Electrode elec, List<PointData> pointData,CMMConfig cmmconfig)
        {
            WriteCMMFile(elec, GetPointInfo.GetCMMPointInfo(pointData, elec, _EactConfigData),cmmconfig);
        }

        /// <summary>
        /// 上传CMM文件
        /// </summary>
        public static void WriteCMMFile(ElecManage.Electrode elec, object data,CMMConfig cmmConfig)
        {
            var info = elec.GetElectrodeInfo();
            var elecName = info.Elec_Name;
            Helper.ShowMsg(string.Format("{0}开始上传取点文件...", elecName));
            var MODEL_NUMBER = info.EACT_MODELNO;
            var fileName = string.Format("{0}{1}", elecName, ".txt");
            var result = Path.Combine(_cmmFilePath, fileName);
            if (Directory.Exists(_cmmFilePath))
            {
                Directory.Delete(_cmmFilePath, true);
            }
            Directory.CreateDirectory(_cmmFilePath);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            File.WriteAllText(result, json);
            if (cmmConfig.IsSelGetPointFilePath && !string.IsNullOrEmpty(cmmConfig.GetPointFilePath))
            {
                var path = Path.Combine(cmmConfig.GetPointFilePath, string.Format(@"{0}\{1}", info.EACT_MODELNO, elecName));
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                File.WriteAllText(Path.Combine(path, fileName), json);
            }
            FtpUpload("CMM", new ElecManage.MouldInfo { MODEL_NUMBER = MODEL_NUMBER }, result, elecName);
            Helper.ShowMsg(string.Format("{0}取点文件上传成功", elecName));
        }

        static void FtpUpload(string type, ElecManage.MouldInfo steelInfo, string fileName, string partName)
        {
            var ConfigData = _EactConfigData;
            CommonInterface.FtpHelper.FtpUpload(type, steelInfo, fileName, partName, ConfigData);
        }

        /// <summary>
        /// 自动取点
        /// </summary>
        public static List<PointData> AutoSelPoint(ElecManage.Electrode electrode, CMMConfig config, bool isUploadFile = true)
        {
            var positions = new List<PointData>();
            var tempPositions = new List<PointData>();
            var elecInfo = electrode.GetElectrodeInfo();
            config.ElecMaxZ = electrode.ElecBody.Box.MaxZ;
            var elecName = elecInfo.Elec_Name;

            Helper.ShowMsg(string.Format("{0}基准面取点", elecName));
            tempPositions = GetHorizontalDatumFacePositions(electrode, config);
            if (tempPositions.Count < 3)
            {
                throw new Exception("基准面取点异常！");
            }
            //根据象限排序
            positions.AddRange(OrderPointDatas(tempPositions));



            Helper.ShowMsg(string.Format("{0}侧面取点", elecName));
            tempPositions = GetVerticalDatumFacesPositions(electrode, config);
            if (tempPositions.Count < 8)
            {
                throw new Exception("侧面取点异常！");
            }
            positions.AddRange(tempPositions);



            Helper.ShowMsg(string.Format("{0}电极头部面取点", elecName));
            tempPositions = GetElectrodeHeadFacePositions(electrode, config);
            if (tempPositions.Count <= 0)
            {
                throw new Exception("电极头部面取点异常！");
            }
            //排序
            tempPositions = tempPositions.OrderBy(u => u.A).ThenBy(u => u.B).ThenByDescending(u => u.Position.Z).ToList();
            positions.AddRange(tempPositions);

            //名称
            foreach (var item in positions)
            {
                item.PointName = string.Format("P{0}", positions.IndexOf(item) + 1);
            }

            if (isUploadFile)
            {
                WriteCMMFile(electrode, GetPointInfo.GetCMMPointInfo(positions, electrode, _EactConfigData), config);
            }
            return positions;
        }

        /// <summary>
        /// 自动取点
        /// </summary>
        public static List<PointData> AutoSelPoint(Snap.NX.Body body, CMMConfig config,bool isUploadFile=true)
        {
            var result = new List<PointData>();
            var electrode = ElecManage.Electrode.GetElectrode(body);
            if (electrode == null)
            {
                throw new Exception("无法识别该电极！");
            }
            electrode.InitAllFace();
            bool CMMResult = true;
            string CMMInfo = string.Empty;
            try
            {
                result = AutoSelPoint(electrode, config, isUploadFile);
            }
            catch (Exception ex)
            {
                CMMResult = false;
                CMMInfo = ex.Message;
                if (!config.IsUploadDataBase)
                {
                    throw ex;
                }
            }

            var info = electrode.GetElectrodeInfo();
            if (!CMMResult)
            {
                Helper.ShowMsg(string.Format("{0}【{1}】", info.Elec_Name, CMMInfo), 1);
            }
          
            if (config.IsUploadDataBase)
            {
                DataAccess.Model.EACT_AUTOCMM_RECORD record= new DataAccess.Model.EACT_AUTOCMM_RECORD();
                record.CMMDATE = DateTime.Now;
                record.CMMINFO = CMMInfo;
                record.CMMRESULT = CMMResult ? 1 : 2;
                record.MODELNO = info.EACT_MODELNO;
                record.PARTNO = info.EACT_PARTNO;
                record.PARTNAME = info.Elec_Name;
                Helper.ShowMsg(string.Format("{0}上传取点记录", info.Elec_Name));
                InitDatabase();
                DataAccess.BOM.UploadAutoCMMRecord(record);
                Helper.ShowMsg(string.Format("{0}取点记录上传完成", info.Elec_Name));
            }

            return result;
        }

        public static void InitDatabase()
        {
            var data = _EactConfigData;
            var connStr = CommonInterface.DatabaseHelper.GetConnStr(data);
            DataAccess.Entry.Instance.Init(connStr);
        }

        /// <summary>
        /// 获取头部面的检测点
        /// </summary>
        static List<PointData> GetElectrodeHeadFacePositions(ElecManage.Electrode elec, CMMConfig config)
        {
            var result = new List<PointData>();
            var faces = elec.ElecHeadFaces;
            foreach (var face in faces)
            {
                if (config.IsEDMFaceGetPoint)
                {
                    LoopVarValue = 100;
                    if (Snap.Color.ColorIndex(face.Color) != _EactConfigData.EDMColor)
                    {
                        continue;
                    }
                }

                var edges = face.EdgeCurves.ToList();
                var positions = Helper.GetFacePoints(face, config, edges);
                var faceMidPoint = face.GetCenterPointEx();
                var faceArea = 0.0;

                if (config.IsMinGetPointArea || config.IsGetTowPointArea)
                {
                    faceArea = face.Area;
                }

                //面积小于某值的不取点（可配置）
                if (config.IsMinGetPointArea)
                {
                    if (faceArea < config.MinGetPointArea)
                    {
                        continue;
                    }
                }

                Action<List<Position>> action = (p) =>
                {
                    for (var i = 0; i < p.Count; i++)
                    {
                        if (i < LoopVarValue)
                        {
                            var item = p[i];
                            var pointVector = face.GetFaceDirectionByPoint(item); 
                            if (double.IsNaN(pointVector.X) || double.IsNaN(pointVector.Y) || double.IsNaN(pointVector.Z))
                            {
                                continue;
                            }
                            var p1 = IsIntervene(elec, item, pointVector, edges, config, PointType.HeadFace);
                            if (p1 != null)
                            {
                                result.Add(p1);
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                };

                //面积超过以上配置的某个值的，取靠近面的UV百分比为25 % 及75 % 的两个点（一个面最多两个点）
                if (config.IsGetTowPointArea && faceArea > config.GetTowPointArea)
                {
                    var faceBoxUV = face.BoxUV;
                    var faceU = faceBoxUV.MaxU - faceBoxUV.MinU;
                    var faceV = faceBoxUV.MaxV - faceBoxUV.MinV;
                    var firstUV = face.Position(faceBoxUV.MinU + faceU / 4, (faceBoxUV.MaxV + faceBoxUV.MinV) / 2);
                    var twoUV = face.Position(faceBoxUV.MinU + faceU * 3 / 4, (faceBoxUV.MaxV + faceBoxUV.MinV) / 2);
                    var centerUV = face.Position((faceBoxUV.MaxU + faceBoxUV.MinU) / 2, (faceBoxUV.MaxV + faceBoxUV.MinV) / 2);
                    var fDistance = Snap.Position.Distance(firstUV, centerUV);
                    var tDistance = Snap.Position.Distance(twoUV, centerUV);
                    var ps1 = positions.Where(u => Snap.Position.Distance(firstUV, u) < fDistance).OrderBy(u => Snap.Position.Distance(firstUV, u)).ToList();
                    var ps2 = positions.Where(u => Snap.Position.Distance(twoUV, u) < tDistance && !ps1.Contains(u)).OrderBy(u => Snap.Position.Distance(twoUV, u)).ToList();
                    action(ps1);
                    action(ps2);
                }
                else
                {
                    var ps = positions.OrderBy(u => Snap.Position.Distance(faceMidPoint, u)).ToList();
                    action(ps);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取基准面的检测点
        /// </summary>
        static List<PointData> GetHorizontalDatumFacePositions(ElecManage.Electrode elec, CMMConfig config)
        {
            var result = new List<PointData>();
            var face = elec.BaseFace;
            var edges = face.EdgeCurves.ToList();
            var positions = Helper.GetFacePoints(face, config, edges, config.IsBaseRoundInt);
            var vector = face.GetFaceDirection();
            //边界点
            var p1 = face.Position(face.BoxUV.MinU, face.BoxUV.MinV);
            var p2 = face.Position(face.BoxUV.MinU, face.BoxUV.MaxV);
            var p3 = face.Position(face.BoxUV.MaxU, face.BoxUV.MaxV);
            var p4 = face.Position(face.BoxUV.MaxU, face.BoxUV.MinV);
            var center = face.GetCenterPoint();

            var tempPs = new List<Snap.Position> { p1, p2, p3, p4 };
            foreach (var tempP in tempPs)
            {
                var ps = positions.Where(u => Helper.IsSameQuadrant(tempP, u, center)).OrderBy(u => Snap.Position.Distance(tempP, u));
                foreach (var item in ps)
                {
                    var interveneP = IsIntervene(elec, item, vector, edges, config, PointType.HorizontalDatumFace, true);
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
        /// 获取侧面的检测点
        /// </summary>
        static List<PointData> GetVerticalDatumFacesPositions(ElecManage.Electrode elec, CMMConfig config)
        {
            var result = new List<PointData>();
            var ufSession = NXOpen.UF.UFSession.GetUFSession();
            //获取所有的侧面
            var faces = elec.BaseSideFaces;
            //获取Z值
            var minZ = elec.BaseFace.Box.MinZ;
            var baseFaceMidPoint = elec.BaseFace.GetCenterPoint();
            var VecZ = minZ - config.VerticalValue;
            var dicFace = new Dictionary<Snap.NX.Face, CMMFaceInfo>();
            var listZ = new List<double>();
            var firstFaces = faces.Where(u =>
            SnapEx.Helper.Equals(u.GetFaceDirection(), new Snap.Vector(0, 1, 0))
             || SnapEx.Helper.Equals(u.GetFaceDirection(), new Snap.Vector(0, -1, 0))
            ).ToList();
            var twoFaces = faces.Where(u =>
            SnapEx.Helper.Equals(u.GetFaceDirection(), new Snap.Vector(1, 0, 0))
            || SnapEx.Helper.Equals(u.GetFaceDirection(), new Snap.Vector(-1, 0, 0))
            ).ToList();

            if (!(firstFaces.Count() >= 2 && twoFaces.Count >= 2))
            {
                throw new Exception("获取侧面异常");
            }
            foreach (var face in faces)
            {
                var edges = face.EdgeCurves.ToList();
                var positions = Helper.GetFacePoints(face, config, edges,config.IsBaseRoundInt);
                var faceDirection = face.GetFaceDirection();
                var faceOrientation = new Orientation(faceDirection);
                var tempP = face.Box.MaxXYZ;
                //侧面变量Z
                if (positions.Count > 0)
                {
                    var sideZP = positions.First();
                    var tempPs = positions
                       .Where(u => (System.Math.Abs(u.Z - sideZP.Z) < SnapEx.Helper.Tolerance)).ToList();
                    tempPs.ForEach(u => {
                        u.Z = VecZ;
                        positions.Add(u);
                    }); 
                }
                var ps = positions.Where(u => System.Math.Abs(u.Z - minZ) <= config.VerticalValue).OrderByDescending(u => Snap.Position.Distance(tempP, u)).ToList();
                dicFace.Add(face, new CMMFaceInfo { Positions = ps, Edges = edges, FaceDirection = faceDirection, FaceOrientation = faceOrientation });
                if (faces.IndexOf(face) == 0)
                {
                    listZ.Add(VecZ);
                    //其他测量Z值不取 update ph
                    //ps.ForEach(u => {
                    //    listZ.Add(u.Z);
                    //}); 
                    listZ = listZ.Distinct().ToList();
                }
            }

            var tempFaces = new Dictionary<Snap.NX.Face, List<Snap.NX.Face>>();
            tempFaces.Add(firstFaces.First(), firstFaces);
            tempFaces.Add(twoFaces.First(), twoFaces);
            foreach (var z in listZ)
            {
                var cmmPointData = new List<PointData>();
                foreach (var tf in tempFaces)
                {
                    var firstFace = tf.Key;
                    var firstCmmFaceInfo = dicFace[firstFace];
                    var twoFace = tf.Value.Last();
                    var twoCMMFaceInfo = dicFace[twoFace];
                    var ftDistance = Snap.Compute.Distance(firstFace, twoFace);
                    var ps = firstCmmFaceInfo.Positions
                            .Where(u => (System.Math.Abs(u.Z - z) < SnapEx.Helper.Tolerance))
                            .OrderByDescending(u => Snap.Position.Distance(u, baseFaceMidPoint))
                            .ToList();
                    //对称点
                    while (ps.Count > 0)
                    {
                        //获取4个对称点
                        var item = ps.First();
                        var trans = Snap.Geom.Transform.CreateReflection(new Snap.Geom.Surface.Plane(baseFaceMidPoint, firstCmmFaceInfo.FaceOrientation.AxisY));
                        var symmetryPoint = item.Copy(trans);
                        var trans1 = Snap.Geom.Transform.CreateTranslation(ftDistance * (-firstCmmFaceInfo.FaceDirection));
                        var tItem = item.Copy(trans1);
                        var tSymmetryPoint = symmetryPoint.Copy(trans1);
                        if (
                            !SnapEx.Helper.Equals(item, symmetryPoint, SnapEx.Helper.Tolerance)
                            && Helper.AskPointContainment(symmetryPoint, firstFace)
                            && Helper.AskPointContainment(tItem, twoFace)
                            && Helper.AskPointContainment(tSymmetryPoint, twoFace))
                        {
                            var p1 = IsIntervene(elec, symmetryPoint, firstCmmFaceInfo.FaceDirection, firstCmmFaceInfo.Edges, config, PointType.VerticalDatumFace,true);
                            var p2 = IsIntervene(elec, item, firstCmmFaceInfo.FaceDirection, firstCmmFaceInfo.Edges, config, PointType.VerticalDatumFace,true);
                            var p3 = IsIntervene(elec, tItem, twoCMMFaceInfo.FaceDirection, twoCMMFaceInfo.Edges, config, PointType.VerticalDatumFace,true);
                            var p4 = IsIntervene(elec, tSymmetryPoint, twoCMMFaceInfo.FaceDirection, twoCMMFaceInfo.Edges, config, PointType.VerticalDatumFace,true);

                            if (p1 != null && p2 != null && p3 != null && p4 != null)
                            {
                                cmmPointData.Add(p1);
                                cmmPointData.Add(p2);
                                cmmPointData.Add(p3);
                                cmmPointData.Add(p4);
                                break;
                            }
                        }
                        ps.Remove(item);
                        ps.RemoveAll(u => SnapEx.Helper.Equals(u, symmetryPoint, SnapEx.Helper.Tolerance));
                    }

                }

                if (cmmPointData.Count >= 8)
                {
                    result = OrderPointDatas(cmmPointData);
                    break;
                }
            }

            return result;
        }

        static List<PointData> OrderPointDatas(List<PointData> tempPositions)
        {
            var positions = new List<PointData>();
            var predicates = new List<Func<PointData, bool>>();
            //p1
            predicates.Add(u => u.Position.X < 0 && u.Position.Y < 0 && SnapEx.Helper.Equals(u.Vector, new Snap.Vector(0, -1, 0)));
            //p2
            predicates.Add(u => u.Position.X > 0 && u.Position.Y < 0 && SnapEx.Helper.Equals(u.Vector, new Snap.Vector(0, -1, 0)));
            //p3
            predicates.Add(u => u.Position.X > 0 && u.Position.Y < 0 && SnapEx.Helper.Equals(u.Vector, new Snap.Vector(1, 0, 0)));
            //p4
            predicates.Add(u => u.Position.X > 0 && u.Position.Y > 0 && SnapEx.Helper.Equals(u.Vector, new Snap.Vector(1, 0, 0)));
            //p5
            predicates.Add(u => u.Position.X > 0 && u.Position.Y > 0 && SnapEx.Helper.Equals(u.Vector, new Snap.Vector(0, 1, 0)));
            //p6
            predicates.Add(u => u.Position.X < 0 && u.Position.Y > 0 && SnapEx.Helper.Equals(u.Vector, new Snap.Vector(0, 1, 0)));
            //p7
            predicates.Add(u => u.Position.X < 0 && u.Position.Y > 0 && SnapEx.Helper.Equals(u.Vector, new Snap.Vector(-1, 0, 0)));
            //p8
            predicates.Add(u => u.Position.X < 0 && u.Position.Y < 0 && SnapEx.Helper.Equals(u.Vector, new Snap.Vector(-1, 0, 0)));

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

        public static PointData IsInterveneBySelPoint(ElecManage.Electrode elec, Snap.Position p, CMMConfig config)
        {
            try
            {
                var electrode = elec;
                if (electrode == null)
                {
                    return null;
                }

                var faces = electrode.ElecHeadFaces;
                var face = faces.FirstOrDefault(u => Helper.AskPointContainment(p, u));
                var pointType = PointType.HeadFace;
                if (face == null&& Helper.AskPointContainment(p, electrode.BaseFace))
                {
                    face = electrode.BaseFace;
                    pointType = PointType.HorizontalDatumFace;
                }

                if (face == null)
                {
                    faces = electrode.BaseSideFaces;
                    face = faces.FirstOrDefault(u => Helper.AskPointContainment(p, u));
                    pointType = PointType.VerticalDatumFace;
                }

                var vector = face.GetFaceDirection();
                var edges = face.EdgeCurves.ToList();
                var pointVector = vector;
                if (double.IsNaN(pointVector.X) || double.IsNaN(pointVector.Y) || double.IsNaN(pointVector.Z))
                {
                    pointVector = face.GetFaceDirectionByPoint(p);
                }
                if (double.IsNaN(pointVector.X) || double.IsNaN(pointVector.Y) || double.IsNaN(pointVector.Z))
                {
                    return null;
                }

                return IsIntervene(electrode, p, pointVector, edges, config, pointType);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        static PointData IsIntervene(ElecManage.Electrode elec, Snap.Position p, Snap.Vector pV, List<Snap.NX.Curve> curves, CMMConfig config, PointType pointType = PointType.UNKOWN, bool isBaseFace = false)
        {
            PointData result = null;
            var targetBody = elec.ElecBody;
            var maxZ = config.ElecMaxZ + config.SafeDistance;
            var minDistance = Helper.GetPointToEdgeMinDistance(p, curves);
            var ProbeDatas = config.ProbeDatas ?? new List<ProbeData>();
            if (isBaseFace)
            {
                var probeData = ProbeDatas.FirstOrDefault(u => u.IsBaseFaceProbe);
                if (probeData != null)
                {
                    ProbeDatas = new List<ProbeData> { probeData };
                }
            }

            foreach (var data in ProbeDatas)
            {
                if (result != null)
                {
                    break;
                }
                //过滤探球半径的点
                if (minDistance < config.MinEdgeDistance)
                {
                    continue;
                }
                var abs = data.GetABList(p, pV);
                foreach (var ab in abs)
                {
                    var toolBody = data.GetBody(ab);
                    var lstTrans = new List<Snap.Geom.Transform>();
                    var centreOfSphere = data.GetCentreOfSphere(ab);
                    var inspectionPoints = new List<Position>();

                    //退点
                    var sRetreatPosition = p.Copy(Snap.Geom.Transform.CreateTranslation((data.D / 2) * pV));
                    lstTrans.Add(Snap.Geom.Transform.CreateTranslation(sRetreatPosition - centreOfSphere));
                    var mRetreatPosition = sRetreatPosition.Copy(Snap.Geom.Transform.CreateTranslation(config.RetreatPoint * pV));
                    lstTrans.Add(Snap.Geom.Transform.CreateTranslation(mRetreatPosition - sRetreatPosition));
                    var fRetreatPosition = new Snap.Position(mRetreatPosition.X, mRetreatPosition.Y, maxZ);
                    //lstTrans.Add(Snap.Geom.Transform.CreateTranslation(fRetreatPosition - mRetreatPosition));
                    if (config.RetreatPoint != config.EntryPoint)
                    {
                        //逼进拐点
                        var sEntryPosition = sRetreatPosition.Copy(Snap.Geom.Transform.CreateTranslation(config.EntryPoint * pV));
                        if (config.EntryPoint > 0)
                        {
                            lstTrans.Add(Snap.Geom.Transform.CreateTranslation(sEntryPosition - mRetreatPosition));
                        }
                        var fEntryPosition = new Snap.Position(sEntryPosition.X, sEntryPosition.Y, maxZ);
                        inspectionPoints.Add(fEntryPosition);
                        inspectionPoints.Add(sEntryPosition);
                    }
                    inspectionPoints.Add(sRetreatPosition);
                    inspectionPoints.Add(mRetreatPosition);
                    inspectionPoints.Add(fRetreatPosition);
                    if (config.IsInspectionPath)
                    {
                        if (data.CheckInspectionPath(ab, inspectionPoints, targetBody))
                        {
                            continue;
                        }
                    }
                    bool isHasInterference = false;
                    foreach (var trans in lstTrans)
                    {
                        toolBody.Move(trans);
                        if (Helper.CheckInterference(targetBody.NXOpenTag, toolBody.NXOpenTag))
                        {
                            isHasInterference = true;
                            break;
                        }
                    }

                    if (!isHasInterference)
                    {
                        result = new PointData() { Vector = pV, Position = p, A = ab.A, B = ab.B, Arrow = data.ProbeName };
                        result.PointType = pointType;
                        break;
                    }
                }

            }

            return result;
        }


        /// <summary>
        /// 是否小于探针半径
        /// </summary>
        static bool IsLessthanProbeR(List<Snap.NX.Curve> curves, Snap.Position p, ProbeData probe)
        {
            foreach (var item in curves)
            {
                var d = Compute.Distance(p, item);
                if (d <= probe.D / 2)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
