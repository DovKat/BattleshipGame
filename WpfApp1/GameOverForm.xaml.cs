using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for GameOverForm.xaml
    /// </summary>
    public partial class GameOverForm : Window
    {
        private bool _hasWonGame;
        public GameOverForm(bool hasWonGame)
        {
            _hasWonGame = hasWonGame;
            InitializeComponent();
            this.PlayGameButton.IsEnabled = false;
            ShowResult();
        }
        private void ShowResult()
        {
            GameOverText.FontSize = 40;
            GameOverText.Text = _hasWonGame ? "You won! Congrats!" : "You lost! booboo...";
            this.PlayGameButton.IsEnabled = true;
        }

        private void HandlePlayAgain(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                var newWindow = new MainWindow();
                newWindow.Show();
                this.Close();
            });

        }
    }
}
