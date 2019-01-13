using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;

public class NextSquare : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
	
	public Square[] squares = new Square[3];
	public GameObject squarePrefab;
	public GameObject squareObj;

	public delegate void TargetSelected(Square square, Square nextSquare);
	public delegate void SquareDropped(Square square);
	public static TargetSelected OnTargetSelected; 
	public static SquareDropped OnSquareDropped;
	public static Subject<Square> squareDropped = new Subject<Square>();
	public static Subject<Square> targetSelected = new Subject<Square>();
	public Square target;
	private Square _newTarget;
	public Vector3 startPos = new Vector3(2, -1, 1); 
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Init (int[] values, Color[] colors, float moveSpeed, float scaleFactor) {
		squareObj = Instantiate(squarePrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
		Square square = squareObj.GetComponent<Square>();
		square.Init(colors[0], values[0], 0, 0, moveSpeed, scaleFactor);
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
			_newTarget = hit.collider.GetComponent<Square>();
			if (_newTarget == this.squares[0]) {
				if (target != null) {
					target.UnHighLight();
				}
				target = null;
			}
			if (_newTarget != target) {
				if (target != null) {
					target.UnHighLight();
				}
				target = _newTarget;
			}
			if (target != null && target != this.squares[0]) {
				target.HighLight(this.squares[0].value);
			} 
		}
                    
		//Calculate the distance between the Camera and the GameObject, and go this distance along the ray
		Vector3 rayPoint = ray.GetPoint(Vector3.Distance(transform.position, Camera.main.transform.position));
		//Move the GameObject when you drag it
		transform.position = rayPoint;
	}

	public void OnEndDrag (PointerEventData data) {
		if (target.isStoned || target == this.squares[0]) {
			target.UnHighLight();
			target = null;
			transform.position = new Vector3(2, -1, 1);
			return;
		}
		squareDropped.OnNext(this.squares[0]);
		Destroy(squareObj);
		Destroy(this.gameObject);
	}
}
