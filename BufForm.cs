using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

namespace AJeHelperD2R
{
    public partial class BufForm : Form
    {
        private bool mIsStart = false;
        private bool mIsPlayWave = false;
        private Image mImage = null;
        private System.Drawing.Pen mPenBlue = new System.Drawing.Pen(System.Drawing.Color.Blue, 2.0f);
        private SolidBrush mBrush = new SolidBrush(System.Drawing.Color.FromArgb(100, System.Drawing.Color.Red));
        private Font mFont = null;
        private float mFontSize = 14.0f;
        private int mHeightSize = 27;

        private string mEffectFileName = "";
        private int mEffectVolume = 100;

        public int RemainTime { get; set; } = 0;

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int nIndex);

        public BufForm(float fontSize, int heightSize, string soundFileName, int volume)
        {
            InitializeComponent();

            mFontSize = fontSize;
            mHeightSize = heightSize;

            this.DoubleBuffered = true;
            this.ShowInTaskbar = false;
            this.TransparencyKey = System.Drawing.Color.Wheat;
            this.BackColor = System.Drawing.Color.Wheat;
            this.Opacity = 0.8;
            this.TopMost = true;

            try
            {
                mFont = new Font(System.Drawing.FontFamily.GenericSansSerif, mFontSize, FontStyle.Bold);
                mEffectFileName = soundFileName;
                mEffectVolume = volume;
            }
            catch { }            
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            mIsStart = true;

            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.DrawRectangle(mPenBlue, new Rectangle(0, mHeightSize, this.Width, this.Height - mHeightSize));
            this.drawStringInCenter("" + RemainTime, mFont, new Rectangle(0, 0, this.Width, mHeightSize), e);

            if (mImage != null)
            {
                var destRect = new Rectangle(1, mHeightSize + 1, this.Width - 2, this.Height - 2 - mHeightSize);
                var srcRect = new Rectangle(0, 0, mImage.Width, mImage.Height);

                e.Graphics.DrawImage(mImage, destRect, srcRect, GraphicsUnit.Pixel);

                if (this.RemainTime == 0)
                {
                    e.Graphics.FillRectangle(mBrush, new Rectangle(1, mHeightSize + 1, this.Width - 2, this.Height - mHeightSize - 2));
                }
            }
        }

        private void drawStringInCenter(string txt, Font font, Rectangle rec, PaintEventArgs e)
        {
            StringFormat sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(txt, font, System.Drawing.Brushes.White, rec, sf);
        }

        public void setImage(string fileName)
        {
            try
            {
                mImage = Image.FromFile(fileName);
                if (mIsStart == true)
                {
                    if (RemainTime == 0)
                    {
                        this.Opacity = 0.8;
                    }
                    else
                    {
                        this.Opacity = 1.0;
                    }
                    this.Invalidate();
                }
            }
            catch
            {
                mImage = null;
            }
        }

        public void redrawForm()
        {
            if (mIsStart == true)
            {
                if (RemainTime == 0)
                {
                    this.Opacity = 0.8;
                    if (mIsPlayWave == true)
                    {
                        mIsPlayWave = false;

                        try
                        {
                            if (mEffectFileName.Length > 0)
                            {
                                var mediaPlayer = new MediaPlayer();
                                mediaPlayer.Open(new Uri(mEffectFileName));
                                mediaPlayer.Volume = (double)mEffectVolume / 100.0;
                                mediaPlayer.Play();
                            }
                        }
                        catch { }
                    }
                }
                else
                {
                    this.Opacity = 1.0;
                    mIsPlayWave = true;
                }

                this.Invalidate();
            }
        }
    }
}
