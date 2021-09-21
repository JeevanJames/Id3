using System;
using System.Collections.Generic;
using System.Text;

namespace Id3
{
    public class WriteTagOptions
    {
        public WriteConflictAction ConflictAction { get; set; } = WriteConflictAction.NoAction;

        public bool WipeOut { get; set; } = true;

        public bool ShrinkFile { get; set; } = true;

        public int ShrinkFileThreshold { get; set; } = 0;
    }
}
