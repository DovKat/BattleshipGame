using backend.Models.Entity;
using backend.Models.Entity.Flyweight;
using backend.Models.Entity.Ships;
using backend.Models.Entity.Ships.Factory;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Xaml.Behaviors;
using Shared;
using Shared.Transfer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Interaction = Microsoft.Xaml.Behaviors.Interaction;
using WpfApp1.States;
using System.Diagnostics;
using System.Data;
using WpfApp1.Interpreter;
using WpfApp1.Mediator;
using System.Windows.Documents;

//// if you want to update UI state, you need to call your change in this
/// as UI changes only allowed on the main thread, and this calls from the main thread. lol
//this.Dispatcher.Invoke(() =>
//{
//   myElement.Something = "lol"
//
//});
//

//todo: add game ending/winner
//todo: classes classes classes classes

//todo later: remove ship attack from dropdown if sunk

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool buttonClicked = false;
        private const int MAP_SIZE_X = 10;
        private const int MAP_SIZE_Y = 10;
        private static ShipFactory shipFactory = new ConcreteShipFactory();
        private Ship[] Ships = {
            shipFactory.GetShip(ShipType.SmallShip),
            shipFactory.GetShip(ShipType.MediumShip),
            shipFactory.GetShip(ShipType.BigShip),
            shipFactory.GetShip(ShipType.MediumShip).SetVertical(),
            shipFactory.GetShip(ShipType.BigShip).SetVertical(),
        };
        private Button[,] MyButtons = new Button[MAP_SIZE_X, MAP_SIZE_Y];
        private Button[,] EnemyButtons = new Button[MAP_SIZE_X, MAP_SIZE_Y];
        private Dictionary<int, Style> EnemeyBoardStyles = new Dictionary<int, Style>();
        private string gameState = ""; // change to class ?
        private DispatcherTimer timer;
        private HubConnection _connection;
        private readonly List<Snowflake> snowflakes = new List<Snowflake>();
        private readonly Random random = new Random();
        private GameState currentGameState = new MatchmakingState();
        private DispatcherTimer _uiTimer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeUi();
            _connection = new HubConnectionBuilder()
             .WithUrl("http://localhost:8080/hub")
             .Build();

            Loaded += async (sender, e) => await ConnectToServer();
            InitializeBombAttackBox();
            SetupListeners();
        
            StartSnowfall();
        }

        public void UpdateState(GameState state)
        {
            currentGameState = state;
            this.Dispatcher.Invoke(() => {
                StateInfo.Content = state.getStateInfo().ToString();
            });
        }

        //private void StartSnowfall()
        //{
        //    var timer = new System.Windows.Threading.DispatcherTimer();
        //    timer.Tick += (sender, args) => GenerateSnowflake();
        //    timer.Interval = TimeSpan.FromMilliseconds(10);
        //    timer.Start();
        //}

        //private void GenerateSnowflake()
        //{
        //    var size = random.Next(5, 15);
        //    var snowflake = new Snowflake
        //    {
        //        X = random.Next((int)ActualWidth),
        //        Y = -size, // Start above the window
        //        FallSpeed = random.Next(1, 5),
        //        Size = size
        //    };

        //    snowflakes.Add(snowflake);

        //    var snowflakeShape = new Ellipse
        //    {
        //        Width = size,
        //        Height = size,
        //        Fill = Brushes.White
        //    };

        //    Canvas.SetLeft(snowflakeShape, snowflake.X);
        //    Canvas.SetTop(snowflakeShape, snowflake.Y);

        //    MainCanvas.Children.Add(snowflakeShape);

        //    var animation = new DoubleAnimation
        //    {
        //        To = ActualHeight,
        //        Duration = TimeSpan.FromSeconds(snowflake.FallSpeed)
        //    };

        //    snowflakeShape.BeginAnimation(Canvas.TopProperty, animation);
        //}

        private void StartSnowfall()
        {
            var timer = new DispatcherTimer();
            timer.Tick += (sender, args) => GenerateSnowflake();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Start();
        }

        private void GenerateSnowflake()
        {
            var type = new SnowflakeType();
            type.Size = random.Next(5, 15);
            type.Speed = random.Next(1, 5);

            var snowflake = SnowflakeFactory.GetSnowflake(type);

            snowflake.X = random.Next((int)ActualWidth);
            snowflake.Y = -snowflake.Type.Size; // Start above the window

            snowflakes.Add(snowflake);

            var snowflakeShape = new Ellipse
            {
                Width = snowflake.Type.Size,
                Height = snowflake.Type.Size,
                Fill = Brushes.White
            };

            Canvas.SetLeft(snowflakeShape, snowflake.X);
            Canvas.SetTop(snowflakeShape, snowflake.Y);

            MainCanvas.Children.Add(snowflakeShape);

            var animation = new DoubleAnimation
            {
                To = ActualHeight,
                Duration = TimeSpan.FromSeconds(snowflake.Type.Speed)
            };

            snowflakeShape.BeginAnimation(Canvas.TopProperty, animation);
        }

        private Button CreateButton(bool myBoard, int x, int y)
        {
            Button b = null;
            this.Dispatcher.Invoke(() =>
            {
                b = new Button();

                EnemeyBoardStyles.TryAdd(x * 10 + y, new Style());

                b.IsEnabled = false;
            });

            if (myBoard && b != null)
            {
                this.Dispatcher.Invoke(() =>
                {
                    b.ContextMenu = new ContextMenu();
                    for (int i = 0; i < Ships.Length; i++)
                    {
                        Ship ship = Ships[i];
                        ArrangementDto tag = new ArrangementDto(ship, x, y);
                        var item = new MenuItem { Header = ship.ToString(), Tag = tag };
                        this.Dispatcher.Invoke(() =>
                        {
                            item.Click += HandleShipArrangement;
                            if ((ship.IsVertical && y + ship.Size <= MAP_SIZE_Y) ||
                                (!ship.IsVertical && x + ship.Size <= MAP_SIZE_X))
                            {
                                b.ContextMenu.Items.Add(item);
                            }
                        });
                    }
                    BehaviorCollection behaviors = Interaction.GetBehaviors(b);
                    behaviors.Add(new DropDownMenuBehavior());
                });
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    b.Tag = new int[2] { x, y };
                    b.MouseEnter += HandleMouseEnter;
                    b.MouseLeave += HandleMouseLeave;
                    b.Click += HandleShot;
                });
            }

            return b;
        }

        private void InitializeUi()
        {
            this.Dispatcher.Invoke(() =>
            {
                ShipAttacksBox.IsEnabled = true;
            });
            this.Dispatcher.Invoke(() =>
            {
                BombAttackBox.IsEnabled = true;
            });
 
            for (int y = 0; y < MAP_SIZE_Y; y++)
            {
                for (int x = 0; x < MAP_SIZE_X; x++)
                {
                    var myButton = CreateButton(true, x, y);
                    var enemyButton = CreateButton(false, x, y);
                    this.Dispatcher.Invoke(() =>
                    {
                        MyButtons[y, x] = myButton;
                        EnemyButtons[y, x] = enemyButton;

                        Grid.SetRow(myButton, y + 1);
                        Grid.SetColumn(myButton, x + 1);
                        MyBoard.Children.Add(myButton);

                        Grid.SetRow(enemyButton, y + 1);
                        Grid.SetColumn(enemyButton, x + 1);
                        EnemyBoard.Children.Add(enemyButton);
                    });
                }
            }
        }

        private async Task ConnectToServer()
        {
            await _connection.StartAsync();
            await _connection.InvokeAsync("JoinGame");

            string message = "Player is connected";

            IMessage debugger = new DebuggerMessages();
            debugger.SendMessage(message);

            IMessage serverMessages = new ServerMessagesAdapter(_connection);
            serverMessages.SendMessage(message);
        }

        private void SetupListeners()
        {
            _connection.On<string>("WaitingForOpponent", HandleOnWaitingForOpponent);
            _connection.On<string>("SetupShips", HandleOnSetupShips);
            _connection.On<string>("GameStarted", HandleOnGameStarted);
            _connection.On<string>("YourTurn", HandleOnPlayerTurn);
            _connection.On<MoveResult>("UndoTurn", HandleOnPlayerUndoTurn);
            //attacks
            _connection.On<MoveResult>("ReturnMove", HandleOnReturnMove);
            _connection.On<MoveResult>("OpponentResult", HandleOnOpponentResult);

            _connection.On<MoveResult>("UndoReturnMove", HandleOnUndoReturnMove);
            _connection.On<MoveResult>("UndoOpponentResult", HandleOnUndoOpponentResult);

            _connection.On<SetupShipResponse>("SetupShipResponse", HandleOnSetShipResult); // need to add DTO or smt
            _connection.On<List<ShipStats>>("ShipsStats", HandleOnShipStats);

            _connection.On<bool>("GameOver", HandleOnGameOver);

            _connection.On<bool>("GameReset", HandleOnRestoreGame);

            _connection.On<RestartGame>("ResetGameShip", HandleOnSetShipRestart);

            // updates
            _connection.On<List<ShipCoordinate>>("AddFlags", HandleAddFlags);
            _connection.On<List<ShipCoordinate>>("AddEnemyFlags", HandleAddEnemyFlags);
            _connection.On<List<ShipCoordinate>>("RerenderCoordinates", HandleRerenderCoordinates);
            _connection.On<List<SetupShipResponse>>("RandomShipsResponse", HandleOnRandomSetShips);
            _connection.On<Shared.Color, string, Shared.Color, Shared.Color>("SetTheme", HandleThemeMode);
            _connection.On<string>("GlobalMessage", SendMessageToClient);
            _connection.On<string>("SetIcon", HandlePlayerIcon);
            _connection.On<int>("SetCoins", HandlePlayerCoins);
            _connection.On<int>("SetMoves", HandlePlayerMoves);
            _connection.On<int, int>("SetLeftShoots", HandleLeftShoots);
            // _connection.On<int>("StartTurnTimer", StartUITimer);


            _connection.On("TurnEnded", () =>
            {
                MessageBox.Show("Your turn has ended.");
                SendMessageToClient("Your turn has ended.");
                _uiTimer.Stop(); 
            });
        }

        private void HandleLeftShoots(int id, int leftShoots)
        {
            this.Dispatcher.Invoke(() => {
                foreach (Ship ship in ShipAttacksBox.Items)
                if(leftShoots == 0 && ship.ID == id)
                {
                    ShipAttacksBox.Items.Remove(ship);
                    break;
                }
                if (ShipAttacksBox.SelectedItem == null && ShipAttacksBox.HasItems)
                {
                    ShipAttacksBox.SelectedIndex = 0;
                }
            });
        }

        private void HandlePlayerMoves(int moves)
        {
            this.Dispatcher.Invoke(() => {
                Coins_MovesLeft.Content = moves;
            });
        }

        private void HandlePlayerCoins(int coins)
        {
            this.Dispatcher.Invoke(() => {
                Coins_MovesLeft.Content = coins;
            });
        }

        private void HandlePlayerIcon(string icon)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(icon);

                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = ms;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze(); // Freeze the image to avoid memory leaks

                    this.Dispatcher.Invoke(() => {
                        PlayerIcon.Source = bitmapImage;
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void HandleOnRestoreGame(bool turn)
        {
            this.Dispatcher.Invoke(() =>
            {
                ResetGame.IsEnabled = false;
                ShipAttacksBox.Items.Clear();
            });

            ClearMessages();
            ClearMessageToShips();

            EnableMyBoard(false);
            EnableMyBoard(false);
            

            EnemeyBoardStyles = new Dictionary<int, Style>();
            InitializeUi();
            SendMessageToClient("Game reset!");

            EnableMyBoard(true);
            EnableEnemyBoard(turn);

        }

        private void HandleOnShipStats(List<ShipStats> shipStats)
        {
            ClearMessageToShips();
            SendMessageToShips("Your ship stats:");
            foreach (var shipStat in shipStats)
            {
                SendMessageToShips(shipStat.ToString());
            }
        }

        private void HandleRerenderCoordinates(List<ShipCoordinate> coordinates)
        {
            foreach (ShipCoordinate coord in coordinates)
            {
                this.Dispatcher.Invoke(() =>
                {
                    UpdateButtonByCoordinate(MyButtons, coord, "");
                    SolidColorBrush? backgroundColor = ParseColorToBrush(coord.Background);
                    if (backgroundColor != null)
                    {
                        MyButtons[coord.Y, coord.X].Background = backgroundColor;
                    }
                    SolidColorBrush? borderColor = ParseColorToBrush(coord.BorderColor);
                    if (borderColor != null)
                    {
                        MyButtons[coord.Y, coord.X].BorderBrush = borderColor;
                    }
                });
            }
        }

        private SolidColorBrush? ParseColorToBrush(Shared.Color color)
        {
            switch (color)
            {
                case Shared.Color.Yellow:
                    return Brushes.Yellow;
                case Shared.Color.Blue:
                    return Brushes.Blue;
                case Shared.Color.Black:
                    return Brushes.Black;
                case Shared.Color.White:
                    return Brushes.White;
                case Shared.Color.Gray:
                    return Brushes.Gray;
                default:
                    return null;
            }
        }

        private void HandleAddFlags(List<ShipCoordinate> coordinates)
        {
            this.Dispatcher.Invoke(() =>
            {
                foreach (ShipCoordinate coordinate in coordinates)
                {
                    UpdateButtonByCoordinate(MyButtons, coordinate, "F");
                }
            });
        }

        private void HandleAddEnemyFlags(List<ShipCoordinate> coordinates)
        {
            this.Dispatcher.Invoke(() =>
            {
                foreach (ShipCoordinate coordinate in coordinates)
                {
                    UpdateButtonByCoordinate(EnemyButtons, coordinate, "F");
                }
            });
        }

        private void HandleOnGameOver(bool hasWonGame)
        {
            this.Dispatcher.Invoke(() =>
            {
                var newWindow = new GameOverForm(hasWonGame);
                newWindow.Show();
                this.Close();
            });
        }

        private void HandleOnOpponentResult(MoveResult moveResult)
        {
            string message = moveResult.IsHit ? "Enemy hit your ship!" + moveResult.X + " " + moveResult.Y : "Enemy missed!" + moveResult.X + " " + moveResult.Y;
            SendMessageToClient(message);
            this.Dispatcher.Invoke(() =>
            {
                Button button = MyButtons[moveResult.Y, moveResult.X];
                button.ContextMenu = null;
                Image? explosionImage = GetCoordinateImage(ShipCoordinateIcon.Explosion);
                button.Content = moveResult.IsHit ? explosionImage != null ? explosionImage : "X" : "O";
                button.Style = (Style)Resources[moveResult.IsHit ? "HitButton" : "NotHitButton"];
            });
        }

        private void HandleOnUndoOpponentResult(MoveResult moveResult)
        {
            string message = "Enemy undone his move!";
            SendMessageToClient(message);
            this.Dispatcher.Invoke(() =>
            {
                Button button = MyButtons[moveResult.Y, moveResult.X];
                button.Content = null;
                EnemeyBoardStyles[moveResult.X * 10 + moveResult.Y] = new Style();
            });
        }

        private void HandleOnReturnMove(MoveResult moveResult)
        {
            string message = moveResult.IsHit ? "You hit enemy ship!" + moveResult.X + " " + moveResult.Y : "You missed enemy ship!" + moveResult.X + " " + moveResult.Y;
            
            SendMessageToClient(message);
            this.Dispatcher.Invoke(() =>
            {
                Button button = EnemyButtons[moveResult.Y, moveResult.X];
                Image? explosionImage = GetCoordinateImage(ShipCoordinateIcon.Explosion);
                button.Content = moveResult.IsHit ? explosionImage != null ? explosionImage : "X" : "O";
                Style newStyle = (Style)Resources[moveResult.IsHit ? "HitButton" : "NotHitButton"];
                EnemeyBoardStyles[moveResult.X * 10 + moveResult.Y] = newStyle;
                if (!moveResult.IsHit)
                {
                    button.Tag = new int[2] { moveResult.X, moveResult.Y };
                    button.MouseEnter += HandleMouseEnter;
                    button.MouseLeave += HandleMouseLeave;
                    button.Click += HandleShot;
                }
                else
                {
                    button.Tag = null;
                }
                button.Style = newStyle;
            });
            //impl
            //_connection.SendAsync("ShipCheck", )
        }
        private void HandleOnUndoReturnMove(MoveResult moveResult)
        {
            string message = "You undone your move";
            SendMessageToClient(message);
            this.Dispatcher.Invoke(() =>
            {
                Button button = EnemyButtons[moveResult.Y, moveResult.X];
                button.Content = null;
                EnemeyBoardStyles[moveResult.X * 10 + moveResult.Y] = new Style();
            });
        }

        private Image? GetCoordinateImage(string relativeImagePath)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = System.IO.Path.Combine(basePath, relativeImagePath);

            BitmapImage pngImage = new BitmapImage();
            try
            {
                pngImage.BeginInit();
                Uri imageUri = new Uri(filePath, UriKind.RelativeOrAbsolute);
                pngImage.UriSource = imageUri;
                pngImage.EndInit();

                Image pngImageView = new Image
                {
                    Source = pngImage,
                    Stretch = Stretch.Fill,
                    StretchDirection = StretchDirection.Both
                };

                return pngImageView;
            }
            catch
            {
                return null;
            }
        }

        private void ClearMessageToShips()
        {
            this.Dispatcher.Invoke(() =>
            {
                MessagesListbox_ShipStats.Items.Clear();
            });
        }
        private void ClearMessages()
        {
            this.Dispatcher.Invoke(() =>
            {
                MessagesListbox.Items.Clear();
            });
        }

        private void SendMessageToShips(string message)
        {
            this.Dispatcher.Invoke(() =>
            {
                MessagesListbox_ShipStats.Items.Add(message);
            });
        }

        private void SendMessageToClient(string message)
        {
            this.Dispatcher.Invoke(() =>
            {
                MessagesListbox.Items.Add(message);
            });
        }

        private void HandleOnPlayerTurn(string _)
        {
            UpdateState(new LocalplayerTurnState(new EnemyTurnState()));
            //StartUITimer(30);
            SendMessageToClient($"Your turn!");
            EnableEnemyBoard(true);
        }

        private async void HandleOnPlayerUndoTurn(MoveResult moveResult)
        {
            SendMessageToClient("You can undo turn!");
            EnableEnemyBoard(false);
            EnableUndo(true);

            // Use a timer to set a timeout for completing the task
            //var timeoutMilliseconds = 3000; // 3 seconds timeout
            //var timer = new Timer(TimerCallback, null, timeoutMilliseconds, Timeout.Infinite);

            //// Wait for the button to be clicked
            //await Task.Delay(Timeout.Infinite); // This line effectively pauses until the timer callback sets the buttonClicked

            //// Cancel the timer
            //timer.Dispose();

            //if (buttonClicked)
            //{
            //    await _connection.SendAsync("UndoMove", moveResult);
            //    buttonClicked = false;
            //}
            //else
            //{
            //    EnableUndo(false);
            //    await _connection.SendAsync("GiveMoveToPlayer");
            //}
            await Task.Delay(3000);

            if (buttonClicked)
            {
                await _connection.SendAsync("UndoMove", moveResult);
                UpdateState(new LocalplayerTurnState(new EnemyTurnState()));
            } 
            else {
                EnableUndo(false); // Attach an event handler
                await _connection.SendAsync("GiveMoveToPlayer");
            }

            buttonClicked = false;
        }

        private void EnableUndo(bool enable)
        {
            this.Dispatcher.Invoke(() =>
            {
                Undo.IsEnabled = enable;
            });
        }

        private void HandleOnGameStarted(string _)
        {
            UpdateState(new TransitionState());
            SendMessageToClient("Game started");
            this.Dispatcher.Invoke(() =>
            {
                TestModeButton.IsEnabled = true;
                ResetGame.IsEnabled = true;
                Coins_MovesLeftLabel.Content = "Moves left";
            });
 
            _connection.SendAsync("ShipsStats");
        }

        private void HandleOnWaitingForOpponent(string username)
        {
            UpdateState(new MatchmakingState());
            this.Dispatcher.Invoke(() =>
            {
                UserName.Text = "Welcome, " + username;
            });
        }

        private void HandleOnSetupShips(string _)
        {
            UpdateState(new PlacingShipsState());
            this.Dispatcher.Invoke(() =>
            {
                MessagesListbox.Items.Add("Please setup your ships!");

                ActionButton.Content = "Ready";
                ActionButton.IsEnabled = true;
                RandomShips.IsEnabled = true;

                EnableMyBoard(true);
                EnableEnemyBoard(false);
            });
        }
        //Impl
        private void HandleShipArrangement(object sender, RoutedEventArgs e)
        {
            var b = sender as MenuItem;
            var tag = b.Tag as ArrangementDto;

            if (tag?.Ship == null)
            {
                return;
            }

            _connection.SendAsync("AddShipToPlayer", tag.x, tag.y, tag.Ship.ShipType, tag.Ship.IsVertical, tag.Ship.Price);
        }
        //impl
        private void HandleShipAttacks(ShipType shipType, int id)
        {
            ShipFactory shipFactory = new ConcreteShipFactory();
            this.Dispatcher.Invoke(() =>
            {
                Ship temp = shipFactory.GetShip(shipType);
                temp.ID = id;
                ShipAttacksBox.Items.Add(temp);
            });
            this.Dispatcher.Invoke(() =>
            {
                if (ShipAttacksBox.SelectedItem == null)
                {
                    ShipAttacksBox.SelectedIndex = 0;
                }
            });
        }

        private void InitializeBombAttackBox()
        {
            this.Dispatcher.Invoke(() =>
            {
                BombAttackBox.Items.Add(BombType.MissileBomb); // type index needs to be same as Items index :D
                BombAttackBox.Items.Add(BombType.AtomicBomb);
            });
            this.Dispatcher.Invoke(() =>
            {
                if (BombAttackBox.SelectedItem == null)
                {
                    BombAttackBox.SelectedIndex = 0;
                }
            });
        }

        private void HandleSetFlag(object sender, RoutedEventArgs e)
        {
            var b = sender as MenuItem;
            int[] tag = b.Tag as int[];
            int x = tag[0];
            int y = tag[1];

            _connection.SendAsync("FlagShip", x, y);
        }

        private void HandleSetYellowBackground(object sender, RoutedEventArgs e)
        {
            var b = sender as MenuItem;
            int[] tag = b.Tag as int[];
            int x = tag[0];
            int y = tag[1];

            _connection.SendAsync("SetShipYellowBackground", x, y);
        }

        private void HandleSetBlueBorder(object sender, RoutedEventArgs e)
        {
            var b = sender as MenuItem;
            int[] tag = b.Tag as int[];
            int x = tag[0];
            int y = tag[1];

            _connection.SendAsync("SetShipBlueBorder", x, y);
        }
        private void HandleOnSetShipRestart(RestartGame gameRestartShip)
        {
            HandleRerenderCoordinates(gameRestartShip.Coordinates);
            HandleShipAttacks(gameRestartShip.ShipType, gameRestartShip.ID);
            foreach (ShipCoordinate coordinate in gameRestartShip.Coordinates)
            {
                this.Dispatcher.Invoke(() =>
                {
                    UpdateButtonByCoordinate(MyButtons, coordinate, "#");
                    MyButtons[coordinate.Y, coordinate.X].ContextMenu = new ContextMenu();

                    int[] tag = new int[] { coordinate.X, coordinate.Y };
                    var item = new MenuItem { Header = "Flag ship", Tag = tag };
                    item.Click += HandleSetFlag;
                    MyButtons[coordinate.Y, coordinate.X].ContextMenu.Items.Add(item);

                    item = new MenuItem { Header = "Set ship yellow background", Tag = tag };
                    item.Click += HandleSetYellowBackground;
                    MyButtons[coordinate.Y, coordinate.X].ContextMenu.Items.Add(item);

                    item = new MenuItem { Header = "Set ship blue border", Tag = tag };
                    item.Click += HandleSetBlueBorder;
                    MyButtons[coordinate.Y, coordinate.X].ContextMenu.Items.Add(item);
                });
            }
        }

        //impl
        private void HandleOnSetShipResult(SetupShipResponse setupShipResponse)
        {
            if (setupShipResponse.CanPlace)
            {
                HandleShipAttacks(setupShipResponse.TypeOfShip, setupShipResponse.ID);
                foreach (ShipCoordinate coordinate in setupShipResponse.ShipCoordinates)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        UpdateButtonByCoordinate(MyButtons, coordinate, "#");
                        MyButtons[coordinate.Y, coordinate.X].ContextMenu = new ContextMenu();

                        int[] tag = new int[] { coordinate.X, coordinate.Y };
                        var item = new MenuItem { Header = "Flag ship", Tag = tag };
                        item.Click += HandleSetFlag;
                        MyButtons[coordinate.Y, coordinate.X].ContextMenu.Items.Add(item);

                        item = new MenuItem { Header = "Set ship yellow background", Tag = tag };
                        item.Click += HandleSetYellowBackground;
                        MyButtons[coordinate.Y, coordinate.X].ContextMenu.Items.Add(item);

                        item = new MenuItem { Header = "Set ship blue border", Tag = tag };
                        item.Click += HandleSetBlueBorder;
                        MyButtons[coordinate.Y, coordinate.X].ContextMenu.Items.Add(item);
                    });
                }
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    Executable successMessage = new SendLocalMessageExecutable(MessagesListbox, ("You dont have enough coins to place that ship"));
                    successMessage.Execute();
                });
            }

            EnableMyBoard(true);
        }

        private void UpdateButtonByCoordinate(Button[,] buttons, ShipCoordinate coordinate, string defaultContent)
        {
            Image? icon = GetCoordinateImage(coordinate.Icon);
            buttons[coordinate.Y, coordinate.X].Content = icon != null ? icon : defaultContent;
        }

        private void EnableMyBoard(bool enable)
        {
            foreach (Button b in MyButtons)
            {
                this.Dispatcher.Invoke(() =>
                {
                    b.IsEnabled = enable;
                });
            }
        }

        private void EnableEnemyBoard(bool enable)
        {
            foreach (Button b in EnemyButtons)
            {
                this.Dispatcher.Invoke(() =>
                {
                    b.IsEnabled = enable && b.Tag != null;
                });
            }

        }

        private void HandleAction(object sender, RoutedEventArgs e)
        {
            if (currentGameState.getName() == State.PlacingShips)
            {
                _connection.SendAsync("DoneShipSetup");
                ClearEmptyButtons();
            }

            ActionButton.IsEnabled = false;
        }

        private void ResetGameAction(object sender, RoutedEventArgs e)
        {
            _connection.SendAsync("RestartGame");
        }

        private void ClearEmptyButtons()
        {
            foreach (Button b in MyButtons)
            {
                var content = b.Content != null ? b.Content.ToString() : "";
                if (content?.Length > 0)
                {
                    continue;
                }
                b.ContextMenu = null;
            }
        }

        private void HandleShot(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int[] tag = button.Tag as int[];
            if (tag == null)
            {
                return;
            }
            int x = tag[0];
            int y = tag[1];

            Ship selectedAttackShip = (Ship)ShipAttacksBox.SelectedItem;
            BombType selectedBomb = (BombType)BombAttackBox.SelectedItem;

            //impl
            _connection.SendAsync("MakeMove", new MakeMove(x, y, selectedAttackShip.ShipType, selectedAttackShip.IsVertical, selectedBomb, selectedAttackShip.ID));
            _connection.SendAsync("ShipsStats");
            EnableEnemyBoard(false);
            UpdateState(currentGameState.getNextState());
        }

        private void HandleMouseEnter(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int[] tag = button.Tag as int[];
            if (tag == null)
            {
                return;
            }
            int x = tag[0];
            int y = tag[1];

            // TODO: Implement bomb size logic
            //if (x + 1 < 10 && EnemyButtons[y, x + 1] != null)
            //{
            //    EnemyButtons[y, x + 1].Style = (Style)Resources["HoveredButton"];
            //}

            button.Style = (Style)Resources["HoveredButton"];
        }

        private void HandleMouseLeave(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int[] tag = button.Tag as int[];
            if (tag == null)
            {
                return;
            }
            int x = tag[0];
            int y = tag[1];

            EnemeyBoardStyles.TryGetValue(x * 10 + y, out var style);
            button.Style = style;

            // TODO: Implement bomb size logic
            //if (x + 1 < 10 && EnemyButtons[y, x + 1] != null)
            //{
            //    EnemyButtons[y, x + 1].Style = style;
            //}
        }

        private void UpdateTimerDisplay(int _timeLeft)
        {
            Timer_TimeLeft.Content = _timeLeft.ToString();
        }

        private void StartUITimer(int _timeLeft)
        {
            // Initial update to display the timer immediately
            UpdateTimerDisplay(_timeLeft);

            // Initialize the DispatcherTimer
            _uiTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Set interval for 1 second
            };

            // Define what happens on each timer tick
            _uiTimer.Tick += (sender, args) =>
            {
                // Decrease the timer
                _timeLeft--;

                // Check if the timer has reached zero
                if (_timeLeft <= 0)
                {
                    // Stop the timer and end the player's turn
                    _uiTimer.Stop();
                    EndPlayerTurn(); // This will trigger the end of the player's turn asynchronously
                }
                else
                {
                    // Update the timer display every second
                    UpdateTimerDisplay(_timeLeft);
                }
            };

            // Start the timer (this won't block the UI thread)
            _uiTimer.Start();
        }

        private void EndPlayerTurn()
        {
            MessageBox.Show("Time's up! Turn ended.");
            _uiTimer.Stop(); 
        }

        public void PlayerTurnFinished()
        {
            _uiTimer.Stop(); 
        }

        private void TestMode(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(async () =>
            {
                var newWindow = new TestModeWindow(_connection.ConnectionId);
                newWindow.Show();
                newWindow.Closed += TestClosed;
                TestModeButton.IsEnabled = false;
            });
        }

        private void TestClosed(object? sender, EventArgs e)
        {
            TestModeButton.IsEnabled = true;
        }

        private void UndoAction(object sender, RoutedEventArgs e)
        {
            buttonClicked = true;

            EnableUndo(false);

            this.Dispatcher.Invoke(async () =>
            {
                await Task.Delay(2000);
                EnableEnemyBoard(true);
            });

        }

        private void PlaceRandomShipAction(object sender, RoutedEventArgs e)
        {
            _connection.InvokeAsync("GenerateRandomShips");
            RandomShips.IsEnabled = false;
        }

        private void HandleOnRandomSetShips(List<SetupShipResponse> setupShipsResponse)
        {
            foreach (SetupShipResponse setupShipResponse in setupShipsResponse)
            {
                HandleOnSetShipResult(setupShipResponse);
            }
        }

        private void ThemeMode(object sender, RoutedEventArgs e)
        {
            _connection.InvokeAsync("SetTheme");
        }

        private void HandleThemeMode(Shared.Color color, string text, Shared.Color textColor, Shared.Color buttonBackgroundColor)
        {
            SolidColorBrush? backgroundColor = ParseColorToBrush(color);
            SolidColorBrush? textColorF = ParseColorToBrush(textColor);
            SolidColorBrush? buttonBackground = ParseColorToBrush(buttonBackgroundColor);
            this.Dispatcher.Invoke(async () =>
            {
                board.Background = backgroundColor;
                Theme.Content = text;
                UserName.Foreground = textColorF;
                Histogram.Foreground = textColorF;
                StateInfo.Foreground = textColorF;
                Coins_MovesLeft.Foreground = textColorF;
                Coins_MovesLeftLabel.Foreground = textColorF;
                Theme.Background = buttonBackground;
                TestModeButton.Background = buttonBackground;
                RandomShips.Background = buttonBackground;
                MessagesListbox.Background = buttonBackground;
                MessagesListbox.Foreground = textColorF;
            });
        }

        private void CommandInput_KeyUp(object sender, KeyEventArgs e)
        {
            LocalMessageConcreteMediator localMessageMediator = new LocalMessageConcreteMediator(
                new ServerMessages(),
                new DebuggerMessages(),
                new SendLocalMessageExecutable(MessagesListbox, "[DEBUGGER]: Enter button was pressed")
            );

            if (e.Key == Key.Enter)
            {
                localMessageMediator.Execute();

                localMessageMediator.SendMessage(string.Format("Trying to execute {0}", CommandInput.Text.ToString()));

                if (!CommandInput.Text.StartsWith("/"))
                {
                    CommandInput.Text = string.Empty;
                    return;
                }

                Command command = new Command(CommandInput.Text);
                MessageCommandInterpreter interpreter = new MessageCommandInterpreter();
                interpreter.Interpret(command);

                List<Executable> executableList = new List<Executable>();
                Executable successMessage = new SendLocalMessageExecutable(MessagesListbox, string.Format("Command \"{0}\" successfuly executed", command.ParsedCommand));

                bool executed = true;
                switch (command.ParsedCommand.Name)
                {
                    case TextCommand.Msg:
                        executableList.Add(successMessage);
                        executableList.Add(new SendGlobalMessageExecutable(_connection, string.Join(" ", command.ParsedCommand.Arguments)));
                        Executables executables = new Executables(executableList);
                        executables.Execute();
                        break;
                    case TextCommand.Clear:
                        Executable clear = new ClearTextBoxExecutable(MessagesListbox);
                        clear.Execute();
                        break;
                    case TextCommand.FreshMsg:
                        Executables successMessageWithClear = new Executables(new List<Executable>
                        {
                            new ClearTextBoxExecutable(MessagesListbox),
                            successMessage
                        });
                        executableList.Add(successMessageWithClear);
                        executableList.Add(new SendGlobalMessageExecutable(_connection, string.Join(" ", command.ParsedCommand.Arguments)));
                        executables = new Executables(executableList);
                        executables.Execute();
                        break;
                    case TextCommand.Reset:
                        ShipAttacksBox.Items.Clear();
                        _connection.SendAsync("RestartGame");
                        break;
                    default:
                        executed = false;
                        localMessageMediator.SendMessageToServer(_connection, string.Format("User tried to execute invalid command - {0}", CommandInput.Text));
                        localMessageMediator.SendMessage(string.Format("Command failed to execute {0}", CommandInput.Text));
                        MessagesListbox.Items.Add(string.Format("Unknown command \"{0}\"", command.ParsedCommand));
                        break;
                }

                if (executed)
                {
                    localMessageMediator.SendMessage(string.Format("Command executed successfully {0}", CommandInput.Text));
                }

                CommandInput.Text = string.Empty;
            }
        }
    }
}
