using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis;

namespace LogRead
{
    public class RedisHelper
    {

        public int Incr(string name)
        {
            using (RedisClient c = new RedisClient())
            {
                long l = c.Incr(name);
                return Convert.ToInt32(l);
            }
        }

        public void Set(Dictionary<string, object> dic)
        {
            using (RedisClient client = new RedisClient())
            {
                client.Set(dic["key"].ToString(), dic);
            }
        }

        public bool Add(string key, int value)
        {
            using (RedisClient client = new RedisClient())
            {
                return client.Add(key, value);
            }
        }

        public long Delete(string key)
        {
            using (RedisClient client = new RedisClient())
            {
                return client.Del(key);
            }
        }

        public int Get(string key)
        {
            int result = 0;
            using (RedisClient client = new RedisClient())
            {
                result = client.Get<int>(key);
            }
            return result;
        }

        public bool Insert(Dictionary<string, string> dic)
        {
            using (RedisClient client = new RedisClient())
            {

                return client.Set(dic["key"], dic["value"]);
            }

        }
    }
}
