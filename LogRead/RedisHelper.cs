using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis;

namespace LogRead
{
    public class RedisHelper
    {

        public void Set(Dictionary<string, object> dic)
        {
            using (RedisClient client = new RedisClient())
            {
                client.Set(dic["key"].ToString(), dic);
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
