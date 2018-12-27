using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

/* 
 * First step of the algorithm 
 */ 
public class ScaleFreeConstruction : MonoBehaviour {
	// global variables
	public List<List<float>> XYs = new List<List<float>>();
	public GameObject mesh;
	Vector3[] vertices;
	double[,] GPrimeInverseB;
	private Main main;
	private Util util;
	int n;

	/* 
	 * Done only once, at the begenning.
	 * Compute G'B. 
	 */ 
	public void initialComputations() {

		// variables
		main = mesh.GetComponent<Main>();
		util = mesh.GetComponent<Util>();
		main.loadTriangles();
		List<List<int>> triangles = main.triangles;
		vertices = main.vertices;
		n = main.numberOfConstraintVertices;
		float[,] G = new float[2 * vertices.Length, 2 * vertices.Length];
		float[,] G00 = new float[2 * (vertices.Length - n), 2 * (vertices.Length - n)];
		float[,] G01 = new float[2 * (vertices.Length - n), n * 2];
		float[,] G10 = new float[n * 2, 2 * (vertices.Length - n)];
		float[,] G11 = new float[n * 2, n * 2];
		double[,] GPrime;
		double[,] B;
		double[,] GPrimeInverseB;


		computeXYs(XYs, triangles);
		fillG(XYs, triangles, G);

		// Compute G00, G01, G10, G11 
		util.decomposeMatrix(G, G00, G01, G10, G11); 

		GPrime = util.AddMatrices(G00, util.Transpose(G00));
		B = util.AddMatrices(G01, util.Transpose(G10));

		util.Invert(GPrime); 
		
		GPrimeInverseB = util.MultiplyMatrix(ref GPrime, ref B);

		// save variables for later use
		main.GPrimeInverseB = GPrimeInverseB;
		
	}

	/*
	 * Done every time one of the handles is moved.
	 * Computes V' (by updating the vertices array)
	 */ 
	public void step() 
	{
		vertices = main.vertices;
		double[,] u = new double[2 * (vertices.Length - n), 1];
		double[,] q = new double[2 * n, 1]; // will return -q
		double[,] u_final;
		
		util.decomposeVector(vertices, u, q);
	 	util.negateArray(q);
	 	
		u_final = util.MultiplyMatrix(ref main.GPrimeInverseB, ref q);

		// update the mesh
		for (int i = 0; i < u_final.GetLength(0); i++) {
			vertices[i / 2][i % 2] = (float) u_final[i, 0];
		}

		main.vertices = vertices;
	}


	// takes two vectors (which have either the same or opposite direction), and returns true if they have opposite direction
	bool isOppositeDirection(Vector3 a, Vector3 b) {
		if ((a.x / b.x < 0) || (a.y / b.y < 0)) {
			return true;
		} else {
			return false;
		}
	}

	// compute XYs for all the triangles
	void computeXYs(List<List<float>> XYs, List<List<int>> triangles) 
	{
		float x01;
		float y01;
		float x12;
		float y12;
		float x20;
		float y20;
		Vector3 v0;
		Vector3 v1;
		Vector3 v2;

		// R90 - anti-clockwise rotation by 90 degrees
		Matrix4x4 R90 = new Matrix4x4();
		R90.SetRow(0, new Vector4(0, -1, 0, 0));
		R90.SetRow(1, new Vector4(1, 0, 0, 0));
		R90.SetRow(2, new Vector4(0, 0, 1, 0));
		R90.SetRow(3, new Vector4(0, 0, 0, 0));

		for (int i = 0; i < triangles.Count; i++) {
			v0 = vertices[triangles[i][0]];
			v1 = vertices[triangles[i][1]];
			v2 = vertices[triangles[i][2]];
			// negative if Vector3.Project(v2 - v0, (v1 - v0).normalized) and (v1 - v0) have opposite directions
			float sign0x = 1;
			float sign0y = 1;
			float sign1x = 1;
			float sign1y = 1;
			float sign2x = 1;
			float sign2y = 1;

			if (isOppositeDirection(Vector3.Project(v2 - v0, (v1 - v0).normalized), (v1 - v0))) {
				sign0x = -1;
			}
			if (isOppositeDirection(Vector3.Project(v2 - v0, (R90.MultiplyVector(v1 - v0)).normalized), R90.MultiplyVector(v1 - v0))) {
				sign0y = -1;
			}
			if (isOppositeDirection(Vector3.Project(v0 - v1, (v2 - v1).normalized), (v2 - v1))) {
				sign1x = -1;
			}
			if (isOppositeDirection(Vector3.Project(v0 - v1, (R90.MultiplyVector(v2 - v1)).normalized), R90.MultiplyVector(v2 - v1))) {
				sign1y = -1;
			}
			if (isOppositeDirection(Vector3.Project(v1 - v2, (v0 - v2).normalized), (v0 - v2))) {
				sign2x = -1;
			}
			if (isOppositeDirection(Vector3.Project(v1 - v2, (R90.MultiplyVector(v0 - v2)).normalized), R90.MultiplyVector(v0 - v2))) { 
				sign2y = -1;
			}

			x01 = sign0x * (Vector3.Project(v2 - v0, (v1 - v0).normalized).magnitude) / ((v1 - v0).magnitude);  
			y01 = sign0y * Vector3.Project(v2 - v0, (R90.MultiplyVector(v1 - v0)).normalized).magnitude / R90.MultiplyVector(v1 - v0).magnitude;
			x12 = sign1x * Vector3.Project(v0 - v1, (v2 - v1).normalized).magnitude / (v2 - v1).magnitude;
			y12 = sign1y * Vector3.Project(v0 - v1, (R90.MultiplyVector(v2 - v1)).normalized).magnitude / R90.MultiplyVector(v2 - v1).magnitude;
			x20 = sign2x * Vector3.Project(v1 - v2, (v0 - v2).normalized).magnitude / (v0 - v2).magnitude;
			y20 = sign2y * Vector3.Project(v1 - v2, (R90.MultiplyVector(v0 - v2)).normalized).magnitude / R90.MultiplyVector(v0 - v2).magnitude;

			XYs.Add(new List<float>(new float[] {x01, y01, x12, y12, x20, y20}));
		}

		main = mesh.GetComponent<Main>();
		main.XYs = XYs;
	}

