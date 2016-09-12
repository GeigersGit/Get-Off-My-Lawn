using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* Player object */
// Our little box that runs around the map
public class Player : PhysicsObj
{
	public int ID = -1; // -1 is an invalid ID, and needs to be changed after spawn

	public Texture2D PlayerTexture;
	public float x, y;
	float velX, velY;

	public float ShootDelayTime = 0.15f; // 150 milliseconds was default in the java/processing project, so .15 is default here

	// variables to track whether or not the user is pressing A/D
	bool goLeft;
	bool goRight;

	// Are we shooting?
	bool shooting;
	bool shootingAlt;

	// last time (ms) a bullet was shot, used to limit the firing rate
//	long lastShot;

	// variables for physics
	public bool onGround; // are we allowed to jump?
	bool topBlocked;
	public int playerWidth, playerHeight;

	public bool canShoot = true; // prevents repeated shots in a single click
	

	public PixelDestruction pD;
	public CustomPhysics physics;
	public CameraFollow targetCamera;
	//CustomRayCast rayCast;
	
	// Constructor
	public Player (int x, int y)
	{
		this.x = x;
		this.y = y;
		velX = 0;
		velY = 0; // set the initial velocity to 0
    	
		// initialize the player as a 32x32 px box
		playerWidth = 26; 
		playerHeight = 32;
	}

	// Shooting toggles
	public void shoot ()
	{
		//Debug.Log("shoot() ran - canShoot: " + canShoot + " - shooting: " + shooting);
		shooting = true;
	}

//	public void stopShooting ()
//	{
//		if (shooting)
//		{
//			shooting = false;
//		}
//	}

	public void shootAlt ()
	{
		if (!shootingAlt)
			shootingAlt = true;
	}

	public void stopShootingAlt ()
	{
		if(shootingAlt)
			shootingAlt = false;  
	}

	// jump
	public void jump ()
	{
		if (onGround && !topBlocked && velY < 300) //velY > -500)
//			velY -= 500;
			velY += 300;
	}

	// moving toggles
	public void moveLeft ()
	{
		goLeft = true;
	}

	public void moveRight ()
	{
		goRight = true;
	}

	public void stopLeft ()
	{
		goLeft = false;
	}

	public void stopRight ()
	{
		goRight = false;
	}

//	IEnumerator ShootTimer()
//	{
//		if(!canShoot)
//		{
//			yield return new WaitForSeconds(ShootDelayTime);
//			canShoot = true;
//		}
//	}

	// checkConstraints - implemented as a PhysicsObj
	public void checkConstraints ()
	{
		if(targetCamera != null)
			targetCamera.TargetPosition = new Vector2(x, y); // tell our camera to follow our movement

//		Debug.Log ("x/y " + x + " " + y);
//		Debug.Log((Time.time * 1000) - lastShot);

		// Shooting code
		if(shooting || shootingAlt)
		{
			// first if were doing primary fire we launch a bullet
			//Debug.Log("shootcode area - shooting: " + shooting + " - canShoot: " + canShoot);
			if (!shootingAlt && canShoot) // if were not ALT firing, and enough time has passed since last shot
			{
				//Debug.Log("Attempting Fire at " + Time.time);
				FirePrimary();
				shooting = false;
				canShoot = false;
				//StartCoroutine("ShootTimer"); // crud this isnt monobehavior...
				pD.StartShootTimer();
			}

			if(!shooting && canShoot)
			{
				FireSecondary();
				shootingAlt = false;
				canShoot = false;
				pD.StartShootTimer();
			}
		}

		// movement
		if (goLeft)
		{
			if (velX > -500)
				velX -= 40;
		}
		else if (velX < 0)
			velX *= 0.8f; // slow down side-ways velocity if we're not moving left
      
		if (goRight)
		{
			if (velX < 500)
				velX += 40;
		}
		else if (velX > 0)
			velX *= 0.8f;
    
		// Collision detection/handling
		// Loop along each edge of the square until we find a solid pixel
		// if there is one, we find out if there's any adjacent to it (loop perpendicular from that pixel into the box)
		// Once we hit empty space, we move the box to that empty space
		
		onGround = false; // onground starts out false and will remain unless set to true
		
		for (int bottomX = (int)x - (playerWidth / 2); bottomX <= (int)x + (playerWidth / 2); bottomX++)
		{
			if (pD.isPixelSolid (bottomX, (int)y - (playerHeight / 2) - 1) && (velY < 0))
			{
				onGround = true;
				for (int yCheck = (int)y - (playerHeight / 4); yCheck < (int)y - (playerHeight / 2); yCheck++)
				{
					if (pD.isPixelSolid (bottomX, yCheck))
					{
						//Debug.Log("Break - oG 1");
						y = yCheck - (playerHeight / 2);
						break;
					}
				}
				if (velY < 0)
				{
					velY *= -0.25f;
				}
			}
		}

		topBlocked = false;
		// start with the top edge
		for (int topX = (int)x - (playerWidth / 2); topX <= (int)x + (playerWidth / 2); topX++)
		{	
			if (pD.isPixelSolid (topX, (int)y + (playerHeight / 2) + 1)) // if the pixel is solid
			{
				topBlocked = true;
				if (velY > 0)
				{
					velY *= -0.5f; 
				}
			} 
		}
		// loop left edge
		if (velX < 0)
		{
			for (int leftY = (int)y - (playerHeight / 2); leftY <= (int)y + (playerHeight / 2); leftY++)
			{
				if (pD.isPixelSolid ((int)x - (playerWidth / 2), leftY))
				{
					// next move from the edge to the right, inside the box (stop it at 1/4th the player width)
					for (int xCheck = (int)x - (playerWidth / 4); xCheck < (int)x - (playerWidth / 2); xCheck--)
					{
						if (pD.isPixelSolid (xCheck, leftY))
						{
							x = xCheck + (playerWidth / 2); // push the block over 
							break; 
						}
					}
					if (leftY < y && !topBlocked)
					{
						y += 1;
					}
					else
					{
						velX *= -0.4f;
						x += 2;
					}
				}
			}
		}
		// do the same for the right edge
		if (velX > 0)
		{
			for (int rightY = (int)y - (playerHeight / 2); rightY <= (int)y + (playerHeight / 2); rightY++)
			{
				if (pD.isPixelSolid ((int)x + (playerWidth / 2), rightY))
				{
					for (int xCheck = (int)x + (playerWidth / 4); xCheck < (int)x + (playerWidth / 2) + 1; xCheck++)
					{
						if (pD.isPixelSolid (xCheck, rightY))
						{
							x = xCheck - (playerWidth / 2);
							break; 
						}
					}
					if (rightY < y && !topBlocked)
					{
						y += 1;
					}
					else
					{
						velX *= -0.4f;
						x -= 2;
					}
				}
			}
		}
    
		// Boundary Checks
		if (x < this.pD.World.Minimum.x && velX < 0)
		{
			x -= x;
			velX *= -1;
		}
        if (y < this.pD.World.Minimum.y && velY < 0)
		{
			y -= y;
			velY *= -1;
		}
        if (x > this.pD.World.Maximum.x && velX > 0)
		{
            x += this.pD.World.Maximum.x - x;
			velX *= -1;
		}
        if (y + (playerHeight / 2) > this.pD.World.Maximum.y && velY > 0)
		{
            y += this.pD.World.Maximum.y - y - (playerHeight / 2);
			velY = 0;
			//Debug.Log ("oG 2");
			//onGround = true; // DISABLED BECAUSE - was causing the player to stick into the top of the map when reaching it
		}
	}
  
