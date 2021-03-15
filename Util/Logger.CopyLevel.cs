﻿namespace txtrconvert.Util
{
    public partial class Logger
    {
        public enum CopyLevel : int
        {
            NONE        = 0b00000000_00000000_00000000_00000000,
            TAG         = 0b00000000_00000000_00000000_00000001,
            LEVEL       = 0b00000000_00000000_00000000_00000010,
            FLAG        = 0b00000000_00000000_00000000_00000100,
            INDENTSEQ   = 0b00000000_00000000_00000000_00001000,
            INDENTLVL   = 0b00000000_00000000_00000000_00010000,
            LOGFILE     = 0b00000000_00000000_00000000_00100000,
            ALL         = 0b00000000_00000000_00000000_00111111
        }
    }
}
