using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Interpreter
{
    public class MessageCommandInterpreter : IInterpreter
    {
        public void Interpret(Command command)
        {
            string[] subString = command.FullCommand.Split(' ');
            string name = subString[0].Remove(0,1);
            List<string> arguments = new List<string>();
            for(int i =1; i< subString.Length; i++)
            {
                arguments.Add(subString[i]);
            }
            ParsedCommand parsedCommand = new ParsedCommand(name, arguments);
            command.ParsedCommand = parsedCommand;
        }
    }
}
