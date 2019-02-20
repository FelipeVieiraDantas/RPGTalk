using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using RPGTALK.Texts;
using RPGTALK.Helper;
using RPGTALK.Localization;
using RPGTALK.Dub;



[AddComponentMenu("Seize Studios/RPGTalk/RPGTalk")]
public class RPGTalk : MonoBehaviour {

	/// <summary>
	/// Should the talk be initiated when the script starts?
	/// </summary>
	public bool startOnAwake = true;

	/// <summary>
	/// An array of objects that will be shown or hidden with the text.
	/// Usually, the canvas with the text UI is set here.
	/// </summary>
	public GameObject[] showWithDialog;
	
	/// <summary>
	/// The UI element that holds a Text component
	/// </summary>
	public Text textUI;

	/// <summary>
	/// This dialog have the name of the talker? The dialoger?
	/// </summary>
	public bool dialoger;

	/// <summary>
	/// To show the name of the talker, another UI that holds a Text component is required
	/// </summary>
	public Text dialogerUI;

	/// <summary>
	/// Should the element follow someone?
	/// </summary>
	public bool shouldFollow;

	/// <summary>
	/// Who to follow? There can be more than one to follow, based on the name of the talker
	/// </summary>
	public RPGTalkFollow[] follow;

	//Who am I following now?
	Transform following;
	Vector3 followingOffset;

	/// <summary>
	/// The objects in showWithDialog should be Billboard?
	/// </summary>
	public bool billboard = true;

	/// <summary>
	/// If billboard is set to true, should it be based on the main camera?
	/// </summary>
	public bool mainCamera = true;

	/// <summary>
	/// If billboard is set to true but not the mainCamera, should it be based on what camera?
	/// </summary>
	public Camera otherCamera;

	/// <summary>
	/// The text file that contains all the talks to be parsed.
	/// </summary>
	public TextAsset txtToParse;	

	/// <summary>
	/// If the player hits the intercation button, should the text be skipped to the end?
	/// </summary>
	public bool enableQuickSkip = true;

	/// <summary>
	/// Some script to look for a feedback when the talk is finished. Leave blank if no feedback is needed
	/// </summary>
	public MonoBehaviour callbackScript;

	/// <summary>
	/// Function to be called when the talk finishes. Will only work if some script is set into callbackScript.
	/// </summary>
	public string callbackFunction;

	/// <summary>
	/// An animator that some parameters can be set by RPGTalk to help animating while the talk is running
	/// </summary>
	public Animator animatorWhenTalking;

	/// <summary>
	/// Name of a boolean property in the animatorWhenTalking that will be set to true when the text is running.
	/// </summary>
	public string animatorBooleanName;

	/// <summary>
	/// Name of an int property in animator that represents the talker (based on the photos array).
	/// </summary>
	public string animatorIntName;

	/// <summary>
	/// Wich position of the talk are we?
	/// </summary>
	public int cutscenePosition = 0;

	/// <summary>
	/// Speed of the text, in characters per second
	/// </summary>
	public float textSpeed = 50.0f;
	/// <summary>
	/// wich character of the current line are we?
	/// </summary>
	public float currentChar = 0.0f;

	/// <summary>
	/// a list with every element of the Talk. Each element is a line on the text
	/// </summary>
	public List<RpgtalkElement> rpgtalkElements;

	/// <summary>
	/// A GameObject to blink when expecting player's intercation. It will blink by alternating the GameObject Active property.
	/// </summary>
	public GameObject blinkWhenReady;

	/// <summary>
	/// An array that can contain any variable and what is its value to be replaced in the talk
	/// </summary>
	public RPGTalkVariable[] variables;

	/// <summary>
	/// Should there be photos of the dialogers?
	/// </summary>
	public bool shouldUsePhotos;

	/// <summary>
	/// The photos and who they belong to.
	/// </summary>
	public RPGTalkPhoto[] photos;

	/// <summary>
	/// An UI element with the Image property that the photo should be applied to
	/// </summary>
	public Image UIPhoto;

	/// <summary>
	/// The dialog and everything in showWithDialog should stay on screen even if the text has ended?
	/// </summary>
	public bool shouldStayOnScreen;

	//Are we expecting a click? 
	bool lookForClick = true;

	/// <summary>
	/// Audio to be played while the character is talking
	/// </summary>
	public AudioClip textAudio;
	/// <summary>
	/// Audio to be played when player passes the Talk
	/// </summary>
	public AudioClip passAudio;
	//The AudioSource that will be used to play the SFXs above
	AudioSource rpgAudioSorce;


	/// <summary>
	/// Pass the text with mouse Click?
	/// </summary>
	public bool passWithMouse = true;

	/// <summary>
	/// Pass the text with some button set on Project Settings > Input
	/// </summary>
	public string passWithInputButton;

	/// <summary>
	/// The user can currently pass the talk?
	/// </summary>
	public bool enablePass = true;

	/// <summary>
	/// Line to start reading the text. Should not be below 1.
	/// Can be a string that the RPGTalk will look for in the text by the pattern [title=MyString]
	/// </summary>
	public string lineToStart = "1";
	/// <summary>
	/// Line to stop reading the text. If it is -1 it will read until the end of the file.
	/// Can be a string that the RPGTalk will look for in the text by the pattern [title=MyString]
	/// </summary>
	public string lineToBreak = "-1";

	//After some calculations, keep the actual line to start or break
	private int actualLineToStart;
	private int actualLineToBreak;

	/// <summary>
	/// Should the RPGTalk try to break long lines into several little ones?
	/// </summary>
	public bool wordWrap = true;
	/// <summary>
	/// If wordWrap is set to true, RPGTalk will only accept a line with maxCharInWidth * maxCharInHeight characters.
	/// If the line in the text passes it, it will be broken into another line.
	/// </summary>
	public int maxCharInWidth = 50;
	/// <summary>
	/// If wordWrap is set to true, RPGTalk will only accept a line with maxCharInWidth * maxCharInHeight characters.
	/// If the line in the text passes it, it will be broken into another line.
	/// </summary>
	public int maxCharInHeight = 4;

