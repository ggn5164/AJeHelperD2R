using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AJeHelperD2R
{
    public class BufKeyData : KeyData
    {
        // 무기 교체
        public int WParamWeapon { get; set; } = HookManager.NONE;

        public int KeyCodeWeapon { get; set; } = (int)Keys.None;

        // 버프 키
        public List<int> WParamBufList { get; set; } = new List<int>();
        public List<int> KeyCodeBufList { get; set; } = new List<int>();

        // 체크박스
        public List<bool> ChangeCheckList { get; set; } = new List<bool>();

        // 딜레이
        public int DelayTime { get; set; } = 500;

        // 버프 표시
        public List<BufRectData> BufRectDataList { get; set; } = new List<BufRectData>();

        public BufKeyData()
        {
            for (int i = 0; i < 10; i++)
            {
                WParamBufList.Add(HookManager.NONE);
                KeyCodeBufList.Add((int)Keys.None);
            }

            for (int i = 0; i < 11; i++)
            {
                ChangeCheckList.Add(false);
            }

            for (int i = 0; i < 10; i++)
            {
                BufRectDataList.Add(new BufRectData());
            }
        }

        public override void clear(int index)
        {
            Index = index;
            Type = (int)TYPE.BUF;
            WParam = HookManager.NONE;
            KeyCode = (int)Keys.None;
            WParamWeapon = HookManager.NONE;
            KeyCodeWeapon = (int)Keys.None;

            for (int i = 0; i < WParamBufList.Count; i++)
            {
                WParamBufList[i] = HookManager.NONE;
                KeyCodeBufList[i] = (int)Keys.None;
            }

            for (int i = 0; i < ChangeCheckList.Count; i++)
            {
                ChangeCheckList[i] = false;
            }

            DelayTime = 500;

            for (int i = 0; i < BufRectDataList.Count; i++)
            {
                BufRectDataList[i].clear();
            }
        }

        public override KeyData clone()
        {
            var keyData = new BufKeyData();
            keyData.Index = this.Index;
            keyData.Type = this.Type;
            keyData.WParam = this.WParam;
            keyData.KeyCode = this.KeyCode;
            keyData.WParamWeapon = this.WParamWeapon;
            keyData.KeyCodeWeapon = this.KeyCodeWeapon;

            for (int i = 0; i < this.WParamBufList.Count; i++)
            {
                keyData.WParamBufList[i] = this.WParamBufList[i];
                keyData.KeyCodeBufList[i] = this.KeyCodeBufList[i];
            }

            for (int i = 0; i < this.ChangeCheckList.Count; i++)
            {
                keyData.ChangeCheckList[i] = this.ChangeCheckList[i];
            }

            keyData.DelayTime = this.DelayTime;

            for (int i = 0; i < BufRectDataList.Count; i++)
            {
                keyData.BufRectDataList[i] = BufRectDataList[i].clone();
            }

            return keyData;
        }

        public override bool isChangeData(KeyData keyData)
        {
            var bufKeyData = (BufKeyData)keyData;

            for (int i = 0; i < this.WParamBufList.Count; i++)
            {
                if (bufKeyData.WParamBufList[i] != this.WParamBufList[i] ||
                    bufKeyData.KeyCodeBufList[i] != this.KeyCodeBufList[i])
                {
                    return true;
                }
            }

            for (int i = 0; i < this.ChangeCheckList.Count; i++)
            {
                if (bufKeyData.ChangeCheckList[i] != this.ChangeCheckList[i])
                {
                    return true;
                }
            }

            for (int i = 0; i < this.BufRectDataList.Count; i++)
            {
                if (bufKeyData.BufRectDataList[i].isChangeData(this.BufRectDataList[i]) == true)
                {
                    return true;
                }
            }

            return (Type != bufKeyData.Type ||
                WParam != bufKeyData.WParam ||
                KeyCode != bufKeyData.KeyCode ||
                WParamWeapon != bufKeyData.WParamWeapon ||
                KeyCodeWeapon != bufKeyData.KeyCodeWeapon ||
                DelayTime != bufKeyData.DelayTime);
        }
    }
}
