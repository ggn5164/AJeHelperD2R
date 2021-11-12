using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJeHelperD2R
{
    public class BufRectData
    {
        public bool Display { get; set; } = false;
        
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;

        public string ImageFile { get; set; } = "";
                
        public string EffectFile { get; set; } = "";

        public int EffectVolume { get; set; } = 50;

        public int BuffTime { get; set; } = 10;

        public float BuffFontSize { get; set; } = 14.0f;

        public int BuffHeightSize { get; set; } = 27;

        public void clear()
        {
            Display = false;
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
            ImageFile = "";
            EffectFile = "";
            EffectVolume = 50;
            BuffTime = 10;
            BuffFontSize = 14.0f;
            BuffHeightSize = 27;
        }

        public BufRectData clone()
        {
            var rectData = new BufRectData();
            rectData.Display = this.Display;
            rectData.X = this.X;
            rectData.Y = this.Y;
            rectData.Width = this.Width;
            rectData.Height = this.Height;
            rectData.ImageFile = this.ImageFile;
            rectData.EffectFile = this.EffectFile;
            rectData.EffectVolume = this.EffectVolume;
            rectData.BuffTime = this.BuffTime;
            rectData.BuffFontSize = this.BuffFontSize;
            rectData.BuffHeightSize = this.BuffHeightSize;
            return rectData;
        }

        public bool isChangeData(BufRectData controlData)
        {
            return (controlData.Display != this.Display ||
                    controlData.X != this.X ||
                    controlData.Y != this.Y ||
                    controlData.Width != this.Width ||
                    controlData.Height != this.Height ||
                    controlData.ImageFile.CompareTo(this.ImageFile) != 0 ||
                    controlData.EffectFile.CompareTo(this.EffectFile) != 0 ||
                    controlData.EffectVolume != this.EffectVolume ||
                    controlData.BuffTime != this.BuffTime ||
                    controlData.BuffFontSize != this.BuffFontSize ||
                    controlData.BuffHeightSize != this.BuffHeightSize);
        }
    }
}
