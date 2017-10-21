using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using LogRead.Plan_C.Arithmetics;

namespace LogRead
{
    public class Program
    {
        private static long Position = 0L;
        public static void Main(string[] args)
        {
            string path = "e:\\logfile.log";
            Arithmetic ar = new Arithmetic();
            while (true)
            {
            ar.ListLine(path,ref Position);
            }
        }
       
    }

}
