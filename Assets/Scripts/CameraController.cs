using UnityEngine;
using System.Collections;
using UnityEngine.VR;

public class CameraController : MonoBehaviour {
	public GameObject player;
	private Vector3 offset;
	private float lookAngle;
	private float rotationGain = 4.0f;
	public Rigidbody rb;
	public float movespeed = 10.0f;
	// Use this for initialization
	void Start () {
		offset = transform.position - player.transform.position;
		rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = player.transform.position + offset;
		
		float mouseXPosition = Input.GetAxis ("Mouse X");
		lookAngle += mouseXPosition * rotationGain;
		Quaternion rotation = Quaternion.AngleAxis (lookAngle, new Vector3 (0.0f, 1.0f, 0.0f));
		transform.rotation = rotation;

//	}
//	void FixedUpdate(){
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");
		
		Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
		
		transform.Translate (movement * Time.deltaTime * movespeed);
	}
}
//WE ARE IMPLEMENTING VR INTO LOOK AND MOVEMENT
//with InputTracking.GetLocalRotation