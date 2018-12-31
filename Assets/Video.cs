using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Video : MonoBehaviour {
	// Global variables
	public UnityEngine.Video.VideoPlayer video1;
	public int hasBeenProcessed = 0; // 0 - has not. 1 - has been. Is used to determine whether to put
																	 // interest point rectangles on the video.

	// Use this for initialization
	void Start () {
		video1 = GetComponent<UnityEngine.Video.VideoPlayer>();
		// Debug.Log(video1.length);

		// we're gonna skip the first frame since it was skipped in GraphTrackPlugin (for some reason)

		video1.Pause();

		// print video size
		// video1.prepareCompleted += (UnityEngine.Video.VideoPlayer source) =>
		// {
		// 	Debug.Log("dimensions " + source.texture.width + " x " + source.texture.height); // do with these dimensions as you wish
		// };
	}

	// Update is called once per frame
	void Update () {
		// video1.frame = 2;
		// displayFrame(1);
		// Debug.Log(video1.frameCount); // 166
		// Debug.Log(video1.frameRate); // 57.91907 / second
	}

	public void displayFrame(int frameNumber) {
		// video1.time = GetFrameTime((float) 57.91907,frameNumber);
		video1.frame = frameNumber;
	}

}
