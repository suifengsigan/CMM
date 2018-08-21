using CMMTool;
using NXOpen.UF;
using Snap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapEx;

namespace CMM
{
    /// <summary>
    /// 自动取点业务
    /// </summary>
    public class CMMBusiness
    {
        /// <summary>
        /// 循环变量值
        /// </summary>
        static int LoopVarValue = 10;

        /// <summary>
        /// 自动取点
        /// </summary>
        public static List<PointData> AutoSelPoint(Snap.NX.Body body,CMMConfig config)
        {
            var electrode = ElecManage.Electrode.GetElectrode(body);
            if (electrode == null)
            {
                throw new Exception("无法识别该电极！");
            }
            electrode.InitAllFace();
            var positions = new List<PointData>();
            var tempPositions = new List<PointData>();
            tempPositions = GetHorizontalDatumFacePositions(electrode, config);
            if (tempPositions.Count < 4)
            {
                throw new Exception("基准面取点异常！");
            }

            //根据象限排序
            positions.AddRange(OrderPointDatas(tempPositions));

            tempPositions = GetVerticalDatumFacesPositions(electrode, config);
            if (tempPositions.Count < 8)
            {
                throw new Exception("侧面取点异常！");
            }
            positions.AddRange(tempPositions);


            tempPositions = GetElectrodeHeadFacePositions(electrode, config);
            positions.AddRange(tempPositions);

            return positions;
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
                var vector = face.GetFaceDirection();
                var positions = SnapHelper.GetFacePoints(face);
                var faceMidPoint = face.Position((face.BoxUV.MaxU + face.BoxUV.MinU) / 2, (face.BoxUV.MaxV + face.BoxUV.MinV) / 2);
                var ps = positions.OrderBy(u => Snap.Position.Distance(faceMidPoint, u)).ToList();
                for (var i = 0; i < LoopVarValue; i++)
                {
                    if (ps.Count > i)
                    {
                        var item = ps[i];
                        var p1 = IsIntervene(elec, item, vector, config, PointType.HeadFace);
                        if (p1 != null)
                        {
                            result.Add(p1);
                            break;
                        }
                    }
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
            var positions = SnapHelper.GetFacePoints(face);
            var vector = face.GetFaceDirection();
            //边界点
            var p1 = face.Position(face.BoxUV.MinU, face.BoxUV.MinV);
            var p2 = face.Position(face.BoxUV.MinU, face.BoxUV.MaxV);
            var p3 = face.Position(face.BoxUV.MaxU, face.BoxUV.MinV);
            var p4 = face.Position(face.BoxUV.MaxU, face.BoxUV.MaxV);

            var tempPs = new List<Snap.Position> { p1, p2, p3, p4 };
            foreach (var tempP in tempPs)
            {
                var ps = positions.OrderBy(u => Snap.Position.Distance(tempP, u));
                foreach (var item in ps)
                {
                    var interveneP = IsIntervene(elec, item, vector, config,PointType.HorizontalDatumFace);
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
            //获取所有的侧面
            var faces = elec.BaseSideFaces;
            //获取Z值
            var minZ = elec.BaseFace.Box.MinZ;
            minZ -= config.SideFaceGetPointValue;
            foreach (var face in faces)
            {
                var positions = SnapHelper.GetFacePoints(face);
                var faceDirection = face.GetFaceDirection();
                var faceOrientation = new Orientation(faceDirection);
                var faceMidPoint = face.Position((face.BoxUV.MaxU + face.BoxUV.MinU) / 2, (face.BoxUV.MaxV + face.BoxUV.MinV) / 2);

                var tempP = face.Box.MaxXYZ;
                var ps = positions.Where(u => System.Math.Abs(u.Z - face.Box.MaxZ) < config.VerticalValue).OrderBy(u => Snap.Position.Distance(tempP, u)).ToList();

                //对称点
                while (ps.Count > 0)
                {
                    var item = ps.First();
                    var trans = Snap.Geom.Transform.CreateReflection(new Snap.Geom.Surface.Plane(faceMidPoint, faceOrientation.AxisY));
                    var symmetryPoint = item.Copy(trans);
                    if (!SnapEx.Helper.Equals(item, symmetryPoint, SnapEx.Helper.Tolerance) && positions.Where(u => SnapEx.Helper.Equals(u, symmetryPoint, SnapEx.Helper.Tolerance)).Count() > 0)
                    {
                        var p1 = IsIntervene(elec,symmetryPoint, faceDirection, config,PointType.VerticalDatumFace);
                        var p2 = IsIntervene(elec,item, faceDirection, config, PointType.VerticalDatumFace);
                        if (p1 != null && p2 != null)
                        {
                            p1.PointType = PointType.VerticalDatumFace;
                            p2.PointType = PointType.VerticalDatumFace;
                            if (SnapEx.Helper.Equals(faceDirection, -Snap.Orientation.Identity.AxisX))
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
                    positions.RemoveAll(u => SnapEx.Helper.Equals(u, symmetryPoint, SnapEx.Helper.Tolerance));
                }
            }
            return result;
        }

        static List<PointData> OrderPointDatas(List<PointData> tempPositions)
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
        
        static PointData IsIntervene(ElecManage.Electrode elec, Snap.Position p,Snap.Vector pV, CMMConfig config, PointType pointType = PointType.UNKOWN)
        {
            PointData result = null;
            var targetBody = elec.ElecBody;
            var box = targetBody.Box;
            var maxZ = box.MaxZ + config.SafeDistance;
         

            foreach (var data in config.ProbeDatas.ToList())
            {
                foreach (var ab in data.GetABList())
                {
                    var toolBody = data.GetBody(ab);
                    var lstTrans = new List<Snap.Geom.Transform>();
                    var centreOfSphere = data.GetCentreOfSphere(ab);
                    //退点
                    var sRetreatPosition = p.Copy(Snap.Geom.Transform.CreateTranslation((data.D / 2) * pV));
                    lstTrans.Add(Snap.Geom.Transform.CreateTranslation(sRetreatPosition - centreOfSphere));
                    var mRetreatPosition = sRetreatPosition.Copy(Snap.Geom.Transform.CreateTranslation(config.RetreatPoint*pV));
                    lstTrans.Add(Snap.Geom.Transform.CreateTranslation(mRetreatPosition-sRetreatPosition));
                    var fRetreatPosition = new Snap.Position(mRetreatPosition.X, mRetreatPosition.Y, maxZ);
                    lstTrans.Add(Snap.Geom.Transform.CreateTranslation(fRetreatPosition - mRetreatPosition));
                    if (config.RetreatPoint != config.EntryPoint)
                    {
                        //逼进拐点
                        var sEntryPosition = sRetreatPosition.Copy(Snap.Geom.Transform.CreateTranslation(config.EntryPoint * pV));
                        lstTrans.Add(Snap.Geom.Transform.CreateTranslation(sEntryPosition - fRetreatPosition));
                    }
                    bool isHasInterference = false;
                    foreach (var trans in lstTrans)
                    {
                        toolBody.Move(trans);
                        if (SnapHelper.CheckInterference(targetBody.NXOpenTag, toolBody.NXOpenTag))
                        {
                            isHasInterference = true;
                            break;
                        }
                    }

                    if (!isHasInterference)
                    {
                        result= new PointData() { Vector = pV, Position = p, A = ab.A, B = ab.B, Arrow = data.ProbeName };
                        result.PointType = pointType;
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
                if (d <= probe.D/2)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
