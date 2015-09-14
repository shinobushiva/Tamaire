using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{

	public RectTransform gamePlay;
	public TeamScoreView[] scores;
	public Text totalScoreText;
	public int gameTimeInSecond;
	public int[] points = new int[]{1, 3, 5};
	public RectTransform gameResult;
	public TeamScoreView[] scoresForResult;

	public enum GameState
	{
		Reseted,
		InGame,
		GameEnding,
		GameEnd,
		ScoreView,
		Corouting
	}
	GameState gameState = GameState.Reseted;

	public void SetGameTime (string s)
	{
		gameTimeInSecond = int.Parse (s);
	}

	private float startTime;

	// Use this for initialization
	void Start ()
	{
		gamePlay.gameObject.SetActive (true);
		gameResult.gameObject.SetActive (false);
	}

	public void GameStart ()
	{
		startTime = Time.time;
		gameState = GameState.InGame;
	}

	public void GameReset ()
	{
		StopAllCoroutines ();

		gamePlay.gameObject.SetActive (true);
		gameResult.gameObject.SetActive (false);

		gameState = GameState.Reseted;

		int sum = 0;
		foreach (TeamScoreView tcv in scores) {
			tcv.Reset ();
		}

		totalScoreText.text = "Waiting for Entry...";

	}

	private string SecondToSting (float time)
	{
		int iTime = (int)time;
		int min = iTime / 60;
		int sec = iTime - 60 * min;

		return min.ToString ("0") + ":" + sec.ToString ("00");
	}
	
	// Update is called once per frame
	void Update ()
	{

		int sum = 0;
		foreach (TeamScoreView tcv in scores) {
			sum += tcv.point;
		}
		TeamScoreView.commonMaxPoint = (int)(Mathf.Max (sum, 10) / 1.5f);

		float remaining = gameTimeInSecond - (Time.time - startTime);
		if (IsInGame ()) {
			if (remaining < 30f)
				gameState = GameState.GameEnding;

			if (remaining <= 0f) {
				gameState = GameState.GameEnd;
			}
		}

		if (gameState == GameState.InGame) {
//			totalScoreText.text = "" + sum;
			totalScoreText.text = SecondToSting (remaining);
	
			int z = 0;
			foreach (TeamScoreView tcv in scores.OrderBy(x=>x.point).ToList()) {
				tcv.UpdateView ();
				//tcv.rect.SetAsFirstSibling();
			}
		}
		if (gameState == GameState.GameEnding) {
			totalScoreText.text = SecondToSting (remaining);

			foreach (TeamScoreView tcv in scores.OrderBy(x=>x.point).ToList()) {
				tcv.HideScore ();
				tcv.UpdateView ();
			}
			totalScoreText.GetComponent<RectTransform> ().SetAsLastSibling ();
		}

		if (gameState == GameState.GameEnd) {

			StartCoroutine (GameEndRoutine ());
			gameState = GameState.Corouting;
		}

		if (gameState == GameState.ScoreView) {

			StartCoroutine (GameResultRoutine ());
			gameState = GameState.Corouting;

		}
	}

	IEnumerator GameResultRoutine ()
	{

		gamePlay.gameObject.SetActive (false);
		gameResult.gameObject.SetActive (true);
		 
		yield return new WaitForEndOfFrame ();

		foreach (TeamScoreView tcv in scoresForResult) {
			tcv.gameObject.SetActive(true);
			tcv.text.text = "" + 0;
			tcv.SetSize (tcv.minSize);
//			tcv.UpdateView ();

		}

		int maxPoint = 0;
		int[] points = new int[scores.Length];
		int i = 0;
		foreach (TeamScoreView tcv in scores) {
			points [i] = tcv.point;
			maxPoint = Mathf.Max (points [i], maxPoint);
			print (points [i]);

			i++;
		}

		print ("MaxPoint:"+maxPoint);
		if (maxPoint == 0)
			yield break;

		float dul = 10f;

		float t = Time.time + dul;
		while (Time.time < t) {
			float tt = 1f - (t - Time.time) / dul;
			i = 0;
			foreach (TeamScoreView tcv in scoresForResult) {

				int p = Mathf.Min ((int)Mathf.Lerp (0, maxPoint, tt * tt+.01f), points [i]);

				float size = Mathf.Lerp (tcv.minSize, tcv.maxSize, Mathf.Pow ((p / (float)maxPoint), 2));
				tcv.text.text = "" + p;
				tcv.SetSize (size);

				i++;
			}
			yield return null;
		}

		yield return new WaitForSeconds (1);

		int count = 0;
		for (int j=0; j<scores.Length; j++) {
			if (scores [j].point == maxPoint) {
				count++;
			}
		}
		print ("Count:" + count);
				
				
		dul = 0.5f;
		t = Time.time + dul;
		while (Time.time < t) {
			float tt = 1f - (t - Time.time) / dul;

			for (int j=0; j<scoresForResult.Length; j++) {

				TeamScoreView tsv = scoresForResult [j];
			
				float csize = tsv.maxSize;
				float maxSize = 1100/count;
				
				if (scores[j].point == maxPoint) {
					float size = Mathf.Lerp (csize, maxSize, tt * tt);
					//			tcv.text.text = "" + p;
					if(count == 1){
						tsv.SetSize (size);
					}
					tsv.GetComponent<RectTransform> ().SetAsLastSibling ();

					tsv.gameObject.SetActive(true);
				}else{
					tsv.gameObject.SetActive(false);
				}

			}
			yield return null;
		}


		yield return new WaitForSeconds (5);


	}

	IEnumerator GameEndRoutine ()
	{
		totalScoreText.text = "Game Set";

		float[] size = new float[scores.Length];
		int i = 0;
		foreach (TeamScoreView tcv in scores) {
			size [i++] = tcv.Size;
		}


		float t = 0;
		while (t <=1) {
			t = (Time.time - (startTime + gameTimeInSecond)) / 2f;
			i = 0;
			foreach (TeamScoreView tcv in scores) {
				tcv.SetSize (Mathf.Lerp (size [i++], tcv.minSize, t));
				tcv.UpdateView ();
			}
			yield return null;
		}
		
		yield return new WaitForSeconds (3);

		gameState = GameState.ScoreView;
	}

	private bool IsInGame ()
	{
		return (gameState == GameState.InGame || gameState == GameState.GameEnding);
	}

	public void Add (byte point, int num)
	{
		if (!IsInGame ())
			return;
		
		int i = num + 2;
		if (point >= 1) {
			
			int idx = num / 3;
			int r = num % 3;
			
			for (int j = 0; j<point; j++) {
				scores [idx].AddPoint (points [r]);
			}
		}
		
	}

	public void PlaySound (AudioClip clip)
	{
		AudioSource.PlayClipAtPoint (clip, Camera.main.transform.position);
//		AudioSource audio = GetComponent<AudioSource> ();
//		audio.clip = clip;
//		audio.Play ();
	}
}
