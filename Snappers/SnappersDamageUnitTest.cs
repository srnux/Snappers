using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Snappers
{
    [TestClass]
    public class SnappersDamageUnitTest
    {
        #region Enums

        private enum Color
        {
            None=0,
            Red=1,
            Green=2,
            Orange=3,
            Blue=4            
        }

        private enum Direction
        {
            // Original:
            Down=0,
            Right=1,
            Up=2,
            Left=3
            /*
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3,*/
        }

        #endregion

        #region Structures

        private class ShrapnelNode
        {
            public int X;
            public int Y;
            public Direction Direction;
            public int TimeStep;
            public bool EndOfRoad = false;
        }

        private class TapResult
        {
            public int X;
            public int Y;
            public Color[,] Result;
            public int Estimate;
        }

        #endregion

        #region Fields

        private List<ShrapnelNode> Shrapnels;
        private List<TapResult> Results;

        #endregion

        #region Solver methods

        /// <summary>
        /// Find a solution to the Snappers problem.
        /// </summary>
        /// <param name="parentMatrix"></param>
        private void Solve(Color[,] parentMatrix, int maxSteps)
        {
            Results = new List<TapResult>();

            if (SolveRecursion(parentMatrix, 0, maxSteps))
            {
                Debug.Print("The board was solved. Results:");
                int i = 0;
                foreach (var item in Results)
                {
                    Debug.Print(String.Format("{0:D3}: X = {1} Y = {2}", i, item.X, item.Y));
                }
            }
            else
            {
                Debug.Print("The board was NOT solved.");
            }
        }

        /// <summary>
        /// Shows the first solution starting with a defined click. 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="maxSteps"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void SolveWithStartClick(Color[,] matrix, int maxSteps, int x, int y)
        {
            Results = new List<TapResult>();

            TapResult res = Tap(matrix, x, y);
            Results.Add(res);

            if (SolveRecursion(res.Result, 0, maxSteps - 1))
            {
                Debug.Print("The board was solved. Results:");
                int i = 0;
                foreach (var item in Results)
                {
                    Debug.Print(String.Format("{0:D3}: X = {1} Y = {2}", i, item.X, item.Y));
                }
            }
            else
            {
                Debug.Print("The board was NOT solved.");
            }
        }

        /// <summary>
        /// Find all solutions to the Snappers problem (testing purposses).
        /// </summary>
        /// <param name="parentMatrix"></param>
        private void SolveAllSolutions(Color[,] parentMatrix, int maxSteps, bool definedFirstPos = false, int x = 0, int y = 0)
        {
            Results = new List<TapResult>();
            SolveRecursionAllSolutions(parentMatrix, 0, maxSteps, definedFirstPos, x, y);
        }

        /// <summary>
        /// The recursion called by Solve method. Solves the first found solution.
        /// </summary>
        /// <param name="parentMatrix"></param>
        /// <param name="currentStep"></param>
        /// <param name="maxSteps"></param>
        /// <returns></returns>
        private bool SolveRecursion(Color[,] parentMatrix, int currentStep, int maxSteps)
        {
            List<TapResult> possibleBoardClicks = new List<TapResult>();

            // If the board is solved, return true
            if (Solved(parentMatrix))
            {
                return true;
            }

            // If we reached the maximum set number of clicks, return false
            if (currentStep == maxSteps)
            {
                return false;
            }

            possibleBoardClicks = FindPossibleClicks(parentMatrix, true, maxSteps - currentStep);

            // For each solution repeat the process
            foreach (var item in possibleBoardClicks)
            {
                Results.Add(item);

                if (SolveRecursion(item.Result, currentStep + 1, maxSteps) == false)
                {
                    Results.Remove(item);
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The recursion called by SolveAllSolutions method. Displays all solutions found. For testing purposses.
        /// </summary>
        /// <param name="parentMatrix"></param>
        /// <param name="currentStep"></param>
        /// <param name="maxSteps"></param>
        /// <returns></returns>
        private bool SolveRecursionAllSolutions(Color[,] parentMatrix, int currentStep, int maxSteps, bool definedFirstPos = false, int x = 0, int y = 0)
        {
            List<TapResult> possibleBoardClicks = new List<TapResult>();

            // If the board is solved, return true
            if (Solved(parentMatrix))
            {
                if (definedFirstPos && (Results[0].X != x || Results[0].Y != y))
                    return true;

                Debug.Print("The board was solved. Results:");
                int i = 0;
                foreach (var item in Results)
                {
                    Debug.Print(String.Format("{0:D3}: X = {1} Y = {2}", i, item.X, item.Y));
                }
                Debug.Print("");

                return true;
            }
            
            // If we reached the maximum set number of clicks, return false
            if (currentStep == maxSteps)
            {
                return false;
            }

            possibleBoardClicks = FindPossibleClicks(parentMatrix, false, maxSteps - currentStep);

            bool res = false;

            // For each solution repeat the process
            foreach (var item in possibleBoardClicks)
            {
                Results.Add(item);

                res = SolveRecursionAllSolutions(item.Result, currentStep + 1, maxSteps, definedFirstPos, x, y);

                Results.Remove(item);
            }

            return res;
        }

        /// <summary>
        /// Tests if the board has been solved.
        /// </summary>
        /// <param name="parentMatrix"></param>
        /// <returns></returns>
        private bool Solved(Color[,] parentMatrix)
        {
            foreach (var element in parentMatrix)
            {
                if (element > Color.None) {return false;}
            }
            return true;
        }

        /// <summary>
        /// Finds all possible clicks on a single board, their result states and the result evaluations.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        private List<TapResult> FindPossibleClicks(Color[,] matrix, bool optimizeForSingleSolution, int clicksLeft)
        {
            List<TapResult> result = new List<TapResult>();
            
            // Find all possible clicks on the board, add them to the possible solutions list
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    if ((matrix[x, y] != Color.None) && ((int)matrix[x,y] <= clicksLeft))
                    {
                        Color[,] cloneMatrix = matrix.Clone() as Color[,];
                        TapResult r = Tap(cloneMatrix, x, y);

                        result.Add(r);

                        // If the last result was a solution, there's no need to check for other results
                        if ((optimizeForSingleSolution) && (r.Estimate == Int32.MaxValue))
                        {
                            return result;
                        }
                    }
                }
            }

            // Sort the list
            return result.OrderByDescending(q => q.Estimate).ToList();
        }

        #endregion

        #region Tap functionality

        /// <summary>
        /// Taps a single field. If a field has a red critter on it, calls Blast method.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        private TapResult Tap(Color[,] matrix, int i, int j)
        {
            if (matrix[i, j] == Color.None)
            {
                // Failed tap, should not ever happen
                return new TapResult { X = i, Y = j, Result = matrix, Estimate = -1000 };
            }
            else if (matrix[i, j] == Color.Red)
            {
                // Blast happened
                matrix[i, j]--;

                return ImproveResult(Blast(matrix, i, j));
            }
            else 
            {
                // No explosion
                matrix[i, j]--;

                return ImproveResult(new TapResult { X = i, Y = j, Result = matrix, Estimate = 1000 });
            }
        }

        /// <summary>
        /// Creates initial shrapnels and does a breath-first expansion of the shrapnel tree. Each tree 
        /// level represents a point in time.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        private TapResult Blast(Color[,] matrix, int i, int j)
        {
            int timestep = -1;
            int estimate = 1000;

            // Clear the global shrapnel list
            Shrapnels = new List<ShrapnelNode>();

            // Create the initial shrapnels
            CreateShrapnelChildren(i, j, 0);

            // Start the breath-first graph expansion
            do
            {
                // Next timestep
                timestep++;

                // Get all elements on the current timestep
                List<ShrapnelNode> currentShrapnels = GetShrapnelsInTimeStep(timestep);

                foreach (var node in currentShrapnels)
                {
                    // Move the shrapnel according to its direction
                    Move(node);

                    node.TimeStep++;

                    // Test if overboard
                    if (Overboard(node.X, node.Y))
                    {
                        // Stop the shrapnel
                        Shrapnels.Remove(node);

                        continue;
                    }

                    // Test if critter hit
                    if (matrix[node.X, node.Y] > Color.None)
                    {
                        // Test if critter red
                        if (matrix[node.X, node.Y] == Color.Red)
                        {
                            // The explosion happens, new shrapnels are created
                            CreateShrapnelChildren(node.X, node.Y, timestep);
                        }

                        // Change critter value
                        matrix[node.X, node.Y]--;

                        // Stop the parent shrapnel
                        Shrapnels.Remove(node);

                        // Increase estimate
                        estimate += 1000;
                    }
                }
            }
            while (Shrapnels.Count > 0);

            // If this has solved the puzzle, estimate should be highest
            if (Solved(matrix))
            {
                estimate = Int32.MaxValue;
            }

            return new TapResult { X = i, Y = j, Result = matrix, Estimate = estimate };
        }

        /// <summary>
        /// Creates the children shrapnels which emanate from the point. The time children are positioned 
        /// and the time is increased. The shrapnels are also added to the global shrapnel list.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="currentTimeStep"></param>
        /// <returns></returns>
        private void CreateShrapnelChildren(int x, int y, int currentTimeStep)
        {
            for (int i = 0; i < 4; i++)
            {
                var Shrapnel = new ShrapnelNode
                {
                    Direction = (Direction)i,
                    X = x,
                    Y = y,
                    TimeStep = currentTimeStep,
                };

                Shrapnels.Add(Shrapnel);
            }
        }

        /// <summary>
        /// Returns all the shrapnels in one point of the time. This is done before any of them are marked as EndOfTheRoad.
        /// They are ordered by the Direction. This should prioritize movements by the direction. First all the shrapnels 
        /// going down, then right, up, left.
        /// </summary>
        /// <param name="timestep"></param>
        /// <returns></returns>
        private List<ShrapnelNode> GetShrapnelsInTimeStep(int timestep)
        {
            return Shrapnels.ToList();//.OrderBy(q => q.Direction).ToList();//.Where(q => q.TimeStep == timestep && q.EndOfRoad == false).OrderBy(q => q.Direction).ToList();
        }

        /// <summary>
        /// Move the shrapnel according to its direction for a single step.
        /// </summary>
        /// <param name="shrapnel"></param>
        private ShrapnelNode Move(ShrapnelNode shrapnel)
        {
            switch (shrapnel.Direction)
            {
                case Direction.Down:
                    shrapnel.Y++;
                    break;
                case Direction.Right:
                    shrapnel.X++;
                    break;
                case Direction.Up:
                    shrapnel.Y--;
                    break;
                case Direction.Left:
                    shrapnel.X--;
                    break;
            }

            return shrapnel;
        }

        /// <summary>
        /// Tests if the shrapnel went outside bounds.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool Overboard(int x, int y)
        {
            return ! ((x >=0 && x<5) && (y>=0 && y<6));
        }

        /// <summary>
        /// Tests if any other node can be hit from the current one.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool CanNodeHitOtherNodes(Color[,] matrix, int x, int y)
        {
            int i;

            // Up
            i = y - 1;
            while (i >= 0)
            {
                if (matrix[x, i] > Color.None)
                {
                    return true;
                }
                i--;
            }

            // Down
            i = y + 1;
            while (i <= 5)
            {
                if (matrix[x, i] > Color.None)
                {
                    return true;
                }
                i++;
            }

            // Left
            i = x - 1;
            while (i >= 0)
            {
                if (matrix[i, y] > Color.None)
                {
                    return true;
                }
                i--;
            }

            // Right
            i = x + 1;
            while (i <= 4)
            {
                if (matrix[i, y] > Color.None)
                {
                    return true;
                }
                i++;
            }

            return false;
        }

        /// <summary>
        /// Improves the estimate so that the boards with the same values can be sorted even better.
        /// 
        /// What we want is more red critters than less critters with biger values.
        /// We also want less graphs (preferably one). A graph is consisted of the critters that can 
        /// hit each other. 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private TapResult ImproveResult(TapResult i)
        {
            int graphs = 1;
            int nodesEstimate = 0;

            // This is already at the maximum
            if (i.Estimate == Int32.MaxValue)
            {
                return i;
            }

            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    if (i.Result[x,y] > Color.None)
                    {
                        if (!CanNodeHitOtherNodes(i.Result, x, y))
                        {
                            graphs++;
                        }

                        nodesEstimate += (5 - (int)i.Result[x, y]) * 100;
                    }
                }
            }

            i.Estimate += nodesEstimate / graphs;

            return i;
        }

        #endregion

        #region Test methods

        [TestMethod]
        public void TestImproveResult()
        {
            Color[,] matrix = new Color[5, 6];

            matrix[0, 0] = Color.Red;
            matrix[0, 4] = Color.Blue;
            matrix[4, 0] = Color.Green;
            matrix[4, 4] = Color.Orange;
            matrix[2, 2] = Color.Blue;

            TapResult r = ImproveResult(new TapResult { Estimate = 0, Result = matrix, X = 0, Y = 0 });
        }


        [TestMethod]
        public void DoAndysLevel()
        {
            try
            {
                var matrix = new Color[5, 6];

                AndysLevel(out matrix);

                SolveWithStartClick(matrix, 2, 1, 5);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        [TestMethod]
        public void DoLevel5()
        {
            try
            {
                var matrix = new Color[5, 6];

                SecondLevelBlueTwoTaps(out matrix);

                Solve(matrix, 2);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        [TestMethod]
        public void TapTest1()
        {
            try
            {
                var matrix = new Color[5, 6];

                SecondLevelBlueTwoTaps(out matrix);

                TapResult r = Tap(matrix, 4, 1);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        // new tests

        [TestMethod]
        public void DoLevel6()
        {
            try
            {
                var matrix = new Color[5, 6];

                Level6(out matrix);

                SolveAllSolutions(matrix, 3);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        [TestMethod]
        public void DoLevel67()
        {
            try
            {
                var matrix = new Color[5, 6];

                Level67(out matrix);

                SolveAllSolutions(matrix, 2);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        [TestMethod]
        public void DoLevel34()
        {
            try
            {
                var matrix = new Color[5, 6];

                Level34(out matrix);

                SolveAllSolutions(matrix, 2);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }
        [TestMethod]
        public void DoLevel75()
        {
            try
            {
                var matrix = new Color[5, 6];

                Level75(out matrix);

                SolveAllSolutions(matrix, 2);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }
        
        [TestMethod]
        public void DoLevel76()
        {
            try
            {
                var matrix = new Color[5, 6];

                Level76(out matrix);

                SolveAllSolutions(matrix, 3);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        [TestMethod]
        public void DoLevel78()
        {
            try
            {
                var matrix = new Color[5, 6];

                Level78(out matrix);

                SolveAllSolutions(matrix, 1);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        [TestMethod]
        public void DoLevel52()
        {
            try
            {
                var matrix = new Color[5, 6];

                Level52(out matrix);

                SolveAllSolutions(matrix, 3);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        [TestMethod]
        public void DoLevel46()
        {
            try
            {
                var matrix = new Color[5, 6];

                Level46(out matrix);

                SolveAllSolutions(matrix, 3);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }
        
        [TestMethod]
        public void DoLevel32With5Taps()
        {
            try
            {
                var matrix = new Color[5, 6];

                Level32Taps5(out matrix);

                SolveAllSolutions(matrix, 5);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        [TestMethod]
        public void DoLevel59With6Taps()
        {
            try
            {
                var matrix = new Color[5, 6];

                Level59Taps6(out matrix);

                SolveAllSolutions(matrix, 6);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        [TestMethod]
        public void DoTricky11()
        {
            try
            {
                var matrix = new Color[5, 6];

                Tricky11(out matrix);

                SolveAllSolutions(matrix, 6);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        [TestMethod]
        public void DoThough49()
        {
            try
            {
                var matrix = new Color[5, 6];

                Though49(out matrix);

                SolveAllSolutions(matrix, 6);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }
        
        #endregion

        #region Board filler methods

        private void FirstLevelExtraSimpleExample(out Color[,] matrix)
        {
            matrix = new Color[5, 6];
            matrix[1, 1] = Color.Red;
            matrix[1, 2] = Color.Red;
        }

        private void SecondLevelExtraSimpleGreen(out Color[,] matrix)
        {
            matrix = new Color[5, 6];
            matrix[1, 1] = Color.Red;
            matrix[1, 2] = Color.Green;
        }

        private void FourRedOneGreen(out Color[,] matrix)
        {
            matrix = new Color[5, 6];
            matrix[1, 1] = Color.Red; 
            matrix[1, 2] = Color.Red; 
            matrix[3, 1] = Color.Red; 
            matrix[3, 2] = Color.Green;
            matrix[3, 3] = Color.Red; 
        }

        private void AndysLevel(out Color[,] matrix)
        {
            matrix = new Color[5, 6];
            matrix[2, 2] = Color.Red;
            matrix[2, 5] = Color.Red;
            matrix[4, 5] = Color.Red;
            matrix[1, 5] = Color.Green;
        }

        private void SecondLevelTwoTaps(out Color[,] matrix)
        {
            matrix = new Color[5, 6];
            
            matrix[0, 0] = Color.Red;
            matrix[0, 1] = Color.Green;
            matrix[0, 2] = Color.Red;


            matrix[1, 1] = Color.Green;
            matrix[1,2]=Color.Red;

            matrix[2, 1] = Color.Green;
            matrix[2, 2] = Color.Orange;
            matrix[2, 3] = Color.Red;

            matrix[3, 1] = Color.Green;
            matrix[3, 2] = Color.Red;


            matrix[4, 0] = Color.Red;
            matrix[4, 1] = Color.Green;
            matrix[4, 2] = Color.Red;;
        }

        private void SecondLevelBlueTwoTaps(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 1] = Color.Red;
            matrix[0, 2] = Color.Blue;

            matrix[1, 0] = Color.Red;
            matrix[1, 1] = Color.Red;
            matrix[1, 2] = Color.Red;
            matrix[1, 3] = Color.Red;

            matrix[2, 0] = Color.Red;


            matrix[3, 0] = Color.Red;
            matrix[3, 1] = Color.Red;
            matrix[3, 2] = Color.Red;
            matrix[3, 3] = Color.Red;


            matrix[4, 1] = Color.Red;
            matrix[4, 2] = Color.Blue;
        }

        // new fillers
        private void Level6(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 0] = Color.Red;
            matrix[0, 2] = Color.Red;

            matrix[1, 0] = Color.Green;
            matrix[1, 4] = Color.Blue;
            
            matrix[2, 0] = Color.Green;
            matrix[2, 1] = Color.Green;
            matrix[2, 2] = Color.Red;
            matrix[2, 3] = Color.Green;
            matrix[2, 4] = Color.Green;

            matrix[3, 0] = Color.Red;
            matrix[3, 1] = Color.Green;
            matrix[3, 2] = Color.Red;
            matrix[3, 3] = Color.Green;
            matrix[4, 4] = Color.Green;


            matrix[4, 0] = Color.Red;
            matrix[4, 1] = Color.Green;
            matrix[4, 2] = Color.Green;
        }

        private void Level67(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 0] = Color.Red;
            matrix[0, 1] = Color.Green;
            matrix[0, 2] = Color.Orange;
            matrix[0, 3] = Color.Orange;
            matrix[0, 4] = Color.Red;

            matrix[1, 1] = Color.Red;
            matrix[1, 5] = Color.Red;

            matrix[2, 0] = Color.Green;
            matrix[2, 2] = Color.Red;
            matrix[2, 3] = Color.Green;
            matrix[2, 4] = Color.Green;

            matrix[3, 1] = Color.Red;
            matrix[3, 5] = Color.Red;

            matrix[4, 0] = Color.Red;
            matrix[4, 1] = Color.Green;
            matrix[4, 2] = Color.Orange;
            matrix[4, 3] = Color.Orange;
            matrix[4, 4] = Color.Red;
        }

        private void Level34(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 0] = Color.Orange;
            matrix[0, 1] = Color.Red;
            matrix[0, 2] = Color.Red;
            matrix[0, 4] = Color.Green;
            matrix[0, 5] = Color.Green;

            matrix[1, 2] = Color.Blue;
            matrix[1, 3] = Color.Red;
            matrix[1, 5] = Color.Red;

            matrix[2, 0] = Color.Green;
            matrix[2, 1] = Color.Orange;
            matrix[2, 2] = Color.Red;

            matrix[3, 2] = Color.Blue;
            matrix[3, 3] = Color.Red;
            matrix[3, 5] = Color.Red;

            matrix[4, 0] = Color.Orange;
            matrix[4, 1] = Color.Red;
            matrix[4, 2] = Color.Red;
            matrix[4, 4] = Color.Green;
            matrix[4, 5] = Color.Green;
        }

        private void Level75(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 0] = Color.Green;
            matrix[0, 2] = Color.Blue;
            matrix[0, 3] = Color.Red;
            matrix[0, 4] = Color.Blue;
            matrix[0, 5] = Color.Green;

            matrix[1, 0] = Color.Orange;
            matrix[1, 1] = Color.Red;
            matrix[1, 4] = Color.Green;
            matrix[1, 5] = Color.Orange;

            matrix[2, 0] = Color.Green;
            matrix[2, 1] = Color.Orange;
            matrix[2, 4] = Color.Red;

            matrix[3, 0] = Color.Blue;
            matrix[3, 1] = Color.Red;
            matrix[3, 2] = Color.Red;
            matrix[3, 4] = Color.Green;
            matrix[3, 5] = Color.Green;

            matrix[4, 2] = Color.Green;
            matrix[4, 3] = Color.Green;
            matrix[4, 4] = Color.Red;
            matrix[4, 5] = Color.Red;

        }

        private void Level76(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 0] = Color.Green;
            matrix[0, 1] = Color.Green;
            matrix[0, 2] = Color.Green;


            matrix[1, 0] = Color.Orange;
            matrix[1, 1] = Color.Red;
            matrix[1, 2] = Color.Green;
            matrix[1, 3] = Color.Green;
            matrix[1, 5] = Color.Green;

            matrix[2, 1] = Color.Red;
            matrix[2, 2] = Color.Red;

            matrix[3, 1] = Color.Green;
            matrix[3, 2] = Color.Red;
            matrix[3, 3] = Color.Blue;
            matrix[3, 4] = Color.Green;

            matrix[4, 1] = Color.Green;
            matrix[4, 2] = Color.Red;

        }

        private void Level78(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 0] = Color.Red;
            matrix[0, 3] = Color.Red;
            matrix[0, 4] = Color.Red;
            matrix[0, 5] = Color.Green;


            matrix[1, 0] = Color.Red;
            matrix[1, 1] = Color.Red;
            matrix[1, 3] = Color.Red;
            matrix[1, 5] = Color.Red;

            matrix[2, 2] = Color.Orange;
            matrix[2, 3] = Color.Red;
            matrix[2, 4] = Color.Orange;
            matrix[2, 5] = Color.Red;

            matrix[3, 0] = Color.Red;
            matrix[3, 1] = Color.Red;
            matrix[3, 3] = Color.Red;
            matrix[3, 5] = Color.Red;

            matrix[4, 0] = Color.Red;
            matrix[4, 3] = Color.Red;
            matrix[4, 4] = Color.Red;
            matrix[4, 5] = Color.Green;

        }

        private void Level52(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 2] = Color.Green;
            matrix[0, 3] = Color.Orange;
            matrix[0, 4] = Color.Green;

            matrix[1, 1] = Color.Green;
            matrix[1, 2] = Color.Red;
            matrix[1, 4] = Color.Blue;

            matrix[2, 1] = Color.Red;
            matrix[2, 2] = Color.Red;
            matrix[2, 3] = Color.Red;
            matrix[2, 4] = Color.Orange;
            matrix[2, 5] = Color.Green;

            matrix[3, 1] = Color.Green;
            matrix[3, 2] = Color.Red;
            matrix[3, 4] = Color.Blue;

            matrix[4, 2] = Color.Green;
            matrix[4, 3] = Color.Orange;
            matrix[4, 4] = Color.Green;

        }

        private void Level46(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 2] = Color.Green;
            matrix[0, 3] = Color.Green;

            matrix[1, 0] = Color.Green;
            matrix[1, 2] = Color.Red;
            matrix[1, 3] = Color.Red;
            matrix[1, 5] = Color.Green;

            matrix[2, 0] = Color.Red;
            matrix[2, 2] = Color.Orange;
            matrix[2, 3] = Color.Orange;
            matrix[2, 5] = Color.Red;

            matrix[3, 0] = Color.Green;
            matrix[3, 1] = Color.Orange;
            matrix[3, 2] = Color.Red;
            matrix[3, 3] = Color.Red;
            matrix[3, 4] = Color.Orange;
            matrix[3, 5] = Color.Green;

            matrix[4, 2] = Color.Blue;
            matrix[4, 3] = Color.Blue;

        }

        private void Level32Taps5(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 0] = Color.Blue;
            matrix[0, 1] = Color.Green;
            matrix[0, 2] = Color.Red;
            matrix[0, 3] = Color.Red;
            matrix[0, 4] = Color.Green;
            matrix[0, 5] = Color.Blue;

            matrix[1, 0] = Color.Blue;
            matrix[1, 2] = Color.Green;
            matrix[1, 3] = Color.Green;
            matrix[1, 5] = Color.Blue;

            matrix[2, 1] = Color.Red;
            matrix[2, 2] = Color.Green;
            matrix[2, 3] = Color.Green;
            matrix[2, 4] = Color.Red;

            matrix[3, 0] = Color.Red;
            matrix[3, 5] = Color.Red;

        }

        private void Level59Taps6(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 0] = Color.Green;
            matrix[0, 1] = Color.Red;
            matrix[0, 2] = Color.Red;
            matrix[0, 4] = Color.Red;

            matrix[1, 2] = Color.Green;
            matrix[1, 3] = Color.Blue;

            matrix[2, 1] = Color.Green;
            matrix[2, 2] = Color.Blue;
            matrix[2, 3] = Color.Green;
            matrix[2, 4] = Color.Red;
            matrix[2, 5] = Color.Green;

            matrix[3, 2] = Color.Green;
            matrix[3, 3] = Color.Blue;

            matrix[4, 0] = Color.Green;
            matrix[4, 1] = Color.Red;
            matrix[4, 2] = Color.Red;
            matrix[4, 4] = Color.Red;
        }

        private void Tricky11(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 1] = Color.Red;
            matrix[0, 2] = Color.Green;
            matrix[0, 3] = Color.Blue;
            matrix[0, 4] = Color.Orange;
            matrix[0, 5] = Color.Green;

            matrix[1, 1] = Color.Red;
            matrix[1, 2] = Color.Green;
            matrix[1, 3] = Color.Red;

            matrix[2, 0] = Color.Blue;
            matrix[2, 3] = Color.Red;
            matrix[2, 4] = Color.Green;

            matrix[3, 1] = Color.Red;
            matrix[3, 2] = Color.Green;
            matrix[3, 3] = Color.Red;

            matrix[4, 1] = Color.Red;
            matrix[4, 2] = Color.Green;
            matrix[4, 3] = Color.Blue;
            matrix[4, 4] = Color.Orange;
            matrix[4, 5] = Color.Green;
        }

        private void Though49(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 0] = Color.Green;
            matrix[0, 2] = Color.Green;
            matrix[0, 3] = Color.Orange;
            matrix[0, 4] = Color.Green;

            matrix[1, 2] = Color.Green;
            matrix[1, 3] = Color.Blue;
            matrix[1, 4] = Color.Green;
            matrix[1, 5] = Color.Red;

            matrix[2, 0] = Color.Green;
            matrix[2, 3] = Color.Red;
            matrix[2, 4] = Color.Blue;

            matrix[3, 2] = Color.Green;
            matrix[3, 3] = Color.Blue;
            matrix[3, 4] = Color.Green;
            matrix[3, 5] = Color.Red;

            matrix[4, 0] = Color.Green;
            matrix[4, 2] = Color.Green;
            matrix[4, 3] = Color.Orange;
            matrix[4, 4] = Color.Green;
        }

        private void RealExam1(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 2] = Color.Green;
            matrix[0, 3] = Color.Green;
            
            matrix[2, 2] = Color.Green;
            matrix[2, 3] = Color.Green;

            matrix[3, 1] = Color.Red;
            matrix[3, 2] = Color.Red;
            matrix[3, 3] = Color.Red;
            matrix[3, 4] = Color.Red;

            matrix[4, 1] = Color.Red;
            matrix[4, 4] = Color.Red;
        }


        [TestMethod]
        public void RealTest1()
        {
            try
            {
                var matrix = new Color[5, 6];

                RealExam1(out matrix);

                SolveAllSolutions(matrix, 2);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        private void RealExam2(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 0] = Color.Green;
            matrix[0, 3] = Color.Red;
            matrix[0, 4] = Color.Green;

            matrix[1, 1] = Color.Red;
            matrix[1, 5] = Color.Red;

            matrix[2, 0] = Color.Red;
            matrix[2, 1] = Color.Green;
            matrix[2, 3] = Color.Blue;
            matrix[2, 5] = Color.Orange;

            matrix[3, 1] = Color.Red;
            matrix[3, 5] = Color.Red;

            matrix[4, 0] = Color.Green;
            matrix[4, 3] = Color.Red;
            matrix[4, 4] = Color.Green;
        }

        [TestMethod]
        public void RealTest2()
        {
            try
            {
                var matrix = new Color[5, 6];

                RealExam2(out matrix);

                SolveAllSolutions(matrix, 3);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        private void RealExam3(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 0] = Color.Green;
            matrix[0, 1] = Color.Green;
            matrix[0, 2] = Color.Red;
            matrix[0, 3] = Color.Blue;
            matrix[0, 4] = Color.Red;
            matrix[0, 5] = Color.Green;

            matrix[1, 0] = Color.Red;
            matrix[1, 1] = Color.Red;
            matrix[1, 3] = Color.Green;


            matrix[2, 1] = Color.Blue;
            matrix[2, 3] = Color.Orange;
            matrix[2, 4] = Color.Green;

            matrix[3, 0] = Color.Green;
            matrix[3, 3] = Color.Red;
            matrix[3, 4] = Color.Orange;
            matrix[3, 5] = Color.Red;

            matrix[4, 0] = Color.Blue;
            matrix[4, 2] = Color.Orange;
            matrix[4, 3] = Color.Red;
            matrix[4, 4] = Color.Green;
            matrix[4, 5] = Color.Green;

        }

        [TestMethod]
        public void RealTest3()
        {
            try
            {
                var matrix = new Color[5, 6];

                RealExam3(out matrix);

                Solve(matrix, 3);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }
        private void RealExam4(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 1] = Color.Red;
            matrix[0, 2] = Color.Blue;
            matrix[0, 3] = Color.Blue;
            matrix[0, 4] = Color.Red;
            
            matrix[1, 2] = Color.Red;
            matrix[1, 3] = Color.Red;

            matrix[2, 0] = Color.Red;
            matrix[2, 2] = Color.Red;
            matrix[2, 3] = Color.Red;
            matrix[2, 5] = Color.Red;

            matrix[3, 0] = Color.Red;
            matrix[3, 1] = Color.Red;
            matrix[3, 2] = Color.Blue;
            matrix[3, 3] = Color.Blue;
            matrix[3, 4] = Color.Red;
            matrix[3, 5] = Color.Red;

            matrix[4, 0] = Color.Red;
            matrix[4, 2] = Color.Red;
            matrix[4, 3] = Color.Red;
            matrix[4, 5] = Color.Red;

        }
        [TestMethod]
        public void RealTest4()
        {
            try
            {
                var matrix = new Color[5, 6];

                RealExam4(out matrix);

                Solve(matrix, 1);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        private void RealExam5(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 0] = Color.Green;
            matrix[0, 2] = Color.Green;
            matrix[0, 3] = Color.Blue;
            matrix[0, 4] = Color.Blue;

            matrix[1, 1] = Color.Blue;
            matrix[1, 2] = Color.Red;
            matrix[1, 3] = Color.Green;
            matrix[1, 4] = Color.Blue;
            matrix[1, 5] = Color.Orange;

            matrix[2, 0] = Color.Red;
            matrix[2, 1] = Color.Red;
            matrix[2, 2] = Color.Red;
            matrix[2, 3] = Color.Blue;
            matrix[2, 4] = Color.Red;
            matrix[2, 5] = Color.Red;

            matrix[3, 3] = Color.Red;
            matrix[3, 4] = Color.Green;
            
            matrix[4, 1] = Color.Red;
            matrix[4, 2] = Color.Orange;
            matrix[4, 3] = Color.Red;
            matrix[4, 4] = Color.Red;
            matrix[4, 5] = Color.Green;

        }

        [TestMethod]
        public void RealTest5()
        {
            try
            {
                var matrix = new Color[5, 6];

                RealExam5(out matrix);

                Solve(matrix, 2);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        private void RealExam6(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 0] = Color.Blue;
            matrix[0, 1] = Color.Green;
            matrix[0, 2] = Color.Red;
            matrix[0, 3] = Color.Red;
            matrix[0, 4] = Color.Green;
            matrix[0, 5] = Color.Blue;

            matrix[1, 0] = Color.Blue;
            matrix[1, 2] = Color.Green;
            matrix[1, 3] = Color.Green;
            matrix[1, 5] = Color.Blue;

            matrix[2, 1] = Color.Red;
            matrix[2, 2] = Color.Green;
            matrix[2, 3] = Color.Green;
            matrix[2, 4] = Color.Red;

            matrix[3, 0] = Color.Red;
            matrix[3, 5] = Color.Red;

        }

        [TestMethod]
        public void RealTest6()
        {
            try
            {
                var matrix = new Color[5, 6];

                RealExam6(out matrix);

                Solve(matrix, 5);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }


        private void RealExam7(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 2] = Color.Green;
            matrix[0, 3] = Color.Green;

            matrix[1, 0] = Color.Green;
            matrix[1, 2] = Color.Red;
            matrix[1, 3] = Color.Red;
            matrix[1, 5] = Color.Green;

            matrix[2, 0] = Color.Red;
            matrix[2, 2] = Color.Orange;
            matrix[2, 3] = Color.Orange;
            matrix[2, 5] = Color.Red;

            matrix[3, 0] = Color.Green;
            matrix[3, 1] = Color.Orange;
            matrix[3, 2] = Color.Red;
            matrix[3, 3] = Color.Red;
            matrix[3, 4] = Color.Orange;
            matrix[3, 5] = Color.Green;

            matrix[4, 2] = Color.Blue;
            matrix[4, 3] = Color.Blue;
        }

        [TestMethod]
        public void RealTest7()
        {
            try
            {
                var matrix = new Color[5, 6];

                RealExam7(out matrix);

                Solve(matrix, 3);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        private void RealExam8(out Color[,] matrix)
        {
            matrix = new Color[5, 6];

            matrix[0, 1] = Color.Red;
            matrix[0, 2] = Color.Red;
            matrix[0, 3] = Color.Orange;
            matrix[0, 4] = Color.Red;

            matrix[1, 1] = Color.Green;
            matrix[1, 5] = Color.Orange;

            matrix[2, 0] = Color.Green;
            matrix[2, 1] = Color.Orange;
            matrix[2, 2] = Color.Red;
            matrix[2, 3] = Color.Orange;
            matrix[2, 5] = Color.Green;

            matrix[3, 0] = Color.Blue;
            matrix[3, 1] = Color.Green;
            matrix[3, 2] = Color.Green;
            matrix[3, 4] = Color.Red;
            matrix[3, 5] = Color.Red;
            
            matrix[4, 0] = Color.Red;
            matrix[4, 1] = Color.Red;
            matrix[4, 2] = Color.Green;
            matrix[4, 3] = Color.Blue;
            matrix[4, 4] = Color.Green;

        }

        [TestMethod]
        public void RealTest8()
        {
            try
            {
                var matrix = new Color[5, 6];

                RealExam8(out matrix);

                Solve(matrix, 2);
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }
        #endregion
    }
}
