using CMDMenu;

namespace Lesson5
{
    public class Program
    {
        static CMDHandler handler = new CMDHandler();
        public static void Main(string[] args)
        {
            handler.PrintHelp();
            handler.Run();
        }
    }
}
