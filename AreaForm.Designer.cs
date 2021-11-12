
namespace AJeHelperD2R
{
    partial class AreaForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AreaForm));
            this.mImageFileTextBox = new System.Windows.Forms.TextBox();
            this.mImageFileButton = new System.Windows.Forms.Button();
            this.mBuffGroupBox = new System.Windows.Forms.GroupBox();
            this.mEffectNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.mEffectVolumnLabel = new System.Windows.Forms.Label();
            this.mEffectLabel = new System.Windows.Forms.Label();
            this.mEffectFileTextBox = new System.Windows.Forms.TextBox();
            this.mEffectFileButton = new System.Windows.Forms.Button();
            this.mBuffTimeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.mBuffTimeLabel = new System.Windows.Forms.Label();
            this.mImageLabel = new System.Windows.Forms.Label();
            this.mOKButton = new System.Windows.Forms.Button();
            this.mBuffGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mEffectNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mBuffTimeNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // mImageFileTextBox
            // 
            this.mImageFileTextBox.Location = new System.Drawing.Point(102, 19);
            this.mImageFileTextBox.Name = "mImageFileTextBox";
            this.mImageFileTextBox.Size = new System.Drawing.Size(243, 21);
            this.mImageFileTextBox.TabIndex = 1;
            // 
            // mImageFileButton
            // 
            this.mImageFileButton.Location = new System.Drawing.Point(351, 18);
            this.mImageFileButton.Name = "mImageFileButton";
            this.mImageFileButton.Size = new System.Drawing.Size(75, 23);
            this.mImageFileButton.TabIndex = 2;
            this.mImageFileButton.Text = "열기";
            this.mImageFileButton.UseVisualStyleBackColor = true;
            this.mImageFileButton.Click += new System.EventHandler(this.onImageFileButtonClick);
            // 
            // mBuffGroupBox
            // 
            this.mBuffGroupBox.Controls.Add(this.mEffectNumericUpDown);
            this.mBuffGroupBox.Controls.Add(this.mEffectVolumnLabel);
            this.mBuffGroupBox.Controls.Add(this.mEffectLabel);
            this.mBuffGroupBox.Controls.Add(this.mEffectFileTextBox);
            this.mBuffGroupBox.Controls.Add(this.mEffectFileButton);
            this.mBuffGroupBox.Controls.Add(this.mBuffTimeNumericUpDown);
            this.mBuffGroupBox.Controls.Add(this.mBuffTimeLabel);
            this.mBuffGroupBox.Controls.Add(this.mImageLabel);
            this.mBuffGroupBox.Controls.Add(this.mImageFileTextBox);
            this.mBuffGroupBox.Controls.Add(this.mImageFileButton);
            this.mBuffGroupBox.Location = new System.Drawing.Point(13, 13);
            this.mBuffGroupBox.Name = "mBuffGroupBox";
            this.mBuffGroupBox.Size = new System.Drawing.Size(432, 133);
            this.mBuffGroupBox.TabIndex = 4;
            this.mBuffGroupBox.TabStop = false;
            this.mBuffGroupBox.Text = "버프 정보";
            // 
            // mEffectNumericUpDown
            // 
            this.mEffectNumericUpDown.Location = new System.Drawing.Point(344, 94);
            this.mEffectNumericUpDown.Name = "mEffectNumericUpDown";
            this.mEffectNumericUpDown.Size = new System.Drawing.Size(76, 21);
            this.mEffectNumericUpDown.TabIndex = 10;
            this.mEffectNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.mEffectNumericUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // mEffectVolumnLabel
            // 
            this.mEffectVolumnLabel.Location = new System.Drawing.Point(206, 98);
            this.mEffectVolumnLabel.Name = "mEffectVolumnLabel";
            this.mEffectVolumnLabel.Size = new System.Drawing.Size(132, 12);
            this.mEffectVolumnLabel.TabIndex = 9;
            this.mEffectVolumnLabel.Text = "효과음 크기 :";
            this.mEffectVolumnLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // mEffectLabel
            // 
            this.mEffectLabel.Location = new System.Drawing.Point(4, 35);
            this.mEffectLabel.Name = "mEffectLabel";
            this.mEffectLabel.Size = new System.Drawing.Size(96, 63);
            this.mEffectLabel.TabIndex = 8;
            this.mEffectLabel.Text = "종료 시 효과음 :";
            this.mEffectLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // mEffectFileTextBox
            // 
            this.mEffectFileTextBox.Location = new System.Drawing.Point(102, 56);
            this.mEffectFileTextBox.Name = "mEffectFileTextBox";
            this.mEffectFileTextBox.Size = new System.Drawing.Size(243, 21);
            this.mEffectFileTextBox.TabIndex = 6;
            // 
            // mEffectFileButton
            // 
            this.mEffectFileButton.Location = new System.Drawing.Point(351, 55);
            this.mEffectFileButton.Name = "mEffectFileButton";
            this.mEffectFileButton.Size = new System.Drawing.Size(75, 23);
            this.mEffectFileButton.TabIndex = 7;
            this.mEffectFileButton.Text = "열기";
            this.mEffectFileButton.UseVisualStyleBackColor = true;
            this.mEffectFileButton.Click += new System.EventHandler(this.onEffectFileButtonClick);
            // 
            // mBuffTimeNumericUpDown
            // 
            this.mBuffTimeNumericUpDown.Location = new System.Drawing.Point(102, 94);
            this.mBuffTimeNumericUpDown.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.mBuffTimeNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.mBuffTimeNumericUpDown.Name = "mBuffTimeNumericUpDown";
            this.mBuffTimeNumericUpDown.Size = new System.Drawing.Size(80, 21);
            this.mBuffTimeNumericUpDown.TabIndex = 5;
            this.mBuffTimeNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.mBuffTimeNumericUpDown.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // mBuffTimeLabel
            // 
            this.mBuffTimeLabel.Location = new System.Drawing.Point(5, 98);
            this.mBuffTimeLabel.Name = "mBuffTimeLabel";
            this.mBuffTimeLabel.Size = new System.Drawing.Size(95, 12);
            this.mBuffTimeLabel.TabIndex = 4;
            this.mBuffTimeLabel.Text = "버프 시간(초) :";
            this.mBuffTimeLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // mImageLabel
            // 
            this.mImageLabel.Location = new System.Drawing.Point(3, 23);
            this.mImageLabel.Name = "mImageLabel";
            this.mImageLabel.Size = new System.Drawing.Size(97, 12);
            this.mImageLabel.TabIndex = 3;
            this.mImageLabel.Text = "이미지 파일   :";
            this.mImageLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // mOKButton
            // 
            this.mOKButton.Location = new System.Drawing.Point(331, 152);
            this.mOKButton.Name = "mOKButton";
            this.mOKButton.Size = new System.Drawing.Size(102, 32);
            this.mOKButton.TabIndex = 7;
            this.mOKButton.Text = "확인";
            this.mOKButton.UseVisualStyleBackColor = true;
            this.mOKButton.Click += new System.EventHandler(this.onOKButtonClick);
            // 
            // AreaForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(457, 196);
            this.Controls.Add(this.mOKButton);
            this.Controls.Add(this.mBuffGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AreaForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "버프 표시";
            this.TopMost = true;
            this.mBuffGroupBox.ResumeLayout(false);
            this.mBuffGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mEffectNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mBuffTimeNumericUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox mImageFileTextBox;
        private System.Windows.Forms.Button mImageFileButton;
        private System.Windows.Forms.GroupBox mBuffGroupBox;
        private System.Windows.Forms.Button mOKButton;
        private System.Windows.Forms.NumericUpDown mBuffTimeNumericUpDown;
        private System.Windows.Forms.Label mBuffTimeLabel;
        private System.Windows.Forms.Label mImageLabel;
        private System.Windows.Forms.Label mEffectLabel;
        private System.Windows.Forms.TextBox mEffectFileTextBox;
        private System.Windows.Forms.Button mEffectFileButton;
        private System.Windows.Forms.NumericUpDown mEffectNumericUpDown;
        private System.Windows.Forms.Label mEffectVolumnLabel;
    }
}