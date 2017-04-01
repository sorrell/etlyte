using System;
using System.IO;
using System.Text;

namespace ETLyteExe
{
    public class InputErrorEventArgs : EventArgs
    {
        public InputErrorEventArgs(string s) { error = s; }
        public string error { get; set; }
    }
    class CustomTextWriter : TextWriter
    {
        private string lastInput { get; set; }
        public CustomTextWriter() { }
        public event EventHandler InputErrorOnUsing;

        protected virtual void OnInputErrorOnUsing(EventArgs e)
        {
            EventHandler handler = InputErrorOnUsing;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public override void Write(string value)
        {
            lastInput = value;
            Console.Write(value);
            if (value.IndexOf("using", StringComparison.OrdinalIgnoreCase) >= 0)
                OnInputErrorOnUsing(new InputErrorEventArgs(value));

        }
        public override void WriteLine(string value)
        {
            lastInput = value;
            Console.WriteLine(value);
            if (value.IndexOf("using", StringComparison.OrdinalIgnoreCase) >= 0)
                OnInputErrorOnUsing(new InputErrorEventArgs(value));
        }
        public override Encoding Encoding
        {
            get
            {
                return Encoding.Default;
            }
        }
    }
}
