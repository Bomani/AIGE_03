using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class Population {

	/* Population class implements a single population. Used by ThreshPop for both
	 * oldP and newP. 
	 */

	const double CROSSOVER_PROB = 0.9;	// 90% chance of crossover in BreedDude()
	int popSize;			// Population size
	Individual [] dudes;
	int nDudes = 0;			// Current number of Individuals
	char[] delim = {' '};	// Used in ReadPop to split input lines

	// Use this for initialization
	void Start () {
	
	}

	public void initialize (int popN) {
		popSize = popN;
		dudes = new Individual[popSize];
		nDudes = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// Returns true if population is full
	public bool Full
	{
		get { return nDudes == popSize; }
	}
	
	// Fills population with new random chromosomes for generation 0
	public void InitPop()
	{
		for (int i = 0; i < popSize; i++)
		{
			dudes [i] = new Individual ();
			dudes[i].initialize((byte) Util.rand.Next (256));
		}
		nDudes = popSize;
	}
	
	// Fills population by reading individuals from a file
	// Assumes file is correctly formatted with correct number of lines
	public void ReadPop(StreamReader inStream)
	{
		for (int i = 0; i < popSize; i++)
		{
			string line = inStream.ReadLine();		// Read a line
			string [] tokens = line.Split (delim);	// Split into "words"
			byte chr = Byte.Parse(tokens[0]);		// Convert words to numbers
			int fit = int.Parse(tokens[1]);
			dudes [i] = new Individual ();	// Put Individual in population
			dudes[i].initialize(chr,fit);
		}
		nDudes = popSize;							// Show the population full
	}
	
	// Write the population out to a data file that can be read by ReadPop
	public void WritePop(StreamWriter outStream)
	{
		for (int i = 0; i < nDudes; i++)
		{
			outStream.WriteLine (dudes [i]);
		}
	}
	
	// Display the Population on the Console
	public void DisplayPop()
	{
		for (int i = 0; i < nDudes; i++)
		{
			Console.WriteLine (dudes [i]);
		}
		Console.WriteLine ();
	}
	
	// Breed a new Individual using crossover and mutation
	public Individual BreedDude()
	{
		Individual p1 = Select ();	// Get 2 parents
		Individual p2 = Select ();
		byte c1 = p1.Chrom;			// Extract their chromosomes
		byte c2 = p2.Chrom;
		
		if (Util.rand.NextDouble () < CROSSOVER_PROB)	// Probably do crossover
		{
			byte kidChrom = CrossOver (c1, c2);		// Make new chromosome
			Individual newDude = new Individual ();	// Make Individual
			newDude.initialize(kidChrom);
			newDude.Mutate ();						// Maybe mutate a bit
			return newDude;							// Send it back
		}
		else
			// No crossover => Pick one of the parents to return unchanged
			return (Util.rand.NextDouble() < 0.5 ? p1 : p2);
	}
	
	// Roulette-wheel selection selects in linear proportion to fitness
	public Individual Select()
	{
		// Get total of all fitness values
		int totFit = 0;
		for (int i = 0; i < nDudes; i++)
			totFit += dudes[i].Fitness;
		
		// Roll a random integer from 0 to totFit - 1
		int roll = Util.rand.Next (totFit);
		
		// Walk through the population accumulating fitness
		int accum = dudes[0].Fitness;	// Initialize to the first one
		int iSel = 0;
		// until the accumulator passes the rolled value
		while (accum <= roll && iSel < nDudes-1)
		{
			iSel++;
			accum += dudes[iSel].Fitness;
		}
		// Return the Individual where we stopped
		return dudes[iSel];
	}
	
	// Find the best (highest fitness) Individual in the population
	// Used to implement elitism => best of old Pop survives in new
	public Individual BestDude ()
	{
		// Initialize to the first Individual in the array
		int whereBest = 0;			// Initialze to the first one
		int bestFit = dudes[0].Fitness;
		
		// Walk through the rest to get the overall best one
		for (int i = 1; i < nDudes; i++)
			if (dudes [i].Fitness > bestFit)
		{
			whereBest = i;
			bestFit = dudes [i].Fitness;
		}
		return dudes[whereBest];
	}
	
	// Add a new Individual to the population in the next open spot
	public int AddNewInd (Individual newDude)
	{
		int wherePut = -1;		// -1 in case something breaks
		if (Full)
			Console.WriteLine ("Panic!  Tried to add too many dudes");
		else
		{
			wherePut = nDudes;
			dudes[wherePut] = newDude;
			nDudes++;		// Increment for next time
		}
		return wherePut;	// Return offset in array where it landed
	}
	
	// Get Individual at offset where in the array
	public Individual GetDude (int where)
	{
		return dudes [where];
	}
	
	// Set fitness of Individual at offset where to fitVal
	public void SetFitness (int where, int fitVal)
	{
		dudes[where].Fitness = fitVal;
	}
	
	// Single-point crossover of two parents, returns new kid
	byte CrossOver(byte p1, byte p2)
	{
		BitArray p1Bits = Util.Byte2BitAra(p1);	// Explode parent chroms
		BitArray p2Bits = Util.Byte2BitAra(p2);
		
		BitArray newBits = new BitArray (8);	// Create kid
		int xOverPt = Util.rand.Next (0, 7);			// Pick random crossover point
		
		// Copy from p1 up to the crossover point, from p2 thereafter
		for (int i = 0; i < 8; i++)
		{
			if (i < xOverPt)
				newBits.Set (i, p1Bits.Get (i));
			else
				newBits.Set (i, p2Bits.Get (i));
		}
		byte newByte = Util.BitAra2Byte(newBits);	// Assemble kid into byte
		return newByte;								// Return new kid
	}
}
