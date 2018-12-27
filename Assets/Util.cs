using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/*
 * contains some helper functions, mostly needed for linear algebra calculations
 */

public class Util : MonoBehaviour {

	// decompose a matrix into four matrices
	public void decomposeMatrix(float[,] M, float[,] M00, float[,] M01, float[,] M10, float[,] M11) 
	{
		// M00
		for (int i = 0; i < M00.GetLength(0); i++) {
			for (int j = 0; j < M00.GetLength(1); j++) {
				M00[i, j] = M[i, j];
			}
		}
		// M01
		for (int i = 0; i < M01.GetLength(0); i++) {
			for (int j = 0; j < M01.GetLength(1); j++) {
				M01[i, j] = M[i, j + M00.GetLength(1)];
			}
		}
		// M10
		for (int i = 0; i < M10.GetLength(0); i++) {
			for (int j = 0; j < M10.GetLength(1); j++) {
				M10[i, j] = M[i + M00.GetLength(0), j];
			}
		}
		// M11
		for (int i = 0; i < M11.GetLength(0); i++) {
			for (int j = 0; j < M11.GetLength(1); j++) {
				M11[i, j] = M[i + M00.GetLength(0), j + M00.GetLength(1)];
			}
		}

	}

	// decompose a vector into two vectors
	public void decomposeVector(Vector3[] vector, double[,] u, double[,] q) 
	{
		// u
		for (int i = 0; i < u.GetLength(0); i++) {
			u[i, 0] = (double) vector[i / 2][i % 2];
		}
		// q
		for (int i = 0; i < q.GetLength(0); i++) {
			q[q.GetLength(0) - 1 - i, 0] = (double) vector[(2 * vector.Length - 1 - i) / 2][(i + 1) % 2];
		}
	}

	public void decomposeFloatArray(float[] vector, double[,] u, double[,] q) 
	{
		// u
		for (int i = 0; i < u.GetLength(0); i++) {
			u[i, 0] = (double) vector[i];
		}
		// q
		for (int i = 0; i < q.GetLength(0); i++) {
			q[i, 0] = (double) vector[i + u.GetLength(0)];
		}
	}

	public void negateArray(double[,] array) 
	{
		for (int i = 0; i < array.GetLength(0); i++) {
			for (int j = 0; j < array.GetLength(1); j++) {
				array[i, j] = -array[i, j];
			}
		}
	}

    public void writeIntoTxt(float[,] arrayToPrint, string description) 
    {

    	using (StreamWriter sw = File.AppendText("debug.txt"))
        {
            // Add some text to the file.
            sw.Write("This is the ");

            sw.WriteLine(description);
            sw.WriteLine("-------------------");
            // Arbitrary objects can also be written to the file.
            for (int i = 0; i < arrayToPrint.GetLength(0); i++) {
            	for (int j = 0; j < arrayToPrint.GetLength(1); j++) {

        			sw.Write(arrayToPrint[i, j]);
        			sw.Write(" ");
            	}
            	sw.WriteLine(" ");
            }
        }

    }

    public void writeIntoTxt2(double[,] arrayToPrint, string description) 
    {

    	using (StreamWriter sw = File.AppendText("debug.txt"))
        {
            // Add some text to the file.
            sw.Write("This is the ");

            sw.WriteLine(description);
            sw.WriteLine("-------------------");
            // Arbitrary objects can also be written to the file.
            for (int i = 0; i < arrayToPrint.GetLength(0); i++) {
            	for (int j = 0; j < arrayToPrint.GetLength(1); j++) {

        			sw.Write(arrayToPrint[i, j]);
        			sw.Write(" ");
            	}
            	sw.WriteLine(" ");
            }
        }
    }

    public float[,] Transpose(float[,] matrix)
    {
        int w = matrix.GetLength(0);
        int h = matrix.GetLength(1);

        float[,] result = new float[h, w];

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                result[j, i] = matrix[i, j];
            }
        }
        return result;
    }
	
	public double[,] TransposeDouble(double[,] matrix)
	{
        int w = matrix.GetLength(0);
        int h = matrix.GetLength(1);

        double[,] result = new double[h, w];

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                result[j, i] = matrix[i, j];
            }
        }
        return result;
	}

    // add two matrices of equal dimensions
    public double[,] AddMatrices(float[,] matrix1, float[,] matrix2) 
    {
    	int w = matrix1.GetLength(1);
        int h = matrix1.GetLength(0);

    	double[,] result = new double[h, w];

    	for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                result[i, j] = matrix1[i, j] + matrix2[i, j];
            }
        }
        return result;
    }

    public double[,] AddMatricesDouble(double[,] matrix1, double[,] matrix2) 
    {
    	int w = matrix1.GetLength(1);
        int h = matrix1.GetLength(0);

    	double[,] result = new double[h, w];

    	for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                result[i, j] = matrix1[i, j] + matrix2[i, j];
            }
        }
        return result;
    }

    // multiply two 2D arrays (as matrices)
    public double[,] MultiplyMatrix(ref double[,] A, ref double[,] B)
    {
        int rA = A.GetLength(0);
        int cA = A.GetLength(1);
        int cB = B.GetLength(1);

        double temp = 0;
        double[,] result = new double[rA, cB];

        for (int i = 0; i < rA; i++)
        {
            for (int j = 0; j < cB; j++)
            {
                temp = 0;
                for (int k = 0; k < cA; k++)
                {
                    temp += A[i, k] * B[k, j];
                }
                    result[i, j] = temp;
            }
        }
        return result;
    }

    public double[,] MultiplyMatrix2(ref double[,] A, ref float[] B)
    {
        int rA = A.GetLength(0);
        int cA = A.GetLength(1);
       // int cB = B.Length;

        double temp = 0;
        double[,] result = new double[rA, 1];

        for (int i = 0; i < rA; i++)
        {
            temp = 0;
            for (int k = 0; k < cA; k++)
            {
                temp += A[i, k] * B[k];
            }
            result[i, 0] = temp;
            
        }
        return result;
    }

    // get a copy of an array
    public double[,] GetArrayCopy(double[,] array) 
    {

    	double[,] arrayCopy = new double[array.GetLength(0), array.GetLength(1)];
    	for (int i = 0; i < arrayCopy.GetLength(0); i++) {
    		for (int j = 0; j < arrayCopy.GetLength(1); j++) {
    			arrayCopy[i, j] = array[i, j];
    		}
    	}
    	return arrayCopy;
    }

    // takes a an array of Vesctor3s of Length n, and returns a double array of length 2n. (x and y elements of Vector3 are put into different cells)
    public double[,] Vector3ArraytoDouble(Vector3[] vertices) 
    {
    	double[,] verticesDouble = new double[vertices.Length * 2, 1];
		for (int i = 0; i < vertices.Length; i++) {
			verticesDouble[i * 2, 0] = vertices[i].x;
			verticesDouble[i * 2 + 1, 0] = vertices[i].y;
		}

		return verticesDouble;
    }

    // inverts a given matrix
    public void Invert(double[,] matrix) 
    {
    	int info;
    	alglib.matinvreport rep;
		alglib.rmatrixinverse(ref matrix, out info, out rep);
    }

    // takes a set of points and puts them into one plane (by setting all the z-coordinates to zero)
	public void flatten(Vector3[] vertices) 
	{
		for (int i = 0; i < vertices.Length; i++) {
			vertices[i].z = 0;
		}
	}

}
