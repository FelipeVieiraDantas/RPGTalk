using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using RPGTALK.Localization;


[CustomEditor(typeof(RPGTalkLocalization))]
public class RPGTalkLocalizationEditor : Editor {

	override public void OnInspectorGUI()
	{
		serializedObject.Update ();

		//Instance of our RPGTalkLocalization class
		RPGTalkLocalization localization = (RPGTalkLocalization)target;

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.HelpBox ("Put into the array below every language that you have. Inside every single one of them, put every text asset that could be used by that language.", MessageType.Info, true);
		EditorGUILayout.PropertyField (serializedObject.FindProperty ("language"), true);

		EditorGUILayout.Space ();

		EditorGUILayout.HelpBox ("Set below the default language of your game and the language that it is currently on. You can set this values by code calling LanguageSettings.defaultLanguage or .currenLanguage.", MessageType.Info, true);

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Default Language:");
		LanguageSettings.defaultLanguage = (SupportedLanguages)EditorGUILayout.EnumPopup (LanguageSettings.defaultLanguage);
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Actual Language:");
		LanguageSettings.actualLanguage = (SupportedLanguages)EditorGUILayout.EnumPopup (LanguageSettings.actualLanguage);
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.HelpBox ("If you need to set more languages, simply add them into the enum SupportedLanguages in the RPGTalkLocalization script.", MessageType.Info, true);
	
	
		if(EditorGUI.EndChangeCheck())
			serializedObject.ApplyModifiedProperties();
	}
}
