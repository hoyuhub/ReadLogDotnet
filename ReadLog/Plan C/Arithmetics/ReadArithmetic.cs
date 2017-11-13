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
using ReadLog;
using ReadLog.Plan_C.Arithmetics;

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
                            ThreadPool.QueueUserWorkItem(AnalyticalArithmetic.HospPhoneStatistics,listStr);
                        }
                        listStr.Add(line);
                    }
                    Position = fs.Length;
                }
            }


        }

        #endregion




    }
}
