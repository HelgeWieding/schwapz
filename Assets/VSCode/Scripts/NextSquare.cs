using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NextSquare : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
	
	public Square[] squares = new Square[3];
	public GameObject squarePrefab;
	public GameObject squareObj;

	public delegate void TargetSelected(Square square, Square nextSquare);
	public delegate void SquareDropped(Square square);
	public static TargetSelected OnTargetSelected; 
	public static SquareDropped OnSquareDropped;
	public Square target;
	public Vector3 startPos = new Vector3(2, -1, 1); 
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Init (int[] values, Color[] colors) {
		squareObj = Instantiate(squarePrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
		Square square = squareObj.GetComponent<Square>();
		square.Init(colors[0], values[0], 0, 0);
		this.squares[0] = square;
		squareObj.transform.SetParent(transform, false);
	}

	public void OnBeginDrag (PointerEventData data) {
		
	}

	public void OnDrag (PointerEventData data) {
		//Create a ray going from the camera through the mouse position
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		RaycastHit2D hit = Physics2D.Raycast(mousePos, -Vector2.up);
		if (hit.collider != null) {
			target = hit.collider.GetComponent<Square>();
			if (target != this.squares[0]) {
				OnTargetSelected(target, this.squares[0]);
			} else {
				target = null;
				OnTargetSelected(null, null);
			}
		}
                    
		//Calculate the distance between the Camera and the GameObject, and go this distance along the ray
		Vector3 rayPoint = ray.GetPoint(Vector3.Distance(transform.position, Camera.main.transform.position));
		//Move the GameObject when you drag it
		transform.position = rayPoint;
	}

	public void OnEndDrag (PointerEventData data) {
		if (target == null) {
			transform.position = new Vector3(2, -1, 1);
			return;
		}
		OnSquareDropped(this.squares[0]);
		Destroy(squareObj);
	}
}
