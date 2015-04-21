using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCFServiceLogger
{
    public class LoggerManager
    {
        private static TextWriterTraceListener writerListener;

        public static Logger GetLogger()
        {
            if (writerListener == null)
            {
                InitTraceListener();
            }
            return new Logger();
        }

        public static Logger GetLogger(string name)
        {
            if (writerListener == null)
            {
                InitTraceListener();
            }
            return new Logger(name);
        }

        private static void InitTraceListener()
        {
            Stream outputFile = File.Open(@"C:\Users\Weronika\Documents\Visual Studio 2013\Logs\output.txt", FileMode.OpenOrCreate);
            writerListener = new TextWriterTraceListener(outputFile);
            Trace.Listeners.Add(writerListener);
        }
    }
}
