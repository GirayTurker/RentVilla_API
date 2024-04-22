using RentVilla_API.Logger.LoogerInterfaces;

namespace RentVilla_API.Logger
{
    public class LoggerDev : ILoggerDev

    {
        public void Log(string message, string type)
        {
            if (type == "error")
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("ERROR --> " + message);
            }
            else if (type == "warning")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning --> " + message);
            }
            else if(type == "info") 
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Info --> "+message);
            }
        }
    }
}
