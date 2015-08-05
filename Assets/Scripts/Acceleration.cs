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
	public GameObject player; 
	private int frames = 1;
	public StreamWriter file;
				  //A		 B					C			 D
	private float amplitude, periodCoefficient, horizShift2, avgOfAccelerations;

	void Start () {
		player = GetComponent<GameObject>();
		positions = new List<Vector3>();
		velocities = new List<Vector3>();
		accelerations = new List<Vector3>();
		sideAccelerations = new List<float>();
		if(VRDevice.isPresent){
			file = new StreamWriter(@"C:\\Users\\admin\\Desktop\\PUT UNITY BUILDS HERE\\week 2\\group 4\\output.txt");
		}
		else{
			file = new StreamWriter(@"C:\\Users\\wilkinsonm\\My Documents\\WalkingInPlace\\output.txt");
		}
	}

	// Update is called once per frame
	void Update () {
		if(VRDevice.isPresent){
			VRRecording(file);
			Debug.Log("A: " + amplitude + "B: " + periodCoefficient + "C: " + horizShift2 + "D: " + avgOfAccelerations);
		}
		else{
			PCRecording(file);
			Debug.Log("A: " + amplitude + "B: " + periodCoefficient + "C: " + horizShift2 + "D: " + avgOfAccelerations);
		}
		gradientDescent(ref amplitude, ref periodCoefficient, ref horizShift2, ref avgOfAccelerations);
	}
	
	//gets position data from VR Device
	void VRRecording (System.IO.StreamWriter file) {
		positions.Add(InputTracking.GetLocalPosition(VRNode.CenterEye));
		CalcAcceleration(file);
		EstimateCurveFit(file);
	}
	
	
	//gets position data from PC
	void PCRecording (System.IO.StreamWriter file) {
		positions.Add (transform.position);
		CalcAcceleration(file);
		EstimateCurveFit(file);
	}
	
	void CalcAcceleration(System.IO.StreamWriter file){
		int length = positions.Count;
		if(length > 1){
			Vector3 posChange = positions[length - 1] - positions[length - 2];
			velocities.Add(posChange/Time.deltaTime);
			if(length > 2){
				Vector3 velChange = velocities[velocities.Count - 1] - velocities[velocities.Count - 2];
				accelerations.Add(velChange/Time.deltaTime);
				
				//calculates how much of acceleration is in the local x direction (to the right)
				if(VRDevice.isPresent){
					Quaternion rotation = InputTracking.GetLocalRotation (VRNode.CenterEye);
					Vector3 newRotation = rotation * transform.right;
					sideAccelerations.Add (Vector3.Dot (accelerations[accelerations.Count - 1], newRotation));
					Debug.Log("SideAcceleration: " + sideAccelerations[accelerations.Count - 1]);
				}
				else{
					Vector3 newRotation = transform.right;
					sideAccelerations.Add (Vector3.Dot (accelerations[accelerations.Count - 1], newRotation));
					Debug.Log("SideAcceleration: " + sideAccelerations[accelerations.Count - 1]);
				}

				//file.WriteLine (sideAccelerations[accelerations.Count - 1] + ", " + Time.time);
				
				if(length > 100){		//100 frames takes into account approximately 3 seconds at a time
					positions.RemoveAt(0);
					velocities.RemoveAt(0);
					accelerations.RemoveAt(0);
					sideAccelerations.RemoveAt(0);
				}
			}
		}
	}
	
	//format of sine curve is Asin(Bx + C) + D
	void EstimateCurveFit(System.IO.StreamWriter file)
	{
		int length = sideAccelerations.Count;
		//finding D (vertical shift)
		float sumOfAccelerations = 0;
		for(int i = 0; i < length; i++)
		{
			sumOfAccelerations += sideAccelerations[i];
		}
		avgOfAccelerations = sumOfAccelerations / (length);		//D
		//file.WriteLine ("D: " + avgOfAccelerations);

		//finding A (amplitude)
		float rms;
		float sumOfSquares = 0;
		for(int i = 0; i < length; i++)
		{
			sumOfSquares += Mathf.Pow(sideAccelerations[i], 2);
		}
		rms = Mathf.Sqrt(sumOfSquares/length);
		amplitude = Mathf.Sqrt(2.0f) * rms;		//A
		file.WriteLine ("A: " + amplitude);

		//finding B (2 * pi / period)
		bool accelOverRMS = false;
		bool firstCrossFound = false;
		int totalCrossings = 0;
		int initialIndex = 0;

		//locates first crossing, going from below to above RMS + mean
		if(!firstCrossFound){
			for (int i = 0; i < length; i++)
			{
				if(sideAccelerations[i] > (avgOfAccelerations + rms))
				{
					accelOverRMS = true;
					firstCrossFound = true;
					totalCrossings++;
					initialIndex = i;
					break;
				}
			}
		}
		
		bool possFinalCross = false;
		List<int> possFinalIndices = new List<int>();
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
					possFinalCross = true;
					possFinalIndices.Add(i); 	  //crossings with the same orientation as initial crossing could be final crossings
				}								  //after 100 iterations the last value in the array will be the final crossing time
			}
		}
		int finalIndex;
		float period;
		if(possFinalIndices.Count > 0 && totalCrossings > 2){
			finalIndex = possFinalIndices[possFinalIndices.Count - 1];
			period = (2.0f * (float)(finalIndex - initialIndex) / (totalCrossings - 1));
			periodCoefficient = Mathf.PI / period;		//B
			file.WriteLine ("B: " + periodCoefficient);
			//finding C (horizontal shift)
			//float horizShift = Mathf.Asin((sideAccelerations[initialIndex] - avgOfAccelerations) / amplitude) - (periodCoefficient * initialIndex);		//C
			horizShift2 =(Mathf.PI * -(finalIndex) / period) + Mathf.PI / 4.0f;

		}

		float test = getError(amplitude, periodCoefficient, horizShift2, avgOfAccelerations);

	}
	float getError(float A, float B, float C, float D){
		float error = 0;
		for(int i = 0; i < sideAccelerations.Count; i++){
			error += Mathf.Pow((sideAccelerations[i] - (A * Mathf.Sin(B * i + C) + D)), 2) * (sideAccelerations.Count + i);
		}
		return error;
	}

	void gradientDescent(ref float A, ref float B, ref float C, ref float D){
		int steps = 100;
		float nA, nB, nC, nD;
		for(int i = 0; i <= steps; i++){
			float delta = Mathf.Pow(((steps + 1 - i)/2 * steps),2);
			float gA = getError (A + delta, B, C, D) - getError(A - delta, B, C, D);
			float gB = getError (A, B + delta, C, D) - getError(A, B - delta, C, D);
			float gC = getError (A, B, C + delta, D) - getError(A, B, C - delta, D);
			float gD = getError (A, B, C, D + delta) - getError(A, B, C, D - delta);
			float total = Mathf.Sqrt((Mathf.Pow(gA,2)) + (Mathf.Pow(gB,2)) + (Mathf.Pow(gC,2)) + (Mathf.Pow(gD,2))) * -1;
			nA = A + delta * gA/total;
			nB = B + delta * gB/total;
			nC = C + delta * gC/total;
			nD = D + delta * gD/total;
			float newError = getError (nA, nB, nC, nD); 
			float oldError = getError (A, B, C, D);
			if (newError < oldError){
				A = nA;
				B = nB;
				C = nC;
				D = nD;
			}
		}

	}

	void getVelocity(){

	}

	void OnApplicationQuit(){
		file.Close();
	}
	
	
}