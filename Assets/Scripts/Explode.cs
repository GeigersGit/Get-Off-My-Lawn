using UnityEngine;
using System.Collections;

public class Explode
{
	public PixelDestruction pD;
	CustomPhysics physics;
//	CustomRayCast rayCast;
	
	/* Explode */
	// Creates an "explosion" by finding all pixels near a point and launching them away
	public void explode (int xPos, int yPos, float radius)
	{
		float radiusSq = radius * radius;
  
		// loop through every x from xPos-radius to xPos+radius
		for (int x = xPos - (int)radius; x < xPos + (int)radius; x += pD.DestructionResolution)
		{
    
			// first make sure that the x is within terrain's boundaries
			if (x >= pD.World.Minimum.x && x <= pD.World.Maximum.x)
			{ 
      
				// next loop through every y pos in this x column
				for (int y = yPos - (int)radius; y < yPos + (int)radius; y += pD.DestructionResolution)
				{

                    if (y >= pD.World.Minimum.y && y <= pD.World.Maximum.y) // boundary check
					{
        
						// first determine if this pixel (or if any contained within its square area) is solid
						int solidX = 0, solidY = 0;
						bool solid = false;
						// loop through every pixel from (xPos,yPos) to (xPos + destructionRes, yPos + destructionRes)
						// to find whether this area is solid or not
						for (int i = 0; i < pD.DestructionResolution && !solid; i++)
						{
							for (int j = 0; j < pD.DestructionResolution && !solid; j++)
							{
								if (pD.isPixelSolid (x + i, y + j))
								{
									solid = true;
									solidX = x + i;
									solidY = y + j;
								}
							}
						}
						if (solid) // we know this pixel is solid, now we need to find out if it's close enough
						{
							float xDiff = x - xPos;
							float yDiff = y - yPos;
							float distSq = xDiff * xDiff + yDiff * yDiff;
							// if the distance squared is less than radius squared, then it's within the explosion radius
							if (distSq < radiusSq)
							{
								// finally calculate the distance
								float distance = Mathf.Sqrt (distSq);
              
								// the speed will be based on how far the pixel is from the center
								float speed = 800 * (1 - distance / radius);
              
								if (distance == 0)
									distance = 0.001f; // prevent divide by zero in next two statements
                
								// velocity
								float velX = speed * (xDiff + Random.Range(-10, 10)) / distance; //random (-10, 10)) / distance; 
								float velY = speed * (yDiff + Random.Range(-10, 10)) / distance;
              
								// create the dynamic pixel
								//DynamicPixel pixel = new DynamicPixel (terrain.getColor (solidX, solidY), x, y, velX, velY, terrain.destructionRes);
								pD.CreateDynamicPixel(pD.getColor(solidX, solidY), x, y, velX, velY, pD.DestructionResolution);
              
								// Remove the static pixels
								for (int i = 0; i < pD.DestructionResolution; i++)
								{
									for (int j = 0; j < pD.DestructionResolution; j++)
									{
										pD.removePixel (x + i, y + j);
									}
								}
							}
						}
					}
				}  
			}
		}
	}
}