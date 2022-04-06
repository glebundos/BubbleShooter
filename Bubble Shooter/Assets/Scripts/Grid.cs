using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

	public int ROWS = 8;

	public int COLUMNS = 6;

	public float TILE_SIZE = 0.68f;

	public float changeTypeRate = 0.5f;

	public int lines = 5;

	public GameObject gridBallGO;

	[HideInInspector]
	public float GRID_OFFSET_X = 0;

	[HideInInspector]
	public  float GRID_OFFSET_Y = 0;

	[HideInInspector]
	public List<List<Ball>> gridBalls;

	private List<Ball> matchList;

	private List<Ball.BALL_TYPE> typePool;

	private Ball.BALL_TYPE lastType;

	public static int levelType = 0;

    public GameController gameController;
    void Start () {

        matchList = new List<Ball> ();
		lastType = (Ball.BALL_TYPE)Random.Range (0, 5);
		typePool = new List<Ball.BALL_TYPE> ();

        GridGenerator(levelType, ref typePool);
		BuildGrid ();
	}

	public void AddLine () {
		//does top line have visible bubbles
		var emptyFirstRow = true;
		foreach (var b in gridBalls[0]) {
			if (b.gameObject.activeSelf) {
				emptyFirstRow = false;
				break;
			}
		}

		if (!emptyFirstRow) {
			var r = ROWS - 2;
			while (r >= 0) {
				foreach (var b in gridBalls[r]) {
					if (b.gameObject.activeSelf) {
						gridBalls [r + 1] [b.column].gameObject.SetActive (true);
						gridBalls [r + 1] [b.column].SetType (b.type);
					} else {
						gridBalls [r + 1] [b.column].gameObject.SetActive (false);
					}
				}
				r--;
			}
		}

		foreach (var b in gridBalls[0]) {
			b.SetType (typePool [0]);
			typePool.RemoveAt (0);
			if (b.type == Ball.BALL_TYPE.NONE)
			{
				b.gameObject.SetActive(false);
			}
			else
            {
				b.gameObject.SetActive(true);
			}
		}
	}

	void BuildGrid ()
	{
		gridBalls = new List<List<Ball>> ();

		GRID_OFFSET_X = (COLUMNS * TILE_SIZE) * 0.5f;
		GRID_OFFSET_Y = (ROWS * TILE_SIZE) * 0.5f;

		GRID_OFFSET_X -= TILE_SIZE * 0.5f;
		GRID_OFFSET_Y -= TILE_SIZE * 0.5f;


		for (int row = 0; row < ROWS; row++) {

			var rowBalls = new List<Ball>();

			for (int column = 0; column < COLUMNS; column++) {

				var item = Instantiate (gridBallGO) as GameObject;
				var ball = item.GetComponent<Ball>();

				ball.SetBallPosition(this, column, row);
				ball.SetType (typePool[0]);
                if (ball.type == Ball.BALL_TYPE.NONE)
                {
                    ball.gameObject.SetActive(false);
                }
                
                typePool.RemoveAt (0);

				ball.transform.parent = gameObject.transform;
				rowBalls.Add (ball);

				if (gridBalls.Count > lines) {
					ball.gameObject.SetActive (false);
				}
			}
				
			gridBalls.Add(rowBalls);
		}
	}

	public void AddBall (Ball collisionBall, Bullet bullet) {

		var neighbors = BallEmptyNeighbors(collisionBall);
        var minDistance = 10000.0f;
		Ball minBall = null;
		foreach (var n in neighbors) {
			var d = Vector2.Distance (n.transform.position, bullet.transform.position);
			if ( d < minDistance ) {
				minDistance = d;
				minBall = n;
			}
		}
		bullet.gameObject.SetActive(false);
		minBall.SetType(bullet.type);
		minBall.gameObject.SetActive(true);

		CheckMatchesForBall (minBall);
	}


	public void CheckMatchesForBall (Ball ball) {

		matchList.Clear ();

		for (int row = 0; row < ROWS; row++) {
			for (int column = 0; column < COLUMNS; column++) {
				gridBalls [row] [column].visited = false;
			}
		}

		//search for matches around ball
		var initialResult = GetMatches(ball);
		matchList.AddRange (initialResult);

		while (true) {
			
			var allVisited = true;
			for (var i = matchList.Count - 1; i >= 0 ; i--) {
				var b = matchList [i];
				if (!b.visited) {
					AddMatches (GetMatches (b));
					allVisited = false;
				}
			}

			if (allVisited) {
				if (matchList.Count > 2) {
					
					foreach (var b in matchList) {
						b.gameObject.SetActive (false);
					}

					CheckForDisconnected ();
					int scoreBallsCount = matchList.Count;
					//remove disconnected balls
					var i = 0;
					while (i < ROWS) {
						foreach (var b in gridBalls[i]) {
							if (!b.connected && b.gameObject.activeSelf) {
								b.gameObject.SetActive (false);
								scoreBallsCount++;
							}
						}
						i++;
					}

					gameController.AddScore(scoreBallsCount);
				}
				return;
			}
		}
	}


	void CheckForDisconnected () {
		//set all balls as disconnected
		foreach (var r in gridBalls) {
			foreach (var b in r) {
				b.connected = false;
			}
		}
		//connect visible balls in first row 
		foreach (var b in gridBalls[0]) {
			if (b.gameObject.activeSelf)
				b.connected = true;
		}

		//now set connect property on the rest of the balls
		var i = 1;
		while (i < ROWS) {
			foreach (var b in gridBalls[i]) {
				if (b.gameObject.activeSelf) {
					var neighbors = BallActiveNeighbors (b);
					var connected = false;

					foreach (var n in neighbors) {
						if (n.connected) {
							connected = true;
							break;
						}
					}

					if (connected) {
						b.connected = true;
						foreach (var n in neighbors) {
							if (n.gameObject.activeSelf) {
								n.connected = true;
							}
						}
					} 
				}
			}
			i++;
		}
	}


	List<Ball> GetMatches (Ball ball) {
		ball.visited = true;
		var result = new List<Ball> () { ball };
		var n = BallActiveNeighbors (ball);

		foreach (var b in n) {
			if (b.type == ball.type) {
				result.Add (b);
			}
		}

		return result;
	}

	void AddMatches (List<Ball> matches) {
		foreach (var b in matches) {
			if (!matchList.Contains (b))
				matchList.Add (b);
		}
	}



	Ball.BALL_TYPE GetBallTypeRandom () {
		var random = Random.Range (0.0f, 1.0f);
		if (random > changeTypeRate) {
			lastType = (Ball.BALL_TYPE)Random.Range (0, 5);
		}
		return lastType;
	}



	List<Ball> BallEmptyNeighbors (Ball ball) {
		var result = new List<Ball> ();
		if (ball.column + 1 < COLUMNS) {
			if (!gridBalls [ball.row] [ball.column + 1].gameObject.activeSelf)
				result.Add (gridBalls [ball.row] [ball.column + 1]);
		}

		//left
		if (ball.column - 1 >= 0) {
			if (!gridBalls [ball.row] [ball.column - 1].gameObject.activeSelf)
				result.Add (gridBalls [ball.row] [ball.column - 1]);
		}
		//top
		if (ball.row - 1 >= 0) {
			if (!gridBalls [ball.row - 1] [ball.column].gameObject.activeSelf)
				result.Add (gridBalls [ball.row - 1] [ball.column]);
		}

		//bottom
		if (ball.row + 1 < ROWS) 
		{
			if (!gridBalls [ball.row + 1] [ball.column].gameObject.activeSelf)
				result.Add (gridBalls [ball.row + 1] [ball.column]);
		}

		if (ball.column % 2 == 0) {
			//bottom-left
			if (ball.row + 1 < ROWS && ball.column - 1 >= 0) 
			{
				if (!gridBalls [ball.row + 1] [ball.column - 1].gameObject.activeSelf)
					result.Add (gridBalls [ball.row + 1] [ball.column - 1]);
			}

			//bottom-right
			if (ball.row + 1 < ROWS && ball.column + 1 < COLUMNS) 
			{
				if (!gridBalls [ball.row + 1] [ball.column + 1].gameObject.activeSelf)
					result.Add (gridBalls [ball.row + 1] [ball.column + 1]);
			}
		} 
		else 
		{
			//top-left
			if (ball.row - 1 >= 0 && ball.column - 1 >= 0) {
				if (!gridBalls [ball.row - 1] [ball.column - 1].gameObject.activeSelf)
					result.Add (gridBalls [ball.row - 1] [ball.column - 1]);
			}

			//top-right
			if (ball.row - 1 >= 0 && ball.column + 1 < COLUMNS) {
				if (!gridBalls [ball.row - 1] [ball.column + 1].gameObject.activeSelf)
					result.Add (gridBalls [ball.row - 1] [ball.column + 1]);
			}
		}


		return result;
	}

	List<Ball> BallActiveNeighbors (Ball ball) {
		var result = new List<Ball> ();
		if (ball.column + 1 < COLUMNS) {
			if (gridBalls [ball.row] [ball.column + 1].gameObject.activeSelf)
				result.Add (gridBalls [ball.row] [ball.column + 1]);
		}

		//left
		if (ball.column - 1 >= 0) {
			if (gridBalls [ball.row] [ball.column - 1].gameObject.activeSelf)
				result.Add (gridBalls [ball.row] [ball.column - 1]);
		}
		//top
		if (ball.row - 1 >= 0) {
			if (gridBalls [ball.row - 1] [ball.column].gameObject.activeSelf)
				result.Add (gridBalls [ball.row - 1] [ball.column]);
		}

		//bottom
		if (ball.row + 1 < ROWS) {
			if (gridBalls [ball.row + 1] [ball.column].gameObject.activeSelf)
				result.Add (gridBalls [ball.row + 1] [ball.column]);
		}


		if (ball.column % 2 == 0) {
			//bottom-left
			if (ball.row + 1 < ROWS && ball.column - 1 >= 0) {
				if (gridBalls [ball.row + 1] [ball.column - 1].gameObject.activeSelf)
					result.Add (gridBalls [ball.row + 1] [ball.column - 1]);
			}

			//bottom-right
			if (ball.row + 1 < ROWS && ball.column + 1 < COLUMNS) {
				if (gridBalls [ball.row + 1] [ball.column + 1].gameObject.activeSelf)
					result.Add (gridBalls [ball.row + 1] [ball.column + 1]);
			}
		} else {
			//top-left
			if (ball.row - 1 >= 0 && ball.column - 1 >= 0) {
				if (gridBalls [ball.row - 1] [ball.column - 1].gameObject.activeSelf)
					result.Add (gridBalls [ball.row - 1] [ball.column - 1]);
			}

			//top-right
			if (ball.row - 1 >= 0 && ball.column + 1 < COLUMNS) {
				if (gridBalls [ball.row - 1] [ball.column + 1].gameObject.activeSelf)
					result.Add (gridBalls [ball.row - 1] [ball.column + 1]);
			}
		}

		return result;
	}

	public Ball BallCloseToPoint (Vector2 point)
	{
		

		int c = Mathf.FloorToInt ((point.x + GRID_OFFSET_X + ( TILE_SIZE * 0.5f )) / TILE_SIZE);
		if (c < 0)
			c = 0;
		if (c >= COLUMNS)
			c = COLUMNS - 1;

		int r =  Mathf.FloorToInt ((GRID_OFFSET_Y + ( TILE_SIZE * 0.5f ) - point.y )/  TILE_SIZE);
		if (r < 0) r = 0;
		if (r >= ROWS) r = ROWS - 1;

		return gridBalls [r] [c];

	}

	private static System.Random rng = new System.Random(); 
	public static void Shuffle<T>(IList<T> list)  {  
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next(n + 1);  
			T value = list[k];  
			list[k] = list[n];  
			list[n] = value;  
		}  
	}

	private void GridGenerator(int levelType, ref List<Ball.BALL_TYPE> typePool)
    {
        switch (levelType)
        {
            case 0: // random
				for (int i = 0; i < 10000; i++)
                {
					typePool.Add(GetBallTypeRandom());
				}

				Shuffle(typePool);
				break;
            case 1:
                for (int i = 0; i < 2000; i++)
                {
                    typePool.Add(Ball.BALL_TYPE.TYPE_1);
					typePool.Add(Ball.BALL_TYPE.TYPE_2);
                    typePool.Add(Ball.BALL_TYPE.TYPE_3);
                    typePool.Add(Ball.BALL_TYPE.TYPE_4);
                    typePool.Add(Ball.BALL_TYPE.TYPE_5);
                }
                
                break;
			case 2:
				for (int i = 0; i < 1000; i++)
				{
					typePool.Add(Ball.BALL_TYPE.TYPE_1);
					typePool.Add(Ball.BALL_TYPE.TYPE_2);
					typePool.Add(Ball.BALL_TYPE.TYPE_3);
					typePool.Add(Ball.BALL_TYPE.TYPE_1);
					typePool.Add(Ball.BALL_TYPE.TYPE_4);
					typePool.Add(Ball.BALL_TYPE.TYPE_2);
					typePool.Add(Ball.BALL_TYPE.TYPE_5);
					typePool.Add(Ball.BALL_TYPE.TYPE_3);
					typePool.Add(Ball.BALL_TYPE.TYPE_5);
					typePool.Add(Ball.BALL_TYPE.TYPE_4);
				}

				break;
			case 3:
				for (int i = 0; i < 2000; i++)
				{
                    if (i % 2 == 0)
                    {
						typePool.Add(Ball.BALL_TYPE.TYPE_1);
						typePool.Add(Ball.BALL_TYPE.TYPE_2);
						typePool.Add(Ball.BALL_TYPE.TYPE_3);
						typePool.Add(Ball.BALL_TYPE.TYPE_4);
						typePool.Add(Ball.BALL_TYPE.TYPE_5);
					}
					else
                    {
						typePool.Add(Ball.BALL_TYPE.TYPE_5);
						typePool.Add(Ball.BALL_TYPE.TYPE_4);
						typePool.Add(Ball.BALL_TYPE.TYPE_3);
						typePool.Add(Ball.BALL_TYPE.TYPE_2);
						typePool.Add(Ball.BALL_TYPE.TYPE_1);
					}

				}
                
                break;
			case 4:
				for (int i = 0; i < 1250; i++)
				{
					typePool.Add(Ball.BALL_TYPE.TYPE_1);
					typePool.Add(Ball.BALL_TYPE.NONE);
					typePool.Add(Ball.BALL_TYPE.TYPE_2);
					typePool.Add(Ball.BALL_TYPE.TYPE_3);
					typePool.Add(Ball.BALL_TYPE.NONE);
					typePool.Add(Ball.BALL_TYPE.TYPE_4);
					typePool.Add(Ball.BALL_TYPE.TYPE_5);
					typePool.Add(Ball.BALL_TYPE.NONE);
				}

				break;
			case 5:
				for (int i = 0; i < 1000; i++)
				{
					typePool.Add(Ball.BALL_TYPE.TYPE_5);
					typePool.Add(Ball.BALL_TYPE.TYPE_4);
					typePool.Add(Ball.BALL_TYPE.NONE);
					typePool.Add(Ball.BALL_TYPE.NONE);
					typePool.Add(Ball.BALL_TYPE.NONE);
					typePool.Add(Ball.BALL_TYPE.NONE);
					typePool.Add(Ball.BALL_TYPE.NONE);
					typePool.Add(Ball.BALL_TYPE.NONE);
					typePool.Add(Ball.BALL_TYPE.NONE);
					typePool.Add(Ball.BALL_TYPE.TYPE_4);
					typePool.Add(Ball.BALL_TYPE.TYPE_5);
				}

				break;
		}
    }
}
