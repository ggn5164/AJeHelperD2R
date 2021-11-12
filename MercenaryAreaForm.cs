using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AJeHelperD2R
{
    public partial class MercenaryAreaForm : Form
    {
        private ControlData mControlData = null;

        private MercenaryRectForm mCaptureForm = null;
        private MercenaryRectForm mDisplayForm = null;

        public event EventHandler onClosingCallback;

        private System.Threading.Timer mTimer = null;
        private object mTimerLock = new object();

        public MercenaryAreaForm(ControlData controlData)
        {
            InitializeComponent();
            this.setLocalize();

            this.TopMost = true;
            mControlData = controlData;
            this.FormClosing += onClosing;

            mOpacityNumericUpDown.Value = mControlData.MerceRectData.Opacity;
            mOpacityNumericUpDown.ValueChanged += (object sender, EventArgs e) =>
            {
                int value = Decimal.ToInt32(mOpacityNumericUpDown.Value);
                mDisplayForm.Opacity = ((double)value / 100.0f);
            };
        }

        private void setLocalize()
        {
            this.Text = StringLib.mMercenaryButton;
            mDescLabel.Text = StringLib.mDescLabel;
            mOpacityLabel.Text = StringLib.mOpacityLabel;
            mOKButton.Text = StringLib.OK;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            mCaptureForm = new MercenaryRectForm((int)MercenaryRectForm.MERCENARY_TYPE.CAPTURE_MOVE);
            mCaptureForm.IsSelected = true;
            mCaptureForm.Show();

            bool isFirst = false;
            if (mControlData.MerceRectData.CaptureWidth == 0 && mControlData.MerceRectData.CaptureHeight == 0)
            {
                isFirst = true;
            }

            if (isFirst == true)
            {
                mCaptureForm.Width = 100;
                mCaptureForm.Height = 100;
                mCaptureForm.Location = new Point(this.Location.X + this.Width / 2 - mCaptureForm.Width / 2 - 100,
                                                    this.Location.Y + this.Height + 10);
            }
            else
            {
                mCaptureForm.Width = mControlData.MerceRectData.CaptureWidth;
                mCaptureForm.Height = mControlData.MerceRectData.CaptureHeight;
                mCaptureForm.Location = new Point(mControlData.MerceRectData.CapturePointX, mControlData.MerceRectData.CapturePointY);
            }

            mDisplayForm = new MercenaryRectForm((int)MercenaryRectForm.MERCENARY_TYPE.DISPLAY_MOVE);
            mDisplayForm.IsSelected = true;
            mDisplayForm.Show();

            isFirst = false;
            if (mControlData.MerceRectData.DisplayWidth == 0 && mControlData.MerceRectData.DisplayHeight == 0)
            {
                isFirst = true;
            }

            if (isFirst == true)
            {
                mDisplayForm.Width = 100;
                mDisplayForm.Height = 100;
                mDisplayForm.Location = new Point(this.Location.X + this.Width / 2 - mDisplayForm.Width / 2 + 100,
                                                    this.Location.Y + this.Height + 10);
            }
            else
            {
                mDisplayForm.Width = mControlData.MerceRectData.DisplayWidth;
                mDisplayForm.Height = mControlData.MerceRectData.DisplayHeight;
                mDisplayForm.Location = new Point(mControlData.MerceRectData.DisplayPointX, mControlData.MerceRectData.DisplayPointY);
            }

            mTimer = new System.Threading.Timer(timerCallback);
            mTimer.Change(100, mControlData.MerceRectData.Delay);
        }

        private void onClosing(object sender, FormClosingEventArgs e)
        {
            mTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            mTimer.Dispose();

            mCaptureForm.Close();
            mCaptureForm.Dispose();

            mDisplayForm.Close();
            mDisplayForm.Dispose();

            if (onClosingCallback != null)
                onClosingCallback(this, EventArgs.Empty);
        }

        private void onOKButtonClick(object sender, EventArgs e)
        {
            mControlData.MerceRectData.Display = true;
            mControlData.MerceRectData.Opacity = Decimal.ToInt32(mOpacityNumericUpDown.Value);
            mControlData.MerceRectData.CapturePointX = mCaptureForm.Location.X;
            mControlData.MerceRectData.CapturePointY = mCaptureForm.Location.Y;
            mControlData.MerceRectData.CaptureWidth = mCaptureForm.Width;
            mControlData.MerceRectData.CaptureHeight = mCaptureForm.Height;
            mControlData.MerceRectData.DisplayPointX = mDisplayForm.Location.X;
            mControlData.MerceRectData.DisplayPointY = mDisplayForm.Location.Y;
            mControlData.MerceRectData.DisplayWidth = mDisplayForm.Width;
            mControlData.MerceRectData.DisplayHeight = mDisplayForm.Height;
            this.Close();
        }

        private void timerCallback(object state)
        {
            if (Monitor.TryEnter(mTimerLock) == false)
                return;

            try
            {
                //size 객체 생성  
                Size size = new Size(mCaptureForm.Width - 2, mCaptureForm.Height - 2);

                //Bitmap 객체 생성   
                Bitmap bitmap = new Bitmap(mCaptureForm.Width - 2, mCaptureForm.Height - 2);

                //Graphics 객체 생성   
                Graphics graphics = Graphics.FromImage(bitmap);

                // Graphics 객체의 CopyFromScreen()메서드로 bitmap 객체에 Screen을 캡처하여 저장   
                graphics.CopyFromScreen(mCaptureForm.Location.X + 1, mCaptureForm.Location.Y + 1, 0, 0, size);

                this.BeginInvoke(new Action(delegate ()
                {
                    mDisplayForm.setImage(bitmap);
                }));
            }
            finally
            {
                Monitor.Exit(mTimerLock);
            }
        }
    }
}
