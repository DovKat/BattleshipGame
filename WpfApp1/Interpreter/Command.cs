using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Interpreter
{
    public class Command
    {
        public string FullCommand { get; set; }
        public ParsedCommand ParsedCommand {get; set;}

        public Command(string fullCommand)
        {
            this.FullCommand = fullCommand;
        }
    }
}
