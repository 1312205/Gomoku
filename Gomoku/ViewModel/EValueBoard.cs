using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku
{
    class EValueBoard
    {
        public int Size;
        public int[,] Board;

        // ************ CONSTRUCTOR ******************************
        public EValueBoard(int Size)
        {
            this.Size = Size;
            Board = new int[Size + 2, Size + 2];

            ResetBoard();
        }

        // ************ ADDING FUNCTION **************************
        public void ResetBoard()
        {
            for (int r = 0; r < Size + 2; r++)
                for (int c = 0; c < Size + 2; c++)
                    Board[r, c] = 0;
        }

        public Node GetMaxNode()
        {
            int r, c, MaxValue = 0;
            Node n = new Node();

            for (r = 1; r <= Size; r++)
                for (c = 1; c <= Size; c++)
                    if (Board[r, c] > MaxValue)
                    {
                        n.Row = r; n.Column = c;
                        MaxValue = Board[r, c];
                    }

            return n;
        }
    }
}
