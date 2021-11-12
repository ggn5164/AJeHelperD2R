using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AJeHelperD2R
{
    public class KeyData
    {
        public enum TYPE
        {
            START = 0,
            PAUSE = 1,
            CONTROL = 2,
            BUF = 3,
            POSION = 4,
        };

        public int Index { get; set; } = 0;

        public int Type { get; set; } = (int)TYPE.START;

        public int WParam { get; set; } = HookManager.NONE;

        public int KeyCode { get; set; } = (int)Keys.None;

        public virtual void clear(int index)
        {
            Index = index;
            Type = (int)TYPE.START;
            WParam = HookManager.NONE;
            KeyCode = (int)Keys.None;
        }

        public virtual KeyData clone()
        {
            var keyData = new KeyData();
            keyData.Index = this.Index;
            keyData.Type = this.Type;
            keyData.WParam = this.WParam;
            keyData.KeyCode = this.KeyCode;
            return keyData;
        }

        public virtual bool isChangeData(KeyData keyData)
        {
            return (Type != keyData.Type ||
                WParam != keyData.WParam ||
                KeyCode != keyData.KeyCode);
        }
    }
}
