using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gomoku
{

    public enum CellPlayer
    {
        Out = -1,
        None = 0,
        Player1 = 1,
        Player2 = 2,
    }
    public struct Node
    {
        public int Row;
        public int Column;
    }
    class BoardViewModel
    {
        
        #region Define
        public int BOARD_SIZE = Properties.Settings.Default.BOAR_SIZE;
        public CellPlayer[,] BoardCells;
        public CellPlayer ActivePlayer;
        public bool isEndGame;
        public EValueBoard EBoard;

        // Phong thu chat, tan cong nhanh.
        public int[] TScore = new int[5] { 0, 1, 9, 85, 769 };
        public int[] KScore = new int[5] { 0, 2, 28, 256, 2308 };


        public delegate void PlayerWinHanler(CellPlayer player);
        public event PlayerWinHanler onWinner;
        public delegate void ComAutoPlayHanler(Node node);
        public event ComAutoPlayHanler ComAutoPlay;

        #endregion
        
        public BoardViewModel()
        {
            // Khai báo bàn cờ có kích thước BOARD_SIZE + 2 để tạo vùng biên cho việc kiểm tra thắng thua và phát sinh nước đi cho máy
            BoardCells = new CellPlayer[BOARD_SIZE + 2, BOARD_SIZE + 2]; 

            EBoard = new EValueBoard(BOARD_SIZE);
            ActivePlayer = CellPlayer.Player1;
            isEndGame = false;
        }

        // Định nghĩa hàm Reset bàn cờ
        public void ResetBoard()
        {
            int r, c;
            //Thiet lap lai gia tri bang.
            for (r = 0; r < BOARD_SIZE + 2; r++)
                for (c = 0; c < BOARD_SIZE + 2; c++)
                {
                    if (r == 0 || c == 0 || r == BOARD_SIZE + 1 || c == BOARD_SIZE + 1)
                        BoardCells[r, c] = CellPlayer.Out;
                    else BoardCells[r, c] = CellPlayer.None;
                }
            ActivePlayer = CellPlayer.Player1;
            isEndGame = false;
        }

        // Định nghĩa hàm chơi cờ offline tại ô [row, col]
        public void PlayAt(int row, int col)
        {
            BoardCells[row, col] = ActivePlayer;

            CellPlayer Test = TestWiner(row, col);
            if (Test != CellPlayer.None)
            {
                if (onWinner != null)
                    onWinner(Test);
            }
            if (ActivePlayer == CellPlayer.Player1)
            {
                ActivePlayer = CellPlayer.Player2;
            }
            else
            {
                ActivePlayer = CellPlayer.Player1;
            }
        }

        // Định nghĩa hàm chơi cờ online tại ô [row, col] 
        public void PlayAtOnline(int row, int col)
        {
            BoardCells[row, col] = ActivePlayer;
            if (ActivePlayer == CellPlayer.Player1)
            {
                ActivePlayer = CellPlayer.Player2;
            }
            else
            {
                ActivePlayer = CellPlayer.Player1;
            }
        }

        // Định nghĩa hàm máy tự động chơi online
        public void ComPlayOnline()
        {
            EBoard.ResetBoard();
            // Gọi hàm phát sinh nước đi cho máy
            GetGenResult();
            Random rand = new Random();
            int count = rand.Next(4);
            Node node = new Node();
            if (Win) // Tim thay.
            {
                node = WinMoves[1];
            }
            else
            {
                EBoard.ResetBoard();
                // Lượng giá theo người chơi là máy.
                EValueBoardViewModel(CellPlayer.Player2);
                node = EBoard.GetMaxNode();
                if (!Lose)
                    for (int i = 0; i < count; i++)
                    {
                        EBoard.Board[node.Row, node.Column] = 0;
                        node = EBoard.GetMaxNode();
                    }
            }
            PlayAtOnline(node.Row, node.Column);
            if (!isEndGame)
            {
                if (ComAutoPlay != null)
                    ComAutoPlay(node);  // Gọi thực hiện sự kiện ComAutoPlay
            }
            else
                ResetBoard();
        }
        // Định nghĩa hàm máy tự động chơi
        public void ComPlay()
        {
            EBoard.ResetBoard();
            // Gọi hàm phát sinh nước đi cho máy
            GetGenResult();
            Random rand = new Random();
            int count = rand.Next(4);
            Node node = new Node();
            if (Win) // Tim thay.
            {
                node = WinMoves[1];
            }
            else
            {
                EBoard.ResetBoard();
                // Lượng giá theo người chơi là máy.
                EValueBoardViewModel(CellPlayer.Player2);
                node = EBoard.GetMaxNode();
                if (!Lose)
                    for (int i = 0; i < count; i++)
                    {
                        EBoard.Board[node.Row, node.Column] = 0;
                        node = EBoard.GetMaxNode();
                    }
            }
            PlayAt(node.Row, node.Column);

            if (!isEndGame)
            {
                if (ComAutoPlay != null)
                    ComAutoPlay(node);  // Gọi thực hiện sự kiện ComAutoPlay
            }
            else
                ResetBoard();
            
        }

        #region Kiểm tra thắng thua
        // Hàm kiểm tra thắng thua
        private CellPlayer TestWiner(int row, int col)
        {
            if (TestRow(row) || TestColumn(col) || TestDiagonalDown(row, col) || TestDiagonalUp(row, col))
            {
                return BoardCells[row, col];
            }
            else
                return CellPlayer.None;
        }

        // Kiểm tra thắng thua trên dòng
        private bool TestRow(int row)
        {
            bool Player1, Player2;
            int c = 1;
            int i;

            // Kiem tra tren hang...
            while (c <= BOARD_SIZE - 4)
            {
                Player1 = true; Player2 = true;

                for (i = 0; i < 5; i++)
                {
                    if (BoardCells[row, c + i] != CellPlayer.Player1)
                        Player1 = false;
                    if (BoardCells[row, c + i] != CellPlayer.Player2)
                        Player2 = false;
                }

                if (Player1 && (BoardCells[row, c - 1] != CellPlayer.Player2 || BoardCells[row, c + 5] != CellPlayer.Player2)
                    || Player2 && (BoardCells[row, c - 1] != CellPlayer.Player1 || BoardCells[row, c + 5] != CellPlayer.Player1))
                {
                    return true;
                }
                c++;
            }
            return false;

        }

        // Kiểm tra thắng thua trên cột
        private bool TestColumn(int col)
        {
            bool Player1, Player2;
            int r = 1;
            int i;

            // Kiem tra tren hang...
            while (r <= BOARD_SIZE - 4)
            {
                Player1 = true; Player2 = true;

                for (i = 0; i < 5; i++)
                {
                    if (BoardCells[r + i, col] != CellPlayer.Player1)
                        Player1 = false;
                    if (BoardCells[r + i, col] != CellPlayer.Player2)
                        Player2 = false;
                }

                if (Player1 && (BoardCells[r - 1, col] != CellPlayer.Player2 || BoardCells[r + 5, col] != CellPlayer.Player2)
                    || Player2 && (BoardCells[r - 1, col] != CellPlayer.Player1 || BoardCells[r + 5, col] != CellPlayer.Player1))
                {
                    return true;
                }
                r++;
            }
            return false;
        }

        // Kiểm tra thắng thua theo đường chéo xuống
        private bool TestDiagonalDown(int row, int col)
        {
            // Kiem tra tren duong cheo xuong.
            bool Player1, Player2;
            int r = row, c = col;
            int i;
            // Di chuyen den dau duong cheo xuong.
            while (r > 1 && c > 1)
            { r--; c--; }

            while (r <= BOARD_SIZE - 4 && c <= BOARD_SIZE - 4)
            {
                Player1 = true; Player2 = true;

                for (i = 0; i < 5; i++)
                {
                    if (BoardCells[r + i, c + i] != CellPlayer.Player1)
                        Player1 = false;
                    if (BoardCells[r + i, c + i] != CellPlayer.Player2)
                        Player2 = false;
                }

                if (Player1 && (BoardCells[r - 1, c - 1] != CellPlayer.Player2 || BoardCells[r + 5, c + 5] != CellPlayer.Player2)
                    || Player2 && (BoardCells[r - 1, c - 1] != CellPlayer.Player1 || BoardCells[r + 5, c + 5] != CellPlayer.Player1))
                {
                    return true;
                }
                r++; c++;
            }
            return false;
        }

        // Kiểm tra thắng thua theo đường chéo lên
        private bool TestDiagonalUp(int row, int col)
        {
            bool Player1, Player2;
            int r = row, c = col;
            int i;
            // Di chuyen den dau duong cheo len...
            while (r < BOARD_SIZE && c > 1) { r++; c--; }
            while (r >= 5 && c <= BOARD_SIZE - 4)
            {
                Player1 = true; Player2 = true;
                for (i = 0; i < 5; i++)
                {
                    if (BoardCells[r - i, c + i] != CellPlayer.Player1)
                        Player1 = false;
                    if (BoardCells[r - i, c + i] != CellPlayer.Player2)
                        Player2 = false;
                }

                if (Player1 && (BoardCells[r + 1, c - 1] != CellPlayer.Player2 || BoardCells[r - 5, c + 5] != CellPlayer.Player2)
                    || Player2 && (BoardCells[r + 1, c - 1] != CellPlayer.Player1 || BoardCells[r - 5, c + 5] != CellPlayer.Player1))
                {
                    return true;
                }
                r--; c++;
            }
            return false;
        }
        #endregion

        // Lượng giá bàn cờ theo người chơi player
        private void EValueBoardViewModel(CellPlayer player)
        {
            int rw, cl, i;
            int countPlayer, countCom;
            #region Lượng giá cho hàng
            // Luong gia cho hang.
            for (rw = 1; rw <= BOARD_SIZE; rw++)
                for (cl = 1; cl <= BOARD_SIZE - 4; cl++)
                {
                    countPlayer = 0;
                    countCom = 0;
                    for (i = 0; i < 5; i++)
                    {
                        if (BoardCells[rw, cl + i] == CellPlayer.Player1) countPlayer++; // Player1 đóng vai trò người chơi
                        if (BoardCells[rw, cl + i] == CellPlayer.Player2) countCom++;             // Player2 đóng vai trò là máy
                    }
                    // Luong gia...
                    if (countPlayer * countCom == 0 && countPlayer != countCom)
                        for (i = 0; i < 5; i++)
                            if (BoardCells[rw, cl + i] == CellPlayer.None)
                            {
                                if (countCom == 0)
                                {
                                    if (player == CellPlayer.Player2) EBoard.Board[rw, cl + i] += TScore[countPlayer];
                                    else EBoard.Board[rw, cl + i] += KScore[countPlayer];
                                    // Truong hop bi chan 2 dau.
                                    if (BoardCells[rw, cl - 1] == CellPlayer.Player2 && BoardCells[rw, cl + 5] == CellPlayer.Player2)
                                        EBoard.Board[rw, cl + i] = 0;
                                }
                                if (countPlayer == 0)
                                {
                                    if (player == CellPlayer.Player1) EBoard.Board[rw, cl + i] += TScore[countCom];
                                    else EBoard.Board[rw, cl + i] += KScore[countCom];
                                    // Truong hop bi chan 2 dau.
                                    if (BoardCells[rw, cl - 1] == CellPlayer.Player1 && BoardCells[rw, cl + 5] == CellPlayer.Player1)
                                        EBoard.Board[rw, cl + i] = 0;
                                }
                                if ((countPlayer == 4 || countCom == 4)
                                    && (BoardCells[rw, cl + i - 1] == CellPlayer.None || BoardCells[rw, cl + i + 1] == CellPlayer.None))
                                    EBoard.Board[rw, cl + i] *= 2;
                            }
                }
            #endregion

            #region Lượng giá cho cột
            for (cl = 1; cl <= BOARD_SIZE; cl++)
                for (rw = 1; rw <= BOARD_SIZE - 4; rw++)
                {
                    countPlayer = 0;
                    countCom = 0;
                    for (i = 0; i < 5; i++)
                    {
                        if (BoardCells[rw + i, cl] == CellPlayer.Player1) countPlayer++; // Player1 đóng vai trò người chơi
                        if (BoardCells[rw + i, cl] == CellPlayer.Player2) countCom++;             // Player2 đóng vai trò là máy
                    }
                    // Luong gia...
                    if (countPlayer * countCom == 0 && countPlayer != countCom)
                        for (i = 0; i < 5; i++)
                            if (BoardCells[rw + i, cl] == CellPlayer.None)
                            {
                                if (countCom == 0)
                                {
                                    if (player == CellPlayer.Player2) EBoard.Board[rw + i, cl] += TScore[countPlayer];
                                    else EBoard.Board[rw + i, cl] += KScore[countPlayer];
                                    // Truong hop bi chan 2 dau.
                                    if (BoardCells[rw - 1, cl] == CellPlayer.Player2 && BoardCells[rw + 5, cl] == CellPlayer.Player2)
                                        EBoard.Board[rw + i, cl] = 0;
                                }
                                if (countPlayer == 0)
                                {
                                    if (player == CellPlayer.Player1) EBoard.Board[rw + i, cl] += TScore[countCom];
                                    else EBoard.Board[rw + i, cl] += KScore[countCom];
                                    // Truong hop bi chan 2 dau.
                                    if (BoardCells[rw - 1, cl] == CellPlayer.Player1 && BoardCells[rw + 5, cl] == CellPlayer.Player1)
                                        EBoard.Board[rw + i, cl] = 0;
                                }
                                if ((countPlayer == 4 || countCom == 4)
                                    && (BoardCells[rw + i - 1, cl] == CellPlayer.None || BoardCells[rw + i + 1, cl] == CellPlayer.None))
                                    EBoard.Board[rw + i, cl] *= 2;
                            }
                }
            #endregion

            #region Lượng giá cho đường chéo xuống

            for (rw = 1; rw <= BOARD_SIZE - 4; rw++)
                for (cl = 1; cl <= BOARD_SIZE - 4; cl++)
                {
                    countPlayer = 0;
                    countCom = 0;
                    for (i = 0; i < 5; i++)
                    {
                        if (BoardCells[rw + i, cl + i] == CellPlayer.Player1) countPlayer++; // Player1 đóng vai trò người chơi
                        if (BoardCells[rw + i, cl + i] == CellPlayer.Player2) countCom++;             // Player2 đóng vai trò là máy
                    }
                    // Luong gia...
                    if (countPlayer * countCom == 0 && countPlayer != countCom)
                        for (i = 0; i < 5; i++)
                            if (BoardCells[rw + i, cl + i] == CellPlayer.None)
                            {
                                if (countCom == 0)
                                {
                                    if (player == CellPlayer.Player2) EBoard.Board[rw + i, cl + i] += TScore[countPlayer];
                                    else EBoard.Board[rw + i, cl + i] += KScore[countPlayer];
                                    // Truong hop bi chan 2 dau.
                                    if (BoardCells[rw - 1, cl - 1] == CellPlayer.Player2 && BoardCells[rw + 5, cl + 5] == CellPlayer.Player2)
                                        EBoard.Board[rw + i, cl + i] = 0;
                                }
                                if (countPlayer == 0)
                                {
                                    if (player == CellPlayer.Player1) EBoard.Board[rw + i, cl + i] += TScore[countCom];
                                    else EBoard.Board[rw + i, cl + i] += KScore[countCom];
                                    // Truong hop bi chan 2 dau.
                                    if (BoardCells[rw - 1, cl - 1] == CellPlayer.Player1 && BoardCells[rw + 5, cl + 5] == CellPlayer.Player1)
                                        EBoard.Board[rw + i, cl + i] = 0;
                                }
                                if ((countPlayer == 4 || countCom == 4)
                                    && (BoardCells[rw + i - 1, cl + i - 1] == CellPlayer.None || BoardCells[rw + i + 1, cl + i + 1] == CellPlayer.None))
                                    EBoard.Board[rw + i, cl + i] *= 2;
                            }
                }
            #endregion

            #region Lượng giá cho đường chéo lên
            for (rw = 5; rw <= BOARD_SIZE - 4; rw++)
                for (cl = 1; cl <= BOARD_SIZE - 4; cl++)
                {
                    countPlayer = 0;
                    countCom = 0;
                    for (i = 0; i < 5; i++)
                    {
                        if (BoardCells[rw - i, cl + i] == CellPlayer.Player1) countPlayer++; // Player1 đóng vai trò người chơi
                        if (BoardCells[rw - i, cl + i] == CellPlayer.Player2) countCom++;             // Player2 đóng vai trò là máy
                    }
                    // Luong gia...
                    if (countPlayer * countCom == 0 && countPlayer != countCom)
                        for (i = 0; i < 5; i++)
                            if (BoardCells[rw - i, cl + i] == CellPlayer.None)
                            {
                                if (countCom == 0)
                                {
                                    if (player == CellPlayer.Player2) EBoard.Board[rw - i, cl + i] += TScore[countPlayer];
                                    else EBoard.Board[rw - i, cl + i] += KScore[countPlayer];
                                    // Truong hop bi chan 2 dau.
                                    if (BoardCells[rw + 1, cl - 1] == CellPlayer.Player2 && BoardCells[rw - 5, cl + 5] == CellPlayer.Player2)
                                        EBoard.Board[rw - i, cl + i] = 0;
                                }
                                if (countPlayer == 0)
                                {
                                    if (player == CellPlayer.Player1) EBoard.Board[rw - i, cl + i] += TScore[countCom];
                                    else EBoard.Board[rw - i, cl + i] += KScore[countCom];
                                    // Truong hop bi chan 2 dau.
                                    if (BoardCells[rw + 1, cl - 1] == CellPlayer.Player1 && BoardCells[rw - 5, cl + 5] == CellPlayer.Player1)
                                        EBoard.Board[rw - i, cl + i] = 0;
                                }
                                if ((countPlayer == 4 || countCom == 4)
                                    && (BoardCells[rw - i + 1, cl + i - 1] == CellPlayer.None || BoardCells[rw - i - 1, cl + i + 1] == CellPlayer.None))
                                    EBoard.Board[rw - i, cl + i] *= 2;
                            }
                }
            #endregion

        }

        // Sinh nuoc di - do thong minh cua may.
        public int Depth = 0;
        static public int MaxDepth = 21;
        static public int MaxBreadth = 8;

        public Node[] WinMoves = new Node[MaxDepth + 1];
        public Node[] MyMoves = new Node[MaxBreadth + 1];
        public Node[] HisMoves = new Node[MaxBreadth + 1];
        public bool Win, Lose;
        // Ham de quy - Sinh nuoc di cho may.
        public void GenerateMoves()
        {
            if (Depth >= MaxDepth) return;
            Depth++;
            bool lose = false;
            Win = false;

            Node MyNode = new Node();   // Duong di quan ta.
            Node HisNode = new Node();  // Duong di doi thu.
            int count = 0;

            // Luong gia cho ma tran.
            EValueBoardViewModel(CellPlayer.Player2);

            // Lay MaxBreadth nuoc di tot nhat.
            for (int i = 1; i <= MaxBreadth; i++)
            {
                MyNode = EBoard.GetMaxNode();
                MyMoves[i] = MyNode;
                EBoard.Board[MyNode.Row, MyNode.Column] = 0;
            }
            // Lay nuoc di ra khoi danh sach - Danh thu nuoc di.
            count = 0;
            while (count < MaxBreadth)
            {
                count++;
                MyNode = MyMoves[count];
                WinMoves.SetValue(MyNode, Depth);
                BoardCells[MyNode.Row, MyNode.Column] = CellPlayer.Player2;

                // Tim cac nuoc di toi uu cua doi thu.
                EBoard.ResetBoard();
                EValueBoardViewModel(CellPlayer.Player1);

                for (int i = 1; i <= MaxBreadth; i++)
                {
                    HisNode = EBoard.GetMaxNode();
                    HisMoves[i] = HisNode;
                    EBoard.Board[HisNode.Row, HisNode.Column] = 0;
                }

                for (int i = 1; i <= MaxBreadth; i++)
                {
                    HisNode = HisMoves[i];
                    BoardCells[HisNode.Row, HisNode.Column] = CellPlayer.Player1;
                    // Kiem tra ket qua nuoc di.
                    if (TestWiner(MyNode.Row, MyNode.Column) == CellPlayer.Player2)
                        Win = true;
                    if (TestWiner(HisNode.Row, HisNode.Column) == CellPlayer.Player1)
                        lose = true;

                    if (lose)
                    {
                        // Loai nuoc di thu.
                        Lose = true;
                        BoardCells[HisNode.Row, HisNode.Column] = CellPlayer.None;
                        BoardCells[MyNode.Row, MyNode.Column] = CellPlayer.None;
                        return;
                    }

                    if (Win)
                    {
                        // Loai nuoc di thu.
                        BoardCells[HisNode.Row, HisNode.Column] = CellPlayer.None;
                        BoardCells[MyNode.Row, MyNode.Column] = CellPlayer.None;
                        return;
                    }
                    else GenerateMoves(); // tim tiep.
                    // Loai nuoc di thu.
                    BoardCells[HisNode.Row, HisNode.Column] = CellPlayer.None;
                }

                BoardCells[MyNode.Row, MyNode.Column] = CellPlayer.None;
            }

        }

        // Hàm lấy nước đi cho máy
        public void GetGenResult()
        {
            Win = Lose = false;
            // Xoa mang duong di.
            WinMoves = new Node[MaxDepth + 1];
            for (int i = 0; i <= MaxDepth; i++)
                WinMoves[i] = new Node();

            // Xoa stack.
            for (int i = 0; i < MaxBreadth; i++)
                MyMoves[i] = new Node();

            Depth = 0;
            GenerateMoves();
        }
     

    }
}
