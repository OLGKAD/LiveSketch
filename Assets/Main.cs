using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;


public class Main : MonoBehaviour {


	// Global Variables
	public Vector3[] vertices;
	public List<List<int>> triangles = new List<List<int>>();
	public List<int> constraintVertices;
	public List<Vector3> ButtonPositions;
	public Vector3[] originalvertices;
	public double[,] GPrimeInverseB;
	public double[,] HPrimeInverse;
	public double[,] D;
	public List<List<float>> XYs;
	public List<Vector3[]> fittedTriangles;
	public List<Matrix4x4> Fs;
	public List<Matrix4x4> Finverses;
	public int numberOfConstraintVertices = 3;

	void Start ()
	{
		// Video: Handle the video here. Do the rest later.
		// TODO

		// Mesh:
		constraintVertices = new List<int>(new int[] {57, 205, 5}); //, 154, 185}); // chosen manually
		ScaleFreeConstruction G = GetComponent<ScaleFreeConstruction>();
		G.initialComputations();
		G.step();

		ScaleAdjustment F = GetComponent<ScaleAdjustment>();
		F.initialComputations();
		F.step();

		ScaleAdjustment2 H = GetComponent<ScaleAdjustment2>();
		H.intialComputation();
		H.step();

		// initially not visible 
		GetComponent<MeshRenderer>().enabled = false;
	}

	void initializeMesh() {

	}
	// called when handles have moved, and so should be constrained vertices => mesh needs to be updated
	public void updateMesh()
	{
		GameObject[] Buttons = GameObject.FindGameObjectsWithTag("button");
		int constraintPointNumber;

		foreach(GameObject button in Buttons) {
			// button.GetComponent<ConstraintPoint>().StartButton();
			constraintPointNumber = button.GetComponent<ConstraintPoint>().constraintPointNumber;
			vertices[constraintVertices[constraintPointNumber]] = button.transform.position;
		}

		ScaleFreeConstruction G = GetComponent<ScaleFreeConstruction>();
		ScaleAdjustment F = GetComponent<ScaleAdjustment>();
		ScaleAdjustment2 H = GetComponent<ScaleAdjustment2>();
		G.step();
		F.step();
		H.step();
	}

	// get triangles and vertices from the Mesh
	public void loadTriangles()
	{
		Mesh mesh = GetComponent<MeshFilter>().mesh;

		vertices = mesh.vertices;

		int[] triangleIndices = mesh.GetTriangles(0);

		for (int i = 0; i < triangleIndices.Length; i += 3) {
			List<int> tr = new List<int>();
			for (int j = 0; j < 3; j++) {
				tr.Add(triangleIndices[i + j]);
			}
			triangles.Add(tr);
		}
		chooseConstraintVertices(mesh, constraintVertices, vertices, triangles);
		originalvertices = (Vector3[]) vertices.Clone();
	}

	// read current positions of buttons
	void UpdateButtonPositions()
	{
		for (int i = 0; i < constraintVertices.Count; i++) {
			ButtonPositions.Add(GameObject.Find("constraintPoint" + i).transform.position);
		}
	}

	// choose constraint vertices.
	// vertices will later be devided into u and q, where q (= set of constraint vertices), goes after u in vertices => elements in vertices
	// should be reordered, as well as in triangles.
	void chooseConstraintVertices(Mesh mesh, List<int> constraintVertices, Vector3[] vertices, List<List<int>> triangles)
	{
		Vector3 temp;
		// divide vertices into u and v (rearange the vertices array so that the constraint vertices go at the end)
		for (int i = 0; i < constraintVertices.Count; i++) {
			// move the vertex to the end of the list, and shift all the elements that went after it to the left
			temp = vertices[constraintVertices[i]];
			for (int j = constraintVertices[i]; j < vertices.Length - 1; j++) {
				vertices[j] = vertices[j + 1];
			}

			vertices[vertices.Length - 1] = temp;

			// relink all the vertices in the triangles array
			// "vertices" were sort of tested, but "triangles" are not tested at all
			for (int j = 0; j < triangles.Count; j++) {
				for (int k = 0; k < 3; k++) {
					if (triangles[j][k] == constraintVertices[i]) {
						triangles[j][k] = vertices.Length - 1;
					} else if (triangles[j][k] > constraintVertices[i]) {
						triangles[j][k] -= 1;
					}
				}
			}

			// update elements of contraintVertices
			// ...
			for (int j = 0; j < constraintVertices.Count; j++) {
				if (constraintVertices[j] > constraintVertices[i]) {
					constraintVertices[j] -= 1;
				} else if (constraintVertices[j] == constraintVertices[i]) {
					constraintVertices[j] = vertices.Length - 1;
				}
			}
		}

		int[] newTriangleIndices = new int[triangles.Count * 3];
		for (int i = 0; i < newTriangleIndices.Length; i++) {
			newTriangleIndices[i] = triangles[i / 3][i % 3];
		}
		mesh.triangles = newTriangleIndices;
	}

}
