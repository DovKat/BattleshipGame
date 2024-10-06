using System.Windows.Controls;

namespace WpfApp1
{
    public class SendLocalMessageExecutable : Executable
    {
        private ListBox MessagesList;
        private string _message;

        public SendLocalMessageExecutable(ListBox messagesList, string message)
        {
            MessagesList = messagesList;
            _message = message;
        }

        public void Execute()
        {
            MessagesList.Items.Add(_message);
        }
    }
}
