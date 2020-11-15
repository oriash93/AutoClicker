using System.Windows.Input;

namespace AutoClicker.Utils
{
    public class Hotkey
    {
        public Key Key { get; set; }
        public int VirtualCode { get; set; }

        private Hotkey(Key key, int virtualCode)
        {
            Key = key;
            VirtualCode = virtualCode;
        }

        public Hotkey(Key key) : this(key, KeyInterop.VirtualKeyFromKey(key)) { }
    }
}
