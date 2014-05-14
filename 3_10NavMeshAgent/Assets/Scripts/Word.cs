using UnityEngine;
using System.Collections;

public class Word : MonoBehaviour {

	private string wordSt;		// The word as a string
	private int count = 1;		// The count of this word in corpus
	private int offset;			// Offset of the word in the graph arrays

	// Use this for initialization
	void Start () {
	
	}

	public Word(string st, int off)
	{
		wordSt = st;
		offset = off;
		count = 1;
	
	}

	public Word init(string st, int off) {
		wordSt = st;
		offset = off;
		count = 1;

		return this;
		
		
	}
	// Update is called once per frame
	void Update () {
	
	}

	public string WordSt
	{
		get { return wordSt; }
		set { wordSt = value; }
	}
	
	public int Offset
	{
		get { return offset; }
		set { offset = value; }
	}
	
	public int Count
	{
		get { return count; }
		set { count = value; }
	}
	
	// override ToString
	public override string ToString()
	{
		return wordSt + " - Count: " + count + " Offset: " + offset;
	}
}
