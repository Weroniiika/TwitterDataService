//Logging solution in most part was implemented with use of the code provided by Microsoft in FixIt app solution.
// 
//Copyright (C) Microsoft Corporation.  All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   http://www.apache.org/licenses/LICENSE-2.0


using System;


namespace WCFServiceLogger
{
    public interface ILogger
    {
        void Information(string message);
        void Information(Exception ex, string format, params object[] vars);
        void Information(string format, params object[] vars);

        void Warning(string message);
        void Warning(Exception ex, string format, params object[] vars);
        void Warning(string format, params object[] vars);

        void Error(string message);
        void Error(Exception ex, string format, params object[] vars);
        void Error(string format, params object[] vars);

        void DbError(Exception ex, string methodName, int requestId);

        // TraceAPI - trace inter-service calls (including latency)
        void TraceApi(string componentName, string method, TimeSpan timespan);
        void TraceApi(string componentName, string method, TimeSpan timespan, string fmt, params object[] vars);
        void TraceDuration(string componentName, string method, TimeSpan timespan, int tweetCount);

    }
}
