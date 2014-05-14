using UnityEngine;
using System;
using System.Collections;
using System.Text;

public class Graph : MonoBehaviour {

	// attributes
	private int[,] matrix; 			// Adjacency matrix
	private Hashtable vertices; 	// Vertex array is hash table
	private int numVertices = 0;    // Max number of vertices in matrix
	private string[] wordString;
	private System.Random rand = new System.Random();

	// Use this for initialization
	void Start () {
		numVertices = 0;
		vertices = new Hashtable();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void init() {
		numVertices = 0;
		vertices = new Hashtable();
	}

	public int NumVertices
	{
		get { return numVertices; }
	}
	
	public void CreateMatrix ()		// Don't call until numVertices is set
	{

		matrix = new int[numVertices, numVertices];		// Declare 2D array
		wordString = new string[numVertices];
		wordString[0] = "BEGIN-END";
		//for (int i = 0; i < numVertices; i++)		// Not needed
		//    for (int j = 0; j < numVertices; j++)
		//        matrix[i, j] = 0;
	}
	
	// Add a vertex to the list of vertices
	public void LoadVertex(string word)
	{
		if (vertices.Contains(word))		// Word already in vertices
		{
			Word w = (Word) vertices[word];
			w.Count++;						// Increment word count
		}
		else
		{									// New word, add to vertices
			numVertices++;					// and count it as a new word
			Word w = GetComponent<Word>().init(word, numVertices - 1);
			vertices.Add(word, w);
		}
	}
	
	public void FixBeginEndCount()	// Correct overcount for BEGIN-END word
	{
		Word begEnd = (Word) vertices["BEGIN-END"];
		//Console.WriteLine("In fix count = " + begEnd.Count);
		int count = begEnd.Count;
		begEnd.Count = count - 1;
		//Console.WriteLine("In fix count = " + begEnd.Count);
	}
	
	public Word GetVertex(string word)	// Safe getter
	{
		if (vertices.Contains(word))
		{
			return (Word) vertices[word];
		}
		else
		{
			Console.WriteLine("Couldn't find " + word + ", so I returned null");
			return null;
		}
	}
	
	// Load an edge between two vertices
	// Assume a directed graph
	public void LoadEdge(string start, string end)
	{
		Word startWord = (Word) vertices[start];	// Look up the words
		Word endWord = (Word) vertices[end];
		int startOffset = startWord.Offset;			// Look up the matrix offsets
		int endOffset = endWord.Offset;
		matrix[startOffset, endOffset]++;			// Increment the count
		
		if (startOffset == 0)
			startWord.Count++;
		
		if (wordString[endOffset] == null)			// Handle the last word
		{
			wordString[endOffset] = end;
		}
	}
	
	public int NumSents()	// Return number of sentences
	{
		Word bE = (Word) vertices["BEGIN-END"];
		return bE.Count;
	}
	
	public string GenNextWord(string prevWord)		// Generate next word of gibberish
	{
		//Debug.Log (prevWord);
		Word prev = (Word) vertices[prevWord];

		int nextOffset = findNextWord(prev.Offset, prev.Count);
		return wordString[nextOffset];
	}
	
	int findNextWord(int prevOff, int prevCount)
	{
		Debug.Log (prevCount);

		int randCt = rand.Next(1, prevCount);
	
		int tot = 0;
		for (int i = 0; i < numVertices; i++)	// Walk across the matrix row
		{
			tot += matrix[prevOff, i];			// Until random stop point
			if (tot >= randCt)
				return i;						// Return where we landed
		}
		return 0;								// Shouldn't happen
	}
	
	// Dump out upper left corner of the transition matrix for debugging purposes
	public void DumpMatrix()
	{
		for (int i = 0; i < 14; i++)
		{
			Console.Write("{0,12} ", wordString[i]);
			Word w = (Word) vertices[wordString[i]];
			Console.Write(" " + w.Count + " ");
			for (int j = 0; j < 14; j++)
				Console.Write(" " + matrix[i, j]);
			Console.WriteLine("");
		}
	}
	
	// Override ToString for debugging purposes
	public override string ToString()
	{
		// List the vertices
		string text = "Vertices: \n";
		for (int i = 0; i < numVertices; i++)
		{
			if (vertices[i] != null)
			{
				text += "vertex " + i + ": " +
					vertices[i].ToString() + "\n";
			}
		}
		
		// list the adjacency matrix
		text += "\nAdjacency:\n";
		for (int i = 0; i < numVertices; i++)
		{
			for (int j = 0; j < numVertices; j++)
			{
				if (matrix[i, j] != 0)
				{
					
					text += "vertex " + i +
						" is connected to vertex " + j + "\n";
				}
			}
		}
		return text;
	}
}
