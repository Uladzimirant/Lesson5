using CMDMenu;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.PortableExecutable;
using System.Text;

namespace Lesson5
{
    public class Program
    {
        static CMDHandler handler = new CMDHandler();

        static string? Text = null;
        static int amountWordNumbersToPrint = 10;

        //I couldn't deside: use linq or not use linq, so I did both. This selects usage of it.
        static bool linq = false;
        static IComparer<int> mostComparer = Comparer<int>.Create((a, b) => Comparer<int>.Default.Compare(b, a));
        static Dictionary<string, string> numberReplaceDictionary = new Dictionary<string, string>
        {
            { "0", "zero" },
            { "1", "one" },
            { "2", "two" },
            { "3", "three" },
            { "4", "four" },
            { "5", "five" },
            { "6", "six" },
            { "7", "seven" },
            { "8", "eight" },
            { "9", "nine" }
        };


        //input from console that is stoped by writing /exit
        static void ReadFromConsole()
        {
            ExitChecker checker = new ExitChecker(new string[] { "/exit" });
            StringBuilder builder = new StringBuilder();

            Console.WriteLine("Enter text. You can also type enter/write new lines. To stop writing and save type \"/exit\"");
            try
            {
                while (true)
                {
                    builder.AppendLine(checker.CheckExit(Console.ReadLine()));
                }
            } catch (MessageException) {}
            Text = builder.ToString();
            Console.WriteLine("=== Text saved ===");
        }


        //reads from file, path provided in console
        static void ReadFromFile()
        {
            string filePath = handler.AskForInput("Enter file path.");
            try
            {
                Text = File.ReadAllText(filePath);
            } catch (IOException e) { throw new MessageException("Error in reading file: " + e.Message); };
        }

        static void PrintText()
        {
            if (Text == null) throw new MessageException("Text haven't been entered");
            Console.WriteLine(Text);
        }


        //Prints first [amountWordNumbersToPrint] words with most numbers in it
        static void FindMostNumbers()
        {
            if (Text == null) throw new MessageException("Text haven't been entered");

            string[] words = splitCheckText(Text);

            Console.WriteLine("Words with most numbers are (from most):");
            if (!linq)
            {
                //Count numbers for every word and place it in priority queue,
                PriorityQueue<string, int> countNumberWordList = new PriorityQueue<string, int>(mostComparer); //comparer need to make from most to least
                //put it in queue and it will sort
                foreach (string word in words) 
                {
                    countNumberWordList.Enqueue(word, word.Count(char.IsDigit));
                }
                //take first n most numbered words and print
                for (int i = 0; i < amountWordNumbersToPrint; i++)
                {
                    if (countNumberWordList.TryDequeue(out string? w, out int amount))
                    {
                        Console.WriteLine(w);
                    }
                    else break;
                }
            } else
            {
                //it self explanatory
                words.OrderByDescending(e => e.Count(char.IsDigit))
                    .Take(amountWordNumbersToPrint)
                    .ToList()
                    .ForEach(Console.WriteLine);
            }
        }


        static void FindLongestCount()
        {
            if (Text == null) throw new MessageException("Text haven't been entered");

            string[] words = splitCheckText(Text);
            string? longest = null;
            int amount = 0;
            
            if (!linq)
            {

                PriorityQueue<string, int> countNumberWordList = new PriorityQueue<string, int>(mostComparer); //comparer need to make from most to least
                //put it in queue and it will sort
                foreach (string word in words)
                {
                    countNumberWordList.Enqueue(word, word.Count());
                }
                longest = countNumberWordList.Peek();
                amount = countNumberWordList.UnorderedItems.Count(e=>e.Element.Equals(longest));
            }
            else
            {
                var t = words.OrderByDescending(e => e.Count());
                longest = t.First();
                amount = t.Count(s => s.Equals(longest));
            }
            Console.WriteLine( $"Longest words is {longest}. It's amount is {amount}.");
        }


        public static void ReplaceNumbers()
        {
            if (Text == null) throw new MessageException("Text haven't been entered");

            //too easy
            string res = Text;
            foreach (var kv in numberReplaceDictionary) 
            {
                res = res.Replace(kv.Key, kv.Value);
            }

            Console.WriteLine("Resulting text:");
            Console.WriteLine(res);
        }


        public static void Main(string[] args)
        {
            handler.RegisterCommand(new string[] { "read", "read console" }, ReadFromConsole, "Reads file from console");
            handler.RegisterCommand("read file", ReadFromFile, "Reads file from console");
            handler.RegisterCommand("print", PrintText, "Prints inputed text");
            handler.RegisterCommand("most numbers", FindMostNumbers, $"Prints first {amountWordNumbersToPrint} of words with most numbers in it from most to less");
            handler.RegisterCommand("longest", FindLongestCount, $"Prints longest word and how much it appears in text");
            handler.RegisterCommand("replace numbers", ReplaceNumbers, $"Prints longest word and how much it appears in text");
            handler.RegisterCommand("switch", () => { linq = !linq; Console.WriteLine($"Linq { (linq ? "on":"off") }"); }, "Switches realization of methods (use linq or not)");


            handler.PrintHelp();
            handler.Run();
        }

        private static string[] splitCheckText(string s)
        {
            var words = splitToWords(s);
            if (words.Length == 0)
            {
                throw new MessageException("There are no words in text");
            }
            return words;
        }
        private static string[] splitToWords(string s)
        {
            return s.Split().Where(e => !String.IsNullOrWhiteSpace(e))
                .Select(e => new string(e.Where(Char.IsLetterOrDigit).ToArray())).ToArray();
        }
    }
}
