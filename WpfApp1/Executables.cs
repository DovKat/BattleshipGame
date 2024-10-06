using System.Collections.Generic;

namespace WpfApp1
{
    public class Executables: Executable
    {
        private List<Executable> children = new List<Executable>();
        
        public Executables(List<Executable> executables) { 
            foreach (Executable executable in executables)
            {
                children.Add(executable);
            }
        }

        public void Execute()
        {
            foreach(Executable executable in children)
            {
                executable.Execute();
            }
        }
    }
}
