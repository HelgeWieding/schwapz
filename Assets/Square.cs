using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Square : MonoBehaviour {

	public GameManager gm;
	public TextMesh textValue;
	public int value;
	public Color color;
	public int x;
	public int y;

	public delegate void SquareSelected(Square square);
	public delegate void DraggingComplete(Square square);
	public delegate void MergeComplete(Square square);
	public static SquareSelected OnSquareSelected; 
	public static DraggingComplete OnDraggingComplete;
	public static MergeComplete OnMergeComplete;

	public Square targetSquare;
	Vector3 targetPos;

	bool swapping = false;
	bool merging = false;
	public bool isLastToMerge;



	// Use this for initialization
	void Start () {
		// targetPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Init (Color color, int depth, int x, int y) {
		textValue.text = depth.ToString();
		value = depth;
		SetPos(x, y);
		this.color = color;
		this.GetComponent<SpriteRenderer>().color = color;
	}

	public void SetPos (int x, int y) {
		this.x = x;
		this.y = y;
	}


	public void SwapTo(Vector3 targetPos) {
		this.targetPos = targetPos;
		this.swapping = true;
		transform.localScale = new Vector3(5, 5, 5);
	}

	public void MergeTo(Square targetSquare, bool isLast) {
		this.isLastToMerge = isLast;
		this.targetPos = targetSquare.transform.position;
		this.targetSquare = targetSquare;
		this.merging = true;
	}

	public void HighLight(int addVal, Color addColor) {
		int newVal =  addVal + this.value;
		textValue.text = newVal.ToString();
		this.GetComponent<SpriteRenderer>().color = addColor;
	}

	public void UnHighLight() {
		textValue.text = this.value.ToString();
		this.GetComponent<SpriteRenderer>().color = this.color;
	}
 
	public void UpdateValue(int value, Color newColor) {
		this.value = value;
		textValue.text = value.ToString();
		this.color = newColor;
		this.GetComponent<SpriteRenderer>().color = newColor;
	}


}
