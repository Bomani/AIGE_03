using UnityEngine;
using System;
using System.Collections;

/* A couple of cumbersome methods to map a byte to and from
	 * a BitArray so that bit level operations for crossover and
	 * mutation can be done straightforwardly.
	 * Also put the Random object here because only need one.
	 */

public class Util {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// Explode a byte chromosome into a bit array
	public static BitArray Byte2BitAra (byte b)
	{
		byte[] bAra = new byte[1];	// Have to have a byte array(!)
		bAra [0] = b;
		return new BitArray (bAra);
	}
	
	// Reassemble a bit array into a byte chromosome
	public static byte BitAra2Byte (BitArray bA)
	{
		byte[] bAra = new byte[1];
		bA.CopyTo (bAra, 0);
		return bAra [0];
	}
	
	// Set up the Random generator here
	public static System.Random rand = new System.Random();
}
