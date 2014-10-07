using System.Diagnostics;

namespace CodeMetricsLoader.Tests
{
    class TestLogger : ILogger
    {
        public void Log(string message)
        {
            Trace.WriteLine(message);
        }
    }
}
