using UnityEngine;
using System.Collections;
using UnityEngine.VR;
using System.Collections.Generic;
using System.IO;
using System;

public class Acceleration : MonoBehaviour {

	private List<Vector3> positions;
	private List<Vector3> velocities;
	private List<Vector3> accelerations;
	private List<float> sideAccelerations;
	private List<float> times;
	public GameObject player; 
	private int frames = 1;
	public StreamWriter file;

	void Start () {
		player = GetComponent<GameObject>();
		positions = new List<Vector3>();
		velocities = new List<Vector3>();
		accelerations = new List<Vector3>();
		sideAccelerations = new List<float>();
		times = new List<float>();
		file = new StreamWriter(@"C:\\Users\\wilkinsonm\\My Documents\\WalkingInPlace\\output.txt");
	}
	
	// Update is called once per frame
	void Update () {
		if(VRDevice.isPresent){
			VRRecording(file);
			frames++;
		}
		else{
			PCRecording(file);
			frames++;
		}
	}

	//gets position data from VR Device
	void VRRecording (System.IO.StreamWriter file) {
		positions.Add(InputTracking.GetLocalPosition(VRNode.CenterEye));
		times.Add (Time.time);
		CalcAcceleration(file);
	}


	//gets position data from PC
	void PCRecording (System.IO.StreamWriter file) {
		positions.Add (transform.position);
		times.Add (Time.time);
		CalcAcceleration(file);
	}

	void CalcAcceleration(System.IO.StreamWriter file){
		int length = positions.Count;
		if(length > 1){
			Vector3 posChange = positions[length - 1] - positions[length - 2];
			velocities.Add(posChange/Time.deltaTime);
			if(length > 3){
				Vector3 velChange = velocities[length - 1] - velocities[length - 2];

				accelerations.Add(velChange/Time.deltaTime);

				//calculates how much of acceleration is in the local x direction (to the right)
				Quaternion rotation = InputTracking.GetLocalRotation (VRNode.CenterEye);
				Vector3 newRotation = rotation * transform.right;
				sideAccelerations.Add (Vector3.Dot (accelerations[length - 1], newRotation));
				Debug.Log (sideAccelerations[length - 1] + ", " + Time.deltaTime);
				file.WriteLine (sideAccelerations[length - 1] + ", " + Time.time);

				if(length > 100){		//100 frames takes into account approximately 3 seconds at a time
					positions.RemoveAt(0);
					velocities.RemoveAt(0);
					accelerations.RemoveAt(0);
					sideAccelerations.RemoveAt(0);
					times.RemoveAt(0);
				}
			}
		}
	}

	//format of sine curve is Asin(Bx + C) + D
	/*void EstimateCurveFit()
	{
		int length = positions.Count;
		//finding D (vertical shift)
		float sumOfAccelerations = 0;
		for(int i = 0; i< length; i++)
		{
			sumOfAccelerations += sideAccelerations[i];
		}
		float avgOfAccelerations = sumOfAccelerations / (length - 1);		//D
		Debug.Log("D: " + avgOfAccelerations); 

		//finding A (amplitude)
		float rms;
		float sumOfSquares = 0;
		for(int i = 0; i< length; i++)
		{
			sumOfSquares += Mathf.Pow(sideAccelerations[i], 2);
		}
		rms = Mathf.Sqrt(sumOfSquares/length);
		float amplitude = Mathf.Sqrt(2.0f) * rms;		//A
		Debug.Log ("A: " + amplitude);

		//finding B (2 * pi / period)
		bool accelOverRMS = false;
		int totalCrossings = 0;
		float initialTime = 0;

		while(!accelOverRMS){		//locates first crossing, going from below to above RMS + mean
			for (int i = 0; i < length; i++)
			{
				if(sideAccelerations[i] > (avgOfAccelerations + rms))
				{
					accelOverRMS = true;
					totalCrossings++;
					initialTime = times[i];
				}
			}
		}

		List<float> possFinalTimes = new List<float>();
		for(int i = 0; i < length; i++){ //counts other crossings
			if(accelOverRMS){
				if(sideAccelerations[i] < (avgOfAccelerations + rms)){
					totalCrossings++;
					accelOverRMS = false;
				}
			}
			else if(!accelOverRMS){
				if(sideAccelerations[i] > (avgOfAccelerations + rms)){
					totalCrossings++;
					accelOverRMS = true;
					possFinalTimes.Add(times[i]); //crossings with the same orientation as initial crossing could be final crossings
				}								  //after 100 iterations the last value in the array will be the final crossing time
			}
		}
		float finalTime = possFinalTimes[possFinalTimes.Count - 1];
		float period = (2.0f * (finalTime - initialTime) / (totalCrossings - 1));
		float periodCoefficient = Mathf.PI / period;		//B
		Debug.Log ("B: " + periodCoefficient);

		//finding C (horizontal shift)


	}*/

	void OnApplicationQuit(){
		file.Close ();
	}


}