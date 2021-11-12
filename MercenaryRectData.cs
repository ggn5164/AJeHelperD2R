using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJeHelperD2R
{
    public class MercenaryRectData
    {
        public bool Display { get; set; } = false;

        public int Delay { get; set; } = 200;

        public int Opacity { get; set; } = 100;

        public int CapturePointX { get; set; } = 0;
        public int CapturePointY { get; set; } = 0;
        public int CaptureWidth { get; set; } = 0;
        public int CaptureHeight { get; set; } = 0;

        public int DisplayPointX { get; set; } = 0;
        public int DisplayPointY { get; set; } = 0;
        public int DisplayWidth { get; set; } = 0;
        public int DisplayHeight { get; set; } = 0;

        public void clear()
        {
            Display = false;
            Delay = 200;
            Opacity = 100;
            CapturePointX = 0;
            CapturePointY = 0;
            CaptureWidth = 0;
            CaptureHeight = 0;
            DisplayPointX = 0;
            DisplayPointY = 0;
            DisplayWidth = 0;
            DisplayHeight = 0;
        }

        public MercenaryRectData clone()
        {
            var rectData = new MercenaryRectData();
            rectData.Display = this.Display;
            rectData.Delay = this.Delay;
            rectData.Opacity = this.Opacity;
            rectData.CapturePointX = this.CapturePointX;
            rectData.CapturePointY = this.CapturePointY;
            rectData.CaptureWidth = this.CaptureWidth;
            rectData.CaptureHeight = this.CaptureHeight;
            rectData.DisplayPointX = this.DisplayPointX;
            rectData.DisplayPointY = this.DisplayPointY;
            rectData.DisplayWidth = this.DisplayWidth;
            rectData.DisplayHeight = this.DisplayHeight;
            return rectData;
        }

        public bool isChangeData(MercenaryRectData controlData)
        {
            return (controlData.Display != this.Display ||
                    controlData.Delay != this.Delay ||
                    controlData.Opacity != this.Opacity ||
                    controlData.CapturePointX != this.CapturePointX ||
                    controlData.CapturePointY != this.CapturePointY ||
                    controlData.CaptureWidth != this.CaptureWidth ||
                    controlData.CaptureHeight != this.CaptureHeight ||
                    controlData.DisplayPointX != this.DisplayPointX ||
                    controlData.DisplayPointY != this.DisplayPointY ||
                    controlData.DisplayWidth != this.DisplayWidth ||
                    controlData.DisplayHeight != this.DisplayHeight);
        }
    }
}
