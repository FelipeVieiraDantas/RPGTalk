using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;

namespace RPGTALK.Helper
{
    [InitializeOnLoad]
    public class RPGTalkInitialization : EditorWindow, IActiveBuildTargetChanged
    {

        public static RPGTalkInitialization instance;

        //The scriptable Object that keeps the saved data
        public static RPGTalkConfig configAsset;

        //Keep if the player finished steps of the configuration
        static bool doneTMP, doneEncoding;

        //Keep if the user has TMP in the project
        static bool hasTMP;

        public static TextAsset[] nonUTF8;

        Vector2 scrollPos;

        //Everytime unity initialize, we want to check if we already did the config. If we didn't, open the window.
        static RPGTalkInitialization()
        {
            if(instance != null)
            {
                return;
            }

            if (configAsset == null)
            {
                GetConfigAsset();
            }
            if(!configAsset.alreadyConfigured)
            {
                Config();
            }
        }

        void OnEnable()
        {
            instance = this;
            EditorApplication.LockReloadAssemblies();
        }

        void OnDisable()
        {
            EditorApplication.UnlockReloadAssemblies();
        }

        [MenuItem("RPGTalk/Configure")]
        public static void Config()
        {
            //This function will open the window
            doneTMP = false;
            doneEncoding = false;
            if (configAsset == null)
            {
                GetConfigAsset();
            }
            GetWindow<RPGTalkInitialization>("Configure RPGTalk");
        }

