using System.Windows.Controls;

namespace WpfApp1
{
    public class ClearTextBoxExecutable : Executable
    {
        private ListBox MessagesList;

        public ClearTextBoxExecutable(ListBox messagesList)
        {
            MessagesList = messagesList;
        }

        public void Execute()
        {
            MessagesList.Items.Clear();
        }
    }
}
