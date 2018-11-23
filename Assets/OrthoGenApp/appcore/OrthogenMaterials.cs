using System;
using System.Collections.Generic;
using System.Linq;
using g3;
using f3;

namespace orthogen
{
    /// <summary>
    /// Materials configuration for Orthogen
    /// </summary>
    public static class OrthogenMaterials
    {
        public static SOMaterial ScanMaterial;

        public static SOMaterial LegMaterial;
        public static SOMaterial RectifiedLegMaterial;

        public static SOMaterial TrimLoopMaterial;
        public static SOMaterial OffsetRegionMaterial;
        public static SOMaterial PlaneCurveMaterial;

        public static SOMaterial SocketMaterial;

        public static int CurvesLayer = FPlatform.WidgetOverlayLayer;


        public static void InitializeMaterials()
        {
            ScanMaterial = new SOMaterial() {
                Name = "ScanMaterial",
                Type = SOMaterial.MaterialType.FlatShadedPerVertexColor
            };

            LegMaterial = new SOMaterial() {
                Name = "LegDepthOnly",
                Type = SOMaterial.MaterialType.DepthWriteOnly
            };
            LegMaterial.RenderQueueShift = 50;


            RectifiedLegMaterial = new SOMaterial() {
                Name = "RectifiedLegMat",
                Type = SOMaterial.MaterialType.FlatShadedPerVertexColor
            };


            SocketMaterial = new SOMaterial() {
                Name = "SockeTMat",
                Type = SOMaterial.MaterialType.FlatShadedPerVertexColor,
                RGBColor = Colorf.White
            };


            TrimLoopMaterial = new SOMaterial() {
                Name = "TrimLoopMat",
                Type = SOMaterial.MaterialType.UnlitRGBColor, RGBColor = Colorf.VideoBlue
            };

            OffsetRegionMaterial = new SOMaterial() {
                Name = "OffsetRegionMat",
                Type = SOMaterial.MaterialType.UnlitRGBColor, RGBColor = Colorf.ForestGreen
            };

            PlaneCurveMaterial = new SOMaterial() {
                Name = "PlaneCurveMat",
                Type = SOMaterial.MaterialType.UnlitRGBColor, RGBColor = Colorf.DarkYellow
            };
            PlaneCurveMaterial.RenderQueueShift = 100;



        }

    }
}
