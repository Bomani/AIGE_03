using UnityEngine;
using System;
using System.Collections;
using System.IO;

/* ThreshPop - Highest level class for the threshold GA package
	 * Implements a single generation of evolution with an old population from
	 * a previous generation (or the initial generation 0), and a new population
	 * created for this generation. Provides all the methods called directly to
	 * implement a threshold population from a calling class.
	 * Uses the Population class to implement its two internal populations
	 * (oldP and newP). Uses the Individual class as well for handling single
	 * Individuals. 
	 */

public class ThreshPop : MonoBehaviour {

	int popSize;		// Number of Individuals in population
	Population oldP;	// Old population read from file or generated randomly
	Population newP;	// New population filled  as Individuals get fitness
	string popPath;		// String for data file path name (in Bin/Debug folder)
	int nextCOut = 0;	// Counter for number of Individuals checked out of oldP
	int nextCIn = 0;	// Counter for number of Individuals checked into of newP
	bool isGeneration0 = false;	// Assume there's a data file from a previous run

	// Use this for initialization
	void Start () {
	
	}

	void Initialize (int size, string path) {

		popSize = size;
		popPath = path;
		oldP = new Population (); // Old population for check out
		oldP.initialize(popSize);
		FillPop();
		newP = new Population ();	// New population for check in
		newP.initialize(popSize);
	}


	// Update is called once per frame
	void Update () {
	
	}

	// Fill oldP either from data file or from scratch (new, random)
	void FillPop ()
	{
		StreamReader inStream = null;	// Open file if it's there
		try
		{
			inStream = new StreamReader(popPath);
			oldP.ReadPop(inStream);		// File opened so read it
			inStream.Close();
		}
		catch (FileNotFoundException ex)
		{
			oldP.InitPop();			// File didn't open so fill with newbies
			isGeneration0 = true;	// Set flag to show it's generation 0
		}
	}
	
	public void WritePop()
	{
		StreamWriter outStream = new StreamWriter(popPath, false);
		newP.WritePop(outStream);
		outStream.Close();
	}
	
	// Display either oldP (0) or newP (1) on Console window
	public void DisplayPop(int which)
	{
		if (which == 0)
			oldP.DisplayPop();
		else
			newP.DisplayPop();
	}
	
	// Check out an individual to use for a threshold in an NPC
	public byte CheckOut ()
	{
		if (isGeneration0)	// Brand new => don't breed
		{
			Individual dude = oldP.GetDude(nextCOut);
			nextCOut++;
			return dude.Chrom;
		}
		else
		{	// Came from file so breed new one
			Individual newDude;
			if (nextCOut == 0)	// First one needs to be Best (elitism)
				newDude = oldP.BestDude();
			else
				newDude = oldP.BreedDude();	// Rest are bred
			nextCOut++;						// Count it
			return newDude.Chrom;			// Return its chromosome
		}
	}
	
	// Returns true if we've checked out a population's worth
	public bool AllCheckedOut()
	{
		return nextCOut == popSize;
	}
	
	// Check in an individual that has now acquired a fitness value
	public void CheckIn (byte chr, int fit)
	{
		Individual NewDude = new Individual();	// Make Individual
		NewDude.initialize(chr,fit);
		newP.AddNewInd(NewDude);						// Add to newP
		nextCIn++;										// Count it
	}
	
	// Returns true if newP is full of checked in Individuals
	public bool AllCheckedIn()
	{
		return nextCIn == popSize;
	}
}
