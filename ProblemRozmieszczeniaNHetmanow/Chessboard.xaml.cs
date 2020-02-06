using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProblemRozmieszczeniaNHetmanow
{
    /// <summary>
    /// Logika interakcji dla klasy Chessboard.xaml
    /// </summary>
    public partial class Chessboard : Window
    {
        int hetmanNumber;
        const int squareSize = 30;
        public Chessboard(int hetmanNumber)
        {
            InitializeComponent();
            this.hetmanNumber = hetmanNumber;
            PlaceHetmans(hetmanNumber);
        }
        private void InitializeChessboard(int[,] chessBoard)
        {

            LayoutRoot.Children.Clear();
            GridLengthConverter myGridLengthConverter = new GridLengthConverter();
            GridLength side = (GridLength)myGridLengthConverter.ConvertFromString("Auto");
            for (int i = 0; i < hetmanNumber; i++)
            {
                LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition());
                LayoutRoot.ColumnDefinitions[i].Width = side;
                LayoutRoot.RowDefinitions.Add(new RowDefinition());
                LayoutRoot.RowDefinitions[i].Height = side;
            }

            Rectangle[,] square = new Rectangle[hetmanNumber, hetmanNumber];
            for (int row = 0; row < hetmanNumber; row++)
                for (int col = 0; col < hetmanNumber; col++)
                {
                    square[row, col] = new Rectangle();
                    square[row, col].Height = 800/ hetmanNumber;
                    square[row, col].Width = 800 / hetmanNumber;
                    Grid.SetColumn(square[row, col], col);
                    Grid.SetRow(square[row, col], row);
                    if ((row + col) % 2 == 0)
                    {
                       
                        square[row, col].Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                        if (chessBoard[row, col] == 2)
                        {
                            square[row, col].Fill = new ImageBrush
                            {
                                ImageSource = new BitmapImage(new Uri("..\\..\\..\\indeks.png", UriKind.Relative))
                            };
                        }
                    }
                    else
                    {
                       
                        square[row, col].Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                        if (chessBoard[row, col] == 2)
                        {
                            square[row, col].Fill = new ImageBrush
                            {
                                ImageSource = new BitmapImage(new Uri("..\\..\\..\\indeks_black.png", UriKind.Relative))
                            };
                        }
                    }
                   
                    LayoutRoot.Children.Add(square[row, col]);
                }
        }



        private void PlaceHetmans(int boardSize)
        {
            bool flag = false;

            List<List<QueenBoard>> openNodes = new List<List<QueenBoard>>();
            List<QueenBoard> closedNodes = new List<QueenBoard>();

            int[,] chessBoard = new int[boardSize, boardSize];

            for (int row = 0; row < boardSize; row++)
            {
                int result = SetQueenInRow(boardSize, row, ref chessBoard, ref openNodes, ref closedNodes, ref flag);
                row -= result;
                // Console.WriteLine(row);
            }
            InitializeChessboard(chessBoard);
        }

        private int SetQueenInRow(int size, int rowPosition, ref int[,] chessBoard, ref List<List<QueenBoard>> openNodes, ref List<QueenBoard> closedNodes, ref bool flag)
        {
            int returnCounter = 0;
            if (CheckIfCanPlaceAnotherHetman(size, rowPosition, chessBoard) == false)
            {
                List<QueenBoard> currentRowNodes = new List<QueenBoard>();

                for (int i = openNodes.Count; i > 0; i--)
                {
                    //operationNumber++;
                    currentRowNodes = openNodes[i - 1];
                    // Sprawdz czy sa dostepne nowe rozwiazania i czy to rozwiazanie nie jest juz odwiedzone
                    var currentOpenedNodes = GetRowOpenedNodes(currentRowNodes, closedNodes);
                    currentOpenedNodes.Reverse();
                    // Jeśli brak możliwości ustawienia to nie pobierać ustawienia
                    if (currentOpenedNodes.Count != 0 && CheckIfCanPlaceAnotherHetman(size, i, currentOpenedNodes[0].chessBoardWithQueen) == true)
                    {
                        chessBoard = currentOpenedNodes[0].chessBoardWithQueen;
                        currentRowNodes.Remove(currentRowNodes[0]);
                        //closedNodes.Add(currentOpenedNodes[0]);
                        break;
                    }
                    returnCounter++;
                }
                flag = true;
                returnCounter++;
                return returnCounter;
            }
            else if (flag == true)
            {
                GetHeuristicOfThisRowAndSuccesor(rowPosition, ref openNodes, size);
                flag = false;
            }

            List<QueenBoard> queenAttackedFields = new List<QueenBoard>();
            QueenBoard queenAttackedField;

            int[,] board = new int[size, size];
            int[,] chessBoardCopy = new int[size, size];

            for (int columnPosition = 0; columnPosition < size; columnPosition++)
            {
                Array.Copy(chessBoard, chessBoardCopy, chessBoard.Length);
                board = MarkAttackedField(chessBoardCopy, size, rowPosition, columnPosition);
                var heuristic = CountAttackedField(board, size, rowPosition);

                if (heuristic > 0)
                {
                    queenAttackedField = new QueenBoard(rowPosition, columnPosition, heuristic, board);
                    queenAttackedFields.Add(queenAttackedField);
                }

            }
            // posortowanie listy obecnego wiersza w kolejnosci rosnacej czyli 1 element bedzie elementem najlepszym
            queenAttackedFields.Sort((x, y) => x.heuristic.CompareTo(y.heuristic));
            // pobranie najlepszego rozwiązania
            var currentBestHeuristicValue = queenAttackedFields[0];
            // dodanie rozwiązania do już odwiedzonych
            //closedNodes.Add(currentBestHeuristicValue);
            // usuniecie tego rozwiązania z listy rozwiązań obecnego wiersza
            queenAttackedFields.Remove(queenAttackedFields[0]);
            // dodanie listy rozwiązań do kontenera wszystkich rozwiązań
            openNodes.Add(queenAttackedFields);
            // skopiowanie do chessboard najlepszego obecnie ustawienia
            //chessBoard = CopyArray(currentBestHeuristicValue.chessBoardWithQueen, chessBoard, size);
            Array.Copy(currentBestHeuristicValue.chessBoardWithQueen, chessBoard, currentBestHeuristicValue.chessBoardWithQueen.Length);
            // wyczyszczenie listy rozwiazań obecnego wiersz
            return 0;
            // wez zerowy element == najmniejszy
        }

        private void GetHeuristicOfThisRowAndSuccesor(int rowPosition, ref List<List<QueenBoard>> openNodes, int size)
        {
            // iteruj od końca --> usuń liste wierzchołków --> aż dojdziesz do obecnego wierchołka
            int numberOfNodes = openNodes.Count; // liczba list np 4
            for (int i = numberOfNodes - 1; i >= rowPosition; i--) // to  row = 3
            {
                openNodes.Remove(openNodes[i]);
            }
        }

        private List<QueenBoard> GetRowOpenedNodes(List<QueenBoard> currentRowNodes, List<QueenBoard> closedNodes)
        {
            List<QueenBoard> openNodes = new List<QueenBoard>();
            foreach (var node in currentRowNodes)
            {
                if (!closedNodes.Contains(node))
                {
                    openNodes.Add(node);
                }
            }
            return openNodes;
        }

        private bool CheckIfCanPlaceAnotherHetman(int size, int rowPosition, int[,] chessBoard)
        {

            for (int i = 0; i < size; i++)
            {
                if (chessBoard[rowPosition, i] == 0)
                {
                    return true;
                }
            }
            return false;
        }

        // ---> zwraca liczbe atakowanych pól
        private int CountAttackedField(int[,] attackedFieldTemp, int size, int rowPosition)
        {
            // Miejsce na poprawe wydajności --> już się troche poprawiło
            int counter = 0;
            for (int i = rowPosition; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (attackedFieldTemp[i, j] == 1)
                    {
                        counter++;
                    }
                }
            }
            return counter;
        }
        // ---> zwraca tablice atakowanych pozycji plus pozycje hetmana
        private int[,] MarkAttackedField(int[,] chessBoard, int size, int queenPositionX, int queenPositionY) // to gówno poprawic 
        {

            int queenStartPosition = queenPositionX - queenPositionY;
            int queenStartPositionSum = queenPositionX + queenPositionY;
            int[,] chessBoardCopy = new int[size, size];
            //CopyArray(chessBoard, chessBoardCopy, size);
            Array.Copy(chessBoard, chessBoardCopy, chessBoard.Length);
            // change row attacked cells to 1
            if (chessBoardCopy[queenPositionX, queenPositionY] != 2 && chessBoardCopy[queenPositionX, queenPositionY] != 1)
            {


                for (int y = 0; y < size; y++)
                {
                    if (chessBoardCopy[queenPositionX, y] != 1 && chessBoardCopy[queenPositionX, y] != 2) // jesli jest to atakowane miejsce to nie licz heurystyki
                    {
                        if (queenPositionY == y)
                        {
                            chessBoardCopy[queenPositionX, y] = 2;
                        }
                        else
                        {
                            chessBoardCopy[queenPositionX, y] = 1;
                        }
                    }
                }

                // change column attacked cells to 1
                for (int x = 0; x < size; x++)
                {
                    if (chessBoardCopy[x, queenPositionY] != 1 && chessBoardCopy[x, queenPositionY] != 2) // jesli jest to atakowane miejsce to nie licz heurystyki
                    {
                        if (x == queenPositionX)
                        {
                            chessBoardCopy[x, queenPositionY] = 2;
                        }
                        else
                        {
                            chessBoardCopy[x, queenPositionY] = 1;
                        }
                    }
                }
                // change 1 diagonal cells to 1
                int j = 0;
                if (queenStartPosition < 0)
                {
                    queenStartPosition = Math.Abs(queenStartPosition);
                    for (int i = queenStartPosition; i < size; i++)
                    {
                        if (chessBoardCopy[j, i] != 1 && chessBoardCopy[j, i] != 2) // jesli jest to atakowane miejsce to nie licz heurystyki
                        {
                            if (i == queenPositionX)
                            {
                                chessBoardCopy[j, i] = 2;
                            }
                            else
                            {
                                chessBoardCopy[j, i] = 1;
                            }

                        }
                        j++;
                    }
                }
                else
                {
                    for (int i = queenStartPosition; i < size; i++)
                    {
                        if (chessBoardCopy[i, j] != 1 && chessBoardCopy[i, j] != 2) // jesli jest to atakowane miejsce to nie licz heurystyki
                        {
                            if (i == queenPositionX)
                            {
                                chessBoardCopy[i, j] = 2;
                            }
                            else
                            {
                                chessBoardCopy[i, j] = 1;
                            }
                        }
                        j++;
                    }
                }
                // change 1 diagonal cells to 1
                j = 0;
                if (queenStartPositionSum < size)
                {

                    for (int i = queenStartPositionSum; i >= 0; i--)
                    {
                        if (chessBoardCopy[i, j] != 1 && chessBoardCopy[i, j] != 2) // jesli jest to atakowane miejsce to nie licz heurystyki
                        {
                            if (i == queenPositionX)
                            {
                                chessBoardCopy[i, j] = 2;
                            }
                            else
                            {
                                chessBoardCopy[i, j] = 1;
                            }
                        }
                        j++;
                    }
                }
                else
                {
                    j = size - 1;
                    int suma = queenStartPositionSum - (size - 1);
                    for (int i = suma; i < size; i++)
                    {
                        if (chessBoardCopy[j, i] != 1 && chessBoardCopy[j, i] != 2) // jesli jest to atakowane miejsce to nie licz heurystyki
                        {
                            if (j == queenPositionX && i == queenPositionY)
                            {
                                chessBoardCopy[j, i] = 2;   // i == queenPosition
                            }
                            else
                            {
                                chessBoardCopy[j, i] = 1;
                            }
                        }
                        j--;
                    }
                }
            }
            else
            {
                return chessBoardCopy = new int[size, size];
            }

            return chessBoardCopy;
        }
    }
}



