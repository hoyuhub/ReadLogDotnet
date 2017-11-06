using LogRead.Plan_C.Entitys;
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
                    redis.Add(logkey, c.count);
                }
                else
                {
                    long l = redis.Delete(logkey);
                    redis.Add(logkey, (c.count + count));
                }
            }
        }


        public void InsertPhoneCount(List<Count> list)
        {
            foreach (Count c in list)
            {
                string logkey = "log_" + c.phone + "_" + c.minutetime;
                int count = redis.Get(logkey);
                if (count == 0)
                {
                    redis.Add(logkey, c.count);
                }
                else
                {
                    long l = redis.Delete(logkey);
                    redis.Add(logkey, (c.count + count));
                }
            }
        }


        public void InsertSecondCount(List<Count> list)
        {
            foreach (Count c in list)
            {
                string logkey = "log_" + c.url + "_" + c.hospid + "_" + c.time;
                int count = redis.Get(logkey);
                if (count == 0)
                {
                    redis.Add(logkey, c.count);
                }
                else
                {
                    long l = redis.Delete(logkey); ;
                    redis.Add(logkey, (c.count + count));
                }
            }
        }
    }
}
