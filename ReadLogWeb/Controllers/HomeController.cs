﻿using log4net;
using LogRead.Plan_C.Arithmetics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace ReadLogWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        private long position;
        public string  TestFun()
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            Arithmetic ar = new Arithmetic(position);
            Thread t1 = new Thread(new ThreadStart(ar.ListLine));
            Thread t2 = new Thread(new ThreadStart(ar.HospStatistics));
            t1.Start();
            t2.Start();
            t2.Join();
            position = ar.Position;
            st.Stop();
            ILog log = LogManager.GetLogger("LogError");
            TimeSpan ts = st.Elapsed;
            log.Error("这一次用时：" + ts);
            return ts.ToString();
        }
    }
}