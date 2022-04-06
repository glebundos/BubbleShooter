using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RayCastShooter : MonoBehaviour
{
    public GameObject[] colorsGO;
	public GameObject dotPrefab;
	public Bullet bullet;
	public Grid grid;
	public LayerMask collisionMask;
    
	private bool mouseDown = false;
	private List<Vector2> dots;
	private List<GameObject> dotsPool;
	private int maxDots = 26;

	private float dotGap = 0.32f;
	private float bulletProgress = 0.0f;
	private float bulletIncrement = 0.0f;

	private int type = 0;
	private int bullets = 0;

    public GameController gameController;

    public static int difficulty = 1;
    private int bulletsBeforeNewLine = 11;
	// Use this for initialization
	void Start () {
        dots = new List<Vector2> ();
		dotsPool = new List<GameObject> ();
        
        maxDots = difficulty switch
        {
            1 => 100,
            2 => 28,
            3 => 22,
            _ => 28
        };

        var i = 0;
		var alpha = 1.0f / maxDots;
		var startAlpha = 1.0f;
		while (i < maxDots) {
			var dot = Instantiate (dotPrefab) as GameObject;
			var sp = dot.GetComponent<SpriteRenderer> ();
			var c = sp.color;

			c.a = startAlpha - alpha;
			startAlpha -= alpha;
			sp.color = c;

			dot.SetActive (false);
			dotsPool.Add (dot);
			i++;
		}

        bulletsBeforeNewLine -= difficulty;
		//select initial type
		SetNextType();
	}

	void SetNextType () {

		foreach (var go in colorsGO) {
			go.SetActive(false);
		}

		type = Random.Range (0, 5);
		colorsGO [type].SetActive (true);

		bullets++;
        gameController.AddShot();

		if (bullets > bulletsBeforeNewLine) {
			bullets = 0;
			Ball.addLine = true;
		}
	}

	void HandleTouchDown (Vector2 touch) 
	{
		Vector2 point = Camera.main.ScreenToWorldPoint(touch);
		RaycastHit2D hitInformation = Physics2D.Raycast(point, Camera.main.transform.forward);
		if (hitInformation.collider != null)
		{
			//We should have hit something with a 2D Physics collider!
			GameObject touchedObject = hitInformation.transform.gameObject;
            if (touchedObject.transform.name == "Panel")
            {
				return;
            }
		}
	}

	void HandleTouchUp (Vector2 touch)
	{
		if (dots == null || dots.Count < 2)
			return;
		
		foreach (var d in dotsPool)
			d.SetActive (false);
		
		bulletProgress = 0.0f;
		bullet.SetType ((Ball.BALL_TYPE) type);
		bullet.gameObject.SetActive (true);
		bullet.transform.position = transform.position;
		InitPath ();


		SetNextType();
	}

	void HandleTouchMove (Vector2 touch) {
		if (bullet.gameObject.activeSelf)
			return;

		if (dots == null) {
			return;
		}

		dots.Clear ();

		foreach (var d in dotsPool)
			d.SetActive (false);

		Vector2 point = Camera.main.ScreenToWorldPoint (touch);
		if (point.y > 4)
        {
			return;
        }
        
        RaycastHit2D hitInformation = Physics2D.Raycast(point, Camera.main.transform.forward);
		if (hitInformation.collider != null)
		{
			//We should have hit something with a 2D Physics collider!
			GameObject touchedObject = hitInformation.transform.gameObject;
			if (touchedObject.transform.name == "Panel")
			{
				return;
			}
		}
        
		var direction = new Vector2 (point.x - transform.position.x, point.y - transform.position.y);

		RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, collisionMask);
		if (hit.collider != null) {
			
			dots.Add (transform.position);

            if (hit.collider.tag == "SideWall")
            {
				DoRayCast(hit, direction);
			}
			else 
			{
				dots.Add (hit.point);
				DrawPaths ();
			}
		}
	}

	void DoRayCast (RaycastHit2D previousHit, Vector2 directionIn) {

		dots.Add (previousHit.point);


		float normal;
		float newDirection;
		Vector2 reflection;
		Vector2 newCastPoint;

        normal = Mathf.Atan2(previousHit.normal.y, previousHit.normal.x);
        newDirection = normal + (normal - Mathf.Atan2(directionIn.y, directionIn.x));
		reflection = new Vector2(-Mathf.Cos(newDirection), -Mathf.Sin(newDirection));
		newCastPoint = previousHit.point + (reflection);


		var hit2 = Physics2D.Raycast(newCastPoint, reflection, collisionMask);
		if (hit2.collider != null) {
            if (hit2.collider.tag == "SideWall")
            {
				//shoot another cast
				DoRayCast(hit2, reflection);
			}
			else {
				dots.Add (hit2.point);
				DrawPaths ();
			}
		} else {
			DrawPaths ();
		}
	}


	// Update is called once per frame
	void Update () {
		
		if (GameController.paused)
        {
			return;
        }
        
		if (bullet.gameObject.activeSelf) 
		{
            return;
		}
		
		if (dots == null)
			return;

		if (Input.touches.Length > 0) {

			Touch touch = Input.touches [0];

			if (touch.phase == TouchPhase.Began)
			{
				HandleTouchDown (touch.position);
			} 
			else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended) 
			{
				HandleTouchUp (touch.position);
			} 
			else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
			{
				HandleTouchMove (touch.position);
			}
			HandleTouchMove (touch.position);	
			return;
		} else if (Input.GetMouseButtonDown (0)) {
			mouseDown = true;
			HandleTouchDown (Input.mousePosition);
		} else if (Input.GetMouseButtonUp (0)) {
			mouseDown = false;
			HandleTouchUp (Input.mousePosition);
		} else if (mouseDown) {
			HandleTouchMove (Input.mousePosition);
		}
	}

	void FixedUpdate()
    {
		if (bullet.gameObject.activeSelf)
		{

			bulletProgress += bulletIncrement;

			if (bulletProgress > 1)
			{
				dots.RemoveAt(0);
				if (dots.Count < 2)
				{
					bullet.gameObject.SetActive(false);
					return;
				}
				else
				{
					InitPath();
				}
			}

			var px = dots[0].x + bulletProgress * (dots[1].x - dots[0].x);
			var py = dots[0].y + bulletProgress * (dots[1].y - dots[0].y);

			bullet.transform.position = new Vector2(px, py);

			return;
		}
	}

	void DrawPaths () {
		
		if (dots.Count > 1) {

			foreach (var d in dotsPool)
				d.SetActive (false);

			int index = 0;

			for (var i = 1; i < dots.Count; i++) {
				DrawSubPath (i - 1, i, ref index);
			}
		}
	}

	void DrawSubPath (int start, int end, ref int index) {
		var pathLength = Vector2.Distance (dots [start], dots [end]);

		int numDots = Mathf.RoundToInt ( (float)pathLength / dotGap );
		float dotProgress = 1.0f / numDots;

		var p = 0.0f;

		while (p < 1) {
			var px = dots [start].x + p * (dots [end].x - dots [start].x);
			var py = dots [start].y + p * (dots [end].y - dots [start].y);

			if (index < maxDots) {
				var d = dotsPool [index];
				d.transform.position = new Vector2 (px, py);
				d.SetActive (true);
				index++;
			}

			p += dotProgress;
		}
	}

	void InitPath () {
		var start = dots [0];
		var end = dots [1];
		var length = Vector2.Distance (start, end);
		var iterations = length / 0.2f; // bullet speed
		bulletProgress = 0.0f;
		bulletIncrement = 1.0f / iterations;
	}


}
