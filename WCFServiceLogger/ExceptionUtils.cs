//Logging solution in most part was implemented with use of the code provided by Microsoft in FixIt app solution.
// 
//Copyright (C) Microsoft Corporation.  All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WCFServiceLogger
{
    class ExceptionUtils
    {
        /// <summary>
        /// By default return Exception.ToString()
        /// </summary>
        private static readonly Func<Exception, string> defaultFormatter = (ex) => (ex == null) ? String.Empty : ex.ToString();

        #region "Specialized Formatters"
        /// <summary>
        /// Keep a map of specialized formatters for types with extra embedded
        /// information
        /// </summary>
        private static readonly IDictionary<Type, Func<Exception, string>> FormatExceptionMap = new Dictionary<Type, Func<Exception, string>>()
            {
                { typeof(SqlException), (ex) => FormatSqlException(ex) }
            };

        private static string FormatSqlException(Exception ex)
        {
            var sqlEx = ex as SqlException;
            if (sqlEx == null)
                return String.Empty;
            var sb = new StringBuilder();

            sb.AppendLine(ex.ToString());
            for (int i = 0; i < sqlEx.Errors.Count; i++)
            {
                var sqlErr = sqlEx.Errors[i];
                sb.AppendFormat("[#{0}] Message: {0}, LineNumber: {1}, Source: {2}, Procedure: {3}\r\n",
                    sqlErr.Number, sqlErr.Message, sqlErr.LineNumber, sqlErr.Source, sqlErr.Procedure);
            }
            return sb.ToString();
        }
        #endregion

        private static void AppendExceptionInfo(StringBuilder sb, Exception exception, int depth)
        {
            Func<Exception, string> formatter = defaultFormatter;

            if (FormatExceptionMap.ContainsKey(exception.GetType()))
                formatter = FormatExceptionMap[exception.GetType()];

            sb.AppendFormat("\r\n------------------------------\r\n{0}", formatter(exception));
        }

        public static string FormatException(Exception ex)
        {
            if (ex == null)
                return String.Empty;
            var sb = new StringBuilder();
            try
            {
                AppendExceptionInfo(sb, ex, 0);
            }
            catch (Exception ex0)
            {
                sb.AppendFormat("Warning; Could not format exception {0}", ex0.ToString());
            }

            return sb.ToString();
        }
    }
}
