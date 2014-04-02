 using UnityEngine;
using System.Collections;
//including some .NET for dynamic arrays called List in C#
using System.Collections.Generic;
using System.IO;
using System;

//directives to enforce that our parent Game Object required components
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Steering))]
[RequireComponent(typeof(NavMeshAgent))]

 // Restructured by gmb9280 3/20/2014
 // Made the class abstract since we are only going to have
 // people that are specialized villagers. 

 // This object will be focused on setting the Vector3 position 
 // that a Villager would seek. 

public class Villager : MonoBehaviour
{
    // We're going to make these NavMeshAgents
	protected FSMmatcher stMatch;
	protected int currSt;
	private Steering steering; // for low-level stuff
	private GameManager gameManager; // Ref to the game manager instance

    protected NavMeshAgent navAgent; // Reference to NavMeshAgent component
	protected CharacterController characterController;

    /**************************************************/
    protected GameObject target; // A gameobject target that can be used to grab coordinates for NavMeshAgent

    // Float that the path following will check to 
    // see if the target point has been reached (Arbitrary for now)
    private const float TARGET_COLLISION_RADIUS = 2.0f; 

    // gmb9280: Method to get the transform of the target object. 
    // Returns null if we don't have a target. 
    protected Vector3 GetTargetPosition()
    {
        /*if (this.target == null) return null;
        else
        {*/
            return target.transform.position;
        //}
    }

    // gmb9280: Getter for the protected variable target. 
    // Did not make setter because that should not happen from outside the class.
    public GameObject Target
    {
        get { return this.target; }
    }

    // gmb9280: protected method to set the target. 
    protected void SetTarget(GameObject target_)
    {
        this.target = target;
    }
    
    // A Vector3 target position list that is used with path following
    protected List<Vector3> targetPointList;

    // Returns the next target in the target list
    // ( Character will seek this first )
    protected Vector3 NextTargetPoint()
    {
        // If there are no points to go to, do nothing
        if (targetPointList.Count == 0)
        {
            //return null; 
			return this.transform.position;
        }
        
        // If we still have a point to go to, 
        else
        {
            // Return the last point in the list (LIFO)
            return targetPointList[targetPointList.Count - 1];
        }
    }

    // Adds a Vector3 to seek at the end of the list
    protected void AddPointToBack(Vector3 additionalPoint_)
    {
        // Don't really know how to sanitize input here, 
        // but maybe that's a TODO
        this.targetPointList.Add(additionalPoint_);
		Debug.Log ("Added to point list. Count: " + targetPointList.Count );
    }

    // Adds a Vector3 to seek at the beginning of the list 
    // ( Character will seek this last )
    protected void AddPointToFront(Vector3 additionalPoint_)
    {
        // Don't really know how to sanitize input here, 
        // but maybe that's a TODO
        this.targetPointList.Insert(0, additionalPoint_);
    }

    // For path following only, NOT character following
    // If point has been reached, look for next target point
    protected void PathUpdate()
    {
        // Has next targetPoint been reached? 
        if (this.WithinRange(NextTargetPoint(), TARGET_COLLISION_RADIUS))
        {
            // Pop the last node off of targetPointList
            this.targetPointList.RemoveAt(this.targetPointList.Count - 1);
        }
        else
        {
            // Make sure that the target is set with the NavMeshAgent component
            this.navAgent.SetDestination(NextTargetPoint());
        }
    }

    // Checks if THIS object is within range of another vector3 position
    protected bool WithinRange(Vector3 vec_, float radius)
    {
        if (Math.Abs(this.transform.position.x - vec_.x) < radius &&
            Math.Abs(this.transform.position.y - vec_.y) < radius &&
            Math.Abs(this.transform.position.x - vec_.z) < radius)
        { return true; }

        else return false;
    }

	
	// Initializes vars
	protected void InitNav()
	{
		// Set target gameobject to self
		this.target = this.gameObject;
		
		// Initialize list
		this.targetPointList = new List<Vector3>();
		
		// Add a few random points on the list for now
		// TODO: integrate list AND enum behavior
		System.Random rand = new System.Random();
		this.AddPointToBack(new Vector3(rand.Next(100), rand.Next (5), rand.Next (100)));
	}

