using Serilog;

namespace WebAPI.configurations
{
    public static class LoggingConfiguration
    {
        public static Serilog.ILogger ConfigureSerilog()
        {
            var logFilePath = "logs/log.txt";
            var retainedFileCountLimit = 7;

            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: retainedFileCountLimit)
                .CreateLogger();
        }
    }
}
