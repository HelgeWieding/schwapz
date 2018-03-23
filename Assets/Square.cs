using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Square : MonoBehaviour {

	public GameManager gm;
	public TextMesh textValue;
	public int value;
	public int x;
	public int y;

	public delegate void SquareSelected(Square square);
	public delegate void SwappingComplete(Square square);
	public delegate void MergeComplete(Square square);
	public static SquareSelected OnSquareSelected; 
	public static SwappingComplete OnSwappingComplete;
	public static MergeComplete OnMergeComplete;

	public Square targetSquare;
	Vector3 targetPos;

	bool swapping = false;
	bool merging = false;
	public bool isLastToMerge;



	// Use this for initialization
	void Start () {
		targetPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (targetPos != transform.position) {
			transform.position = Vector3.MoveTowards(transform.position, targetPos, 3 * Time.deltaTime);
		}

		if (targetPos == transform.position && this.swapping) {
			this.swapping = false;
			OnSwappingComplete(this);
		}

		if (targetPos == transform.position && this.merging) {
			this.merging = false;
			if (isLastToMerge) {
				OnMergeComplete(this.targetSquare);
			}
			Destroy(gameObject);
		}
	}

	public void Init (Color color, int depth, int x, int y) {
		textValue.text = depth.ToString();
		value = depth;
		SetPos(x, y);
		this.GetComponent<SpriteRenderer>().color = color;
	}

	public void SetPos (int x, int y) {
		this.x = x;
		this.y = y;
	}

	void OnMouseDown() {
		// trigger event to gm
		if (OnSquareSelected != null) {
			OnSquareSelected(this);
		}
    }

	void OnMouseEnter() {
		if (!merging && !swapping) 
			transform.localScale += new Vector3(0.5F, 0.5F, 0);
    }

	void OnMouseExit() {
		if (!merging && !swapping) 
			transform.localScale += new Vector3(-0.5F, -0.5F, 0);
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

	public void UpdateValue(int value, Color newColor) {
		this.value += 1;
		textValue.text = value.ToString();
		this.GetComponent<SpriteRenderer>().color = newColor;
	}


}
