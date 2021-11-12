using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AJeHelperD2R
{
    public partial class AreaForm : Form
    {
        private ControlData mControlData = null;

        private int mIndex = 0;
        private List<RectForm> mRectFormList = new List<RectForm>();
        private List<BufRectData> mBufRectDataList = new List<BufRectData>();

        public event EventHandler onClosingCallback;

        public AreaForm(ControlData controlData, int index)
        {
            InitializeComponent();
            this.setLocalize();
            this.TopMost = true;
            mControlData = controlData;
            this.FormClosing += onClosing;
            mIndex = index;
        }

        private void setLocalize()
        {
            this.Text = StringLib.mBufDisplayButton;
            mBuffGroupBox.Text = StringLib.mBuffGroupBox;
            mImageLabel.Text = StringLib.mImageLabel;
            mEffectLabel.Text = StringLib.mEffectLabel;
            mBuffTimeLabel.Text = StringLib.mBuffTimeLabel;
            mEffectVolumnLabel.Text = StringLib.mEffectVolumnLabel;
            mImageFileButton.Text = StringLib.Open;
            mEffectFileButton.Text = StringLib.Open;
            mOKButton.Text = StringLib.OK;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            for (int i = 0; i < mControlData.BuffKeyData.BufRectDataList.Count; i++)
            {
                mBufRectDataList.Add(mControlData.BuffKeyData.BufRectDataList[i].clone());

                var form = new RectForm((mIndex == i) ? true : false);
                mRectFormList.Add(form);

                if (mControlData.BuffKeyData.BufRectDataList[i].Display == true)
                {
                    if (mIndex == i)
                    {
                        form.IsSelected = true;
                    }

                    form.setImage(mBufRectDataList[i].ImageFile);
                    form.Show();

                    bool isFirst = false;
                    if (mControlData.BuffKeyData.BufRectDataList[i].Width == 0 && mControlData.BuffKeyData.BufRectDataList[i].Height == 0)
                    {
                        isFirst = true;
                    }

                    if (isFirst == true)
                    {
                        form.Width = 70;
                        form.Height = 70;
                        form.Location = new Point(this.Location.X + this.Width / 2 - form.Width / 2,
                                                    this.Location.Y + this.Height + 10);
                    }
                    else
                    {
                        form.Width = mControlData.BuffKeyData.BufRectDataList[i].Width;
                        form.Height = mControlData.BuffKeyData.BufRectDataList[i].Height;
                        form.Location = new Point(mControlData.BuffKeyData.BufRectDataList[i].X, mControlData.BuffKeyData.BufRectDataList[i].Y);
                    }
                }
            }

            mImageFileTextBox.Text = mControlData.BuffKeyData.BufRectDataList[mIndex].ImageFile;
            mEffectFileTextBox.Text = mControlData.BuffKeyData.BufRectDataList[mIndex].EffectFile;
            mEffectNumericUpDown.Value = mControlData.BuffKeyData.BufRectDataList[mIndex].EffectVolume;
            mEffectNumericUpDown.ValueChanged += (object sender, EventArgs e2) =>
            {
                mBufRectDataList[mIndex].EffectVolume = Decimal.ToInt32(mEffectNumericUpDown.Value);
            };

            mBuffTimeNumericUpDown.Value = mControlData.BuffKeyData.BufRectDataList[mIndex].BuffTime;
            mBuffTimeNumericUpDown.ValueChanged += (object sender, EventArgs e2) =>
            {
                mBufRectDataList[mIndex].BuffTime = Decimal.ToInt32(mBuffTimeNumericUpDown.Value);
            };
        }

        private void onClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var temp in mRectFormList)
            {
                temp.Close();
                temp.Dispose();
            }
            mRectFormList.Clear();

            if (onClosingCallback != null)
                onClosingCallback(this, EventArgs.Empty);
        }

        private void onImageFileButtonClick(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Title = "Image file";
            ofd.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*";
            var dr = ofd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string fileName = ofd.FileName;

                mImageFileTextBox.Text = fileName;
                mBufRectDataList[mIndex].ImageFile = fileName;
                mRectFormList[mIndex].setImage(fileName);
            }
        }

        private void onEffectFileButtonClick(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Title = "Sound file";
            ofd.Filter = "Sound Files(*.WAV;*.MP3)|*.WAV;*.MP3|All files (*.*)|*.*";
            var dr = ofd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string fileName = ofd.FileName;

                mEffectFileTextBox.Text = fileName;
                mBufRectDataList[mIndex].EffectFile = fileName;
            }
        }

        private void onOKButtonClick(object sender, EventArgs e)
        {
            var form = mRectFormList[mIndex];
            var rectData = new BufRectData();
            rectData.Display = true;
            rectData.X = form.Location.X;
            rectData.Y = form.Location.Y;
            rectData.Width = form.Width;
            rectData.Height = form.Height;
            rectData.ImageFile = mImageFileTextBox.Text;
            rectData.EffectFile = mEffectFileTextBox.Text;
            rectData.EffectVolume = Decimal.ToInt32(mEffectNumericUpDown.Value);
            rectData.BuffTime = Decimal.ToInt32(mBuffTimeNumericUpDown.Value);
            mControlData.BuffKeyData.BufRectDataList[mIndex] = rectData;
            this.Close();
        }

        
    }
}
