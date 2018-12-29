using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using System.IO;

public class ButtonControls : MonoBehaviour {

	// Global variables
	GameObject meshObject;
	Video videoClass;
	// UnityEngine.Video.VideoPlayer video;
	Main main;
	GameObject[] constraintPoints;
	int number_of_frames = 45;
	float[] interestPointsXCoordinates = new float[45]; // depends on the number of frames
	float[] interestPointsYCoordinates = new float[45];

	public Button extractMotionBtn;
	public Button transferMotionBtn;
	public Button markInterestPointBtn;
	public Slider slider;
	public GameObject interestPoint1;

	// C++ functions
	[DllImport ("GraphTrackPlugin")]
	private static extern void precomputations();
	[DllImport ("GraphTrackPlugin")]
	private static extern void compute_path();
	[DllImport ("GraphTrackPlugin")]
	private static extern void mark_all_interest_points();

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
		markInterestPointBtn.onClick.AddListener(markInterestPointBtnOnClick);
	}


	void extractMotionBtnOnClick()
	{
		// Debug.Log("Tracking the interest point in the video");
		// // all the C++ functions will be called here.
		// Debug.Log("Reading and compressing the video");
		// precomputations();
		// mark_all_interest_points();
		// Debug.Log("Computing the path");
		// compute_path();

		// save the coordinates of the interest points in an array (extract from the txt file)
		StreamReader reader = new StreamReader("Unity_C++_communication/computed_path.txt");
		string next_line = "initial_value";
		int frame_count = 0;
		string[] xy;
		int x;
		int y;

		for (int i = 0; i < number_of_frames; i++) {
			next_line = reader.ReadLine();
			// Debug.Log(next_line);
			xy = next_line.Split(' ');
			x = Convert.ToInt32(xy[1]);
			y = Convert.ToInt32(xy[2]);
			// pixel_to_position_x(int pixel, float screen_size, int scale_factor, int patch_size)
			interestPointsXCoordinates[frame_count] = pixel_to_position_x(x, (float) 14.4, (int) 50, (int) 15);
			interestPointsYCoordinates[frame_count] = pixel_to_position_y(y, (float) 10.56, (int) 50, (int) 15);
			frame_count++;
		}

		reader.Close();

		// When done
		videoClass.hasBeenProcessed = 1;
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

			// display the mesh again & animate it

	}

	void onSliderValueChange(float value)
	{
		// display a particular frame
		videoClass.displayFrame((int) value);
		int current_frame = (int) slider.value;

		videoClass.hasBeenProcessed = 1; // remove
		if (videoClass.hasBeenProcessed == 1) {
			// put the rectangle (interest point) on the right spot
			interestPoint1.transform.position = new Vector3(interestPointsXCoordinates[current_frame], interestPointsYCoordinates[current_frame], -3);
		}
	}

	// Has been tested. Returns the coordinates correctly.
	void markInterestPointBtnOnClick() {
		// the point of click is the center of the rectangle.
		// However patches are defined by their top-left corner => x and y should be adjusted.
		int patch_width = 15;
		int patch_height = 15;
		// write the coordinates of the interest point into txt file: (frameNumber, x-coordinate, y-coordinate)
		// using (StreamWriter outputFile = new StreamWriter("Unity_C++_communication/interest_points.txt", true)) {
		//     outputFile.WriteLine(slider.value + " " + ((int) ((7.2 + interestPoint1.transform.position.x) * 50 - patch_width / 2.0)) + " " +
		// 																							 ((int) ((5.28 - interestPoint1.transform.position.y) * 50 - patch_height / 2.0)));
		// }
		float x = interestPoint1.transform.position.x;
		float y = interestPoint1.transform.position.y;
		using (StreamWriter outputFile = new StreamWriter("Unity_C++_communication/interest_points.txt", true)) {
		    outputFile.WriteLine(slider.value + " " + (position_to_pixel_x(x, (float) 14.4, (int) 50, (int) 15)) + " " +
																									 (position_to_pixel_y(y, (float) 10.56, (int) 50, (int) 15)));
		}
	}

	// coordinate transformations between real values and pixels
	int position_to_pixel_x(float position, float screen_size, int scale_factor, int patch_size) {
		int result = (int) ((screen_size / 2 + position) * scale_factor - patch_size / 2.0);
		return result;
	}
	int position_to_pixel_y(float position, float screen_size, int scale_factor, int patch_size) {
		int result = (int) ((screen_size / 2 - position) * scale_factor - patch_size / 2.0);
		return result;
	}
	float pixel_to_position_x(int pixel, float screen_size, int scale_factor, int patch_size) {
		float result = (float) (((pixel + patch_size / 2.0) / scale_factor) - (screen_size / 2));
		return result;
	}
	float pixel_to_position_y(int pixel, float screen_size, int scale_factor, int patch_size) {
		float result = (float) (-(((pixel + patch_size / 2.0) / scale_factor) - (screen_size / 2)));
		return result;
	}
}
