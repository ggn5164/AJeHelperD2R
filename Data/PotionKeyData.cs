using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AJeHelperD2R
{
    public class PotionKeyData : KeyData
    {
        public int WParamNow { get; set; } = HookManager.NONE;

        public int KeyCodeNow { get; set; } = (int)Keys.None;

        public override void clear(int index)
        {
            Index = index;
            Type = (int)TYPE.POSION;
            WParam = HookManager.NONE;
            KeyCode = (int)Keys.None;
            WParamNow = HookManager.NONE;
            KeyCodeNow = (int)Keys.None;
        }

        public override KeyData clone()
        {
            var keyData = new PotionKeyData();
            keyData.Index = this.Index;
            keyData.Type = this.Type;
            keyData.WParam = this.WParam;
            keyData.KeyCode = this.KeyCode;
            keyData.WParamNow = this.WParamNow;
            keyData.KeyCodeNow = this.KeyCodeNow;
            return keyData;
        }
    }
}
