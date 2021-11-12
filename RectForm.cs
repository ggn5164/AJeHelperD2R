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

namespace AJeHelperD2R
{
    public partial class RectForm : Form
    {
        private bool mIsEnableMove = false;
        private bool mIsStart = false;
        private bool mIsMove = false;
        private Point mDownPoint = new Point();

        private const int cGrip = 16;
        private const int cCaption = 16;

        const int WM_NCHITTEST = 0x84;
        const int HTCLIENT = 1;
        const int HTCAPTION = 2;

        public bool IsSelected { get; set; } = false;

        public Image mImage { get; set; } = null;

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int nIndex);

        public RectForm(bool isEnableMove)
        {
            InitializeComponent();

            mIsEnableMove = isEnableMove;
            this.DoubleBuffered = true;

            this.ShowInTaskbar = false;
            this.BackColor = Color.White;
            //this.Opacity = 0.7;
            this.TopMost = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            mIsStart = true;

            if (mIsEnableMove == true)
            {
                this.SetStyle(ControlStyles.ResizeRedraw, true);
                this.MouseDown += onMouseDown;
                this.MouseMove += onMouseMove;
                this.MouseUp += onMouseUp;
            }
            else
            {
                this.TransparencyKey = Color.Wheat;
                this.BackColor = Color.White;
                int initialStyle = GetWindowLong(this.Handle, -20);
                SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                (IsSelected == true) ? Color.Blue : Color.Red, 2, ButtonBorderStyle.Solid,
                (IsSelected == true) ? Color.Blue : Color.Red, 2, ButtonBorderStyle.Solid,
                (IsSelected == true) ? Color.Blue : Color.Red, 2, ButtonBorderStyle.Solid,
                (IsSelected == true) ? Color.Blue : Color.Red, 2, ButtonBorderStyle.Solid);

            if (mImage != null)
            {
                var destRect = new Rectangle(1, 1, this.Width - 2, this.Height - 2);
                var srcRect = new Rectangle(0, 0, mImage.Width, mImage.Height);

                e.Graphics.DrawImage(mImage, destRect, srcRect, GraphicsUnit.Pixel);
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case WM_NCHITTEST:
                    if (m.Result == (IntPtr)HTCLIENT)
                    {
                        var p = this.PointToClient(new Point(m.LParam.ToInt32()));

                        m.Result =
                            (IntPtr)
                            (p.X <= 6
                                 ? p.Y <= 6 ? 13 : p.Y >= this.Height - 7 ? 16 : 10
                                 : p.X >= this.Width - 7
                                       ? p.Y <= 6 ? 14 : p.Y >= this.Height - 7 ? 17 : 11
                                       : p.Y <= 6 ? 12 : p.Y >= this.Height - 7 ? 15 : p.Y <= 24 ? 2 : 1);
                    }
                    break;
                default:
                    break;
            }
        }

        private void onMouseDown(object sender, MouseEventArgs e)
        {
            if (IsSelected == false)
                return;

            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                mIsMove = true;
                mDownPoint.X = e.X;
                mDownPoint.Y = e.Y;

                this.Invalidate();
            }
        }

        private void onMouseMove(object sender, MouseEventArgs e)
        {
            if (mIsMove == true)
            {
                this.Location = new Point(this.Left - (mDownPoint.X - e.X), this.Top - (mDownPoint.Y - e.Y));
            }
        }

        private void onMouseUp(object sender, MouseEventArgs e)
        {
            if (mIsMove == true)
            {
                mIsMove = false;
            }
        }

        public void setImage(string fileName)
        {
            try
            {
                mImage = Image.FromFile(fileName);

                if (mIsStart == true)
                    this.Invalidate();
            }
            catch
            {
                mImage = null;
            }
        }
    }
}
