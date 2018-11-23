using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using f3;
using gs;

namespace orthogen
{
    public class SpatialDeviceScanAlignmentToolBuilder : IToolBuilder
    {
        public bool IsSupported(ToolTargetType type, List<SceneObject> targets)
        {
            return (type == ToolTargetType.SingleObject && targets[0].IsSurface);
        }

        public virtual ITool Build(FScene scene, List<SceneObject> targets)
        {
            SpatialDeviceScanAlignmentTool tool = new SpatialDeviceScanAlignmentTool(scene, targets[0]);
            return tool;
        }
    }


    public class SpatialDeviceScanAlignmentTool : ITool
    {
        static readonly public string Identifier = "spatial_scan_alignment";
        virtual public string Name { get { return "SpatialDeviceScanAlignmentTool"; } }
        virtual public string TypeIdentifier { get { return Identifier; } }

        protected FScene Scene;
        protected SceneObject Target;

        InputBehaviorSet behaviors;
        virtual public InputBehaviorSet InputBehaviors {
            get { return behaviors; }
            set { behaviors = value; }
        }

        ParameterSet parameters = new ParameterSet();
        public ParameterSet Parameters { get { return parameters; } }

        ToolIndicatorSet indicators;

        public SpatialDeviceScanAlignmentTool(FScene scene, SceneObject target)
        {
            this.Scene = scene;
            this.Target = target;
        }

        public virtual void Setup()
        {
            // do this here ??
            behaviors = new InputBehaviorSet();
            SpatialDeviceGrabBehavior behavior = new SpatialDeviceGrabBehavior(Scene.Context, (so) => { return so == this.Target; }) {
                Priority = 1,
                RotationSpeed = 0.25f,
                TranslationSpeed = 0.25f
            };
            behaviors.Add(behavior);


            indicators = new ToolIndicatorSet(this, Scene);

            float h = 300.0f;
            Frame3f f1 = Frame3f.Identity.Rotated(Quaternionf.AxisAngleD(Vector3f.AxisZ, 90.0f)).Translated(h * 0.5f * Vector3f.AxisY);
            var plane1 = new SectionPlaneIndicator() {
                Width = fDimension.Scene(h),   // in mm
                SceneFrameF = () => { return f1; }
            };
            indicators.AddIndicator(plane1);

            Frame3f f2 = Frame3f.Identity.Rotated(Quaternionf.AxisAngleD(Vector3f.AxisX, 90.0f)).Translated(h * 0.5f * Vector3f.AxisY);
            var plane2 = new SectionPlaneIndicator() {
                Width = fDimension.Scene(h),   // in mm
                SceneFrameF = () => { return f2; }
            };
            indicators.AddIndicator(plane2);

        }


        virtual public void Shutdown()
        {
            indicators.Disconnect(true);
        }


        virtual public void PreRender()
        {
            indicators.PreRender();
        }


        public virtual bool AllowSelectionChanges { get { return false; } }

        virtual public bool HasApply { get { return true; } }
        virtual public bool CanApply { get { return true; } }
        virtual public void Apply()
        {
        }





    }

}
