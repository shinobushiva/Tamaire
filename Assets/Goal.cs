using UnityEngine;
using System.Collections;

public class Goal : MonoBehaviour {

	public int point = 2;

	public Color baseColor = Color.white;
	public Color goalColor = Color.red;

	Renderer renderer;

	public TeamScoreView scoreView;

	// Use this for initialization
	void Start () {
		renderer = GetComponent<Renderer> ();
		renderer.material.color = baseColor;
	
	}
	
	// Update is called once per frame
	void Update () {
	

	}

	void OnTriggerEnter(Collider c){
		if (c.tag == "Ball") {
			print ("Goal!");
			StartCoroutine(GoalAction());
			Destroy(c.gameObject);

			if(scoreView)
				scoreView.AddPoint(1);
		}

	}

	IEnumerator GoalAction(){
		float d = 1f;
		float t = Time.time + d;
		while (Time.time < t) {
			renderer.material.color = Color.Lerp(baseColor, goalColor, 1f - (t-Time.time)/d);
			yield return new WaitForEndOfFrame();
		}

		 d = 1f;
		 t = Time.time + d;
		while (Time.time < t) {
			renderer.material.color = Color.Lerp(goalColor, baseColor, 1f - (t-Time.time)/d);
			yield return new WaitForEndOfFrame();
		}
	}


}
