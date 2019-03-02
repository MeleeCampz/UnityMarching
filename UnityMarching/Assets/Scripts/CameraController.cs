using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float lookSensitivity;
	public float moveSensitivity;

	Vector3 _lastMousePos;
	// Update is called once per frame
	void Update()
	{
		//Rotate if right mouse button is down
		if (Input.GetMouseButton(1))
		{ 
			Vector3 mousePos = Input.mousePosition;
			Vector3 delta = mousePos - _lastMousePos;

			Vector2 angleDiff = delta * Time.deltaTime * lookSensitivity;

			Vector3 currentRot = transform.rotation.eulerAngles;
			transform.rotation = Quaternion.Euler(currentRot.x - angleDiff.y, currentRot.y + angleDiff.x, currentRot.z);
		}

		Vector3 dir = Vector3.zero;

		if (Input.GetKey(KeyCode.W))
		{
			dir += transform.forward;
		}
		if (Input.GetKey(KeyCode.A))
		{
			dir -= transform.right;
		}
		if (Input.GetKey(KeyCode.S))
		{
			dir -= transform.forward;
		}
		if (Input.GetKey(KeyCode.D))
		{
			dir += transform.right;
		}

		dir.Normalize();

		transform.position += dir * moveSensitivity * Time.deltaTime;

		_lastMousePos = Input.mousePosition; ;
	}
}