	//Any RichTexts around here?
	private List<RPGTalkRichText> richText;
	private List<string> unclosedTags;

	/// <summary>
	/// The sprites that can be used in this talk
	/// </summary>
	public List<RPGTalkSprite> sprites;

	/// <summary>
	/// The sprites that are being used in this talk
	/// </summary>
	public List<RPGTalkSprite> spritesUsed;

	//Any dubs around here?
	List<RPGTalkDub> dubs;
	RPGTalkDubSounds dubSounds;

	//Any speed changes around here?
	List<RPGTalkSpeed> speeds;

	//Any questions around here?
	List<RPGTalkQuestion> questions;

	/// <summary>
	/// The actual speed that the text will be scrolled. This usually is equal to textSpeed 
	/// but can be changed within the text with the [speed=X] tag
	/// </summary>
	public float actualTextSpeed;

	//Event to be called when a New Talk Start
	public delegate void NewTalkAction();
	public event NewTalkAction OnNewTalk;

	//Event to be called when a it play next line in the talk
	public delegate void PlayNextAction();
	public event PlayNextAction OnPlayNext;

	//Event to be called when a it play next line in the talk
	public delegate void EndTalkAction();
	public event EndTalkAction OnEndTalk;

	/// <summary>
	/// Is the RPGTalk currently playing the text?
	/// </summary>
	public bool isPlaying;


	/// <summary>
	/// The prefab of a Button that will be the choice in case of questions in the text
	/// </summary>
	public GameObject choicePrefab;
	/// <summary>
	/// A parent that each choice will be instantiated to in case of questions
	/// </summary>
	public Transform choicesParent;

	//Event to be called when a it play next line in the talk
	public delegate void MadeAChoiceAction(int questionID, int choiceNumber);
	public event MadeAChoiceAction OnMadeChoice;


	void Start(){
		//Set the txtToParse again, so it can check for the correct language
		if (txtToParse != null) {
			CheckCurrentLanguage ();
		}

		//If it is set to start on awake, start it! If not, make sure that we hide anything that shouldn't be there
		if (startOnAwake) {
			NewTalk ();
		} else {
			foreach (GameObject GO in showWithDialog) {
				GO.SetActive (false);
			}
		}
	}

	//Change txtToParse to be the correct for other language
	void CheckCurrentLanguage(){
		if (RPGTalkLocalization.singleton != null) {
			txtToParse = RPGTalkLocalization.singleton.CheckForCorrectLanguage (txtToParse);
		}
	}

	/// <summary>
	/// Before start a New Talk, change the values
	/// </summary>
	/// <param name="_lineToStart">Line to start reading the text.</param>
	/// <param name="_lineToBreak">Line to stop reading the text.</param>
	public void NewTalk(string _lineToStart,string _lineToBreak){
		lineToStart = _lineToStart;
		lineToBreak = _lineToBreak;
		NewTalk ();
	}

	/// <summary>
	/// Before start a New Talk, change the values
	/// </summary>
	/// <param name="_lineToStart">Line to start reading the text.</param>
	/// <param name="_lineToBreak">Line to stop reading the text.</param>
	/// <param name="_txtToParse">Text to read from</param>
	public void NewTalk(string _lineToStart,string _lineToBreak,TextAsset _txtToParse){
		lineToStart = _lineToStart;
		lineToBreak = _lineToBreak;
		txtToParse = _txtToParse;
		NewTalk ();
	}

	/// <summary>
	/// Before start a New Talk, change the values
	/// </summary>
	/// <param name="_lineToStart">Line to start reading the text.</param>
	/// <param name="_lineToBreak">Line to stop reading the text.</param>
	/// <param name="_txtToParse">Text to read from</param>
	/// <param name="_callbackScript">Script to be called when the talk ends</param>
	/// <param name="_txtToParse">Function to be called when the talk ends</param>
	public void NewTalk(string _lineToStart,string _lineToBreak,TextAsset _txtToParse, MonoBehaviour _callbackScript, string _callbackFunction){
		lineToStart = _lineToStart;
		lineToBreak = _lineToBreak;
		txtToParse = _txtToParse;
		callbackScript = _callbackScript;
		callbackFunction = _callbackFunction;
		NewTalk ();
	}

