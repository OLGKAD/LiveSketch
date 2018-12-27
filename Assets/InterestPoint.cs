using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterestPoint : MonoBehaviour {
	// Global Variables
	float distanceToCamera = 50; // can be any
	GameObject meshObject;
	Mesh mesh;
	Main main;

	// Use this for initialization
	void Start () {
		meshObject = GameObject.Find("mesh");
		main = meshObject.GetComponent<Main>();
		// GetComponent<Renderer>().material.color = new Color(0, 255, 0);
		Color color = GetComponent<Renderer>().material.color;
		color.a = 0f;
		GetComponent<Renderer>().material.color = color;
	}

	// Update is called once per frame
	void Update () {

	}

	void OnMouseDrag() {
		Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToCamera);
		Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);
		objPosition.z = -1;
		transform.position = objPosition;
	}

}