	void FirePrimary()
	{
		// Create a vector between the player and the mouse, then normalize that vector (to change its length to 1)
		// after multiplying by the desired bullet speed, we get how fast along each axis we want the bullet to be traveling

		// convert mouse pos from screen coordinates to world coordinates
		Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		// compare mouse pos to our position, this will be used to aim our projectile
		float diffX = mouseWorld.x - (x);
		float diffY = mouseWorld.y - (y);

		Vector2 diffNormal = new Vector2(diffX, diffY).normalized;
		diffX = diffNormal.x;
		diffY = diffNormal.y;

		// create the bullet at 1500 px/sec, and Add it to our Physics
		Bullet bullet = new Bullet (x, y, 1500 * diffX, 1500 * diffY);
		bullet.pD = pD; // set the pixel destruction reference
		bullet.Start(); // init the bullet
		physics.Add (bullet); // Add to physics
	}

	void FireSecondary()
	{
		// Create a vector between the player and the mouse, then normalize that vector (to change its length to 1)
		// after multiplying by the desired bullet speed, we get how fast along each axis we want the bullet to be traveling

		// convert mouse pos from screen coordinates to world coordinates
		Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		
		// compare mouse pos to our position, this will be used to aim our projectile
		float diffX = mouseWorld.x - (x);
		float diffY = mouseWorld.y - (y);
		
		Vector2 diffNormal = new Vector2(diffX, diffY).normalized;
		diffX = diffNormal.x;
		diffY = diffNormal.y;

		int sprayCount = 100; // originally 150 - lowered to improve performance
		//sprayCount = sprayCount / pD.DestructionResolution; // we lower the total fired colored pixels if resolution is higher

		for (int i = 0; i < sprayCount; i++)
		{
			//float rand1 = Random.Range(0, 255);
			//float rand2 = Random.Range(0, 255);
			//float rand3 = Random.Range(0, 255);
			//Vector3 randoms = new Vector3(rand1, rand2, rand3).normalized;
			//rand1 = randoms.x;
			//rand2 = randoms.y;
			//rand3 = randoms.z; // a hackey way to get normalized (0-1 range) floats
			//RANDOM COLORS

			float red = Random.Range(180, 180);
			float green = Random.Range(130, 130);
			float blue = Random.Range(80, 80);
			Vector3 randoms = new Vector3(red, green, blue).normalized;
			red = randoms.x;
			green = randoms.y;
			blue = randoms.z; // a hackey way to get normalized (0-1 range) floats
			//SAND

			//GRASS

			//SNOW

			// create dynamic pixels
			pD.CreateDynamicPixel(new Color (red, green, blue, 1), // color
									x, y, // position
									Random.Range (-50, 50) + Random.Range (350, 500) * diffX, Random.Range (-50, 50) + Random.Range (350, 500) * diffY, // speed
		              				pD.DestructionResolution); // size
		}
	}

	// Methods implemented as a PhysicsObj
	public float getX ()
	{
		return x;
	}

	public float getY ()
	{
		return y;
	}

	public float getVX ()
	{
		return velX;
	}

	public float getVY ()
	{
		return velY;
	}

	public void setX (float pX)
	{
		x = pX;
	}

	public void setY (float pY)
	{
		y = pY;
	}

	public void setVX (float vX)
	{
		velX = vX;
	}

	public void setVY (float vY)
	{
		velY = vY;
	}

	public int getID()
	{
		return ID;
	}
	
	public void setID(int newID)
	{
		ID = newID;
	}
}