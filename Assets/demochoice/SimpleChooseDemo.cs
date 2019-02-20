using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class SimpleChooseDemo : MonoBehaviour {

	public PlayableDirector director;
	public PlayableAsset timelineToChange;
	public RPGTalk rpgtalk;

	// Use this for initialization
	void Start () {
		rpgtalk.OnMadeChoice += OnMadeChoice;
	}
	
	void OnMadeChoice(string questionId, int choiceID){
		if(choiceID == 1){
			//Change the timeline
			director.playableAsset = timelineToChange;
			//Change go back to the beggining
			director.time = 0;
			//Force to play it again
			director.Play ();
		}
	}

	void OnDestroy(){
		rpgtalk.OnMadeChoice -= OnMadeChoice;
	}
}
