﻿using LogRead.Plan_C.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadLog
{
    public class RedisDal : RedisOperatorBase
    {
        /// <summary>
        /// 当天医院接口调用次数累加
        /// 每家医院和接口都有保存每天日志数的的hash表
        /// key为 LogDayCount:hospId:url
        /// </summary>
        public void DayCount(List<Count> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string hashKey = "LogDayCount:" + list[i].hospid + ":" + list[i].url;

                Redis.HIncrby(hashKey, Encoding.Default.GetBytes(list[i].daytime), list[i].count);
            }


        }


        /// <summary>
        /// 保存每家医院每秒钟的日志数
        /// 每家医院和接口都有保存每秒日志数的Sorted Set
        /// key为 LogSecondCount:hospId:url
        /// <param name="list">医院接口日志数集合</param>
        /// </summary>
        public void SecondCount(List<Count> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string sortedSetKey = "LogSecondCount:" + list[i].hospid + ":" + list[i].url;
                //添加时间后缀，确保成员唯一性
                string value = string.Empty;
                long time = Common.ConvertDateTimeInt(list[i].time);
                List<string> listStr = Redis.GetRangeFromSortedSetByLowestScore(sortedSetKey, time, time);
                if (listStr.Count > 0)
                {
                    string[] str = listStr[0].Split('_');
                    value = (Convert.ToInt32(str[0]) + list[i].count).ToString() + "_" + list[i].time.ToString();
                    Redis.RemoveItemFromSortedSet(sortedSetKey, listStr[0]);
                }
                else
                {
                    value = list[i].count.ToString() + "_" + list[i].time.ToString();
                }
                Redis.AddItemToSortedSet(sortedSetKey, value, time);
            }
        }


        /// <summary>
        /// 保存每个手机号每分钟调用短讯发送接口的次数
        /// 针对每一个手机号都有一个自己的Sorted Set
        /// key为 LogPhoneCount:phone
        /// </summary>
        /// <param name="list"></param>
        public void PhoneMinuteCount(List<Count> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string sortedSetKey = "LogPhoneMinuteCount:" + list[i].phone;
                string value = string.Empty;
                long time = Common.ConvertDateTimeInt(list[i].time);
                List<string> listStr = Redis.GetRangeFromSortedSetByLowestScore(sortedSetKey, time, time);
                if (listStr.Count > 0)
                {
                    string[] str = listStr[0].Split('_');
                    value = (Convert.ToInt32(str[0]) + list[i].count).ToString() + "_" + list[i].time.ToString();
                    Redis.RemoveItemFromSortedSet(sortedSetKey, listStr[0]);
                }
                else
                {
                    value = list[i].count.ToString() + "_" + list[i].time.ToString();
                }
                Redis.AddItemToSortedSet(sortedSetKey,value,time);
            }
        }

    }
}
