using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class Markov : MonoBehaviour {


	Graph biGrams;		// Graph attribute holds the Markov Chain

	// Use this for initialization
	void Start () {

		biGrams = new Graph();	// Declare the Graph but don't initialize it yet
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/* CreateGraph method
		 * Makes two passes through the text file whose path is passed in.
		 * First pass creates the Vertex array in the graph and counts number of different
		 * words in the text file.
		 * Second pass fills the transition matrix with counts of occurrances of row word
		 * followed by column word in the corpus.
		 * A special non-word "BEGIN-END" represents sentence boundaries,
		 * i.e., the first word in a sentence follows BEGIN-END, and the last word in a
		 * sentence is followed by BEGIN-END.
		 */
	public void CreateGraph(string filePath)
	{
		StreamReader input = null;
		try
		{
			// Get ready to read the corpus data file
			input = new StreamReader(filePath);
			
			// Read until out of data (first pass through the corpus)
			string line = "";
			char[] delimiters = { ' ' };
			
			biGrams.LoadVertex("BEGIN-END");	// Load up the sentence boundary token
			
			while ((line = input.ReadLine()) != null)	// Read file line by line
			{
				// Convert the letters to all lower case
				line = line.ToLower();
				
				// Handle punctuation by replacing it with whitespace
				line = line.Replace('.', ' ');
				line = line.Replace(',', ' ');
				line = line.Replace(';', ' ');
				line = line.Replace(':', ' ');
				line = line.Replace('?', ' ');
				line = line.Replace('!', ' ');
				line = line.Replace('"', ' ');
				line = line.Replace('(', ' ');
				line = line.Replace(')', ' ');
				line = line.Replace("--", " ");
				//line = line.Replace('-', ' ');	// Leave - in for hyphenated words
				
				// Split the line of text up
				string[] myWords = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				
				// Add words in the line to the vertex array (hash table)
				for (int i = 0; i < myWords.Length; i++)
				{
					biGrams.LoadVertex(myWords[i]);
				}
			}
			
			biGrams.FixBeginEndCount();
			
			Console.Write("Loaded " + biGrams.NumVertices + " words");
		}
		catch (Exception ex)
		{
			Console.WriteLine("Exception: " + ex.Message);
		}
		finally
		{
			if (input != null)
			{
				input.Close();
			}
		}
		
		biGrams.CreateMatrix();  // Now know how many vertices, so create transition matrix
		
		// Now ready for second pass to fill the matrix with bi-gram counts
		
		try
		{
			// Read in the data file again (second pass)
			input = new StreamReader(filePath);
			
			// Read until out of data
			string line = "";
			char[] delimiters = { ' ' };
			string oldWord = "BEGIN-END";
			string newWord = null;
			
			while ((line = input.ReadLine()) != null)
			{
				// Convert the string to all lower case
				line = line.ToLower();
				
				// Handle punctuation differently in this pass
				line = line.Replace(',', ' ');	// Remove , ; : "
				line = line.Replace(';', ' ');
				line = line.Replace(':', ' ');
				line = line.Replace('"', ' ');
				line = line.Replace(".", "! ");	// Use '!' as universal end of sentence char
				line = line.Replace("?", "! ");	// Map . and ? to !      ! stays put
				line = line.Replace("--", " ");	// Remove --
				line = line.Replace("(", "");	// Eat paraentheses (not white space)
				line = line.Replace(")", "");
				//line = line.Replace('-', ' ');	// Leave - in for hyphenated words
				
				// Split the line of text up into words
				string[] myWords = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				
				// Add bi-gram pairs to the transition matrix to count them
				for (int i = 0; i < myWords.Length; i++)
				{
					Boolean isLastWord = false;
					newWord = myWords[i];
					if (LastWordInSent (newWord))	// Handle special end of sentence "word"
					{
						isLastWord = true;			// Map ! to setting Boolean true
						newWord = newWord.Replace("!", "");	// Then remove !
					}
					biGrams.LoadEdge (oldWord, newWord);	// Add/update graph edge
					oldWord = newWord;						// Update old word
					if (isLastWord)
					{
						newWord = "BEGIN-END";				// Handle end of sentence edge
						biGrams.LoadEdge(oldWord, newWord);	// using the special non-word
						oldWord = newWord;
					}
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Exception: " + ex.Message);
		}
		finally
		{
			if (input != null)
			{
				input.Close();
			}
		}
		Console.WriteLine(" and " + biGrams.NumSents() + " sentences.");
		//biGrams.DumpMatrix();		// Only upper left of matrix for debugging purposes
	}
	
	bool LastWordInSent(string word)	// Returns true if "word" is end of sentence (!)
	{
		return word.Contains("!");
	}
	
	public string [] GenGibSent(int maxSent)
	{
		string[] gibSent = new string[maxSent];		// Maximum maxSent-word sentence
		string prevWord = "BEGIN-END";				// Start with sentence boundary
		for (int i = 0; i < gibSent.Length; i++)
		{
			string nextWord = biGrams.GenNextWord(prevWord);	// Get next word
			if (nextWord.Equals("BEGIN-END"))					// If end of sentence
				return gibSent;									// Return words so far
			else
			{
				gibSent[i] = nextWord;				// Store the word
				prevWord = nextWord;				// Update previous word
			}
		}
		return gibSent;
	}

}
