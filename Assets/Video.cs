using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Video : MonoBehaviour {
	// Global variables
	UnityEngine.Video.VideoPlayer video1;
	public int hasBeenProcessed = 0; // 0 - has not. 1 - has been. Is used to determine whether to put
																	 // interest point rectangles on the video. 

	// Use this for initialization
	void Start () {
		video1 = GetComponent<UnityEngine.Video.VideoPlayer>();
		video1.Pause();
	}

	// Update is called once per frame
	void Update () {

	}

	float GetFrameTime(int fps,int frame)
	{
		 float periot = 1.0f/fps;
		 return periot*frame;
	}

	public void displayFrame(int frameNumber) {
		video1.time = GetFrameTime(60,frameNumber);
	}

	void play() {
		video1.Play();
	}
}
