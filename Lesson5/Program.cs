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
        static bool priorityQueue = false;
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


        //main functions

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
            Text = builder.ToString().TrimEnd();
            Console.WriteLine("=== Text saved ===");
        }


        //reads from file, path provided in console
        static void ReadFromFile()
        {
            string filePath = handler.AskForInput("Enter file path.");
            try
            {
                Text = File.ReadAllText(filePath);
                Console.WriteLine("Text has been imported from file");
            } catch (IOException e) { throw new MessageException("Error in reading file: " + e.Message); };
        }


        static void PrintText()
        {
            checkText();
            Console.WriteLine(Text);
        }


        //Prints first [amountWordNumbersToPrint] words with most numbers in it
        static void FindMostNumbers()
        {
            checkText();

            string[] words = splitCheckText(Text, splitToWords);

            Console.WriteLine("Words with most numbers are (from most):");
            if (priorityQueue)
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
                enumerableCheckPrint(
                    words.OrderByDescending(e => e.Count(char.IsDigit))
                    .Take(amountWordNumbersToPrint)
                );
            }
        }


        static void FindLongestCount()
        {
            checkText();

            string[] words = splitCheckText(Text, splitToWords);
            string? longest = null;
            int amount = 0;
            
            if (priorityQueue)
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
            checkText();

            //too easy
            string res = Text;
            foreach (var kv in numberReplaceDictionary) 
            {
                res = res.Replace(kv.Key, kv.Value);
            }

            Console.WriteLine("Resulting text:");
            Console.WriteLine(res);
        }


        public static void PrintQuestionExclamationSentences()
        {
            checkText();
            string[] sentences = splitCheckText(Text, splitToSentences);
            printSentencesByCondition(sentences, sent => sent.EndsWith("?"));
            printSentencesByCondition(sentences, sent => sent.EndsWith("!"));
        }



        public static void PrintNoCommaSentences()
        {
            checkText();
            string[] sentences = splitCheckText(Text, splitToSentences);
            printSentencesByCondition(sentences, sent => sent.All(ch => ch != ','));
        }


        public static void PrintFirstLastWords()
        {
            checkText();
            string[] words = splitCheckText(Text, splitToWords);
            enumerableCheckPrint(words.Where(word => word.First() == word.Last()));
        }


        //helper functions
        private static void checkText()
        {
            if (Text == null) throw new MessageException("Text haven't been entered");
        }


        private static void enumerableCheckPrint(IEnumerable<string> e)
        {
            if (e.Count() == 0) Console.WriteLine("No such elements");
            else
            {
                e.ToList().ForEach(Console.WriteLine);
            } 
        }


        static void printSentencesByCondition(string[] sentences, Func<string, bool> condition)
        {
            enumerableCheckPrint(sentences.Where(condition));
        }


        private static string[] splitCheckText(string s, Func<string, string[]> splitFunction)
        {
            var words = splitFunction.Invoke(s);
            if (words.Length == 0)
            {
                throw new MessageException("There are no words in text");
            }
            return words;
        }


        private static string[] splitToWords(string s)
        {
            return s.Split(new char[] {' ', '\t', '\n'}, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(e => new string(e.Where(Char.IsLetterOrDigit).ToArray())).ToArray();
        }


        private static string[] splitToSentences(string s)
        {
            return s.Replace(".",".\b").Replace("!","!\b").Replace("?","?\b")
                .Split('\b', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray();
        }

        

        //just main
        public static void Main(string[] args)
        {
            handler.RegisterCommand(new string[] { "read", "read console" }, ReadFromConsole, "Reads file from console");
            handler.RegisterCommand("read file", ReadFromFile, "Reads file from console");
            handler.RegisterCommand("print", PrintText, "Prints inputed text");
            handler.RegisterCommand("most numbers", FindMostNumbers, $"Prints first {amountWordNumbersToPrint} of words with most numbers in it from most to less");
            handler.RegisterCommand("longest", FindLongestCount, $"Prints longest word and how much it appears in text");
            handler.RegisterCommand("replace numbers", ReplaceNumbers, $"Prints longest word and how much it appears in text");
            handler.RegisterCommand(new string[] { "question", "exclamation", "question exclamation", "exclamation question" },
                PrintQuestionExclamationSentences, $"Prints question sentences then exclamation sentenses");
            handler.RegisterCommand("no commas", PrintNoCommaSentences, $"Prints sentences without commas");
            handler.RegisterCommand("begin end", PrintFirstLastWords, $"Prints words which begins and ends with same letter");
            handler.RegisterCommand("switch", () => { priorityQueue = !priorityQueue; Console.WriteLine($"Priority queues {(priorityQueue ? "on" : "off")}"); }, "Switches realization of methods in most numbers and longest (use linq or not)");


            handler.PrintHelp();
            handler.Run();
        }
    }
}
