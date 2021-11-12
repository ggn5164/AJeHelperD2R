using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AJeHelperD2R
{
    public class ControlData
    {
        public List<StartKeyData> StartKeyDataList { get; set; } = new List<StartKeyData>();
        
        public List<PauseKeyData> PauseKeyDataList { get; set; } = new List<PauseKeyData>();

        public List<ControlKeyData> ControlKeyDataList { get; set; } = new List<ControlKeyData>();

        public KeyData ShiftKeyData { get; set; } = new KeyData();

        // 반복 딜레이
        public int ControlRepeatTime { get; set; } = 30;

        public BufKeyData BuffKeyData { get; set; } = new BufKeyData();

        public List<PotionKeyData> PotionKeyDataList { get; set; } = new List<PotionKeyData>();

        // 용병 표시
        public MercenaryRectData MerceRectData = new MercenaryRectData();

        public string ProgramName { get; set; } = "";

        public ControlData()
        {
            for (int i = 0; i < 4; i++)
            {
                StartKeyDataList.Add(new StartKeyData());
            }

            for (int i = 0; i < 7; i++)
            {
                PauseKeyDataList.Add(new PauseKeyData());
            }

            for (int i = 0; i < 12; i++)
            {
                ControlKeyDataList.Add(new ControlKeyData());
            }

            ShiftKeyData.clear(0);
            ShiftKeyData.WParam = HookManager.WM_KEYDOWN;
            ShiftKeyData.KeyCode = (int)Keys.LShiftKey;

            ControlRepeatTime = 30;

            for (int i = 0; i < 4; i++)
            {
                PotionKeyDataList.Add(new PotionKeyData());
            }

            MerceRectData.clear();

            this.clear();
        }

        public void clear()
        {
            int index = 0;
            foreach (var data in StartKeyDataList)
            {
                data.clear(index++);
            }

            index = 0;
            foreach (var data in PauseKeyDataList)
            {
                data.clear(index++);
            }

            index = 0;
            foreach (var data in ControlKeyDataList)
            {
                data.clear(index++);
            }

            ShiftKeyData.clear(0);
            ShiftKeyData.WParam = HookManager.WM_KEYDOWN;
            ShiftKeyData.KeyCode = (int)Keys.LShiftKey;

            ControlRepeatTime = 30;

            index = 0;
            foreach (var data in PotionKeyDataList)
            {
                data.clear(index++);
            }

            BuffKeyData.clear(0);

            MerceRectData.clear();

            ProgramName = "Diablo II: Resurrected";
        }

        public ControlData clone()
        {
            var controlData = new ControlData();

            for (int i = 0; i < StartKeyDataList.Count; i++)
            {
                controlData.StartKeyDataList[i] = (StartKeyData)StartKeyDataList[i].clone();
            }

            for (int i = 0; i < PauseKeyDataList.Count; i++)
            {
                controlData.PauseKeyDataList[i] = (PauseKeyData)PauseKeyDataList[i].clone();
            }

            for (int i = 0; i < ControlKeyDataList.Count; i++)
            {
                controlData.ControlKeyDataList[i] = (ControlKeyData)ControlKeyDataList[i].clone();
            }

            controlData.ShiftKeyData = this.ShiftKeyData.clone();
            controlData.ControlRepeatTime = this.ControlRepeatTime;

            for (int i = 0; i < PotionKeyDataList.Count; i++)
            {
                controlData.PotionKeyDataList[i] = (PotionKeyData)PotionKeyDataList[i].clone();
            }

            controlData.BuffKeyData = (BufKeyData)BuffKeyData.clone();

            controlData.MerceRectData = MerceRectData.clone();

            controlData.ProgramName = ProgramName;

            return controlData;
        }

        public bool isChangeData(ControlData controlData)
        {
            for (int i = 0; i < StartKeyDataList.Count; i++)
            {
                if (StartKeyDataList[i].isChangeData(controlData.StartKeyDataList[i]) == true)
                    return true;
            }

            for (int i = 0; i < PauseKeyDataList.Count; i++)
            {
                if (PauseKeyDataList[i].isChangeData(controlData.PauseKeyDataList[i]) == true)
                    return true;
            }

            for (int i = 0; i < ControlKeyDataList.Count; i++)
            {
                if (ControlKeyDataList[i].isChangeData(controlData.ControlKeyDataList[i]) == true)
                    return true;
            }

            if (ShiftKeyData.isChangeData(controlData.ShiftKeyData) == true)
                return true;

            if (ControlRepeatTime != controlData.ControlRepeatTime)
                return true;

            for (int i = 0; i < PotionKeyDataList.Count; i++)
            {
                if (PotionKeyDataList[i].isChangeData(controlData.PotionKeyDataList[i]) == true)
                    return true;
            }

            if (BuffKeyData.isChangeData(controlData.BuffKeyData) == true)
                return true;

            if (MerceRectData.isChangeData(controlData.MerceRectData) == true)
                return true;

            if (ProgramName.Length != controlData.ProgramName.Length || ProgramName.CompareTo(controlData.ProgramName) != 0)
                return true;

            return false;
        }
    }
}
