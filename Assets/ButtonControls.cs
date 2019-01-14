using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using System.IO;

public class ButtonControls : MonoBehaviour {

	// Global variables

	// exposed variables
	GameObject meshObject;
	Video videoClass;
	public Button extractMotionBtn;
	public Button transferMotionBtn;
	public Button markInterestPointBtn;
	public Button initializeBtn;
	public Slider slider;
	public GameObject interestPoint1;
	// public GameObject constraintHandle1;
	public Text frameNumber;

	UnityEngine.Video.VideoPlayer video;
	Main main;
	GameObject[] constraintPoints;
	int number_of_frames = 45;
	float[] interestPointsXCoordinates = new float[45]; // depends on the number of frames
	float[] interestPointsYCoordinates = new float[45];
	public int patch_width = 15;
	public int patch_height = 15;



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
		constraintPoints = GameObject.FindGameObjectsWithTag("button");
		extractMotionBtn.onClick.AddListener(extractMotionBtnOnClick);
		transferMotionBtn.onClick.AddListener(transferMotionBtnOnClick);
		slider.onValueChanged.AddListener(onSliderValueChange);
		markInterestPointBtn.onClick.AddListener(markInterestPointBtnOnClick);
		initializeBtn.onClick.AddListener(initializeBtnOnClick);
	}

	// TRY:
	// 1. Try using 20x20 patches. If results are not accurate for that either, there’s a problem.
  // 2. Try marking 5 interest points in the same frames (using first GraphTrack and then GraphTrackPlugin)
	// 		and see if the same values are stored in txt / cout in each case.
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
			interestPointsXCoordinates[frame_count] = pixel_to_position_x(x, (float) 14.4, (int) 50, patch_width);
			interestPointsYCoordinates[frame_count] = pixel_to_position_y(y, (float) 10.56, (int) 50, patch_height);
			frame_count++;
		}

		reader.Close();

		// When done
		videoClass.hasBeenProcessed = 1;
	}

	void transferMotionBtnOnClick()
	{
			// hide video
			GameObject.Find("Screen").GetComponent<MeshRenderer>().enabled = false;

			// display the mesh
			meshObject.GetComponent<MeshRenderer>().enabled = true;
			foreach(GameObject constraintPoint in constraintPoints) {
				constraintPoint.GetComponent<MeshRenderer>().enabled = true;
			}

			// hide all unneccessary Buttons
			extractMotionBtn.gameObject.SetActive(false);
			markInterestPointBtn.gameObject.SetActive(false);
			transferMotionBtn.gameObject.SetActive(false);

			// When done
			videoClass.hasBeenProcessed = 2;

			/* Display the mesh again & animate it.
			* Will be animated by moving the slider.
			*/

	}

	void moveObjectToPoint(GameObject object1, Vector3 position) {
		object1.transform.position = position;
	}


	void onSliderValueChange(float value)
	{
		// display a particular frame
		// ERROR should be 1. The problem is GetFrameTime and displayFrame do some roundings (time is discreet)
		// Using 2 instead of 1 might fix it for the first frame, but it might fuck up the rest.
		videoClass.displayFrame((int) (value + 2)); // +1 because the first frame is skipped in GraphTrackPlugin
		int current_frame = (int) slider.value;
		frameNumber.text = current_frame.ToString();

		// path has been computed
		if (videoClass.hasBeenProcessed == 1) {
			// put the rectangle (interest point) on the right spot
			interestPoint1.transform.position = new Vector3(interestPointsXCoordinates[current_frame], interestPointsYCoordinates[current_frame], -3);
		// animating the mesh
		} else if (videoClass.hasBeenProcessed == 2) {
			/* Display the mesh again & animate it.
			* Get the coordinates of the arrays: interestPointsXCoordinates, interestPointsYCoordinates.
			* Use them to animate a single constrain point.
			*/
			Vector3 nextPosition = new Vector3(0, 0, -3);

			nextPosition.x = interestPointsXCoordinates[current_frame];
			nextPosition.y = interestPointsYCoordinates[current_frame];
			constraintPoints[0].transform.position = nextPosition;
			main.updateMesh();
		}
	}

	void markInterestPointBtnOnClick() {
		// the point of click is the center of the rectangle.
		// However patches are defined by their top-left corner => x and y should be adjusted.
		// write the coordinates of the interest point into txt file: (frameNumber, x-coordinate, y-coordinate)
		// using (StreamWriter outputFile = new StreamWriter("Unity_C++_communication/interest_points.txt", true)) {
		//     outputFile.WriteLine(slider.value + " " + ((int) ((7.2 + interestPoint1.transform.position.x) * 50 - patch_width / 2.0)) + " " +
		// 																							 ((int) ((5.28 - interestPoint1.transform.position.y) * 50 - patch_height / 2.0)));
		// }
		float x = interestPoint1.transform.position.x;
		float y = interestPoint1.transform.position.y;
		using (StreamWriter outputFile = new StreamWriter("Unity_C++_communication/interest_points.txt", true)) {
		    outputFile.WriteLine(slider.value + " " + (position_to_pixel_x(x, (float) 14.4, (int) 50, (int) patch_width)) + " " +
																									 (position_to_pixel_y(y, (float) 10.56, (int) 50, (int) patch_height)));
		}
	}

	void initializeBtnOnClick() {
		video = videoClass.video1;
		video.frame = 2;
		initializeBtn.gameObject.SetActive(false);
	}

	// coordinate transformations between real values and pixels
	// ALL SEEM TO WORK AS INTENDED
	public int position_to_pixel_x(float position, float screen_size, int scale_factor, int patch_size) {
		int result = (int) ((screen_size / 2 + position) * scale_factor - patch_size / 2.0);
		return result;
	}
	public int position_to_pixel_y(float position, float screen_size, int scale_factor, int patch_size) {
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
