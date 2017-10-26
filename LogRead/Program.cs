using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using LogRead.Plan_C.Arithmetics;
using System.Threading;
using LogRead.Plan_C.Entitys;

namespace LogRead
{
    public class Program
    {
        private static long Position = 0L;
        public static void Main(string[] args)
        {
            Arithmetic ar = new Arithmetic();

            //Thread t1 = new Thread(new ThreadStart(ar.ListLine));
            //Thread t2 = new Thread(new ThreadStart(ar.HospStatistics));
            //t1.Start();
            //t2.Start();

            ar.PhoneSendCounts(new List<LogEntity>());
        }

    }

}
