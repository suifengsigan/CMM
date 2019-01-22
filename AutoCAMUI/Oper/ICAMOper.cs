using NXOpen;

namespace AutoCAMUI
{
    public interface ICAMOper
    {
        string AUTOCAM_SUBTYPE { get; }
        string AUTOCAM_TYPE { get; }
        CAMCutter CAMCutter { get; set; }
        /// <summary>
        /// 火花位
        /// </summary>
        double FRIENUM { get; set; }
        Tag MethodGroupRoot { get; set; }
        bool OperIsValid { get; }
        Tag OperTag { get; }
        Tag ProgramGroup { get; set; }
        E_TmplateOper TmplateOper { get; }
        Tag WorkGeometryGroup { get; set; }

        void AutoAnalysis(CAMElectrode ele, Tag WorkGeometryGroup, Tag ProgramGroup, Tag MethodGroupRoot, CAMCutter CAMCutter, CAMCutter refCAMCutter);
        void CreateOper(Tag WorkGeometryGroup, Tag ProgramGroup, Tag MethodGroupRoot, CAMCutter CAMCutter, bool operIsValid = true);
        void GenerateProgram(string postName, string path, string extension);
        bool IsPathGouged();
        string PathGenerate();
        void SetCutDepth(double depth);
        void SetFeedRate(double feedRate);
        void SetReferenceCutter(CAMCutter cutter);
    }
}