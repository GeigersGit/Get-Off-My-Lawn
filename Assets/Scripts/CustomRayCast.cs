using UnityEngine;
using System.Collections;

public class CustomRayCast
{

	/* RayCast */
	// Uses Bresenham's line algorithm to efficiently loop between two points, and find the first solid pixel
	// This particular variation always starts from the first point, so collisions don't happen at the wrong end.
	// returns an int array
	//       ||| x = ([0],[1]) point in empty space before collision point
	//       ||| o = ([2],[3]) collision point
	//(end)--||ox------- (start)
	//       |||
	// using http://www.gamedev.net/page/resources/_/reference/programming/sweet-snippets/line-drawing-algorithm-explained-r1275
	
	//DestructibleTerrain dT; // these vars and Start() are all about gettin everything connected...
	public PixelDestruction pD;
	//CustomTerrain terrain;
	
	public int[] rayCast (int startX, int startY, int lastX, int lastY)
	{
		int deltax = (int)Mathf.Abs (lastX - startX);        // The difference between the x's
		int deltay = (int)Mathf.Abs (lastY - startY);        // The difference between the y's
		int x = (int)startX;                       // Start x off at the first pixel
		int y = (int)startY;                       // Start y off at the first pixel
		int xinc1, xinc2, yinc1, yinc2;
		if (lastX >= startX) {                // The x-values are increasing
			xinc1 = 1;
			xinc2 = 1;
		} else {                         // The x-values are decreasing
			xinc1 = -1;
			xinc2 = -1;
		}
  
		if (lastY >= startY) {                // The y-values are increasing
			yinc1 = 1;
			yinc2 = 1;
		} else {                         // The y-values are decreasing
			yinc1 = -1;
			yinc2 = -1;
		}
		int den, num, numadd, numpixels;
		if (deltax >= deltay) {        // There is at least one x-value for every y-value
			xinc1 = 0;                  // Don't change the x when numerator >= denominator
			yinc2 = 0;                  // Don't change the y for every iteration
			den = deltax;
			num = deltax / 2;
			numadd = deltay;
			numpixels = deltax;         // There are more x-values than y-values
		} else {                         // There is at least one y-value for every x-value
			xinc2 = 0;                  // Don't change the x for every iteration
			yinc1 = 0;                  // Don't change the y when numerator >= denominator
			den = deltay;
			num = deltay / 2;
			numadd = deltax;
			numpixels = deltay;         // There are more y-values than x-values
		}
		int prevX = (int)startX;
		int prevY = (int)startY;
  
		//CustomTerrain ourTerrain; // added this...
		//ourTerrain = new CustomTerrain(new Texture2D(1, 1), 4); // added this... probably a really bad idea...
		
		for (int curpixel = 0; curpixel <= numpixels; curpixel++) {
			if (pD.isPixelSolid (x, y))
				return new int[]{prevX, prevY, x, y};
			prevX = x;
			prevY = y;  
    
			num += numadd;              // Increase the numerator by the top of the fraction
    
			if (num >= den) {             // Check if numerator >= denominator
				num -= den;               // Calculate the new numerator value
				x += xinc1;               // Change the x as appropriate
				y += yinc1;               // Change the y as appropriate
			}
    
			x += xinc2;                 // Change the x as appropriate
			y += yinc2;                 // Change the y as appropriate
		}  
		return new int[]{};
	}
}
