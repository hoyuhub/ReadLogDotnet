using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogRead
{
    public class FileReadPoint
    {
        private long startPoint = 0L;
        private long readCount = 1L;
        public long StartPoint { get => startPoint; set => startPoint = value; }
        public long ReadCount { get => readCount; set { if (value >= 1) { readCount = value; } } }
    }
}
