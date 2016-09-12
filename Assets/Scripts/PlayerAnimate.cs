using UnityEngine;
using System.Collections;

public class PlayerAnimate : MonoBehaviour
{
	public float X = 0;
	public float Y = 0;
	public bool Enabled = false;

	public PixelDestruction pD;
//	private float lastX = 0;
//	private float lastY = 0;
//
//	private float smoothX = 0;
//	private float smoothY = 0;
//
//	private bool firstRun = true;

	void Start()
	{
//		lastX = X;
//		lastY = Y;
	}

	void Update()
	{
//		if(firstRun)
//		{
//			lastX = X;
//			lastY = Y;
//			firstRun = false;
//		}

		if(Enabled)
		{
//			smoothX = Mathf.Lerp(lastX, X, 5 * Time.deltaTime);
//			smoothY = Mathf.Lerp(lastY, Y, 5 * Time.deltaTime);
//
//			lastX = smoothX;
//			lastY = smoothY;

			transform.position = new Vector2(X, Y);
		}
	}
}
