using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class GameManager : MonoBehaviour {

	public int width;
	public int height;
	public int depth;
	public GameObject squarePrefab; 
	public GameObject nextSquare; 
	public Color[] colors = new Color[8];
	public Color stoneColor;

	public int level = 3;

	public int score = 0;

	public float moveSpeed = 0.2F;

	public float scaleFactor = 0.5F;

	public GameObject scoreText;
	public GameObject levelText;

	public GameObject ptsTnxtLvl;
	public int[] levelPoints = new int[10];

	
	public Square[,] grid;

	public GameObject menu;
	public Square currentSquare;
	public Square targetSquare;

	public delegate void SquareChecked(Square square);

	public delegate void LevelUp(int level);

	// public NextLevel = new Behavi
	public Subject<int> nextLevel = new Subject<int>();

	public static LevelUp OnLevelUp; 
	

	public Square lastSquareToMerge;
	public Square mergeTarget;

	public bool mergeRoundCompleted = true;
	// Use this for initialization
	void Start () {
		// instantiate field	
		grid = new Square[width,height];
		this.levelText.GetComponent<Text>().text = "Level " + this.level.ToString();
		
		
		NextSquare.squareDropped.Subscribe(square => {
			CreateNextSquare();
			if (targetSquare != null) {
				int newVal = targetSquare.value + square.value;
				Color newColor = colors[newVal];
				targetSquare.UpdateValue(newVal, newColor);

				// clear neighbours
				List<Square> neighboursToRemove = CheckValues(targetSquare, null);
				
				if (neighboursToRemove.Count > 0) {
					neighboursToRemove.Add(targetSquare);
					RemoveSquares(neighboursToRemove);
					grid[targetSquare.x, targetSquare.y] = null;
				} else {
					OnRemoveComplete();
				}		
			}
		});

		NextSquare.targetSelected.Subscribe(square => {
			Debug.Log(square);
		});
	}

	
	// Update is called once per frame
	void Update () {
		
	}	


	void OnEnable () {
		Square.OnRemoveComplete += OnRemoveComplete;
		Square.OnMoveComplete += OnMoveComplete;
	}

	void OnDisable () {
		Square.OnRemoveComplete -= OnRemoveComplete;
		Square.OnMoveComplete -= OnMoveComplete;
	}

	void OnRemoveComplete () {
		List<int> gapColumns = FindGapColumns();
		if (gapColumns.Count > 0) {
			CheckColums(gapColumns);
		} else {
			FillField();
		}
	}

	void OnMoveComplete() {
		if (SquaresToRemove()) {
			CheckField();
		} else {
			FillField();
		}
	}

	bool SquaresToRemove() {
		bool found = false;
		foreach (Square square in grid) {
			if (square != null) {
				List<Square> foundList = CheckValues(square, null);
				if (foundList.Count > 0) {
					found = true;
					break;
				}
			}
		}
		return found;
	}

	public void CreateGrid () {
		FillField();
		// turn UI off
		menu.SetActive(false);
		CreateNextSquare();
		this.nextLevel.OnNext(this.level + 1);
	}

	void FillField() {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (grid[x,y] == null) {
					GameObject squareObj = Instantiate(squarePrefab, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
					int rand =  GenerateValue(x, y);
					Color color = colors[rand];
					
					squareObj.GetComponent<Square>().Init(color, rand, x, y, this.moveSpeed, this.scaleFactor);
					grid[x,y] = squareObj.GetComponent<Square>();
				}
			}
		}
	}

	void CreateNextSquare() {
		int rand =  Random.Range(1, level + 2);
		int[] nextValues = new int[level + 2];
		Color[] nextColors = new Color[5];
		GameObject nextObj = Instantiate(nextSquare, new Vector3(width / 2, -1, 0), Quaternion.identity) as GameObject;
		nextValues[0] = rand;
		nextColors[0] = colors[rand];
		nextObj.GetComponent<NextSquare>().Init(nextValues, nextColors, this.moveSpeed, this.scaleFactor);
	}

	bool AreNeighbours (Square squareOne, Square squareTwo) {
		Square[] neighbours = GetNeighbours(squareOne.x , squareOne.y);
		foreach (var neighbour in neighbours) {
			if (neighbour == null) 
				continue;
			int[] pos = new int[2] { neighbour.x, neighbour.y };
			if (pos[0] == squareTwo.x && pos[1] == squareTwo.y)
				return true;
		}
		return false;
	}

	List<Square> CheckValues (Square square, Square excludeSquare) {
		Square[] neighbours = GetNeighbours(square.x, square.y);
		List<Square> neighboursToDelete = new List<Square>();
		foreach (var neighbour in neighbours) {
			if (neighbour == null) 
				continue;
			if (neighbour.value == square.value && neighbour != excludeSquare && square.value <= level) {
				neighboursToDelete.Add(neighbour);
			}
		} 
		return neighboursToDelete;
	}

	void RemoveSquares (List<Square> squares) {	
		foreach (Square square in squares) {
			// remove square to be merged from grid
			bool isLast = squares[squares.Count - 1] == square;
			square.Remove(isLast);
			grid[square.x, square.y] = null;
			this.score += square.value;
		}
		if (score > this.levelPoints[this.level + 1]) {
			// OnLevelUp(this.level + 1);
			this.level += 1;
			this.levelText.GetComponent<Text>().text = "Level " + this.level.ToString();
			CheckField();
		}
		this.ptsTnxtLvl.GetComponent<Text>().text = "PtsTnxtLvl " + (this.levelPoints[this.level + 1] - this.score).ToString();
		this.scoreText.GetComponent<Text>().text = "Score " + this.score.ToString();
	}

	Square[] GetNeighbours (int x, int y) {
		
		Square[] neighbours = new Square[4];
	
		// top 
		if (y -1 >= 0) {
			neighbours[0] = grid[x, y -1];
		}
		// right
		if (x + 1 < width) {
			neighbours[1] = grid[x + 1, y];
		}
		// bottom
		if (y + 1 < height) {
			neighbours[2] = grid[x, y + 1];
		}
		// left
		if (x - 1 >= 0) {
			neighbours[3] = grid[x -1, y];
		}
		return neighbours;
	}
	
	void CheckColums (List<int> gapColumns) {
		foreach (int col in gapColumns) {
			List<Square> squareList = new List<Square>();
			int gapCounter = 0;

			bool isLast = gapColumns[gapColumns.Count - 1] == col;				

			for (int y = 0; y < height; y++) {		
				if (grid[col,y] == null) {
					gapCounter += 1;
				}	

				if (grid[col,y] != null && gapCounter > 0) {
					squareList.Add(grid[col,y]);
				}
			}	
			if (squareList.Count > 0) {	
				MoveSquares(squareList, gapCounter, isLast);
			}
		}
	}

	List<int> FindGapColumns() {
		List<int> columns = new List<int>();
		int longestGap = 0;
		for (int x = 0; x < width; x++) {
			int gapCounter = 0;
			for (int y = 1; y < height; y++) {	
				if (grid[x,y] == null) {
					gapCounter += 1;
				}
			}	
			if (longestGap > 0 && gapCounter > longestGap) {
				columns.Add(x);
				longestGap = gapCounter;
			} else if (gapCounter > 0) {
				columns.Insert(0, x);
			}
		}
		return columns;
	}

	void MoveSquares(List<Square> squares, int gapCount, bool isLastCol) {
		foreach(Square square in squares) {
			int yPos = square.y - gapCount;
			if (square.y != yPos) {
				// remove square from old grid pos
				Square last = squares[squares.Count - 1];
				bool isLast = isLastCol && (square == last);
				grid[square.x, square.y] = null;
				Vector3 targetPos = new Vector3(
											square.x,
											yPos,
											square.transform.position.z);
				square.SetPos(square.x, yPos);
				square.MoveTo(targetPos, isLast);
				// add square to new grid pos
				grid[square.x, yPos] = square;
			}
		}
	}

	void CheckField() {
		List<Square> removeSquares = new List<Square>();
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				Square checkSquare = grid[x,y];
				if (checkSquare != null) {
					List<Square> neighbours = CheckValues(checkSquare, null);
					foreach (Square nei in neighbours) {
						if (!removeSquares.Contains(nei)) {
							removeSquares.Add(nei);
						}
					}
				}
			}
		}
		if (removeSquares.Count > 0) {
			RemoveSquares(removeSquares);
		}
	}

	public int GenerateValue (int x, int y) {	
	
		List<int> validColors = new List<int>();
		// check values to exclude
		for (int i = 0; i < depth; i++) {
			Square[] neighbours = GetNeighbours(x, y);
			bool found = false;
			foreach (Square neighbour in neighbours) {
				if (neighbour && neighbour.value == i)
					found = true;
			}
			if (!found) 
				validColors.Add(i);
		}

		int rand =  Random.Range(0, validColors.Count);

		return validColors[rand];
	}
}
