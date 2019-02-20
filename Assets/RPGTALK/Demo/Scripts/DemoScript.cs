using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//We might change the language in this script, so let's use the localization
using RPGTALK.Localization;

public class DemoScript : MonoBehaviour {
	//The speed that our 'hero' will run
	public float speed = 10f;

	//A few variables to move/animate this guy
	Rigidbody2D rigid;
	Animator anim;
	SpriteRenderer render;

	//The user can move the hero?
	public bool controls;

	//We will sometimes initialize the talk by script, so let's keep a instance of the current RPGTalk
	public RPGTalk rpgTalk;

	//A canvas that will be shown asking for the player's name
	public GameObject askWho;
	//The input that the player should write its name
	public InputField myName;

	//A wall to desappear and a particle to play when that happens
	public GameObject wall;
	public GameObject particle;

	// Get the right references...
	void Start () {
		rigid = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
		render = GetComponent<SpriteRenderer> ();
		//In the tagsDemo scene, we want to do something when we make a choice...
		rpgTalk.OnMadeChoice += OnMadeChoice;
	}

	// Update is called once per frame
	void Update () {

		//skip the Talk to the end if the player hit Return
		if(Input.GetKeyDown(KeyCode.Return)){
			rpgTalk.EndTalk ();
		}



		//if the user have the controls
		if (controls) {

			//let's move around!
			float moveX = Input.GetAxis ("Horizontal");
			float moveY = Input.GetAxis ("Vertical");
			rigid.MovePosition (new Vector2 (transform.position.x + moveX * speed, transform.position.y + moveY * speed));

			//Not the best way to do it but... change the animator
			if (moveX > 0) {
				anim.SetBool ("side", true);
				anim.SetBool ("top", false);
				anim.SetBool ("bottom", false);
				render.flipX = true;
				anim.speed = 1;
			} else if (moveX < 0) {
				anim.SetBool ("side", true);
				anim.SetBool ("top", false);
				anim.SetBool ("bottom", false);
				render.flipX = false;
				anim.speed = 1;
			} else if (moveY < 0) {
				anim.SetBool ("side", false);
				anim.SetBool ("top", false);
				anim.SetBool ("bottom", true);
				anim.speed = 1;
			} else if (moveY > 0) {
				anim.SetBool ("side", false);
				anim.SetBool ("top", true);
				anim.SetBool ("bottom", false);
				anim.speed = 1;
			} else {
				anim.speed = 0;
			}


		} else {
			anim.speed = 0;
		}
	}

	//the player cant move
	public void CancelControls(){
		controls = false;
	}

	//give back the controls to player
	public void GiveBackControls(){
		controls = true;
	}

	//Open the screen to enter Player's name
	public void WhoAreYou(){
		askWho.SetActive(true);
		myName.Select ();
	}

	//This callback will be called by RPGTalk after the first talk ends with the "FunnyGuy"
	//Here, we will change the value of a variable in RPGTalk to be the name of the player
	//And then, we will start a new talk =D
	public void IKnowYouNow(){
		askWho.SetActive (false);
		rpgTalk.variables [0].variableValue = myName.text;
		rpgTalk.NewTalk ("17", "25", rpgTalk.txtToParse, this, "ByeWall");
	}

	//Let's get rid of that wall. This function was called by RPGTalk bacause the function above
	//setted it to be its callback.
	public void ByeWall(){
		wall.SetActive (false);
		particle.SetActive (true);
		Invoke ("FunnyGuyEnd", 2f);
	}

	//After the wall exploded, let the Funny Guy end his talking
	void FunnyGuyEnd(){
		rpgTalk.NewTalk ("26", "29", rpgTalk.txtToParse, this, "GiveBackControls");
	}

	//In the TagsDemo scene, when we make a choice let's find out what we chose
	//and change the current language based on it
	void OnMadeChoice(int questionId, int choiceID){
		if (choiceID == 0) {
			LanguageSettings.actualLanguage = SupportedLanguages.EN_US;
		} else {
			LanguageSettings.actualLanguage = SupportedLanguages.PT_BR;
		}
	}
	
}
