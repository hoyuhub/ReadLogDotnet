using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace LogRead
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RedisHelper r = new RedisHelper();
            MyFileReader file = new MyFileReader("e://20171011.txt");
            file.Create();
            //Dictionary<string, object> dic = new Dictionary<string, object>();
            ////dic.Add("counts", HospStatistics(Read()));
            ////dic.Add("phone", PhoneSendCounts(Read(), 1));
            ////r.Set(dic);
            //dic = r.Get();
        }

        //需要指定文件路径（包括文件名称）
        public static List<Entity> Read()
        {
            string path = "e://20171011.txt";
            string line = string.Empty;
            List<Entity> list = new List<Entity>();
            //获取文件流
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                //去读文本信息，根据内容判断是否进行处理
                using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                {

                    var matches = Regex.Matches(sr.ReadToEnd(), @"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}),\d{3} \[\d+] [A-Z]+ .* - Request .* (\{.*}) ([a-z]+|[a-z]+/[a-z]+)", RegexOptions.RightToLeft);
                    foreach (Match m in matches)
                    {
                        Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(m.Groups[2].Value);
                        string hospid = string.Empty;
                        string phone = string.Empty;
                        if (dic.Keys.Contains("request"))
                        {
                            Dictionary<string, string> dicRequest = JsonConvert.DeserializeObject<Dictionary<string, string>>(dic["request"].ToString());
                            if (dicRequest.Keys.Contains("hospid"))
                            {
                                hospid = dicRequest["hospid"];
                            }
                            if (dicRequest.Keys.Contains("phone"))
                            {
                                phone = dicRequest["phone"];
                            }
                        }
                        list.Add(new Entity(Convert.ToDateTime(m.Groups[1].Value), m.Groups[2].Value, m.Groups[3].Value, hospid, phone));
                    }
                }
            }
            return list;
        }

        //按手机号统计一段时间内短讯发送接口的调用此时
        //时间单位：分钟
        public static List<Dictionary<string, string>> PhoneSendCounts(List<Entity> list, int minute, string phone = "null", string url = "null")
        {
            //DateTime dt = DateTime.Now.AddMinutes(-minute);
            DateTime dt = Convert.ToDateTime("2017-10-11 14:49:10");
            List<Dictionary<string, string>> listDic = new List<Dictionary<string, string>>();

            if (url == "null") { }
            if (phone == "null")
            {
                var Counts = list.Where(d => d.time > dt && d.url == (url == "null" ? "sms/send" : url)).GroupBy(d => d.phone).Select(d => new { phone = d.Key, count = d.Count() });
                foreach (var d in Counts)
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("phone", d.phone);
                    dic.Add("count", d.count.ToString());
                    listDic.Add(dic);
                }
            }
            else
            {
                var Counts = list.Where(d => d.time > dt && d.phone == phone && d.url == (url == "null" ? "sms/send" : url)).Count();
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("phone", phone);
                dic.Add("count", Counts.ToString());
                listDic.Add(dic);
            }
            return listDic;
        }

        //统计每家医院每秒调用次数
        public static List<Dictionary<string, string>> HospStatistics(List<Entity> list)
        {
            var result =
            list.GroupBy(
                 e => e.url,
                  (url, urlGroup) => new
                  {
                      url,
                      hospGroups = urlGroup
                      .GroupBy(
                         e2 => e2.hospid,
                          (hospid, hospGroup) => new
                          {
                              hospid,
                              timeGroups = hospGroup
                              .OrderBy(e3 => e3.time)
                              .GroupBy(e3 => e3.time)
                              .Select(g => new { time = g.Key, count = g.Count() })
                          }
                          ).Select(e4 => new
                          {
                              hospid = e4.hospid,
                              timeGroups = e4.timeGroups,
                              timeGroupsCount = e4.timeGroups.Count()
                          }
                                 )
                  }
                  ).Select(s => new
                  {
                      url = s.url,
                      hospGroups = s.hospGroups
                  }
                        );

            List<Counts> listCounts = new List<Counts>();
            List<Dictionary<string, string>> listDayCount = new List<Dictionary<string, string>>();
            foreach (var e in result)
            {
                foreach (var e2 in e.hospGroups)
                {
                    int dayCount = 0;
                    foreach (var e3 in e2.timeGroups)
                    {
                        listCounts.Add(new Counts(e.url, e2.hospid, e3.count, e3.time));
                        dayCount += e3.count;
                    }
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("url", e.url);
                    dic.Add("hospid", e2.hospid);
                    dic.Add("dayCount", dayCount.ToString());
                    listDayCount.Add(dic);
                }
            }

            return listDayCount;
        }
    }

    //日志文件提炼后的行实体
    public class Entity
    {
        public Entity() { }
        public Entity(DateTime time, string json, string url, string hospid, string phone)
        {
            this.time = time;
            this.json = json;
            this.url = url;
            this.hospid = hospid;
            this.phone = phone;
        }
        public string phone { get; set; }
        public DateTime time { get; set; }
        public string hospid { get; set; }
        public string json { get; set; }
        public string url { get; set; }
    }


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
