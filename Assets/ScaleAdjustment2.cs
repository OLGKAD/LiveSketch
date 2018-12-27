using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/* 
 * Second part of the 2nd step of the algorithm 
 */ 
public class ScaleAdjustment2 : MonoBehaviour {

	// Global variables
	private Main main;
	private Util util;
	public GameObject mesh;
	List<List<int>> triangles;
	double[,] HPrime;
	double[,] D;
	Vector3[] vertices;
	float[,] H;
	float c;
	int n;

	/* 
	 * Done only once, at the begenning.
	 * Compute H' and D
	 */ 
	public void intialComputation () 
	{
		main = mesh.GetComponent<Main>();
		util = mesh.GetComponent<Util>();
		triangles = main.triangles;
		vertices = main.vertices;
		n = main.numberOfConstraintVertices;
		H = new float[2 * vertices.Length, 2 * vertices.Length];
		float[] f = new float[2 * vertices.Length];
		float[,] H00 = new float[2 * (vertices.Length - n), 2 * (vertices.Length - n)];
		float[,] H01 = new float[2 * (vertices.Length - n), n * 2];
		float[,] H10 = new float[n * 2, 2 * (vertices.Length - n)];
		float[,] H11 = new float[n * 2, n * 2];

		fillH(triangles, H); 
		util.decomposeMatrix(H, H00, H01, H10, H11); 

		HPrime = util.AddMatrices(H00, util.Transpose(H00));
		D = util.AddMatrices(H01, util.Transpose(H10));

		util.Invert(HPrime);

		// save variables for later use
		main.HPrimeInverse = HPrime;
		main.D = D; 

	}

	// done every time one of the handles is moved
	public void step() 
	{
		// variables
		List<Vector3[]> fittedTriangles = main.fittedTriangles;
		vertices = main.vertices;
		double[,] u = new double[2 * (vertices.Length - n), 1];
		double[,] q = new double[2 * n, 1]; 
		util.decomposeVector(vertices, u, q);
		float[] f = new float[vertices.Length * 2];
		double[,] f0 = new double[2 * (vertices.Length - n), 1];
		double[,] f1 = new double[2 * n, 1]; 
		double[,] HPrimeInverse;
		double[,] Dq;
		double[,] RHS;
		double[,] u_final;

		fillf(triangles, fittedTriangles, f);

		util.decomposeFloatArray(f, f0, f1);

		HPrimeInverse = main.HPrimeInverse;

		Dq = util.MultiplyMatrix(ref main.D, ref q); 
		RHS = util.AddMatricesDouble(Dq, f0);
		util.negateArray(RHS); 
		u_final = util.MultiplyMatrix(ref HPrimeInverse, ref RHS);

		// update the vertices
		for (int i = 0; i < u_final.GetLength(0); i++) {
			vertices[i / 2][i % 2] = (float) u_final[i, 0]; 
		}

		Mesh Mesh = GetComponent<MeshFilter>().mesh;
		Mesh.vertices = vertices; 
		main.vertices = vertices;
	}

	void fillH(List<List<int>> triangles, float[,] H) 
	{

		List<int> triangle;

		// initialize H with zeroes
		for (int i = 0; i < H.GetLength(0); i++) {
			for (int j = 0; j < H.GetLength(1); j++) {
				H[i, j] = 0;
			}
		}

		for  (int i = 0; i < triangles.Count; i++) {
			triangle = triangles[i];

			// main diagonal
			H[triangle[0] * 2, triangle[0] * 2] += 2;
			H[triangle[0] * 2 + 1, triangle[0] * 2 + 1] += 2;
			H[triangle[1] * 2, triangle[1] * 2] += 2;
			H[triangle[1] * 2 + 1, triangle[1] * 2 + 1] += 2;
			H[triangle[2] * 2, triangle[2] * 2] += 2;
			H[triangle[2] * 2 + 1, triangle[2] * 2 + 1] += 2;

			// upper part of the matrix
			H[triangle[0] * 2, triangle[1] * 2] -= 1;
			H[triangle[0] * 2, triangle[2] * 2] -= 1;

			H[triangle[0] * 2 + 1, triangle[1] * 2 + 1] -= 1;
			H[triangle[0] * 2 + 1, triangle[2] * 2 + 1] -= 1;

			H[triangle[1] * 2, triangle[2] * 2] -= 1;

			H[triangle[1] * 2 + 1, triangle[2] * 2 + 1] -= 1;

			// lower part of the matrix
			H[triangle[1] * 2, triangle[0] * 2] -= 1;
			H[triangle[2] * 2, triangle[0] * 2] -= 1;

			H[triangle[1] * 2 + 1, triangle[0] * 2 + 1] -= 1;
			H[triangle[2] * 2 + 1, triangle[0] * 2 + 1] -= 1;

			H[triangle[2] * 2, triangle[1] * 2] -= 1;

			H[triangle[2] * 2 + 1, triangle[1] * 2 + 1] -= 1;
		}
	}

	void fillf(List<List<int>> triangles, List<Vector3[]> fittedTriangles, float[] f) 
	{
		Vector3 v0;
		Vector3 v1;
		Vector3 v2;
		List<int> triangle;

		// initialize H with zeroes
		for (int i = 0; i < f.Length; i++) {
			f[i] = 0;
		}


		for (int i = 0; i < fittedTriangles.Count; i++) {
			v0 = fittedTriangles[i][0];
			v1 = fittedTriangles[i][1];
			v2 = fittedTriangles[i][2];
			triangle = triangles[i];

			f[triangle[0] * 2] += 2 * (v1.x + v2.x - 2 * v0.x);
			f[triangle[0] * 2 + 1] += 2 * (v1.y + v2.y - 2 * v0.y);
			f[triangle[1] * 2] += 2 * (v0.x + v2.x - 2 * v1.x);
			f[triangle[1] * 2 + 1] += 2 * (v0.y + v2.y - 2 * v1.y);
			f[triangle[2] * 2] += 2 * (v1.x + v0.x - 2 * v2.x);
			f[triangle[2] * 2 + 1] += 2 * (v1.y + v0.y - 2 * v2.y);
		}
	}	

	float computeC(List<Vector3[]> fittedTriangles) 
	{
		float c = 0; 
		Vector3 v0;
		Vector3 v1;
		Vector3 v2;

		for (int i = 0; i < fittedTriangles.Count; i++) {
			v0 = fittedTriangles[i][0];
			v1 = fittedTriangles[i][1];
			v2 = fittedTriangles[i][2];
			
			c += 2 * (v0.x * v0.x + v1.x * v1.x + v2.x * v2.x) - 2 * (v0.x * v1.x + v0.x * v2.x + v1.x * v2.x);
			c += 2 * (v0.y * v0.y + v1.y * v1.y + v2.y * v2.y) - 2 * (v0.y * v1.y + v0.y * v2.y + v1.y * v2.y);

		}
		return c;
	}
}
