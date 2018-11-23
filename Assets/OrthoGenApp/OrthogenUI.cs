using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using g3;
using f3;

namespace orthogen
{
    class OrthogenUI
    {
        public static FContext ActiveContext;
		public static FScene ActiveScene {
			get { return ActiveContext.Scene; }
		}


        public static Colorf ButtonTextColor = Colorf.Silver;
        public static Colorf DisabledButtonTextColor = new Colorf(0.2f);
        public static Colorf ButtonBGColor = Colorf.DarkSlateGrey;


        public static float PixelScale {
            get { return ActiveContext.ActiveCockpit.GetPixelScale(); }
        }


        public static float StandardPanelCornerRadius() {
            return 5 * PixelScale;
        }
        public static float StandardButtonBorderWidth {
            get { return 1.5f * PixelScale; }
        }


        public static float MenuButtonWidth {
            get { return 125 * PixelScale; }
        }
        public static float MenuButtonHeight {
            get { return 30 * PixelScale; }
        }
        public static float MenuButtonTextHeight {
            get { return 20 * PixelScale; }
        }

        public static HUDShape MakeMenuButtonRect(float width, float height)
        {
//            return new HUDShape(HUDShapeType.RoundRect, width, height, height * 0.25f, 6, false);
			return new HUDShape(HUDShapeType.Rectangle, width, height);
		}





        public static float CurveOnSurfaceOffsetTol = 0.05f;



    }
}
