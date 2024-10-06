using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Interpreter
{
    public interface IInterpreter
    {
        void Interpret(Command command);
    }
}
