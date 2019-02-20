using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


[CustomEditor(typeof(RPGTalkArea))]
public class RPGTalkAreaEditor : Editor {

	override public void OnInspectorGUI()
	{
		serializedObject.Update ();

		//Instance of our RPGTalkLocalization class
		RPGTalkArea area = (RPGTalkArea)target;

		EditorGUI.BeginChangeCheck ();


		EditorGUILayout.BeginVertical( (GUIStyle) "HelpBox"); 
		EditorGUILayout.LabelField("What should be changed?",EditorStyles.boldLabel);


		EditorGUILayout.LabelField("Put below the RPGTalk instance that can be changed by this area");
		EditorGUILayout.PropertyField (serializedObject.FindProperty("rpgtalkTarget"),GUIContent.none);
		EditorGUILayout.LabelField("Put below the Playable Director instance that can be played by this area");
		EditorGUILayout.PropertyField (serializedObject.FindProperty("timelineDirectorToPlay"),GUIContent.none);

		if(serializedObject.FindProperty("timelineDirectorToPlay").objectReferenceValue == null &&
			serializedObject.FindProperty("rpgtalkTarget").objectReferenceValue == null){
			EditorGUILayout.HelpBox("This Area is not doing nothing! You need to set at least a RPGTalk instance or a Timeline instance!", MessageType.Error, true);
		}
		EditorGUILayout.EndVertical ();


		EditorGUILayout.BeginVertical( (GUIStyle) "HelpBox"); 
		EditorGUILayout.LabelField("When it should be changed?",EditorStyles.boldLabel);

		area.shouldInteractWithButton = GUILayout.Toggle(area.shouldInteractWithButton, "Should wait for user's interaction to start?");
		if (area.shouldInteractWithButton) {
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField ("Key that needs to be pressed to interect:");
			area.interactionKey = (KeyCode)EditorGUILayout.EnumPopup (area.interactionKey);

			EditorGUILayout.PropertyField (serializedObject.FindProperty("showWhenInteractionIsPossible"),true);
			if(serializedObject.FindProperty("showWhenInteractionIsPossible").arraySize == 0){
				EditorGUILayout.HelpBox("Not a single element to be shown when the intercation is possible?" +
					"It would be nice to put a element here such as 'Press X to talk'", MessageType.Warning, true);
			}
			EditorGUI.indentLevel--;
		} else {
			area.triggerEnter = GUILayout.Toggle(area.triggerEnter, "Start OnTriggerEnter?");
			area.triggerExit = GUILayout.Toggle(area.triggerExit, "Start OnTriggerExit?");
		}

		area.happenOnlyOnce = GUILayout.Toggle(area.happenOnlyOnce, "Can only be played once?");
		if (serializedObject.FindProperty ("rpgtalkTarget").objectReferenceValue != null) {
			area.forbidPlayIfRpgtalkIsPlaying = GUILayout.Toggle (area.forbidPlayIfRpgtalkIsPlaying, "Cannot be played if the RPGTalk instance is already playing");
		}

		EditorGUILayout.LabelField("Check if the collider that hit this are has this tag");
		EditorGUILayout.PropertyField (serializedObject.FindProperty("checkIfColliderHasTag"),GUIContent.none);
		if (serializedObject.FindProperty ("checkIfColliderHasTag").stringValue == "") {
			EditorGUILayout.HelpBox ("If the tag is leaved in blank, this Area will work for every Collider that hits it.", MessageType.Info, true);
		}

		EditorGUILayout.EndVertical ();

		EditorGUILayout.BeginVertical( (GUIStyle) "HelpBox"); 
		EditorGUILayout.LabelField("Callbacks",EditorStyles.boldLabel);

		EditorGUILayout.LabelField("Any script should be called before the area beggins?");
		EditorGUILayout.PropertyField (serializedObject.FindProperty("callbackScriptBeforeTalk"),GUIContent.none);
		if(serializedObject.FindProperty("callbackScriptBeforeTalk").objectReferenceValue != null){
			EditorGUILayout.LabelField("What function on that script should be called?");
			EditorGUILayout.PropertyField (serializedObject.FindProperty("callbackFunctionBeforeTalk"),GUIContent.none);
			if(serializedObject.FindProperty("callbackFunctionBeforeTalk").stringValue == ""){
				EditorGUILayout.HelpBox("You said that a script should be called as callback, but didn't set the name of the functions to be called in that script", MessageType.Error, true);
			}
		}

		if (serializedObject.FindProperty ("rpgtalkTarget").objectReferenceValue != null) {
			EditorGUILayout.LabelField ("Any script should be called when the Talk is done?");
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("callbackScriptAfterTalk"), GUIContent.none);
			if (serializedObject.FindProperty ("callbackScriptAfterTalk").objectReferenceValue != null) {
				EditorGUILayout.LabelField ("What function on that script should be called?");
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("callbackFunctionAfterTalk"), GUIContent.none);
				if (serializedObject.FindProperty ("callbackFunctionAfterTalk").stringValue == "") {
					EditorGUILayout.HelpBox ("You said that a script should be called as callback, but didn't set the name of the functions to be called in that script", MessageType.Error, true);
				}
			}
		}
			
		EditorGUILayout.EndVertical ();

		if (serializedObject.FindProperty ("rpgtalkTarget").objectReferenceValue != null) {
			EditorGUILayout.BeginVertical ((GUIStyle)"HelpBox"); 
			EditorGUILayout.LabelField ("Change in RPGTalk:", EditorStyles.boldLabel);

			EditorGUILayout.LabelField ("Put below the Text file to be parsed and become the talks!");
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("txtToParse"), GUIContent.none);
			if (serializedObject.FindProperty ("txtToParse").objectReferenceValue == null) {
				EditorGUILayout.HelpBox ("If no TXT file is set, it will be used the same that this RPGTalk instance already have", MessageType.Info, true);
			} 

			EditorGUILayout.Space ();

			EditorGUILayout.LabelField ("What line of the text should the Talk start? And in what line shoult it end?");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("lineToStart"), GUIContent.none);
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("lineToBreak"), GUIContent.none);
			EditorGUILayout.EndHorizontal ();
			string lineToStart = serializedObject.FindProperty ("lineToStart").stringValue;
			int actualLinaToStart = -2;
			if (int.TryParse (lineToStart, out actualLinaToStart) && actualLinaToStart < 1) {
				EditorGUILayout.HelpBox ("The line that the Text should start must be 1 or greater!", MessageType.Error, true);
			}
			string lineToBreak = serializedObject.FindProperty ("lineToBreak").stringValue;
			int actualLinaToBreak = -2;
			if (int.TryParse (lineToBreak, out actualLinaToBreak) && actualLinaToBreak != -1 &&
			  actualLinaToBreak < actualLinaToStart) {
				EditorGUILayout.HelpBox ("The line of the Text to stop the Talk comes before the line of the Text to start the Talk? " +
				"That makes no sense! If you want to read the Text file until the end, leave the line to break as '-1'", MessageType.Error, true);
			}
			EditorGUILayout.HelpBox ("The line to start or to end might be set as strings! For instance, you can set lineToStart as 'MyString' and in your text, RPGTalk will start reading the line just after the tag [title=MyString].", MessageType.Info, true);

			EditorGUILayout.Space ();
			area.shouldStayOnScreen = GUILayout.Toggle (area.shouldStayOnScreen, "Should the canvas stay on screen after the talk ended?");


			EditorGUILayout.EndVertical ();
		}


		if(EditorGUI.EndChangeCheck())
			serializedObject.ApplyModifiedProperties();
	}
}
