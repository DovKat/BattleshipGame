using backend.Models.Entity.Ships;
using backend.Models.Entity;
using Microsoft.Xaml.Behaviors;
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
using Shared.Transfer;
using Microsoft.AspNetCore.SignalR.Client;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for TestModeWindow.xaml
    /// </summary>
    public partial class TestModeWindow : Window
    {
        private const int MAP_SIZE_X = 10;
        private const int MAP_SIZE_Y = 10;
        private Label[,] EnemyLabels = new Label[MAP_SIZE_X, MAP_SIZE_Y];
        private HubConnection _connection;
        private string _currentPlayerConnectionId;
        public TestModeWindow(string currentPlayerConnectionId)
        {
            _currentPlayerConnectionId = currentPlayerConnectionId;
            InitializeComponent();

            InitializeUi();
            _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5220/testhub")
            .Build();
            SetupConnection();
            
            Loaded += async (sender, e) => await ConnectToServer();
        }

        private async Task ConnectToServer()
        {
            await _connection.StartAsync();
            await _connection.InvokeAsync("EnterTestMode", _currentPlayerConnectionId);
        }

        private void SetupConnection()
        {
            _connection.On<List<ShipCoordinate>>("ReturnEnterTestMode", HandleEnemyShips);
        }

        private void InitializeUi()
        {
            for (int y = 0; y < MAP_SIZE_Y; y++)
            {
                for (int x = 0; x < MAP_SIZE_X; x++)
                {
                    var enemyLabel = CreateLabel();
                    this.Dispatcher.Invoke(() =>
                    {
                        EnemyLabels[y, x] = enemyLabel;
                        Grid.SetRow(enemyLabel, y + 1);
                        Grid.SetColumn(enemyLabel, x + 1);
                        TestBoard.Children.Add(enemyLabel);
                    });
                }
            }
        }

        private Label CreateLabel()
        {
            Label b = new Label();
            //b.Background = Brushes.Gray;
            b.BorderThickness = new Thickness(1);
            b.BorderBrush = Brushes.Gray;
            return b;
        }

        private void HandleEnemyShips(List<ShipCoordinate> enemyships)
        {
            foreach (ShipCoordinate shipCoordinate in enemyships)
            {
                this.Dispatcher.Invoke(() =>
                {
                    EnemyLabels[shipCoordinate.Y, shipCoordinate.X].Content = "#";
                    EnemyLabels[shipCoordinate.Y, shipCoordinate.X].HorizontalContentAlignment = HorizontalAlignment.Center;
                    EnemyLabels[shipCoordinate.Y, shipCoordinate.X].VerticalContentAlignment = VerticalAlignment.Center;
                });
            }
        }
    }
}
