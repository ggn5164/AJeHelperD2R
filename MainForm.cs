using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AJeHelperD2R
{
    public partial class MainForm : Form
    {
        private string mJsonFileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "AJeHelperD2R.json";

        private object mLock = new object();

        private List<Control> mControlList = new List<Control>();

        private List<TextBox> mKeyTextBoxList = new List<TextBox>();

        private List<TextBox> mStartTextBoxList = new List<TextBox>();
        private List<TextBox> mPauseTextBoxList = new List<TextBox>();
        private List<TextBox> mControlKeyTextBoxList = new List<TextBox>();
        private List<TextBox> mTechKeyTextBoxList = new List<TextBox>();
        private List<ComboBox> mButtonTypeComboBoxList = new List<ComboBox>();
        private List<CheckBox> mShiftCheckBoxList = new List<CheckBox>();
        private List<ComboBox> mControlTypeComboBoxList = new List<ComboBox>();
        private List<TextBox> mMemoTextBoxList = new List<TextBox>();
        private List<TextBox> mPotionNowKeyTextBoxList = new List<TextBox>();
        private List<TextBox> mPotionReplaceKeyTextBoxList = new List<TextBox>();
        private List<TextBox> mBuffKeyTextBoxList = new List<TextBox>();
        private List<CheckBox> mBuffCheckBoxList = new List<CheckBox>();
        private List<CheckBox> mBuffDisplayCheckBoxList = new List<CheckBox>();
        private List<Button> mBuffDisplayButtonList = new List<Button>();

        private ControlData mControlData = new ControlData();
        private ControlData mOriginControlData = new ControlData();

        private Dictionary<int, StartKeyData> mStartMouseDictionary = new Dictionary<int, StartKeyData>();
        private Dictionary<int, StartKeyData> mStartKeyDictionary = new Dictionary<int, StartKeyData>();
        private Dictionary<int, PauseKeyData> mPauseMouseDictionary = new Dictionary<int, PauseKeyData>();
        private Dictionary<int, PauseKeyData> mPauseKeyDictionary = new Dictionary<int, PauseKeyData>();
        private Dictionary<int, ControlKeyData> mControlMouseDictionary = new Dictionary<int, ControlKeyData>();
        private Dictionary<int, ControlKeyData> mControlKeyDictionary = new Dictionary<int, ControlKeyData>();
        private Dictionary<int, PotionKeyData> mPotionMouseDictionary = new Dictionary<int, PotionKeyData>();
        private Dictionary<int, PotionKeyData> mPotionKeyDictionary = new Dictionary<int, PotionKeyData>();

        private bool mIsSetKey = false;
        private int mWParam = HookManager.NONE;
        private int mKeyCode = (int)Keys.None;

        private bool mIsArea = false;
        private bool mIsStart = false;
        private bool mIsPause = false;
        private bool mIsBuff = false;

        // 마지막 용병 포션 먹은 시간
        private long mLastPotionTime = 0;

        private int mPauseWParam = HookManager.NONE;
        private int mPauseKeyCode = (int)Keys.None;

        private bool mIsShiftDown = false;
        private bool mIsLeftDown = false;
        private bool mIsRightDown = false;

        private System.Threading.Timer mControlTimer = null;
        private object mControlTimerLock = new List<object>();
        private ControlKeyData mNowControlKeyData = null;

        private BufForm[] mBufFormList = null;
        private System.Threading.Timer[] mBufDisplayTimerList = null;
        private object[] mBufDisplayTimerLockList = null;

        private System.Threading.Timer mMercenaryTimer = null;
        private object mMercenaryTimerLock = new object();
        private MercenaryRectForm mMercenaryCaptureForm = null;
        private MercenaryRectForm mMercenaryDisplayForm = null;

        public MainForm()
        {
            InitializeComponent();
            this.setLocalize();

            this.FormClosing += onClosing;

            mControlTypeToolTip.ToolTipIcon = ToolTipIcon.None;
            mControlTypeToolTip.IsBalloon = true;
            mControlTypeToolTip.ShowAlways = true;
            mControlTypeToolTip.SetToolTip(mControlTypeToolTipButton, StringLib.ToolTip);

            this.setList();

            // 헬퍼사용 시작/종료 키
            for (int i = 0; i < mStartTextBoxList.Count; i++)
            {
                mStartTextBoxList[i].GotFocus += onTextBoxGotFocus;
                mStartTextBoxList[i].LostFocus += onTextBoxLostFocus;
                mKeyTextBoxList.Add(mStartTextBoxList[i]);
                mControlList.Add(mStartTextBoxList[i]);
            }

            // 잠시 멈춤 키
            for (int i = 0; i < mPauseTextBoxList.Count; i++)
            {
                mPauseTextBoxList[i].GotFocus += onTextBoxGotFocus;
                mPauseTextBoxList[i].LostFocus += onTextBoxLostFocus;
                mKeyTextBoxList.Add(mPauseTextBoxList[i]);
                mControlList.Add(mPauseTextBoxList[i]);
            }

            // 정지 키
            mShiftKeyTextBox.GotFocus += onTextBoxGotFocus;
            mShiftKeyTextBox.LostFocus += onTextBoxLostFocus;
            mKeyTextBoxList.Add(mShiftKeyTextBox);
            mControlList.Add(mShiftKeyTextBox);            

            // 동작 시작/종료 키
            for (int i = 0; i < mControlKeyTextBoxList.Count; i++)
            {
                mControlKeyTextBoxList[i].GotFocus += onTextBoxGotFocus;
                mControlKeyTextBoxList[i].LostFocus += onTextBoxLostFocus;
                mKeyTextBoxList.Add(mControlKeyTextBoxList[i]);
                mControlList.Add(mControlKeyTextBoxList[i]);
            }

            // 기술 키
            for (int i = 0; i < mTechKeyTextBoxList.Count; i++)
            {
                mTechKeyTextBoxList[i].GotFocus += onTextBoxGotFocus;
                mTechKeyTextBoxList[i].LostFocus += onTextBoxLostFocus;
                mKeyTextBoxList.Add(mTechKeyTextBoxList[i]);
                mControlList.Add(mTechKeyTextBoxList[i]);
            }

            for (int i = 0; i < mButtonTypeComboBoxList.Count; i++)
            {
                mButtonTypeComboBoxList[i].Items.Add(StringLib.Mouse_Left);
                mButtonTypeComboBoxList[i].Items.Add(StringLib.Mouse_Right);
                mButtonTypeComboBoxList[i].SelectedIndex = 0;

                int index = i;
                mButtonTypeComboBoxList[i].SelectedValueChanged += (object sender, EventArgs e) =>
                {
                    var comboBox = (ComboBox)sender;
                    mControlData.ControlKeyDataList[index].ButtonType = comboBox.SelectedIndex;
                };

                mControlList.Add(mButtonTypeComboBoxList[i]);
            }

            for (int i = 0; i < mShiftCheckBoxList.Count; i++)
            {
                int index = i;
                mShiftCheckBoxList[i].CheckedChanged += (object sender, EventArgs e) =>
                {
                    var checkBox = (CheckBox)sender;
                    mControlData.ControlKeyDataList[index].IsShift = checkBox.Checked;
                };

                mControlList.Add(mShiftCheckBoxList[i]);
            }

            for (int i = 0; i < mControlTypeComboBoxList.Count; i++)
            {
                mControlTypeComboBoxList[i].Items.Add(StringLib.Repeat_Toggle);
                mControlTypeComboBoxList[i].Items.Add(StringLib.Repeat_Holding);
                mControlTypeComboBoxList[i].Items.Add(StringLib.Push_Toggle);
                mControlTypeComboBoxList[i].Items.Add(StringLib.Push_Holding);
                mControlTypeComboBoxList[i].Items.Add(StringLib.Just_Once);
                mControlTypeComboBoxList[i].SelectedIndex = 0;

                int index = i;
                mControlTypeComboBoxList[i].SelectedValueChanged += (object sender, EventArgs e) =>
                {
                    var comboBox = (ComboBox)sender;
                    mControlData.ControlKeyDataList[index].ControlType = comboBox.SelectedIndex;
                };

                mControlList.Add(mControlTypeComboBoxList[i]);
            }

            for (int i = 0; i < mMemoTextBoxList.Count; i++)
            {
                int index = i;
                mMemoTextBoxList[i].TextChanged += (object sender, EventArgs e) =>
                {
                    var textBox = (TextBox)sender;
                    mControlData.ControlKeyDataList[index].MemoString = textBox.Text;
                };

                mControlList.Add(mMemoTextBoxList[i]);
            }

            // 반복 딜레이
            mControlRepeatNumericUpDown.ValueChanged += (object sender, EventArgs e) =>
            {
                mControlData.ControlRepeatTime = Decimal.ToInt32(mControlRepeatNumericUpDown.Value);
            };
            mControlList.Add(mControlRepeatNumericUpDown);


            // 용병 물약 키
            // 현재 물약 키
            for (int i = 0; i < mPotionNowKeyTextBoxList.Count; i++)
            {
                mPotionNowKeyTextBoxList[i].GotFocus += onTextBoxGotFocus;
                mPotionNowKeyTextBoxList[i].LostFocus += onTextBoxLostFocus;
                mKeyTextBoxList.Add(mPotionNowKeyTextBoxList[i]);
                mControlList.Add(mPotionNowKeyTextBoxList[i]);
            }

            // 대체 키
            for (int i = 0; i < mPotionReplaceKeyTextBoxList.Count; i++)
            {
                mPotionReplaceKeyTextBoxList[i].GotFocus += onTextBoxGotFocus;
                mPotionReplaceKeyTextBoxList[i].LostFocus += onTextBoxLostFocus;
                mKeyTextBoxList.Add(mPotionReplaceKeyTextBoxList[i]);
                mControlList.Add(mPotionReplaceKeyTextBoxList[i]);
            }


            // 버프 키
            // 버프 시작
            mBufStartTextBox.GotFocus += onTextBoxGotFocus;
            mBufStartTextBox.LostFocus += onTextBoxLostFocus;
            mKeyTextBoxList.Add(mBufStartTextBox);
            mControlList.Add(mBufStartTextBox);

            // 무기 교체
            mBufChangeTextBox.GotFocus += onTextBoxGotFocus;
            mBufChangeTextBox.LostFocus += onTextBoxLostFocus;
            mKeyTextBoxList.Add(mBufChangeTextBox);
            mControlList.Add(mBufChangeTextBox);

            // 버프키
            for (int i = 0; i < mBuffKeyTextBoxList.Count; i++)
            {
                mBuffKeyTextBoxList[i].GotFocus += onTextBoxGotFocus;
                mBuffKeyTextBoxList[i].LostFocus += onTextBoxLostFocus;
                mKeyTextBoxList.Add(mBuffKeyTextBoxList[i]);
                mControlList.Add(mBuffKeyTextBoxList[i]);
            }

            // 체크박스
            for (int i = 0; i < mBuffCheckBoxList.Count; i++)
            {
                int index = i;
                mBuffCheckBoxList[i].CheckedChanged += (object sender, EventArgs e) =>
                {
                    var checkBox = (CheckBox)sender;
                    mControlData.BuffKeyData.ChangeCheckList[index] = checkBox.Checked;
                };

                mControlList.Add(mBuffCheckBoxList[i]);
            }

            // 딜레이
            mControlList.Add(mBufNumericUpDown);
            mBufNumericUpDown.ValueChanged += (object sender, EventArgs e) =>
            {
                mControlData.BuffKeyData.DelayTime = Decimal.ToInt32(mBufNumericUpDown.Value);
            };

            // 버프 표시
            for (int i = 0; i < mBuffDisplayCheckBoxList.Count; i++)
            {
                int index = i;
                mBuffDisplayButtonList[index].Enabled = false;
                mBuffDisplayCheckBoxList[i].CheckedChanged += (object sender, EventArgs e) =>
                {
                    var checkBox = (CheckBox)sender;
                    mControlData.BuffKeyData.BufRectDataList[index].Display = checkBox.Checked;

                    mBuffDisplayButtonList[index].Enabled = checkBox.Checked;
                };
                mControlList.Add(mBuffDisplayCheckBoxList[i]);
                mControlList.Add(mBuffDisplayButtonList[i]);
            }

            // 용병 표시
            mMercenaryCheckBox.CheckedChanged += (object sender, EventArgs e) =>
            {
                var checkBox = (CheckBox)sender;
                mControlData.MerceRectData.Display = checkBox.Checked;
                mMercenaryButton.Enabled = checkBox.Checked;
            };
            mControlList.Add(mMercenaryCheckBox);
            mControlList.Add(mMercenaryButton);

            mProgramTextBox.TextChanged += (object sender, EventArgs e) =>
            {
                mControlData.ProgramName = mProgramTextBox.Text;
            };
            mProgramTextBox.Text = "Diablo II: Resurrected";
            mControlData.ProgramName = mProgramTextBox.Text;
            mControlList.Add(mProgramTextBox);

            mControlList.Add(mNewButton);
            mControlList.Add(mSaveButton);
            mControlList.Add(mLoadButton);
            //mControlList.Add(mDonateButton);

            string fileName = this.read();
            if (fileName.Length > 0)
            {
                this.Text = "AJeHelperD2R(v" + Application.ProductVersion + ") - " + Path.GetFileName(fileName);
            }
            else
            {
                this.Text = "AJeHelperD2R(v" + Application.ProductVersion + ")";
            }

            mControlTimer = new System.Threading.Timer(controlButtonProcess);

            // 버프 표시
            mBufFormList = new BufForm[mControlData.BuffKeyData.BufRectDataList.Count];
            mBufDisplayTimerList = new System.Threading.Timer[mControlData.BuffKeyData.BufRectDataList.Count];
            mBufDisplayTimerLockList = new object[mControlData.BuffKeyData.BufRectDataList.Count];
            for (int i = 0; i < mControlData.BuffKeyData.BufRectDataList.Count; i++)
            {
                int index = i;
                mBufDisplayTimerList[i] = new System.Threading.Timer((object sender) =>
                {
                    this.bufDisplayTimerCallback(index);
                });

                mBufDisplayTimerLockList[i] = new object();
            }

            // 용병 표시 타이머
            mMercenaryTimer = new System.Threading.Timer(mercenaryDisplayTimerCallback);
        }

        private void setLocalize()
        {
            mGroupBox1.Text = StringLib.mGoupBox1;
            mGroupBox2.Text = StringLib.mGoupBox2;
            mGroupBox3.Text = StringLib.mGoupBox3;
            mGroupBox4.Text = StringLib.mGoupBox4;
            mGroupBox5.Text = StringLib.mGoupBox5;
            mShiftKeyLabel.Text = StringLib.mShiftKeyLabel;
            mControlRepeatLabel.Text = StringLib.mControlRepeatLabel;
            mControlKeyLabel.Text = StringLib.mControlKeyLabel;
            mTechKeyLabel.Text = StringLib.mTechKeyLabel;
            mButtonTypeLabel.Text = StringLib.mButtonTypeLabel;
            mShiftLabel.Text = StringLib.mShiftLabel;
            mControlTypeLabel.Text = StringLib.mControlTypeLabel;
            mMemoLabel.Text = StringLib.mMemoLabel;
            mShiftCheckBox1.Text = StringLib.Push;
            mShiftCheckBox2.Text = StringLib.Push;
            mShiftCheckBox3.Text = StringLib.Push;
            mShiftCheckBox4.Text = StringLib.Push;
            mShiftCheckBox5.Text = StringLib.Push;
            mShiftCheckBox6.Text = StringLib.Push;
            mShiftCheckBox7.Text = StringLib.Push;
            mShiftCheckBox8.Text = StringLib.Push;
            mShiftCheckBox9.Text = StringLib.Push;
            mShiftCheckBox10.Text = StringLib.Push;
            mShiftCheckBox11.Text = StringLib.Push;
            mShiftCheckBox12.Text = StringLib.Push;
            mPotionNowKeyLabel1.Text = StringLib.mPotionNowKeyLabel + " ①";
            mPotionNowKeyLabel2.Text = StringLib.mPotionNowKeyLabel + " ②";
            mPotionNowKeyLabel3.Text = StringLib.mPotionNowKeyLabel + " ③";
            mPotionNowKeyLabel4.Text = StringLib.mPotionNowKeyLabel + " ④";
            mPotionReplaceKeyLabel1.Text = StringLib.mPotionReplaceKeyLabel + " ①";
            mPotionReplaceKeyLabel2.Text = StringLib.mPotionReplaceKeyLabel + " ②";
            mPotionReplaceKeyLabel3.Text = StringLib.mPotionReplaceKeyLabel + " ③";
            mPotionReplaceKeyLabel4.Text = StringLib.mPotionReplaceKeyLabel + " ④";
            mBufStartLabel.Text = StringLib.mBufStartLabel;
            mBufChangeLabel.Text = StringLib.mBufChangeLabel;
            mBufLabel.Text = StringLib.mBufLabel;
            mBufKeyLabel1.Text = StringLib.mBufKeyLabel + " ①";
            mBufKeyLabel2.Text = StringLib.mBufKeyLabel + " ②";
            mBufKeyLabel3.Text = StringLib.mBufKeyLabel + " ③";
            mBufKeyLabel4.Text = StringLib.mBufKeyLabel + " ④";
            mBufKeyLabel5.Text = StringLib.mBufKeyLabel + " ⑤";
            mBufKeyLabel6.Text = StringLib.mBufKeyLabel + " ⑥";
            mBufKeyLabel7.Text = StringLib.mBufKeyLabel + " ⑦";
            mBufKeyLabel8.Text = StringLib.mBufKeyLabel + " ⑧";
            mBufKeyLabel9.Text = StringLib.mBufKeyLabel + " ⑨";
            mBufKeyLabel10.Text = StringLib.mBufKeyLabel + " ⑩";
            mMercenaryButton.Text = StringLib.mMercenaryButton;
            mBufDisplayButton1.Text = StringLib.mBufDisplayButton;
            mBufDisplayButton2.Text = StringLib.mBufDisplayButton;
            mBufDisplayButton3.Text = StringLib.mBufDisplayButton;
            mBufDisplayButton4.Text = StringLib.mBufDisplayButton;
            mBufDisplayButton5.Text = StringLib.mBufDisplayButton;
            mBufDisplayButton6.Text = StringLib.mBufDisplayButton;
            mBufDisplayButton7.Text = StringLib.mBufDisplayButton;
            mBufDisplayButton8.Text = StringLib.mBufDisplayButton;
            mBufDisplayButton9.Text = StringLib.mBufDisplayButton;
            mBufDisplayButton10.Text = StringLib.mBufDisplayButton;
            mDonateButton.Text = StringLib.Donate;
            mProgramLabel.Text = StringLib.mProgramLabel;
            mNewButton.Text = StringLib.New;
            mSaveButton.Text = StringLib.Save;
            mLoadButton.Text = StringLib.Load;
            mToolStripStatusLabel.Text = StringLib.Status + " " + StringLib.Stop;
        }

        private void setList()
        {
            mStartTextBoxList.Add(mStartTextBox1);
            mStartTextBoxList.Add(mStartTextBox2);
            mStartTextBoxList.Add(mStartTextBox3);
            mStartTextBoxList.Add(mStartTextBox4);

            mPauseTextBoxList.Add(mPauseTextBox1);
            mPauseTextBoxList.Add(mPauseTextBox2);
            mPauseTextBoxList.Add(mPauseTextBox3);
            mPauseTextBoxList.Add(mPauseTextBox4);
            mPauseTextBoxList.Add(mPauseTextBox5);
            mPauseTextBoxList.Add(mPauseTextBox6);
            mPauseTextBoxList.Add(mPauseTextBox7);

            mControlKeyTextBoxList.Add(mControlKeyTextBox1);
            mControlKeyTextBoxList.Add(mControlKeyTextBox2);
            mControlKeyTextBoxList.Add(mControlKeyTextBox3);
            mControlKeyTextBoxList.Add(mControlKeyTextBox4);
            mControlKeyTextBoxList.Add(mControlKeyTextBox5);
            mControlKeyTextBoxList.Add(mControlKeyTextBox6);
            mControlKeyTextBoxList.Add(mControlKeyTextBox7);
            mControlKeyTextBoxList.Add(mControlKeyTextBox8);
            mControlKeyTextBoxList.Add(mControlKeyTextBox9);
            mControlKeyTextBoxList.Add(mControlKeyTextBox10);
            mControlKeyTextBoxList.Add(mControlKeyTextBox11);
            mControlKeyTextBoxList.Add(mControlKeyTextBox12);

            mTechKeyTextBoxList.Add(mTechKeyTextBox1);
            mTechKeyTextBoxList.Add(mTechKeyTextBox2);
            mTechKeyTextBoxList.Add(mTechKeyTextBox3);
            mTechKeyTextBoxList.Add(mTechKeyTextBox4);
            mTechKeyTextBoxList.Add(mTechKeyTextBox5);
            mTechKeyTextBoxList.Add(mTechKeyTextBox6);
            mTechKeyTextBoxList.Add(mTechKeyTextBox7);
            mTechKeyTextBoxList.Add(mTechKeyTextBox8);
            mTechKeyTextBoxList.Add(mTechKeyTextBox9);
            mTechKeyTextBoxList.Add(mTechKeyTextBox10);
            mTechKeyTextBoxList.Add(mTechKeyTextBox11);
            mTechKeyTextBoxList.Add(mTechKeyTextBox12);

            mButtonTypeComboBoxList.Add(mButtonTypeComboBox1);
            mButtonTypeComboBoxList.Add(mButtonTypeComboBox2);
            mButtonTypeComboBoxList.Add(mButtonTypeComboBox3);
            mButtonTypeComboBoxList.Add(mButtonTypeComboBox4);
            mButtonTypeComboBoxList.Add(mButtonTypeComboBox5);
            mButtonTypeComboBoxList.Add(mButtonTypeComboBox6);
            mButtonTypeComboBoxList.Add(mButtonTypeComboBox7);
            mButtonTypeComboBoxList.Add(mButtonTypeComboBox8);
            mButtonTypeComboBoxList.Add(mButtonTypeComboBox9);
            mButtonTypeComboBoxList.Add(mButtonTypeComboBox10);
            mButtonTypeComboBoxList.Add(mButtonTypeComboBox11);
            mButtonTypeComboBoxList.Add(mButtonTypeComboBox12);

            mShiftCheckBoxList.Add(mShiftCheckBox1);
            mShiftCheckBoxList.Add(mShiftCheckBox2);
            mShiftCheckBoxList.Add(mShiftCheckBox3);
            mShiftCheckBoxList.Add(mShiftCheckBox4);
            mShiftCheckBoxList.Add(mShiftCheckBox5);
            mShiftCheckBoxList.Add(mShiftCheckBox6);
            mShiftCheckBoxList.Add(mShiftCheckBox7);
            mShiftCheckBoxList.Add(mShiftCheckBox8);
            mShiftCheckBoxList.Add(mShiftCheckBox9);
            mShiftCheckBoxList.Add(mShiftCheckBox10);
            mShiftCheckBoxList.Add(mShiftCheckBox11);
            mShiftCheckBoxList.Add(mShiftCheckBox12);

            mControlTypeComboBoxList.Add(mControlTypeComboBox1);
            mControlTypeComboBoxList.Add(mControlTypeComboBox2);
            mControlTypeComboBoxList.Add(mControlTypeComboBox3);
            mControlTypeComboBoxList.Add(mControlTypeComboBox4);
            mControlTypeComboBoxList.Add(mControlTypeComboBox5);
            mControlTypeComboBoxList.Add(mControlTypeComboBox6);
            mControlTypeComboBoxList.Add(mControlTypeComboBox7);
            mControlTypeComboBoxList.Add(mControlTypeComboBox8);
            mControlTypeComboBoxList.Add(mControlTypeComboBox9);
            mControlTypeComboBoxList.Add(mControlTypeComboBox10);
            mControlTypeComboBoxList.Add(mControlTypeComboBox11);
            mControlTypeComboBoxList.Add(mControlTypeComboBox12);

            mMemoTextBoxList.Add(mMemoTextBox1);
            mMemoTextBoxList.Add(mMemoTextBox2);
            mMemoTextBoxList.Add(mMemoTextBox3);
            mMemoTextBoxList.Add(mMemoTextBox4);
            mMemoTextBoxList.Add(mMemoTextBox5);
            mMemoTextBoxList.Add(mMemoTextBox6);
            mMemoTextBoxList.Add(mMemoTextBox7);
            mMemoTextBoxList.Add(mMemoTextBox8);
            mMemoTextBoxList.Add(mMemoTextBox9);
            mMemoTextBoxList.Add(mMemoTextBox10);
            mMemoTextBoxList.Add(mMemoTextBox11);
            mMemoTextBoxList.Add(mMemoTextBox12);

            mPotionNowKeyTextBoxList.Add(mPotionNowKeyTextBox1);
            mPotionNowKeyTextBoxList.Add(mPotionNowKeyTextBox2);
            mPotionNowKeyTextBoxList.Add(mPotionNowKeyTextBox3);
            mPotionNowKeyTextBoxList.Add(mPotionNowKeyTextBox4);

            mPotionReplaceKeyTextBoxList.Add(mPotionReplaceKeyTextBox1);
            mPotionReplaceKeyTextBoxList.Add(mPotionReplaceKeyTextBox2);
            mPotionReplaceKeyTextBoxList.Add(mPotionReplaceKeyTextBox3);
            mPotionReplaceKeyTextBoxList.Add(mPotionReplaceKeyTextBox4);

            mBuffKeyTextBoxList.Add(mBufKeyTextBox1);
            mBuffKeyTextBoxList.Add(mBufKeyTextBox2);
            mBuffKeyTextBoxList.Add(mBufKeyTextBox3);
            mBuffKeyTextBoxList.Add(mBufKeyTextBox4);
            mBuffKeyTextBoxList.Add(mBufKeyTextBox5);
            mBuffKeyTextBoxList.Add(mBufKeyTextBox6);
            mBuffKeyTextBoxList.Add(mBufKeyTextBox7);
            mBuffKeyTextBoxList.Add(mBufKeyTextBox8);
            mBuffKeyTextBoxList.Add(mBufKeyTextBox9);
            mBuffKeyTextBoxList.Add(mBufKeyTextBox10);

            mBuffCheckBoxList.Add(mBufCheckBox1);
            mBuffCheckBoxList.Add(mBufCheckBox2);
            mBuffCheckBoxList.Add(mBufCheckBox3);
            mBuffCheckBoxList.Add(mBufCheckBox4);
            mBuffCheckBoxList.Add(mBufCheckBox5);
            mBuffCheckBoxList.Add(mBufCheckBox6);
            mBuffCheckBoxList.Add(mBufCheckBox7);
            mBuffCheckBoxList.Add(mBufCheckBox8);
            mBuffCheckBoxList.Add(mBufCheckBox9);
            mBuffCheckBoxList.Add(mBufCheckBox10);
            mBuffCheckBoxList.Add(mBufCheckBox11);

            mBuffDisplayCheckBoxList.Add(mBufDisplayCheckBox1);
            mBuffDisplayCheckBoxList.Add(mBufDisplayCheckBox2);
            mBuffDisplayCheckBoxList.Add(mBufDisplayCheckBox3);
            mBuffDisplayCheckBoxList.Add(mBufDisplayCheckBox4);
            mBuffDisplayCheckBoxList.Add(mBufDisplayCheckBox5);
            mBuffDisplayCheckBoxList.Add(mBufDisplayCheckBox6);
            mBuffDisplayCheckBoxList.Add(mBufDisplayCheckBox7);
            mBuffDisplayCheckBoxList.Add(mBufDisplayCheckBox8);
            mBuffDisplayCheckBoxList.Add(mBufDisplayCheckBox9);
            mBuffDisplayCheckBoxList.Add(mBufDisplayCheckBox10);

            mBuffDisplayButtonList.Add(mBufDisplayButton1);
            mBuffDisplayButtonList.Add(mBufDisplayButton2);
            mBuffDisplayButtonList.Add(mBufDisplayButton3);
            mBuffDisplayButtonList.Add(mBufDisplayButton4);
            mBuffDisplayButtonList.Add(mBufDisplayButton5);
            mBuffDisplayButtonList.Add(mBufDisplayButton6);
            mBuffDisplayButtonList.Add(mBufDisplayButton7);
            mBuffDisplayButtonList.Add(mBufDisplayButton8);
            mBuffDisplayButtonList.Add(mBufDisplayButton9);
            mBuffDisplayButtonList.Add(mBufDisplayButton10);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            HookManager.getInstance().setKeyHookCallback(onHookHandler);
            HookManager.getInstance().setMouseHookCallback(onHookHandler);

            if (HookManager.getInstance().startHook() == false)
            {
                this.BeginInvoke(new Action(delegate ()
                {
                    string captionString = "AJeHelperD2R";
                    MessageBox.Show(this, StringLib.Message1, captionString);
                    this.Close();
                }));
            }
        }

        private void onClosing(object sender, FormClosingEventArgs e)
        {
            if (mOriginControlData.isChangeData(mControlData) == true)
            {
                string captionString = "AJeHelperD2R";
                var result = MessageBox.Show(this, StringLib.Message2, captionString, MessageBoxButtons.OKCancel);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            this.startAndStopControl(true);

            HookManager.getInstance().unKeyHookCallback();
            HookManager.getInstance().unMouseHookCallback();
            HookManager.getInstance().stopHook();

            mControlTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            mControlTimer.Dispose();

            for (int i = 0; i < mBufDisplayTimerList.Length; i++)
            {
                lock (mBufDisplayTimerLockList[i])
                {
                    mBufDisplayTimerList[i].Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    mBufDisplayTimerList[i].Dispose();
                }
            }

            mMercenaryTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            mMercenaryTimer.Dispose();
        }

        private void onTextBoxGotFocus(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            textBox.Text = "";
            mIsSetKey = true;
            mWParam = HookManager.NONE;
            mKeyCode = (int)Keys.None;
            this.debugPrintln("onTextBoxGotFocus()");
        }

        private void onTextBoxLostFocus(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            if (mWParam == HookManager.NONE && mKeyCode == (int)Keys.None)
            {
                if (mShiftKeyTextBox == textBox)
                {
                    textBox.Text = "Shift";
                }
                else
                {
                    textBox.Text = "";
                }
            }
            mWParam = HookManager.NONE;
            mKeyCode = (int)Keys.None;
            mIsSetKey = false;

            this.debugPrintln("onTextBoxLostFocus()");
        }

        private void startAndStopControl(bool haveToStop = false)
        {
            lock (mLock)
            {
                if (mIsStart == false && haveToStop == false)
                {
                    if (mControlData.ProgramName.Length > 0)
                    {
                        string nameString = HookManager.getCaptionOfActiveWindow();
                        if (nameString.CompareTo(mControlData.ProgramName) != 0)
                            return;
                    }

                    this.debugPrintln("startAndStopControl() : start");
                    mIsStart = true;
                    mIsPause = false;
                    mIsBuff = false;
                    foreach (var temp in mControlList)
                    {
                        temp.Enabled = false;
                    }

                    // 버프 표시
                    for (int i = 0; i < mControlData.BuffKeyData.BufRectDataList.Count; i++)
                    {
                        var form = new BufForm(mControlData.BuffKeyData.BufRectDataList[i].BuffFontSize,
                                                mControlData.BuffKeyData.BufRectDataList[i].BuffHeightSize,
                                                mControlData.BuffKeyData.BufRectDataList[i].EffectFile,
                                                mControlData.BuffKeyData.BufRectDataList[i].EffectVolume);
                        mBufFormList[i] = form;

                        if (mControlData.BuffKeyData.BufRectDataList[i].Display == true)
                        {
                            form.setImage(mControlData.BuffKeyData.BufRectDataList[i].ImageFile);
                            form.Show();

                            form.Width = mControlData.BuffKeyData.BufRectDataList[i].Width;
                            form.Height = mControlData.BuffKeyData.BufRectDataList[i].Height + mControlData.BuffKeyData.BufRectDataList[i].BuffHeightSize;
                            form.Location = new Point(mControlData.BuffKeyData.BufRectDataList[i].X, mControlData.BuffKeyData.BufRectDataList[i].Y - mControlData.BuffKeyData.BufRectDataList[i].BuffHeightSize);
                        }
                    }

                    // 용병 표시
                    if (mControlData.MerceRectData.Display == true)
                    {
                        mMercenaryCaptureForm = new MercenaryRectForm((int)MercenaryRectForm.MERCENARY_TYPE.CAPTURE_OVERLAY);
                        mMercenaryCaptureForm.Show();
                        mMercenaryCaptureForm.Width = mControlData.MerceRectData.CaptureWidth;
                        mMercenaryCaptureForm.Height = mControlData.MerceRectData.CaptureHeight;
                        mMercenaryCaptureForm.Location = new Point(mControlData.MerceRectData.CapturePointX, mControlData.MerceRectData.CapturePointY);

                        mMercenaryDisplayForm = new MercenaryRectForm((int)MercenaryRectForm.MERCENARY_TYPE.DISPLAY_OVERLAY);
                        mMercenaryDisplayForm.Show();
                        mMercenaryDisplayForm.Width = mControlData.MerceRectData.DisplayWidth;
                        mMercenaryDisplayForm.Height = mControlData.MerceRectData.DisplayHeight;
                        mMercenaryDisplayForm.Location = new Point(mControlData.MerceRectData.DisplayPointX, mControlData.MerceRectData.DisplayPointY);

                        mMercenaryDisplayForm.Opacity = ((double)mControlData.MerceRectData.Opacity / 100.0f);

                        mMercenaryTimer.Change(100, mControlData.MerceRectData.Delay);
                    }
                }
                else
                {
                    this.debugPrintln("startAndStopControl() : stop");
                    mIsStart = false;
                    mIsPause = false;
                    mIsBuff = false;

                    try
                    {
                        Monitor.Enter(mControlTimerLock);

                        mControlTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                        mNowControlKeyData = null;
                        mIsPause = false;
                    }
                    finally
                    {
                        Monitor.Exit(mControlTimerLock);
                    }

                    // 버프 표시 타이머
                    for (int i = 0; i < mControlData.BuffKeyData.BufRectDataList.Count; i++)
                    {
                        lock (mBufDisplayTimerLockList[i])
                        {
                            mBufDisplayTimerList[i].Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                            if (mBufFormList[i] != null)
                            {
                                mBufFormList[i].Close();
                                mBufFormList[i].Dispose();
                                mBufFormList[i] = null;
                            }
                        }
                    }

                    // 용병 표시 타이머                    
                    try
                    {
                        Monitor.Enter(mMercenaryTimerLock);
                        mMercenaryTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                        if (mMercenaryCaptureForm != null)
                        {
                            mMercenaryCaptureForm.Close();
                            mMercenaryCaptureForm.Dispose();
                            mMercenaryCaptureForm = null;
                        }
                        if (mMercenaryDisplayForm != null)
                        {
                            mMercenaryDisplayForm.Close();
                            mMercenaryDisplayForm.Dispose();
                            mMercenaryDisplayForm = null;
                        }
                    }
                    finally
                    {
                        Monitor.Exit(mMercenaryTimerLock);
                    }

                    bool isShiftDown = mIsShiftDown;
                    bool isLeftDown = mIsLeftDown;
                    bool isRightDown = mIsRightDown;

                    mIsShiftDown = false;
                    mIsLeftDown = false;
                    mIsRightDown = false;

                    new Task(delegate
                    {
                        if (isShiftDown == true)
                        {
                            Thread.Sleep(10);
                            this.keyMouseEvent(mControlData.ShiftKeyData.WParam, mControlData.ShiftKeyData.KeyCode, false);
                        }

                        if (isLeftDown == true)
                        {
                            Thread.Sleep(10);
                            this.keyMouseEvent(HookManager.WM_LBUTTONDOWN, 0, false);
                        }

                        if (isRightDown == true)
                        {
                            Thread.Sleep(10);
                            this.keyMouseEvent(HookManager.WM_RBUTTONDOWN, 0, false);
                        }
                    }).Start();

                    foreach (var temp in mControlList)
                    {
                        temp.Enabled = true;
                    }

                    for (int i = 0; i < mBuffDisplayButtonList.Count; i++)
                    {
                        mBuffDisplayButtonList[i].Enabled = mBuffDisplayCheckBoxList[i].Checked;
                    }                    

                    this.setStateLabel(true, false, false, 0);
                }
                
            }
        }

        private void bufDisplayTimerCallback(int index)
        {
            lock (mBufDisplayTimerLockList[index])
            {
                if (mControlData.BuffKeyData.BufRectDataList[index].Display == false)
                    return;

                if (mBufFormList[index] == null)
                    return;

                mBufFormList[index].RemainTime = mBufFormList[index].RemainTime - 1;
                if (mBufFormList[index].RemainTime <= 0)
                {
                    mBufFormList[index].RemainTime = 0;
                }

                this.BeginInvoke(new Action(delegate ()
                {
                    lock (mBufDisplayTimerLockList[index])
                    {
                        if (mBufFormList[index] != null)
                            mBufFormList[index].redrawForm();
                    }
                }));
            }
        }

        private void mercenaryDisplayTimerCallback(object state)
        {
            if (Monitor.TryEnter(mMercenaryTimerLock) == false)
                return;

            try
            {
                if (mMercenaryCaptureForm == null || mMercenaryDisplayForm == null)
                    return;

                //size 객체 생성  
                Size size = new Size(mMercenaryCaptureForm.Width - 2, mMercenaryCaptureForm.Height - 2);

                //Bitmap 객체 생성   
                Bitmap bitmap = new Bitmap(mMercenaryCaptureForm.Width - 2, mMercenaryCaptureForm.Height - 2);

                //Graphics 객체 생성   
                Graphics graphics = Graphics.FromImage(bitmap);

                // Graphics 객체의 CopyFromScreen()메서드로 bitmap 객체에 Screen을 캡처하여 저장   
                graphics.CopyFromScreen(mMercenaryCaptureForm.Location.X + 1, mMercenaryCaptureForm.Location.Y + 1, 0, 0, size);

                this.BeginInvoke(new Action(delegate ()
                {                   
                    try
                    {
                        Monitor.Enter(mMercenaryTimerLock);

                        if (mMercenaryDisplayForm != null)
                            mMercenaryDisplayForm.setImage(bitmap);
                    }
                    finally
                    {
                        Monitor.Exit(mMercenaryTimerLock);
                    }
                }));
            }
            finally
            {
                Monitor.Exit(mMercenaryTimerLock);
            }
        }

        private int controlButtonDown(int wParam, int keyCode)
        {
            this.debugPrintln("controlButtonDown()");

            if (mControlData.ProgramName.Length > 0)
            {
                string nameString = HookManager.getCaptionOfActiveWindow();
                if (nameString.CompareTo(mControlData.ProgramName) != 0)
                    return 1;
            }

            try
            {
                Monitor.Enter(mControlTimerLock);

                // 버프 중이면 실행 안함
                if (mIsBuff == true)
                {
                    return 1;
                }

                bool isReturn = false;
                if (mNowControlKeyData != null)
                {
                    // 이미 동작 중인 키와 같으면
                    if (mNowControlKeyData.WParam == wParam && mNowControlKeyData.KeyCode == keyCode)
                    {
                        // 반복(홀딩), 누르고 있기(홀딩) 일 경우 리턴
                        if (mNowControlKeyData.ControlType == (int)ControlKeyData.CONTROL_TYPE.REPEAT_HOLDING ||
                            mNowControlKeyData.ControlType == (int)ControlKeyData.CONTROL_TYPE.HOLDING_HOLDING)
                        {
                            this.debugPrintln("controlButtonDown() : return(" + mNowControlKeyData.ControlType + ")");
                            return 0;
                        }

                        isReturn = true;
                    }

                    // 이미 동작 중인 프로세스가 있으면 중지
                    mControlTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                    mNowControlKeyData = null;
                    this.setStateLabel(true, false, false, 0);
                }

                bool isShiftDown = mIsShiftDown;
                bool isLeftDown = mIsLeftDown;
                bool isRightDown = mIsRightDown;

                mIsShiftDown = false;
                mIsLeftDown = false;
                mIsRightDown = false;

                new Task(delegate
                {
                    if (isShiftDown == true)
                    {
                        Thread.Sleep(10);
                        this.debugPrintln("controlButtonDown() : Shift up");
                        this.keyMouseEvent(mControlData.ShiftKeyData.WParam, mControlData.ShiftKeyData.KeyCode, false);
                        Thread.Sleep(10);
                    }

                    if (isLeftDown == true)
                    {
                        Thread.Sleep(10);
                        this.debugPrintln("controlButtonDown() : Left up");
                        this.keyMouseEvent(HookManager.WM_LBUTTONDOWN, 0, false);
                    }

                    if (isRightDown == true)
                    {
                        Thread.Sleep(10);
                        this.debugPrintln("controlButtonDown() : Right up");
                        this.keyMouseEvent(HookManager.WM_RBUTTONDOWN, 0, false);
                    }
                }).Start();

                if (isReturn == true)
                {
                    return 1;
                }

                if (wParam == HookManager.WM_KEYDOWN)
                {
                    if (mControlKeyDictionary.ContainsKey(keyCode) == false)
                    {
                        return 1;
                    }
                    mNowControlKeyData = mControlKeyDictionary[keyCode];
                }
                else
                {
                    if (mControlMouseDictionary.ContainsKey(wParam) == false)
                    {
                        return 1;
                    }
                    mNowControlKeyData = mControlMouseDictionary[wParam];
                }

                this.setStateLabel(false, false, false, mNowControlKeyData.Index);

                this.debugPrintln("controlButtonDown() : " + mNowControlKeyData.ControlType);

                // 반복
                if (mNowControlKeyData.ControlType == (int)ControlKeyData.CONTROL_TYPE.REPEAT_TOGGLE ||
                    mNowControlKeyData.ControlType == (int)ControlKeyData.CONTROL_TYPE.REPEAT_HOLDING)
                {
                    // 기술키 실행
                    if (mNowControlKeyData.WParamTech != HookManager.NONE)
                    {
                        int wParamTech = mNowControlKeyData.WParamTech;
                        int keyCodeTech = mNowControlKeyData.KeyCodeTech;
                        int controlRepeatTime = mControlData.ControlRepeatTime;

                        new Task(delegate
                        {
                            Thread.Sleep(10);

                            this.keyMouseEvent(wParamTech, keyCodeTech, true);
                            Thread.Sleep(1);
                            this.keyMouseEvent(wParamTech, keyCodeTech, false);

                            Thread.Sleep(10);

                            try
                            {
                                Monitor.Enter(mControlTimerLock);
                                mControlTimer.Change(10, controlRepeatTime);
                            }
                            finally
                            {
                                Monitor.Exit(mControlTimerLock);
                            }
                        }).Start();
                    }
                    else
                    {
                        mControlTimer.Change(10, mControlData.ControlRepeatTime);
                    }
                }

                // 누르고 있기
                else if (mNowControlKeyData.ControlType == (int)ControlKeyData.CONTROL_TYPE.HOLDING_TOGGLE ||
                           mNowControlKeyData.ControlType == (int)ControlKeyData.CONTROL_TYPE.HOLDING_HOLDING)
                {
                    // 기술키 실행
                    if (mNowControlKeyData.WParamTech != HookManager.NONE)
                    {
                        int wParamTech = mNowControlKeyData.WParamTech;
                        int keyCodeTech = mNowControlKeyData.KeyCodeTech;
                        int controlRepeatTime = 100;

                        new Task(delegate
                        {
                            Thread.Sleep(10);

                            this.keyMouseEvent(wParamTech, keyCodeTech, true);
                            Thread.Sleep(1);
                            this.keyMouseEvent(wParamTech, keyCodeTech, false);

                            Thread.Sleep(10);

                            try
                            {
                                Monitor.Enter(mControlTimerLock);
                                mControlTimer.Change(100, controlRepeatTime);
                            }
                            finally
                            {
                                Monitor.Exit(mControlTimerLock);
                            }
                        }).Start();
                    }
                    else
                    {
                        mControlTimer.Change(100, 100);
                    }
                }

                // 한번만 실행
                else if (mNowControlKeyData.ControlType == (int)ControlKeyData.CONTROL_TYPE.ONE)
                {
                    int mouseButton = (mNowControlKeyData.ButtonType == (int)ControlKeyData.BUTTON_TYPE.LEFT) ? HookManager.WM_LBUTTONDOWN : HookManager.WM_RBUTTONDOWN;
                    bool isShift = mNowControlKeyData.IsShift;
                    int wParamTech = mNowControlKeyData.WParamTech;
                    int keyCodeTech = mNowControlKeyData.KeyCodeTech;

                    new Task(delegate
                    {
                        Thread.Sleep(10);

                        // 기술키
                        if (wParamTech != HookManager.NONE)
                        {                            
                            this.keyMouseEvent(wParamTech, keyCodeTech, true);
                            Thread.Sleep(1);
                            this.keyMouseEvent(wParamTech, keyCodeTech, false);
                        }

                        // 시프트
                        if (isShift == true)
                        {
                            Thread.Sleep(10);
                            this.keyMouseEvent(mControlData.ShiftKeyData.WParam, mControlData.ShiftKeyData.KeyCode, true);
                            Thread.Sleep(10);
                        }

                        // Left or Right
                        Thread.Sleep(10);
                        this.keyMouseEvent(mouseButton, 0, true);
                        Thread.Sleep(1);
                        this.keyMouseEvent(mouseButton, 0, false);

                        // 시프트
                        if (isShift == true)
                        {
                            Thread.Sleep(10);
                            this.keyMouseEvent(mControlData.ShiftKeyData.WParam, mControlData.ShiftKeyData.KeyCode, false);                            
                        }
                    }).Start();

                    mNowControlKeyData = null;

                    this.debugPrintln("controlButtonDown() : ONE");
                }
            }
            finally
            {
                Monitor.Exit(mControlTimerLock);
            }
            return 1;
        }

        private int controlButtonUp(int wParam, int keyCode)
        {
            this.debugPrintln("controlButtonUp()");

            if (mControlData.ProgramName.Length > 0)
            {
                string nameString = HookManager.getCaptionOfActiveWindow();
                if (nameString.CompareTo(mControlData.ProgramName) != 0)
                    return 1;
            }

            try
            {
                Monitor.Enter(mControlTimerLock);

                if (mNowControlKeyData == null)
                    return 1;

                // 동작 중인 키와 다르면 리턴
                if (mNowControlKeyData.WParam != wParam || mNowControlKeyData.KeyCode != keyCode)
                {
                    return 1;
                }

                // 반복(토글), 누르고 있기(토글) 일 경우 리턴
                if (mNowControlKeyData.ControlType == (int)ControlKeyData.CONTROL_TYPE.REPEAT_TOGGLE ||
                    mNowControlKeyData.ControlType == (int)ControlKeyData.CONTROL_TYPE.HOLDING_TOGGLE)
                {
                    return 1;
                }

                this.debugPrintln("controlButtonUp() : " + mNowControlKeyData.ControlType);

                // 이미 동작 중인 프로세스가 있으면 중지
                mControlTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                mNowControlKeyData = null;
                this.setStateLabel(true, false, false, 0);

                bool isShiftDown = mIsShiftDown;
                bool isLeftDown = mIsLeftDown;
                bool isRightDown = mIsRightDown;

                mIsShiftDown = false;
                mIsLeftDown = false;
                mIsRightDown = false;

                new Task(delegate
                {
                    if (isShiftDown == true)
                    {
                        Thread.Sleep(10);
                        this.debugPrintln("controlButtonUp() : Shift up");
                        this.keyMouseEvent(mControlData.ShiftKeyData.WParam, mControlData.ShiftKeyData.KeyCode, false);
                        Thread.Sleep(10);
                    }

                    if (isLeftDown == true)
                    {
                        Thread.Sleep(10);
                        this.debugPrintln("controlButtonUp() : Left up");
                        this.keyMouseEvent(HookManager.WM_LBUTTONDOWN, 0, false);
                    }

                    if (isRightDown == true)
                    {
                        Thread.Sleep(10);
                        this.debugPrintln("controlButtonUp() : Right up");
                        this.keyMouseEvent(HookManager.WM_RBUTTONDOWN, 0, false);
                    }
                }).Start();
            }
            finally
            {
                Monitor.Exit(mControlTimerLock);
            }

            return 1;
        }

        private void controlButtonProcess(object sender)
        {
            if (mIsStart == false)
                return;

            if (mControlData.ProgramName.Length > 0)
            {
                string nameString = HookManager.getCaptionOfActiveWindow();
                if (nameString.CompareTo(mControlData.ProgramName) != 0)
                {
                    return;
                }
            }

            if (Monitor.TryEnter(mControlTimerLock) == false)
                return;

            try
            {
                if (mNowControlKeyData == null)
                {
                    return;
                }

                // 잠시 멈춤일 경우
                if (mIsPause == true)
                {
                    bool isShiftDown = mIsShiftDown;
                    bool isLeftDown = mIsLeftDown;
                    bool isRightDown = mIsRightDown;

                    mIsShiftDown = false;
                    mIsLeftDown = false;
                    mIsRightDown = false;

                    int pauseWParam = mPauseWParam;
                    int pauseKeyCode = mPauseKeyCode;

                    new Task(delegate
                    {
                        if (isShiftDown == true && pauseKeyCode != (int)Keys.LShiftKey)
                        {
                            Thread.Sleep(10);
                            this.debugPrintln("controlButtonProcess() : Shift up");
                            this.keyMouseEvent(mControlData.ShiftKeyData.WParam, mControlData.ShiftKeyData.KeyCode, false);
                            Thread.Sleep(10);
                        }

                        if (isLeftDown == true && pauseWParam != HookManager.WM_LBUTTONDOWN)
                        {
                            Thread.Sleep(10);
                            this.debugPrintln("controlButtonProcess() : Left up");
                            this.keyMouseEvent(HookManager.WM_LBUTTONDOWN, 0, false);
                        }

                        if (isRightDown == true && pauseWParam != HookManager.WM_RBUTTONDOWN)
                        {
                            Thread.Sleep(10);
                            this.debugPrintln("controlButtonProcess() : Right up");
                            this.keyMouseEvent(HookManager.WM_RBUTTONDOWN, 0, false);
                        }
                    }).Start();
                    return;
                }

                // 반복
                if (mNowControlKeyData.ControlType == (int)ControlKeyData.CONTROL_TYPE.REPEAT_TOGGLE ||
                    mNowControlKeyData.ControlType == (int)ControlKeyData.CONTROL_TYPE.REPEAT_HOLDING)
                {
                    this.debugPrintln("controlButtonProcess() : repeat");
                    bool isShiftDown = (mIsShiftDown == false && mNowControlKeyData.IsShift == true);
                    if (isShiftDown == true)
                    {
                        mIsShiftDown = true;
                    }

                    int mouseButton = (mNowControlKeyData.ButtonType == (int)ControlKeyData.BUTTON_TYPE.LEFT) ? HookManager.WM_LBUTTONDOWN : HookManager.WM_RBUTTONDOWN;

                    new Task(delegate
                    {
                        if (isShiftDown == true)
                        {
                            this.debugPrintln("controlButtonProcess() : Shift down");
                            this.keyMouseEvent(mControlData.ShiftKeyData.WParam, mControlData.ShiftKeyData.KeyCode, true);
                            Thread.Sleep(10);
                        }

                        this.keyMouseEvent(mouseButton, 0, true);
                        Thread.Sleep(1);
                        this.keyMouseEvent(mouseButton, 0, false);
                    }).Start();
                }

                // 누르고 있기
                else if (mNowControlKeyData.ControlType == (int)ControlKeyData.CONTROL_TYPE.HOLDING_TOGGLE ||
                        mNowControlKeyData.ControlType == (int)ControlKeyData.CONTROL_TYPE.HOLDING_HOLDING)
                {
                    bool isShiftDown = (mIsShiftDown == false && mNowControlKeyData.IsShift == true);
                    if (isShiftDown == true)
                    {
                        mIsShiftDown = true;
                    }

                    bool isLeft = (mNowControlKeyData.ButtonType == (int)ControlKeyData.BUTTON_TYPE.LEFT && mIsLeftDown == false);
                    if (isLeft == true)
                    {
                        mIsLeftDown = true;
                    }

                    bool isRight = (mNowControlKeyData.ButtonType == (int)ControlKeyData.BUTTON_TYPE.RIGHT && mIsRightDown == false);
                    if (isRight == true)
                    {
                        mIsRightDown = true;
                    }

                    new Task(delegate
                    {
                        if (isShiftDown == true)
                        {
                            this.debugPrintln("controlButtonProcess() : Shift down");
                            this.keyMouseEvent(mControlData.ShiftKeyData.WParam, mControlData.ShiftKeyData.KeyCode, true);
                            Thread.Sleep(10);
                        }

                        // 마우스 좌버튼
                        if (isLeft == true)
                        {
                            this.debugPrintln("controlButtonProcess() : Left down");
                            this.keyMouseEvent(HookManager.WM_LBUTTONDOWN, 0, true);
                        }

                        // 마우스 우버튼                    
                        else if (isRight == true)
                        {
                            this.debugPrintln("controlButtonProcess() : Right down");
                            mIsRightDown = true;
                            this.keyMouseEvent(HookManager.WM_RBUTTONDOWN, 0, true);
                        }
                    }).Start();
                }
            }
            finally
            {
                Monitor.Exit(mControlTimerLock);
            }
        }

        private void pauseButtonDown(int wParam, int keyCode)
        {
            if (mIsStart == false)
                return;

            if (mControlData.ProgramName.Length > 0)
            {
                string nameString = HookManager.getCaptionOfActiveWindow();
                if (nameString.CompareTo(mControlData.ProgramName) != 0)
                {
                    return;
                }
            }

            try
            {
                Monitor.Enter(mControlTimerLock);

                mIsPause = true;
                this.debugPrintln("pauseButtonDown()");

                if (mNowControlKeyData != null)
                {
                    mPauseWParam = wParam;
                    mPauseKeyCode = keyCode;

                    this.setStateLabel(false, true, false, 0);
                }
            }
            finally
            {
                Monitor.Exit(mControlTimerLock);
            }
        }

        private void pauseButtonUp()
        {
            if (mIsStart == false)
                return;

            try
            {
                Monitor.Enter(mControlTimerLock);

                mIsPause = false;
                this.debugPrintln("pauseButtonUp()");

                mPauseWParam = HookManager.NONE;
                mPauseKeyCode = (int)Keys.None;

                if (mNowControlKeyData != null)
                {
                    this.setStateLabel(false, false, false, mNowControlKeyData.Index);
                }
            }
            finally
            {
                Monitor.Exit(mControlTimerLock);
            }
        }

        private int potionButtonDown(int wParam, int keyCode)
        {
            if (mIsStart == false)
                return 1;

            if (mControlData.ProgramName.Length > 0)
            {
                string nameString = HookManager.getCaptionOfActiveWindow();
                if (nameString.CompareTo(mControlData.ProgramName) != 0)
                {
                    return 1;
                }
            }

            try
            {
                Monitor.Enter(mControlTimerLock);

                long nowTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                if (mLastPotionTime == 0 || nowTime - mLastPotionTime >= 1000)
                {
                    mLastPotionTime = nowTime;

                    PotionKeyData potionKeyData = null;
                    if (wParam == HookManager.WM_KEYDOWN)
                    {
                        if (mPotionKeyDictionary.ContainsKey(keyCode) == false)
                        {
                            return 1;
                        }
                        potionKeyData = mPotionKeyDictionary[keyCode];
                    }
                    else
                    {
                        if (mPotionMouseDictionary.ContainsKey(wParam) == false)
                        {
                            return 1;
                        }
                        potionKeyData = mPotionMouseDictionary[wParam];
                    }

                    this.debugPrintln("posionButtonDown() : WParam(" + potionKeyData.WParam + "), KeyCode(" + potionKeyData.KeyCode + ")");
                    this.debugPrintln("posionButtonDown() : WParamNow(" + potionKeyData.WParamNow + "), KeyCodeNow(" + potionKeyData.KeyCodeNow + ")");

                    bool isShiftDown = false;
                    if (mControlData.ShiftKeyData.WParam == HookManager.WM_KEYDOWN && mControlData.ShiftKeyData.KeyCode == (int)Keys.LShiftKey)
                    {
                        isShiftDown = mIsShiftDown;
                    }

                    new Task(delegate
                    {
                        Thread.Sleep(100);

                        if (isShiftDown == false)
                        {
                            this.keyMouseEvent(HookManager.WM_KEYDOWN, (int)Keys.LShiftKey, true);
                            Thread.Sleep(10);
                        }

                        this.keyMouseEvent(potionKeyData.WParamNow, potionKeyData.KeyCodeNow, true);
                        Thread.Sleep(1);
                        this.keyMouseEvent(potionKeyData.WParamNow, potionKeyData.KeyCodeNow, false);

                        if (isShiftDown == false)
                        {
                            Thread.Sleep(10);
                            this.keyMouseEvent(HookManager.WM_KEYDOWN, (int)Keys.LShiftKey, false);
                        }
                    }).Start();
                }
            }
            finally
            {
                Monitor.Exit(mControlTimerLock);
            }
            return 1;
        }

        private void buffButtonDown()
        {
            this.debugPrintln("buffButtonDown()");

            if (mControlData.ProgramName.Length > 0)
            {
                string nameString = HookManager.getCaptionOfActiveWindow();
                if (nameString.CompareTo(mControlData.ProgramName) != 0)
                    return;
            }

            try
            {
                Monitor.Enter(mControlTimerLock);

                // 이미 버프 중이면 패스
                if (mIsBuff == true)
                {
                    return;
                }

                if (mNowControlKeyData != null)
                {
                    // 이미 동작 중인 프로세스가 있으면 중지
                    mControlTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

                    mNowControlKeyData = null;
                }

                mIsBuff = true;
                this.setStateLabel(false, false, true, 0);

                bool isLeftDown = mIsLeftDown;
                bool isRightDown = mIsRightDown;
                bool isShiftDown = mIsShiftDown;

                mIsLeftDown = false;
                mIsRightDown = false;
                mIsShiftDown = false;

                new Task(delegate
                {
                    if (isLeftDown == true)
                    {
                        Thread.Sleep(10);
                        this.debugPrintln("buffButtonDown() : LMouse up");
                        this.keyMouseEvent(HookManager.WM_LBUTTONDOWN, 0, false);
                    }
                    if (isRightDown == true)
                    {
                        Thread.Sleep(10);
                        this.debugPrintln("buffButtonDown() : RMouse up");
                        this.keyMouseEvent(HookManager.WM_RBUTTONDOWN, 0, false);
                    }
                    if (isShiftDown == true)
                    {
                        Thread.Sleep(10);
                        this.debugPrintln("buffButtonDown() : LShiftKey up");
                        this.keyMouseEvent(mControlData.ShiftKeyData.WParam, mControlData.ShiftKeyData.KeyCode, false);
                    }

                    // 버프 키
                    bool isPrevBuf = false;
                    for (int i = 0; i < mControlData.BuffKeyData.WParamBufList.Count; i++)
                    {
                        // 무기교체
                        if (mControlData.BuffKeyData.ChangeCheckList[i] == true)
                        {
                            if (mControlData.BuffKeyData.WParamWeapon == HookManager.NONE) { }
                            else
                            {
                                if (isPrevBuf == true)
                                {
                                    Thread.Sleep(100);
                                }

                                Thread.Sleep(10);
                                this.keyMouseEvent(mControlData.BuffKeyData.WParamWeapon, mControlData.BuffKeyData.KeyCodeWeapon, true);
                                Thread.Sleep(1);
                                this.keyMouseEvent(mControlData.BuffKeyData.WParamWeapon, mControlData.BuffKeyData.KeyCodeWeapon, false);
                                Thread.Sleep(100);
                            }
                        }

                        isPrevBuf = false;

                        // 버프
                        if (mControlData.BuffKeyData.WParamBufList[i] == HookManager.NONE) { }
                        else
                        {
                            Thread.Sleep(10);
                            this.keyMouseEvent(mControlData.BuffKeyData.WParamBufList[i], mControlData.BuffKeyData.KeyCodeBufList[i], true);
                            Thread.Sleep(1);
                            this.keyMouseEvent(mControlData.BuffKeyData.WParamBufList[i], mControlData.BuffKeyData.KeyCodeBufList[i], false);
                            Thread.Sleep(100);

                            this.keyMouseEvent(HookManager.WM_RBUTTONDOWN, 0, true);
                            Thread.Sleep(1);
                            this.keyMouseEvent(HookManager.WM_RBUTTONDOWN, 0, false);

                            // 버프 표시 시간 세팅
                            lock (mBufDisplayTimerLockList[i])
                            {
                                if (mControlData.BuffKeyData.BufRectDataList[i].Display == true)
                                {
                                    if (mBufFormList[i] != null)
                                    {
                                        mBufFormList[i].RemainTime = mControlData.BuffKeyData.BufRectDataList[i].BuffTime;                                  
                                        mBufDisplayTimerList[i].Change(1000, 1000);

                                        int index = i;
                                        this.BeginInvoke(new Action(delegate ()
                                        {
                                            lock (mBufDisplayTimerLockList[index])
                                            {
                                                if (mBufFormList[index] != null)
                                                {
                                                    mBufFormList[index].redrawForm();
                                                }
                                            }
                                        }));
                                    }
                                }
                            }

                            Thread.Sleep(mControlData.BuffKeyData.DelayTime);

                            isPrevBuf = true;
                        }
                    }

                    // 무기교체
                    if (mControlData.BuffKeyData.ChangeCheckList[mControlData.BuffKeyData.ChangeCheckList.Count - 1] == true)
                    {
                        if (mControlData.BuffKeyData.WParamWeapon == HookManager.NONE) { }
                        else
                        {
                            if (isPrevBuf == true)
                            {
                                Thread.Sleep(100);
                            }

                            Thread.Sleep(10);
                            this.keyMouseEvent(mControlData.BuffKeyData.WParamWeapon, mControlData.BuffKeyData.KeyCodeWeapon, true);
                            Thread.Sleep(1);
                            this.keyMouseEvent(mControlData.BuffKeyData.WParamWeapon, mControlData.BuffKeyData.KeyCodeWeapon, false);
                        }
                    }

                    try
                    {
                        Monitor.Enter(mControlTimerLock);
                        mIsBuff = false;
                        this.setStateLabel(true, false, false, 0);
                    }
                    finally
                    {
                        Monitor.Exit(mControlTimerLock);
                    }
                }).Start();
            }
            finally
            {
                Monitor.Exit(mControlTimerLock);
            }
        }

        private void setControlData(TextBox textbox)
        {
            string captionString = "AJeHelperD2R";
            for (int i = 0; i < mStartTextBoxList.Count; i++)
            {
                if (mStartTextBoxList[i] == textbox)
                {
                    if (mWParam == HookManager.NONE && mKeyCode == (int)Keys.None)
                    {
                        mControlData.StartKeyDataList[i].WParam = mWParam;
                        mControlData.StartKeyDataList[i].KeyCode = mKeyCode;
                        this.setDictionaryData();
                        return;
                    }

                    else if (mWParam == HookManager.WM_LBUTTONDOWN)
                    {
                        mControlData.StartKeyDataList[i].WParam = HookManager.NONE;
                        mControlData.StartKeyDataList[i].KeyCode = (int)Keys.None;
                        this.setDictionaryData();
                        MessageBox.Show(this, StringLib.Message3, captionString);
                        return;
                    }

                    else if (mWParam == HookManager.WM_RBUTTONDOWN)
                    {
                        mControlData.StartKeyDataList[i].WParam = HookManager.NONE;
                        mControlData.StartKeyDataList[i].KeyCode = (int)Keys.None;
                        this.setDictionaryData();
                        MessageBox.Show(this, StringLib.Message4, captionString);
                        return;
                    }

                    else if (this.isPauseKey(mWParam, mKeyCode, -1) == true ||
                            this.isControlKey(mWParam, mKeyCode, -1) == true ||
                            this.isShiftKey(mWParam, mKeyCode) == true ||
                            this.isBuffKey(mWParam, mKeyCode) == true ||
                            this.isPotionKey(mWParam, mKeyCode, -1) == true)
                    {
                        mControlData.StartKeyDataList[i].WParam = HookManager.NONE;
                        mControlData.StartKeyDataList[i].KeyCode = (int)Keys.None;
                        this.setDictionaryData();
                        MessageBox.Show(this, StringLib.Message5, captionString);
                        return;
                    }

                    mControlData.StartKeyDataList[i].WParam = mWParam;
                    mControlData.StartKeyDataList[i].KeyCode = mKeyCode;
                    this.setDictionaryData();
                    return;
                }
            }

            for (int i = 0; i < mPauseTextBoxList.Count; i++)
            {
                if (mPauseTextBoxList[i] == textbox)
                {
                    if (mWParam == HookManager.NONE && mKeyCode == (int)Keys.None)
                    {
                        mControlData.PauseKeyDataList[i].WParam = mWParam;
                        mControlData.PauseKeyDataList[i].KeyCode = mKeyCode;
                        this.setDictionaryData();
                        return;
                    }

                    else if (this.isStartKey(mWParam, mKeyCode, -1) == true ||
                            this.isControlKey(mWParam, mKeyCode, -1) == true ||
                            this.isShiftKey(mWParam, mKeyCode) == true ||
                            this.isBuffKey(mWParam, mKeyCode) == true ||
                            this.isPotionKey(mWParam, mKeyCode, -1) == true)
                    {
                        mControlData.PauseKeyDataList[i].WParam = HookManager.NONE;
                        mControlData.PauseKeyDataList[i].KeyCode = (int)Keys.None;
                        this.setDictionaryData();
                        MessageBox.Show(this, StringLib.Message5, captionString);
                        return;
                    }

                    mControlData.PauseKeyDataList[i].WParam = mWParam;
                    mControlData.PauseKeyDataList[i].KeyCode = mKeyCode;
                    this.setDictionaryData();
                    return;
                }
            }

            if (mShiftKeyTextBox == textbox)
            {
                if (mWParam == HookManager.NONE && mKeyCode == (int)Keys.None)
                {
                    mControlData.ShiftKeyData.WParam = HookManager.WM_KEYDOWN;
                    mControlData.ShiftKeyData.KeyCode = (int)Keys.LShiftKey;
                    this.setDictionaryData();
                    return;
                }

                else if (mWParam == HookManager.WM_LBUTTONDOWN)
                {
                    mControlData.ShiftKeyData.WParam = HookManager.WM_KEYDOWN;
                    mControlData.ShiftKeyData.KeyCode = (int)Keys.LShiftKey;
                    this.setDictionaryData();
                    MessageBox.Show(this, StringLib.Message6, captionString);
                    return;
                }

                else if (mWParam == HookManager.WM_RBUTTONDOWN)
                {
                    mControlData.ShiftKeyData.WParam = HookManager.WM_KEYDOWN;
                    mControlData.ShiftKeyData.KeyCode = (int)Keys.LShiftKey;
                    this.setDictionaryData();
                    MessageBox.Show(this, StringLib.Message7, captionString);
                    return;
                }

                else if (this.isStartKey(mWParam, mKeyCode, -1) == true ||
                        this.isPauseKey(mWParam, mKeyCode, -1) == true ||
                        this.isControlKey(mWParam, mKeyCode, -1) == true ||
                        this.isBuffKey(mWParam, mKeyCode) == true ||
                        this.isPotionKey(mWParam, mKeyCode, -1) == true)
                {
                    mControlData.ShiftKeyData.WParam = HookManager.WM_KEYDOWN;
                    mControlData.ShiftKeyData.KeyCode = (int)Keys.LShiftKey;
                    this.setDictionaryData();
                    MessageBox.Show(this, StringLib.Message5, captionString);
                    return;
                }

                mControlData.ShiftKeyData.WParam = mWParam;
                mControlData.ShiftKeyData.KeyCode = mKeyCode;
                this.setDictionaryData();
                return;
            }

            for (int i = 0; i < mControlKeyTextBoxList.Count; i++)
            {
                if (mControlKeyTextBoxList[i] == textbox)
                {
                    if (mWParam == HookManager.NONE && mKeyCode == (int)Keys.None)
                    {
                        mControlData.ControlKeyDataList[i].WParam = mWParam;
                        mControlData.ControlKeyDataList[i].KeyCode = mKeyCode;
                        this.setDictionaryData();
                        return;
                    }

                    else if (this.isStartKey(mWParam, mKeyCode, -1) == true ||
                            this.isPauseKey(mWParam, mKeyCode, -1) == true ||
                            this.isControlKey(mWParam, mKeyCode, i) == true ||
                            this.isShiftKey(mWParam, mKeyCode) == true ||
                            this.isBuffKey(mWParam, mKeyCode) == true ||
                            this.isPotionKey(mWParam, mKeyCode, -1) == true)
                    {
                        mControlData.ControlKeyDataList[i].WParam = HookManager.NONE;
                        mControlData.ControlKeyDataList[i].KeyCode = (int)Keys.None;
                        this.setDictionaryData();
                        MessageBox.Show(this, StringLib.Message5, captionString);
                        return;
                    }

                    mControlData.ControlKeyDataList[i].WParam = mWParam;
                    mControlData.ControlKeyDataList[i].KeyCode = mKeyCode;
                    this.setDictionaryData();
                    return;
                }
            }

            for (int i = 0; i < mTechKeyTextBoxList.Count; i++)
            {
                if (mTechKeyTextBoxList[i] == textbox)
                {
                    if (mWParam == HookManager.NONE && mKeyCode == (int)Keys.None)
                    {
                        mControlData.ControlKeyDataList[i].WParamTech = mWParam;
                        mControlData.ControlKeyDataList[i].KeyCodeTech = mKeyCode;
                        this.setDictionaryData();
                        return;
                    }

                    mControlData.ControlKeyDataList[i].WParamTech = mWParam;
                    mControlData.ControlKeyDataList[i].KeyCodeTech = mKeyCode;
                    this.setDictionaryData();
                    return;
                }
            }

            // 현재 물약 키
            for (int i = 0; i < mPotionNowKeyTextBoxList.Count; i++)
            {
                if (mPotionNowKeyTextBoxList[i] == textbox)
                {
                    if (mWParam == HookManager.NONE && mKeyCode == (int)Keys.None)
                    {
                        mControlData.PotionKeyDataList[i].WParamNow = mWParam;
                        mControlData.PotionKeyDataList[i].KeyCodeNow = mKeyCode;
                        this.setDictionaryData();
                        return;
                    }

                    mControlData.PotionKeyDataList[i].WParamNow = mWParam;
                    mControlData.PotionKeyDataList[i].KeyCodeNow = mKeyCode;
                    this.setDictionaryData();
                    return;
                }
            }

            // 대체 키
            for (int i = 0; i < mPotionReplaceKeyTextBoxList.Count; i++)
            {
                if (mPotionReplaceKeyTextBoxList[i] == textbox)
                {
                    if (mWParam == HookManager.NONE && mKeyCode == (int)Keys.None)
                    {
                        mControlData.PotionKeyDataList[i].WParam = mWParam;
                        mControlData.PotionKeyDataList[i].KeyCode = mKeyCode;
                        this.setDictionaryData();
                        return;
                    }

                    else if (this.isStartKey(mWParam, mKeyCode, -1) == true ||
                            this.isPauseKey(mWParam, mKeyCode, -1) == true ||
                            this.isControlKey(mWParam, mKeyCode, i) == true ||
                            this.isShiftKey(mWParam, mKeyCode) == true ||
                            this.isBuffKey(mWParam, mKeyCode) == true ||
                            this.isPotionKey(mWParam, mKeyCode, i) == true)
                    {
                        mControlData.PotionKeyDataList[i].WParam = HookManager.NONE;
                        mControlData.PotionKeyDataList[i].KeyCode = (int)Keys.None;
                        this.setDictionaryData();
                        MessageBox.Show(this, StringLib.Message5, captionString);
                        return;
                    }

                    mControlData.PotionKeyDataList[i].WParam = mWParam;
                    mControlData.PotionKeyDataList[i].KeyCode = mKeyCode;
                    this.setDictionaryData();
                    return;
                }
            }

            // 버프 시작 키
            if (mBufStartTextBox == textbox)
            {
                if (mWParam == HookManager.NONE && mKeyCode == (int)Keys.None)
                {
                    mControlData.BuffKeyData.WParam = mWParam;
                    mControlData.BuffKeyData.KeyCode = mKeyCode;
                    this.setDictionaryData();
                    return;
                }

                else if (this.isStartKey(mWParam, mKeyCode, -1) == true ||
                        this.isPauseKey(mWParam, mKeyCode, -1) == true ||
                        this.isControlKey(mWParam, mKeyCode, -1) == true ||
                        this.isShiftKey(mWParam, mKeyCode) == true ||
                        this.isPotionKey(mWParam, mKeyCode, -1) == true)
                {
                    mControlData.BuffKeyData.WParam = HookManager.NONE;
                    mControlData.BuffKeyData.KeyCode = (int)Keys.None;
                    this.setDictionaryData();
                    MessageBox.Show(this, StringLib.Message5, captionString);
                    return;
                }

                mControlData.BuffKeyData.WParam = mWParam;
                mControlData.BuffKeyData.KeyCode = mKeyCode;
                this.setDictionaryData();
                return;
            }

            // 무기 교체 키
            if (mBufChangeTextBox == textbox)
            {
                if (mWParam == HookManager.NONE && mKeyCode == (int)Keys.None)
                {
                    mControlData.BuffKeyData.WParamWeapon = mWParam;
                    mControlData.BuffKeyData.KeyCodeWeapon = mKeyCode;
                    this.setDictionaryData();
                    return;
                }

                mControlData.BuffKeyData.WParamWeapon = mWParam;
                mControlData.BuffKeyData.KeyCodeWeapon = mKeyCode;
                this.setDictionaryData();
                return;
            }

            // 버프 키
            for (int i = 0; i < mBuffKeyTextBoxList.Count; i++)
            {
                if (mBuffKeyTextBoxList[i] == textbox)
                {
                    if (mWParam == HookManager.NONE && mKeyCode == (int)Keys.None)
                    {
                        mControlData.BuffKeyData.WParamBufList[i] = mWParam;
                        mControlData.BuffKeyData.KeyCodeBufList[i] = mKeyCode;
                        this.setDictionaryData();
                        return;
                    }

                    mControlData.BuffKeyData.WParamBufList[i] = mWParam;
                    mControlData.BuffKeyData.KeyCodeBufList[i] = mKeyCode;
                    this.setDictionaryData();
                    return;
                }
            }
        }

        private void setDictionaryData()
        {
            mStartMouseDictionary.Clear();
            mStartKeyDictionary.Clear();
            for (int i = 0; i < mControlData.StartKeyDataList.Count; i++)
            {
                if (mControlData.StartKeyDataList[i].WParam == HookManager.NONE) { }
                else if (mControlData.StartKeyDataList[i].WParam == HookManager.WM_KEYDOWN)
                {
                    mStartKeyDictionary[mControlData.StartKeyDataList[i].KeyCode] = mControlData.StartKeyDataList[i];
                }
                else
                {
                    mStartMouseDictionary[mControlData.StartKeyDataList[i].WParam] = mControlData.StartKeyDataList[i];
                }
            }

            mPauseMouseDictionary.Clear();
            mPauseKeyDictionary.Clear();
            for (int i = 0; i < mControlData.PauseKeyDataList.Count; i++)
            {
                if (mControlData.PauseKeyDataList[i].WParam == HookManager.NONE) { }
                else if (mControlData.PauseKeyDataList[i].WParam == HookManager.WM_KEYDOWN)
                {
                    mPauseKeyDictionary[mControlData.PauseKeyDataList[i].KeyCode] = mControlData.PauseKeyDataList[i];
                }
                else
                {
                    mPauseMouseDictionary[mControlData.PauseKeyDataList[i].WParam] = mControlData.PauseKeyDataList[i];
                }
            }

            mControlMouseDictionary.Clear();
            mControlKeyDictionary.Clear();
            for (int i = 0; i < mControlData.ControlKeyDataList.Count; i++)
            {
                if (mControlData.ControlKeyDataList[i].WParam == HookManager.NONE) { }
                else if (mControlData.ControlKeyDataList[i].WParam == HookManager.WM_KEYDOWN)
                {
                    mControlKeyDictionary[mControlData.ControlKeyDataList[i].KeyCode] = mControlData.ControlKeyDataList[i];
                }
                else
                {
                    mControlMouseDictionary[mControlData.ControlKeyDataList[i].WParam] = mControlData.ControlKeyDataList[i];
                }
            }

            mPotionMouseDictionary.Clear();
            mPotionKeyDictionary.Clear();
            for (int i = 0; i < mControlData.PotionKeyDataList.Count; i++)
            {
                if (mControlData.PotionKeyDataList[i].WParam == HookManager.NONE) { }
                else if (mControlData.PotionKeyDataList[i].WParam == HookManager.WM_KEYDOWN)
                {
                    mPotionKeyDictionary[mControlData.PotionKeyDataList[i].KeyCode] = mControlData.PotionKeyDataList[i];
                }
                else
                {
                    mPotionMouseDictionary[mControlData.PotionKeyDataList[i].WParam] = mControlData.PotionKeyDataList[i];
                }
            }
        }

        private bool isStartKey(int wParam, int keyCode, int exceptIndex)
        {
            for (int i = 0; i < mControlData.StartKeyDataList.Count; i++)
            {
                if (exceptIndex >= 0 && exceptIndex == i)
                    continue;

                if (wParam != HookManager.NONE && mControlData.StartKeyDataList[i].WParam == wParam && mControlData.StartKeyDataList[i].KeyCode == keyCode)
                    return true;
            }
            return false;
        }

        private bool isPauseKey(int wParam, int keyCode, int exceptIndex)
        {
            for (int i = 0; i < mControlData.PauseKeyDataList.Count; i++)
            {
                if (exceptIndex >= 0 && exceptIndex == i)
                    continue;

                if (wParam != HookManager.NONE && mControlData.PauseKeyDataList[i].WParam == wParam && mControlData.PauseKeyDataList[i].KeyCode == keyCode)
                    return true;
            }
            return false;
        }

        private bool isControlKey(int wParam, int keyCode, int exceptIndex)
        {
            for (int i = 0; i < mControlData.ControlKeyDataList.Count; i++)
            {
                if (exceptIndex >= 0 && exceptIndex == i)
                    continue;

                if (wParam != HookManager.NONE && mControlData.ControlKeyDataList[i].WParam == wParam && mControlData.ControlKeyDataList[i].KeyCode == keyCode)
                    return true;
            }
            return false;
        }

        private bool isShiftKey(int wParam, int keyCode)
        {
            return (wParam != HookManager.NONE && mControlData.ShiftKeyData.WParam == wParam && mControlData.ShiftKeyData.KeyCode == keyCode);
        }

        private bool isBuffKey(int wParam, int keyCode)
        {
            return (wParam != HookManager.NONE && mControlData.BuffKeyData.WParam == wParam && mControlData.BuffKeyData.KeyCode == keyCode);
        }

        private bool isPotionKey(int wParam, int keyCode, int exceptIndex)
        {
            for (int i = 0; i < mControlData.PotionKeyDataList.Count; i++)
            {
                if (exceptIndex >= 0 && exceptIndex == i)
                    continue;

                if (wParam != HookManager.NONE && mControlData.PotionKeyDataList[i].WParam == wParam && mControlData.PotionKeyDataList[i].KeyCode == keyCode)
                    return true;
            }
            return false;
        }

        private void setStateLabel(bool isStop, bool isPause, bool isBuff, int controlIndex)
        {
            this.BeginInvoke(new Action(delegate ()
            {
                if (isStop == true)
                {
                    mToolStripStatusLabel.Text = StringLib.Status + " " + StringLib.Stop;
                    return;
                }

                if (isPause == true)
                {
                    mToolStripStatusLabel.Text = StringLib.Status + " " + StringLib.Pause;
                    return;
                }

                if (isBuff == true)
                {
                    mToolStripStatusLabel.Text = StringLib.Status + " " + StringLib.Buff;
                    return;
                }

                string indexString = "";
                switch (controlIndex)
                {
                    case 0: indexString = "① "; break;
                    case 1: indexString = "② "; break;
                    case 2: indexString = "③ "; break;
                    case 3: indexString = "④ "; break;
                    case 4: indexString = "⑤ "; break;
                    case 5: indexString = "⑥ "; break;
                    case 6: indexString = "⑦ "; break;
                    case 7: indexString = "⑧ "; break;
                    case 8: indexString = "⑨ "; break;
                    case 9: indexString = "⑩ "; break;
                    case 10: indexString = "⑪ "; break;
                    case 11: indexString = "⑫ "; break;
                    default: indexString = "① "; break;
                }

                string localString = StringLib.Localize;
                if (localString.CompareTo("ko") == 0)
                {
                    indexString = indexString + StringLib.Ing;
                }
                else
                {
                    indexString = StringLib.Ing + " " + indexString;
                }                

                if (mControlData.ControlKeyDataList[controlIndex].MemoString.Length > 0)
                {
                    indexString = indexString + " (" + (mControlData.ControlKeyDataList[controlIndex].MemoString) + ")";
                }

                mToolStripStatusLabel.Text = StringLib.Status + " " + indexString;
            }));
        }

        private string getKeyString(int wParam, Keys keyCode)
        {
            if (wParam == HookManager.WM_KEYDOWN)
            {
                switch (keyCode)
                {
                    case Keys.LControlKey: return "Ctrl";
                    case Keys.RControlKey: return "Ctrl";
                    case Keys.LShiftKey: return "Shift";
                    case Keys.RShiftKey: return "Shift";
                    case Keys.LMenu: return "Alt";
                    case Keys.RMenu: return "Alt";
                    case Keys.CapsLock: return "CapsLock";
                    case Keys.Oemtilde: return "`";
                    case Keys.OemMinus: return "-";
                    case Keys.Oemplus: return "+";
                    case Keys.OemOpenBrackets: return "[";
                    case Keys.OemCloseBrackets: return "]";
                    case Keys.Oem5: return "\\";
                    case Keys.Oem1: return ";";
                    case Keys.Oem7: return "'";
                    case Keys.Oemcomma: return ",";
                    case Keys.OemPeriod: return ".";
                    case Keys.OemQuestion: return "/";
                    case Keys.Divide: return "/";
                    case Keys.Multiply: return "*";
                    case Keys.Subtract: return "-";
                    case Keys.Add: return "+";
                    default:
                        break;
                }

                var kc = new KeysConverter();
                return kc.ConvertToString(keyCode);
            }
            else
            {
                switch (wParam)
                {
                    case HookManager.WM_LBUTTONDOWN:
                        return "Mouse left";
                    case HookManager.WM_RBUTTONDOWN:
                        return "Mouse right";
                    case HookManager.WM_MBUTTONDOWN:
                        return "Mouse middle";
                    case HookManager.WM_XBUTTONDOWN:
                        return "Mouse xbutton1";
                    case HookManager.WM_XBUTTONDOWN2:
                        return "Mouse xbutton2";
                    case HookManager.WM_MWHEELUP:
                        return "Wheel up";
                    case HookManager.WM_MWHEELDOWN:
                        return "Wheel down";
                    default:
                        break;
                }
            }
            return "";
        }

        private int onHookHandler(int nCode, int wParam, IntPtr lParam)
        {
            int value = 1;
            if (mIsArea == true) { }
            else if (mIsSetKey == true)
            {
                value = this.onSetKey(nCode, wParam, lParam);
            }

            else
            {
                value = this.onProcessKey(nCode, wParam, lParam);
                this.onDebugKey(nCode, wParam, lParam);
            }
            return value;
        }

        private int onSetKey(int nCode, int wParam, IntPtr lParam)
        {
            TextBox textBox = null;
            foreach (var box in mKeyTextBoxList)
            {
                if (box.Focused == true)
                {
                    textBox = box;
                    break;
                }
            }
            if (textBox == null)
                return 1;

            if (wParam == HookManager.WM_KEYDOWN || wParam == HookManager.WM_SYSKEYDOWN)
            {
                var keyData = (HookManager.KeyHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.KeyHookStruct));
                if (keyData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                    return 1;

                switch (keyData.vkCode)
                {
                    // pass window key
                    case (int)Keys.LWin:
                    case (int)Keys.RWin:
                        return 1;

                    // esc
                    case (int)Keys.Escape:
                        {
                            mWParam = HookManager.NONE;
                            mKeyCode = (int)Keys.None;
                            this.setControlData(textBox);
                            this.ActiveControl = null;
                        }
                        break;

                    // enter
                    case (int)Keys.Enter:
                        {
                            mWParam = HookManager.NONE;
                            mKeyCode = (int)Keys.None;
                            this.setControlData(textBox);
                            this.ActiveControl = null;
                        }
                        break;

                    case (int)Keys.LControlKey:
                    case (int)Keys.RControlKey:
                    case (int)Keys.LShiftKey:
                    case (int)Keys.RShiftKey:
                    case (int)Keys.LMenu:
                    case (int)Keys.RMenu:
                    case (int)Keys.F1:
                    case (int)Keys.F2:
                    case (int)Keys.F3:
                    case (int)Keys.F4:
                    case (int)Keys.F5:
                    case (int)Keys.F6:
                    case (int)Keys.F7:
                    case (int)Keys.F8:
                    case (int)Keys.F9:
                    case (int)Keys.F10:
                    case (int)Keys.F11:
                    case (int)Keys.F12:
                    case (int)Keys.Oemtilde:            // `~
                    case (int)Keys.OemMinus:            // -_
                    case (int)Keys.Oemplus:             // =+
                    case (int)Keys.OemOpenBrackets:     // [{
                    case (int)Keys.OemCloseBrackets:    // ]}
                    case (int)Keys.Oem5:                // \|
                    case (int)Keys.Oem1:                // ;:
                    case (int)Keys.Oem7:                // '"
                    case (int)Keys.Oemcomma:            // ,<
                    case (int)Keys.OemPeriod:           // .>
                    case (int)Keys.OemQuestion:         // /?
                    case (int)Keys.Divide:              // /
                    case (int)Keys.Multiply:            // *
                    case (int)Keys.Subtract:            // -
                    case (int)Keys.Add:                 // +
                    case (int)Keys.Decimal:             // ·
                    case (int)Keys.Tab:
                    case (int)Keys.CapsLock:
                    case (int)Keys.NumLock:
                    case (int)Keys.Space:
                    case (int)Keys.Insert:
                    case (int)Keys.Delete:
                    case (int)Keys.Home:
                    case (int)Keys.End:
                    case (int)Keys.PageUp:
                    case (int)Keys.PageDown:
                    case (int)Keys.Left:
                    case (int)Keys.Right:
                    case (int)Keys.Up:
                    case (int)Keys.Down:
                    case (int)Keys.NumPad0:
                    case (int)Keys.NumPad1:
                    case (int)Keys.NumPad2:
                    case (int)Keys.NumPad3:
                    case (int)Keys.NumPad4:
                    case (int)Keys.NumPad5:
                    case (int)Keys.NumPad6:
                    case (int)Keys.NumPad7:
                    case (int)Keys.NumPad8:
                    case (int)Keys.NumPad9:
                    case (int)Keys.D1:
                    case (int)Keys.D2:
                    case (int)Keys.D3:
                    case (int)Keys.D4:
                    case (int)Keys.D5:
                    case (int)Keys.D6:
                    case (int)Keys.D7:
                    case (int)Keys.D8:
                    case (int)Keys.D9:
                    case (int)Keys.D0:
                    case (int)Keys.A:
                    case (int)Keys.B:
                    case (int)Keys.C:
                    case (int)Keys.D:
                    case (int)Keys.E:
                    case (int)Keys.F:
                    case (int)Keys.G:
                    case (int)Keys.H:
                    case (int)Keys.I:
                    case (int)Keys.J:
                    case (int)Keys.K:
                    case (int)Keys.L:
                    case (int)Keys.M:
                    case (int)Keys.N:
                    case (int)Keys.O:
                    case (int)Keys.P:
                    case (int)Keys.Q:
                    case (int)Keys.R:
                    case (int)Keys.S:
                    case (int)Keys.T:
                    case (int)Keys.U:
                    case (int)Keys.V:
                    case (int)Keys.W:
                    case (int)Keys.X:
                    case (int)Keys.Y:
                    case (int)Keys.Z:
                        {
                            mWParam = HookManager.WM_KEYDOWN;
                            mKeyCode = (int)keyData.vkCode;
                            this.setControlData(textBox);

                            textBox.Text = this.getKeyString(mWParam, (Keys)mKeyCode);

                            this.ActiveControl = null;
                        }
                        break;

                    default:
                        break;
                }
            }

            else if (wParam == HookManager.WM_LBUTTONDOWN)
            {
                var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                    return 1;

                this.debugPrintln("onSetKey() : WM_LBUTTONDOWN");

                mWParam = HookManager.WM_LBUTTONDOWN;
                this.setControlData(textBox);

                textBox.Text = this.getKeyString(mWParam, 0);
                this.ActiveControl = null;
                //return 0;
            }

            else if (wParam == HookManager.WM_RBUTTONDOWN)
            {
                var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                    return 1;

                mWParam = HookManager.WM_RBUTTONDOWN;
                this.setControlData(textBox);

                textBox.Text = this.getKeyString(mWParam, 0);
                this.ActiveControl = null;
                //return 0;
            }

            else if (wParam == HookManager.WM_MBUTTONDOWN)
            {
                var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                    return 1;

                mWParam = HookManager.WM_MBUTTONDOWN;
                this.setControlData(textBox);

                textBox.Text = this.getKeyString(mWParam, 0);
                this.ActiveControl = null;
            }

            else if (wParam == HookManager.WM_XBUTTONDOWN)
            {
                var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                    return 1;

                // back
                if (mouseData.mouseData == 0x10000)
                {
                    mWParam = HookManager.WM_XBUTTONDOWN;
                    this.setControlData(textBox);

                    textBox.Text = this.getKeyString(mWParam, 0);
                    this.ActiveControl = null;
                }

                // forward
                else
                {
                    mWParam = HookManager.WM_XBUTTONDOWN2;
                    this.setControlData(textBox);

                    textBox.Text = this.getKeyString(mWParam, 0);
                    this.ActiveControl = null;
                }
            }

            else if (wParam == HookManager.WM_MWHEEL)
            {
                var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                    return 1;

                // 헬퍼 사용 시작/종료키
                bool isEnable = false;
                foreach (var temp in mStartTextBoxList)
                {
                    if (temp == textBox)
                    {
                        isEnable = true;
                        break;
                    }
                }

                // 현재 물약 키
                foreach (var temp in mPotionNowKeyTextBoxList)
                {
                    if (temp == textBox)
                    {
                        isEnable = true;
                        break;
                    }
                }

                // 용병 포션 대체 키
                foreach (var temp in mPotionReplaceKeyTextBoxList)
                {
                    if (temp == textBox)
                    {
                        isEnable = true;
                        break;
                    }
                }

                // 버프키
                if (mBufStartTextBox == textBox)
                {
                    isEnable = true;
                }

                if (isEnable == true)
                {
                    uint up = 0x70;
                    uint wheelData = (mouseData.mouseData >> 16);
                    if ((up & wheelData) == up)
                    {
                        mWParam = HookManager.WM_MWHEELUP;
                        this.setControlData(textBox);

                        textBox.Text = this.getKeyString(mWParam, 0);
                        this.ActiveControl = null;
                    }
                    else
                    {
                        mWParam = HookManager.WM_MWHEELDOWN;
                        this.setControlData(textBox);

                        textBox.Text = this.getKeyString(mWParam, 0);
                        this.ActiveControl = null;
                    }
                }
            }
            return 1;
        }

        private int onProcessKey(int nCode, int wParam, IntPtr lParam)
        {
            bool isStart = false;
            lock (mLock)
            {
                isStart = mIsStart;
            }

            switch (wParam)
            {
                case HookManager.WM_KEYDOWN:
                case HookManager.WM_SYSKEYDOWN:
                    {
                        var keyData = (HookManager.KeyHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.KeyHookStruct));
                        if (keyData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return 1;

                        // 헬퍼 사용 시작/종료키 체크
                        if (mStartKeyDictionary.ContainsKey(keyData.vkCode) == true)
                        {
                            this.startAndStopControl();
                            return 1;
                        }

                        // 잠시멈춤 키 체크
                        else if (isStart == true && (mPauseKeyDictionary.ContainsKey(keyData.vkCode) == true))
                        {
                            this.pauseButtonDown(HookManager.WM_KEYDOWN, keyData.vkCode);
                            return 1;
                        }

                        // control 체크
                        else if (isStart == true && mControlKeyDictionary.ContainsKey(keyData.vkCode) == true)
                        {
                            return this.controlButtonDown(HookManager.WM_KEYDOWN, keyData.vkCode);
                        }

                        // 용병 물약키 체크
                        else if (isStart == true && mPotionKeyDictionary.ContainsKey(keyData.vkCode) == true)
                        {
                            return this.potionButtonDown(HookManager.WM_KEYDOWN, keyData.vkCode);
                        }

                        // 버프 시작 키 체크
                        else if(isStart == true && mControlData.BuffKeyData.WParam == HookManager.WM_KEYDOWN && mControlData.BuffKeyData.KeyCode == keyData.vkCode)
                        {
                            this.buffButtonDown();
                            return 1;
                        }
                    }
                    break;

                case HookManager.WM_LBUTTONDOWN:
                case HookManager.WM_RBUTTONDOWN:
                case HookManager.WM_MBUTTONDOWN:
                    {
                        var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                        if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return 1;

                        // 헬퍼 사용 시작/종료키 체크
                        if (mStartMouseDictionary.ContainsKey(wParam) == true)
                        {
                            this.startAndStopControl();
                            return 1;
                        }

                        // 잠시멈춤 키 체크
                        else if (isStart == true && mPauseMouseDictionary.ContainsKey(wParam) == true)
                        {
                            this.pauseButtonDown(wParam, (int)Keys.None);
                            return 1;
                        }

                        // control 체크
                        else if (isStart == true && mControlMouseDictionary.ContainsKey(wParam) == true)
                        {
                            return this.controlButtonDown(wParam, (int)Keys.None);
                        }

                        // 용병 물약키 체크
                        else if (isStart == true && mPotionMouseDictionary.ContainsKey(wParam) == true)
                        {
                            return this.potionButtonDown(wParam, (int)Keys.None);
                        }

                        // 버프키 체크
                        else if (isStart == true && mControlData.BuffKeyData.WParam == wParam)
                        {
                            this.buffButtonDown();
                            return 1;
                        }
                    }
                    break;
                case HookManager.WM_XBUTTONDOWN:
                    {
                        var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                        if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return 1;

                        // back
                        if (mouseData.mouseData == 0x10000)
                        {
                            // 헬퍼 사용 시작/종료키 체크
                            if (mStartMouseDictionary.ContainsKey(wParam) == true)
                            {
                                this.startAndStopControl();
                                return 1;
                            }

                            // 잠시멈춤 키 체크
                            else if (isStart == true && mPauseMouseDictionary.ContainsKey(wParam) == true)
                            {
                                this.pauseButtonDown(wParam, (int)Keys.None);
                                return 1;
                            }

                            // control 체크
                            else if (isStart == true && mControlMouseDictionary.ContainsKey(wParam) == true)
                            {
                                return this.controlButtonDown(wParam, (int)Keys.None);
                            }

                            // 용병 물약키 체크
                            else if (isStart == true && mPotionMouseDictionary.ContainsKey(wParam) == true)
                            {
                                return this.potionButtonDown(wParam, (int)Keys.None);
                            }

                            // 버프키 체크
                            else if (isStart == true && mControlData.BuffKeyData.WParam == wParam)
                            {
                                this.buffButtonDown();
                                return 1;
                            }
                        }

                        // forward 
                        else
                        {
                            // 헬퍼 사용 시작/종료키 체크
                            if (mStartMouseDictionary.ContainsKey(HookManager.WM_XBUTTONDOWN2) == true)
                            {
                                this.startAndStopControl();
                                return 1;
                            }

                            // 잠시멈춤 키 체크
                            else if (isStart == true && mPauseMouseDictionary.ContainsKey(HookManager.WM_XBUTTONDOWN2) == true)
                            {
                                this.pauseButtonDown(HookManager.WM_XBUTTONDOWN2, (int)Keys.None);
                                return 1;
                            }

                            // control 체크
                            else if (isStart == true && mControlMouseDictionary.ContainsKey(HookManager.WM_XBUTTONDOWN2) == true)
                            {
                                return this.controlButtonDown(HookManager.WM_XBUTTONDOWN2, (int)Keys.None);
                            }

                            // 용병 물약키 체크
                            else if (isStart == true && mPotionMouseDictionary.ContainsKey(HookManager.WM_XBUTTONDOWN2) == true)
                            {
                                return this.potionButtonDown(HookManager.WM_XBUTTONDOWN2, (int)Keys.None);
                            }

                            // 버프키 체크
                            else if (isStart == true && mControlData.BuffKeyData.WParam == HookManager.WM_XBUTTONDOWN2)
                            {
                                this.buffButtonDown();
                                return 1;
                            }
                        }
                    }
                    break;

                case HookManager.WM_MWHEEL:
                    {
                        var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                        if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return 1;

                        uint up = 0x70;
                        uint wheelData = (mouseData.mouseData >> 16);
                        if ((up & wheelData) == up)
                        {
                            // 헬퍼 사용 시작/종료키 체크
                            if (mStartMouseDictionary.ContainsKey(HookManager.WM_MWHEELUP) == true)
                            {
                                this.startAndStopControl();
                                return 1;
                            }

                            // control 체크
                            else if (isStart == true && mControlMouseDictionary.ContainsKey(HookManager.WM_MWHEELUP) == true)
                            {
                                return this.controlButtonDown(HookManager.WM_MWHEELUP, (int)Keys.None);
                            }

                            // 용병 물약키 체크
                            else if (isStart == true && mPotionMouseDictionary.ContainsKey(HookManager.WM_MWHEELUP) == true)
                            {
                                return this.potionButtonDown(HookManager.WM_MWHEELUP, (int)Keys.None);
                            }

                            // 버프키 체크
                            else if (isStart == true && mControlData.BuffKeyData.WParam == HookManager.WM_MWHEELUP)
                            {
                                this.buffButtonDown();
                                return 1;
                            }
                        }
                        else
                        {
                            // 헬퍼 사용 시작/종료키 체크
                            if (mStartMouseDictionary.ContainsKey(HookManager.WM_MWHEELDOWN) == true)
                            {
                                this.startAndStopControl();
                                return 1;
                            }

                            // control 체크
                            else if (isStart == true && mControlMouseDictionary.ContainsKey(HookManager.WM_MWHEELDOWN) == true)
                            {
                                return this.controlButtonDown(HookManager.WM_MWHEELDOWN, (int)Keys.None);
                            }

                            // 용병 물약키 체크
                            else if (isStart == true && mPotionMouseDictionary.ContainsKey(HookManager.WM_MWHEELDOWN) == true)
                            {
                                return this.potionButtonDown(HookManager.WM_MWHEELDOWN, (int)Keys.None);
                            }

                            // 버프키 체크
                            else if (isStart == true && mControlData.BuffKeyData.WParam == HookManager.WM_MWHEELDOWN)
                            {
                                this.buffButtonDown();
                                return 1;
                            }
                        }
                    }
                    break;

                case HookManager.WM_KEYUP:
                case HookManager.WM_SYSKEYUP:
                    {
                        if (isStart == false)
                            break;

                        var keyData = (HookManager.KeyHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.KeyHookStruct));
                        if (keyData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return 1;

                        if (keyData.vkCode == (int)Keys.LShiftKey)
                        {
                            try
                            {
                                Monitor.Enter(mControlTimerLock);
                                mIsShiftDown = false;
                            }
                            finally
                            {
                                Monitor.Exit(mControlTimerLock);
                            }
                        }

                        // 잠시멈춤 키 체크
                        if (isStart == true && mPauseKeyDictionary.ContainsKey(keyData.vkCode) == true)
                        {
                            this.pauseButtonUp();
                            return 1;
                        }

                        // control 체크
                        else if (isStart == true && mControlKeyDictionary.ContainsKey(keyData.vkCode) == true)
                        {
                            return this.controlButtonUp(HookManager.WM_KEYDOWN, keyData.vkCode);
                        }
                    }
                    break;

                case HookManager.WM_LBUTTONUP:
                    {
                        if (isStart == false)
                            break;

                        var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                        if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return 1;

                        try
                        {
                            Monitor.Enter(mControlTimerLock);
                            mIsLeftDown = false;
                            mIsRightDown = false;
                        }
                        finally
                        {
                            Monitor.Exit(mControlTimerLock);
                        }

                        // 잠시멈춤 키 체크
                        if (isStart == true && mPauseMouseDictionary.ContainsKey(HookManager.WM_LBUTTONDOWN) == true)
                        {
                            this.pauseButtonUp();
                            return 1;
                        }

                        // control 체크
                        else if (isStart == true && mControlMouseDictionary.ContainsKey(HookManager.WM_LBUTTONDOWN) == true)
                        {
                            return this.controlButtonUp(HookManager.WM_LBUTTONDOWN, (int)Keys.None);
                        }
                    }
                    break;
                case HookManager.WM_RBUTTONUP:
                    {
                        if (isStart == false)
                            break;

                        var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                        if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return 1;

                        try
                        {
                            Monitor.Enter(mControlTimerLock);
                            mIsLeftDown = false;
                            mIsRightDown = false;
                        }
                        finally
                        {
                            Monitor.Exit(mControlTimerLock);
                        }

                        // 잠시멈춤 키 체크
                        if (isStart == true && mPauseMouseDictionary.ContainsKey(HookManager.WM_RBUTTONDOWN) == true)
                        {
                            this.pauseButtonUp();
                            return 1;
                        }

                        // control 체크
                        else if (isStart == true && mControlMouseDictionary.ContainsKey(HookManager.WM_RBUTTONDOWN) == true)
                        {
                            return this.controlButtonUp(HookManager.WM_RBUTTONDOWN, (int)Keys.None);
                        }
                    }
                    break;
                case HookManager.WM_MBUTTONUP:
                    {
                        if (isStart == false)
                            break;

                        var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                        if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return 1;

                        // 잠시멈춤 키 체크
                        if (isStart == true && mPauseMouseDictionary.ContainsKey(HookManager.WM_MBUTTONDOWN) == true)
                        {
                            this.pauseButtonUp();
                            return 1;
                        }

                        // control 체크
                        else if (isStart == true && mControlMouseDictionary.ContainsKey(HookManager.WM_MBUTTONDOWN) == true)
                        {
                            return this.controlButtonUp(HookManager.WM_MBUTTONDOWN, (int)Keys.None);
                        }
                    }
                    break;

                case HookManager.WM_XBUTTONUP:
                    {
                        if (isStart == false)
                            break;

                        var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                        if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return 1;

                        // back
                        if (mouseData.mouseData == 0x10000)
                        {
                            // 잠시멈춤 키 체크
                            if (isStart == true && mPauseMouseDictionary.ContainsKey(HookManager.WM_XBUTTONDOWN) == true)
                            {
                                this.pauseButtonUp();
                                return 1;
                            }

                            // control 체크
                            else if (isStart == true && mControlMouseDictionary.ContainsKey(HookManager.WM_XBUTTONDOWN) == true)
                            {
                                return this.controlButtonUp(HookManager.WM_XBUTTONDOWN, (int)Keys.None);
                            }
                        }

                        // forward
                        else
                        {
                            // 잠시멈춤 키 체크
                            if (isStart == true && mPauseMouseDictionary.ContainsKey(HookManager.WM_XBUTTONDOWN2) == true)
                            {
                                this.pauseButtonUp();
                                return 1;
                            }

                            // control 체크
                            else if (isStart == true && mControlMouseDictionary.ContainsKey(HookManager.WM_XBUTTONDOWN2) == true)
                            {
                                return this.controlButtonUp(HookManager.WM_XBUTTONDOWN2, (int)Keys.None);
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
            return 1;
        }

        private void onDebugKey(int nCode, int wParam, IntPtr lParam)
        {
            switch (wParam)
            {
                case HookManager.WM_KEYDOWN:
                case HookManager.WM_SYSKEYDOWN:
                    {
                        var keyData = (HookManager.KeyHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.KeyHookStruct));
                        if (keyData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return;
                        this.debugPrintln("onDebugKey() : key down (" + keyData.vkCode + "), dwExtraInfo(" + keyData.dwExtraInfo + ")");
                    }
                    break;

                case HookManager.WM_KEYUP:
                case HookManager.WM_SYSKEYUP:
                    {
                        var keyData = (HookManager.KeyHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.KeyHookStruct));
                        if (keyData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return;
                        this.debugPrintln("onDebugKey() : key up (" + keyData.vkCode + "), dwExtraInfo(" + keyData.dwExtraInfo + ")");
                    }
                    break;

                case HookManager.WM_LBUTTONDOWN:
                    {
                        var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                        if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return;
                        this.debugPrintln("onDebugKey() : mouse left down, dwExtraInfo(" + mouseData.dwExtraInfo + ")");
                    }
                    break;

                case HookManager.WM_RBUTTONDOWN:
                    {
                        var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                        if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return;
                        this.debugPrintln("onDebugKey() : mouse right down, dwExtraInfo(" + mouseData.dwExtraInfo + ")");
                    }
                    break;

                case HookManager.WM_MBUTTONDOWN:
                    {
                        var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                        if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return;
                        this.debugPrintln("onDebugKey() : mouse middle down, dwExtraInfo(" + mouseData.dwExtraInfo + ")");
                    }
                    break;

                case HookManager.WM_XBUTTONDOWN:
                    {
                        var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                        if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return;

                        // back
                        if (mouseData.mouseData == 0x10000)
                        {
                            this.debugPrintln("onDebugKey() : mouse xbutton1 down, dwExtraInfo(" + mouseData.dwExtraInfo + ")");
                        }

                        // forward
                        else
                        {
                            this.debugPrintln("onDebugKey() : mouse xbutton2 down, dwExtraInfo(" + mouseData.dwExtraInfo + ")");
                        }
                    }
                    break;

                case HookManager.WM_LBUTTONUP:
                    {
                        var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                        if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return;
                        this.debugPrintln("onDebugKey() : mouse left up, dwExtraInfo(" + mouseData.dwExtraInfo + ")");
                    }
                    break;

                case HookManager.WM_RBUTTONUP:
                    {
                        var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                        if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return;
                        this.debugPrintln("onDebugKey() : mouse right up, dwExtraInfo(" + mouseData.dwExtraInfo + ")");
                    }
                    break;

                case HookManager.WM_MBUTTONUP:
                    {
                        var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                        if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return;
                        this.debugPrintln("onDebugKey() : mouse middle up, dwExtraInfo(" + mouseData.dwExtraInfo + ")");
                    }
                    break;

                case HookManager.WM_XBUTTONUP:
                    {
                        var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                        if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return;

                        // back
                        if (mouseData.mouseData == 0x10000)
                        {
                            this.debugPrintln("onDebugKey() : mouse xbutton1 up, dwExtraInfo(" + mouseData.dwExtraInfo + ")");
                        }

                        // forward
                        else
                        {
                            this.debugPrintln("onDebugKey() : mouse xbutton2 up, dwExtraInfo(" + mouseData.dwExtraInfo + ")");
                        }
                    }
                    break;

                case HookManager.WM_MWHEEL:
                    {
                        var mouseData = (HookManager.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(HookManager.MouseHookStruct));
                        if (mouseData.dwExtraInfo == (IntPtr)HookManager.EXTRA_INFO)
                            return;

                        uint up = 0x70;
                        uint down = 0x80;
                        uint wheelData = (mouseData.mouseData >> 16);
                        if ((up & wheelData) == up)
                        {
                            this.debugPrintln("onDebugKey() : mouse wheel up");
                        }
                        else if ((down & wheelData) == down)
                        {
                            this.debugPrintln("onDebugKey() : mouse wheel down");
                        }
                    }
                    break;

                default:
                    break;
            }
        }
        
        private void keyMouseEvent(int wParam, int keyCode, bool isDown)
        {
            switch (wParam)
            {
                case HookManager.WM_KEYDOWN:
                case HookManager.WM_KEYUP:
                    {
                        uint keyFlags = (isDown == true) ? (uint)HookManager.KEY_DOWN : (uint)HookManager.KEY_UP;

                        switch (keyCode)
                        {
                            case (int)Keys.LShiftKey:
                            case (int)Keys.RShiftKey:
                                HookManager.keybd_event(HookManager.VK_SHIFT, HookManager.MapVirtualKey(HookManager.VK_SHIFT, 0), keyFlags, (IntPtr)HookManager.EXTRA_INFO);
                                break;

                            case (int)Keys.LControlKey:
                            case (int)Keys.RControlKey:
                                HookManager.keybd_event(HookManager.VK_CTRL, HookManager.MapVirtualKey(HookManager.VK_CTRL, 0), keyFlags, (IntPtr)HookManager.EXTRA_INFO);
                                break;

                            case (int)Keys.LMenu:
                            case (int)Keys.RMenu:
                                HookManager.keybd_event(HookManager.VK_MENU, HookManager.MapVirtualKey(HookManager.VK_MENU, 0), keyFlags, (IntPtr)HookManager.EXTRA_INFO);
                                break;

                            default:
                                HookManager.keybd_event((uint)keyCode, 0, keyFlags, (IntPtr)HookManager.EXTRA_INFO);
                                break;
                        }
                    }
                    break;

                case HookManager.WM_LBUTTONDOWN:
                case HookManager.WM_LBUTTONUP:
                    {
                        uint dwFlags = (isDown == true) ? (uint)HookManager.LB_DOWN : (uint)HookManager.LB_UP;
                        HookManager.mouse_event(dwFlags, 0, 0, 0, (IntPtr)HookManager.EXTRA_INFO);
                    }
                    break;

                case HookManager.WM_RBUTTONDOWN:
                case HookManager.WM_RBUTTONUP:
                    {
                        uint dwFlags = (isDown == true) ? (uint)HookManager.RB_DOWN : (uint)HookManager.RB_UP;
                        HookManager.mouse_event(dwFlags, 0, 0, 0, (IntPtr)HookManager.EXTRA_INFO);
                    }
                    break;

                case HookManager.WM_MBUTTONDOWN:
                case HookManager.WM_MBUTTONUP:
                    {
                        uint dwFlags = (isDown == true) ? (uint)HookManager.MB_DOWN : (uint)HookManager.MB_UP;
                        HookManager.mouse_event(dwFlags, 0, 0, 0, (IntPtr)HookManager.EXTRA_INFO);
                    }
                    break;

                case HookManager.WM_XBUTTONDOWN:
                case HookManager.WM_XBUTTONUP:
                    {
                        uint dwFlags = (isDown == true) ? (uint)HookManager.XB_DOWN : (uint)HookManager.XB_UP;
                        HookManager.mouse_event(dwFlags, 0, 0, HookManager.XBUTTON1, (IntPtr)HookManager.EXTRA_INFO);
                    }
                    break;

                case HookManager.WM_XBUTTONDOWN2:
                case HookManager.WM_XBUTTONUP2:
                    {
                        uint dwFlags = (isDown == true) ? (uint)HookManager.XB_DOWN : (uint)HookManager.XB_UP;
                        HookManager.mouse_event(dwFlags, 0, 0, HookManager.XBUTTON2, (IntPtr)HookManager.EXTRA_INFO);
                    }
                    break;

                case HookManager.WM_MWHEELUP:
                    {
                        this.debugPrintln("keyMouseEvent() : HookManager.WM_MWHEELUP");

                        int wheel = 60;
                        HookManager.mouse_event(HookManager.WHEEL, 0, 0, (uint)wheel, (IntPtr)HookManager.EXTRA_INFO);
                    }
                    break;

                case HookManager.WM_MWHEELDOWN:
                    {
                        this.debugPrintln("keyMouseEvent() : HookManager.WM_MWHEELDOWN");

                        int wheel = -60;
                        HookManager.mouse_event(HookManager.WHEEL, 0, 0, (uint)wheel, (IntPtr)HookManager.EXTRA_INFO);
                    }
                    break;

                default:
                    break;
            }
        }

        private string read()
        {
            try
            {
                var jsonString = File.ReadAllText(mJsonFileName);
                var rootObject = JObject.Parse(jsonString);

                if (rootObject.ContainsKey("filename") == true)
                {
                    string fileName = rootObject.Value<string>("filename");
                    if (this.readData(fileName) == true)
                    {
                        return fileName;
                    }
                }
            }
            catch { }
            return "";
        }

        private bool readData(string fileName)
        {
            try
            {
                mControlData.clear();
                mOriginControlData.clear();

                var jsonString = File.ReadAllText(fileName);
                var rootObject = JObject.Parse(jsonString);

                if (rootObject.ContainsKey("StartKey") == true)
                {
                    var keyArray = rootObject.Value<JArray>("StartKey");
                    for (int i = 0; i < keyArray.Count; i++)
                    {
                        var tempObject = (JObject)keyArray[i];
                        mControlData.StartKeyDataList[i].WParam = (tempObject.ContainsKey("WParam") == true) ? tempObject.Value<int>("WParam") : 0;
                        mControlData.StartKeyDataList[i].KeyCode = (tempObject.ContainsKey("KeyCode") == true) ? tempObject.Value<int>("KeyCode") : 0;

                        mStartTextBoxList[i].Text = this.getKeyString(mControlData.StartKeyDataList[i].WParam, (Keys)mControlData.StartKeyDataList[i].KeyCode);
                    }
                }

                if (rootObject.ContainsKey("PauseKey") == true)
                {
                    var keyArray = rootObject.Value<JArray>("PauseKey");
                    for (int i = 0; i < keyArray.Count; i++)
                    {
                        var tempObject = (JObject)keyArray[i];
                        mControlData.PauseKeyDataList[i].WParam = (tempObject.ContainsKey("WParam") == true) ? tempObject.Value<int>("WParam") : 0;
                        mControlData.PauseKeyDataList[i].KeyCode = (tempObject.ContainsKey("KeyCode") == true) ? tempObject.Value<int>("KeyCode") : 0;

                        mPauseTextBoxList[i].Text = this.getKeyString(mControlData.PauseKeyDataList[i].WParam, (Keys)mControlData.PauseKeyDataList[i].KeyCode);
                    }
                }

                if (rootObject.ContainsKey("ControlKey") == true)
                {
                    var keyArray = rootObject.Value<JArray>("ControlKey");
                    for (int i = 0; i < keyArray.Count; i++)
                    {
                        var tempObject = (JObject)keyArray[i];
                        mControlData.ControlKeyDataList[i].WParam = (tempObject.ContainsKey("WParam") == true) ? tempObject.Value<int>("WParam") : 0;
                        mControlData.ControlKeyDataList[i].KeyCode = (tempObject.ContainsKey("KeyCode") == true) ? tempObject.Value<int>("KeyCode") : 0;
                        mControlData.ControlKeyDataList[i].WParamTech = (tempObject.ContainsKey("WParamTech") == true) ? tempObject.Value<int>("WParamTech") : 0;
                        mControlData.ControlKeyDataList[i].KeyCodeTech = (tempObject.ContainsKey("KeyCodeTech") == true) ? tempObject.Value<int>("KeyCodeTech") : 0;
                        mControlData.ControlKeyDataList[i].ButtonType = (tempObject.ContainsKey("ButtonType") == true) ? tempObject.Value<int>("ButtonType") : 0;
                        mControlData.ControlKeyDataList[i].IsShift = (tempObject.ContainsKey("IsShift") == true) ? tempObject.Value<bool>("IsShift") : false;
                        mControlData.ControlKeyDataList[i].ControlType = (tempObject.ContainsKey("ControlType") == true) ? tempObject.Value<int>("ControlType") : 0;
                        mControlData.ControlKeyDataList[i].MemoString = (tempObject.ContainsKey("MemoString") == true) ? tempObject.Value<string>("MemoString") : "";

                        mControlKeyTextBoxList[i].Text = this.getKeyString(mControlData.ControlKeyDataList[i].WParam, (Keys)mControlData.ControlKeyDataList[i].KeyCode);
                        mTechKeyTextBoxList[i].Text = this.getKeyString(mControlData.ControlKeyDataList[i].WParamTech, (Keys)mControlData.ControlKeyDataList[i].KeyCodeTech);

                        mButtonTypeComboBoxList[i].SelectedIndex = mControlData.ControlKeyDataList[i].ButtonType;
                        mShiftCheckBoxList[i].Checked = mControlData.ControlKeyDataList[i].IsShift;
                        mControlTypeComboBoxList[i].SelectedIndex = mControlData.ControlKeyDataList[i].ControlType;
                        mMemoTextBoxList[i].Text = mControlData.ControlKeyDataList[i].MemoString;
                    }
                }

                if (rootObject.ContainsKey("ControlRepeatTime") == true)
                {
                    int delayTime = (rootObject.ContainsKey("ControlRepeatTime") == true) ? rootObject.Value<int>("ControlRepeatTime") : 30;

                    if (delayTime > 1000)
                        delayTime = 1000;
                    else if (delayTime < 20)
                        delayTime = 20;

                    mControlData.ControlRepeatTime = delayTime;
                    mControlRepeatNumericUpDown.Value = mControlData.ControlRepeatTime;
                }

                if (rootObject.ContainsKey("ShiftKey") == true)
                {
                    var shiftKeyObject = rootObject.Value<JObject>("ShiftKey");

                    mControlData.ShiftKeyData.WParam = (shiftKeyObject.ContainsKey("WParam") == true) ? shiftKeyObject.Value<int>("WParam") : HookManager.KEY_DOWN;
                    mControlData.ShiftKeyData.KeyCode = (shiftKeyObject.ContainsKey("KeyCode") == true) ? shiftKeyObject.Value<int>("KeyCode") : (int)Keys.LShiftKey;

                    mShiftKeyTextBox.Text = this.getKeyString(mControlData.ShiftKeyData.WParam, (Keys)mControlData.ShiftKeyData.KeyCode);
                    if (mShiftKeyTextBox.Text.Length == 0)
                        mShiftKeyTextBox.Text = "Shift";
                }

                if (rootObject.ContainsKey("PotionKey") == true)
                {
                    var keyArray = rootObject.Value<JArray>("PotionKey");
                    for (int i = 0; i < keyArray.Count; i++)
                    {
                        var tempObject = (JObject)keyArray[i];
                        mControlData.PotionKeyDataList[i].WParam = (tempObject.ContainsKey("WParam") == true) ? tempObject.Value<int>("WParam") : 0;
                        mControlData.PotionKeyDataList[i].KeyCode = (tempObject.ContainsKey("KeyCode") == true) ? tempObject.Value<int>("KeyCode") : 0;
                        mControlData.PotionKeyDataList[i].WParamNow = (tempObject.ContainsKey("WParamNow") == true) ? tempObject.Value<int>("WParamNow") : 0;
                        mControlData.PotionKeyDataList[i].KeyCodeNow = (tempObject.ContainsKey("KeyCodeNow") == true) ? tempObject.Value<int>("KeyCodeNow") : 0;

                        mPotionReplaceKeyTextBoxList[i].Text = this.getKeyString(mControlData.PotionKeyDataList[i].WParam, (Keys)mControlData.PotionKeyDataList[i].KeyCode);
                        mPotionNowKeyTextBoxList[i].Text = this.getKeyString(mControlData.PotionKeyDataList[i].WParamNow, (Keys)mControlData.PotionKeyDataList[i].KeyCodeNow);
                    }
                }

                if (rootObject.ContainsKey("BuffKey") == true)
                {
                    var buffObject = rootObject.Value<JObject>("BuffKey");

                    mControlData.BuffKeyData.WParam = (buffObject.ContainsKey("WParam") == true) ? buffObject.Value<int>("WParam") : 0;
                    mControlData.BuffKeyData.KeyCode = (buffObject.ContainsKey("KeyCode") == true) ? buffObject.Value<int>("KeyCode") : 0;

                    mBufStartTextBox.Text = this.getKeyString(mControlData.BuffKeyData.WParam, (Keys)mControlData.BuffKeyData.KeyCode);

                    mControlData.BuffKeyData.WParamWeapon = (buffObject.ContainsKey("WParamWeapon") == true) ? buffObject.Value<int>("WParamWeapon") : 0;
                    mControlData.BuffKeyData.KeyCodeWeapon = (buffObject.ContainsKey("KeyCodeWeapon") == true) ? buffObject.Value<int>("KeyCodeWeapon") : 0;

                    mBufChangeTextBox.Text = this.getKeyString(mControlData.BuffKeyData.WParamWeapon, (Keys)mControlData.BuffKeyData.KeyCodeWeapon);

                    if (buffObject.ContainsKey("list") == true)
                    {
                        var keyArray = buffObject.Value<JArray>("list");
                        for (int i = 0; i < keyArray.Count; i++)
                        {
                            var tempObject = (JObject)keyArray[i];
                            mControlData.BuffKeyData.WParamBufList[i] = (tempObject.ContainsKey("WParam") == true) ? tempObject.Value<int>("WParam") : 0;
                            mControlData.BuffKeyData.KeyCodeBufList[i] = (tempObject.ContainsKey("KeyCode") == true) ? tempObject.Value<int>("KeyCode") : 0;

                            mBuffKeyTextBoxList[i].Text = this.getKeyString(mControlData.BuffKeyData.WParamBufList[i], (Keys)mControlData.BuffKeyData.KeyCodeBufList[i]);
                        }
                    }

                    if (buffObject.ContainsKey("check") == true)
                    {
                        var checkArray = buffObject.Value<JArray>("check");
                        for (int i = 0; i < checkArray.Count; i++)
                        {
                            var tempObject = (JObject)checkArray[i];
                            mControlData.BuffKeyData.ChangeCheckList[i] = (tempObject.ContainsKey("Check") == true) ? tempObject.Value<bool>("Check") : false;

                            mBuffCheckBoxList[i].Checked = mControlData.BuffKeyData.ChangeCheckList[i];
                        }
                    }

                    int delayTime = (buffObject.ContainsKey("DelayTime") == true) ? buffObject.Value<int>("DelayTime") : 500;
                    
                    if (delayTime > 10000)
                        delayTime = 10000;
                    else if (delayTime < 100)
                        delayTime = 100;

                    mControlData.BuffKeyData.DelayTime = delayTime;

                    mBufNumericUpDown.Value = mControlData.BuffKeyData.DelayTime;

                    if (buffObject.ContainsKey("BufRectData") == true)
                    {
                        var bufRectDataArray = buffObject.Value<JArray>("BufRectData");
                        for (int i = 0; i < bufRectDataArray.Count; i++)
                        {
                            var tempObject = (JObject)bufRectDataArray[i];
                            mControlData.BuffKeyData.BufRectDataList[i].Display = (tempObject.ContainsKey("Display") == true) ? tempObject.Value<bool>("Display") : false;
                            mControlData.BuffKeyData.BufRectDataList[i].X = (tempObject.ContainsKey("X") == true) ? tempObject.Value<int>("X") : 0;
                            mControlData.BuffKeyData.BufRectDataList[i].Y = (tempObject.ContainsKey("Y") == true) ? tempObject.Value<int>("Y") : 0;
                            mControlData.BuffKeyData.BufRectDataList[i].Width = (tempObject.ContainsKey("Width") == true) ? tempObject.Value<int>("Width") : 0;
                            mControlData.BuffKeyData.BufRectDataList[i].Height = (tempObject.ContainsKey("Height") == true) ? tempObject.Value<int>("Height") : 0;
                            mControlData.BuffKeyData.BufRectDataList[i].ImageFile = (tempObject.ContainsKey("ImageFile") == true) ? tempObject.Value<string>("ImageFile") : "";
                            mControlData.BuffKeyData.BufRectDataList[i].EffectFile = (tempObject.ContainsKey("EffectFile") == true) ? tempObject.Value<string>("EffectFile") : "";
                            mControlData.BuffKeyData.BufRectDataList[i].EffectVolume = (tempObject.ContainsKey("EffectVolume") == true) ? tempObject.Value<int>("EffectVolume") : 50;
                            mControlData.BuffKeyData.BufRectDataList[i].BuffTime = (tempObject.ContainsKey("BuffTime") == true) ? tempObject.Value<int>("BuffTime") : 10;
                            mControlData.BuffKeyData.BufRectDataList[i].BuffFontSize = (tempObject.ContainsKey("BuffFontSize") == true) ? tempObject.Value<float>("BuffFontSize") : 14.0f;
                            mControlData.BuffKeyData.BufRectDataList[i].BuffHeightSize = (tempObject.ContainsKey("BuffHeightSize") == true) ? tempObject.Value<int>("BuffHeightSize") : 27;

                            mBuffDisplayCheckBoxList[i].Checked = mControlData.BuffKeyData.BufRectDataList[i].Display;
                        }
                    }
                }

                for (int i = 0; i < mBuffDisplayButtonList.Count; i++)
                {
                    mBuffDisplayButtonList[i].Enabled = mBuffDisplayCheckBoxList[i].Checked;
                }

                if (rootObject.ContainsKey("MercenaryArea") == true)
                {
                    var mercenaryObject = rootObject.Value<JObject>("MercenaryArea");

                    mControlData.MerceRectData.Display = (mercenaryObject.ContainsKey("Display") == true) ? mercenaryObject.Value<bool>("Display") : false;
                    mControlData.MerceRectData.Delay = (mercenaryObject.ContainsKey("Delay") == true) ? mercenaryObject.Value<int>("Delay") : 200;
                    mControlData.MerceRectData.Opacity = (mercenaryObject.ContainsKey("Opacity") == true) ? mercenaryObject.Value<int>("Opacity") : 100;
                    mControlData.MerceRectData.CapturePointX = (mercenaryObject.ContainsKey("CapturePointX") == true) ? mercenaryObject.Value<int>("CapturePointX") : 0;
                    mControlData.MerceRectData.CapturePointY = (mercenaryObject.ContainsKey("CapturePointY") == true) ? mercenaryObject.Value<int>("CapturePointY") : 0;
                    mControlData.MerceRectData.CaptureWidth = (mercenaryObject.ContainsKey("CaptureWidth") == true) ? mercenaryObject.Value<int>("CaptureWidth") : 0;
                    mControlData.MerceRectData.CaptureHeight = (mercenaryObject.ContainsKey("CaptureHeight") == true) ? mercenaryObject.Value<int>("CaptureHeight") : 0;
                    mControlData.MerceRectData.DisplayPointX = (mercenaryObject.ContainsKey("DisplayPointX") == true) ? mercenaryObject.Value<int>("DisplayPointX") : 0;
                    mControlData.MerceRectData.DisplayPointY = (mercenaryObject.ContainsKey("DisplayPointY") == true) ? mercenaryObject.Value<int>("DisplayPointY") : 0;
                    mControlData.MerceRectData.DisplayWidth = (mercenaryObject.ContainsKey("DisplayWidth") == true) ? mercenaryObject.Value<int>("DisplayWidth") : 0;
                    mControlData.MerceRectData.DisplayHeight = (mercenaryObject.ContainsKey("DisplayHeight") == true) ? mercenaryObject.Value<int>("DisplayHeight") : 0;
                }

                mMercenaryCheckBox.Checked = mControlData.MerceRectData.Display;
                mMercenaryButton.Enabled = mControlData.MerceRectData.Display;

                mControlData.ProgramName = (rootObject.ContainsKey("Program") == true) ? rootObject.Value<string>("Program") : "";
                mProgramTextBox.Text = mControlData.ProgramName;

                mOriginControlData = mControlData.clone();

                this.setDictionaryData();

                return true;
            }
            catch { }
            return false;
        }

        private void write(string fileName)
        {
            try
            {
                this.writeData(fileName);

                var rootObject = new JObject();
                rootObject["filename"] = fileName;
                File.WriteAllText(mJsonFileName, rootObject.ToString());
            }
            catch { }
        }

        private void writeData(string fileName)
        {
            try
            {
                var rootObject = new JObject();

                var startKeyArray = new JArray();
                for (int i = 0; i < mControlData.StartKeyDataList.Count; i++)
                {
                    var tempObject = new JObject();

                    var keyData = mControlData.StartKeyDataList[i];
                    tempObject["WParam"] = keyData.WParam;
                    tempObject["KeyCode"] = keyData.KeyCode;

                    startKeyArray.Add(tempObject);
                }
                rootObject["StartKey"] = startKeyArray;

                var pauseKeyArray = new JArray();
                for (int i = 0; i < mControlData.PauseKeyDataList.Count; i++)
                {
                    var tempObject = new JObject();

                    var keyData = mControlData.PauseKeyDataList[i];
                    tempObject["WParam"] = keyData.WParam;
                    tempObject["KeyCode"] = keyData.KeyCode;

                    pauseKeyArray.Add(tempObject);
                }
                rootObject["PauseKey"] = pauseKeyArray;

                var controlKeyArray = new JArray();
                for (int i = 0; i < mControlData.ControlKeyDataList.Count; i++)
                {
                    var tempObject = new JObject();

                    var keyData = mControlData.ControlKeyDataList[i];
                    tempObject["WParam"] = keyData.WParam;
                    tempObject["KeyCode"] = keyData.KeyCode;
                    tempObject["WParamTech"] = keyData.WParamTech;
                    tempObject["KeyCodeTech"] = keyData.KeyCodeTech;
                    tempObject["ButtonType"] = keyData.ButtonType;
                    tempObject["IsShift"] = keyData.IsShift;
                    tempObject["ControlType"] = keyData.ControlType;
                    tempObject["MemoString"] = keyData.MemoString;

                    controlKeyArray.Add(tempObject);
                }
                rootObject["ControlKey"] = controlKeyArray;

                rootObject["ControlRepeatTime"] = mControlData.ControlRepeatTime;

                var shiftKeyObject = new JObject();
                shiftKeyObject["WParam"] = mControlData.ShiftKeyData.WParam;
                shiftKeyObject["KeyCode"] = mControlData.ShiftKeyData.KeyCode;
                rootObject["ShiftKey"] = shiftKeyObject;

                var potionKeyArray = new JArray();
                for (int i = 0; i < mControlData.PotionKeyDataList.Count; i++)
                {
                    var tempObject = new JObject();

                    var keyData = mControlData.PotionKeyDataList[i];
                    tempObject["WParam"] = keyData.WParam;
                    tempObject["KeyCode"] = keyData.KeyCode;
                    tempObject["WParamNow"] = keyData.WParamNow;
                    tempObject["KeyCodeNow"] = keyData.KeyCodeNow;

                    potionKeyArray.Add(tempObject);
                }
                rootObject["PotionKey"] = potionKeyArray;

                var buffKeyObject = new JObject();
                buffKeyObject["WParam"] = mControlData.BuffKeyData.WParam;
                buffKeyObject["KeyCode"] = mControlData.BuffKeyData.KeyCode;
                buffKeyObject["WParamWeapon"] = mControlData.BuffKeyData.WParamWeapon;
                buffKeyObject["KeyCodeWeapon"] = mControlData.BuffKeyData.KeyCodeWeapon;
                
                var buffKeyArray = new JArray();
                for (int i = 0; i < mControlData.BuffKeyData.WParamBufList.Count; i++)
                {
                    var tempObject = new JObject();
                    tempObject["WParam"] = mControlData.BuffKeyData.WParamBufList[i];
                    tempObject["KeyCode"] = mControlData.BuffKeyData.KeyCodeBufList[i];

                    buffKeyArray.Add(tempObject);
                }
                buffKeyObject["list"] = buffKeyArray;

                var buffCheckArray = new JArray();
                for (int i = 0; i < mControlData.BuffKeyData.ChangeCheckList.Count; i++)
                {
                    var tempObject = new JObject();
                    tempObject["Check"] = mControlData.BuffKeyData.ChangeCheckList[i];

                    buffCheckArray.Add(tempObject);
                }
                buffKeyObject["check"] = buffCheckArray;

                buffKeyObject["DelayTime"] = mControlData.BuffKeyData.DelayTime;

                var bufRectDataArray = new JArray();
                for (int i = 0; i < mControlData.BuffKeyData.BufRectDataList.Count; i++)
                {
                    var tempObject = new JObject();
                    tempObject["Display"] = mControlData.BuffKeyData.BufRectDataList[i].Display;
                    tempObject["X"] = mControlData.BuffKeyData.BufRectDataList[i].X;
                    tempObject["Y"] = mControlData.BuffKeyData.BufRectDataList[i].Y;
                    tempObject["Width"] = mControlData.BuffKeyData.BufRectDataList[i].Width;
                    tempObject["Height"] = mControlData.BuffKeyData.BufRectDataList[i].Height;
                    tempObject["ImageFile"] = mControlData.BuffKeyData.BufRectDataList[i].ImageFile;
                    tempObject["EffectFile"] = mControlData.BuffKeyData.BufRectDataList[i].EffectFile;
                    tempObject["EffectVolume"] = mControlData.BuffKeyData.BufRectDataList[i].EffectVolume;
                    tempObject["BuffTime"] = mControlData.BuffKeyData.BufRectDataList[i].BuffTime;
                    tempObject["BuffFontSize"] = mControlData.BuffKeyData.BufRectDataList[i].BuffFontSize;
                    tempObject["BuffHeightSize"] = mControlData.BuffKeyData.BufRectDataList[i].BuffHeightSize;
                    bufRectDataArray.Add(tempObject);
                }
                buffKeyObject["BufRectData"] = bufRectDataArray;
                rootObject["BuffKey"] = buffKeyObject;

                var mercenaryAreaObject = new JObject();
                mercenaryAreaObject["Display"] = mControlData.MerceRectData.Display;
                mercenaryAreaObject["Delay"] = mControlData.MerceRectData.Delay;
                mercenaryAreaObject["Opacity"] = mControlData.MerceRectData.Opacity;
                mercenaryAreaObject["CapturePointX"] = mControlData.MerceRectData.CapturePointX;
                mercenaryAreaObject["CapturePointY"] = mControlData.MerceRectData.CapturePointY;
                mercenaryAreaObject["CaptureWidth"] = mControlData.MerceRectData.CaptureWidth;
                mercenaryAreaObject["CaptureHeight"] = mControlData.MerceRectData.CaptureHeight;
                mercenaryAreaObject["DisplayPointX"] = mControlData.MerceRectData.DisplayPointX;
                mercenaryAreaObject["DisplayPointY"] = mControlData.MerceRectData.DisplayPointY;
                mercenaryAreaObject["DisplayWidth"] = mControlData.MerceRectData.DisplayWidth;
                mercenaryAreaObject["DisplayHeight"] = mControlData.MerceRectData.DisplayHeight;
                rootObject["MercenaryArea"] = mercenaryAreaObject;

                rootObject["Program"] = mControlData.ProgramName;

                File.WriteAllText(fileName, rootObject.ToString());

                mOriginControlData = mControlData.clone();
            }
            catch { }
        }

        private void newUI()
        {
            foreach (var temp in mKeyTextBoxList)
            {
                temp.Text = "";
            }

            mShiftKeyTextBox.Text = "Shift";

            foreach (var temp in mButtonTypeComboBoxList)
            {
                temp.SelectedIndex = 0;
            }

            foreach (var temp in mShiftCheckBoxList)
            {
                temp.Checked = false;
            }

            foreach (var temp in mControlTypeComboBoxList)
            {
                temp.SelectedIndex = 0;
            }

            foreach (var temp in mMemoTextBoxList)
            {
                temp.Text = "";
            }

            mControlRepeatNumericUpDown.Value = 30;

            foreach (var temp in mPotionNowKeyTextBoxList)
            {
                temp.Text = "";
            }

            foreach (var temp in mPotionReplaceKeyTextBoxList)
            {
                temp.Text = "";
            }

            mBufStartTextBox.Text = "";
            mBufChangeTextBox.Text = "";
            foreach (var temp in mBuffKeyTextBoxList)
            {
                temp.Text = "";
            }
            foreach (var temp in mBuffCheckBoxList)
            {
                temp.Checked = false;
            }
            mBufNumericUpDown.Value = 500;
            foreach (var temp in mBuffDisplayCheckBoxList)
            {
                temp.Checked = false;
            }

            for (int i = 0; i < mBuffDisplayButtonList.Count; i++)
            {
                mBuffDisplayButtonList[i].Enabled = mBuffCheckBoxList[i].Checked;
            }

            mMercenaryCheckBox.Checked = false;
            mMercenaryButton.Enabled = false;

            mProgramTextBox.Text = "Diablo II: Resurrected";
            this.Text = "AJeHelperD2R(v" + Application.ProductVersion + ")";
        }

        private void onNewButtonClick(object sender, EventArgs e)
        {
            this.startAndStopControl(true);

            mControlData.clear();
            mOriginControlData.clear();

            this.newUI();

            mIsSetKey = false;
            mWParam = HookManager.NONE;
            mKeyCode = (int)Keys.None;
            mPauseWParam = HookManager.NONE;
            mPauseKeyCode = (int)Keys.None;
            mIsStart = false;
            mIsPause = false;
            mIsBuff = false;

            mControlData.ProgramName = mProgramTextBox.Text;

            try
            {
                var rootObject = new JObject();
                rootObject["filename"] = "";
                File.WriteAllText(mJsonFileName, rootObject.ToString());
            }
            catch { }

            this.setDictionaryData();
        }

        private void onSaveButtonClick(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Title = StringLib.Save;
            sfd.Filter = "AJHD2R file (*.ad2) | *.ad2;";
            var dr = sfd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string fileName = sfd.FileName;
                this.write(fileName);
                this.Text = "AJeHelperD2R(v" + Application.ProductVersion + ") - " + Path.GetFileName(fileName);
            }
        }

        private void onLoadButtonClick(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Title = StringLib.Load;
            ofd.Filter = "AJHD2R file (*.ad2) | *.ad2;";
            var dr = ofd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                mControlData.clear();
                mOriginControlData.clear();
                this.newUI();

                string fileName = ofd.FileName;
                if (this.readData(fileName) == true)
                {
                    this.Text = "AJeHelperD2R(v" + Application.ProductVersion + ") - " + Path.GetFileName(fileName);

                    try
                    {
                        var rootObject = new JObject();
                        rootObject["filename"] = fileName;
                        File.WriteAllText(mJsonFileName, rootObject.ToString());
                    }
                    catch { }
                }
            }
        }

        private void onDonateButtonClick(object sender, EventArgs e)
        {
            string localString = StringLib.Localize;
            if (localString.CompareTo("ko") == 0)
            {
                var form = new DonateForm();
                form.ShowDialog();
            }
            else
            {
                System.Diagnostics.Process.Start("https://www.buymeacoffee.com/lich");
            }            
        }

        private void onBufButtonClick(int index)
        {
            mIsArea = true;

            this.Visible = false;
            this.ShowInTaskbar = false;
            this.Hide();

            var areaForm = new AreaForm(mControlData, index);
            areaForm.onClosingCallback += (object sender2, EventArgs e2) =>
            {
                this.BeginInvoke(new Action(delegate ()
                {
                    mIsArea = false;
                    this.ShowInTaskbar = true;
                    this.Visible = true;
                    this.Activate();

                    areaForm.Dispose();
                }));
            };
            areaForm.Show(this);
        }
        
        private void onBufDisplayButton1Click(object sender, EventArgs e)
        {
            this.onBufButtonClick(0);
        }

        private void onBufDisplayButton2Click(object sender, EventArgs e)
        {
            this.onBufButtonClick(1);
        }

        private void onBufDisplayButton3Click(object sender, EventArgs e)
        {
            this.onBufButtonClick(2);
        }

        private void onBufDisplayButton4Click(object sender, EventArgs e)
        {
            this.onBufButtonClick(3);
        }

        private void onBufDisplayButton5Click(object sender, EventArgs e)
        {
            this.onBufButtonClick(4);
        }

        private void onBufDisplayButton6Click(object sender, EventArgs e)
        {
            this.onBufButtonClick(5);
        }

        private void onBufDisplayButton7Click(object sender, EventArgs e)
        {
            this.onBufButtonClick(6);
        }

        private void onBufDisplayButton8Click(object sender, EventArgs e)
        {
            this.onBufButtonClick(7);
        }

        private void onBufDisplayButton9Click(object sender, EventArgs e)
        {
            this.onBufButtonClick(8);
        }

        private void onBufDisplayButton10Click(object sender, EventArgs e)
        {
            this.onBufButtonClick(9);
        }

        private void onMercenaryButtonClick(object sender, EventArgs e)
        {
            mIsArea = true;

            this.Visible = false;
            this.ShowInTaskbar = false;
            this.Hide();

            var areaForm = new MercenaryAreaForm(mControlData);
            areaForm.onClosingCallback += (object sender2, EventArgs e2) =>
            {
                this.BeginInvoke(new Action(delegate ()
                {
                    mIsArea = false;
                    this.ShowInTaskbar = true;
                    this.Visible = true;
                    this.Activate();

                    areaForm.Dispose();
                }));
            };
            areaForm.Show(this);
        }

        private void debugPrintln(string printString)
        {
            //Console.WriteLine(printString);
        }

        
    }
}
