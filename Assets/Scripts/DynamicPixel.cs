using System;
using System.Linq;
using Assets.Scripts;
using MathUtil;
using UnityEngine;
using System.Collections;

/* Dynamic Pixel */
// Pixels in motion
public class DynamicPixel : PhysicsObj
{
	public int ID = -1; // -1 is an invalid ID, and needs to be changed after spawn

	float x, y; // location
	float lastX, lastY; // last location, used for our "ray casting"
  
	float velX, velY;
	public float stickiness = 1500; // minimum speed for this pixel to stick
	float bounceFriction = 0.85f; // scalar multiplied to velocity after bouncing
  
	public Color col; // color of the pixel
	
	int size = 1; // width and height of the pixel
  	
	//DestructibleTerrain dT; // these vars and Start() are all about gettin everything connected...
	public PixelDestruction pD;
	//CustomTerrain terrain;
	CustomPhysics physics;
	//CustomRenderer renderer;
	CustomRayCast rayCast;
	//Explode explode;
    private World world;
    private DynamicLayerEntry worldEntry;
	
	public DynamicPixel (World world, Color c, float x, float y, float vX, float vY, int size)
	{
		col = c;
		this.x = x;
		this.y = y;
		lastX = x;
		lastY = y;
		velX = vX;
		velY = vY;
		
		this.size = size;
	    this.world = world;
	    this.worldEntry = world.CreateDecal(new Vector3I((int)x, (int)y, 1), c);
	}


    public void Start()
    {
        physics = pD.physics;
        rayCast = pD.rayCast;
        //explode = dT.explode;
    }

    public void Destroy()
    {
        pD.DestroyDynamicPixel(this);
        this.world.RemoveDecal(this.worldEntry);
        this.worldEntry = null;
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
  
	// CheckConstraints, also implemented as a PhysicsObj
	public void checkConstraints ()
	{
 		// Boundary constraints... only Remove the pixel if it exits the sides or bottom of the map
        if (!pD.World.IsInside(new Vector2I((int)x + 1, (int)y + 1)) || !pD.World.IsInside(new Vector2I((int)x - 1, (int)y - 1)))
		{ // we do + or - 1 to destroy before it gets stuck to sides/top/bottom of terrain edges
			Destroy();
//			renderer.Remove (this);
//			physics.Remove (this);
			return;
		}

		// Find if there's a collision between the current and last points
		int[] collision = rayCast.rayCast((int)lastX, (int)lastY, (int)x, (int)y);
		if (collision.Length > 0) 
			collide (collision [0], collision [1], collision [2], collision [3]);
		
		// reset last positions
		lastX = x;
		lastY = y;

        if (this.worldEntry != null)
            this.worldEntry.MoveTo(new Vector2I((int)x, (int)y));
	}
  
	/* Collide */
	// called whenever checkConstraints() detects a solid pixel in the way
	void collide (int thisX, int thisY, int thatX, int thatY)
	{
		//Check if the pixels below the collision point are empty, and allow the pixel to "fall into the cracks"
		int pixelHeight = thisY - 1;
		while(!pD.isPixelSolid(thisX, pixelHeight))
		{
			pixelHeight--;
		}


		// first determine if we should stick or if we should bounce
		if (velX * velX + velY * velY < stickiness * stickiness)
		{ // if the velocity's length is less than our stickiness property, Add the pixel
			// draw a rectangle by looping from x to size, and from y to size
			for (int i = 0; i < 1; i++)
			{ 
				for (int j = 0; j < 1; j++)
				{
					col = pD.getColor(thisX, pixelHeight);
					//if(pD.getColor(thisX - 1, pixelHeight) ==  col && pD.getColor(thisX + 1, pixelHeight) == col)
						pD.addPixel (col, thisX, pixelHeight + 1);
					//else
					//{
					//	float picker =  
					//}
					pD.addPixel (col, thisX, pixelHeight + 1);
					//DynamicPixelGraphic.AddPixel(col, thisX + i, thisY + j);
				}  
			}
			// Remove this dynamic pixel
			Destroy();
		}
		else
		{ // otherwise, bounce
			// find the normal at the collision point
      
			// to do this, we need to reflect the velocity across the edge normal at the collision point
			// this is done using a 2D vector reflection formula ( http://en.wikipedia.org/wiki/Reflection_(mathematics) )
      
			float[] pixelNormal = pD.getNormal ((int)thatX, (int)thatY);
		    if (pixelNormal.Any())
		    {
		        float d = 2*(velX*pixelNormal[0] + velY*pixelNormal[1]);

		        velX -= pixelNormal[0]*d;
		        velY -= pixelNormal[1]*d;
		    }

		    // apply bounce friction
			velX *= bounceFriction;
			velY *= bounceFriction;
      
			// reset x and y so that the pixel starts at point of collision
			x = thisX;
			y = thisY;
		}
	}
}