    /**************************************************/

    //Constructor for "generic" type Villager
    protected void Start()
    {
		Debug.Log ("New villager created.");
        // Retrieve component references from settings in Unity
        this.characterController = gameObject.GetComponent<CharacterController>();

        this.navAgent = gameObject.GetComponent<NavMeshAgent>();

        this.steering = gameObject.GetComponent<Steering>(); // for basic forces?

        gameManager = GameManager.Instance; // Only one GameManager
		InitNav();

        // Reading in from text files...
        FSMPath = "Assets/Resources/VillagerFSM.txt";
        LoadFSM();
        currSt = 0; // Initialize state in constructor???? TODO: Fix state machine

        //leaderFollowBool = false; // following mayor TODO: What the hell is this boolean and change it to a dynamic thing

        //nearWere = false; // if near werewolf for decision tree: TODO: make this a method so we don't need a member boolean

        //wereInCity = false; //if werewolves have infiltrated the city (This should be part of the game state maybe?) TODO: fix name of this variable
    }




    // TODO: Possibly change these to game globals or private functions. 
	private bool nearWere;
	private bool wereInCity;

    // TODO: Make states an enum dynamically? Because I really want it to be an enum.


	

	//Wander variables for Steering. 
	public int _wanAngle;
	public int radiusOfCircle;
	public int _wanChange;
	
	//Follower reference, mostly for deletion
	private Follow follower;

    // Getter and setter for the private variable follower
	public Follow Follower
    {
        get{return follower;} 
        set{follower = value;}
    }
	
	// Unique identification index assigned by the Game Manager TODO: discover why we need this
	protected int index = -1; // auto assigned to an error variable to show that it is unassigned

    // Getter/setter for the private variable index
	public int Index
    {
		get { return index; }
		set { index = value; }
	}

	// Sets a reference to the manager's GameManager component (script)
	protected void SetGameManager (GameObject gManager)
	{
		gameManager = gManager.GetComponent<GameManager> ();
	}
	
	// We won't need movement variables because we are going to make this nice and 
    // NavMesh-y

	//list of nearby villagers
	protected List<GameObject> nearVillagers = new List<GameObject> ();
	protected List<float> nearVillagersDistances = new List<float> ();


    /******************* UPDATE ********************/

	// Update is called once per frame
	public void Update ()
	{
        /* Removed to see how we fare without
		CalcSteeringForce ();
		ClampSteering (); 
		
		moveDirection = transform.forward * steering.Speed;
		// movedirection equals velocity
		//add acceleration
		moveDirection += steeringForce * Time.deltaTime;
		//update speed
		steering.Speed = moveDirection.magnitude;
		if (steering.Speed != moveDirection.magnitude) {
			moveDirection = moveDirection.normalized * steering.Speed;
		}
		//orient transform
		if (moveDirection != Vector3.zero)
			transform.forward = moveDirection;
		
		// Apply gravity
		moveDirection.y -= gravity;
        */
		// the CharacterController moves us subject to physical constraints

		//characterController.Move (moveDirection * Time.deltaTime);
		
		PathUpdate ();
	}

	
    /**************** end UPDATE ********************/
	
	//Movement AI Behaviors -----------------------------------------------------------------------

