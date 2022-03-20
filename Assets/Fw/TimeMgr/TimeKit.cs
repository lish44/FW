using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FW
{
    class TimeKit
    {
        private static readonly DateTime initTime = new DateTime(1970, 1, 1, 8, 0, 0);
        public static DateTime InitTime { get; private set; }
        public static int offsetSecond { get; private set; }
        public static int utcOffsetSecond { get; private set; }
        public static void SetOffsetSecond(int second)
        {
            offsetSecond = second;
            utcOffsetSecond = 8 * 3600 + offsetSecond;
            InitTime = initTime.AddSeconds(second);
        }

        /// <summary>
        /// 登录服务器游戏开始时间
        /// </summary>
        public static long LoginServerTime;
        /// <summary>
        /// 最后校准服务器时间(毫秒)
        /// </summary>
        public static long LastServerMillisecond;

        private static int time = 0;

        /// <summary>
        /// 校准时间
        /// </summary>
        public static void ModifyTime(long millisecond)
        {

            time++;
            if (time >= int.MaxValue)
            {
                time = 0;
            }
            //Log.Debug("<color=#00FF00>===================={0}次校准======================</color>".format(time));
            //Log.Debug("校准服务器时间(毫秒)：{0}".format(millisecond));
            //Log.Debug("本地时间(秒)：{0}, 毫秒：{1}".format(LoginServerTime, LastServerMillisecond));

            long oldLastServerMillisecond = LastServerMillisecond;
            long oldLoginServerTime = LoginServerTime;

            //校准时间时，减去游戏已经运行的时间
            //LastServerMillisecond = millisecond - (long)(ConnectManager.manager().RealtimeSinceStartup * 1000.0f); 连接器链接时传过来的时间 其实就是在update里面每秒调realtimeSinceStartup
            LastServerMillisecond = millisecond - (long)(Time.realtimeSinceStartup * 1000.0f);
            LoginServerTime = LastServerMillisecond / 1000;

            //string key0 = oldLoginServerTime != LoginServerTime ? ("<color=#FF0000>" + LoginServerTime + "</color>") : LoginServerTime.ToString();
            //string key1 = oldLastServerMillisecond != LastServerMillisecond ? ("<color=#FF0000>" + LastServerMillisecond + "</color>") : LastServerMillisecond.ToString();

            //Log.Debug("校准本地时间(秒)：{0}, 毫秒：{1}".format(key0, key1));
        }

        /// <summary>
        /// 获取当前时间的时间戳
        /// </summary>
        public static long GetNowUnixTime()
        {
            //return LoginServerTime + (long)ConnectManager.manager().RealtimeSinceStartup;
            return LoginServerTime + (long) Time.realtimeSinceStartup;
        }
        /// <summary>
        /// 获取当前时间的时间戳(毫秒值)
        /// 与后台时间有最大1秒的误差.
        /// </summary>
        public static long GetNowUnixTimeMillis()
        {
            //return LastServerMillisecond + (long)(ConnectManager.manager().RealtimeSinceStartup * 1000.0f);
            return LastServerMillisecond + (long)(Time.realtimeSinceStartup * 1000.0f);
        }

        /// <summary>
        /// 获取当前时间datetime对象
        /// </summary>
        public static DateTime GetNowDateTime()
        {
            DateTime dtStart = InitTime;
            return dtStart.Add(new TimeSpan(GetNowUnixTimeMillis() * 10000));
        }

        /// <summary>
        /// 得到当月的第一天
        /// </summary>
        /// <param name = "Year" ></ param >
        /// < param name="Month"></param>
        /// <returns></returns>
        public static DateTime GetFirstDayOfMonth(int Year, int Month)
        {
            return Convert.ToDateTime(Year.ToString() + "-" + Month.ToString() + "-1");
        }

        /// <summary>
        /// 得到当月的最后一天
        /// </summary>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public static DateTime GetLastDayOfMonth(int Year, int Month)
        {
            int Days = DateTime.DaysInMonth(Year, Month);
            return Convert.ToDateTime(Year.ToString() + "-" + Month.ToString() + "-" + Days.ToString());

        }

        ///** 得到校正后时间，秒为单位 */
        public static int GetSecondTime()
        {
            return (int)GetNowUnixTime();
        }

        ////得到DateTime   从1970.1.1 8:00 开始 秒
        public static DateTime GetDateTime(int time = 0)
        {
            if (time == 0)
                time = GetSecondTime();
            return InitTime.AddSeconds(time);
        }

        //得到DateTime   从1970.1.1 8:00 开始 毫秒
        public static DateTime GetDateTimeMillis(long time)
        {
            return InitTime.AddMilliseconds(time);
        }

        //得到DateTime   从1970.1.1 8:00 开始 毫秒
        public static DateTime GetDateTimeMin(int time)
        {
            return InitTime.AddMinutes(time / 60);
        }

        /// <summary>
        /// 得到当前时间的时间戳，未做时间修正，功能开发不能用
        /// </summary>
        /// <returns>长整型时间</returns>
        public static long CurrentTimeMillis()
        {
            return Convert.ToInt64(DateTime.Now.Subtract(InitTime).TotalMilliseconds);
        }
    }
}
