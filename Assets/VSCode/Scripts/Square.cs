using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class Square : MonoBehaviour {

	public GameObject gm;
	private GameManager gameManager;
	public TextMesh textValue;
	public int value;
	public Color color;
	public int x;
	public int y;

	public bool isStoned;

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
	float moveSpeed;
	float scaleFactor;
	public Vector3 finalScale = new Vector3(5, 5, 1);



	// Use this for initialization
	void Start () {
		// targetPos = transform.position;
		this.OnMouseDownAsObservable().Subscribe(x => {
			Debug.Log(x);
		});
	}

	void OnEnable () {
		// GameManager.OnLevelUp += OnLevelUp;
		gm = GameObject.Find("GameManager");
		gameManager = gm.GetComponent<GameManager>();
		this.gameManager.nextLevel.Subscribe(lvl => {
			Debug.Log(lvl);
		});
	}

	void OnDisable () {
		GameManager.OnLevelUp -= OnLevelUp;
	}
	
	// Update is called once per frame
	void Update () {
		// spawning

		if (transform.localScale.x < finalScale.x && !removing) {
			transform.localScale += new Vector3(scaleFactor, scaleFactor, 0);
		} 

		if (moving && transform.position != targetPos) {
			transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed);
		} else if (moving && transform.position == targetPos) {
			moving = false;
			
			if (this.isLastToMove) {
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

	void OnLevelUp(int level) {
		if (value <= level) {
			this.GetComponent<SpriteRenderer>().color = this.color;
			this.isStoned = false;
		}
	}

	public void Init (Color color, int depth, int x, int y, float moveSpeed, float scaleFactor) {
		textValue.text = depth.ToString();
		value = depth;
		SetPos(x, y);
		this.color = color;
		this.GetComponent<SpriteRenderer>().color = color;
		this.scaleFactor = scaleFactor;
		this.moveSpeed = moveSpeed;
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

	public void HighLight(int addVal) {
		bool tooBig = addVal + this.value > gameManager.level;
		int newVal =  addVal + this.value;
		this.textValue.text = newVal.ToString();
		this.GetComponent<SpriteRenderer>().color = tooBig ? gameManager.stoneColor : gameManager.colors[addVal];
	}

	public void UnHighLight() {
		this.textValue.text = this.value.ToString();
		if (this.value <= gameManager.level) {
			this.GetComponent<SpriteRenderer>().color = this.color;
		}
	}
 
	public void UpdateValue(int value, Color newColor) {
		this.value = value;
		this.color = newColor;

		if (this.value > gameManager.level) {
			this.isStoned = true;
			this.GetComponent<SpriteRenderer>().color = gameManager.stoneColor;
		} else {
			this.textValue.text = this.value.ToString();
			this.GetComponent<SpriteRenderer>().color = newColor;
		}
	}
}
