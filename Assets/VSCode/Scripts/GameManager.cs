using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public int width;
	public int height;
	public int depth;
	public GameObject squarePrefab; 
	public GameObject nextSquare; 
	public Color[] colors = new Color[8];

	
	public Square[,] grid;

	public GameObject menu;
	public Square currentSquare;
	public Square targetSquare;
	public delegate void SquareChecked(Square square);
	

	public Square lastSquareToMerge;
	public Square mergeTarget;

	public bool mergeRoundCompleted = true;
	// Use this for initialization
	void Start () {
		// instantiate field	
		grid = new Square[width,height];
	}

	
	// Update is called once per frame
	void Update () {
		
	}	


	void OnEnable () {
		NextSquare.OnTargetSelected += OnTargetSelected;
		NextSquare.OnSquareDropped += OnSquareDropped;
		Square.OnRemoveComplete += OnRemoveComplete;
		Square.OnMoveComplete += OnMoveComplete;
	}

	void OnDisable () {
		NextSquare.OnTargetSelected -= OnTargetSelected;
		NextSquare.OnSquareDropped -= OnSquareDropped;
		Square.OnRemoveComplete -= OnRemoveComplete;
		Square.OnMoveComplete -= OnMoveComplete;
	}


	void OnTargetSelected (Square square, Square nextSquare) {
		if (square != null && targetSquare != null && square != targetSquare) {		
			targetSquare.UnHighLight();
		}

		if (square != null && nextSquare != null) {
			targetSquare = square;
			int newVal = (nextSquare.value + square.value > 6) ? 6 : nextSquare.value + square.value;
			Color newColor = colors[newVal];
			targetSquare.HighLight(nextSquare.value, newColor);
		} else {
			if (targetSquare != null) 
				targetSquare.UnHighLight();
			targetSquare = null;
		}
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

	void OnSquareDropped (Square square) {
		CreateNextSquare();
		if (targetSquare != null) {
			int newVal = (targetSquare.value + square.value > 3) ? 4 : targetSquare.value + square.value;
			Color newColor = colors[newVal];
			targetSquare.UpdateValue(newVal, newColor);

			// clear neighbours
			List<Square> neighboursToRemove = CheckValues(targetSquare, null);
			
			RemoveSquares(neighboursToRemove);
			if (neighboursToRemove.Count > 0) {
				targetSquare.Remove(false);
				grid[targetSquare.x, targetSquare.y] = null;
			} else {
				OnRemoveComplete();
			}		
		}
	}

	public void CreateGrid () {
		FillField();
		// turn UI off
		menu.SetActive(false);
		CreateNextSquare();
	}

	void FillField() {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (grid[x,y] == null) {
					GameObject squareObj = Instantiate(squarePrefab, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
					int rand =  GenerateValue(x, y);
					Color color = colors[rand];
					
					squareObj.GetComponent<Square>().Init(color, rand, x, y);
					grid[x,y] = squareObj.GetComponent<Square>();
				}
			}
		}
	}

	void CreateNextSquare() {
		int rand =  Random.Range(1, 5);
		int[] nextValues = new int[5];
		Color[] nextColors = new Color[5];
		GameObject nextObj = Instantiate(nextSquare, new Vector3(width / 2, -1, 0), Quaternion.identity) as GameObject;
		nextValues[0] = rand;
		nextColors[0] = colors[rand];
		nextObj.GetComponent<NextSquare>().Init(nextValues, nextColors);
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
			if (neighbour.value == square.value && neighbour != excludeSquare && square.value <= 3) {
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
		}
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

			for (int y = 0; y < height; y++) {	
				bool isLast = gapColumns[gapColumns.Count - 1] == col;				
				if (height - 1 == y) {
					if (grid[col,y] != null) {
						squareList.Add(grid[col,y]);
					}
					if (gapCounter > 0) {
						MoveSquares(squareList, gapCounter, isLast);
						break;
					}
				}

				if (grid[col,y] == null &&  squareList.Count > 0) {	
					if (gapCounter > 0) {
						MoveSquares(squareList, gapCounter, isLast);
						break;
					}
				}
		
				if (grid[col,y] == null) {
					gapCounter += 1;
				}
				
				if (grid[col,y] != null && gapCounter > 0) {
					squareList.Add(grid[col,y]);
				}
			}	
		}
	}

	List<int> FindGapColumns() {
		List<int> columns = new List<int>();
		for (int x = 0; x < width; x++) {
			for (int y = 1; y < height; y++) {	
				if (grid[x,y - 1] == null && grid[x,y] != null) {
					columns.Add(x);
					break;
				}
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

	int GenerateValue (int x, int y) {	
	
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
