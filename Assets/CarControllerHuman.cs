using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// A hummingbird Machine Learning Agent
/// </summary>
public class CarControllerHuman : Agent
{
    [Tooltip("Acceleration speed")]
    public float acceleration = .3f;
    public GameObject[] otherCars;
    [Tooltip("Max speed")]
    public float maxSpeed = 20f;

    [Tooltip("Speed to rotate around the up axis")]
    public float yawSpeed = 100f;

    public GameObject[] wayPoints;
    private int currentWP;
    [Tooltip("The agent's camera")]
    public Camera agentCamera;

    [Tooltip("Whether this is training mode or gameplay mode")]
    public bool trainingMode;

    // The rigidbody of the agent
    new private Rigidbody rigidbody;

   public float speed = 0f;
    private Vector3 move = Vector3.zero;
   
    // Allows for smoother yaw changes
    private float smoothYawChange = 0f;

    private Vector3 startPosition;
    // Whether the agent is frozen ()
    private bool frozen = false;

    public PlaceChecker placeTracker;
    private Quaternion startRot;
    private int fourthCount = 0;
    /// <summary>
    /// Initialize the agent
    /// </summary>
    public override void Initialize()
    {
        rigidbody = GetComponent<Rigidbody>();

        startPosition = this.transform.position;
        startRot = this.transform.rotation;
        // If not training mode, no max step, play forever
        if (!trainingMode) MaxStep = 0;
    }

    /// <summary>
    /// Reset the agent when an episode begins
    /// </summary>
    public override void OnEpisodeBegin()
    {
        if (trainingMode)
        {
            // Only reset flowers in training when there is one agent per area
           
        }

        // Reset nectar obtained


        // Zero out velocities so that movement stops before a new episode begins
        speed = 0;
        currentWP = 0;
        placeTracker.leadingWP = 0;
      /*
        otherCars[0].transform.position = otherCars[0].GetComponent<CarControllerAI>().startPosition;
        
        otherCars[0].GetComponent<Rigidbody>().velocity = Vector3.zero;
        otherCars[1].transform.position = otherCars[1].GetComponent<CarControllerAI>().startPosition;
        otherCars[1].GetComponent<Rigidbody>().velocity = Vector3.zero;

        otherCars[2].transform.position = otherCars[2].GetComponent<CarControllerAI>().startPosition;
        otherCars[2].GetComponent<Rigidbody>().velocity = Vector3.zero;*/
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        this.transform.position = startPosition;
        this.transform.rotation = startRot;
        placeTracker.leadingWP = 0;

    }
   
    /// <summary>
    /// Called when and action is received from either the player input or the neural network
    /// 
    /// vectorAction[i] represents:
    /// Index 0: move vector x (+1 = right, -1 = left)
    /// Index 1: move vector y (+1 = up, -1 = down)
    /// Index 2: move vector z (+1 = forward, -1 = backward)
    /// Index 3: pitch angle (+1 = pitch up, -1 = pitch down)
    /// Index 4: yaw angle (+1 = turn right, -1 = turn left)
    /// </summary>
    /// <param name="vectorAction">The actions to take</param>
    public override void OnActionReceived(float[] vectorAction)
    {
        // Don't take actions if frozen
        if (frozen) return;
        
        // Calculate movement vector
        move.Set(vectorAction[0], vectorAction[1], vectorAction[2]);

        // Add force in the direction of the move vector
        //rigidbody.velocity.Set();
        //rigidbody.AddForce(move * moveForce);




        

        // Get the current rotation
        Vector3 rotationVector = transform.rotation.eulerAngles;
        //rigidbody.AddForce(move * speed);
        // Calculate pitch and yaw rotation
     
        float yawChange = vectorAction[3];
        
        // Calculate smooth rotation changes

        smoothYawChange = Mathf.MoveTowards(smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);

        // Calculate new pitch and yaw based on smoothed values
        // Clamp  pitch to avoid flipping upside down


        float yaw = rotationVector.y + smoothYawChange * Time.fixedDeltaTime * yawSpeed;

        // Apply the new rotation
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    /// <summary>
    /// Collect vector observations from the environment
    /// </summary>
    /// <param name="sensor">The vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe the agents local rotation (4 observations due to quaternions beiing composed of 4)
        sensor.AddObservation(transform.localRotation.normalized);
        Vector3 toWP =  wayPoints[currentWP].transform.position - this.transform.position ;


        //1 observation
        sensor.AddObservation(toWP.normalized);
        sensor.AddObservation(Vector3.Dot(this.transform.forward.normalized, -wayPoints[currentWP].transform.position));
        //Total observation size of six

       
        //3 obsversation
        sensor.AddObservation(this.rigidbody.velocity);

    }