	/// <summary>
	/// Starts a new Talk.
	/// </summary>
	public void NewTalk(){
		//call the event
		if(OnNewTalk != null){
			OnNewTalk ();
		}

		//Check if we are using the right txtToParse based on the language
		CheckCurrentLanguage ();

		//check if we have the dubsounds component on
		if (dubSounds == null) {
			dubSounds = GetComponent<RPGTalkDubSounds> ();
		}


		//reduce one for the line to Start and break, if they were ints
		//return the default lines to -2 if they were not ints
		if (int.TryParse (lineToStart, out actualLineToStart)) {
			actualLineToStart -= 1;
		} else {
			actualLineToStart = -2;
		}
		if (int.TryParse (lineToBreak, out actualLineToBreak)) {
			if (lineToBreak != "-1") {
				actualLineToBreak -= 1;
			}
		} else {
			actualLineToBreak = -2;
		}


		if (textAudio != null) {
			if (rpgAudioSorce == null) {
				if (gameObject.GetComponent<AudioSource> () && gameObject.GetComponent<AudioSource> ().clip == textAudio) {
					rpgAudioSorce = gameObject.GetComponent<AudioSource> ();
				} else {
					rpgAudioSorce = gameObject.AddComponent<AudioSource> ();
				}
			}
		}

		lookForClick = true;

		//Stop any blinking arrows that shouldn't appear
		CancelInvoke ("blink");
		if (blinkWhenReady) {
			blinkWhenReady.SetActive (false);
		}


		//reset positions
		cutscenePosition = 1;
		currentChar = 0;


		//create a new CutsCeneElement
		rpgtalkElements = new List<RpgtalkElement>();

		//Resets the Rich Texts list
		richText = new List<RPGTalkRichText> ();

		//If there was any unclosed tags... Reset it
		unclosedTags = new List<string>();

		//if there was any sprites used... reset it
		spritesUsed = new List<RPGTalkSprite>();
		CleanDirtySprites ();

		//if there was any speeds used... reset it
		speeds = new List<RPGTalkSpeed>();

		//if there was any dubs used... reset it
		dubs = new List<RPGTalkDub>();

		//if there was any questions used... reset it
		questions = new List<RPGTalkQuestion>();

		//the speed at the start is the default
		actualTextSpeed = textSpeed;


		//resets any text that might have been left from previous talks
		textUI.text = "";
		
		
		if(txtToParse != null) {
			// read the TXT file into the elements list
			StringReader reader = new StringReader (txtToParse.text);
			
			string line = reader.ReadLine(); 
			int currentLine = 0;

			while (line != null) {
				//if the lineToStart or lineToBreak were strings, find out what line they actually were
				if (actualLineToStart == -2) {
					if (line == "[title="+lineToStart+"]") {
						actualLineToStart = currentLine+1;
					} else {
						line = reader.ReadLine();
						currentLine++;
						continue;
					}
				}
				if (actualLineToBreak == -2) {
					if (line == "[title="+lineToBreak+"]") {
						actualLineToBreak = currentLine-1;
					}
				}
				
				if (currentLine >= actualLineToStart) {
					if (actualLineToBreak < 0 || currentLine <= actualLineToBreak) {
						//If this line was a choice, we don't want to keep track of it
						if (LookForChoices (line)) {
							line = reader.ReadLine();
							currentLine++;
							continue;
						}

						if (wordWrap) {
							CheckIfTheTextFits (line);
						} else {
							rpgtalkElements.Add (readSceneElement (line));
						}
					} else {
						break;
					}
				}
				
				line = reader.ReadLine();
				currentLine++;
			}
				
			if(rpgtalkElements.Count == 0){
				Debug.LogError ("The Line To Start and the Line To Break are not fit for the given TXT");
				return;
			}

			//After reading all the elements in the talk, let's check if the text should be ready to fit some sprites
			if (textUI.GetComponent<TextWithIcon> ()) {
				textUI.GetComponent<TextWithIcon> ().RepopulateImages ();
			}

		}



		//show what need to be shown
		textUI.enabled = true;
		if (dialoger) {
			if (dialogerUI) {
				dialogerUI.enabled = true;
			}

		}
		for (int i = 0; i < showWithDialog.Length; i++) {
			showWithDialog[i].SetActive(true);
		}

		//Set the speaker name and photo
		if (dialoger) {
			if (dialogerUI) {
				dialogerUI.text = rpgtalkElements [0].speakerName;
			}
			if (shouldUsePhotos) {
				for (int i = 0; i < photos.Length; i++) {
					if (photos [i].name == rpgtalkElements [0].originalSpeakerName) {

						if (UIPhoto) {
							UIPhoto.sprite = photos [i].photo;
						}

						if(animatorWhenTalking && animatorIntName != ""){
							animatorWhenTalking.SetInteger (animatorIntName, i);
						}
					}
				}
			}
		}

		//Check if and who the elements should follow
		CheckWhoToFollow (rpgtalkElements [0]);



		//check if there should be any dubs in this line
		CheckDubsInThisLine();

		//if we have an animator.. play it
		if (animatorWhenTalking != null) {
			animatorWhenTalking.SetBool (animatorBooleanName, true);
		}

		isPlaying = true;
	}

	private RpgtalkElement readSceneElement(string line) {
		
		RpgtalkElement newElement = new RpgtalkElement();

		newElement.originalSpeakerName = line;

		//replace any variable that may exist on the text
		for (int i = 0; i < variables.Length; i++) {
			if (line.Contains (variables[i].variableName)) {
				line = line.Replace (variables[i].variableName, variables[i].variableValue);
			}
		}

		//If we want to show the dialoger's name, slipt the line at the ':'
		if (dialoger) {

			if (line.IndexOf (':') != -1) {

				string[] splitLine = line.Split (new char[] { ':' }, 2);

				newElement.speakerName = splitLine [0].Trim ();

				//newElement.dialogText = LookForRichTexts(splitLine [1].Trim ());
				line = splitLine [1].Trim ();

				string[] originalSplitLine = newElement.originalSpeakerName.Split (new char[] { ':' },2);

				newElement.originalSpeakerName = originalSplitLine [0].Trim ();

			}
		} 

		//Check for any question that should come along with the text
		line = LookForQuestions(line);

		//Check for any dubs that should come along with the text
		line = LookForDubs (line);

		//Check for any speed changes that should be on the text
		line = LookForSpeed(line);

		//Check for any sprites that should be on the text
		line = LookForSprites (line);

		//Check for any rich texts on the text
		line = LookForRichTexts (line);

		//Finally apply the text to the new element
		newElement.dialogText = line;
		newElement.hasDialog = true;


		
		return newElement;
		
	}



	private void CheckTextUIScript(){
		//If we are using sprites inside the text, the regular Text script need to be changed.
		if (!textUI.GetComponent<TextWithIcon> ()) {
			//Lets create a copy of the Text that the user created
			GameObject tempGO = new GameObject ();
			TextWithIcon newText = tempGO.AddComponent<TextWithIcon> ();
			RPGTalkHelper.CopyTextParameters (textUI, newText);

			//now remove the previous one
			GameObject originalGO = textUI.gameObject;
			DestroyImmediate(textUI);

			//finally, add the new text to the ancient Game Object
			textUI = originalGO.AddComponent<TextWithIcon>();
			textUI.GetComponent<TextWithIcon> ().rpgtalk = this;
			RPGTalkHelper.CopyTextParameters (newText, textUI);

			Destroy (tempGO);
		}
	}

