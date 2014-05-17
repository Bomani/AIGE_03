using UnityEngine;
using System;
using System.Collections;
using System.Text;

/// <summary>
/// This script is attached to the player and it writes the 
/// name of players above them.
/// </summary>

public class PlayerLabel : MonoBehaviour {

	public Camera myCamera;
	private Transform myTransform;	
	private Transform triggerTransform;
	private Markov markov;
	private System.Random rand;
	private bool talking = false;
	
	//These are used in determining whether the label should be drawn
	//and where on the screen.
	
	private Vector3 worldPosition  = new Vector3();
	private Vector3 screenPosition  = new Vector3();
	private Vector3 cameraRelativePosition = new Vector3();
	private float minimumZ = 1.5f;
	
	private Villager villager;
	
	//Used in displaying the player's name.
	public string objectName = "No Name";
	private string playerName;  
	public string PlayerName { 
		get { return playerName;}
		set {playerName = value; }
	}

	private GUIStyle myStyle = new GUIStyle();
	private GUIStyle theirStyle = new GUIStyle();
	
	private int labelTop = 20;
	private int labelWidth = 40;
	private int labelHeight = 15;
	private int adjustment = 3;
	
	//Variables End_____________________________________
	
	
	void Awake ()
	{
		playerName = objectName;
		playerName = " ";
		myTransform = transform;
		myCamera = Camera.main;
		rand = new System.Random();
		villager = GetComponent<Villager>();

		if(networkView.isMine)
		{		
			myStyle.normal.textColor = Color.black;	
			myStyle.fontSize = 14;
			myStyle.fontStyle = FontStyle.Normal;
			//Allow the text to extend beyond the width of the label	
			myStyle.clipping = TextClipping.Overflow;
			
		}else 
		{
			
			theirStyle.normal.textColor = Color.white;	
			theirStyle.fontSize = 12;
			theirStyle.fontStyle = FontStyle.Bold;
			theirStyle.clipping = TextClipping.Overflow;	
		}

		markov = GetComponent<Markov>();
	}
	
	
	// Update is called once per frame
	void Update () 
	{	
		//Capture whether the player is in front or behind the camera.
		cameraRelativePosition = myCamera.transform.InverseTransformPoint(myTransform.position);
	}
	
	
	void OnGUI ()
	{
		//Only display the player's name if they are in front of the camera and also the 
		//player should be in front of the camera by at least minimumZ.
		
		if(cameraRelativePosition.z > minimumZ)
		{
			//Set the world position to be just a bit above the player.
			
			worldPosition = new Vector3(myTransform.position.x, myTransform.position.y + adjustment ,
			                            myTransform.position.z);
			
			//Convert the world position to a point on the screen.
			
			screenPosition = myCamera.WorldToScreenPoint(worldPosition);
			
			GUIStyle style;
			if(networkView.isMine) style = myStyle;
			else style = theirStyle;
			
			GUI.Label(new Rect(screenPosition.x - labelWidth / 2,
			                   Screen.height - screenPosition.y - labelTop,
			                   labelWidth, labelHeight), playerName, style);


			
		}
	}

	public void newSentence() {

		if(talking == false) {
			playerName = sentStringArr(markov.GenGibSent(rand.Next(60)));
			Invoke("spacer", 5 + (playerName.Length/20));
			talking = true;
		}

	}

	public void resetSentence() {

		talking = false;

	}

	private void spacer() {

		playerName = " ";
		Invoke("resetSentence", 3);

	}
	string sentStringArr (string[] sentWords) {

		string returned = "";

		if(sentWords[0] != null) {
			string firstChar = sentWords[0].Substring(0, 1).ToUpper();	// Cap first word
			string capitalized = firstChar + sentWords[0].Substring(1, sentWords[0].Length-1);
			sentWords[0] = capitalized;
		}

		returned = returned + sentWords[0];

		for (int i = 1; i < sentWords.Length; i++)	// Print rest of words
		{
			returned = returned + " ";
			if (sentWords[i] == null)
			{
				returned = returned + ".";
				return returned;
			}
			else
				returned = returned + sentWords[i];
		}

		returned = returned + ".";					// Put period at the end
		return returned;
	}

}

