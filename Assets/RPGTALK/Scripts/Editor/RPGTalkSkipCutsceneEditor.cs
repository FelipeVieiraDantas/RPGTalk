using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RPGTalkSkipCutscene))]
public class RPGTalkSkipCutsceneEditor : Editor
{
    override public void OnInspectorGUI()
    {
        serializedObject.Update();

        //Instance of our RPGTalkLocalization class
        RPGTalkSkipCutscene skip = (RPGTalkSkipCutscene)target;

        EditorGUI.BeginChangeCheck();


        if (!skip.gameObject.GetComponent<RPGTalk>())
        {
            EditorGUILayout.HelpBox("This component should be along with a RPGTalk", MessageType.Error);
        }



        EditorGUILayout.BeginVertical((GUIStyle)"HelpBox");
        EditorGUILayout.LabelField("How the user should skip the cutscene?", EditorStyles.boldLabel);



        EditorGUILayout.LabelField("Key that needs to be pressed to skip:");
        skip.keyToSkip = (KeyCode)EditorGUILayout.EnumPopup(skip.keyToSkip);
        EditorGUILayout.LabelField("It can also be skipped with some button set on Project Settings > Input:");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("buttonToSkip"), true);
        skip.skipWithMouse = GUILayout.Toggle(skip.skipWithMouse, "Also skip with Mouse Click?");

        skip.needToSkipTwice = GUILayout.Toggle(skip.needToSkipTwice, "Does the user have to press the button twice?");
        if (skip.needToSkipTwice)
        {
            EditorGUILayout.LabelField("How maximum time can be between the interactions to actually skip?");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("timeBetweenSkips"), true);
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical((GUIStyle)"HelpBox");
        EditorGUILayout.LabelField("What should happen when skipping?", EditorStyles.boldLabel);
        if (skip.needToSkipTwice)
        {
            EditorGUILayout.LabelField("When the user presses the button for the first time:");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnFirstTwiceSkip"), true);
            EditorGUILayout.LabelField("When the user DON'T press the button for the second time:");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnCancelTwiceSkip"), true);
            EditorGUILayout.Space();
        }
        EditorGUILayout.LabelField("When the skip starts to happen:");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnSkip"), true);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical((GUIStyle)"HelpBox");
        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);

        skip.canSkip = GUILayout.Toggle(skip.canSkip, "Can this cutscene be skipped?");
        skip.jumpQuestions = GUILayout.Toggle(skip.jumpQuestions, "If there are questions in the cutscene, it should be skipped as well?");
        EditorGUILayout.LabelField("When skipped, delay a few seconds to actually finish the talk:");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("delaySkip"), true);

        EditorGUILayout.EndVertical();



        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}
