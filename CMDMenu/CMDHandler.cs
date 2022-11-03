using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CMDMenu
{

    /* Class designed to provide a console interface with enterable commands
     * It implements default exit and help behavior and exception interception
     * Also there is special exception for stopping executing function with printing only message.
     */
    public class CMDHandler
    {
        private bool _continueRunning = true;

        // Dictionary that maps commandName to function to run when command is called, tuple format is
        // <Function itself, list of all aliases including commandName, description>
        private IDictionary<string, Tuple<Action, List<string>, string?>> _commands = new Dictionary<string, Tuple<Action, List<string>, string?>>();
        
        //helper classes, inited in constructor
        private ICollection<Tuple<Action, List<string>, string?>> _customCommands; 
        private ICollection<Tuple<Action, List<string>, string?>> _defaultCommands;

        //prefix before reading line
        public string? Prefix = "> ";
        //description shown in help
        public string? Description = null;

        private void regByStrArr(string[]? command, Action action, string description)
        {
            if (command != null && command.Length > 0) RegisterCommand(command, action, description);
        }
        public CMDHandler() : this(new string[] { "quit", "exit" }, new string[] { "help" }) { }
        //Creates CMDHandler with predefined help and quit commands/
        //If they empty or null then it don't create that command
        public CMDHandler(string[]? quitCommands, string[]? helpCommands)
        {
            _customCommands = new List<Tuple<Action, List<string>, string?>>();
            regByStrArr(helpCommands, PrintHelp, "This message");
            regByStrArr(quitCommands, exit, "Ends program");
            _defaultCommands = _customCommands;
            _customCommands = new List<Tuple<Action, List<string>, string?>>();
        }

        /* Main cycle where program will run
         * until spectial command stops it.
         * All exceptions will be intercepted and then
         * the class will await new command.
         */
        public void Run()
        {
            _continueRunning = true;
            while (_continueRunning)
            {
                string inputCommand = AskForInput(checkExitInput:false).ToLower();
                try
                {
                    if (_commands.TryGetValue(inputCommand, out var action))
                    {
                        action.Item1.Invoke();
                    }
                    else
                    { handleNoCommand(inputCommand); }
                }
                catch (MessageException e) { handleMessageException(e); }
                catch (Exception e) { handleException(e); }
            }
        }

        //Prints message if present and awaits line to enter for return
        public string AskForInput(string? message = null, bool checkExitInput = true)
        {
            if (!string.IsNullOrEmpty(message)) Console.WriteLine(message);
            string? s = "";
            while (string.IsNullOrEmpty(s?.Trim()))
            {
                if (Prefix != null) Console.Write(Prefix);
                s = Console.ReadLine();
                if (checkExitInput) ExitChecker.Check(s);
            }
            return s.Trim();
        }

        //Registers one command and it aliases if present and function that will be called by entering that command
        public void RegisterCommand(string command, Action action, string? description = null)
        {
            RegisterCommand(new string[] { command }, action, description);
        }
        public void RegisterCommand(string[] commands, Action action, string? description = null)
        {
            for (int i = 0; i < commands.Length; i++) commands[i] = commands[i].ToLower();
            var t = Tuple.Create(action, new List<string>(commands), description);
            foreach (var c in commands) _commands.Add(c, t);
            _customCommands.Add(t);
        }

        //create aliase for already existing command
        public void RegisterAlias(string newAlias, string existingCommand)
        {
            if (!_commands.ContainsKey(existingCommand)) throw new KeyNotFoundException(existingCommand + " is not registered");
            var t = _commands[existingCommand];
            _commands.Add(newAlias, t);
            t.Item2.Add(newAlias);
        }

        //Is to register in commands to be able to quit
        private void exit()
        {
            _continueRunning = false;
        }

        //prints list of commands in help
        private void printFunc(StringBuilder builder, IEnumerable<Tuple<Action, List<string>, string?>> elems)
        {
            foreach (var elem in elems)
            {
                builder.Append(" ");
                //print all command aliases
                builder.AppendJoin(", ", elem.Item2);
                //if description provided, write it
                if (elem.Item3 != null)
                {
                    builder.Append(" - ");
                    builder.Append(elem.Item3);
                }
                builder.AppendLine();
            }
        }
        public void PrintHelp()
        {
            StringBuilder builder = new StringBuilder();
            if (Description != null) builder.AppendLine(Description);
            builder.AppendLine("Avaliable commands:");
            printFunc(builder, _customCommands);
            printFunc(builder, _defaultCommands);
            Console.Write(builder.ToString());
        }

        //These 3 methods will handle diffrent situations in cmd cycle
        private void handleNoCommand(string s)
        {
            Console.WriteLine($"No such command \"{s}\"");
        }

        private void handleMessageException(MessageException e)
        {
            if (!string.IsNullOrEmpty(e.Message))
            {
                Console.WriteLine(e.Message);
            }
            if (e.InnerException != null)
            {
                Console.WriteLine("Exception in question:");
                Console.WriteLine(e.InnerException.ToString());
            }
        }
        private void handleException(Exception e)
        {
            Console.WriteLine(e.ToString());
        }

    }
}
