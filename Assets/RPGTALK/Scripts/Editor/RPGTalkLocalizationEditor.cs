using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace RPGTALK.Localization
{

    [CustomEditor(typeof(RPGTalkLocalization))]
    public class RPGTalkLocalizationEditor : Editor
    {

        override public void OnInspectorGUI()
        {
            serializedObject.Update();

            //Instance of our RPGTalkLocalization class
            RPGTalkLocalization localization = (RPGTalkLocalization)target;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.HelpBox("Put into the array below every language that you have. To create a new Language, simply go to your project, click with the right button, go to Create/Rpgtalk/Language ", MessageType.Info, true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("languages"), true);

            EditorGUILayout.Space();

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("Default Language: " + LanguageSettings.defaultLanguage);
                EditorGUILayout.LabelField("Actual Language: " + LanguageSettings.actualLanguage);
            }
            else
            {
                EditorGUILayout.LabelField("Debug only available on Play Mode.");
            }



            EditorGUILayout.HelpBox("You can change the actual language by script calling LanguageSettings.actualLanguage", MessageType.Info, true);


            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }
}