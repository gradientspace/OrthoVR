using System;
using System.Collections.Generic;
using UnityEngine;
using FBCapture;

namespace f3
{
    public class FBEncoderCaptureBehavior : StandardInputBehavior
    {
        CaptureOption CaptureControl;


        public string ScreenshotPath = "C:\\";
        public string ScreenshotPrefix = "Screenshot_";

        public FBEncoderCaptureBehavior()
        {
            CaptureControl = GameObject.Find("FBCaptureEncoder").GetComponent<CaptureOption>();
        }

        public override InputDevice SupportedDevices
        {
            get { return InputDevice.AnySpatialDevice; }
        }

        public override CaptureRequest WantsCapture(InputState input)
        {
            throw new NotImplementedException("FBEncoderCaptureBehavior.WantsCapture: this is an override behavior and does not capture!!");
        }


        public override Capture BeginCapture(InputState input, CaptureSide eSide)
        {
            throw new NotImplementedException("FBEncoderCaptureBehavior.BeginCapture: this is an override behavior and does not capture!!");
        }

        double press_time = 0;
        bool in_video_capture = false;

        public override Capture UpdateCapture(InputState input, CaptureData data)
        {
            //CaptureControl.doSurroundCapture = false;
#if false
            if (input.bLeftMenuButtonPressed) {
                press_time = FPlatform.RealTime();
                if (input.bLeftShoulderDown)
                    CaptureControl.EnableSurroundCapture = true;
                else
                    CaptureControl.EnableSurroundCapture = false;

            } else if (input.bLeftMenuButtonReleased) {
                if (in_video_capture) {
                    CaptureControl.EndVideoCapture();
                    string s = string.Format("Captured {0} video!", CaptureControl.EnableSurroundCapture ? "360" : "WideAngle");
                    HUDUtil.ShowToastPopupMessage(s, FContext.ActiveContext_HACK.ActiveCockpit);
                    in_video_capture = false;
                    press_time = 0;
                } else if (FPlatform.RealTime() - press_time > 1.0f) {
                    CaptureControl.BeginVideoCapture();
                    in_video_capture = true;
                } else {
                    CaptureControl.CaptureScreen();
                    string s = string.Format("Captured {0} screenshot!", CaptureControl.EnableSurroundCapture ? "360" : "WideAngle");
                    HUDUtil.ShowToastPopupMessage(s, FContext.ActiveContext_HACK.ActiveCockpit);
                }

            }
#endif
            return Capture.Ignore;
        }

        public override Capture ForceEndCapture(InputState input, CaptureData data)
        {
            throw new NotImplementedException("FBEncoderCaptureBehavior.ForceEndCapture: this is an override behavior and does not capture!!");
        }
    }
}
