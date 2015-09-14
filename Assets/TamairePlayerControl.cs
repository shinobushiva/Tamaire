using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]  
public class TamairePlayerControl : MonoBehaviour {

	private Animator animator;

	public bool hasBall;
	private bool inAction;

	public Rigidbody ball;

	public Rigidbody currenBall;

	public Transform ballPositionTarget;

	public float releaseTime = 0.8f;

	public float power = 300;

	public Transform aimingGoal;

	public float initialVelocity = 15f;

	public bool isPlayer = true;

	public Color color = new Color(104, 211, 255);


	// Use this for initialization
	void Start () {

		Renderer[] rs = GetComponentsInChildren<Renderer> ();
		foreach (Renderer r in rs) {
			if(r.material.name.Contains("Body"))
				r.material.color = color;
		}

		animator = GetComponent<Animator> ();

		if (!isPlayer) {
			StartCoroutine(UpdateAuto());
		}


		float g = Physics.gravity.y;
		print("g:"+g);
		float v0 = 30;


		Vector3 vec;//= (aimingGoal.position - currenBall.position);
		vec = new Vector3(79.5f, 34.5f, 0f);
		Vector3 vecY0 = vec;
		vecY0.y = 0;
		
		float x = vecY0.magnitude;
		float y = vec.y;
		

		
		float A = g*x*x/(2*v0*v0);
		
		float a = x/A;
		float b = y/A;
		
		float XP = Mathf.Sqrt (a*a/4-b-a/2);
		float XN = -Mathf.Sqrt (a*a/4-b-a/2);
		
		float tP = Mathf.Atan(XP);
		float tN = Mathf.Atan(XN);
		
		print (Mathf.Rad2Deg*tP);
		print (Mathf.Rad2Deg*tN);
	
	}


	IEnumerator UpdateAuto () {

		float runTime = 0;
		float waitTimeAfterThrow = 0;
		
		Goal[] goals;
		goals = FindObjectsOfType<Goal> ();
		WaitForEndOfFrame w = new WaitForEndOfFrame ();

		while (true) {
			yield return w;

			if (inAction)
				continue;
		
			if (!hasBall) {

				inAction = true;
				animator.SetTrigger ("Pickup");
				
				StartCoroutine (CreateBall ());

				waitTimeAfterThrow = Time.time + Random.Range(5f, 10f);
			} else {
				if(waitTimeAfterThrow < Time.time){

					foreach (Goal g in goals) {
//						print (Vector3.Angle (transform.forward, (g.transform.position - transform.position).normalized));
						if (Vector3.Distance (g.transform.position, transform.position) < 20) {
							if (Vector3.Angle (transform.forward, (g.transform.position - transform.position).normalized) < 30) {

								animator.SetBool ("VHIn", false);
								animator.SetFloat ("Direction", 0);
								animator.SetFloat ("Speed", 0);

								inAction = true;
								aimingGoal = g.transform;
								animator.SetTrigger ("Throw");
							
								StartCoroutine (ThrowBall ());

								break;
							}
						}
					}
				}
			}


			if (!hasBall || inAction){
				continue;
			}

			
			if(Vector3.Angle (transform.forward, (-transform.position).normalized) > 90f){
				animator.SetBool ("VHIn", true);
				animator.SetFloat ("Direction", 1);
				animator.SetFloat ("Speed", .5f);
				animator.SetBool ("Run", true);
			} else
			{
		
				float x = Random.Range (-1f, 1f);
				float y = Random.Range (-1f, 1f);

				if (runTime < Time.time) {

					if (Mathf.Abs (x) > 0.1f || Mathf.Abs (y) > 0.1f) {
						//			GetComponent<TamaireNavigation> ().enabled = false;
						animator.SetBool ("VHIn", true);
					} else {
						//			GetComponent<TamaireNavigation>().enabled = true;
						animator.SetBool ("VHIn", false);
					}
			
					animator.SetFloat ("Direction", x);
					animator.SetFloat ("Speed", y);

					runTime = Time.time + Random.Range (1f, 5f);
				}

				animator.SetBool ("Run", true);

			}

		}
	}
	
