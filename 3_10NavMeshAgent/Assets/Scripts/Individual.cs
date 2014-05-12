using UnityEngine;
using System;
using System.Collections;

/* Individual class implements a single Individual ffrom a population.
	 * In addition to being the base typ for the Population array, it is
	 * used to pass Individuals around in Population and ThreshPop, but it
	 * is not used by the Main method.
	 */

public class Individual : MonoBehaviour {



	const float MUT_PROB = 0.2f;	// Mutation probability
	int fitness;
	byte chrom;		// 8-bit chromosome

	// Use this for initialization
	void Start () {
	
	}

	public void initialize (byte newChrom) {
		chrom = newChrom;
		fitness = 1;
	}

	public void initialize (byte newChrom, int fit) {
		chrom = newChrom;
		fitness = fit;
	}

	// Update is called once per frame
	void Update () {
	
	}

	public byte Chrom
	{
		get { return this.chrom; }
	}
	
	public int Fitness
	{
		get { return this.fitness; }
		set { this.fitness = value; }
	}
	
	// Mutates a random bit MUT_PROB of the time
	public void Mutate ()
	{
		if (Util.rand.NextDouble() < MUT_PROB)
		{
			BitArray chromBits = Util.Byte2BitAra(chrom); // Explode chrom
			int mutPt = Util.rand.Next(0, 8);		// 0 to 7
			bool locus = chromBits.Get(mutPt);	// Get the bit to mutate
			locus = ! locus;					// Flip it
			chromBits.Set (mutPt, locus);		// Put it back
			chrom = Util.BitAra2Byte(chromBits); // Reassemble the chrom
		}
	}
	
	// Make it easier to write an Individual
	public override string ToString()
	{
		return (chrom + " " + fitness);
	}
}
