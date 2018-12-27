using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;

public class ButtonControls : MonoBehaviour {

	// Global variables
	GameObject meshObject;
	Video videoClass;
	// UnityEngine.Video.VideoPlayer video;
	Main main;
	GameObject[] constraintPoints;
	public Button extractMotionBtn;
	public Button transferMotionBtn;
	public Slider slider;

	// C++ functions
	[DllImport ("GraphTrackPlugin")]
	private static extern void precomputations();
	[DllImport ("GraphTrackPlugin")]
	private static extern void compute_path();

	// Use this for initialization
	void Start () {
		meshObject = GameObject.Find("mesh");
		main = meshObject.GetComponent<Main>();
		videoClass = GameObject.Find("video").GetComponent<Video>();
		// video = videoClass.video1;
		constraintPoints = GameObject.FindGameObjectsWithTag("button");
		extractMotionBtn.onClick.AddListener(extractMotionBtnOnClick);
		transferMotionBtn.onClick.AddListener(transferMotionBtnOnClick);
		slider.onValueChanged.AddListener(onSliderValueChange);

	}


	void extractMotionBtnOnClick()
	{
		Debug.Log("Tracking the interest point in the video");
		// all the C++ functions will be called here
	}

	void transferMotionBtnOnClick()
	{
			// hide video
			// GameObject.Find("Video")
			// display the mesh
			meshObject.GetComponent<MeshRenderer>().enabled = true;
			foreach(GameObject constraintPoint in constraintPoints) {
				constraintPoint.GetComponent<MeshRenderer>().enabled = true;
			}

			// animate the mesh

	}

	void onSliderValueChange(float value)
	{
		// display a particular frame
		videoClass.displayFrame((int) value);
	}
}
