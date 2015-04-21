//Logging solution in most part was implemented with use of the code provided by Microsoft in FixIt application.
// 
//Copyright (C) Microsoft Corporation.  All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   http://www.apache.org/licenses/LICENSE-2.0


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCFServiceLogger
{
    public class Logger : ILogger
    {
        private string loggerName= "";

        public Logger(string name)
        {
            loggerName = name;
        }

        public Logger()
        {
        }

        public void Information(string message)
        {
            Trace.TraceInformation(loggerName + ";" + message);
        }

        public void Information(Exception ex, string format, params object[] vars)
        {
            var msg = String.Format(format, vars);
            Trace.TraceInformation(loggerName + ";" + msg + ";Exception Details={0}", ExceptionUtils.FormatException(ex));
        }

        public void Information(string format, params object[] vars)
        {
            Trace.TraceInformation(format, vars);
        }

        public void Warning(string message)
        {
            Trace.TraceWarning(loggerName + ";" + message);
        }
        public void Warning(Exception ex, string format, params object[] vars)
        {
            var msg = String.Format(format, vars);
            Trace.TraceWarning(loggerName + ";" + msg + ";Exception Details = {0}", ExceptionUtils.FormatException(ex));
        }
        public void Warning(string format, params object[] vars)
        {
            Trace.TraceWarning(format, vars);
        }

        public void Error(string message)
        {
            Trace.TraceError(loggerName + ";",message);
        }
        public void Error(Exception ex, string format, params object[] vars)
        {
            var msg = String.Format(format, vars);
            Trace.TraceError(loggerName + ";" + msg + ";ExceptionDetails = {0}", ExceptionUtils.FormatException(ex));
        }
        public void Error(string format, params object[] vars)
        {
            Trace.TraceError(format, vars);
        }

        //DbError
        public void DbError(Exception ex, string methodName, int requestId)
        {
            Trace.TraceError("DbError: " +loggerName + ";" + methodName + ";" + "ExceptionDetails = {0};RequestId = {1}", ExceptionUtils.FormatException(ex), requestId);
        }

        // TraceAPI - trace inter-service calls (including latency)

        public void TraceApi(string componentName, string method, TimeSpan timespan)
        {
            string message = String.Concat("component:", componentName, ";method:", method, ";timespan:", timespan.ToString());
            Trace.TraceInformation(message);
        }

        public void TraceApi(string componentName, string method, TimeSpan timespan, string fmt, params object[] vars)
        {
            TraceApi(componentName, method, timespan, string.Format(fmt, vars));
        }

        public void TraceDuration(string componentName, string method, TimeSpan timespan, int tweetCount)
        {
            string message = String.Concat("component:", componentName, ";method:", method, ";timespan:", timespan.ToString(), ";tweet count:", tweetCount);
            Trace.TraceInformation(message);
        }
    }
}
