using UnityEngine;
using System.Collections;
//including some .NET for dynamic arrays called List in C#
using System.Collections.Generic;

[System.Serializable]
public class GameManager : MonoBehaviour
{
	// Weights for steering
	public float alignmentWt;
	public float separationWt;
	public float cohesionWt;
	public float avoidWt;
	public float inBoundsWt;

	// Also for steering behaviors
	public float avoidDist;
	public float separationDist;

	//Genetic Algorithm Variables
	ThreshPop tp;
	byte [] chroms;

	// "Constants"
	public int numberOfvillagers;
	public int numberOfWerewolves;
	private int currentNumberOfVillagers;

	public int currVillagers {
		get {return currentNumberOfVillagers;}
		set {currentNumberOfVillagers = value;}
	}

	public Object villagerPrefab;
	public Object werewolfPrefab;
	public Object obstaclePrefab;
	public Object followerPrefab;

	//values used by all villagers that are calculated by controller on update
	private Vector3 flockDirection;
	private Vector3 centroid;
	
	//accessors
	private static GameManager instance;
	public static GameManager Instance { get { return instance; } }

	public Vector3 FlockDirection {
		get { return flockDirection; }
	}
	
	public Vector3 Centroid { get { return centroid; } }
	public GameObject centroidContainer;
	
	 
		
	//mayor and accessor
	private GameObject mayor;
	public GameObject Mayor {get{return mayor;}}
	
	//Text GUI and accessors
	private Saved savedText;
	public Saved Saved {get{return savedText;}}
	
	private Dead deadText;
	public Dead Dead {get{return deadText;}}
	
	//list of werewolves with accessor 
	private List<GameObject> werewolves = new List<GameObject>();
	public List<GameObject> Werewolves {get{return werewolves;}}
	
	// list of villagers with accessor
	private List<GameObject> villagers = new List<GameObject>();
	public List<GameObject> Villagers {get{return villagers;}}
	
	//list of Mayor followers
	private List<Villager> mfollowers = new List<Villager>();
	public List<Villager> Followers {get{return mfollowers;}}
	
	//list of villager followers
	public List<GameObject> VillageFollowers = new List<GameObject>();
	public List<GameObject> vFollowers {get{return VillageFollowers;}}
	
	//list of werewolf followers
	public List<GameObject> WerewolfFollowers = new List<GameObject>();
	public List<GameObject> wFollowers {get{return WerewolfFollowers;}}

	public int[] lifeTime;
	

	// array of obstacles with accessor
	private  GameObject[] obstacles;
	public GameObject[] Obstacles {get{return obstacles;}}
	
	// this is a 2-dimensional array for distances between villagers
	// it is recalculated each frame on update (TODO: CHANGE THIS PLZ TO MORE EFFICENT)
	private float[,] distances;

		
	//Set stage for game, creating characters and the simple GUI implemented.
	public void Start ()
	{
		instance = this;
		//construct our 2d array based on the value set in the editor
		distances = new float[numberOfvillagers, numberOfvillagers];
		
		//reference to Vehicle script component for each flocker
		//Flocking flocker; // reference to flocker scripts
		Villager villager;
		Werewolf werewolf;
		Follow follower;

		currentNumberOfVillagers = 6;

		obstacles = GameObject.FindGameObjectsWithTag ("Obstacle");
		
		mayor = GameObject.FindGameObjectWithTag ("Mayor");

		tp = new ThreshPop(numberOfvillagers, "Assets/Resources/GA_Population");
		chroms = new byte[numberOfvillagers];
		lifeTime = new int[numberOfvillagers];

		for (int i = 0; i < numberOfvillagers; i++) {
			//Instantiate a flocker prefab, catch the reference, cast it to a GameObject
			//and add it to our list all in one line.
			villagers.Add ((GameObject)Instantiate (villagerPrefab, 
				new Vector3 (Random.Range(300 + 5 * i,600 + 5 * i), 5, Random.Range(100,600)), Quaternion.identity));
			//grab a component reference
			villager = villagers [i].GetComponent<Villager> ();
			// HOW ABOUT NO //villagers[i].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
			//set values in the Vehicle script
			villager.Index = i;

			chroms[i] = tp.CheckOut();

			villager.MayorFollowDistance = (int)Gen2Phen(chroms[i]);

			VillageFollowers.Add((GameObject)Instantiate(followerPrefab, 
				new Vector3(600 + 5 * i, 150,400), Quaternion.identity));
			
			//Create a follower for the minimap
			follower = VillageFollowers[i].GetComponent<Follow> ();
			follower.ToFollow = villagers[i];
			VillageFollowers[i].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
			villager.Follower = follower;
			
		}
		
		for (int i=0; i < numberOfWerewolves; i++)
		{
			if(i == 0)
			{
				werewolves.Add( (GameObject)Instantiate(werewolfPrefab, 
				new Vector3(491, 35, 931), Quaternion.identity));
			}
			else if(i == 1)
			{
				werewolves.Add( (GameObject)Instantiate(werewolfPrefab, 
				new Vector3(930, 35, 441), Quaternion.identity));
			}
			else if(i == 2)
			{
				werewolves.Add( (GameObject)Instantiate(werewolfPrefab, 
				new Vector3(385, 35, 50), Quaternion.identity));
			}
			else if(i == 3)
			{
				werewolves.Add( (GameObject)Instantiate(werewolfPrefab, 
				new Vector3(89, 35, 489), Quaternion.identity));	
			}
			else
			{
				werewolves.Add( (GameObject)Instantiate(werewolfPrefab, 
				new Vector3(700 + 5 * i, 5, 600), Quaternion.identity));
			}
			
			//grab a component reference
			werewolf = werewolves [i].GetComponent<Werewolf>();
			werewolves[i].GetComponent<MeshRenderer>().material.SetColor("_Color",Color.red);
			//set value in the Vehicle script
			werewolf.Index = i;
			
			WerewolfFollowers.Add((GameObject)Instantiate(followerPrefab, 
				new Vector3(600 + 5 * i, 150,400), Quaternion.identity));
			follower = WerewolfFollowers[i].GetComponent<Follow> ();
			follower.ToFollow = werewolves[i];
			WerewolfFollowers[i].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
			
			
			
		}
		
		//references to GUI texts in Game
		savedText = GameObject.FindGameObjectWithTag("Saved").GetComponent<Saved>();
		deadText = GameObject.FindGameObjectWithTag("Dead").GetComponent<Dead>();
		
	}
	
