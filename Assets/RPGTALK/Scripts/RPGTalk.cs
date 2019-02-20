using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Events;
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
    /// The object setted by the user to see if we can obtain a TMP_Translator out of it
    /// </summary>
    public GameObject textUIObj;

    /// <summary>
    /// The UI element that holds a Text component. TMP_Translator deal with differences in regular UI to TMP
    /// </summary>
    public TMP_Translator textUI;

    /// <summary>
    /// This dialog have the name of the talker? The dialoger?
    /// </summary>
    public bool dialoger;

    /// <summary>
    /// The object setted by the user to see if we can obtain a TMP_Translator out of it
    /// </summary>
    public GameObject dialogerObj;

    /// <summary>
    /// To show the name of the talker, another UI. TMP_Translater deal with differences in regular UI to TMP
    /// </summary>
    public TMP_Translator dialogerUI;

    /// <summary>
    /// Should the element follow someone?
    /// </summary>
    public bool shouldFollow;

    /// <summary>
    /// Who am I currently following
    /// </summary>
    public Transform following;
    /// <summary>
    /// With what offset am I following someone
    /// </summary>
    public Vector3 followingOffset;

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
    /// You can assign events to be called when the talk ends
    /// </summary>
    public UnityEvent callback;

    /// <summary>
    /// An animator that some parameters can be set by RPGTalk to help animating while the talk is running
    /// </summary>
    public Animator animatorWhenTalking;

    /// <summary>
    /// The actual animator. This can be different from animatorWhenTalking if this options was set on the Character Settings option.
    /// </summary>
    public Animator actualAnimator;

    /// <summary>
    /// Name of a boolean property in the animatorWhenTalking that will be set to true when the text is running.
    /// </summary>
    public string animatorBooleanName;

    /// <summary>
    /// Name of an int property in animator that represents the talker (based on the characters array).
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
    /// An array that can contain any variable and what is its value to be replaced in the talk
    /// </summary>
    public RPGTalkVariable[] variables;

    /// <summary>
    /// Should there be photos of the dialogers?
    /// </summary>
    public bool shouldUsePhotos;

    /// <summary>
    /// A list of all Characters available in the talks with settings of stuff that should be on the scene
    /// </summary>
    public RPGTalkCharacterSettings[] characters;

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
    /// What is the key that should be used to interact? You can override the Update function and write your 
    /// own conditions, if needed
    /// </summary>
    public KeyCode passWithKey = KeyCode.None;

    /// <summary>
    /// The user can currently pass the talk?
    /// </summary>
    public bool enablePass = true;

    /// <summary>
    /// Should the talk pass itself?
    /// </summary>
    public bool autoPass = false;
    /// <summary>
    /// How many seconds should RPGTalk wait after the animation stopped to autoPass
    /// </summary>
    public float secondsAutoPass = 3f;

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

    /// <summary>
    /// The sprite atlas from Text Mesh Pro that should be used in the text
    /// </summary>
    public string tmpSpriteAtlas = "Default Sprite Asset";

    //Any dubs around here?
    List<RPGTalkDub> dubs;
    RPGTalkDubSounds dubSounds;

    //Any speed changes around here?
    List<RPGTalkSpeed> speeds;

    //Any questions around here?
    List<RPGTalkQuestion> questions;

    //Any jitter changes around here?
    List<RPGTalkJitter> jitters;
    Coroutine jitterRoutine;

    /// <summary>
    /// The actual speed that the text will be scrolled. This usually is equal to textSpeed 
    /// but can be changed within the text with the [speed=X] tag
    /// </summary>
    public float actualTextSpeed;

    //Event to be called when a New Talk Start
    public delegate void NewTalkAction();
    public event NewTalkAction OnNewTalk;

    //Event to be called when RPGTalk play next line in the talk
    public delegate void PlayNextAction();
    public event PlayNextAction OnPlayNext;

    //Event to be called when a talk ends
    public delegate void EndTalkAction();
    public event EndTalkAction OnEndTalk;

    //Event to be called when a line finish animating
    public delegate void EndAnimatingAction();
    public event EndAnimatingAction OnEndAnimating;

    /// <summary>
    /// Is the RPGTalk currently playing the text?
    /// </summary>
    public bool isPlaying;

    /// <summary>
    /// Is the RPGTalk currently animating the text?
    /// </summary>
    public bool isAnimating;


    /// <summary>
    /// The prefab of a Button that will be the choice in case of questions in the text
    /// </summary>
    public GameObject choicePrefab;
    /// <summary>
    /// A parent that each choice will be instantiated to in case of questions
    /// </summary>
    public Transform choicesParent;

    //Event to be called when a it play next line in the talk
    public delegate void MadeAChoiceAction(string questionID, int choiceNumber);
    public event MadeAChoiceAction OnMadeChoice;

    /// <summary>
    /// The Expression that this character is expressing
    /// </summary>
    public Expression expressing;

    //if we will change talks in the middle our talk, these variables will be set
    string changeToStart;
    string changeToBreak;

    List<RPGtalkSaveStatement> saves;
    /// <summary>
    /// The RPGTalkSaveInstance element, if there is any
    /// </summary>
    public RPGTalkSaveInstance saveInstance;


    void Start(){
        //Get the TMP_Translate Object
        textUI = new TMP_Translator(textUIObj);
        if (dialogerObj != null)
        {
            dialogerUI = new TMP_Translator(dialogerObj);
        }


        //If it is set to start on awake, start it! If not, make sure that we hide anything that shouldn't be there
        if (startOnAwake) {
            NewTalk ();
        } else {
            foreach (GameObject GO in showWithDialog) {
                GO.SetActive (false);
            }
        }

        saveInstance = GetComponent<RPGTalkSaveInstance>();
    }

    //Change txtToParse to be the correct for other language
    TextAsset CheckCurrentLanguage(){
        if (RPGTalkLocalization.singleton != null) {
            return RPGTalkLocalization.singleton.CheckForCorrectLanguage (txtToParse);
        }
        return txtToParse;
    }

    void CreateAudioSource()
    {
        AudioSource aS = gameObject.GetComponent<AudioSource>();
        if (aS && aS.clip == textAudio)
        {
            rpgAudioSorce = aS;
        }
        else
        {
            rpgAudioSorce = gameObject.AddComponent<AudioSource>();
        }
    }

    #region newtalk

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
    /// <param name="_callback">Events to be called when the talk ends</param>
    public void NewTalk(string _lineToStart,string _lineToBreak,TextAsset _txtToParse, UnityEvent _callback){
        lineToStart = _lineToStart;
        lineToBreak = _lineToBreak;
        txtToParse = _txtToParse;
        callback = _callback;
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
        TextAsset internalTxtToParse = CheckCurrentLanguage ();

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
                CreateAudioSource();
            }
        }

        lookForClick = true;

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

        //Zero saves
        saves = new List<RPGtalkSaveStatement>();

        //The jitters that could be in this lines
        jitters = new List<RPGTalkJitter>();


        //resets any text that might have been left from previous talks
        if(textUI == null)
        {
            if(textUIObj == null)
            {
                Debug.LogError("You need to set an UI Element to be the text!");
            }
            else
            {
                textUI = new TMP_Translator(textUIObj);
            }
        }
        textUI.ChangeTextTo("");
        
        
        if(internalTxtToParse != null) {
            // read the TXT file into the elements list
            StringReader reader = new StringReader (internalTxtToParse.text);
            
            string line = reader.ReadLine(); 
            int currentLine = 0;

            if(line == null)
            {
                Debug.LogError("There was an error reading your file! Check your encoding settings.");
                EndTalk();
                return;
            }

            while (line != null) {
                //if the lineToStart or lineToBreak were strings, find out what line they actually were
                if (actualLineToStart == -2) {
                    if (line.IndexOf("[title="+lineToStart+"]") != -1) {
                        actualLineToStart = currentLine+1;
                    } else {
                        line = reader.ReadLine();
                        currentLine++;
                        continue;
                    }
                }
                if (actualLineToBreak == -2) {
                    if (line.IndexOf("[title="+lineToBreak+"]") != -1) {
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
                        //If this line was a save, we don't want to keep track of it
                        if (LookForSave(line))
                        {
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
            if (textUIObj.GetComponent<ITextWithIcon> () != null) {
                textUIObj.GetComponent<ITextWithIcon> ().RepopulateImages ();
            }

        }



        //show what need to be shown
        textUI.Enabled(true);
        if (dialoger) {
            if (dialogerObj) {
                dialogerUI.Enabled(true);
            }

        }
        for (int i = 0; i < showWithDialog.Length; i++) {
            showWithDialog[i].SetActive(true);
        }

        //Set the speaker name and photo
        if (dialoger) {
            if (dialogerObj) {
                dialogerUI.ChangeTextTo(rpgtalkElements [0].speakerName);
            }
            if (shouldUsePhotos) {
                for (int i = 0; i < characters.Length; i++) {
                    //If we fond the character that is talking
                    if (characters[i].character.dialoger == rpgtalkElements [0].originalSpeakerName) {
                        //Change its photo
                        if (UIPhoto) {
                            UIPhoto.sprite = characters [i].character.photo;
                        }
                        //Change its animator
                        if(characters[i].animatorOverwrite != null)
                        {
                            actualAnimator = characters[i].animatorOverwrite;
                        }
                        else
                        {
                            actualAnimator = animatorWhenTalking;
                        }
                        //animate it
                        if (actualAnimator && animatorIntName != ""){
                            actualAnimator.SetInteger (animatorIntName, i);
                        }
                        break;
                    }
                }
            }
        }

        //Check if and who the elements should follow
        CheckWhoToFollow (rpgtalkElements [0]);



        //check if there should be any dubs in this line
        CheckDubsInThisLine();

        //check if we are expressing something
        expressing = IsExpressing(rpgtalkElements[0]);

        //if we have an animator.. play it
        PlayAnimator(rpgtalkElements[0]);

        //check if after this line we should start another talk
        rpgtalkElements[0].dialogText = LookForNewTalk(rpgtalkElements[0].dialogText);

        isPlaying = true;
        isAnimating = true;
    }

    #endregion

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

        //Check for any expressions to play with the text
        newElement.expression = LookForExpression(line);
        line = LookForExpression(line, true);

        //Check for any jitters on the text
        line = LookForJitter(line);


        //Finally apply the text to the new element
        newElement.dialogText = line;
        newElement.hasDialog = true;



        
        return newElement;
        
    }

    #region sprites

    private void CheckTextUIScript(){
        //If we are using sprites inside the text, the regular Text script need to be changed.
        if (textUIObj.GetComponent<ITextWithIcon> () == null && textUI.hasUIText) {
            //Lets create a copy of the Text that the user created
            GameObject tempGO = new GameObject ();
            ITextWithIcon newText = textUI.AddTextWithIconComponent(tempGO);
            RPGTalkHelper.CopyTextParameters (textUI.GetTextObject(), newText as Object);

            //now remove the previous one
            DestroyImmediate(textUI.GetTextObject());

            //finally, add the new text to the ancient Game Object
            textUI.AddTextWithIconComponent(textUIObj);
            textUI = new TMP_Translator( textUIObj);
            textUIObj.GetComponent<ITextWithIcon> ().rpgtalk = this;
            RPGTalkHelper.CopyTextParameters (newText as Object, textUI.GetTextObject());

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

                        //Change the line differently if we have TMP
                        line = textUI.GetCorrectSpriteLine(line, ref sprites, ref spritesUsed, spriteNum, initialBracket, finalBracket, rpgtalkElements.Count, tmpSpriteAtlas);


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


    void CleanDirtySprites()
    {
        if (textUI == null)
        {
            return;
        }
        foreach (Transform child in textUIObj.transform)
        {
            DestroyImmediate(child.gameObject);
            StopCoroutine(TryToPutImageOnText(0));
        }
        if (textUIObj.GetComponent<ITextWithIcon>() != null)
        {
            textUIObj.GetComponent<ITextWithIcon>().RepopulateImages();
        }
    }

    //Tries to force image to be put on the text. If it cant, wait half a second so that the text mesh can be updated
    IEnumerator TryToPutImageOnText(int spriteToUse)
    {
        spritesUsed[spriteToUse].alreadyInPlace = textUIObj.GetComponent<ITextWithIcon>().FitImagesOnText(spriteToUse);
        if (!spritesUsed[spriteToUse].alreadyInPlace)
        {
            yield return new WaitForSeconds(0.2f);
            if (spritesUsed[spriteToUse].lineWithSprite == cutscenePosition - 1)
            {
                spritesUsed[spriteToUse].alreadyInPlace = textUIObj.GetComponent<ITextWithIcon>().FitImagesOnText(spriteToUse);
            }
        }
        yield return null;
    }

    #endregion

    #region dubs

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

    void CheckDubsInThisLine()
    {
        for (int i = 0; i < dubs.Count; i++)
        {
            if (dubs[i].lineWithDub == cutscenePosition - 1)
            {
                if (dubSounds == null)
                {
                    Debug.LogError("A dub was found in this line but there is no RPGTalkDubSounds component added to the object");
                    return;
                }
                dubSounds.PlayDubClip(dubs[i].dubNumber);
            }
        }
    }

    #endregion

    #region questions

    private string LookForQuestions(string line){
        //check if the user have some question and the line asks for one
        if (line.IndexOf("[question=")!=-1 && line.IndexOf("]",line.IndexOf("[question="))!= -1) {
            int initialBracket = line.IndexOf ("[question=");
            int finalBracket = line.IndexOf ("]", initialBracket);

            //Ok, new question around! Let's get its id
            string questionID = line.Substring(initialBracket + 10, finalBracket - (initialBracket + 10));
            if (questionID.Length > 0) {
                //Neat, we definely have a question with a valid number. Time to keep track of it
                RPGTalkQuestion newQuestion = new RPGTalkQuestion();
                newQuestion.questionID = questionID;
                newQuestion.lineWithQuestion = rpgtalkElements.Count;

                questions.Add (newQuestion);

                //Looking good! We found out that a question should be there and we are already keeping track of it
                //But now we should remove the [question=X] from the line.
                line = line.Substring(0,initialBracket) +
                    line.Substring(finalBracket+1);


            } else {
                Debug.LogWarning ("Found a [question=x] variable in the text but something is wrong with it. Check The spelling");
            }
        }

        return line;
    }

    public bool LookForChoices(string line){
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

    #endregion

    #region speed

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

    #endregion

    #region richtext

    private string LookForRichTexts(string line){
        //If you had any sprites added to your line... I'm sorry, you need to enable Rich Text
        if (spritesUsed.Count > 0) {
            textUI.ChangeRichText(true);
        }

        //check for any rich text (only it is marked as so on the UI element)
        if (textUI.RichText() && line.IndexOf('<') != -1)
        {
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
                        //But we don't want to add it if it is a "sprite" tag from TMP
                        if (tagName == "sprite")
                        {
                            closedTag = "";
                            closedTagLine = finalBracket +1;
                        }
                        else
                        {
                            line += closedTag;
                            unclosedTags.Add(line.Substring(inicialBracket, finalBracket - inicialBracket + 1));

                            closedTagLine = line.IndexOf(closedTag, finalBracket);
                        }
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

    #endregion

    #region expressions

    private string LookForExpression(string line, bool returnLine = false)
    {
        //check if the user have some expression and the line asks for one
        if (line.IndexOf("[expression=") != -1 && line.IndexOf("]") != -1)
        {
            //We do have one!
            int initialBracket = line.IndexOf("[expression=");
            int finalBracket = -1;
            if (initialBracket != -1)
            {
                finalBracket = line.IndexOf("]", initialBracket);
            }

            //There still are any '[expression=' and it is before a ']'? 
            if (initialBracket < finalBracket)
            {
                if (line.Substring(initialBracket + 12, finalBracket - (initialBracket + 12)).Length > 0)
                {
                    if (returnLine)
                    {
                        return line.Substring(0, initialBracket) +
                            line.Substring(finalBracket + 1);
                    }
                    else
                    {
                        return line.Substring(initialBracket + 12, finalBracket - (initialBracket + 12));
                    }

                }
                else
                {
                    Debug.LogWarning("Found a [expression=x] variable in the text but something is wrong with it. Check The spelling");
                }


            }


        }
        if (returnLine)
        {
            return line;
        }
        else
        {
            return "";
        }
    }

    Expression IsExpressing(RpgtalkElement element)
    {
        if(element.expression != "")
        {
            foreach(RPGTalkCharacterSettings character in characters)
            {
                if(character.character.dialoger == element.originalSpeakerName)
                {
                    foreach(Expression expression in character.character.expressions)
                    {
                        if(expression.name == element.expression)
                        {

                            //if we have to change a photo, let's change it already
                            if(shouldUsePhotos && UIPhoto != null)
                            {
                                UIPhoto.sprite = expression.photo;
                            }

                            //if we have a default audio for this expression, let's play it too
                            if(expression.audio != null)
                            {
                                if(rpgAudioSorce == null)
                                {
                                    CreateAudioSource();
                                }
                                rpgAudioSorce.PlayOneShot(expression.audio);
                            }

                            return expression;
                        }
                    }
                }
            }

            Debug.LogError("An expression was used in your TXT, but that expression wasn't found on the character talking");
            return null;

        }
        else
        {
            return null;
        }
    }


    #endregion

    #region NewTalkTag

    private string LookForNewTalk(string line)
    {
        changeToBreak = "";
        changeToStart = "";


        //check if the user have some newtalk and the line asks for one
        if (line.IndexOf("[newtalk") != -1 && line.IndexOf("]") != -1)
        {
            //We do have one!
            int initialBracket = line.IndexOf("[newtalk");
            int finalBracket = -1;
            if (initialBracket != -1)
            {
                finalBracket = line.IndexOf("]", initialBracket);
            }

            //There still are any '[newtalk' and it is before a ']'?
            if (initialBracket < finalBracket)
            {

                //Everything fine until now. Now let's check the start and break variables
                int indexOfStart = line.IndexOf("start=", initialBracket + 8);
                int endOfStart = line.IndexOf(" ", indexOfStart);
                if(endOfStart == -1)
                {
                    endOfStart = line.IndexOf("]", indexOfStart);
                }
                int indexOfBreak = line.IndexOf("break=", initialBracket + 8);
                int endOfBreak = line.IndexOf(" ", indexOfBreak);
                if (endOfBreak == -1)
                {
                    endOfBreak = line.IndexOf("]", indexOfBreak);
                }



                if (indexOfStart != -1 && indexOfBreak != -1 && endOfBreak != -1 && endOfStart != -1)
                {
                    string newLineToStart = line.Substring(indexOfStart + 6, endOfStart - (indexOfStart + 6));
                    string newLineToBreak = line.Substring(indexOfBreak + 6, endOfBreak - (indexOfBreak + 6));

                    if (newLineToStart.Length > 0 && newLineToBreak.Length > 0)
                    {
                        changeToStart = newLineToStart;
                        changeToBreak = newLineToBreak;

                        return line.Substring(0, initialBracket) +
                            line.Substring(finalBracket + 1);
                    }
                    else
                    {
                        Debug.LogWarning("There was a problem in your start=x or break=y. Check The spelling");
                    }
                }
                else
                {
                    Debug.LogWarning("Found a [newtalk] variable in the text but it didn't had start=x or break=y. Check The spelling");
                }





            }


        }
       
            return line;
       
    }

    #endregion


    #region Save

    private bool LookForSave(string line)
    {
        //check if the user have some newtalk and the line asks for one
        if (line.IndexOf("[save") != -1 && line.IndexOf("]") != -1)
        {
            //We do have one!
            int initialBracket = line.IndexOf("[save");
            int finalBracket = -1;
            if (initialBracket != -1)
            {
                finalBracket = line.IndexOf("]", initialBracket);
            }

            //There still are any '[save' and it is before a ']'?
            if (initialBracket < finalBracket)
            {

                //Everything fine until now. Now let's check the start and break variables
                int indexOfStart = line.IndexOf("start=", initialBracket + 5);
                int endOfStart = line.IndexOf(" ", indexOfStart);
                if (endOfStart == -1)
                {
                    endOfStart = line.IndexOf("]", indexOfStart);
                }
                int indexOfBreak = line.IndexOf("break=", initialBracket + 5);
                int endOfBreak = line.IndexOf(" ", indexOfBreak);
                if (endOfBreak == -1)
                {
                    endOfBreak = line.IndexOf("]", indexOfBreak);
                }
                int indexOfSavedData = line.IndexOf("data=", initialBracket + 5);
                int endOfSavedData = line.IndexOf(" ", indexOfSavedData);
                if (endOfSavedData == -1)
                {
                    endOfSavedData = line.IndexOf("]", indexOfSavedData);
                }
                int indexOfModifier = line.IndexOf("mod=", initialBracket + 5);
                int endOfModifier = line.IndexOf(" ", indexOfModifier);
                if (endOfModifier == -1)
                {
                    endOfModifier = line.IndexOf("]", indexOfModifier);
                }



                if (indexOfStart != -1 && indexOfBreak != -1 && endOfBreak != -1 && endOfStart != -1 
                && indexOfSavedData != -1 && endOfSavedData != -1 && indexOfModifier != -1 && endOfModifier != -1)
                {
                    string newLineToStart = line.Substring(indexOfStart + 6, endOfStart - (indexOfStart + 6));
                    string newLineToBreak = line.Substring(indexOfBreak + 6, endOfBreak - (indexOfBreak + 6));
                    string newSavedData = line.Substring(indexOfSavedData + 5, endOfSavedData - (indexOfSavedData + 5));
                    string newModfier = line.Substring(indexOfModifier + 4, endOfModifier - (indexOfModifier + 4));
                    int intModifier;

                    if (newLineToStart.Length > 0 && newLineToBreak.Length > 0 && newSavedData.Length > 0 && int.TryParse(newModfier, out intModifier))
                    {
                        saves.Add(new RPGtalkSaveStatement());
                        saves[saves.Count - 1].lineToStart = newLineToStart;
                        saves[saves.Count - 1].lineToBreak = newLineToBreak;
                        saves[saves.Count - 1].savedData = newSavedData;
                        saves[saves.Count - 1].modifier = intModifier;

                        return true;
                    }
                    else
                    {
                        Debug.LogWarning("There was a problem in your save variables. Check The spelling");
                    }
                }
                else
                {
                    Debug.LogWarning("Found a [save] variable in the text but it didn't had a variable. Check The spelling");
                }





            }


        }

        return false;

    }

    #endregion


    #region jitter

    private string LookForJitter(string line)
    {
        //check if the user have some jitter and the line asks for one
        if ((line.IndexOf("[jitter=") != -1 && line.IndexOf("]", line.IndexOf("[jitter=")) != -1))
        {
            bool thereAreJittersLeft = true;

            //repeat as long as we find a jitter
            while (thereAreJittersLeft)
            {
                int initialBracket = line.IndexOf("[jitter=");
                int finalBracket = -1;
                int endOfJitter = -1;
                int spaceInJitter = -1;
                if (initialBracket != -1)
                {
                    finalBracket = line.IndexOf("]", initialBracket);
                    spaceInJitter = line.IndexOf(" ", initialBracket);
                }
                if (finalBracket != -1)
                {
                    endOfJitter = line.IndexOf("[/jitter]", finalBracket);
                }




                //There still are any '[jitter=' and it is before a ']'? 
                if (initialBracket < finalBracket)
                {
                    if (endOfJitter == -1)
                    {
                        Debug.LogError("A [jitter] tag was found. But not a [/jitter] !");
                        return line;
                    }

                    //Ok, new jitter chang around! Let's get its number
                    float jitterNum = 0;

                    int finalJitterNum = finalBracket;
                    if(spaceInJitter != -1 && spaceInJitter < finalBracket)
                    {
                        finalJitterNum = spaceInJitter;
                    }


                    //Check if the number was a valid float
                    if (float.TryParse(line.Substring(initialBracket + 8, finalJitterNum - (initialBracket + 8)), out jitterNum))
                    {
                        //Neat, we definely have a jitter with a valid number. Time to keep track of it
                        RPGTalkJitter newJitter = new RPGTalkJitter();
                        newJitter.jitter = Mathf.Abs(jitterNum);
                        //subtract from the jitter position any rpgtalk tag that might have come before it
                        newJitter.jitterPosition = initialBracket - RPGTalkHelper.CountRPGTalkTagCharacters(line.Substring(0, initialBracket));
                        newJitter.lineWithJitter = rpgtalkElements.Count;

                        //ok, looking good! Now, we want to check if we have an angle set in this jitter tag.
                        int anglePos = line.IndexOf("angle=", initialBracket);
                        float angle = 1;
                        if(anglePos != -1)
                        {
                            if (float.TryParse(line.Substring(anglePos + 6, finalBracket - (anglePos + 6)), out angle))
                            {
                                newJitter.angle = angle;
                            }
                            else
                            {
                                Debug.LogWarning("Found a angle in a [jitter] variable but something is wrong with it. Check The spelling.");
                                thereAreJittersLeft = false;
                            }
                        }
                       

                        //let's look for the number of characters that should recieve the jitter
                        newJitter.numberOfCharacters = endOfJitter - (finalBracket + 1);

                        jitters.Add(newJitter);


                        //Looking good! We found out that a jitter should be there and we are already keeping track of it
                        //But now we should remove the [jitter=X] from the line.
                        line = line.Substring(0, initialBracket) +
                        line.Substring(finalBracket + 1, endOfJitter - (finalBracket + 1)) +
                            line.Substring(endOfJitter+9);


                    }
                    else
                    {
                        Debug.LogWarning("Found a [jitter=x] variable in the text but something is wrong with it. Check The spelling.");
                        thereAreJittersLeft = false;
                    }
                    

                }
                else
                {
                    thereAreJittersLeft = false;
                }
            }
        }

        return line;
    }


    #endregion



    // Update is called once per frame
    void Update () {
        //We don't want to do nothing if the text isn't even showing
        if (!textUIObj.activeInHierarchy) {
            return;
        }


        if (textUI!= null && textUI.Enabled() &&
            currentChar >= rpgtalkElements [cutscenePosition - 1].dialogText.Length) {
            //if we hit the end of the talk, but we should stay on screen, return.
            //but if we have a callback, he can click on it once more.
            if (cutscenePosition >= rpgtalkElements.Count && shouldStayOnScreen) {
                if(lookForClick && (
                    (passWithMouse && Input.GetMouseButtonDown (0)) ||
                    (passWithInputButton != "" && Input.GetButtonDown(passWithInputButton)) || (passWithKey != KeyCode.None && Input.GetKeyDown(passWithKey))
                )){
                    //if have an audio... playit
                    if (passAudio != null && !rpgAudioSorce.isPlaying) {
                        rpgAudioSorce.clip = passAudio;
                        rpgAudioSorce.Play ();
                    }
                    if(callback.GetPersistentEventCount() > 0){
                        callback.Invoke();
                    }
                    lookForClick = false;
                }

                return;
            }

            //if we reached the end of the line and click on the screen...
            if (
                enablePass && (
                (passWithMouse && Input.GetMouseButtonDown (0)) ||
                (passWithInputButton != "" && Input.GetButtonDown(passWithInputButton)) || (passWithKey != KeyCode.None && Input.GetKeyDown(passWithKey))

            ) ){//if have an audio... playit
                if (passAudio != null) {
                    rpgAudioSorce.clip = passAudio;
                    rpgAudioSorce.Play ();
                }
                textUI.Enabled(false);
                PlayNext ();

            }
            return;
        }




        //if we're currently showing dialog, then start scrolling it
        if(textUI.Enabled()) {
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
                    (passWithInputButton != "" && Input.GetButtonDown(passWithInputButton)) || (passWithKey != KeyCode.None && Input.GetKeyDown(passWithKey))
                && currentChar > 3)) {

                currentChar = rpgtalkElements[cutscenePosition - 1].dialogText.Length;
                PutRightTextToShow ();
                //Do what we have to do if the the text just ended
                TextEnded();
            }

        




            
        }


    }

    //The text just ended to be written on the screen
    void TextEnded(){

        isAnimating = false;

        //If we want the talk to auto pass... Auto pass it
        if (autoPass)
        {
            Invoke("AutoPass", secondsAutoPass);
        }

        //call the event
        if (OnEndAnimating != null)
        {
            OnEndAnimating();
        }

        //if we have an animator.. stop it
        if (actualAnimator != null) {
            actualAnimator.SetBool (animatorBooleanName, false);
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
                        Button newChoiceBtn = newChoice.GetComponent<Button>();
                        if (newChoiceBtn) {
                            string thisText = q.choices[i];
                            string correctText = thisText;
                            //make sure we will not want to make it to a new talk
                            correctText = LookForNewTalk(correctText);

                            newChoice.GetComponentInChildren<Text> ().text = correctText;
                            int choiceNumber = i;
                            newChoiceBtn.onClick.AddListener (delegate{MadeAChoice (q.questionID, choiceNumber, thisText);});
                            if (i == 0) {
                                StartCoroutine(SelectButton(newChoiceBtn));
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

    IEnumerator SelectButton(Button button)
    {
        yield return new WaitForSeconds(0.3f);
        button.Select();
    }


    void AutoPass()
    {
        PlayNext();
    }

    /// <summary>
    /// Function to be called by the buttons when the user makes a choice.
    /// This passes the talk and call the OnMadeChoice event
    /// </summary>
    public void MadeAChoice(string questionID, int choiceNumber, string text){
        foreach (RPGTalkQuestion q in questions) {
            if (q.questionID == questionID) {
                q.alreadyHappen = true;
                LookForNewTalk(text);
                break;
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


        //Did we reach the point where a jitter should happen?
        for (int i = 0; i < jitters.Count; i++)
        {
            if (jitters[i].jitterPosition <= currentChar && !jitters[i].alreadyGone && jitters[i].lineWithJitter == cutscenePosition - 1)
            {
                jitterRoutine = StartCoroutine(textUI.Jitter(jitters[i]));

                jitters[i].alreadyGone = true;

                //Change currentChar to the position of the [speed] tag so no word gets jumped
                currentChar = jitters[i].jitterPosition;

            }
        }




        //Check if a line break is starting to appear
        if (rpgtalkElements[cutscenePosition - 1].dialogText.Length > currentChar + 2 &&
            rpgtalkElements[cutscenePosition - 1].dialogText.Substring((int)currentChar, 2) == "\\n")
        {
            currentChar += 2;
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
        textUI.ChangeTextTo(textToDisplay);

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
                spritesUsed [i].alreadyInPlace = textUIObj.GetComponent<ITextWithIcon> ().FitImagesOnText (i);
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
        if (textUIObj && textUIObj.activeInHierarchy) {
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

        //If we had auto pass, cancel it
        if (autoPass)
        {
            CancelInvoke("AutoPass");
        }

        //stop any dubs
        if (dubSounds != null) {
            dubSounds.StopCurrentDub ();
        }

        //Let's set to false any expression left
        if(actualAnimator != null)
        {
            if (expressing != null)
            {
                actualAnimator.SetBool(expressing.boolInAnimator, false);
            }
        }

        //if there was any jitter here.. Let's remove it!
        if (jitterRoutine != null)
        {
            StopCoroutine(jitterRoutine);
            jitterRoutine = null;
        }





        // increment the cutscene counter
        cutscenePosition++;
        currentChar = 0;

        //if there was any sprite in the text, deactivate it
        if (spritesUsed.Count > 0) {
            foreach (Transform child in textUIObj.transform) {
                child.gameObject.SetActive (false);
            }
        }

        if(cutscenePosition <= rpgtalkElements.Count) {

            textUI.Enabled(true);
            
            RpgtalkElement currentRpgtalkElement = rpgtalkElements[cutscenePosition - 1];

            if (dialoger) {
                if (dialogerObj) {
                    dialogerUI.Enabled(true);

                    dialogerUI.ChangeTextTo(currentRpgtalkElement.speakerName);
                }
                if (shouldUsePhotos) {
                    for (int i = 0; i < characters.Length; i++) {
                        if (characters [i].character.dialoger == currentRpgtalkElement.originalSpeakerName) {
                            if (UIPhoto) {
                                UIPhoto.sprite = characters [i].character.photo;
                            }
                            //Change its animator
                            if (characters[i].animatorOverwrite != null)
                            {
                                actualAnimator = characters[i].animatorOverwrite;
                            }
                            else
                            {
                                actualAnimator = animatorWhenTalking;
                            }
                            if (actualAnimator && animatorIntName != ""){
                                actualAnimator.SetInteger (animatorIntName, i);

                            }
                            break;
                        }
                    }
                }
            }

            CheckWhoToFollow (currentRpgtalkElement);


            //check if there should be any dubs in this line
            CheckDubsInThisLine();

            //check if we are expressing something
            expressing = IsExpressing(rpgtalkElements[cutscenePosition - 1]);

            //if we have an animator.. play it
            PlayAnimator(rpgtalkElements[cutscenePosition - 1]);

            //check if after this line we should start another talk
            currentRpgtalkElement.dialogText = LookForNewTalk(currentRpgtalkElement.dialogText);


        } else {
            //The talk has finished



            //check if we are supposed to be playing another talk
            if (!string.IsNullOrEmpty(changeToStart) && !string.IsNullOrEmpty(changeToBreak))
            {
                NewTalk(changeToStart, changeToBreak);
                return;
            }

            //check if we are saved something that should take me to another talk
            foreach(RPGtalkSaveStatement saved in saves)
            {
                if(saveInstance != null)
                {
                    if (saveInstance.GetSavedData(saved.savedData, saved.modifier))
                    {
                        NewTalk(saved.lineToStart, saved.lineToBreak);
                        return;
                    }
                }
                else
                {
                    Debug.LogWarning("Found a save but you didn't had the RPGTalkSaveInstance on this object");
                }
            }



            //call the event
            if (OnEndTalk != null){
                OnEndTalk ();
            }


            isPlaying = false;

            if (!shouldStayOnScreen) {
                textUI.Enabled(false);
                if (dialoger) {
                    if (dialogerObj) {
                        dialogerUI.Enabled(false);
                    }
                }
                for (int i = 0; i < showWithDialog.Length; i++) {
                    showWithDialog [i].SetActive (false);
                }
            }

            callback.Invoke();
        }

        
    }

    //This function play the right bools on the animator.
    void PlayAnimator(RpgtalkElement whoIsPlaying)
    {
        if (actualAnimator != null)
        {
            if (expressing != null)
            {
                actualAnimator.SetBool(expressing.boolInAnimator, true);
            }

            actualAnimator.SetBool(animatorBooleanName, true);
        }
    }



    void CheckWhoToFollow(RpgtalkElement element){
        //Set it to follow someone
        //resets anyone that is being followed
        following = null;
        followingOffset = Vector3.zero;
        if (shouldFollow && characters.Length > 0) {
            foreach (RPGTalkCharacterSettings character in characters) {
                //if the character in the follow array have the same name as the talker or an empty name, follow it!
                if(character.character.dialoger == element.speakerName ||
                    character.character.dialoger == ""){
                    following = character.follow;
                    followingOffset = character.followOffset;
                }
            }
        }
    }

}