	private string LookForSprites(string line){
		//check if the user have some sprites and the line asks for one
		if (sprites.Count > 0 && line.IndexOf("[sprite=")!=-1 && line.IndexOf("]",line.IndexOf("[sprite="))!= -1) {
			bool thereAreSpritesLeft = true;

			//There is at least one sprite in this line! Let's check if our UI uses the correct script
			CheckTextUIScript();


			//repeat as long as we find a sprite
			while (thereAreSpritesLeft) {
				int initialBracket = line.IndexOf ("[sprite=");
				int finalBracket = -1;
				if (initialBracket != -1) {
					finalBracket = line.IndexOf ("]", initialBracket);
				}

				//There still are any '[sprite=' and it is before a ']'?
				if (initialBracket < finalBracket) {
					//Ok, new sprite around! Let's get its number
					int spriteNum = -1;
					//Check if the number was a valid int
					if (int.TryParse (line.Substring (initialBracket + 8, finalBracket - (initialBracket + 8)), out spriteNum) &&
						sprites.Count > spriteNum) {
						//Neat, we definely have a sprite with a valid number. Time to keep track of it
						RPGTalkSprite newSprite = new RPGTalkSprite();
						newSprite.sprite = sprites [spriteNum].sprite;
						newSprite.width = sprites [spriteNum].width;
						newSprite.height = sprites [spriteNum].height;
						newSprite.spritePosition = initialBracket;
						//Make sure that the the sprite only work for that next line to be added to RpgTalkElements
						newSprite.lineWithSprite = rpgtalkElements.Count;
						newSprite.animator = sprites [spriteNum].animator;

						spritesUsed.Add (newSprite);

						//Looking good! We found out that a sprite should be there and we are already keeping track of it
						//But now we should remove the [sprite=X] from the line.
						//The magic here is that we will replace it with the <color=#00000000> tag and the content will be
						//a text with the length of the sprite's width. So in fact there will be text in there so the next word
						//will have the right margin, but the text will be invisible so the sprite can take its place
						string filledText = "";
						for (int i = 0; i < Mathf.CeilToInt(newSprite.width); i++) {
							//The letter "S" is used to fill because in most fonts the letter S occupies a perfect character square
							filledText += "S";
						}
							
						if (finalBracket == line.Length-1) {
							//if the sprite was the last thing on the text, we shoul place an empty space to align correctly the vertexes
							filledText += "SS";
						}
						line = line.Substring(0,initialBracket) + 
							"<color=#00000000>"+filledText+"</color>"+
							line.Substring(finalBracket+1);


					} else {
						Debug.LogWarning ("Found a [sprite=x] variable in the text but something is wrong with it. Check The spelling and check if the number used exists in the 'Sprites' section");
						thereAreSpritesLeft = false;
					}

				} else {
					thereAreSpritesLeft = false;
				}
			}
		}

		return line;
	}

	private string LookForDubs(string line){
		//check if the user have some dubs and the line asks for one
		if (line.IndexOf("[dub=")!=-1 && line.IndexOf("]",line.IndexOf("[dub="))!= -1) {
			
			bool thereAreDubsLeft = true;

			//repeat as long as we find a dub
			while (thereAreDubsLeft) {
				int initialBracket = line.IndexOf ("[dub=");
				int finalBracket = -1;
				if (initialBracket != -1) {
					finalBracket = line.IndexOf ("]", initialBracket);
				}

				//There still are any '[dub=' and it is before a ']'?
				if (initialBracket < finalBracket) {
					//Ok, new dub around! Let's get its number
					int dubNum = -1;
					//Check if the number was a valid int
					if (int.TryParse (line.Substring (initialBracket + 5, finalBracket - (initialBracket + 5)), out dubNum)) {
						//Neat, we definely have a dub with a valid number. Time to keep track of it
						RPGTalkDub newDub = new RPGTalkDub();
						newDub.dubNumber = dubNum;
						//Make sure that the the sprite only work for that next line to be added to RpgTalkElements
						newDub.lineWithDub = rpgtalkElements.Count;

						dubs.Add (newDub);

						//Looking good! We found out that a dub should be there and we are already keeping track of it
						//But now we should remove the [dub=X] from the line.
						line = line.Substring(0,initialBracket) +
							line.Substring(finalBracket+1);


					} else {
						Debug.LogWarning ("Found a [dub=x] variable in the text but something is wrong with it. Check The spelling");
						thereAreDubsLeft = false;
					}

				} else {
					thereAreDubsLeft = false;
				}
			}
		}

		return line;
	}

	private string LookForQuestions(string line){
		//check if the user have some question and the line asks for one
		if (line.IndexOf("[question=")!=-1 && line.IndexOf("]",line.IndexOf("[question="))!= -1) {
			int initialBracket = line.IndexOf ("[question=");
			int finalBracket = line.IndexOf ("]", initialBracket);

			//Ok, new question around! Let's get its number
			int questionNum = -1;
			//Check if the number was a valid int
			if (int.TryParse (line.Substring (initialBracket + 10, finalBracket - (initialBracket + 10)), out questionNum)) {
				//Neat, we definely have a question with a valid number. Time to keep track of it
				RPGTalkQuestion newQuestion = new RPGTalkQuestion();
				newQuestion.questionNum = questionNum;
				newQuestion.lineWithQuestion = rpgtalkElements.Count;

				questions.Add (newQuestion);

				//Looking good! We found out that a dub should be there and we are already keeping track of it
				//But now we should remove the [question=X] from the line.
				line = line.Substring(0,initialBracket) +
					line.Substring(finalBracket+1);


			} else {
				Debug.LogWarning ("Found a [question=x] variable in the text but something is wrong with it. Check The spelling");
			}
		}

		return line;
	}

