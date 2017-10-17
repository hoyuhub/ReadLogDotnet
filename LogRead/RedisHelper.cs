using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Redis;

namespace LogRead
{
    public class RedisHelper
    {

        public void Set(Dictionary<string,object> dic)
        {
            using (RedisClient client = new RedisClient())
            {
                client.Set("15:44",dic);
            }
        }


        public Dictionary<string,object> Get()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            using (RedisClient client = new RedisClient())
            {
                result = client.Get<Dictionary<string,object>>("15:44");
            }
            return result;
        }
    }
}
