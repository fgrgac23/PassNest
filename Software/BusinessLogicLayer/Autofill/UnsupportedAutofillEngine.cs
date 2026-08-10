using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Autofill
{
    public class UnsupportedAutofillEngine : IAutofillEngine
    {
        public bool IsSupported => false;
        public event Action? HotkeyPressed;
        public void RegisterHotkeys()
        {
        }
        public void UnregisterHotkeys()
        {
        }
        public bool TriggerAutofill(int accountId)
        {
            return false;
        }
    }
}