	private bool LookForChoices(string line){
		//check if the user have some choice and the line asks for one
		if (line.IndexOf("[choice]")!=-1) {
			int initialBracket = line.IndexOf ("[choice]");

			//Ok! Let's isolate its string
			line = line.Substring(initialBracket+8);

			//Add it to the last question found
			if (questions.Count > 0) {
				questions [questions.Count - 1].choices.Add (line);
				return true;
			} else {
				Debug.LogWarning ("Found a [choice] in the text but there was no [question=x] in a line before it");
			}
		}

		return false;
	}

	private string LookForSpeed(string line){
		//check if the user have some speed changes and the line asks for one
		if ((line.IndexOf("[speed=")!=-1 && line.IndexOf("]",line.IndexOf("[speed="))!= -1) || line.IndexOf ("[/speed]") != -1) {
			bool thereAreSpeedsLeft = true;

			//There is at least one sprite in this line! Let's check if our UI uses the correct script
			CheckTextUIScript();


			//repeat as long as we find a sprite
			while (thereAreSpeedsLeft) {
				int initialBracket = line.IndexOf ("[speed=");
				int closingBracket = line.IndexOf ("[/speed]");
				int finalBracket = -1;
				if (initialBracket != -1) {
					finalBracket = line.IndexOf ("]", initialBracket);
				}

				//There still are any '[speed=' and it is before a ']'? 
				if (initialBracket < finalBracket || closingBracket != -1) {
					//Ok, new speed chang around! Let's get its number
					int speedNum = 0;

					//it was a opening [speed=
					if (closingBracket == -1 || (initialBracket < closingBracket && initialBracket != -1 && finalBracket != -1 && initialBracket < finalBracket)) {
						//Check if the number was a valid int
						if (int.TryParse (line.Substring (initialBracket + 7, finalBracket - (initialBracket + 7)), out speedNum)) {
							//Neat, we definely have a speed with a valid number. Time to keep track of it
							RPGTalkSpeed newSpeed = new RPGTalkSpeed ();
							newSpeed.speed = Mathf.Abs(speedNum);
							//subtract from the speed position any rpgtalk tag that might have come before it
							newSpeed.speedPosition = initialBracket - RPGTalkHelper.CountRPGTalkTagCharacters (line.Substring (0, initialBracket));
							newSpeed.lineWithSpeed = rpgtalkElements.Count;
							speeds.Add (newSpeed);

							//Looking good! We found out that a speed should be there and we are already keeping track of it
							//But now we should remove the [speed=X] from the line.
							line = line.Substring (0, initialBracket) +
							line.Substring (finalBracket + 1);


						} else {
							Debug.LogWarning ("Found a [speed=x] variable in the text but something is wrong with it. Check The spelling.");
							thereAreSpeedsLeft = false;
						}
					} else {
						//it was a closgin [/speed]
						RPGTalkSpeed newSpeed = new RPGTalkSpeed ();
						newSpeed.speed = 0;
						//subtract from the speed position any rpgtalk tag that might have come before it
						newSpeed.speedPosition = closingBracket - RPGTalkHelper.CountRPGTalkTagCharacters (line.Substring (0, closingBracket));
						newSpeed.lineWithSpeed = rpgtalkElements.Count;
						speeds.Add (newSpeed);

						line = line.Substring (0, closingBracket) +
							line.Substring (closingBracket + 8);
					}

				} else {
					thereAreSpeedsLeft = false;
				}
			}
		}

		return line;
	}

