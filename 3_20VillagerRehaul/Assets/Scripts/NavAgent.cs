using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class NavAgent : MonoBehaviour {

	NavMeshAgent agent;

	// Use this for initialization
	void Start () {

		agent = GetComponent<NavMeshAgent>();
	
	}
	
	// Update is called once per frame
	void Update (Vector3 target) {
		// Basically just set this to a Vec3
        if(target == null) return; 
        else
        {
		    agent.SetDestination(targetPoint);
        }
	}
}
