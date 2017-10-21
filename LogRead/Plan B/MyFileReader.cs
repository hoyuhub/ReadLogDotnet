using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogRead
{

    public class MyFileReader :FileReader
    {
        public MyFileReader(string path) :base(path){ }

        protected override void DoFileRead(ThreadStream threadStream)
        {
            throw new NotImplementedException();
        }

        protected override void GetPoint(FileReadPoint point, FileStream stream, long length)
        {
            throw new NotImplementedException();
        }
    }
}
