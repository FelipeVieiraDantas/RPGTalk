using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using RPGTALK.Snippets;

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
	public KeyCode interactionKey = KeyCode.None;
    /// <summary>
    /// What button, set in the Input Settings, should interact with the area
    /// </summary>
    public string interactionButton;
    /// <summary>
    /// Should the are interact when the player clicks with the left button of the mouse?
    /// </summary>
    public bool interactWithMouse;
    bool canInteract;
	/// <summary>
	/// The talk will only begin if someone with this tag hits it. Leave blank to accept anyone
	/// </summary>
	public string checkIfColliderHasTag = "";
    /// <summary>
    /// A series of events to be called before the talk starts
    /// </summary>
    public UnityEvent callbackBeforeTalk;
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
    /// Overwrite the variable callback on RPGTalk. Leave empty if no ovewrite is wanted
    /// </summary>
    public UnityEvent overwriteCallbackAfterTalk;
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
    /// Save (With RPGTAlkSaveInstance on RPGTalk Holder), if this area has already happened. The name of the object will be the saveData value.
    /// </summary>
    public bool saveAlreadyHappened;
    /// <summary>
    /// Should the talk pass itself?
    /// </summary>
    public bool autoPass = false;
    /// <summary>
    /// How many seconds should RPGTalk wait after the animation stopped to autoPass
    /// </summary>
    public float secondsAutoPass = 3f;

    ///<summary>
    /// If it has a RPGTalkFollowCharacter, should it be contained inside the screen?
    /// </summary>
    public bool containInsideScreen;

    /// <summary>
    /// Hide anything that shouldn't be showing upon the start
    /// </summary>
    protected virtual void Start () {
		HideInteractionInstruction ();
        if (saveAlreadyHappened)
        {
            if (rpgtalkTarget.saveInstance)
            {
                alreadyHappened = rpgtalkTarget.saveInstance.GetSavedData(name, 1);
            }
        }
    }
	
	/// <summary>
	/// Check for the interaction. Override this method to implement your own rules
	/// </summary>
	protected virtual void Update () {
		if (shouldInteractWithButton && canInteract) {
			if ((interactionKey != KeyCode.None && Input.GetKeyDown (interactionKey)) || 
            (interactionButton != "" && Input.GetButtonDown(interactionButton)) || 
                (interactWithMouse && Input.GetMouseButtonDown(0)) ) {
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
        if (saveAlreadyHappened)
        {
            if (rpgtalkTarget.saveInstance)
            {
                rpgtalkTarget.saveInstance.SaveData(name, 1);
            }
        }


        callbackBeforeTalk.Invoke();
		TextAsset newTxt = rpgtalkTarget.txtToParse;
		if (txtToParse != null) {
			newTxt = txtToParse;
		}

		rpgtalkTarget.shouldStayOnScreen = shouldStayOnScreen;
        rpgtalkTarget.autoPass = autoPass;
        rpgtalkTarget.secondsAutoPass = secondsAutoPass;

        RPGTalkFollowCharacter followCharacter = rpgtalkTarget.GetComponent<RPGTalkFollowCharacter>();
        if (followCharacter)
        {
            followCharacter.containInsideScreen = containInsideScreen;
        }

        rpgtalkTarget.NewTalk (lineToStart, lineToBreak, newTxt, overwriteCallbackAfterTalk);
		
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