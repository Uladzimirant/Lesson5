namespace CMDMenu
{
    /* Helper class that checks is string exit command and throws MessageException to stop command if it is
     * Intended for use in functions registered in handler
     */
    public class ExitChecker
    {
        public static readonly ExitChecker Default = new ExitChecker();

        public ExitChecker() : this(new string[] { "stop", "exit", "quit" }) { }
        public ExitChecker(IEnumerable<string> stopWords) //for custom exit words
        {
            this.stopWords.AddRange(stopWords);
        }
        private List<string> stopWords = new List<string>();
        
        //Shortcut for ExitChecker.Default.CheckExit(...)
        public static string Check(string? input)
        {
            return Default.CheckExit(input);
        }

        public string CheckExit(string? input)
        {
            input ??= "";
            if (stopWords.Contains(input.Trim())) throw new MessageException("");
            return input;
        }
    }
}
