using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class IOCamera: MonoBehaviour {

	public float turnSpeed = 4.0f;
	public Transform player;

	private Camera cam;
	private Vector3 offset, rightoff;

	void Start () {
		offset = new Vector3(player.position.x, player.position.y + 10.0f, player.position.z);
		cam = GetComponent<Camera> ();
	}
	void Update(){
		if(Input.GetButtonDown("Fire2")){
			RaycastHit hit;
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray,out hit)){
				Transform temp = hit.transform;
				if(temp.CompareTag("npc")){
					this.player = temp;
				}
			}

		}

	}
	void LateUpdate()
	{
		offset = Quaternion.AngleAxis (Input.GetAxis("Horizontal") * turnSpeed, Vector3.up) * offset;
		rightoff = Vector3.Cross (Vector3.up, offset);
		offset = Quaternion.AngleAxis (Input.GetAxis ("Vertical")*-1.0f *turnSpeed, rightoff) * offset;
		//offset = offset * Input.GetAxis ("Mouse ScrollWheel");
		if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
			offset = offset / 1.1f;
		} else if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
			offset = offset * 1.1f;
		}
		transform.position = player.position + offset; 
		transform.LookAt(player.position);
	}
}