using UnityEngine;
using System.Collections;
//
public class Controls
{
	// much of this has to be replaced by unity's control methods...
	
	public Player player;
	
	public void Update()
	{
		if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
		{
			player.jump();
		}
		
		if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
		{
			player.moveLeft();
		}
		else
			player.stopLeft();
		
		if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
		{
			player.moveRight();
		}
		else
			player.stopRight();
		
		if(Input.GetMouseButtonDown(0))
			player.shoot();
//		else
//			player.stopShooting();
		
		if(Input.GetMouseButton(1))
			player.shootAlt();
		else
			player.stopShootingAlt();
	}
}