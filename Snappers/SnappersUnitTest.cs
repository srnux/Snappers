using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Snappers
{
    [TestClass]
    public class SnappersUnitTest
    {
        private enum Colors
        {
            None=0,
            Red=1,
            Green=2,
            Orange=3,
            Blue=4            
        }
        // 5 columns, 6 rows
        private Colors[,] _matrix;
        // damage matrix
        private int[,] _damageMatrix = new int[5, 6];

        public SnappersUnitTest()
        {
            RestoreMatrix();
        }

        private void RestoreMatrix()
        {
            _matrix=new Colors[5,6];
            _matrix[0, 0] = Colors.Green; // green
            _matrix[1, 5] = Colors.Red; // red
            _matrix[4, 2] = Colors.Orange; // orange
            _matrix[4, 3] = Colors.Blue; // blue
            _matrix[1, 2] = Colors.Orange; // orange
            _matrix[4, 5] = Colors.Red; // orange
        }

        private void RestoreMatrixWithNoReds()
        {
            _matrix = new Colors[5, 6];
            _matrix[0, 0] = Colors.Green; 
            _matrix[1, 5] = Colors.Green; 
            _matrix[4, 2] = Colors.Orange; 
            _matrix[4, 3] = Colors.Blue; 
            _matrix[1, 2] = Colors.Orange; 
            _matrix[4, 5] = Colors.Green; 
        }

        [TestMethod]
        public void ExplodeRed()
        {
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        if (_matrix[i, j] == Colors.Red)
                        {
                            Assert.AreEqual(i, 1);
                            Assert.AreEqual(j, 5);
                            Assert.IsTrue( Explode(i, j) );
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }

        }

        [TestMethod]
        public void DamageLevelMatrixFill()
        {
            try
            {
                _damageMatrix = new int[5, 6];
                RestoreMatrix();
                // try each for damage level
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        if (_matrix[i,j]!= Colors.None)
                        {
                            Assert.IsTrue(Explode(i, j));
                            // simulate destruction to get damage level
                            RestoreMatrix();
                        }
                        
                    }
                }
            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        [TestMethod]
        public void DamageLevelMatrixFillWithNoReds()
        {
            try
            {
                RestoreMatrixWithNoReds();
                while (!TryFindReds())
                {
                    IncrementLevel();
                }
                var storeMatrix = _matrix.Clone();

                _damageMatrix = new int[5,6];
                    
                // try each for damage level
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        if (_matrix != null && _matrix[i, j] != Colors.None)
                        {
                            Assert.IsTrue(Explode(i, j));
                            // simulate destruction to get damage level
                            _matrix = storeMatrix as Colors[,]; // restore matrix
                        }

                    }
                }


            }
            catch (Exception e)
            {
                Assert.Fail();
                Debug.WriteLine(e);
            }
        }

        private void IncrementLevel()
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (_matrix[i, j] != Colors.None)
                    {
                        _matrix[i, j]--;
                    }
                }
            }
        }

        private bool Explode(int column, int row)
        {
            var storeHistory = _matrix[column, row];
            
            // self
            _matrix[column, row]--;
            _damageMatrix[column, row]++;
            
            // explode with shrapnels but only if it was red when entered
            if (storeHistory == Colors.Red)
            {
                ExplodeColumn(column, row);
                ExplodeRow(column, row);
            }

            return true;
        }

        private void ExplodeColumn(int column, int row)
        {
            // explode column
            // up
            for (int i = row; i >= 0; i--)
            {
                if (_matrix[column, i] != Colors.None)
                {
                    PopColumn(column, row, i);
                    break;
                }
            }
            // down
            for (int i = row; i < 6; i++)
            {
                if (_matrix[column, i] != Colors.None)
                {
                    PopColumn(column, row, i);
                    break;
                }
            }
        }
        
        private void ExplodeRow(int column, int row)
        {
            // explode row
            // left
            for (int i = column; i >= 0; i--)
            {
                if (_matrix[i, row] != Colors.None)
                {
                    PopRow(column, row, i);
                    break;
                }
            }
            // right
            for (int i = column; i < 5; i++)
            {
                if (_matrix[i, row] != Colors.None)
                {
                    PopRow(column, row, i);
                    break;
                }
            }
        }

        private void PopColumn(int column, int row, int i)
        {
            var storeHistory = _matrix[column, i];
            _matrix[column, i]--;
            _damageMatrix[column, row]++;
            // recursion in case it was Red and is None now - new explosion
            if (storeHistory == Colors.Red)
            {
                Explode(column, i);
            }
        }

        private void PopRow(int column, int row, int i)
        {
            var storeHistory = _matrix[i, row];
            _matrix[i,row ]--;
            _damageMatrix[column, row]++;
            // recursion in case it was Red and is None now - new explosion
            if (storeHistory == Colors.Red)
            {
                Explode(i, row);
            }
        }

        public bool TryFindReds()
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (_matrix[i, j] == Colors.Red)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
