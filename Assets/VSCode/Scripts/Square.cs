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
	public delegate void RemoveComplete();
	public delegate void MoveComplete();
	public static SquareSelected OnSquareSelected; 
	public static DraggingComplete OnDraggingComplete;
	public static RemoveComplete OnRemoveComplete;
	public static MoveComplete OnMoveComplete;


	public Square targetSquare;
	Vector3 targetPos;

	bool moving = false;
	bool removing = false;
	bool spawning = false;
	public bool isLastToRemove = false;
	public bool isLastToMove = false;
	public Vector3 finalScale = new Vector3(5, 5, 1);



	// Use this for initialization
	void Start () {
		// targetPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		// spawning
		if (transform.localScale.x < finalScale.x && !removing) {
			transform.localScale += new Vector3(0.5F, 0.5F, 0);
		} 

		if (moving && transform.position != targetPos) {
			transform.position = Vector3.MoveTowards(transform.position, targetPos, 0.2F);
		} else if (moving && transform.position == targetPos) {
			// transform.localPosition = new Vector3(this.x, this.y, 0);
			moving = false;
			
			if (this.isLastToMove) {
				// Debug.Log(transform.position.y);
				// Debug.Log(targetPos.y);
				OnMoveComplete();
				this.isLastToMove = false;
			}
		}

		if (transform.localScale.x > 0.0F && removing) {
			transform.localScale -= new Vector3(0.5F, 0.5F, 0);
		} else if (removing && transform.localScale.x <= 0.0F) {
			// dispatch destroy event
			removing = false;
			Destroy(gameObject);
			if (isLastToRemove) {
				OnRemoveComplete();	
			}
		}
			
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

	public void Remove (bool isLast) {
		isLastToRemove = isLast;
		removing = true;	
	}


	public void MoveTo(Vector3 targetPos, bool isLast) {
		if (isLast && targetPos != transform.position) {
			
		}
		this.isLastToMove = isLast;
		this.targetPos = targetPos;
		this.moving = true;	
	}

	public void HighLight(int addVal, Color addColor) {
		int newVal =  (addVal + this.value > 3) ? 4 : addVal + this.value;
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