	public void Update( )
	{
		if(currentNumberOfVillagers == 0) {

			int i = 0;

			while(!tp.AllCheckedIn()) {
				int fit = Fitness(Gen2Phen(chroms[i]), i);
				tp.CheckIn(chroms[i],fit);
				i++;
			}

			tp.WritePop();



			while(currentNumberOfVillagers != 6) {

				createNewVillager(currentNumberOfVillagers);

				currentNumberOfVillagers++;
			}


		}
	}
	
	private void calcFlockDirection ()
	{
		
		flockDirection = new Vector3();
		
		// calculate the average heading of the flock
		// use transform.
		for(int i = 0; i < villagers.Count; i++)
		{	
			flockDirection = flockDirection + villagers[i].transform.forward; 
		}
		
	}
	
	public void createNewVillager(int index)
	{
		Villager villager;
		Follow follower;
		
		villagers.Add ((GameObject)Instantiate (villagerPrefab, 
				new Vector3 (371 + UnityEngine.Random.Range(0,10), 5, 365), Quaternion.identity));
			//grab a component reference
			villager = villagers[villagers.Count-1].GetComponent<Villager> ();
			villagers[villagers.Count-1].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
			//set values in the Vehicle script
			villager.Index = villagers.Count-1;
			
			Debug.Log (villager.Index);

			chroms[villager.Index] = tp.CheckOut();
			
			villager.MayorFollowDistance = (int)Gen2Phen(chroms[villager.Index]);

			VillageFollowers.Add((GameObject)Instantiate(followerPrefab, 
				new Vector3(371 + UnityEngine.Random.Range(0,10), 5, 365), Quaternion.identity));
			
			//Create a follower for the minimap
			follower = VillageFollowers[VillageFollowers.Count-1].GetComponent<Follow> ();
			follower.ToFollow = villagers[villagers.Count-1];
			VillageFollowers[VillageFollowers.Count-1].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
			villager.Follower = follower;
		
		
	}
	
	void calcDistances( )
	{
		float dist;
		for(int i = 0 ; i < numberOfvillagers; i++)
		{
			for( int j = i+1; j < numberOfvillagers; j++)
			{
				dist = Vector3.Distance(villagers[i].transform.position, villagers[j].transform.position);
				distances[i, j] = dist;
				distances[j, i] = dist;
			}
		}
	}
	
	public float getDistance(int i, int j)
	{
		return distances[i, j];
	}
		
	private void calcCentroid ()
	{
		// calculate the current centroid of the flock
		// use transform.position
		
		centroid = new Vector3();
		
		for(int i = 0; i < villagers.Count; i++)
		{
			centroid = centroid + villagers[i].transform.position;	
		}
		
		centroid = centroid / villagers.Count;
		
		centroidContainer.transform.position = new Vector3(100,100,100);
	}

	static float Gen2Phen (byte gen)
	{
		float lb = 10.0f;	// Lower bound for threshold range in game
		float ub = 50.0f;	// Upper bound
		float step = (ub - lb) / 256;	// Step size for chrom values
		return (gen * step + lb);
	}

	int Fitness(float phen, int i) {
		
		return (int) (phen * 2 + lifeTime[i]); 
	}
	
	
}