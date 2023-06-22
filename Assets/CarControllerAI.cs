using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// A hummingbird Machine Learning Agent
/// </summary>
public class CarControllerAI : Agent
{

    GameObject tracker;
    public GameObject[] waypoints;
    int currentWP = 0;      
    public float lookAhead = 10.0f;
    GameObject player;
    [Tooltip("Acceleration speed")]
    public float acceleration = .3f;

    [Tooltip("Max speed")]
    public float maxSpeed = 20f;

    [Tooltip("Speed to rotate around the up axis")]
    public float yawSpeed = 100f;


    [Tooltip("The agent's camera")]
    public Camera agentCamera;

    [Tooltip("Whether this is training mode or gameplay mode")]
    public bool trainingMode;

    // The rigidbody of the agent
    new private Rigidbody rigidbody;
    
    private float speed = 0f;
    private Vector3 move = Vector3.zero;

    // Allows for smoother yaw changes
    private float smoothYawChange = 0f;

    public Vector3 startPosition;
    // Whether the agent is frozen ()
    private bool frozen = false;


    private Quaternion startRot;
    /// <summary>
    /// Initialize the agent
    /// </summary>
    public override void Initialize()
    {

        rigidbody = GetComponent<Rigidbody>();
        tracker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        DestroyImmediate(tracker.GetComponent<Collider>());
        tracker.transform.position = this.transform.position;
        tracker.transform.rotation = this.transform.rotation;
        tracker.GetComponent<MeshRenderer>().enabled = false;
        // If not training mode, no max step, play forever
        if (!trainingMode) MaxStep = 0;
        startPosition = this.transform.position;
        startRot = this.transform.rotation;

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
        speed = 0;
        this.transform.rotation = startRot;
        this.transform.position = startPosition;
        tracker.transform.position = this.transform.position;
        tracker.transform.rotation = this.transform.rotation;
        // Zero out velocities so that movement stops before a new episode begins
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        currentWP = 0;
        // Default to spawning in front of a flower
        bool inFrontOfFlower = true;
       

       
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
       // if (frozen) return;

        // Calculate movement vector
        move.Set(vectorAction[0], vectorAction[1], vectorAction[2]);

        // Add force in the direction of the move vector
        //rigidbody.velocity.Set();
        //rigidbody.AddForce(move * moveForce);






        

        Quaternion lookAtWP = Quaternion.LookRotation(tracker.transform.position - this.transform.position);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookAtWP, yawSpeed * Time.deltaTime);

        this.rigidbody.velocity = move * speed;
        if (Vector3.Distance(this.transform.position, tracker.transform.position) > lookAhead)
        {
            return;
        }
        tracker.transform.LookAt(waypoints[currentWP].transform);

        tracker.transform.Translate(0.0f, 0.0f, (speed + 20.0f) * Time.deltaTime);
        // Get the current rotation
        //Vector3 rotationVector = transform.rotation.eulerAngles;
        //rigidbody.AddForce(move * speed);
        // Calculate pitch and yaw rotation

        //float yawChange = vectorAction[3];

        // Calculate smooth rotation changes

        // smoothYawChange = Mathf.MoveTowards(smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);

        //float yaw = rotationVector.y + smoothYawChange * Time.fixedDeltaTime * yawSpeed;
        //this.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        
    }

    /// <summary>
    /// Collect vector observations from the environment
    /// </summary>
    /// <param name="sensor">The vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // If nearestFlower is null, observe an empty array and return early
        /*if (nearestFlower == null)
        {
            sensor.AddObservation(new float[10]);
            return;
        }*/

        // Observe the agent's local rotation (4 observations)
        sensor.AddObservation(transform.localRotation.normalized);

       
        

        
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

        if (speed <= maxSpeed)
        {
            speed = speed + acceleration;
            
        }
        Vector3 forward = Vector3.zero;
        if (Vector3.Distance(tracker.transform.position, waypoints[currentWP].transform.position) < 3)
        {
            currentWP++;
        }
        if (currentWP >= waypoints.Length)
        {
            currentWP = 0;
        }



       

        tracker.transform.Translate(0, 0, (speed + 1) * Time.deltaTime);

        
        forward = this.transform.forward;
        // Combine the movement vectors and normalize
        Vector3 combined = (forward).normalized;

        // Add the 3 movement values, pitch, and yaw to the actionsOut array
        actionsOut[0] = combined.x;
        actionsOut[1] = combined.y;
        actionsOut[2] = combined.z;
        
        actionsOut[3] = yaw;
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
    /// Called every frame
    /// </summary>
    private void Update()
    {
         


    }

    /// <summary>
    /// Called every .02 seconds
    /// </summary>
    private void FixedUpdate()
    {

       
    }
}
