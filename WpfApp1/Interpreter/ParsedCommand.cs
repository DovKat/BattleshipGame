using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Interpreter
{
    public class ParsedCommand
    {
        public string Name { get; set; }
        public List<string> Arguments { get; set; }

        public ParsedCommand(string name, List<string> arguments)
        {
            this.Name = name;
            this.Arguments = arguments;
        }
    }
}
