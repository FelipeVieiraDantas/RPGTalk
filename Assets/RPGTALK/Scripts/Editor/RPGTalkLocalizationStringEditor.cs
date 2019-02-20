using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGTALK.Texts;

namespace RPGTALK.Localization
{
    [CustomEditor(typeof(RPGTalkLocalizationString))]
    public class RPGTalkLocalizationStringEditor : Editor
    {
        override public void OnInspectorGUI()
        {
            serializedObject.Update();

            //Instance of our RPGTalkLocalization class
            RPGTalkLocalizationString localization = (RPGTalkLocalizationString)target;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.HelpBox("There are two ways to make localization work with a simple string on UI. You can set a TXT file, and add it to a Language like you would usually do in RPGTalk, or you can set this string manully.", MessageType.Info, true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("txtToParse"), true);
            if(serializedObject.FindProperty("txtToParse").objectReferenceValue != null)
            {
                EditorGUILayout.LabelField("What line of the TXT should be written here?");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("lineToRead"), GUIContent.none);

            }
            else
            {
                GUI.skin.label.wordWrap = true;
                GUILayout.Label("If you won't use a TXT, you need to add manually every language option for this text");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("language"), true);
            }

            //Show the correct text
            if ((localization.txtToParse != null && localization.lineToRead != "") || (localization.language != null && localization.language.Length > 0))
            {

                if (TMP_Translator.IsValidType(localization.gameObject))
                {
                    localization.ChangeCurrentTextToActualLanguage();
                }
                else
                {
                    EditorGUILayout.HelpBox("RPGTalkLocalization String works only in an object with Text or Text Mesh Pro UGUI Componenets", MessageType.Error, true);
                }
            }



            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }
}