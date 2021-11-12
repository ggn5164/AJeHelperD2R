
namespace AJeHelperD2R
{
    partial class MercenaryAreaForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MercenaryAreaForm));
            this.mDescLabel = new System.Windows.Forms.Label();
            this.mOKButton = new System.Windows.Forms.Button();
            this.mOpacityLabel = new System.Windows.Forms.Label();
            this.mOpacityNumericUpDown = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.mOpacityNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // mDescLabel
            // 
            this.mDescLabel.Location = new System.Drawing.Point(8, 9);
            this.mDescLabel.Name = "mDescLabel";
            this.mDescLabel.Size = new System.Drawing.Size(206, 54);
            this.mDescLabel.TabIndex = 0;
            this.mDescLabel.Text = "빨간색 박스가 용병 화면 있는 위치    파란색 박스가 복사해서 표시할 위치";
            this.mDescLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mOKButton
            // 
            this.mOKButton.Location = new System.Drawing.Point(5, 97);
            this.mOKButton.Name = "mOKButton";
            this.mOKButton.Size = new System.Drawing.Size(209, 40);
            this.mOKButton.TabIndex = 1;
            this.mOKButton.Text = "확인";
            this.mOKButton.UseVisualStyleBackColor = true;
            this.mOKButton.Click += new System.EventHandler(this.onOKButtonClick);
            // 
            // mOpacityLabel
            // 
            this.mOpacityLabel.Location = new System.Drawing.Point(5, 70);
            this.mOpacityLabel.Name = "mOpacityLabel";
            this.mOpacityLabel.Size = new System.Drawing.Size(75, 12);
            this.mOpacityLabel.TabIndex = 2;
            this.mOpacityLabel.Text = "투명도 :";
            this.mOpacityLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // mOpacityNumericUpDown
            // 
            this.mOpacityNumericUpDown.Location = new System.Drawing.Point(83, 66);
            this.mOpacityNumericUpDown.Name = "mOpacityNumericUpDown";
            this.mOpacityNumericUpDown.Size = new System.Drawing.Size(85, 21);
            this.mOpacityNumericUpDown.TabIndex = 3;
            this.mOpacityNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.mOpacityNumericUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // MercenaryAreaForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(220, 147);
            this.Controls.Add(this.mOpacityNumericUpDown);
            this.Controls.Add(this.mOpacityLabel);
            this.Controls.Add(this.mOKButton);
            this.Controls.Add(this.mDescLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MercenaryAreaForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "용병 표시";
            ((System.ComponentModel.ISupportInitialize)(this.mOpacityNumericUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label mDescLabel;
        private System.Windows.Forms.Button mOKButton;
        private System.Windows.Forms.Label mOpacityLabel;
        private System.Windows.Forms.NumericUpDown mOpacityNumericUpDown;
    }
}