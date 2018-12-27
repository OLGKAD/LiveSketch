using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * For the handles
 */
public class ConstraintPoint : MonoBehaviour {

	float distanceToCamera = 50; // can be any
	public int constraintPointNumber;
	GameObject meshObject;
	Mesh mesh;
	Main main;
	Vector3[] vertices;
	List<int> constraintVertices;

	void Start() {
		meshObject = GameObject.Find("mesh");
		main = meshObject.GetComponent<Main>();
		vertices = main.vertices;
		constraintVertices = main.constraintVertices;
		// put handles on top of the constraint vertices
		transform.position = vertices[constraintVertices[constraintPointNumber]];

		// initially not visible
		GetComponent<MeshRenderer>().enabled = false;
	}

	void OnMouseDrag()
	{

		Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToCamera);
		Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);
		objPosition.z = -1;
		transform.position = objPosition;

		main.updateMesh();
		// print vertices
		// for (int i = 0; i < vertices.Length; i++) {
		// 	// if ((i == 141) || (i == 142) || (i == 143) || (i == 148) || (i == 153) || (i == 154) || (i == 155) || (i == 156) || (i == 207) || (i == 208)
		// 	// || (i == 209)){
		// 	// 	Debug.Log("vertice# " + i);
		// 	// 	Debug.Log(vertices[i]);
		// 	// }
		// 	if (i == 210) {
		// 			Debug.Log("vertice# " + i);
		// 			Debug.Log(vertices[i]);
		// 	}
		// 	// if ((vertices[i].y < -0.5) && (vertices[i].x < 0.4) && (vertices[i].x > 0)){
		// 	// 	Debug.Log("vertice# " + i);
		// 	// 	Debug.Log(vertices[i]);
		// 	// }
		// }

	}

}