	// Compute the G matrix
	void fillG(List<List<float>> XYs, List<List<int>> triangles, float[,] G) 
	{
		float x01;
		float y01;
		float x12;
		float y12;
		float x20;
		float y20;
		List<int> triangle;

		for (int i = 0; i < 2 * vertices.Length; i++) {
			for (int j = 0; j < 2 * vertices.Length; j++) {
				G[i, j] = 0;
			}
		}

		for  (int i = 0; i < triangles.Count; i++) {
			// get XYs
			x01 = XYs[i][0];
			y01 = XYs[i][1];
			x12 = XYs[i][2];
			y12 = XYs[i][3];
			x20 = XYs[i][4];
			y20 = XYs[i][5];

			triangle = triangles[i];

			// the main diagonal
			G[triangle[0] * 2, triangle[0] * 2] += 2 + x01*x01 + y01*y01 + x20*x20 + y20*y20 - 2*x01;
			G[triangle[0] * 2 + 1, triangle[0] * 2 + 1] += 2 + x01 * x01 + y01 * y01 + x20 * x20 + y20 * y20 - 2 * x01;
			G[triangle[1] * 2, triangle[1] * 2] += 2 + x01 * x01 + y01 * y01 + x12 * x12 + y12 * y12 - 2 * x12;
			G[triangle[1] * 2 + 1, triangle[1] * 2 + 1] += 2 + x01 * x01 + y01 * y01 + x12 * x12 + y12 * y12 - 2 * x12;
			G[triangle[2] * 2, triangle[2] * 2] += 2 + x12 * x12 + y12 * y12 + x20 * x20 + y20 * y20 - 2 * x20;
			G[triangle[2] * 2 + 1, triangle[2] * 2 + 1] += 2 + x12 * x12 + y12 * y12 + x20 * x20 + y20 * y20 - 2 * x20;

			// the upper part of the matrix
			G[triangle[0] * 2, triangle[1] * 2]  += x01 - x01 * x01 - y01 * y01 + x12 - 1 - x20; 
			G[triangle[0] * 2, triangle[1] * 2 + 1] += 0 - y01 - y12 - y20;
			G[triangle[0] * 2, triangle[2] * 2] += x01 - 1 - x12 - x20 * x20 - y20 * y20 + x20; 
			G[triangle[0] * 2, triangle[2] * 2 + 1] += y01 + y12 + y20;

			G[triangle[0] * 2 + 1, triangle[1] * 2] += y01 + y12 + y20;
			G[triangle[0] * 2 + 1, triangle[1] * 2 + 1] += x01 - x01 * x01 - y01 * y01 + x12 - 1 - x20;
			G[triangle[0] * 2 + 1, triangle[2] * 2] += 0 - y01 - y12 - y20;
			G[triangle[0] * 2 + 1, triangle[2] * 2 + 1] += x01 - 1 - x12 - x20 * x20 - y20 * y20 + x20; 

			G[triangle[1] * 2, triangle[2] * 2] += x20 - x01 - x12 * x12 - y12 * y12 - 1 + x12; 
			G[triangle[1] * 2, triangle[2] * 2 + 1] += 0 - y01 - y12 - y20;
 
			G[triangle[1] * 2 + 1, triangle[2] * 2] += y01 + y12 + y20;
			G[triangle[1] * 2 + 1, triangle[2] * 2 + 1] += x20 - x01 - x12 * x12 - y12 * y12 - 1 + x12; 


			// the lower part of the matrix
			G[triangle[1] * 2, triangle[0] * 2]  += x01 - x01 * x01 - y01 * y01 + x12 - 1 - x20;
			G[triangle[1] * 2 + 1, triangle[0] * 2] += 0 - y01 - y12 - y20; 
			G[triangles[i][2] * 2, triangle[0] * 2] += x01 - 1 - x12 - x20 * x20 - y20 * y20 + x20;;
			G[triangle[2] * 2 + 1, triangle[0] * 2] += y01 + y12 + y20;

			G[triangle[1] * 2, triangle[0] * 2 + 1] += y01 + y12 + y20;
			G[triangle[1] * 2 + 1, triangle[0] * 2 + 1] += x01 - x01 * x01 - y01 * y01 + x12 - 1 - x20;
			G[triangle[2] * 2, triangle[0] * 2 + 1] += 0 - y01 - y12 - y20;
			G[triangle[2] * 2 + 1, triangle[0] * 2 + 1] += x01 - 1 - x12 - x20 * x20 - y20 * y20 + x20;

			G[triangle[2] * 2, triangle[1] * 2] += x20 - x01 - x12 * x12 - y12 * y12 - 1 + x12;
			G[triangle[2] * 2 + 1, triangle[1] * 2] += 0 - y01 - y12 - y20;
 
			G[triangle[2] * 2, triangle[1] * 2 + 1] += y01 + y12 + y20;
			G[triangle[2] * 2 + 1, triangle[1] * 2 + 1] += x20 - x01 - x12 * x12 - y12 * y12 - 1 + x12;
		}

	}



}
