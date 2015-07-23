using UnityEngine;
using System.Collections;
using UnityEngine.VR;
using System.Collections.Generic;

public class Acceleration : MonoBehaviour {

	private List<Vector3> positions;
	private List<Vector3> velocities;
	private List<Vector3> accelerations;
	public GameObject player; 
	private int frames = 1;

	// Use this for initialization
	void Start () {
		player = GetComponent<GameObject>();
		positions = new List<Vector3>();
		velocities = new List<Vector3>();
		accelerations = new List<Vector3>();
	}
	
	// Update is called once per frame
	void Update () {
		if(VRDevice.isPresent){
			VRRecording();
		}
		else{
			PCRecording();
			frames++;

		}
		//Debug.Log ("The total number of time passed is " + Time.time);
	}

	//gets position data from VR Device
	void VRRecording () {
		positions.Add(InputTracking.GetLocalPosition(VRNode.CenterEye));
		CalcAcceleration();
	}

	//gets position data from PC
	void PCRecording () {
		positions.Add (transform.position);
		//Debug.Log(positions[frames]);

		CalcAcceleration();
	}

	void CalcAcceleration(){
		Vector3 posChange = positions[frames] - positions[frames - 1];

		velocities.Add(posChange/Time.deltaTime);
		Vector3 velChange = velocities[frames] - velocities[frames - 1];
		//Debug.Log(velocities[frames]);

		accelerations.Add(velChange/Time.deltaTime);
		Debug.Log(accelerations[frames] + ", " + Time.deltaTime + ", " + frames);
	}
}
