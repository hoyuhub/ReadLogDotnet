﻿using LogRead.Plan_C.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogRead.Plan_C
{
    public class CountDal
    {
        RedisHelper redis = new RedisHelper();
        public void InsertDayCount(List<Count> list)
        {
            foreach (Count c in list)
            {
                string logkey = "log_" + c.hospid + "_" + c.url + "_" + c.daytime;
                int count = redis.Get(logkey);
                if (count == 0)
                {

                }
            }
        }
    }
}