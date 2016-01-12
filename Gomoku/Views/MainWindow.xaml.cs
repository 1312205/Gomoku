using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Gomoku.Properties;

namespace Gomoku
{

    #region Định nghĩa class CaroButton
    public class CaroButton : Button
    {

        private int x;
        private int y;
        public int X
        {
            get { return x; }
            set { x = value; }
        }


        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public CaroButton()
        {
            x = 0;
            y = 0;
        }
    }
    #endregion
    enum PlayingType
    {
        PvP = 0,
        PvCom = 1,
        POnline = 2,
        ComOnline = 3,
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Define
        private string ClientName;
        private BoardViewModel Board;
        PlayingType Type = PlayingType.PvP;
        private Socket socket;
        private Thread thread;
        public delegate object GetButtonDelegate(int row, int col);
        public bool isError = false;
        #endregion
        // ví dụ như em thêm 1 comment vào đây sau đó lưu lại
        public MainWindow()
        {
            InitializeComponent();
        }
        #region Window_Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VeBanCo();
            Board = new BoardViewModel();
            Board.onWinner += Board_onWinner;
            Board.ComAutoPlay += Board_ComAutoPlay;
        }
        #endregion

        #region Hàm cho sự kiện máy tự động vẽ
        private void Board_ComAutoPlay(Node node)
        {
            if(Type == PlayingType.ComOnline && !Board.isEndGame)
            {
                socket.Emit("MyStepIs", JObject.FromObject(new { row = node.Row - 1, col = node.Column - 1}));
            }
            else
            {
                CaroButton button = (CaroButton)this.Dispatcher.Invoke(new GetButtonDelegate(GetButton), node.Row, node.Column);
                if (button != null)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        Ellipse ell = new Ellipse();
                        ell.Width = ell.Height = 25;
                        RadialGradientBrush RadialBrush = new RadialGradientBrush();
                        RadialBrush.GradientOrigin = new Point(0.9, 0.9);
                        RadialBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.0));
                        RadialBrush.GradientStops.Add(new GradientStop(Colors.White, 0.75));
                        ell.Stroke = Brushes.Black;
                        ell.Fill = RadialBrush;
                        button.Content = ell;
                    });
                }
            }
            
        }
        #endregion

        #region Hàm Lấy 1 Button trong bàn cờ
        public CaroButton GetButton(int row, int col)
        {
            for (int i = 0; i < ugrid.Children.Count; i++)
            {
                if (ugrid.Children[i] is CaroButton)
                {
                    if (row == ((CaroButton)ugrid.Children[i]).X && col == ((CaroButton)ugrid.Children[i]).Y)
                        return (CaroButton)ugrid.Children[i];
                }
            }
            return null;
        }
        #endregion

        #region Hàm cho sự kiện thông báo thắng khi chơi offline
        private void Board_onWinner(CellPlayer player)
        {

            if (Type == PlayingType.PvCom)
            {
                if (player == CellPlayer.Player2)
                {
                    MessageBox.Show("COM is Winer!");
                }
                else
                    MessageBox.Show(player.ToString() + " is Winer!");
            }
            else
                MessageBox.Show(player.ToString() + " is Winer!");
            Board.isEndGame = true;
            ugrid.Dispatcher.Invoke(() => ugrid.Children.Clear());
            VeBanCo();
            Type = PlayingType.PvP;
        }
        #endregion

        #region Hàm vẽ Bàn cờ
        private void VeBanCo()
        {
            int size = Properties.Settings.Default.BOAR_SIZE;
            for (int i = 1; i <= size; i++)
                for (int j = 1; j <= size; j++)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        CaroButton Cell = new CaroButton();
                        Cell.X = i;
                        Cell.Y = j;
                        Cell.Width = ugrid.Width / size;
                        Cell.Height = Cell.Width;
                        Cell.BorderBrush = Brushes.Black;
                        Cell.BorderThickness = new Thickness(0.01f);
                        Grid.SetRow(Cell, i);
                        Grid.SetColumn(Cell, j);
                        Grid.SetZIndex(Cell, 1);
                        Cell.Click += Cell_Click;
                        if ((i % 2 == 0 && j % 2 == 0) || (i % 2 != 0 && j % 2 != 0))
                        {
                            Cell.Background = Brushes.White;
                            ugrid.Children.Add(Cell);

                        }
                        else
                        {
                            RadialGradientBrush RadialBrush = new RadialGradientBrush();
                            RadialBrush.GradientOrigin = new Point(0.5, 0.5);
                            RadialBrush.GradientStops.Add(new GradientStop(Colors.White, 0.0));
                            RadialBrush.GradientStops.Add(new GradientStop(Colors.Gray, 2));
                            Cell.Background = RadialBrush;
                            ugrid.Children.Add(Cell);

                        }
                    });
                }
        }
        #endregion

        #region Hàm ListenData khi chơi Online
        public void ListenData()
        {
            socket = IO.Socket(Settings.Default.IP_server);
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                Dispatcher.Invoke(() =>
                {
                    //   Lview.Items.Add("Connected");
                });
            });
            socket.On(Socket.EVENT_MESSAGE, (data) =>
            {
                Dispatcher.Invoke(() =>
                {
                    Lview.Items.Add(((Newtonsoft.Json.Linq.JObject)data).ToString());
                });
            });
            socket.On(Socket.EVENT_CONNECT_ERROR, (data) =>
            {
                if (!isError)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Lview.Items.Add("XẢY RA LỖI KẾT NỐI VỚI SERVER");
                    });
                    isError = true;
                }
            });

            socket.On(Socket.EVENT_ERROR, (data) =>
            {
                Dispatcher.Invoke(() =>
                {
                    Lview.Items.Add(((Newtonsoft.Json.Linq.JObject)data).ToString());
                });
            });

            #region Socket nhận ChatMessage
            socket.On("ChatMessage", (data) =>
            {
                string Message = ((Newtonsoft.Json.Linq.JObject)data)["message"].ToString();
                string Msg = "";
                DateTime dt = DateTime.Now;
                #region Tin nhắn Welcome!
                if (Message == "Welcome!")
                {
                    Dispatcher.Invoke(() =>
                    {
                        TextBlock tblock = new TextBlock();
                        tblock.FontWeight = FontWeights.Bold;
                        Msg = "Server";
                        Msg += "\t\t\t\t" + dt.ToLongTimeString();
                        tblock.Text = Msg;
                        Lview.Items.Add(tblock);
                        Lview.Items.Add(Message);
                        Msg = "------------------------------------------------------";
                        Lview.Items.Add(Msg);
                        ClientName = txtName.Text.Trim();
                    });
                    
                    if(!ClientName.Equals("Guest"))
                    {
                        socket.Emit("MyNameIs", ClientName);
                    }
                    socket.Emit("ConnectToOtherPlayer");

                }
                #endregion

                #region Tin nhắn thông báo kết nối và thứ tự 2 người chơi
                else if (Message.Contains("<br />"))
                {
                    int index = Message.IndexOf("<br />");
                    string s1 = Message.Substring(0, index);
                    string s2 = Message.Substring(index + 6);
                    Dispatcher.Invoke(() =>
                    {
                        TextBlock tblock = new TextBlock();
                        tblock.FontWeight = FontWeights.Bold;
                        Msg = "Server";
                        Msg += "\t\t\t\t" + dt.ToLongTimeString();
                        tblock.Text = Msg;
                        Lview.Items.Add(tblock);
                        Lview.Items.Add(s1);
                        Lview.Items.Add(s2);
                        Msg = "------------------------------------------------------";
                        Lview.Items.Add(Msg);
                    });
                    // Nếu kiểu chơi là máy tự chơi online thì thực hiện kiểm tra thứ tự người chơi
                    // Nếu là người chơi thứ nhất thì cho máy tiến hành tự đánh trước vị trí giữa bàn cờ
                    if(Type == PlayingType.ComOnline)
                    {
                        if (s2 != "You are the second player!")
                        {
                            Board.ActivePlayer = CellPlayer.Player2;
                            Board.PlayAtOnline(Board.BOARD_SIZE / 2 + 1, Board.BOARD_SIZE / 2 + 1);
                            socket.Emit("MyStepIs", JObject.FromObject(new { row = Board.BOARD_SIZE / 2, col = Board.BOARD_SIZE / 2}));
                        }
                    }

                }
                #endregion

                #region Tin nhắn từ người chơi khác
                else if (((Newtonsoft.Json.Linq.JObject)data).Count > 1)
                {
                    Dispatcher.Invoke(() =>
                    {
                        TextBlock tblock = new TextBlock();
                        tblock.FontWeight = FontWeights.Bold;
                        Msg = ((Newtonsoft.Json.Linq.JObject)data)["from"].ToString();
                        Msg += "\t\t\t\t" + dt.ToLongTimeString();
                        tblock.Text = Msg;
                        Lview.Items.Add(tblock);
                        Lview.Items.Add(Message);
                        Msg = "------------------------------------------------------";
                        Lview.Items.Add(Msg);
                    });
                }
                #endregion

                #region Tin nhắn bình thường từ Server
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        TextBlock tblock = new TextBlock();
                        tblock.FontWeight = FontWeights.Bold;
                        Msg = "Server";
                        Msg += "\t\t\t\t" + dt.ToLongTimeString();
                        tblock.Text = Msg;
                        Lview.Items.Add(tblock);
                        Lview.Items.Add(Message);
                        Msg = "------------------------------------------------------";
                        Lview.Items.Add(Msg);
                    });
                }
                #endregion
            });
            #endregion

            #region Socket nhận thông báo kết thúc lượt chơi
            socket.On("EndGame", (data) =>
            {
                Dispatcher.Invoke(() =>
                {
                    string Msg;
                    TextBlock tblock = new TextBlock();
                    tblock.FontWeight = FontWeights.Bold;
                    Msg = "Server";
                    Msg += "\t\t\t\t" + DateTime.Now.ToLongTimeString();
                    tblock.Text = Msg;
                    Lview.Items.Add(tblock);
                    Msg = ((Newtonsoft.Json.Linq.JObject)data)["message"].ToString();
                    Lview.Items.Add(Msg);
                    Msg = "------------------------------------------------------";
                    Lview.Items.Add(Msg);
                    Dispatcher.Invoke(() =>
                    {
                        ugrid.Children.Clear();
                        VeBanCo();
                    });
                    if(Type == PlayingType.ComOnline)
                    {
                        Board.ResetBoard();
                        Board.isEndGame = true;
                        btnComOnline.Content = "New Game!";
                    }
                    else if(Type == PlayingType.POnline)
                    {
                        btnMain.Content = "New Game!";
                    }
                });
            });
            #endregion

            #region Socket nhận thông tin nước đi
            socket.On("NextStepIs", (data) =>
            {
                int player = int.Parse(((Newtonsoft.Json.Linq.JObject)data)["player"].ToString());
                int row = int.Parse(((Newtonsoft.Json.Linq.JObject)data)["row"].ToString());
                int col = int.Parse(((Newtonsoft.Json.Linq.JObject)data)["col"].ToString());
                CaroButton button = (CaroButton)this.Dispatcher.Invoke(new GetButtonDelegate(GetButton), row + 1, col + 1);
                // player = 0 là nước đi của người chơi
                if (player == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Ellipse ell = new Ellipse();
                        ell.Width = ell.Height = 25;
                        ell.Fill = Brushes.Black;
                        button.Content = ell;
                    });
                }
                // player = 1 là nước đi của đối thủ
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        Ellipse ell = new Ellipse();
                        ell.Width = ell.Height = 25;
                        RadialGradientBrush RadialBrush = new RadialGradientBrush();
                        RadialBrush.GradientOrigin = new Point(0.9, 0.9);
                        RadialBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.0));
                        RadialBrush.GradientStops.Add(new GradientStop(Colors.White, 0.75));
                        ell.Stroke = Brushes.Black;
                        ell.Fill = RadialBrush;
                        button.Content = ell;
                    });
                    // Nếu người chơi là máy tự động chơi thì thực hiện tự động tìm nước đi để đánh
                    if(Type == PlayingType.ComOnline)
                    {
                        Board.ActivePlayer = CellPlayer.Player1;
                        Board.PlayAtOnline(row + 1, col + 1);
                        Thread Com = new Thread(Board.ComPlayOnline);
                        Com.IsBackground = true;
                        Com.Start();
                    }
                }
            });
            #endregion
        }
        #endregion

        #region Hàm click Ô cờ
        void Cell_Click(object sender, RoutedEventArgs e)
        {
            CaroButton temp = (CaroButton)sender;
            if (Board.BoardCells[temp.X, temp.Y] == CellPlayer.None && (Type == PlayingType.PvP || Type == PlayingType.PvCom))
            {
                if (Type == PlayingType.PvP)
                {
                    if (Board.ActivePlayer == CellPlayer.Player1)
                    {
                        Ellipse ell = new Ellipse();
                        ell.Width = ell.Height = 25;
                        ell.Fill = Brushes.Black;
                        temp.Content = ell;
                    }

                    else
                    {
                        Ellipse ell = new Ellipse();
                        ell.Width = ell.Height = 25;
                        RadialGradientBrush RadialBrush = new RadialGradientBrush();
                        RadialBrush.GradientOrigin = new Point(0.9, 0.9);
                        RadialBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.0));
                        RadialBrush.GradientStops.Add(new GradientStop(Colors.White, 0.75));
                        ell.Stroke = Brushes.Black;
                        ell.Fill = RadialBrush;
                        temp.Content = ell;
                    }
                    Board.PlayAt(temp.X, temp.Y);
                    if (Board.isEndGame)
                        Board.ResetBoard();
                }
                else if (Type == PlayingType.PvCom)
                {
                    if (Board.ActivePlayer == CellPlayer.Player1)
                    {
                        Ellipse ell = new Ellipse();
                        ell.Width = ell.Height = 25;
                        ell.Fill = Brushes.Black;
                        temp.Content = ell;
                        Board.PlayAt(temp.X, temp.Y);
                        if(!Board.isEndGame)
                        {
                            Board.ActivePlayer = CellPlayer.Player2;
                            Thread Com = new Thread(Board.ComPlay);
                            Com.IsBackground = true;
                            Com.Start();
                        }
                        else
                        {
                            Board.ResetBoard();
                        }
                        
                    }
                }
            }
            else if (Type == PlayingType.POnline)
            {
                socket.Emit("MyStepIs", JObject.FromObject(new {row = temp.X - 1, col = temp.Y - 1}));
            }
        }
        #endregion

        #region Button Send Click
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (Type == PlayingType.POnline || Type == PlayingType.ComOnline)
            {
                
                    ClientName = txtMsg.Text.Trim();
                    socket.Emit("ChatMessage", ClientName);
            }
            else
                MessageBox.Show("Bạn chưa New game để kết nối với Server", "Thông báo", MessageBoxButton.OK);
        }
        #endregion

        #region Button Main Click
        private void btnMain_Click(object sender, RoutedEventArgs e)
        {
            
            if (Type == PlayingType.POnline)
            {
                if(btnMain.Content.Equals("New Game!"))
                {
                    socket.Emit("ConnectToOtherPlayer");
                    btnMain.Content = "Change!";
                }
                else
                {
                    ClientName = txtName.Text.Trim();
                    socket.Emit("MyNameIs", ClientName);
                }
            }
            else
            {
                Type = PlayingType.POnline;
                ugrid.Dispatcher.Invoke(() => ugrid.Children.Clear());
                VeBanCo();
                btnMain.Content = "Change!";
                if (socket != null && thread != null)
                {
                    socket.Close();
                    thread.Interrupt();
                }
                thread = new Thread(ListenData);
                thread.IsBackground = true;
                thread.Start();
            }
        }
        #endregion
         
        #region Button PvsP Click
        private void btnPvP_Click(object sender, RoutedEventArgs e)
        {
            Type = PlayingType.PvP;
            if(socket != null && thread != null)
            {
                socket.Close();
                thread.Interrupt();
                btnMain.Content = "Start!";
                btnComOnline.Content = "Start!";
            }
            Lview.Items.Clear();
            Board.ResetBoard();
            ugrid.Children.Clear();
            VeBanCo();
        }
        #endregion

        #region Button PvsC Click
        private void btnPvC_Click(object sender, RoutedEventArgs e)
        {
            Type = PlayingType.PvCom;
            if (socket != null && thread != null)
            {
                socket.Close();
                thread.Interrupt();
                btnMain.Content = "Start!";
                btnComOnline.Content = "Start!";
            }
            Lview.Items.Clear();
            Board.ResetBoard();
            ugrid.Children.Clear();
            VeBanCo();
            if (MessageBox.Show("Bạn có muốn chơi trước không?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                CaroButton center = GetButton(Board.BOARD_SIZE / 2 + 1, Board.BOARD_SIZE / 2 + 1);
                if (center != null)
                {
                    Board.ActivePlayer = CellPlayer.Player2;
                    Ellipse ell = new Ellipse();
                    ell.Width = ell.Height = 25;
                    RadialGradientBrush RadialBrush = new RadialGradientBrush();
                    RadialBrush.GradientOrigin = new Point(0.9, 0.9);
                    RadialBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.0));
                    RadialBrush.GradientStops.Add(new GradientStop(Colors.White, 0.75));
                    ell.Stroke = Brushes.Black;
                    ell.Fill = RadialBrush;
                    center.Content = ell;
                    Board.PlayAt(center.X, center.Y);
                }

            }

        }
        #endregion

        #region Button ComOnline Click
        private void btnComOnline_Click(object sender, RoutedEventArgs e)
        {

            if (Type == PlayingType.ComOnline)
            {
                Board.isEndGame = false;
                if (btnComOnline.Content.Equals("New Game!"))
                {
                    socket.Emit("ConnectToOtherPlayer");
                    btnComOnline.Content = "Change!";
                }
                else
                {
                    ClientName = txtName.Text.Trim();
                    socket.Emit("MyNameIs", ClientName);
                }
            }
            else
            {
                Type = PlayingType.ComOnline;
                Board.isEndGame = false;
                ugrid.Dispatcher.Invoke(() => ugrid.Children.Clear());
                VeBanCo();
                btnComOnline.Content = "Change!";
                btnMain.Content = "Start!";
                if (socket != null && thread != null)
                {
                    socket.Close();
                    thread.Interrupt();
                }
                thread = new Thread(ListenData);
                thread.IsBackground = true;
                thread.Start();
            }
        }
        #endregion
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

    }
}
