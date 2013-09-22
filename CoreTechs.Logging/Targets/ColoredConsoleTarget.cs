using System;
using System.Xml.Linq;
using CoreTechs.Logging.Configuration;

namespace CoreTechs.Logging.Targets
{
    [FriendlyTypeName("ColoredConsole")]
    public class ColoredConsoleTarget : ConsoleTarget, IConfigurableTarget
    {
        const ConsoleColor DefaultTraceColor = ConsoleColor.DarkGray;
        const ConsoleColor DefaultDebugColor = ConsoleColor.Gray;
        const ConsoleColor DefaultInfoColor = ConsoleColor.White;
        const ConsoleColor DefaultWarnColor = ConsoleColor.Magenta;
        const ConsoleColor DefaultErrorColor = ConsoleColor.Yellow;
        const ConsoleColor DefaultFatalColor = ConsoleColor.Red;

        public ConsoleColor FatalColor { get; set; }
        public ConsoleColor ErrorColor { get; set; }
        public ConsoleColor WarnColor { get; set; }
        public ConsoleColor InfoColor { get; set; }
        public ConsoleColor DebugColor { get; set; }
        public ConsoleColor TraceColor { get; set; }

        public ColoredConsoleTarget()
        {
            FatalColor = DefaultFatalColor;
            ErrorColor = DefaultErrorColor;
            WarnColor = DefaultWarnColor;
            InfoColor = DefaultInfoColor;
            DebugColor = DefaultDebugColor;
            TraceColor = DefaultTraceColor;
        }

        public override void Write(LogEntry entry)
        {
            // backup color
            var color = Console.ForegroundColor;

            // change color
            switch (entry.Level)
            {
                case Level.Trace:
                    Console.ForegroundColor = TraceColor;
                    break;
                case Level.Debug:
                    Console.ForegroundColor = DebugColor;
                    break;
                case Level.Info:
                    Console.ForegroundColor = InfoColor;
                    break;
                case Level.Warn:
                    Console.ForegroundColor = WarnColor;
                    break;
                case Level.Error:
                    Console.ForegroundColor = ErrorColor;
                    break;
                case Level.Fatal:
                    Console.ForegroundColor = FatalColor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            base.Write(entry);

            // restore color
            Console.ForegroundColor = color;
        }

        public  void Configure(XElement xml)
        {
            base.Configure(xml);

            Func<string, ConsoleColor> getColor =
                x => Try.Get(() => Enums.Parse<ConsoleColor>(xml.GetAttributeValue(x))).Value;

            TraceColor = getColor("TraceColor");
            DebugColor = getColor("DebugColor");
            InfoColor = getColor("InfoColor");
            WarnColor = getColor("WarnColor");
            ErrorColor = getColor("ErrorColor");
            FatalColor = getColor("FatalColor");
        }
    }
}