using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections;

namespace ACT4
{
    public partial class Form1 : Form
    {
        int side;
        int n = 6;
        SixState startState;
        SixState currentState;
        int moveCounter;
        int beam = 6;
        int[,] hTable;
        ArrayList bMoves;
        Object chosenMove;
        Boolean Solved = true;

        public Form1()
        {
            InitializeComponent();

            side = pictureBox1.Width / n;

            startState = randomSixState();
            currentState = new SixState(startState);

            updateUI();
            label1.Text = "Attacking pairs: " + getAttackingPairs(startState);
            label6.Text = "K = ";

            numericUpDown1.Value = beam;
        }

        private void updateUI()
        {
            pictureBox2.Refresh();

            label3.Text = "Attacking pairs: " + getAttackingPairs(currentState);
            label4.Text = "Moves: " + moveCounter;
            hTable = getHeuristicTableForPossibleMoves(currentState);
            bMoves = getBestMoves(hTable);

            listBox1.Items.Clear();
            foreach (Point move in bMoves)
            {
                listBox1.Items.Add(move);
            }

            if (bMoves.Count > 0)
                chosenMove = chooseMove(bMoves);
            label2.Text = "Chosen move: " + chosenMove;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Blue, i * side, j * side, side, side);
                    }
                    if (j == startState.Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if ((i + j) % 2 == 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Black, i * side, j * side, side, side);
                    }
                    if (j == currentState.Y[i])
                        e.Graphics.FillEllipse(Brushes.Fuchsia, i * side, j * side, side, side);
                }
            }
        }

        private SixState randomSixState()
        {
            Random r = new Random();
            SixState random = new SixState(r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n),
                                             r.Next(n));

            return random;
        }

        private int getAttackingPairs(SixState f)
        {
            int attackers = 0;

            for (int rf = 0; rf < n; rf++)
            {
                for (int tar = rf + 1; tar < n; tar++)
                {
                    if (f.Y[rf] == f.Y[tar])
                        attackers++;
                }
                for (int tar = rf + 1; tar < n; tar++)
                {
                    if (f.Y[tar] == f.Y[rf] + tar - rf)
                        attackers++;
                }
                for (int tar = rf + 1; tar < n; tar++)
                {
                    if (f.Y[rf] == f.Y[tar] + tar - rf)
                        attackers++;
                }
            }

            return attackers;
        }

        private int[,] getHeuristicTableForPossibleMoves(SixState thisState)
        {
            int[,] hStates = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    SixState possible = new SixState(thisState);
                    possible.Y[i] = j;
                    hStates[i, j] = getAttackingPairs(possible);
                }
            }

            return hStates;
        }

        private ArrayList getBestMoves(int[,] heuristicTable)
        {
            // Local Beam Search:
            // Get k randomized
            // Get k best of each possible
            // Repeat until h = 0

            ArrayList bestMoves = new ArrayList();
            ArrayList movesWithHeuristics = new ArrayList();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Point move = new Point(i, j);
                    if (currentState.Y[i] != j)
                    {
                        movesWithHeuristics.Add(new Tuple<int, Point>(heuristicTable[i, j], move));
                    }
                }
            }
            movesWithHeuristics.Sort(new ComparisonComparer<Tuple<int, Point>>((x, y) => x.Item1.CompareTo(y.Item1)));
            ArrayList sortedMoves = new ArrayList();
            foreach (Tuple<int, Point> moveWithHeuristic in movesWithHeuristics)
            {
                sortedMoves.Add(moveWithHeuristic.Item2);
            }
            if (movesWithHeuristics.Count > 0)
            {
                label5.Text = "Possible Moves (H=" + ((Tuple<int, Point>)movesWithHeuristics[0]).Item1 + ")";
            }
            int k = Math.Min(beam, sortedMoves.Count);
            return new ArrayList(sortedMoves.GetRange(0, k));
        }

        public class ComparisonComparer<T> : IComparer
        {
            private readonly Comparison<T> comparison;

            public ComparisonComparer(Comparison<T> comparison)
            {
                this.comparison = comparison;
            }

            public int Compare(object x, object y)
            {
                return comparison((T)x, (T)y);
            }
        }

        private ArrayList previousMoves = new ArrayList();

        private Object chooseMove(ArrayList possibleMoves)
        {
            if (possibleMoves.Count == 0)
                return null;
            ArrayList movesWithHeuristics = new ArrayList();
            foreach (Point move in possibleMoves)
            {
                int heuristicValue = getHeuristicValueForMove(move);
                movesWithHeuristics.Add(new Tuple<int, Point>(heuristicValue, move));
            }
            movesWithHeuristics.Sort(new ComparisonComparer<Tuple<int, Point>>((x, y) => x.Item1.CompareTo(y.Item1)));
            for (int i = 0; i < movesWithHeuristics.Count; i++)
            {
                Point currentMove = ((Tuple<int, Point>)movesWithHeuristics[i]).Item2;
                if (!previousMoves.Contains(currentMove))
                {
                    previousMoves.Add(currentMove);
                    if (previousMoves.Count > 30)
                    {
                        previousMoves.RemoveAt(0);
                    }
                    return currentMove;
                }
            }
            Point bestMove = ((Tuple<int, Point>)movesWithHeuristics[0]).Item2;
            previousMoves.Add(bestMove);
            if (previousMoves.Count > 30)
            {
                previousMoves.RemoveAt(0);
            }
            return bestMove;
        }




        private int getHeuristicValueForMove(Point move)
        {
            SixState possibleState = new SixState(currentState);
            possibleState.Y[move.X] = move.Y;
            return getAttackingPairs(possibleState);
        }

        private void executeMove(Point move)
        {
            for (int i = 0; i < n; i++)
            {
                startState.Y[i] = currentState.Y[i];
            }
            currentState.Y[move.X] = move.Y;
            moveCounter++;

            chosenMove = null;
            updateUI();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            numericUpDown1.Enabled = false;

            if (getAttackingPairs(currentState) > 0)
                executeMove((Point)chosenMove);

            if (getAttackingPairs(currentState) == 0)
            {
                MessageBox.Show("Solved in " + moveCounter + " moves!");
                Solved = true;
                numericUpDown1.Enabled = true;
            }
                
        }

        private void button3_Click(object sender, EventArgs e)
        {
            startState = randomSixState();
            currentState = new SixState(startState);

            moveCounter = 0;

            updateUI();
            pictureBox1.Refresh();
            label1.Text = "Attacking pairs: " + getAttackingPairs(startState);

            Solved = false;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            while (getAttackingPairs(currentState) > 0)
            {
                executeMove((Point)chosenMove);
            }

            if (getAttackingPairs(currentState) == 0)
            {
                MessageBox.Show("Solved in " + moveCounter + " moves!");
                Solved = true;
                numericUpDown1.Enabled = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (Solved)
            {
                beam = (int)numericUpDown1.Value;
            } 
                
        }
    }
}
