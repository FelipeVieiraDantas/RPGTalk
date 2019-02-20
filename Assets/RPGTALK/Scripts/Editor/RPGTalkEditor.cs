using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;


[CustomEditor(typeof(RPGTalk))]
public class RPGTalkEditor : Editor
{

	override public void OnInspectorGUI()
	{
		serializedObject.Update ();

		//Instance of our RPGTalk class
		RPGTalk rpgTalk = (RPGTalk)target;

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.LabelField("Put below the Text file to be parsed and become the talks!");
		EditorGUILayout.PropertyField (serializedObject.FindProperty("txtToParse"),GUIContent.none);
		if (serializedObject.FindProperty ("txtToParse").objectReferenceValue == null) {
			EditorGUILayout.HelpBox ("RPGTalk needs a TXT file to retrieve the lines from", MessageType.Error, true);
		} 

		EditorGUILayout.PropertyField (serializedObject.FindProperty("showWithDialog"),true);
		if(serializedObject.FindProperty("showWithDialog").arraySize == 0){
			EditorGUILayout.HelpBox("Not a single element to be shown with the Talk? Not even the Canvas?" +
				"Are you sure that is the correct bahaviour?", MessageType.Warning, true);
		}

		EditorGUILayout.BeginVertical( (GUIStyle) "HelpBox"); 

		EditorGUILayout.LabelField("Regular Options:",EditorStyles.boldLabel);
		rpgTalk.startOnAwake = GUILayout.Toggle(rpgTalk.startOnAwake, "Start On Awake?");
		rpgTalk.dialoger = GUILayout.Toggle(rpgTalk.dialoger, "Should show the name of the talker?");
		rpgTalk.shouldUsePhotos = GUILayout.Toggle(rpgTalk.shouldUsePhotos, "Should there be the photo of the talker?");
		rpgTalk.shouldStayOnScreen = GUILayout.Toggle(rpgTalk.shouldStayOnScreen, "Should the canvas stay on screen after the talk ended?");
		rpgTalk.shouldFollow = GUILayout.Toggle(rpgTalk.shouldFollow, "Should the canvas follow someone?");
		if(rpgTalk.shouldFollow){
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField("Who should it follow? Should there be an Offset?");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField (serializedObject.FindProperty("follow"),true);
			//EditorGUILayout.PropertyField (serializedObject.FindProperty("followOffset"),GUIContent.none);
			EditorGUILayout.EndHorizontal();

			if(rpgTalk.shouldFollow && rpgTalk.follow.Length == 0){
				EditorGUILayout.HelpBox("You didn't set anyone to follow!", MessageType.Error, true);
			}

			EditorGUILayout.LabelField("The object that follows should be Billboard? Based on which camera?");
			EditorGUILayout.BeginHorizontal();
			rpgTalk.billboard = GUILayout.Toggle(rpgTalk.billboard,"Billboard?");
			if(rpgTalk.billboard){
				rpgTalk.mainCamera = GUILayout.Toggle (rpgTalk.mainCamera, "Based on Main Camera?");
			}
			EditorGUILayout.EndHorizontal();
			if (rpgTalk.billboard && !rpgTalk.mainCamera) {
				EditorGUILayout.LabelField("What camera should the Billboard be based on?");
				EditorGUILayout.PropertyField (serializedObject.FindProperty("otherCamera"),GUIContent.none);
			}
			EditorGUI.indentLevel--;
		}
		rpgTalk.enableQuickSkip = GUILayout.Toggle(rpgTalk.enableQuickSkip, "Enable QuickStep (the player can jump the animation)?");
		EditorGUILayout.PropertyField (serializedObject.FindProperty("textSpeed"));

		rpgTalk.passWithMouse = GUILayout.Toggle(rpgTalk.passWithMouse, "Pass the Talk with Mouse Click?");
		EditorGUILayout.LabelField("The Talk can also be passed with some button set on Project Settings > Input:");
		EditorGUILayout.PropertyField (serializedObject.FindProperty("passWithInputButton"),GUIContent.none);
		if(!rpgTalk.passWithMouse && serializedObject.FindProperty("passWithInputButton").stringValue == ""){
			EditorGUILayout.HelpBox("There is no condition to pass the Talk. Is it really the expected behaviour?", MessageType.Warning, true);
		}

		EditorGUILayout.Space ();
		EditorGUILayout.LabelField("RPGTalk can try to make a Word Wrap for long texts in the same line.");
		rpgTalk.wordWrap = GUILayout.Toggle(rpgTalk.wordWrap, "Word Wrap?");
		if (rpgTalk.wordWrap) {
			EditorGUILayout.LabelField ("Set manually the maximum chars in Width/Height that fit in the screen:");
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("maxCharInWidth"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("maxCharInHeight"));
			EditorGUI.indentLevel--;

		}

		EditorGUILayout.EndVertical ();


		//create a nice box with round edges.
		EditorGUILayout.BeginVertical( (GUIStyle) "HelpBox"); 
		EditorGUILayout.LabelField("Interface:",EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Put below the UI for the text itself:");
		EditorGUILayout.PropertyField (serializedObject.FindProperty("textUI"));
		if(serializedObject.FindProperty("textUI").objectReferenceValue == null){
			EditorGUILayout.HelpBox("There should be a Text, inside of a Canvas, to show the Talk.", MessageType.Error, true);
		}
		if (rpgTalk.dialoger) {
			EditorGUILayout.LabelField("Put below the UI for the name of the talker:");
			EditorGUILayout.PropertyField (serializedObject.FindProperty("dialogerUI"));
			if(serializedObject.FindProperty("dialogerUI").objectReferenceValue == null){
				EditorGUILayout.HelpBox("There should be a Text, inside of a Canvas, to show the talker's name.", MessageType.Warning, true);
			}
		}
		if (rpgTalk.shouldUsePhotos) {
			EditorGUILayout.LabelField("Put below the UI for the photo of the talker:");
			EditorGUILayout.PropertyField (serializedObject.FindProperty("UIPhoto"));
			if(serializedObject.FindProperty("UIPhoto").objectReferenceValue == null){
				EditorGUILayout.HelpBox("There should be a Image, inside of a Canvas, to show the talker's photo.", MessageType.Warning, true);
			}
		}
		EditorGUILayout.LabelField("An object can blink when expecting player action:");
		EditorGUILayout.PropertyField (serializedObject.FindProperty("blinkWhenReady"),GUIContent.none);
		EditorGUILayout.EndVertical ();


		EditorGUILayout.BeginVertical( (GUIStyle) "HelpBox"); 
		EditorGUILayout.LabelField("Callback, Breaks & Variables:",EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Any script should be called when the Talk is done?");
		EditorGUILayout.PropertyField (serializedObject.FindProperty("callbackScript"),GUIContent.none);
		if(serializedObject.FindProperty("callbackScript").objectReferenceValue != null){
			EditorGUILayout.LabelField("What function on that script should be called?");
			EditorGUILayout.PropertyField (serializedObject.FindProperty("callbackFunction"),GUIContent.none);
			if(serializedObject.FindProperty("callbackFunction").stringValue == ""){
				EditorGUILayout.HelpBox("You said that a script should be called as callback, but didn't set the name of the functions to be called in that script", MessageType.Error, true);
			}
		}

		EditorGUILayout.Space ();
		EditorGUILayout.LabelField("What line of the text should the Talk start? And in what line shoult it end?");
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.PropertyField (serializedObject.FindProperty("lineToStart"),GUIContent.none);
		EditorGUILayout.PropertyField (serializedObject.FindProperty("lineToBreak"),GUIContent.none);
		EditorGUILayout.EndHorizontal ();
		string lineToStart = serializedObject.FindProperty ("lineToStart").stringValue;
		int actualLinaToStart = -2;
		if( int.TryParse(lineToStart, out actualLinaToStart) && actualLinaToStart < 1){
			EditorGUILayout.HelpBox("The line that the Text should start must be 1 or greater!", MessageType.Error, true);
		}
		string lineToBreak = serializedObject.FindProperty ("lineToBreak").stringValue;
		int actualLinaToBreak = -2;
		if(int.TryParse(lineToBreak, out actualLinaToBreak) && actualLinaToBreak != -1 &&
			actualLinaToBreak < actualLinaToStart){
			EditorGUILayout.HelpBox("The line of the Text to stop the Talk comes before the line of the Text to start the Talk? " +
				"That makes no sense! If you want to read the Text file until the end, leave the line to break as '-1'", MessageType.Error, true);
		}
		EditorGUILayout.HelpBox("The line to start or to end might be set as strings! For instance, you can set lineToStart as 'MyString' and in your text, RPGTalk will start reading the line just after the tag [title=MyString].", MessageType.Info, true);

		EditorGUILayout.Space ();
		EditorGUILayout.LabelField("Variables can be set to change some word in the text");
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField (serializedObject.FindProperty("variables"),true);
		EditorGUI.indentLevel--;

		EditorGUILayout.EndVertical ();

		EditorGUILayout.BeginVertical( (GUIStyle) "HelpBox"); 
		EditorGUILayout.LabelField("Photos and Sprites:",EditorStyles.boldLabel);

		if (rpgTalk.shouldUsePhotos) {
			EditorGUILayout.LabelField ("Change photo for different talkers:");
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("photos"), true);
			EditorGUI.indentLevel--;
			EditorGUILayout.Space ();
		}

		EditorGUILayout.LabelField ("Add sprites that can be used inside the text:");
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("sprites"), true);
		EditorGUI.indentLevel--;
		if (rpgTalk.sprites != null && rpgTalk.sprites.Count > 0) {
			EditorGUILayout.HelpBox("To use sprites inside the text, write the tag [sprite=X] and RPGTalk will replace it with the corresponding sprite above", MessageType.Info, true);
		}


		EditorGUILayout.EndVertical ();

		EditorGUILayout.BeginVertical( (GUIStyle) "HelpBox"); 
		EditorGUILayout.LabelField("Animation:",EditorStyles.boldLabel);
		EditorGUILayout.LabelField("A Animator can be manipulated when talking:");
		EditorGUILayout.PropertyField (serializedObject.FindProperty("animatorWhenTalking"),GUIContent.none);
		if(serializedObject.FindProperty("animatorWhenTalking").objectReferenceValue != null){
			EditorGUILayout.LabelField("A Boolean to be set true when the character is talking:");
			EditorGUILayout.PropertyField (serializedObject.FindProperty("animatorBooleanName"),GUIContent.none);
			EditorGUILayout.LabelField("A int can be set with the number of the talker, based on the list of photos:");
			EditorGUILayout.PropertyField (serializedObject.FindProperty("animatorIntName"),GUIContent.none);
		}
		EditorGUILayout.EndVertical ();

		EditorGUILayout.BeginVertical( (GUIStyle) "HelpBox"); 
		EditorGUILayout.LabelField("Audio:",EditorStyles.boldLabel);
		EditorGUILayout.LabelField("The audio to be played by each letter:");
		EditorGUILayout.PropertyField (serializedObject.FindProperty("textAudio"),GUIContent.none);
		EditorGUILayout.LabelField("The audio to be played when the player passes the Talk:");
		EditorGUILayout.PropertyField (serializedObject.FindProperty("passAudio"),GUIContent.none);
		EditorGUILayout.EndVertical ();

		EditorGUILayout.BeginVertical( (GUIStyle) "HelpBox"); 
		EditorGUILayout.LabelField("Choices:",EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Put below a Button prefab that will be instantiated when the player has choices");
		EditorGUILayout.PropertyField (serializedObject.FindProperty("choicePrefab"),GUIContent.none);
		EditorGUILayout.LabelField("Put below a object that the choice buttons will be instantiated to:");
		EditorGUILayout.PropertyField (serializedObject.FindProperty("choicesParent"),GUIContent.none);
		if(serializedObject.FindProperty("choicePrefab").objectReferenceValue != null && serializedObject.FindProperty("choicesParent").objectReferenceValue == null){
			EditorGUILayout.HelpBox("There should be a parent to instantiate the choices to. Usually, it as an object inside the dialog box with a 'Layout Group' element.", MessageType.Warning, true);
		}
		EditorGUILayout.EndVertical ();

		EditorGUILayout.Separator ();

		EditorGUILayout.BeginVertical( (GUIStyle) "HelpBox"); 
		EditorGUILayout.LabelField("So, I heard you like free stuff, hum?",EditorStyles.boldLabel);
		GUIStyle style = new GUIStyle ();
		style.richText = true;
		string color = "#cc0000";
		GUILayout.Label(string.Format("Be sure to show your support by following <b><color={0}>Seize Studios</color></b> on social medias!", color), style);
		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button ("Like our page on Facebook!")) {
			Application.OpenURL(@"https://www.facebook.com/seizestudios/");
		}
		if (GUILayout.Button ("Follow us on Twitter!")) {
			Application.OpenURL(@"https://twitter.com/seizestudios");
		}
		EditorGUILayout.EndHorizontal ();
		GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
		centeredStyle.alignment = TextAnchor.UpperCenter;
		EditorGUILayout.LabelField("www.seizestudios.com",centeredStyle);
		EditorGUILayout.EndVertical ();



		if(EditorGUI.EndChangeCheck())
			serializedObject.ApplyModifiedProperties();
		//EditorGUIUtility.LookLikeControls();

	}
}