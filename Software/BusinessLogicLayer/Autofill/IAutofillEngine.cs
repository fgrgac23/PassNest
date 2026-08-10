using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Autofill
{
    public interface IAutofillEngine
    {
        bool IsSupported { get; }

        event Action? HotkeyPressed;

        void RegisterHotkeys();
        void UnregisterHotkeys();
        bool TriggerAutofill(int accountId);
    }
}
