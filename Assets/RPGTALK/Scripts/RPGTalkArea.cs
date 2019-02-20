using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[AddComponentMenu("Seize Studios/RPGTalk/RPGTalk Area")]
public class RPGTalkArea : MonoBehaviour {

	/// <summary>
	/// The RPGTalk to be played. Can be left null if you only need to play a timeline director, for instance
	/// </summary>
	public RPGTalk rpgtalkTarget;
	/// <summary>
	/// Should the Talk start only when the player hit a button?
	/// </summary>
	public bool shouldInteractWithButton;
	/// <summary>
	/// When the player can interact, show/hide any GameObjects. Useful for something like 'Press A to talk'
	/// </summary>
	public GameObject[] showWhenInteractionIsPossible;
	/// <summary>
	/// What is the key that should be used to interact? You can override the Update function and write your 
	/// own conditions, if needed
	/// </summary>
	public KeyCode interactionKey = KeyCode.E;
	bool canInteract;
	/// <summary>
	/// The talk will only begin if someone with this tag hits it. Leave blank to accept anyone
	/// </summary>
	public string checkIfColliderHasTag = "";
	/// <summary>
	/// A callback script might be called before the talk happens. Useful to freeze player's controller, for instance.
	/// Leave blank if nothing is needed
	/// </summary>
	public MonoBehaviour callbackScriptBeforeTalk;
	/// <summary>
	/// What funcition of the callbackScriptBeforeTalk should be called?
	/// </summary>
	public string callbackFunctionBeforeTalk;
	/// <summary>
	/// What line to start the talk
	/// </summary>
	public string lineToStart = "1";
	/// <summary>
	/// What line to finish the talk (leave -1 to read the file until the end)
	/// </summary>
	public string lineToBreak = "-1";
	/// <summary>
	/// The text file to be parsed into the talks. Leave empty if it is the same as the rpgtalkTarget
	/// </summary>
	public TextAsset txtToParse;
	/// <summary>
	/// A callback script might be called after the talk happens. Useful to unfreeze player's controller, for instance.
	/// Leave blank if nothing is needed
	/// </summary>
	public MonoBehaviour callbackScriptAfterTalk;
	/// <summary>
	/// What funcition of the callbackScriptAfterTalk should be called?
	/// </summary>
	public string callbackFunctionAfterTalk;
	/// <summary>
	/// Should this talk/interaction be available only once?
	/// </summary>
	public bool happenOnlyOnce;
	[HideInInspector]
	/// <summary>
	/// This talk already happened before?
	/// </summary>
	public bool alreadyHappened;
	/// <summary>
	/// Activate talk on trigger enter? (ignored if shouldInteractWithButton is true)
	/// </summary>
	public bool triggerEnter = true;
	/// <summary>
	/// Activate talk on trigger exit? (ignored if shouldInteractWithButton is true)
	/// </summary>
	public bool triggerExit = false;
	/// <summary>
	/// After the talk finishes, the canvas shold stay on the screen?
	/// </summary>
	public bool shouldStayOnScreen;
	/// <summary>
	/// Forbids the talk to be initialized if the rpgtalkTarget is already showing something else.
	/// </summary>
	public bool forbidPlayIfRpgtalkIsPlaying;
	/// <summary>
	/// The timeline director can be played with the same rules as the talk.
	/// </summary>
	public PlayableDirector timelineDirectorToPlay;

	/// <summary>
	/// Hide anything that shouldn't be showing upon the start
	/// </summary>
	protected virtual void Start () {
		HideInteractionInstruction ();
	}
	
	/// <summary>
	/// Check for the interaction. Override this method to implement your own rules
	/// </summary>
	protected virtual void Update () {
		if (shouldInteractWithButton && canInteract) {
			if (Input.GetKeyDown (interactionKey)) {
				StartTalk ();
			}
		}
	}

	/// <summary>
	/// Check the rules and put it into rpgtalkTarget, initializing a new talk.
	/// </summary>
	protected virtual void NewTalk(){
		if (rpgtalkTarget == null || (happenOnlyOnce && alreadyHappened) || (forbidPlayIfRpgtalkIsPlaying && rpgtalkTarget.isPlaying)) {
			return;
		}

		alreadyHappened = true;
		if (callbackScriptBeforeTalk != null) {
			callbackScriptBeforeTalk.Invoke (callbackFunctionBeforeTalk, 0);
		}
		TextAsset newTxt = rpgtalkTarget.txtToParse;
		if (txtToParse != null) {
			newTxt = txtToParse;
		}

		rpgtalkTarget.shouldStayOnScreen = shouldStayOnScreen;

		if (callbackScriptAfterTalk == null) {
			rpgtalkTarget.NewTalk (lineToStart, lineToBreak, newTxt);
		} else {
			rpgtalkTarget.NewTalk (lineToStart, lineToBreak, newTxt,callbackScriptAfterTalk,callbackFunctionAfterTalk);
		}
	}

	/// <summary>
	/// Show anything in the showWhenInteractionIsPossible array
	/// </summary>
	protected virtual void ShowInteractionInstruction(){
		if (happenOnlyOnce && alreadyHappened) {
			return;
		}
		foreach (GameObject GO in showWhenInteractionIsPossible) {
			GO.SetActive (true);
		}
	}

	/// <summary>
	/// Hides anything in the showWhenInteractionIsPossible array
	/// </summary>
	protected virtual void HideInteractionInstruction(){
		foreach (GameObject GO in showWhenInteractionIsPossible) {
			GO.SetActive (false);
		}
	}

	/// <summary>
	/// Prepare the variables for the interaction or just call a new talk
	/// </summary>
	/// <param name="tagName">Should only work with specifc tag?</param>
	/// <param name="gettingOut">Was called from an OnTriggerExite?</param>
	protected virtual void PrepareInteraction(string tagName, bool gettingOut = false){
		if(tagName == checkIfColliderHasTag || checkIfColliderHasTag == ""){
			if (shouldInteractWithButton) {
				if (!gettingOut) {
					canInteract = true;
					ShowInteractionInstruction ();
				} else {
					canInteract = false;
					HideInteractionInstruction ();
				}
			} else {
				if((gettingOut && triggerExit) || (!gettingOut && triggerEnter)){
					StartTalk ();
				}
			}
		}
	}

	protected virtual void OnTriggerEnter(Collider col){
		PrepareInteraction (col.tag);
	}

	protected virtual void OnTriggerExit(Collider col){
		PrepareInteraction (col.tag,true);
	}

	protected virtual void OnTriggerEnter2D(Collider2D col){
		PrepareInteraction (col.tag);
	}

	protected virtual void OnTriggerExit2D(Collider2D col){
		PrepareInteraction (col.tag,true);
	}

	/// <summary>
	/// Hide anything that should be showing, starts a new talk and plays the timeline director
	/// </summary>
	protected virtual void StartTalk(){
		HideInteractionInstruction ();
		if (rpgtalkTarget != null) {
			NewTalk ();
		}
		if (timelineDirectorToPlay != null) {
			timelineDirectorToPlay.Play ();
		}
	}


}