        private void OnGUI()
        {
            minSize = new Vector2(300, 300);
            if (configAsset == null)
            {
                EditorGUILayout.HelpBox("None or Multiple Config Asset were found. It should be on Scripts/Helpers", MessageType.Error, true);
                return;
            }




            GUI.skin.label.wordWrap = true;

            scrollPos = GUILayout.BeginScrollView(scrollPos);



            if (!doneTMP)
            {
                //RPGTalk Logo
                GUILayout.Label(configAsset.logo.texture);
                GUILayout.Label("Welcome to RPGTalk!");
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                GUILayout.Label("To make the most out of it, let's go to a simple configuration, shall we?");


                EditorGUILayout.Space();
                GUILayout.Label("Do you use or plan to use Text Mesh PRO?");

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Yes I do!"))
                {
                    doneTMP = true;
                    AddDefineIfNecessary("RPGTalk_TMP", BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
                    configAsset.usingTMP = true;
                    hasTMP = NamespaceExists("TMPro");
                }
                if (GUILayout.Button("No, I do not (Or I don't know what it is)"))
                {
                    doneTMP = true;
                    RemoveDefineIfNecessary("RPGTalk_TMP", BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
                    configAsset.usingTMP = false;
                    hasTMP = NamespaceExists("TMPro");
                }
                EditorGUILayout.EndHorizontal();
            }
            else if (!doneEncoding)
            {

                //RPGTalk Logo
                EditorGUI.DrawTextureTransparent(new Rect(0, 0, 50, 50), configAsset.logo.texture);
                EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("One of the main features of RPGTalk is to read .TXT files. So encoding should be a concern.");
                EditorGUILayout.HelpBox("What is encoding? \n Computer files are, after all, just bits of data. They are all aligned in a way that the computer can understand and show to us on the screen. But there are several languages around the world, some use different characters than the others, so the way that the computer reads those characters should be different as well. There are a lot of types of encoding, but the one that access most of the characters of the world is called Unicode.", MessageType.Info);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space();
                GUILayout.Label("Unity and RPGTalk reads TXT files with Unicode. This shouldn't be an issue if you are only writting ASCII languages (like english). But if you plan on have a language with accents, latin, asian or different types of characters, you should make sure your .TXT files are Unicode. UTF-8 should work fine.");
                EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space();
                GUILayout.Label("With that little context in mind, this step of the configuration is just a WARNING: make sure your .TXT file is Unicode (like UTF-8) before using it with RPGTalk. It is easy to convert. Google it up!");

                if (GUILayout.Button("Ok, I got it!"))
                {
                    doneEncoding = true;
                    SaveConifg();
                }

            
            }
            else
            {

                //RPGTalk Logo
                EditorGUI.DrawTextureTransparent(new Rect(0, 0, 50, 50), configAsset.logo.texture);
                EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space();

                if (hasTMP && !configAsset.usingTMP)
                {
                    EditorGUILayout.HelpBox("You said you won't use Text Mesh Pro but it is in your project. Be sure to check this configuration again if you change your mind.", MessageType.Warning, true);
                }
                if (!hasTMP && configAsset.usingTMP)
                {
                    EditorGUILayout.HelpBox("You said you will use Text Mesh Pro but it is not in your project. Be sure to add it and avoid errors.", MessageType.Warning, true);
                }

                EditorGUILayout.Space(); EditorGUILayout.Space();

                GUILayout.Label("All Set =)");
                if (GUILayout.Button("Finish!"))
                {
                    this.Close();
                }
            }




            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("You can change this settings anytime going to the menu RPGTalk/Configure");

            EditorGUILayout.EndScrollView();

        }

        void SaveConifg()
        {
            configAsset.alreadyConfigured = true;
            EditorUtility.SetDirty(configAsset);
            AssetDatabase.SaveAssets();
        }

        public bool NamespaceExists(string desiredNamespace)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Namespace == desiredNamespace)
                        return true;
                }
            }
            return false;
        }

        //Find out where is the config asset
        static void GetConfigAsset()
        {
            string[] assetPath = AssetDatabase.FindAssets("t:RPGTalkConfig");
            if(assetPath.Length == 0 || assetPath.Length > 1)
            {
                configAsset = null;
                Debug.LogWarning("Couldn't fint the RPGTalk Configuration file in your project D= This will cause bad erros!");
                return;
            }
            configAsset = AssetDatabase.LoadAssetAtPath<RPGTalkConfig>(AssetDatabase.GUIDToAssetPath(assetPath[0]));
        }




        //needed to use the interface IActiveBuildTargetChanged
        public int callbackOrder { get { return 0; } }

        //Called by IActiveBuildTargetChanged everytime the player changed the build platform. Write the Defines 
        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            RemoveDefineIfNecessary("RPGTalk_TMP", BuildPipeline.GetBuildTargetGroup(previousTarget));
            AddDefineIfNecessary("RPGTalk_TMP", BuildPipeline.GetBuildTargetGroup(newTarget));
        }

        //Add defines to player settings so we can use #if ...
        public static void AddDefineIfNecessary(string _define, BuildTargetGroup _buildTargetGroup)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(_buildTargetGroup);

            if (defines == null) { defines = _define; }
            else if (defines.Length == 0) { defines = _define; }
            else { if (defines.IndexOf(_define, 0) < 0) { defines += ";" + _define; } }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(_buildTargetGroup, defines);
        }

        //Remove defines to player settings
        public static void RemoveDefineIfNecessary(string _define, BuildTargetGroup _buildTargetGroup)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(_buildTargetGroup);

            if (defines.StartsWith(_define + ";"))
            {
                // First of multiple defines.
                defines = defines.Remove(0, _define.Length + 1);
            }
            else if (defines.StartsWith(_define))
            {
                // The only define.
                defines = defines.Remove(0, _define.Length);
            }
            else if (defines.EndsWith(";" + _define))
            {
                // Last of multiple defines.
                defines = defines.Remove(defines.Length - _define.Length - 1, _define.Length + 1);
            }
            else
            {
                // Somewhere in the middle or not defined.
                var index = defines.IndexOf(_define, 0, System.StringComparison.Ordinal);
                if (index >= 0) { defines = defines.Remove(index, _define.Length + 1); }
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(_buildTargetGroup, defines);
        }
    }
}