    /// <summary>
    /// When Behavior Type is set to "Heuristic Only" on the agent's Behavior Parameters,
    /// this function will be called. Its return values will be fed into
    /// <see cref="OnActionReceived(float[])"/> instead of using the neural network
    /// </summary>
    /// <param name="actionsOut">And output action array</param>
    public override void Heuristic(float[] actionsOut)
    {
        // Create placeholders for all movement/turning
        
        float yaw = 0f;
        Vector3 forward = Vector3.zero;

        // Convert keyboard inputs to movement and turning
        // All values should be between -1 and +1

        // Forward/backward
        if (Input.GetKey(KeyCode.W))
        {
            if ((speed < maxSpeed))
            {
                if (speed >= 0)
                    speed = speed + acceleration;
                if (speed < 0)
                    speed = speed + 3 * acceleration;
            }
            forward = transform.forward;

        }
        else if (Input.GetKey(KeyCode.S))
        {
            if ((speed > -maxSpeed))
            {

                if (speed <= 0)
                    speed = speed - acceleration;
                if (speed > 0)
                    speed = speed - 3 * acceleration;
            }
            forward = transform.forward;
        }
       



        // Turn left/right
        if (Input.GetKey(KeyCode.A)) yaw = -1f;
        else if (Input.GetKey(KeyCode.D)) yaw = 1f;

        
        // Combine the movement vectors and normalize
        Vector3 combined = (forward).normalized;

        // Add the 3 movement values, pitch, and yaw to the actionsOut array
        actionsOut[0] = combined.x;
        actionsOut[1] = combined.y;
        actionsOut[2] = combined.z;
        
        actionsOut[3] = yaw;
        //actionsOut[4] = speed;
    }

    /// <summary>
    /// Prevent the agent from moving and taking actions
    /// </summary>
    public void FreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Freeze/Unfreeze not supported in training");
        frozen = true;
        rigidbody.Sleep();
    }

    /// <summary>
    /// Resume agent movement and actions
    /// </summary>
    public void UnfreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Freeze/Unfreeze not supported in training");
        frozen = false;
        rigidbody.WakeUp();
    }


   

    

    /// <summary>
    /// Called when the agent collides with something solid
    /// </summary>
    /// <param name="collision">The collision info</param>
    private void OnCollisionEnter(Collision collision)
    {
        /*if (trainingMode && collision.collider.CompareTag("boundary"))
        {
            // Collided with the area boundary, give a negative reward
            AddReward(-.5f);
        }*/
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    private void Update()
    {
        if (speed < maxSpeed)
            speed = speed + acceleration;
        this.rigidbody.velocity = move * speed;

        if (Vector3.Distance(this.transform.position, wayPoints[currentWP].transform.position) < 3)
        {
            AddReward(100f);
            currentWP++;
        }
        if (currentWP >= wayPoints.Length)
        {
            currentWP = 0;
        }
        if (placeTracker.racers[3] == placeTracker.learner)
        {
            fourthCount++;
        }
        else
        {
            fourthCount = 0;
        }
        if (placeTracker.racers[0] == placeTracker.learner)
        {
            AddReward(1f);
        }
        else if (placeTracker.racers[1] == placeTracker.learner)
        {
            AddReward(.5f);
        }
        else if (placeTracker.racers[2] == placeTracker.learner)
        {
            AddReward(.1f);
        }

        if(fourthCount == 10)
        {
            otherCars[0].GetComponent<CarControllerAI>().EndEpisode();
            otherCars[1].GetComponent<CarControllerAI>().EndEpisode();
            otherCars[2].GetComponent<CarControllerAI>().EndEpisode();
            EndEpisode();
        }

        if ((Vector3.Distance(this.transform.position, wayPoints[wayPoints.Length - 1].transform.position) < 3) && wayPoints[currentWP] == wayPoints[wayPoints.Length-1])
        {
            AddReward(100000f);
        }
        Debug.Log("Reward: " + this.GetCumulativeReward());
        //Debug.Log(this.GetCumulativeReward());
        if (this.GetCumulativeReward() > 100000)
        {
            otherCars[0].GetComponent<CarControllerAI>().EndEpisode();
            otherCars[1].GetComponent<CarControllerAI>().EndEpisode();
            otherCars[2].GetComponent<CarControllerAI>().EndEpisode();
            speed = 0;
            EndEpisode();
        }
        else if (this.GetCumulativeReward() < -10)
        {
            otherCars[0].GetComponent<CarControllerAI>().EndEpisode();
            otherCars[1].GetComponent<CarControllerAI>().EndEpisode();
            otherCars[2].GetComponent<CarControllerAI>().EndEpisode();
            speed = 0;
            EndEpisode();
           
        }

    }

    /// <summary>
    /// Called every .02 seconds
    /// </summary>
    private void FixedUpdate()
    {


       

    }
}