	private string LookForRichTexts(string line){
		//If you had any sprites added to your line... I'm sorry, you need to enable Rich Text
		if (spritesUsed.Count > 0) {
			textUI.supportRichText = true;
		}

		//check for any rich text (only it is marked as so on the UI element)
		if (textUI.supportRichText && line.IndexOf('<') != -1) {
			bool thereIsRichTextLeft = true;

			//repeat for as long as we find a tag
			while (thereIsRichTextLeft) {
				int inicialBracket = line.IndexOf ('<');
				int finalBracket = line.IndexOf ('>');
				//Here comes the tricky part... First check if there are any '<' before a '>'
				if (inicialBracket < finalBracket) {
					//Ok, there is! It should be a tag. But first let's check if it isn't a closing one
					//This could happen because the text was automatically clipped with word wrap to fit the UI
					if (line.Substring (inicialBracket + 1, 1) == "/") {
						//Oh! It is a closing tag! Who would say?
						//If there wasn't some unclosed tags in some other line in this talk, it was just a mistake.
						if (unclosedTags.Count == 0) {
							thereIsRichTextLeft = false;
						}
						//Let's check the openned tags in previous lines.
						for (int i = unclosedTags.Count-1; i >= 0; i--) {
							line = unclosedTags [i] + line;
						}
						//After that... Let's reset the unclosed tags, shall we? No infinity loops wanted
						unclosedTags = new List<string>();

						//Cool, we openned the tags... Let's try this search again
						inicialBracket = line.IndexOf ('<');
						finalBracket = line.IndexOf ('>');
					}


					//Ok, we got an openning tag. Let's found out the name of it
					int endOfTag = line.IndexOf (' ', inicialBracket);
					//Let's check if the tag ends (>) before a ' ' is found
					if (finalBracket < endOfTag || endOfTag == -1) {
						endOfTag = finalBracket;
					}
					//Now let's check if there was an '=' before the '>' or ' '.
					int equalSign = line.IndexOf ('=', inicialBracket);
					if (equalSign < endOfTag && equalSign != -1) {
						endOfTag = equalSign;
					}

					string tagName = line.Substring (inicialBracket + 1, endOfTag - inicialBracket - 1);
					//Good! We know the tag name. Now let's find its closing point
					string closedTag = "</" + tagName + '>';
					int closedTagLine = line.IndexOf (closedTag, finalBracket);

					if(closedTagLine == -1){
						//Well would you look at that... We found a tag, but not its closing point (</tag>)... What to do?
						//This could have happened because the text was automatically clipped with word wrap to fit the UI,
						//Or you just forgot to put a </tag> somewhere...
						//Anyway, we will forcelly add the closing tag at the end of the line
						//And keep track of it, so if the tag is closed in another line, we know how it started
						line += closedTag;
						unclosedTags.Add (line.Substring (inicialBracket, finalBracket-inicialBracket+1));

						closedTagLine = line.IndexOf (closedTag, finalBracket);
					}



					//Ok, we found (or forcelly added) the closing tag, there is definely a rich text here.
					//Let's add it to a list so we can read later on.
					RPGTalkRichText newRichText = new RPGTalkRichText();
					newRichText.initialTagPosition = inicialBracket;
					newRichText.initialTag = line.Substring (inicialBracket, finalBracket-inicialBracket+1);
					newRichText.finalTagPosition = closedTagLine;
					newRichText.finalTag = closedTag;
					//Make sure that the the rich text only work for that next line to be added to RpgTalkElements
					newRichText.lineWithTheRichText = rpgtalkElements.Count;
					richText.Add (newRichText);

					//Good! Now finaly, remove it from the original text
					string textWithoutRichText = line.Substring (0, inicialBracket);
					textWithoutRichText += line.Substring (finalBracket + 1, closedTagLine - finalBracket - 1);
					textWithoutRichText += line.Substring (closedTagLine + closedTag.Length);
					line = textWithoutRichText;

				} else {
					thereIsRichTextLeft = false;
				}
			}

			return line;
		}

		return line;
	}

	
	// Update is called once per frame
	void Update () {
		//We don't want to do nothing if the text isn't even showing
		if (!textUI.gameObject.activeInHierarchy) {
			return;
		}

		//Make sure that anything that should follow is following and is billboarding
		if(following != null && shouldFollow){
			Vector3 newPos = following.position + followingOffset;
			Quaternion newRotation = following.rotation;
			if (billboard) {
				if (mainCamera) {
					newRotation = Camera.main.transform.rotation;
				} else {
					newRotation = otherCamera.transform.rotation;
				}
			}

			for (int i = 0; i < showWithDialog.Length; i++) {
				showWithDialog[i].transform.position = newPos;
				showWithDialog [i].transform.rotation = newRotation;
			}

		}


		if (textUI.enabled &&
			currentChar >= rpgtalkElements [cutscenePosition - 1].dialogText.Length) {
			//if we hit the end of the talk, but we should stay on screen, return.
			//but if we have a callback, he can click on it once more.
			if (cutscenePosition >= rpgtalkElements.Count && shouldStayOnScreen) {
				if(lookForClick && (
					(passWithMouse && Input.GetMouseButtonDown (0)) ||
					(passWithInputButton != "" && Input.GetButtonDown(passWithInputButton))
				)){
					//if have an audio... playit
					if (passAudio != null && !rpgAudioSorce.isPlaying) {
						rpgAudioSorce.clip = passAudio;
						rpgAudioSorce.Play ();
					}
					if(callbackScript != null){
						callbackScript.Invoke(callbackFunction,0f);
						//Stop any blinking arrows that shouldn't appear
						CancelInvoke ("blink");
						if (blinkWhenReady) {
							blinkWhenReady.SetActive (false);
						}
					}
					lookForClick = false;
				}

				CancelInvoke ("blink");
				if (blinkWhenReady) {
					blinkWhenReady.SetActive (false);
				}
				return;
			}

			//if we reached the end of the line and click on the screen...
			if (
				enablePass && (
				(passWithMouse && Input.GetMouseButtonDown (0)) ||
				(passWithInputButton != "" && Input.GetButtonDown(passWithInputButton))
				)
			) {//if have an audio... playit
				if (passAudio != null) {
					rpgAudioSorce.clip = passAudio;
					rpgAudioSorce.Play ();
				}
				textUI.enabled = false;
				PlayNext ();

			}
			return;
		}




		//if we're currently showing dialog, then start scrolling it
		if(textUI.enabled) {
			// if there's still text left to show
			if(currentChar < rpgtalkElements[cutscenePosition - 1].dialogText.Length) {

				//ensure that we don't accidentally blow past the end of the string
				currentChar = Mathf.Min(currentChar + actualTextSpeed * Time.deltaTime,
					rpgtalkElements[cutscenePosition - 1].dialogText.Length);

				//Do what we have to do if the the text just ended
				if(currentChar >= rpgtalkElements[cutscenePosition - 1].dialogText.Length){
					TextEnded();
				}

				//Get the current char and the text and put it into the U
				PutRightTextToShow ();


			} 
			
			if(enableQuickSkip == true &&
				(
					(passWithMouse && Input.GetMouseButtonDown (0)) ||
					(passWithInputButton != "" && Input.GetButtonDown(passWithInputButton))
				)
				&& currentChar > 3) {

				currentChar = rpgtalkElements[cutscenePosition - 1].dialogText.Length;
				PutRightTextToShow ();
				//Do what we have to do if the the text just ended
				TextEnded();
			}

		




			
		}


	}

	//The text just ended to be written on the screen
	void TextEnded(){
		if (enablePass) {
			blink ();
		}

		//if we have an animator.. stop it
		if (animatorWhenTalking != null) {
			animatorWhenTalking.SetBool (animatorBooleanName, false);
		}

		//Check if this text was a question
		if (questions.Count > 0) {
			if (choicePrefab == null) {
				Debug.LogError ("There was a question here, but no object was set in choicePrefab to be the answer");
				return;
			}

			foreach (RPGTalkQuestion q in questions) {
				if(q.lineWithQuestion == cutscenePosition-1 && !q.alreadyHappen){
					//This line was a question! Put the correct answers here
					enablePass = false;
						
					for (int i = 0; i < q.choices.Count; i++) {
						GameObject newChoice = (GameObject)Instantiate (choicePrefab, choicesParent);
						if (newChoice.GetComponent<Button> ()) {
							newChoice.GetComponentInChildren<Text> ().text = q.choices[i];
							int choiceNumber = i;
							newChoice.GetComponent<Button> ().onClick.AddListener (delegate{MadeAChoice (q.questionNum, choiceNumber);});
							if (i == 0) {
								newChoice.GetComponent<Button> ().Select ();
							}
						} else {
							Debug.LogWarning ("RPGTalk can only put the choice's text correctly if choicePrefab is a button with a child of type Text.");
						}
					}
					break;
				}
			}

		}
	}