	/*
	private Vector3 Separation ()
	{
		//empty our lists
		nearVillagers.Clear ();
		nearVillagersDistances.Clear ();
		
		//method variables
		Vector3 dv = new Vector3(); // the desired velocity
		Vector3 sum = new Vector3();
		
		for(int i = 0; i < gameManager.Villagers.Count; i++)
		{
			//retireves distance between two flockers of reference numbers
			// findFlocker and i
			
			GameObject villager = gameManager.Villagers[i];
			
			float dist = Vector3.Distance(this.transform.position, gameManager.Villagers[i].transform.position);
			
			if(dist < 10.0 && dist != 0)
			{
				dv =  this.transform.position - villager.transform.position;
				
				dv.Normalize();
				
				dv = dv * ((1.0f/dist));
				sum += dv;
			}
		}
		
		float dist2 = Vector3.Distance(this.transform.position, gameManager.Mayor.transform.position);
		
		if(dist2 <= 10.0 && dist2 != 0)
		{
			dv = this.transform.position - gameManager.Mayor.transform.position;
			
			dv.Normalize();
			
			dv = dv * ((1.0f/dist2));
			
			sum += dv;
		}
		
		
		//sum.Normalize();
		//sum = sum * (steering.maxSpeed);
		sum = sum - this.steering.Velocity;

		return steering.AlignTo(sum);
	}
	
	private Vector3 runAway()
	{
		
		steeringForce = Vector3.zero;
		
		for(int i = 0; i < gameManager.Werewolves.Count; i++)
		{
			if(Vector3.Distance(gameManager.Werewolves[i].transform.position, this.transform.position) < 80)
			{
				steeringForce += steering.Evasion(gameManager.Werewolves[i].transform.position);	
			}
			else
			{
				steeringForce += Vector3.zero;	
			}
		}
		
		for(int i = 0; i < gameManager.Werewolves.Count; i++)
		{
			if(Vector3.Distance(gameManager.Werewolves[i].transform.position, this.transform.position) < 20)
			{
				steeringForce += steering.Flee(gameManager.Werewolves[i]);	
			}
			else
			{
				steeringForce += Vector3.zero;
			}
		}
		
		return steeringForce;
	}*/
	
	
	//---------------------------------------------------------------------------------------------

    //Handles Collision with Cart for Scoring and Clean Up and UI purposes
    public void OnCollisionEnter(Collision wCollision)
    {
        if (wCollision.gameObject.tag == "Cart")
        {
            GameObject savedVillager = this.gameObject;
            Villager safe = this;
            gameManager.Villagers.Remove(savedVillager);
            gameManager.vFollowers.Remove(follower.gameObject);
            gameManager.Followers.Remove(safe);
            Destroy(follower.gameObject);
            Destroy(follower);
            Destroy(savedVillager);
            Destroy(this);
            gameManager.createNewVillager();
            gameManager.Saved.SavedVillagers = gameManager.Saved.SavedVillagers + 1;

            Destroy(savedVillager);

        }
    }

    /****************** File I/O ********************/
    //File IO variables
    int nStates;		// Number of states
    int nInputs;		// Number of input classes
    string[] states;	// Array of state names
    string[] inputs;	// Array of input class names
    int[,] trans;	// Transition table derived from a transition diagram
    private string FSMPath = null; // Data file name expected in bin folder

    // Look up the next state from the current state and the input class
    public int MakeTrans(int currState, int inClass)
    {
        return trans[currState, inClass];
    }

    // Read the data file to define and fill the tables
    void LoadFSM()
    {

        StreamReader inStream = new StreamReader(FSMPath);

        // State table
        nStates = int.Parse(inStream.ReadLine());
        states = new string[nStates];
        for (int i = 0; i < nStates; i++)
            states[i] = inStream.ReadLine();

        // Input table
        nInputs = int.Parse(inStream.ReadLine());
        inputs = new string[nInputs];
        for (int i = 0; i < nInputs; i++)
            inputs[i] = inStream.ReadLine();

        // Transition table
        trans = new int[nStates, nInputs];
        for (int i = 0; i < nStates; i++)
        {
            string[] nums = inStream.ReadLine().Split(' ');
            for (int j = 0; j < nInputs; j++)
                trans[i, j] = int.Parse(nums[j]);
        }
        //EchoFSM ();	// See it verything got into the tables correctly
    }

    public int NInputs	// Main needs to know this
    {
        get
        {
            return nInputs;
        }
    }

    public string[] Inputs	// Ghost classes need to see this
    {
        get
        {
            return inputs;
        }
    }

    /******************** end File I/O *****************/
	
	
}