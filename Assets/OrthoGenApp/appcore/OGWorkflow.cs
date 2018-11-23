using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace orthogen
{
    /// <summary>
    /// State and Transition strings for Orthogen Workflows
    /// </summary>
    public static class OGWorkflow
    {
        public const string ScanState = "Scan";
        public const string RectifyState = "Rectification";
        public const string SocketState = "Socket";



        //
        // Scan cleanup states/transitions
        //
        public static string TrimScanState = "TrimScan";
        public static string TrimScanStartT = "Trim_Start";
        public static string TrimScanAcceptT = "Trim_Accept";
        public static string TrimScanCancelT = "Trim_Cancel";


        public static string AlignScanState = "AlignScan";
        public static string AlignScanStartT = "Align_Start";
        public static string AlignScanAcceptT = "Align_Accept";
        public static string AlignScanCancelT = "Align_Cancel";

        // 
        // Model tool states/transitions
        //


        public static string DrawAreaState = "DrawArea";
        public static string DrawAreaStartT = "DrawArea_Start";
        public static string DrawAreaExitT = "DrawArea_Exit";

        public static string AddDeformRingState = "DeformRing";
        public static string AddDeformRingStartT = "DeformRing_Start";
        public static string AddDeformRingAcceptT = "DeformRing_Accept";
        public static string AddDeformRingCancelT = "DeformRing_Cancel";

        public const string SculptAreaState = "SculptCurve";
        public const string SculptAreaStartT = "SculptCurve_Start_Model";
        public const string SculptAreaExitT = "SculptCurve_Exit_Model";




        //
        // Socket tool states/transitions
        //

        public static string DrawTrimlineState = "DrawTrimLine";
        public static string DrawTrimlineStartT = "DrawTrimLine_Start";
        public static string DrawTrimlineExitT = "DrawTrimLine_Exit";

        public static string PlaneTrimlineState = "PlaneTrimLine";
        public static string PlaneTrimlineStartT = "PlaneTrimLine_Start";
        public static string PlaneTrimlineAcceptT = "PlaneTrimLine_Accept";
        public static string PlaneTrimlineCancelT = "PlaneTrimLine_Cancel";

        public const string SculptTrimlineState = "SculptTrimLine";
        public const string SculptTrimlineStartT = "SculptTrimLine_Start";
        public const string SculptTrimlineExitT = "SculptTrimLine_End";


    }
}
