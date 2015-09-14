using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TeamScoreView : MonoBehaviour {

	private GameMaster gameMaster;

	
	public static int commonMaxPoint = 10;

	public AudioClip sound;


	public int minSize = 100;
	public int maxSize = 800;

	public int point;
	public int maxPoint = 10;


	private float size;

	public float Size {
		get {
			return size;
		}
	}

	public float timeToBig = 0.5f;
	public float timeToSmall = 0.2f;
	public float magRate = 2f;


	public Text text;
	public RectTransform rect;


	public void AddPoint(int p){
		//int pp = p * Random.Range (1, 10);
		point += p;

		//commonMaxPoint = Mathf.Max (commonMaxPoint, point);

		
		if (!isGettingBig) {
			isInAction = true;
			isGettingBig = true;

			if(actionRoutine != null)
				StopCoroutine(actionRoutine);
			actionRoutine = StartCoroutine (Action (2));
			gameMaster.PlaySound(sound);
		}

	}

	private Coroutine actionRoutine;

	private bool isInAction = false;
	private bool isGettingBig = false;

	public void UpdateView(){
		
		text.text = "" + point;

		size = Mathf.Lerp (minSize, maxSize, (point / (float)commonMaxPoint));

//		if (isInAction)
//			return;

	}

	public void SetSize(float size){
		rect.sizeDelta = Vector2.one * size;
	}

	public void Reset(){
		point = 0;
		commonMaxPoint = 10;
		text.enabled = true;
		UpdateView ();
		SetSize (minSize);
	}

	public void HideScore(){
		text.enabled = false;
	}


	
	// Use this for initialization
	void Start () {
		rect = GetComponent<RectTransform> ();
		UpdateView ();

//		text = GetComponentInChildren<Text> ();
		commonMaxPoint = maxPoint;

		gameMaster = FindObjectOfType<GameMaster> ();

		//StartCoroutine (Vibration ());
	}

	public IEnumerator Vibration(){
		while(true){
			yield return new WaitForSeconds(Random.Range (0.01f, 0.2f));
			rect.sizeDelta = Vector2.one * size * Random.Range(.8f, 1.2f);
		}

	}

	public IEnumerator Action(int p){
		rect.SetAsLastSibling();


		RectTransform r = text.GetComponent<RectTransform> ();
		Vector2 s = Vector3.one*size;
		Vector2 targetS = s * 2;

		float d = timeToBig;
		float t = Time.time+d;

		while (Time.time < t) {

			rect.sizeDelta = Vector3.Lerp(rect.sizeDelta, Vector2.one*size*p, 1 - (t - Time.time) / d );

			yield return new WaitForEndOfFrame ();
		}

		isGettingBig = false;

		d = timeToSmall;
		t = Time.time+d;
		while (Time.time < t) {
			rect.sizeDelta = Vector3.Lerp(Vector2.one*size*p, Vector2.one*size, 1 - (t - Time.time) / d  );

			yield return new WaitForEndOfFrame ();
		}
		
		isInAction = false;
	}

	// Update is called once per frame
	void Update () {
		//UpdateView ();
	}
}
