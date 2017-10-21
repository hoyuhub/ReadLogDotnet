using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogRead.Plan_C.Entitys
{
   public class Counts
    {
        public Counts() { }
        public Counts(string url, string hospid, int count, DateTime time)
        {
            this.url = url;
            this.hospid = hospid;
            this.count = count;
            this.time = time;
        }
        public string url { get; set; }
        public string hospid { get; set; }
        public DateTime time { get; set; }
        public int count { get; set; }

    }
}
