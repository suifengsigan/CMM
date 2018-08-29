using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMM
{
    public class CMMFaceInfo
    {
        public List<Snap.Position> Positions = new List<Snap.Position>();
        public List<Snap.NX.Curve> Edges = new List<Snap.NX.Curve>();
        public Snap.Vector FaceDirection = new Snap.Vector(0, 0, 1);
        public Snap.Orientation FaceOrientation = Snap.Orientation.Identity;
        public Snap.Position FaceMidPoint = Snap.Position.Origin;
    }
}
