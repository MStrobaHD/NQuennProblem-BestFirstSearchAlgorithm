using System;
using System.Collections.Generic;
using System.Text;

namespace ProblemRozmieszczeniaNHetmanow
{
    public class QueenBoard
    {

        public QueenBoard(int positionX, int positionY, int heuristic, int[,] chessBoardWithQueen)
        {
            this.positionX = positionX;
            this.positionY = positionY;
            this.heuristic = heuristic;
            this.chessBoardWithQueen = chessBoardWithQueen;
        }

        public int positionX { get; set; }
        public int positionY { get; set; }
        public int heuristic { get; set; }
        public int[,] chessBoardWithQueen { get; set; }
    }
}
