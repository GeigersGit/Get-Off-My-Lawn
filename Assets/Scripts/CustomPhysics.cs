using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* PhysicsObj */
// Any object that will need motion integrated will implement this
// these methods allows the Physics class to forward the object's position using its velocity
public interface PhysicsObj
{
	float getX ();
	float getY ();
	float getVX ();
	float getVY ();
	void setX (float pX);
	void setY (float pY);
	void setVX (float vX);
	void setVY (float vY);
	void checkConstraints ();

	int getID();
	void setID(int ID);
}

/* Physics */
// Apply motion to objects
public class CustomPhysics
{
	long previousTime;
	long currentTime;
	readonly int fixedDeltaTime = 16; // this was "final" in Java...
	float fixedDeltaTimeSeconds; //= (float)fixedDeltaTime / 1000.0;    // I ask myself, why not just do the math ahead of time... perhaps in processing this would work...
	//float fixedDeltaTimeSeconds = 0.016f;
	int leftOverDeltaTime = 0;
	
    // We will be doing lots of looping and less occassionally, inserts and removes
    // List should be fine as long we do our removals smart and never erase from middle
    public List<PhysicsObj> activeObjects = new List<PhysicsObj>();

    // Batch together all additions and removals so we don't interfere with looping through the list
    public Queue<PhysicsObj> newObjects = new Queue<PhysicsObj>();
    public Queue<PhysicsObj> abandonedObjects = new Queue<PhysicsObj>();
	
	// Constructor
	public CustomPhysics ()
	{

	}
  
	public void Add (PhysicsObj obj)
	{
        //this.newObjects.Enqueue(obj);
        // Switched to immediate insertion
        this.activeObjects.Add(obj);
	}

	public void Remove (PhysicsObj obj)
	{
        this.abandonedObjects.Enqueue(obj);
	}
  
	// integrate motion
	public void Update ()
	{
		// This game uses fixed-sized timesteps.
		// The amount of time elapsed since last Update is split up into units of 16 ms
		// any left over is pushed over to the next Update
		// we take those units of 16 ms and Update the simulation that many times.
		// a fixed timestep will make collision detection and handling (in the Player class, esp.) a lot simpler
		// A low framerate will not compromise any collision detections, while it'll still run at a consistent speed. 
		
        Profiler.BeginSample("Physics.Update()");

        ProcessQueuedRemovals();
	    ProcessQueuedAdditions();

		fixedDeltaTimeSeconds = (float)fixedDeltaTime / 1000.0f; // added this here instead of in initializer
		currentTime = (long)Time.time * 1000; //millis (); // time passed * 1000 = milliseconds passed... casted as long...
		long deltaTimeMS = currentTime - previousTime; // how much time has elapsed since the last Update
    
		previousTime = currentTime; // reset previousTime
    
		// Find out how many timesteps we can fit inside the elapsed time
		int timeStepAmt = (int)((float)(deltaTimeMS + leftOverDeltaTime) / (float)fixedDeltaTime); 
    
		// Limit the timestep amount to prevent freezing
		timeStepAmt = Mathf.Min(timeStepAmt, 1);//min (timeStepAmt, 1);
  
		// store left over time for the next frame
		leftOverDeltaTime = (int)deltaTimeMS - (timeStepAmt * (int)fixedDeltaTime);
    
		for (int iteration = 1; iteration <= timeStepAmt; iteration++)
		{
            for (int i =0; i < activeObjects.Count; ++i)
		    {
				CheckPhysics(this.activeObjects[i]); // check its physics
			}
		}

        Profiler.EndSample();
	}

    private void ProcessQueuedAdditions()
    {
        this.activeObjects.AddRange(this.newObjects);
        this.newObjects.Clear();
    }

    private void ProcessQueuedRemovals()
    {
        Profiler.BeginSample("Physics.ProcessQueuedRemovals");

        foreach (var obj in this.abandonedObjects)
        {
            // Prevent moving a part of the entire list by doing a swap-erase
            int index = this.activeObjects.IndexOf(obj);
            if (index == -1)
                Debug.LogWarning("Could not find physicsObj in list of activeObjects");
            else
            {
                // Swap with last element, then Remove from end
                this.activeObjects[index] = this.activeObjects.Last();
                this.activeObjects.RemoveAt(this.activeObjects.Count - 1);
            }
        }
        
        this.abandonedObjects.Clear();
        
        Profiler.EndSample();
    }

	void CheckPhysics(PhysicsObj obj)
	{
		float velX = obj.getVX ();
		float velY = obj.getVY ();
		
		// Add gravity
		velY -= 980 * fixedDeltaTimeSeconds;
		
		obj.setVY (velY);
		
		// Always Add x velocity
		obj.setX (obj.getX () + velX * fixedDeltaTimeSeconds);

		// if it's a player, only Add y velocity if he's not on the ground.
		//if (obj instanceof Player){ // in unity we will need to change this...
		if(obj.GetType() == typeof(Player))
		{
			if (!(((Player)obj).onGround && velY < 0))
				obj.setY (obj.getY () + velY * fixedDeltaTimeSeconds);
		}
		else
			obj.setY (obj.getY () + velY * fixedDeltaTimeSeconds);
		
		// allow the object to do collision detection and other things
		obj.checkConstraints ();
	}
}