using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AJeHelperD2R
{
    public class ControlKeyData : KeyData
    {
        public enum BUTTON_TYPE
        {
            LEFT = 0,
            RIGHT = 1,
        };

        public enum CONTROL_TYPE
        {
            REPEAT_TOGGLE = 0,
            REPEAT_HOLDING = 1,
            HOLDING_TOGGLE = 2,
            HOLDING_HOLDING = 3,
            ONE = 4,
        };

        public int WParamTech { get; set; } = HookManager.NONE;

        public int KeyCodeTech { get; set; } = (int)Keys.None;

        public int ButtonType { get; set; } = (int)BUTTON_TYPE.LEFT;

        public bool IsShift { get; set; } = false;

        public int ControlType { get; set; } = (int)CONTROL_TYPE.REPEAT_TOGGLE;

        public string MemoString { get; set; } = "";

        public override void clear(int index)
        {
            Index = index;
            Type = (int)TYPE.CONTROL;
            WParam = HookManager.NONE;
            KeyCode = (int)Keys.None;
            WParamTech = HookManager.NONE;
            KeyCodeTech = (int)Keys.None;
            ButtonType = (int)BUTTON_TYPE.LEFT;
            IsShift = false;
            ControlType = (int)CONTROL_TYPE.REPEAT_TOGGLE;
            MemoString = "";
        }

        public override KeyData clone()
        {
            var keyData = new ControlKeyData();
            keyData.Index = this.Index;
            keyData.Type = this.Type;
            keyData.WParam = this.WParam;
            keyData.KeyCode = this.KeyCode;
            keyData.WParamTech = this.WParamTech;
            keyData.KeyCodeTech = this.KeyCodeTech;
            keyData.ButtonType = this.ButtonType;
            keyData.IsShift = this.IsShift;
            keyData.ControlType = this.ControlType;
            keyData.MemoString = this.MemoString;
            return keyData;
        }

        public override bool isChangeData(KeyData keyData)
        {
            var controlKeyData = (ControlKeyData)keyData;
            return (Type != controlKeyData.Type ||
                WParam != controlKeyData.WParam ||
                KeyCode != controlKeyData.KeyCode ||
                WParamTech != controlKeyData.WParamTech ||
                KeyCodeTech != controlKeyData.KeyCodeTech ||
                ButtonType != controlKeyData.ButtonType ||
                IsShift != controlKeyData.IsShift ||
                ControlType != controlKeyData.ControlType ||
                MemoString.CompareTo(controlKeyData.MemoString) != 0);
        }
    }
}