	/// <summary>
	/// Function to be called by the buttons when the user makes a choice.
	/// This passes the talk and call the OnMadeChoice event
	/// </summary>
	public void MadeAChoice(int questionID, int choiceNumber){
		foreach (RPGTalkQuestion q in questions) {
			if (q.questionNum == questionID) {
				q.alreadyHappen = true;
			}
		}
		enablePass = true;
		PlayNext ();
		if (OnMadeChoice != null) {
			OnMadeChoice (questionID, choiceNumber);
		}
		//delete all the buttons (and other childs) in the buttons parent
		foreach (Transform child in choicesParent) {
			Destroy (child.gameObject);
		}
	}



	/// <summary>
	/// Given the current character, look for variables or rich text and put everything in the textUI.
	/// </summary>
	public void PutRightTextToShow(){
		//ensure that we don't accidentally blow past the end of the string
		currentChar = Mathf.Min(currentChar, rpgtalkElements[cutscenePosition - 1].dialogText.Length);

		//Did we reach the point where a speed should be changed?
		for (int i = 0; i < speeds.Count; i++) {
			if (speeds [i].speedPosition <= currentChar && !speeds [i].alreadyGone && speeds [i].lineWithSpeed == cutscenePosition - 1) {
				//Change the actual speed of the text. if it is 0 or below, change back to the default
				actualTextSpeed = speeds [i].speed;
				if (actualTextSpeed <= 0) {
					actualTextSpeed = textSpeed;
				}
				speeds [i].alreadyGone = true;

				//Change currentChar to the position of the [speed] tag so no word gets jumped
				currentChar = speeds [i].speedPosition;


			}
		}
			
		//Select the right text to display
		string textToDisplay = rpgtalkElements[cutscenePosition - 1].dialogText.Substring(0, (int)currentChar);

		//Check if there should be any rich text text beggining or ending at this position
		//Check from the bottom, because a tag might have been openned inside another
		for (int i = richText.Count-1; i >= 0; i--) {
			if (richText[i].lineWithTheRichText != cutscenePosition - 1 || currentChar < richText [i].initialTagPosition) {
				continue;
			}
			string beforeTextToDisplay = textToDisplay;
			textToDisplay = textToDisplay.Substring (0,richText [i].initialTagPosition);
			textToDisplay += richText [i].initialTag;

			if (currentChar + richText [i].initialTag.Length >= richText [i].finalTagPosition) {
				textToDisplay += beforeTextToDisplay.Substring (richText [i].initialTagPosition, 
					richText [i].finalTagPosition-richText [i].initialTagPosition-richText [i].initialTag.Length);
				textToDisplay += richText [i].finalTag;
				textToDisplay += beforeTextToDisplay.Substring (richText[i].finalTagPosition - richText[i].initialTag.Length);
			} else {
				textToDisplay += beforeTextToDisplay.Substring(richText [i].initialTagPosition); 
				textToDisplay += richText [i].finalTag;
			}

		}


		textToDisplay = textToDisplay.Replace("\\n","\n");
		//Put the text in the UI
		textUI.text = textToDisplay;

		//if have an audio... playit
		if (textAudio != null && !rpgAudioSorce.isPlaying) {
			rpgAudioSorce.clip = textAudio;
			rpgAudioSorce.Play ();
		}

		//Keep the amount of rich text in the string so we can count them later on
		int RichTextUntilNow = RPGTalkHelper.CountRichTextCharacters(textToDisplay);

		//Did we reach the point where an image should be shown?
		for (int i = 0; i < spritesUsed.Count; i++) {
			if (spritesUsed [i].spritePosition <= currentChar + RichTextUntilNow && !spritesUsed [i].alreadyInPlace && spritesUsed[i].lineWithSprite == cutscenePosition - 1) {
				spritesUsed [i].alreadyInPlace = textUI.GetComponent<TextWithIcon> ().FitImagesOnText (i);
			}
		}


		//check again, at the end of the text, if there should be any images on it
		if (currentChar >= rpgtalkElements [cutscenePosition - 1].dialogText.Length) {
			for (int i = 0; i < spritesUsed.Count; i++) {
				if (spritesUsed[i].lineWithSprite == cutscenePosition - 1 && !spritesUsed[i].alreadyInPlace) {
					StartCoroutine (TryToPutImageOnText (i));
				}
			}
		}


	}

	//Tries to force image to be put on the text. If it cant, wait half a second so that the text mesh can be updated
	IEnumerator TryToPutImageOnText(int spriteToUse){
		spritesUsed [spriteToUse].alreadyInPlace = textUI.GetComponent<TextWithIcon> ().FitImagesOnText (spriteToUse);
		if(!spritesUsed [spriteToUse].alreadyInPlace){
			yield return new WaitForSeconds (0.2f);
			if(spritesUsed[spriteToUse].lineWithSprite == cutscenePosition - 1){
				spritesUsed [spriteToUse].alreadyInPlace = textUI.GetComponent<TextWithIcon> ().FitImagesOnText (spriteToUse);
			}
		}
	}

	void CheckDubsInThisLine(){
		for (int i = 0; i < dubs.Count; i++) {
			if (dubs [i].lineWithDub == cutscenePosition - 1) {
				if (dubSounds == null) {
					Debug.LogError ("A dub was found in this line but there is no RPGTalkDubSounds component added to the object");
					return;
				}
				dubSounds.PlayDubClip (dubs [i].dubNumber);
			}
		}
	}

