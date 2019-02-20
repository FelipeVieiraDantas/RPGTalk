using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGTALK.Localization;

[CustomEditor(typeof(RPGTalkLanguage))]
public class RPGTalkLanguageEditor : Editor
{

    bool firstRun;
    RPGTalkLanguage main;

    override public void OnInspectorGUI()
    {
        //Instance of our RPGTalk class
        RPGTalkLanguage language = (RPGTalkLanguage)target;

        if (!firstRun)
        {
            firstRun = true;
            GetMainLanguage(language);
        }


        serializedObject.Update();

        GUI.skin.label.wordWrap = true;



        EditorGUI.BeginChangeCheck();

        GUILayout.Label("Put below the identifier name of this language:");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("identifier"), GUIContent.none);
        EditorGUILayout.HelpBox("Aside from identification, you can use this variable later on to find out what language the game is currently at. It usually is some code like \"EN_US\", \"FR\", \"PT_BR\" and so on ", MessageType.Info, true);

        EditorGUILayout.Space();
        if (!language.mainLanguage)
        {
            if(main == null)
            {
                GUILayout.Label("There is no main language of your game. You should make one!");
            }
            else
            {
                GUILayout.Label("The main language of your game is "+main.identifier+". You can change that anytime if you want.");
            }
            if(GUILayout.Button("Make this the main language") &&
            EditorUtility.DisplayDialog("Are you sure you want to make " + language.identifier + " the main language of your game?", 
            "Every other language will be based on this one. Your game will start in that language and will fallback to it if anything goes wrong",    
            "Yes I am!", "No, hold up..."))
            {
                language.mainLanguage = true;
                if(main != null)
                    main.mainLanguage = false;
                main = language;
            }

        }
        else
        {
            GUILayout.Label("This is the main language of your game.");
        }
        EditorGUILayout.HelpBox("The main language sets the sample files to any other languages you have. It is the language your game will start with and the language it will use if anything goes wrong. If you mark this as your main language, any other wont be anymore. Every TXT you put in your scene must be in your Main Language.", MessageType.Info, true);

        EditorGUILayout.Space();
        if (language.mainLanguage)
        {
            GUILayout.Label("Set below every txt file that will be used on your game, in this current language.");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("txts"), true);
            EditorGUI.indentLevel--;
        }
        else if (main == null)
        {
            EditorGUILayout.HelpBox("Something went terribly wrong. No Main Language was found. Please set one to continue.", MessageType.Error, true);
        }
        else{
            GUILayout.Label("Set below every txt file according to wich one it represents of the main language");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Main Language ("+main.identifier+ ")");
            EditorGUILayout.LabelField(language.identifier);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();

            for (int i = 0; i < serializedObject.FindProperty("txts").arraySize; i++)
            {
                if (main.txts.Length-1 < i || main.txts[i] == null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("txts").GetArrayElementAtIndex(i),
                new GUIContent("ELEMENT NONEXISTENT IN MAIN LANGUAGE"));
                }
                else
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("txts").GetArrayElementAtIndex(i),
                new GUIContent(main.txts[i].name));
                }

            }


            EditorGUILayout.EndVertical();

        }

        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }

   


    //Search Every Language asset to see who is the main language
    void GetMainLanguage(RPGTalkLanguage myself)
    {
        string[] assetPath = AssetDatabase.FindAssets("t:RPGTalkLanguage");
        if (assetPath.Length == 0)
        {
            main = null;
            return;
        }
        foreach(string path in assetPath)
        {
            RPGTalkLanguage lang = AssetDatabase.LoadAssetAtPath<RPGTalkLanguage>(AssetDatabase.GUIDToAssetPath(path));
            if (lang.mainLanguage)
            {
                main = lang;

                //If it isn't me, let's make sure I have the same amount of TXTs that the main language
                if(main != myself)
                {
                    if(main.txts.Length != myself.txts.Length)
                    {
                        TextAsset[] newArray = new TextAsset[main.txts.Length];
                        //We would want to keep any TXTs in the old array though
                        for (int i = 0; i < newArray.Length; i++)
                        {
                            if (myself.txts.Length > i)
                            {
                                newArray[i] = myself.txts[i];
                            }
                            else
                            {
                                break;
                            }
                        }
                        myself.txts = newArray;
                    }
                }


                break;
            }
        }
    }
}