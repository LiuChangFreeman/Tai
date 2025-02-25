﻿using Core.Librarys.SQLite;
using Core.Models;
using Core.Servicers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Servicers.Instances
{
    public class Data : IData
    {

        public void Set(string processName, string processDescription, string file, int seconds)
        {
            if (string.IsNullOrEmpty(processName) || string.IsNullOrEmpty(file) || seconds <= 0)
            {
                return;
            }
            processDescription = processDescription == null ? string.Empty : processDescription;
            using (var db = new StatisticContext())
            {
                var today = DateTime.Now.Date;
                var res = db.DailyLog.SingleOrDefault(m => m.Date == today && m.ProcessName == processName && m.ProcessDescription == processDescription && m.File == file);
                if (res == null)
                {
                    //数据库中没有时则创建
                    db.DailyLog.Add(new Models.DailyLogModel()
                    {
                        Date = today,
                        File = file,
                        ProcessDescription = processDescription,
                        ProcessName = processName,
                        Time = seconds,
                    });
                }
                else
                {
                    res.Time += seconds;
                }
                db.SaveChanges();
            }
        }

        public List<DailyLogModel> GetTodaylogList()
        {
            using (var db = new StatisticContext())
            {
                var today = DateTime.Now.Date;
                var res = db.DailyLog.Where(m => m.Date == today);
                return res.ToList();
            }
        }

        public List<DailyLogModel> GetDateRangelogList(DateTime start, DateTime end)
        {

            using (var db = new StatisticContext())
            {
                var today = DateTime.Now.Date;
                var res = db.DailyLog.SqlQuery("Select Sum(Time) as Time,ProcessName,File,ID,Date,ProcessDescription from DailyLogModels WHERE Date between '" + start.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + end.ToString("yyyy-MM-dd HH:mm:ss") + "' GROUP BY ProcessName,File ").ToList();
                return res.ToList();
            }
        }

        public List<DailyLogModel> GetThisWeeklogList()
        {
            DateTime weekStartDate = DateTime.Now, weekEndDate = DateTime.Now;
            if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
            {
                weekStartDate = DateTime.Now.Date;
                weekEndDate = DateTime.Now.Date.AddDays(6);
            }
            else
            {
                int weekNum = (int)DateTime.Now.DayOfWeek;
                if (weekNum == 0)
                {
                    weekNum = 7;
                }
                weekNum -= 1;
                weekStartDate = DateTime.Now.Date.AddDays(-weekNum);
                weekEndDate = weekStartDate.Date.AddDays(6);
            }

            return GetDateRangelogList(weekStartDate, weekEndDate);
        }

        public List<DailyLogModel> GetLastWeeklogList()
        {
            DateTime weekStartDate = DateTime.Now, weekEndDate = DateTime.Now;

            int weekNum = (int)DateTime.Now.DayOfWeek;
            if (weekNum == 0)
            {
                weekNum = 7;
            }


            weekStartDate = DateTime.Now.Date.AddDays(-6 - weekNum);
            weekEndDate = weekStartDate.AddDays(6);

            return GetDateRangelogList(weekStartDate, weekEndDate);
        }

        public void Set(string processName, int seconds)
        {
            if (string.IsNullOrEmpty(processName) || seconds <= 0)
            {
                return;
            }
            using (var db = new StatisticContext())
            {
                var today = DateTime.Now.Date;
                var res = db.DailyLog.SingleOrDefault(m => m.Date == today && m.ProcessName == processName);
                if (res != null)
                {
                    res.Time += seconds;
                    db.SaveChanges();
                }

            }
        }

        public List<DailyLogModel> GetProcessMonthLogList(string file, DateTime month)
        {
            using (var db = new StatisticContext())
            {

                var res = db.DailyLog.Where(
                    m =>
                    m.Date.Year == month.Year
                    && m.Date.Month == month.Month
                    //&& m.ProcessName == processName
                    //&& m.ProcessDescription == processDescription
                    && m.File == file
                    );
                if (res != null)
                {
                    return res.ToList();
                }
                return new List<DailyLogModel>();
            }
        }

        public DailyLogModel GetLast(string file)
        {
            using (var db = new StatisticContext())
            {

                var res = db.DailyLog.Where(
                    m =>
                    m.File == file
                    ).OrderByDescending(m => m.ID).Take(1);
                if (res != null)
                {
                    return res.FirstOrDefault();
                }
                return null;
            }
        }
    }
}