	void blink(){
		if (blinkWhenReady) {
			CancelInvoke ("blink");
			blinkWhenReady.SetActive (!blinkWhenReady.activeInHierarchy);
			Invoke ("blink", .5f);
		}
	}

	void CheckIfTheTextFits(string line){

		int maxCharsOnUI = maxCharInWidth * maxCharInHeight;
		if (line.Length > maxCharsOnUI) {

			//how many talks would be necessary to fit this text?
			int howMuchMore = Mathf.CeilToInt((float)line.Length / (float)maxCharsOnUI);
			string newLine = "";
			int cuttedInSpace = -1;

			for (int i = 0; i < howMuchMore; i++) {
				//get the characeter that we should start saying
				int startChar = i * maxCharsOnUI;
				if(cuttedInSpace != -1){
					startChar = cuttedInSpace;
					cuttedInSpace = -1;
				}


				//if the new line fits the talk...
				if (line.Substring (startChar, 
					line.Length - (startChar)).Length < maxCharsOnUI) {
					newLine = line.Substring (startChar, 
						line.Length - (startChar));
				} else {
					//if it not, search for spaces near to the last word and cut it
					cuttedInSpace = line.IndexOf (" ", startChar+ (maxCharsOnUI - 20));
					if(cuttedInSpace != -1){
						newLine = line.Substring (startChar, cuttedInSpace-startChar);
					}else{
						newLine = line.Substring (startChar, maxCharsOnUI);
					}
				}

				rpgtalkElements.Add (readSceneElement (newLine));
			}
		} else {

			rpgtalkElements.Add (readSceneElement (line));
		}
	}


	/// <summary>
	/// Finish the talk, skipping every dialog. The callback function will still going to be called
	/// </summary>
	/// <param name="jumpQuestions">If set to <c>true</c> goes to the end of the talk, even if there were questions between it</param>
	public void EndTalk(bool jumpQuestions = false) {
		if (textUI && textUI.gameObject.activeInHierarchy) {
			cutscenePosition = rpgtalkElements.Count - 1;
			if (!shouldStayOnScreen) {
				CleanDirtySprites ();
				cutscenePosition = rpgtalkElements.Count;
			}

			if(!jumpQuestions){
				//Look for questions that hasn't already happen
				foreach(RPGTalkQuestion q in questions){
					if (!q.alreadyHappen) {
						cutscenePosition = q.lineWithQuestion;
						break;
					}
				}
			}

			PlayNext (true);
		}
	}



	/// <summary>
	/// Plays the next dialog in the current Talk.
	/// </summary>
	/// <param name="forcePlay">If set to <c>true</c> play the next talk in line even if "enablePass" is false.</param>
	public void PlayNext(bool forcePlay = false) {
		if (!enablePass && !forcePlay) {
			return;
		}
		//call the event
		if(OnPlayNext != null){
			OnPlayNext ();
		}


		//stop any dubs
		if (dubSounds != null) {
			dubSounds.StopCurrentDub ();
		}

		// increment the cutscene counter
		cutscenePosition++;
		currentChar = 0;

		//if there was any sprite in the text, deactivate it
		if (spritesUsed.Count > 0) {
			foreach (Transform child in textUI.transform) {
				child.gameObject.SetActive (false);
			}
		}

		CancelInvoke ("blink");
		if (blinkWhenReady) {
			blinkWhenReady.SetActive (false);
		}
		
		if(cutscenePosition <= rpgtalkElements.Count) {

			textUI.enabled = true;
			
			RpgtalkElement currentRpgtalkElement = rpgtalkElements[cutscenePosition - 1];

			if (dialoger) {
				if (dialogerUI) {
					dialogerUI.enabled = true;

					dialogerUI.text = currentRpgtalkElement.speakerName;
				}

				if (shouldUsePhotos) {
					for (int i = 0; i < photos.Length; i++) {
						if (photos [i].name == currentRpgtalkElement.originalSpeakerName) {
							if (UIPhoto) {
								UIPhoto.sprite = photos [i].photo;
							}
							if(animatorWhenTalking && animatorIntName != ""){
								animatorWhenTalking.SetInteger (animatorIntName, i);

							}
						}
					}
				}
			}

			CheckWhoToFollow (currentRpgtalkElement);


			//check if there should be any dubs in this line
			CheckDubsInThisLine();


			//if we have an animator.. play it
			if (animatorWhenTalking != null) {
				animatorWhenTalking.SetBool (animatorBooleanName, true);
			}




		} else {
			//The talk has finished

			//call the event
			if(OnEndTalk != null){
				OnEndTalk ();
			}

			isPlaying = false;

			if (!shouldStayOnScreen) {
				textUI.enabled = false;
				if (dialoger) {
					if (dialogerUI) {
						dialogerUI.enabled = false;
					}
				}
				for (int i = 0; i < showWithDialog.Length; i++) {
					showWithDialog [i].SetActive (false);
				}
			}

			if(callbackScript != null){
				callbackScript.Invoke(callbackFunction,0f);
			}
		}

		
	}

	void CleanDirtySprites(){
		foreach (Transform child in textUI.transform) {
			DestroyImmediate (child.gameObject);
			StopCoroutine (TryToPutImageOnText (0));
		}
		if (textUI.GetComponent<TextWithIcon> ()) {
			textUI.GetComponent<TextWithIcon> ().RepopulateImages ();
		}
	}


	void CheckWhoToFollow(RpgtalkElement element){
		//Set it to follow someone
		//resets anyone that is being followed
		following = null;
		followingOffset = Vector3.zero;
		if (shouldFollow && follow.Length > 0) {
			foreach (RPGTalkFollow character in follow) {
				//if the character in the follow array have the same name as the talker or an empty name, follow it!
				if(character.name == element.speakerName || 
					character.name == ""){
					following = character.follow;
					followingOffset = character.followOffset;
				}
			}
		}
	}

}
