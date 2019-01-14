using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterestPoint : MonoBehaviour {
	// Global Variables
	float distanceToCamera = 50; // can be any
	GameObject meshObject;
	GameObject buttonControlObject;
	ButtonControls btnControl;
	Mesh mesh;
	Main main;
	public int pixel_position_x;
	public int pixel_position_y;

	// Use this for initialization
	void Start () {
		meshObject = GameObject.Find("mesh");
		buttonControlObject = GameObject.Find("ButtonControl");
		btnControl = buttonControlObject.GetComponent<ButtonControls>();
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

		// display the position in pixels
		float x = transform.position.x;
		float y = transform.position.y;
		pixel_position_x = btnControl.position_to_pixel_x(x, (float) 14.4, (int) 50, (int) btnControl.patch_width);
		pixel_position_y = btnControl.position_to_pixel_y(y, (float) 10.56, (int) 50, (int) btnControl.patch_height);
	}
}
