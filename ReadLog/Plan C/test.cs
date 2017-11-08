using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadLog.Plan_C
{
    public class test : RedisOperatorBase
    {
        public void TestFun()
        {
            Stopwatch sw = Stopwatch.StartNew();
           
            sw.Stop();
            long test = sw.ElapsedMilliseconds;
           
        }
    }
}
