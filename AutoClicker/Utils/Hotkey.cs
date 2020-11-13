using System.Windows.Input;

namespace AutoClicker.Utils
{
    public enum Operation
    {
        Start,
        Stop
    }

    public class Hotkey
    {
        public Key Key { get; set; }
        public int VirtualCode { get; set; }
        public Operation Operation { get; set; }

        public Hotkey(Key key, int virtualCode, Operation operation)
        {
            Key = key;
            VirtualCode = virtualCode;
            Operation = operation;
        }

        public Hotkey(Key key, Operation operation) : this(key, KeyInterop.VirtualKeyFromKey(key), operation) { }
    }
}
