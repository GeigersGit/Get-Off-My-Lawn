using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
	public Vector2 TargetPosition = Vector2.zero;

	// Use this for initialization
	void Start()
	{
	
	}
	
	// Update is called once per frame
	void Update()
	{
		if(TargetPosition != Vector2.zero)
		{
			transform.position = new Vector3(TargetPosition.x, TargetPosition.y, -50);
		}
	}
}
