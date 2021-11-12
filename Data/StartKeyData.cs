using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AJeHelperD2R
{
    public class StartKeyData : KeyData
    {
        public override void clear(int index)
        {
            Index = index;
            Type = (int)TYPE.START;
            WParam = HookManager.NONE;
            KeyCode = (int)Keys.None;
        }

        public override KeyData clone()
        {
            var keyData = new StartKeyData();
            keyData.Index = this.Index;
            keyData.Type = this.Type;
            keyData.WParam = this.WParam;
            keyData.KeyCode = this.KeyCode;
            return keyData;
        }
    }
}
