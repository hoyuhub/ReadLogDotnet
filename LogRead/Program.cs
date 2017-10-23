using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using LogRead.Plan_C.Arithmetics;
using System.Threading;

namespace LogRead
{
    public class Program
    {
        private static long Position = 0L;
        public static void Main(string[] args)
        {
            string path = "e:\\logfile.log";
            Arithmetic ar = new Arithmetic();
         
            ar.GetLogEntitys( ar.ListLine(path, ref Position));
            Program p = new Program();
            Thread t1 = new Thread(new ThreadStart());

        }

    }

}
