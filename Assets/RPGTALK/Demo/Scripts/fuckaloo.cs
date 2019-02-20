using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fuckaloo : MonoBehaviour {

	//We will sometimes initialize the talk by script, so let's keep a instance of the current RPGTalk
	public RPGTalk rpgTalk;

	void Start(){
		rpgTalk.OnMadeChoice += OnMadeChoice;
	}

	void OnMadeChoice(int questionId, int choiceID){
		if (choiceID == 0) {
			Debug.Log ("Choosed 1");
		} else {
			Debug.Log ("Choosed 2");
		}
	}
}
