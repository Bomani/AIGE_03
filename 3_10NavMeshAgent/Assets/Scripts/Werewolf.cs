using UnityEngine;
using System.Collections;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Steering))]


public class Werewolf : MonoBehaviour {
	
	private CharacterController characterController;
	private Steering steering;
	private GameManager gameManager;
	
	private int index = -1;
	public int Index 
	{
		get { return index; }
		set { index = value; }
	}
	
	//movement variables
	private float gravity = 200.0f;
	private Vector3 moveDirection;
	
	//steering variable
	private Vector3 steeringForce;
	private GameObject respawnPont;
	
	//Hunting variables
	private GameObject target;
	private int preyIndex;
	
	// Use this for initialization
	void Start () 
	{
		//get component references
		characterController = gameObject.GetComponent<CharacterController> ();
		steering = gameObject.GetComponent<Steering> ();
		
		respawnPont = GameObject.FindGameObjectWithTag("Respawn");
		
		gameManager = GameManager.Instance;
		
		preyIndex = 0;
		target = gameManager.Villagers[preyIndex];
	}
	
	public void OnCollisionEnter(Collision wCollision)
	{
		if(wCollision.gameObject.tag == "Villager")
		{
			// Kill and destroy collided villager, and respawn
			GameObject deadVillager = wCollision.gameObject;
			Villager death = wCollision.gameObject.GetComponent<Villager>();

			gameManager.lifeTime[death.Index] = death.Lifetime;

			gameManager.Villagers.Remove(deadVillager);
			gameManager.vFollowers.Remove(death.Follower.gameObject);
			gameManager.Followers.Remove(death);
			Destroy(death.Follower.gameObject);
			Destroy(death.Follower);
			Destroy(deadVillager);
			gameManager.currVillagers -= 1;
			gameManager.Dead.DeadVillagers = gameManager.Dead.DeadVillagers + 1;
			
			findTarget(); // find a new target
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		CalcSteeringForce ();
		ClampSteering ();
		
		moveDirection = transform.forward * steering.Speed;
		// movedirection equals velocity
		//add acceleration
		moveDirection += steeringForce * Time.deltaTime;
		//update speed
		steering.Speed = moveDirection.magnitude;
		
		//area of fix
		if (steering.Speed != moveDirection.magnitude) {
			moveDirection = moveDirection.normalized * steering.Speed;
		}
		
		//orient transform
		if (moveDirection != Vector3.zero)
			transform.forward = moveDirection;
		
		// Apply gravity
		moveDirection.y -= gravity;
		
		// the CharacterController moves us subject to physical constraints
		characterController.Move (moveDirection * Time.deltaTime);
	}
	
	private void findTarget()
	{
		
		GameObject prey;
		
		for (int i = 0; i < gameManager.Villagers.Count; i++)
		{	
			prey = gameManager.Villagers[i];
			
			if(Vector3.Distance(this.transform.position, prey.transform.position) 
				< Vector3.Distance(this.transform.position, target.transform.position))
			{
				target = gameManager.Villagers[i];		
			}
		}
	}
		
	private void CalcSteeringForce ()
	{
		steeringForce = Vector3.zero;
		
		//Keeps werewolves away from Villager Spawn point
		steeringForce += gameManager.avoidWt * steering.AvoidObstacle(respawnPont, 1000f);
		
		
		float mayDist = Vector3.Distance(this.transform.position, gameManager.Mayor.transform.position);
		
		//Choose new villager to chase (closest villager)
		target = gameManager.Villagers[0];
		findTarget();
		
		float tarDist = Vector3.Distance(this.transform.position, target.transform.position);
		
			if(mayDist < 20)
			{
				steeringForce += 10 * steering.Flee(gameManager.Mayor);	
				
			}
			else if(tarDist > 30 && mayDist > 10)
			{
				steeringForce += 10 * steering.Seek(target.transform.forward +
			                                    target.transform.position);
				
			}
			else if(tarDist < 10 && mayDist > 5)
			{
				steeringForce += 10 * steering.Seek(target.transform.position);	
			}
			else
			{
				steeringForce += 2 * steering.Seek(target.transform.position);
			}
		
		//avoid close obstacles
		for(int i =0; i < gameManager.Obstacles.Length; i++)
		{
			if(Vector3.Distance(this.transform.position, gameManager.Obstacles[i].transform.position) < 60)
			{
				steeringForce += gameManager.avoidWt * steering.AvoidObstacle(gameManager.Obstacles[i], gameManager.avoidDist);	
			}
		}

        // Avoid mayor some more
        steeringForce += gameManager.avoidWt * steering.AvoidObstacle(gameManager.Mayor, 100f);
	}
	
	private void ClampSteering ()
	{
		if (steeringForce.magnitude > steering.maxForce) {
			steeringForce.Normalize ();
			steeringForce *= steering.maxForce;
		}
	}
	
	// tether type containment - not very good!
	private Vector3 StayInBounds ( float radius, Vector3 center)
	{
		steeringForce = Vector3.zero;
		
		if(transform.position.x > 750 || transform.position.x < 200 || 
			transform.position.z > 715 || transform.position.z < 205)
		{
			steeringForce += steering.Seek(gameManager.gameObject);
		}
		
		return steeringForce;
	
	}

	
}
