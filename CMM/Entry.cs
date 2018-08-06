using ElecManage;
using NXOpen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CMM
{
    public class Entry
    {
        public static void Test()
        {
            PartLoadStatus partLoadStatus1;
            Session theSession = Session.GetSession();
            var fileName = Path.Combine(@"C:\Users\PENGHUI\Desktop", "18160-201.prt") ;
            var basePart = theSession.Parts.OpenBase(fileName, out partLoadStatus1) as Part;
            Snap.Globals.WorkPart = basePart;
            var workPart = Snap.Globals.WorkPart;
            //去参数
            try
            {
                SnapEx.Create.RemoveParameters(Enumerable.Select(workPart.Bodies.Where(u => u.ObjectSubType == Snap.NX.ObjectTypes.SubType.BodySolid), u => u.NXOpenBody).ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
