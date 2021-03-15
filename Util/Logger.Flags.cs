namespace txtrconvert.Util
{
    public partial class Logger
    {
        public enum Flags : int
        {
            NONE            = 0b00000000_00000000_00000000_00000000,
            DEBUG           = 0b00000000_00000000_00000000_00000001,
            DEBUGCONSOLE    = 0b00000000_00000000_00000000_00000010,
            CLICONSOLE      = 0b00000000_00000000_00000000_00000100,
            CLICOLORS       = 0b00000000_00000000_00000000_00001000,
            LOGFILE         = 0b00000000_00000000_00000000_00010000,
            ALL             = 0b00000000_00000000_00000000_00011111
        }
    }
}
