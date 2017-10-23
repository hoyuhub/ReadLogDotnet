using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;
using System.IO.MemoryMappedFiles;
using LogRead.Plan_C.Entitys;
using System.Threading;



namespace LogRead.Plan_C.Arithmetics
{
    public class Arithmetic
    {
        public Arithmetic() { }

        public Arithmetic(string path,ref long Position)
        {
        
            Thread t1 = new Thread(new ThreadStart(ListLine(path,ref Position)));
        }

        #region 线程一

        /// <summary>
        /// 从指定位置
        /// 获取一百行数据
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public List<string> ListLine(string path, ref long Position)
        {
            List<string> list = new List<string>();
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (Position != 0L) fs.Position = Position;
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    string line = string.Empty;
                    while ((line = sr.ReadLine()) != null)
                    {
                        list.Add(line);
                        if (list.Count == 100)
                        {
                            Position = fs.Length;
                            return list;
                        }
                    }
                    Position = fs.Length;
                }
            }

            return list;

            #region 内存映射
            //using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(path))
            //{
            //    using (MemoryMappedViewStream mmvs = mmf.CreateViewStream(Position, 200))
            //    {
            //        using (StreamReader sr = new StreamReader(mmvs, Encoding.UTF8))
            //        {
            //            string line = string.Empty;
            //            while ((line = sr.ReadLine()) != null)
            //            {
            //                list.Add(line);
            //            }
            //            long l = Encoding.Default.GetBytes(list[list.Count - 1]).LongLength;
            //            Console.WriteLine("内容：" + list[list.Count - 1]);
            //            Console.WriteLine("最后一条记录的长度：" + l);
            //        }
            //    }
            //} 
            #endregion

        }

        #endregion



        #region 线程二

        /// <summary>
        /// 根据得到的文本返回符合要求的数据集合
        /// </summary>
        /// <param name="list">获取到的文本</param>
        /// <returns></returns>
        public List<LogEntity> GetLogEntitys(List<string> list)
        {
            List<LogEntity> logList = new List<LogEntity>();
            //定义正则表达式
            string pattern = @"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}),\d{3} \[\d+] [A-Z]+ .* - Request .* (\{.*}) ([a-z]+|[a-z]+/[a-z]+)";

            foreach (string str in list)
            {
                Match match = Regex.Match(str, pattern);
                //解析符合表达式的内容添加到集合里
                if (match.Groups.Count > 1)
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
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
                    logList.Add(new LogEntity(Convert.ToDateTime(match.Groups[1].Value), match.Groups[2].Value, match.Groups[3].Value, hospid, phone));
                }
            }
            return logList;
        }

        //统计每家医院每秒调用次数
        public List<Dictionary<string, string>> HospStatistics(List<LogEntity> list)
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

        #endregion







        //需要指定文件路径（包括文件名称）
        public List<LogEntity> Read()
        {
            string path = "e://20171011.txt";
            string line = string.Empty;
            List<LogEntity> list = new List<LogEntity>();
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
                        list.Add(new LogEntity(Convert.ToDateTime(m.Groups[1].Value), m.Groups[2].Value, m.Groups[3].Value, hospid, phone));
                    }
                }
            }
            return list;
        }

        //按手机号统计一段时间内短讯发送接口的调用此时
        //时间单位：分钟
        public List<Dictionary<string, string>> PhoneSendCounts(List<LogEntity> list, int minute, string phone = "null", string url = "null")
        {
            DateTime dt = Convert.ToDateTime("2017-10-11 14:49:10");
            List<Dictionary<string, string>> listDic = new List<Dictionary<string, string>>();
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

    }
}