	// Update is called once per frame
	void Update () {
		if(!isPlayer){
			return;
		}

		if (inAction)
			return;

		if (!hasBall) {

			if (Input.GetKeyDown ("space")) {
				inAction = true;
				animator.SetTrigger ("Pickup");

				StartCoroutine (CreateBall ());
			}
		} else {
			if (Input.GetKeyDown ("space")) {
				inAction = true;
				aimingGoal = null;
				animator.SetTrigger ("Throw");

				StartCoroutine (ThrowBall ());
			}

			if (Input.GetButtonDown ("Fire1")) {
				var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit = new RaycastHit ();
				if (Physics.Raycast (ray, out hit)) {
					if (hit.transform.tag == "Goal") {
						if (animator.GetCurrentAnimatorStateInfo (0).IsName ("Idle")) {

							inAction = true;
							aimingGoal = hit.transform;
							animator.SetTrigger ("Throw");

							StartCoroutine (ThrowBall ());
						}
					}
				}
			}
		}

		float x = Input.GetAxis ("Horizontal");
		float y = Input.GetAxis ("Vertical");

		if (Mathf.Abs (x) > 0.1f || Mathf.Abs (y) > 0.1f) {
//			GetComponent<TamaireNavigation> ().enabled = false;
			animator.SetBool("VHIn", true);
		} else {
//			GetComponent<TamaireNavigation>().enabled = true;
			animator.SetBool("VHIn", false);
		}
	
		animator.SetFloat ("Direction", x);
		animator.SetFloat ("Speed", y);

		animator.SetBool ("Run", Input.GetKey (KeyCode.LeftShift));


	
	}

	void FixedUpdate(){
		if (currenBall) {
			currenBall.position = ballPositionTarget.position;
		}
	}



	private IEnumerator CreateBall(){

		yield return new WaitForSeconds (0.5f);

		Rigidbody b = Instantiate<Rigidbody> (ball);
		b.isKinematic = true;
		b.transform.position = ballPositionTarget.position;
		currenBall = b;

		b.GetComponent<TrailRenderer> ().enabled = false;

		while (animator.GetCurrentAnimatorStateInfo (0).IsName ("PickingUp")) {
			yield return new WaitForEndOfFrame ();
		}

		hasBall = true;
		inAction = false;

	}

	private IEnumerator ThrowBall(){

		yield return new WaitForSeconds (releaseTime);

		currenBall.isKinematic = false;

		if (aimingGoal != null) {

			float g = Physics.gravity.y;
			float v0 = initialVelocity;// 25; //90km/h

			Vector3 vec = (aimingGoal.position - currenBall.position);
			Vector3 vecY0 = vec;
			vecY0.y = 0;
			print (vecY0);
			
			float x = vecY0.magnitude;
			float y = vec.y;

			float A = g*x*x/(2*v0*v0);
			
			float a = x/A;
			float b = y/A;
			
			float XP = Mathf.Sqrt (a*a/4f-b-a/2f);
			float XN = -Mathf.Sqrt (a*a/4f-b-a/2f);
			
			float tP = Mathf.Rad2Deg*Mathf.Atan(XP);
			float tN = Mathf.Rad2Deg*Mathf.Atan(XN);
			
			print (tP);
			print (tN);

			currenBall.transform.eulerAngles = Vector3.zero;
			currenBall.transform.LookAt(currenBall.position+vecY0);
			currenBall.transform.Rotate(Vector3.left*tP, Space.Self);
			currenBall.velocity = currenBall.transform.forward*v0*0.8f;
			currenBall.angularVelocity = Vector3.zero;
//			currenBall.AddForce (d * power);
		} else {

			Vector3 d = transform.TransformDirection (new Vector3 (0, 1, 1)).normalized;
			currenBall.velocity = Vector3.zero;
			currenBall.angularVelocity = Vector3.zero;

			currenBall.AddForce (d * power);
		}

		currenBall.GetComponent<TrailRenderer> ().enabled = true;
		Destroy (currenBall.gameObject, 20);
		currenBall = null;

		while (animator.GetCurrentAnimatorStateInfo (0).IsName ("Throwing")) {
			yield return new WaitForEndOfFrame ();
		}

		hasBall = false;
		inAction = false;
	}

	void OnDrawGizmos(){
		Gizmos.color = Color.green;
		Vector3 f = ballPositionTarget.position;
		Vector3 t = f + transform.TransformDirection (new Vector3 (0, 1, 1));
		Gizmos.DrawLine (f, t);
	}
}
