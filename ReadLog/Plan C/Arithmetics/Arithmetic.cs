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

        //文本行公共区
        private List<string> listStr = new List<string>();

        //日志文件路路径,传入或者以配置文件的形式读取
        private string path = "e:\\logfile.log";

        //本次读取是否完毕
        private bool flag = false;

        //文件流读取起始位置
        private long position = 0L;

        //每家医院每个接口每天的调用次数
        private List<Count> listDayCount = new List<Count>();

        //每家医院每个接口没秒的调用次数
        private List<Count> listSecondCount = new List<Count>();

        //每个手机号每分钟调用指定接口的次数
        private List<Count> listPhoneCount = new List<Count>();

        //用于设置redis key的时间
        private string redisKeyTime = string.Empty;

        public long Position { get => position; set => position = value; }


        public Arithmetic(long position)
        {
            this.position = position;
        }

        #region 线程一

        /// <summary>
        /// 从指定位置
        /// 获取一百行数据
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public void ListLine()
        {
            lock (this)
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    if (Position != 0L)
                    {
                        fs.Position = Position;
                    }
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        string line = string.Empty;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (listStr.Count == 100)
                            {
                                Monitor.Pulse(this);
                                Monitor.Wait(this);
                            }
                            listStr.Add(line);
                        }
                        Position = fs.Length;

                        flag = true;
                        Monitor.Pulse(this);
                    }
                }
            }

        }

        #endregion



        #region 线程二

        /// <summary>
        /// 根据得到的文本返回符合要求的数据集合
        /// </summary>
        /// <param name="list">获取到的文本</param>
        /// <returns></returns>
        public List<LogEntity> GetLogEntitys()
        {
            List<LogEntity> logList = new List<LogEntity>();
            //定义正则表达式
            string pattern = @"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}),\d{3} \[\d+] [A-Z]+ .* - Request .* (\{.*}) ([a-z]+|[a-z]+/[a-z]+)";

            foreach (string str in listStr)
            {
                Match match = Regex.Match(str, pattern);
                //解析符合表达式的内容添加到集合里
                if (match.Groups.Count > 1)
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    string hospid = string.Empty;
                    string phone = string.Empty;
                    dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(match.Groups[2].ToString());
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

        /// <summary>
        /// 统计每家医院每个接口每秒的调用次数
        /// 统计每家医院每个接口每天的调用次数
        /// </summary>
        public void HospStatistics()
        {
            lock (this)
            {
                while (true)
                {
                    if (listStr.Count > 0)
                    {
                        //根据问本行公共区的到需要处理的数据，并清空公共区（消费）
                        List<LogEntity> list = GetLogEntitys();
                        //执行获取手机短讯接口调用查询方法
                        PhoneSendCounts(list);
                        listStr.Clear();
                        //分别按照接口、医院、时间分组，返回对应结果
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

                        foreach (var e in result)
                        {
                            foreach (var e2 in e.hospGroups)
                            {
                                foreach (var e3 in e2.timeGroups)
                                {
                                    //将按秒分组的数据取出
                                    listSecondCount.Add(new Count(e.url, e2.hospid, e3.count, e3.time));
                                    string daytime = e3.time.Year + "-" + e3.time.Month + "-" + e3.time.Day;
                                    //获取每个医院每个接口当天的调用次数
                                    bool thisday = false;
                                    foreach (Count c in listDayCount)
                                    {
                                        if (c.daytime == daytime)
                                        {
                                            thisday = true;
                                            c.count += e3.count;
                                            break;
                                        }
                                    }
                                    if (!thisday)
                                    {
                                        listDayCount.Add(new Count(e.url, e2.hospid, e3.count, daytime));
                                    }
                                }

                            }
                        }
                        //如果本次读取已完成，跳出死循环
                        if (flag)
                        {
                            break;
                        }

                        Monitor.Pulse(this);

                    }
                    else
                    {
                        Monitor.Wait(this);
                    }
                }

                //在这里执行数据库插入操作

                RedisHelper re = new RedisHelper();
                Dictionary<string, object> dic = new Dictionary<string, object>();
                CountDal dal = new CountDal();
                dal.InsertDayCount(listDayCount);
                dal.InsertPhoneCount(listPhoneCount);
                dal.InsertSecondCount(listSecondCount);
            }

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
        public void PhoneSendCounts(List<LogEntity> list)
        {

            list = GetLogEntitys();
            foreach (LogEntity l in list)
            {
                string minuteTime = l.time.ToString("yyyy/MM/dd HH:mm:00");
                bool thisMinute = false;
                foreach (Count c in listPhoneCount)
                {
                    if (string.Compare(c.minutetime, minuteTime) == 0 && string.Compare(c.phone, l.phone) == 0)
                    {
                        c.count += 1;
                        thisMinute = true;
                        break;
                    }
                }
                if (!thisMinute)
                {
                    if (!string.IsNullOrEmpty(l.phone))
                        listPhoneCount.Add(new Count(l.phone, minuteTime, 1));
                }


            }

        }

    }
}
