using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

/* 
 * First part of the 2nd step of the algorithm 
 */ 
public class ScaleAdjustment : MonoBehaviour {

	public Vector3[] vertices;
	public GameObject meshObject;
	public ScaleFreeConstruction ScaleFreeConstruction;
	Main main;
	private List<List<int>> triangles;
	List<List<float>> XYs; 
	int n;


	/* 
	 * Done only once, at the begenning.
	 * Compute F-s (and inverses of F-s)
	*/
	public void initialComputations () 
	{
		List<Matrix4x4> Fs = new List<Matrix4x4>();
		List<Matrix4x4> Finverses = new List<Matrix4x4>();
		Matrix4x4 tempF; 
		ScaleFreeConstruction = meshObject.GetComponent<ScaleFreeConstruction>();
		main = meshObject.GetComponent<Main>();
		triangles = main.triangles;
		vertices = main.vertices;
		XYs = main.XYs;
		n = main.numberOfConstraintVertices;

		for (int i = 0; i < XYs.Count; i++) {
			tempF = fillF (XYs[i]);
			Fs.Add(tempF);
			Finverses.Add(tempF.inverse);
		}

		// save variables for later use
		main.Fs = Fs;
		main.Finverses = Finverses;
	}

	/* 
	 * Done every time one of the handles is moved. 
	 * Computes fitted triangles
	*/
	public void step () 
	{
		// variables
		List<Matrix4x4> Finverses = main.Finverses;
		List<Matrix4x4> Fs = main.Fs;
		List<Vector4> Cs = new List<Vector4>();
		List<Vector4> Ws = new List<Vector4>();
		List<Vector3[]> fittedTriangles = new List<Vector3[]>();
		float temp;
		Vector3[] originalvertices = main.originalvertices;

		findCs (Finverses, Cs); 
		findWs (Ws, Finverses, Cs); 

		for (int i = 0; i < triangles.Count; i++) {
			temp =  (new Vector3(Ws[i].x, Ws[i].y, 0) - new Vector3(Ws[i].z, Ws[i].w, 0)).magnitude / (originalvertices[triangles[i][0]] - originalvertices[triangles[i][1]]).magnitude;
			Ws[i].Set(Ws[i].x / temp, Ws[i].y / temp, Ws[i].z / temp, Ws[i].w / temp); // divide or not, fittedTriangles == trianngles
			Ws[i] = new Vector4(Ws[i].x / temp, Ws[i].y / temp, Ws[i].z / temp, Ws[i].w / temp);
		}

		computeFittedTriangles(Ws, fittedTriangles, main.XYs); 
		main.fittedTriangles = fittedTriangles;
	}

	// SolveC for all the trianlges
	void findCs (List<Matrix4x4> Finverses, List<Vector4> Cs) 
	{

		Mesh mesh = GetComponent<MeshFilter>().mesh;
		vertices = main.vertices;
		Vector4 C;
		Vector3[] vertice = new Vector3[3];

		for (int i = 0; i < Finverses.Count; i++) {
			vertice[0] = new Vector3(vertices[triangles[i][0]].x, vertices[triangles[i][0]].y, vertices[triangles[i][0]].z);
			vertice[1] = new Vector3(vertices[triangles[i][1]].x, vertices[triangles[i][1]].y, vertices[triangles[i][1]].z);
			vertice[2] = new Vector3(vertices[triangles[i][2]].x, vertices[triangles[i][2]].y, vertices[triangles[i][2]].z);

			C = solveC (vertice, XYs[i]);

			Cs.Add (C);
		}
	}

	// SolveW for all the triangles
	void findWs (List<Vector4> Ws, List<Matrix4x4> Finverses, List<Vector4> Cs) 
	{
		Vector4 W;
		for (int i = 0; i < Finverses.Count; i++) {
			W = solveW (Finverses [i], Cs [i]);
			Ws.Add (W);
		}
	}

	// compute C (for a single triangle)
	Vector4 solveC (Vector3[] vertrices, List<float> XYs) 
	{
		Vector4 C = new Vector4 (-2 * vertrices[0].x - 2 * vertrices[2].x + 2 * vertrices[2].x * XYs[0] + 2 * vertrices[2].y * XYs[1],
								 -2 * vertrices[0].y - 2 * vertrices[2].y + 2 * vertrices[2].y * XYs[0] - 2 * vertrices[2].x * XYs[1],
							   	 -2 * vertrices[1].x - 2 * vertrices[2].x * XYs[0] - 2 * vertrices[2].y * XYs[1],
				                 -2 * vertrices[1].y - 2 * vertrices[2].y * XYs[0] + 2 * vertrices[2].x * XYs[1]);
		return C;
	}

	/*
	 * Solve the equation F * W + C = 0 in W
	 * This solves the 4x4 equation system, then returns.
	 */
	Vector4 solveW (Matrix4x4 Finverse, Vector4 C) 
	{
		Vector4 W;
		W = Finverse * (-C);
		return W;
	}

	// compute F (for a single triangle)
	Matrix4x4 fillF(List<float> XYs) 
	{
		Matrix4x4 F = new Matrix4x4();

		F.SetRow(0, new Vector4(2 * (2 - XYs[0] * 2 + Mathf.Pow(XYs[0],2) + Mathf.Pow(XYs[1],2)), 0, 2 * (XYs[0] - Mathf.Pow(XYs[0],2) - Mathf.Pow(XYs[1],2)), -2 * XYs[1]));
		F.SetRow(1, new Vector4(0, 2 * (2 - 2 * XYs[0] + Mathf.Pow(XYs[0],2) + Mathf.Pow(XYs[1],2)), 2 * XYs[1], 2 * (XYs[0] - Mathf.Pow(XYs[0],2) - Mathf.Pow(XYs[1],2))));
		F.SetRow(2, new Vector4(2 * ( XYs[0] - Mathf.Pow(XYs[0],2) - Mathf.Pow(XYs[1],2)), 2 * XYs[1], 2 * (1 + Mathf.Pow(XYs[0],2) + Mathf.Pow(XYs[1],2)), 0));
		F.SetRow(3, new Vector4(-2 * XYs[1], 2 * (XYs[0] - Mathf.Pow(XYs[0],2) - Mathf.Pow(XYs[1],2)), 0, 2 * (1 + Mathf.Pow(XYs[0],2) + Mathf.Pow(XYs[1],2))));
		return F;
	}


	// Given Ws and XYs commuted all the fitted triangles
	void computeFittedTriangles(List<Vector4> Ws, List<Vector3[]> fittedTriangles, List<List<float>> XYs) 
	{
		Vector4 W;
		List<float> XY;
		Vector3 v0;
		Vector3 v1;
		Vector3 v2;

		// R90 - anti-clockwise rotation by 90 degrees
		Matrix4x4 R90 = new Matrix4x4();
		R90.SetRow(0, new Vector4(0, -1, 0, 0));
		R90.SetRow(1, new Vector4(1, 0, 0, 0));
		R90.SetRow(2, new Vector4(0, 0, 1, 0));
		R90.SetRow(3, new Vector4(0, 0, 0, 0));

		for (int i = 0; i < Ws.Count; i++) {
			W = Ws[i];
			XY = XYs[i];
			v0 = new Vector3(W.x, W.y, 0);
			v1 = new Vector3(W.z, W.w, 0);
			v2 = v0 + XY[0] * (v1 - v0) + XY[1] * R90.MultiplyVector(v1 - v0);
			fittedTriangles.Add(new Vector3[] {v0, v1, v2});
		}
		
	}
}