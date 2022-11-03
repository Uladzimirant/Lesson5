namespace CMDMenu
{
    //exception class that should be placed in functions with
    //the purpose to print the message and await new command
    public class MessageException : Exception
    {
        public MessageException(){}
        public MessageException(string? message) : base(message){}
        public MessageException(string? message, Exception? innerException) : base(message, innerException){}
    }
}
