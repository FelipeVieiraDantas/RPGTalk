using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

using NodeEditorFramework.IO;

using GenericMenu = NodeEditorFramework.Utilities.GenericMenu;

using RPGTALK.Nodes;


namespace NodeEditorFramework.Standard
{
	public class NodeEditorInterface
	{
		public NodeEditorUserCache canvasCache;
		public Action<GUIContent> ShowNotificationAction;

		// GUI
		public string sceneCanvasName = "";
		public float toolbarHeight = 17;

		// Modal Panel
		public bool showModalPanel;
		public Rect modalPanelRect = new Rect(20, 50, 250, 70);
		public Action modalPanelContent;

		// IO Format modal panel
		private ImportExportFormat IOFormat;
		private object[] IOLocationArgs;
		private delegate bool? DefExportLocationGUI(string canvasName, ref object[] locationArgs);
		private delegate bool? DefImportLocationGUI(ref object[] locationArgs);
		private DefImportLocationGUI ImportLocationGUI;
		private DefExportLocationGUI ExportLocationGUI;

		public void ShowNotification(GUIContent message)
		{
			if (ShowNotificationAction != null)
				ShowNotificationAction(message);
		}

#region GUI

		public void DrawToolbarGUI(Rect rect)
		{
			rect.height = toolbarHeight;
			GUILayout.BeginArea (rect, NodeEditorGUI.toolbar);
			GUILayout.BeginHorizontal();
			float curToolbarHeight = 0;

            //RPGTalk removed to make it more simple to use only RPGTalk functions.

            if (GUILayout.Button("New File", NodeEditorGUI.toolbarButton, GUILayout.Width(70)))
            {
                if (EditorUtility.DisplayDialog("Are you sure you want to create a new canvas?", "Any unsaved changes on this one will be lost", "I'm sure", "Hold up...")){
                    canvasCache.NewNodeCanvas(typeof(RPGTalkNodeCanvas));
                }
            }

            if (GUILayout.Button("Load TXT", NodeEditorGUI.toolbarButton, GUILayout.Width(70)))
            {
                if (EditorUtility.DisplayDialog("Are you sure you want to load a new canvas?", "Any unsaved changes on this one will be lost", "I'm sure", "Hold up...")){
                    LoadTXTFile();
                }
            }

            if (GUILayout.Button("Save TXT", NodeEditorGUI.toolbarButton, GUILayout.Width(70)))
            {
                SaveTXTFile();
            }

            //RPGTalk commented to make the interface more simple to the end user
            /*
            if (GUILayout.Button("File", NodeEditorGUI.toolbarDropdown, GUILayout.Width(50)))
			{
				GenericMenu menu = new GenericMenu(!Application.isPlaying);

				// New Canvas filled with canvas types
				NodeCanvasManager.FillCanvasTypeMenu(ref menu, NewNodeCanvas, "New Canvas/");
				menu.AddSeparator("");

                menu.AddItem(new GUIContent("Save TXT"), false, SaveTXTFile);






                // Load / Save
#if UNITY_EDITOR
                menu.AddItem(new GUIContent("Load Canvas"), false, LoadCanvas);
				menu.AddItem(new GUIContent("Reload Canvas"), false, ReloadCanvas);
				menu.AddSeparator("");
				if (canvasCache.nodeCanvas.allowSceneSaveOnly)
				{
					menu.AddDisabledItem(new GUIContent("Save Canvas"));
					menu.AddDisabledItem(new GUIContent("Save Canvas As"));
				}
				else
				{
					menu.AddItem(new GUIContent("Save Canvas"), false, SaveCanvas);
					menu.AddItem(new GUIContent("Save Canvas As"), false, SaveCanvasAs);
				}
				menu.AddSeparator("");
#endif

				// Import / Export filled with import/export types
				ImportExportManager.FillImportFormatMenu(ref menu, ImportCanvasCallback, "Import/");
				if (canvasCache.nodeCanvas.allowSceneSaveOnly)
				{
					menu.AddDisabledItem(new GUIContent("Export"));
				}
				else
				{
					ImportExportManager.FillExportFormatMenu(ref menu, ExportCanvasCallback, "Export/");
				}
				menu.AddSeparator("");

				// Scene Saving
				string[] sceneSaves = NodeEditorSaveManager.GetSceneSaves();
				if (sceneSaves.Length <= 0) // Display disabled item
					menu.AddItem(new GUIContent("Load Canvas from Scene"), false, null);
				else foreach (string sceneSave in sceneSaves) // Display scene saves to load
						menu.AddItem(new GUIContent("Load Canvas from Scene/" + sceneSave), false, LoadSceneCanvasCallback, sceneSave);
				menu.AddItem(new GUIContent("Save Canvas to Scene"), false, SaveSceneCanvasCallback);

				// Show dropdown
				menu.Show(new Vector2(5, toolbarHeight));
			}*/
			curToolbarHeight = Mathf.Max(curToolbarHeight, GUILayoutUtility.GetLastRect().yMax);

			GUILayout.Space(10);
			GUILayout.FlexibleSpace();

            //RPGTalk removed to make it more simple
			/*GUILayout.Label(new GUIContent("" + canvasCache.nodeCanvas.saveName + " (" + (canvasCache.nodeCanvas.livesInScene ? "Scene Save" : "Asset Save") + ")", 
											"Opened Canvas path: " + canvasCache.nodeCanvas.savePath), NodeEditorGUI.toolbarLabel);
			GUILayout.Label("Type: " + canvasCache.typeData.DisplayString, NodeEditorGUI.toolbarLabel);
			curToolbarHeight = Mathf.Max(curToolbarHeight, GUILayoutUtility.GetLastRect().yMax);

			GUI.backgroundColor = new Color(1, 0.3f, 0.3f, 1);
			if (GUILayout.Button("Force Re-init", NodeEditorGUI.toolbarButton, GUILayout.Width(100)))
			{
				NodeEditor.ReInit(true);
				canvasCache.nodeCanvas.Validate();
			} */
#if !UNITY_EDITOR
			GUILayout.Space(5);
			if (GUILayout.Button("Quit", NodeEditorGUI.toolbarButton, GUILayout.Width(100)))
				Application.Quit ();
#endif
			curToolbarHeight = Mathf.Max(curToolbarHeight, GUILayoutUtility.GetLastRect().yMax);
			GUI.backgroundColor = Color.white;

			GUILayout.EndHorizontal();
			GUILayout.EndArea();
			if (Event.current.type == EventType.Repaint)
				toolbarHeight = curToolbarHeight;
		}

		private void SaveSceneCanvasPanel()
		{
			GUILayout.Label("Save Canvas To Scene");

			GUILayout.BeginHorizontal();
			sceneCanvasName = GUILayout.TextField(sceneCanvasName, GUILayout.ExpandWidth(true));
			bool overwrite = NodeEditorSaveManager.HasSceneSave(sceneCanvasName);
			if (overwrite)
				GUILayout.Label(new GUIContent("!!!", "A canvas with the specified name already exists. It will be overwritten!"), GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Cancel"))
				showModalPanel = false;
			if (GUILayout.Button(new GUIContent(overwrite ? "Overwrite" : "Save", "Save the canvas to the Scene")))
			{
				showModalPanel = false;
				if (!string.IsNullOrEmpty (sceneCanvasName))
					canvasCache.SaveSceneNodeCanvas(sceneCanvasName);
			}
			GUILayout.EndHorizontal();
		}

		public void DrawModalPanel()
		{
			if (showModalPanel)
			{
				if (modalPanelContent == null)
					return;
				GUILayout.BeginArea(modalPanelRect, NodeEditorGUI.nodeBox);
				modalPanelContent.Invoke();
				GUILayout.EndArea();
			}
		}

#endregion

#region Menu Callbacks

		private void NewNodeCanvas(Type canvasType)
		{
			canvasCache.NewNodeCanvas(canvasType);
		}

#if UNITY_EDITOR

        class FollowUpNode
        {
            public Node node;
            public string followUpTitle;
            public int identifier;
        }

        List<FollowUpNode> followUpNodes;
        List<RPGTalkNode> createdNodes;

        // RPGTalk added to load the TXT!
        void LoadTXTFile()
        {
            //Get the actual selected path
            string selectedPath = "Assets";
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                selectedPath = AssetDatabase.GetAssetPath(obj);
                if (File.Exists(selectedPath))
                {
                    selectedPath = Path.GetDirectoryName(selectedPath);
                }
                break;
            }
            string path = EditorUtility.OpenFilePanel("Load dialog TXT", selectedPath, "txt");

            //first, clear the canvas.
            canvasCache.NewNodeCanvas(typeof(RPGTalkNodeCanvas));

            // read the TXT file into the elements list
            StreamReader reader = new StreamReader(path);
            string txtFile = reader.ReadToEnd();
            reader.Close();
            StringReader sReader = new StringReader(txtFile);

            string line = sReader.ReadLine();
            int currentLine = 0;

            if (line == null)
            {
                Debug.LogError("There was an error reading your file! Check your encoding settings.");
                return;
            }

            RPGTalkNode fatherNode = null;
            Vector2 position = Vector2.zero;

            followUpNodes = new List<FollowUpNode>();
            createdNodes = new List<RPGTalkNode>();

            while (line != null)
            {
                //If the line is empty, we will just ignore it
                if (string.IsNullOrEmpty(line))
                {
                    line = sReader.ReadLine();
                    currentLine++;
                    continue;
                }




                //Lets check with this is an oppening node
                int titleLine = line.IndexOf("[title=");
                if (titleLine != -1)
                {
                    string title = "";
                    int titleEnd = line.IndexOf("]",titleLine);
                    if (titleEnd != -1)
                    {
                        title = line.Substring(titleLine + 7, titleEnd - (titleLine + 7));
                    }
                    else
                    {
                        Debug.LogError("Error reading title");
                    }

                    if (title.Length > 0)
                    {
                        //Good. We got a title. Let's find out if it is an oppening or a closing one
                        if (title.IndexOf("_begin") != -1)
                        {
                            title = title.Substring(0, title.IndexOf("_begin"));

                            CreateNode(ref line, ref position, ref currentLine, title, ref sReader, ref fatherNode,true);

                            continue;
                        }
                        else if (title.IndexOf("_end") != -1)
                        {
                            //end last title node
                            position.y += 250;
                            position.x = 0;
                            fatherNode = null;

                            line = sReader.ReadLine();
                            currentLine++;
                            continue;
                        }
                        else
                        {
                            Debug.LogError("Right now, Node Editor only reads TXT made with node editor. You can change your TXT title to have _begin and _end tags");
                        }


                       
                    }

                }// end if title



                //Let's check if this line is a save
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

                                //Good, this a valid save. Let's create the node
                                RPGTalkSaveNode thisNode = Node.Create("rpgtalkSaveNode", position) as RPGTalkSaveNode;
                                position.x += 300;

                                thisNode.modifier = intModifier;
                                thisNode.savedData = newSavedData;

                                //Lets keep this node to move and connect later
                                FollowUpNode followUp = new FollowUpNode();
                                followUp.node = thisNode;
                                followUp.followUpTitle = newLineToStart;
                                //followUp.identifier = choiceNum;
                                line = sReader.ReadLine();
                                currentLine++;
                                followUpNodes.Add(followUp);


                                //Connect the node
                                if (fatherNode != null)
                                {
                                    ConnectionPort myPort = null;
                                    foreach (ConnectionPort port in thisNode.connectionPorts)
                                    {
                                        if (port.direction == Direction.In)
                                        {
                                            myPort = port;
                                            break;
                                        }
                                    }

                                    foreach (ConnectionPort port in fatherNode.connectionPorts)
                                    {
                                        if (port.direction == Direction.Out)
                                        {
                                            port.ApplyConnection(myPort);
                                            break;
                                        }
                                    }
                                }


                                continue;
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








                //This is none of the special lines. So it must be a common node.
                CreateNode(ref line, ref position, ref currentLine, "", ref sReader, ref fatherNode);

            }



            //Everything created. Everthing fine. Let's now add connections to the follow up nodes (choices and saves)
            foreach (FollowUpNode follow in followUpNodes)
            {
                //the follow up node is exclusive to the prior node? if it isn't, we don't want to move it. Just add the connection
                if (follow.followUpTitle.IndexOf("FollowUp_") != -1 && follow.followUpTitle.IndexOf("Choice_" + follow.identifier.ToString()) != -1)
                {
                    //TODO: Move the followups

                }


                //Find out what node should if be connected to
                foreach (RPGTalkNode talkNode in createdNodes)
                {
                    string removeBegin = follow.followUpTitle.Substring(0, follow.followUpTitle.LastIndexOf("_begin"));

                    if (talkNode.CutsceneTitle == removeBegin)
                    {


                        //Connect the nodes
                        ConnectionPort myPort = null;
                        foreach (ConnectionPort port in follow.node.connectionPorts)
                        {
                            if (port.direction == Direction.Out)
                            {
                                myPort = port;
                                break;
                            }
                        }

                        foreach (ConnectionPort port in talkNode.connectionPorts)
                        {
                            if (port.direction == Direction.In)
                            {
                                port.ApplyConnection(myPort);
                                break;
                            }
                        }


                        break;
                    }
                }




            }



        }

        void CreateNode(ref string line, ref Vector2 position, ref int currentLine, string cutsceneTitle, ref StringReader sReader, ref RPGTalkNode fatherNode, bool startNode = false)
        {

            RPGTalkNode thisNode = null;
            int characterID = 0;
            int expressionID = 0;
            string dialoger;


            //Good. Let's create the node
            thisNode = Node.Create("rpgtalkNode", position) as RPGTalkNode;
            position.x += 400;

            //If it is a [title] line, we don't want to actually read it, but the line after it
            if (startNode)
            {
                thisNode.CutsceneTitle = cutsceneTitle;
                line = sReader.ReadLine();
                currentLine++;
            }

            //dialoger
            GetDialoger(line, out line, out characterID, out expressionID, out dialoger);
            thisNode.characterID = characterID;
            thisNode.expressionID = expressionID;

            //questions
            string questionID;
            GetQuestion(line, out line, out questionID);

            thisNode.DialogLine = line;


            //Connect the node
            if (fatherNode != null)
            {
                ConnectionPort myPort = null;
                foreach (ConnectionPort port in thisNode.connectionPorts)
                {
                    if (port.direction == Direction.In)
                    {
                        myPort = port;
                        break;
                    }
                }

                foreach (ConnectionPort port in fatherNode.connectionPorts)
                {
                    if (port.direction == Direction.Out)
                    {
                        port.ApplyConnection(myPort);
                        break;
                    }
                }
            }

            fatherNode = thisNode;


            if (questionID.Length > 0)
            {
                thisNode.questionID = questionID;
                line = sReader.ReadLine();
                currentLine++;
                int choices = 0;
                float originalY = position.y;
                while (line != null && line.IndexOf("[choice]") != -1)
                {
                    choices++;
                    if (choices % 2 == 0)
                    {
                        position.y = originalY + 100;
                    }
                    else
                    {
                        position.y = originalY - 40;
                    }

                    CreateChoiceNode(ref line, ref position, ref fatherNode ,choices, ref sReader, ref currentLine);

                    line = sReader.ReadLine();
                    currentLine++;

                    if (choices % 2 == 0 || line == null || line.IndexOf("[choice]") == -1)
                    {
                        position.x += 180;
                    }

                    continue;
                }
                position.y = originalY;
            }
            else
            {

                line = sReader.ReadLine();
                currentLine++;
            }


            createdNodes.Add(thisNode);
        }

        void GetNewTalkTag(ref string line, ref Vector2 position, ref int currentLine, ref StringReader sReader, Node fatherNode, int choiceNum = -1)
        {

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
                    if (endOfStart == -1)
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
                            line = line.Substring(0, initialBracket) + line.Substring(finalBracket + 1);

                            //Lets keep this node to move and connect later
                            FollowUpNode followUp = new FollowUpNode();
                            followUp.node = fatherNode;
                            followUp.followUpTitle = newLineToStart;
                            followUp.identifier = choiceNum;
                            followUpNodes.Add(followUp);



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
        }

        void CreateChoiceNode(ref string line, ref Vector2 position, ref RPGTalkNode fatherNode, int choiceNum, ref StringReader sReader, ref int currentLine)
        {
            int initialBracket = line.IndexOf("[choice]");
            //Ok! Let's isolate its string
            line = line.Substring(initialBracket + 8);

            RPGTalkChoiceNode choiceNode = Node.Create("rpgtalkChoiceNode", position) as RPGTalkChoiceNode;


            //Connect the node
            ConnectionPort myPort = null;
            foreach (ConnectionPort port in choiceNode.connectionPorts)
            {
                if (port.direction == Direction.In)
                {
                    myPort = port;
                    break;
                }
            }

            foreach (ConnectionPort port in fatherNode.connectionPorts)
            {
                if (port.direction == Direction.Out)
                {
                    port.ApplyConnection(myPort);
                    break;
                }
            }


            //Let's check if it has a follow up
            GetNewTalkTag(ref line, ref position, ref currentLine, ref sReader, choiceNode, choiceNum - 1);

            choiceNode.DialogLine = line;
        }

        void GetQuestion(string line, out string returnLine, out string questionID)
        {
            questionID = "";
            //check if the user have some question and the line asks for one
            if (line.IndexOf("[question=") != -1 && line.IndexOf("]", line.IndexOf("[question=")) != -1)
            {
                int initialBracket = line.IndexOf("[question=");
                int finalBracket = line.IndexOf("]", initialBracket);

                //Ok, new question around! Let's get its id
                questionID = line.Substring(initialBracket + 10, finalBracket - (initialBracket + 10));
                line = line.Substring(0, initialBracket) + line.Substring(finalBracket + 1);
            }

            returnLine = line;
        }

        void GetDialoger(string line, out string returnLine, out int characterID, out int expressionID, out string dialoger)
        {
            returnLine = line;
            characterID = 0;
            expressionID = 0;
            dialoger = "";
            if (line.IndexOf(':') != -1)
            {
                string[] splitLine = line.Split(new char[] { ':' }, 2);
                dialoger = splitLine[0].Trim();
                returnLine = splitLine[1].Trim();
                RPGTALK.Helper.RPGTalkCharacter[] characters = (NodeEditor.curNodeCanvas as RPGTalkNodeCanvas).GetAllCharactersInGame();
                for (int i = 0; i < characters.Length; i++)
                {
                    if (characters[i].dialoger == dialoger)
                    {
                        characterID = i + 1;

                        //expression
                        string expression = GetExpression(returnLine, out returnLine);
                        string[] expressions = (NodeEditor.curNodeCanvas as RPGTalkNodeCanvas).GetExpressionsNamesByCharacterName(dialoger);
                        for (int j = 0; j < expressions.Length; j++)
                        {
                            if (expressions[j] == expression)
                            {
                                expressionID = j;
                                break;
                            }
                        }
                        break;
                    }
                }

            }
        }


        string GetExpression(string line, out string returnLine)
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
                       
                            returnLine = line.Substring(0, initialBracket) +
                                line.Substring(finalBracket + 1);
                       
                            return line.Substring(initialBracket + 12, finalBracket - (initialBracket + 12));


                    }
                    else
                    {
                        Debug.LogWarning("Found a [expression=x] variable in the text but something is wrong with it. Check The spelling");
                    }


                }

            }

           
                returnLine = line;
          
                return "";

        }

        // RPGTalk added to save the TXT!
        void SaveTXTFile()
        {
            RPGTalkNodeCanvas canvas = (NodeEditor.curNodeCanvas as RPGTalkNodeCanvas);
            string file = "";
            //force remaking the lines
            foreach(RPGTalkNode node in canvas.GetStartCutsceneNodes())
            {
                file += "[title=" + node.CutsceneTitle + "_begin]\n";
                int actualLine = file.Split('\n').Length;
                file += node.GetText(actualLine);
                //file += node.GetText();
                file += "[title=" + node.CutsceneTitle + "_end]\n";
            }
            file = "";
            //Actually write
            foreach (RPGTalkNode node in canvas.GetStartCutsceneNodes())
            {
                file += "[title=" + node.CutsceneTitle + "_begin]\n";
                file += node.GetText();
                file += "[title=" + node.CutsceneTitle + "_end]\n";
            }

            string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Save Dialog as TXT", "NewDialog", "txt", "");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, file);
            }

        }

        private void LoadCanvas()
		{
			string path = UnityEditor.EditorUtility.OpenFilePanel("Load Node Canvas", NodeEditor.editorPath + "Resources/Saves/", "asset");
			if (!path.Contains(Application.dataPath))
			{
				if (!string.IsNullOrEmpty(path))
					ShowNotification(new GUIContent("You should select an asset inside your project folder!"));
			}
			else
				canvasCache.LoadNodeCanvas(path);
		}

		private void ReloadCanvas()
		{
			string path = canvasCache.nodeCanvas.savePath;
			if (!string.IsNullOrEmpty(path))
			{
				if (path.StartsWith("SCENE/"))
					canvasCache.LoadSceneNodeCanvas(path.Substring(6));
				else
					canvasCache.LoadNodeCanvas(path);
				ShowNotification(new GUIContent("Canvas Reloaded!"));
			}
			else
				ShowNotification(new GUIContent("Cannot reload canvas as it has not been saved yet!"));
		}

		private void SaveCanvas()
		{
			string path = canvasCache.nodeCanvas.savePath;
			if (!string.IsNullOrEmpty(path))
			{
				if (path.StartsWith("SCENE/"))
					canvasCache.SaveSceneNodeCanvas(path.Substring(6));
				else
					canvasCache.SaveNodeCanvas(path);
				ShowNotification(new GUIContent("Canvas Saved!"));
			}
			else
				ShowNotification(new GUIContent("No save location found. Use 'Save As'!"));
		}

		private void SaveCanvasAs()
		{
			string panelPath = NodeEditor.editorPath + "Resources/Saves/";
			if (canvasCache.nodeCanvas != null && !string.IsNullOrEmpty(canvasCache.nodeCanvas.savePath))
				panelPath = canvasCache.nodeCanvas.savePath;
			string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Save Node Canvas", "Node Canvas", "asset", "", panelPath);
			if (!string.IsNullOrEmpty(path))
				canvasCache.SaveNodeCanvas(path);
		}
#endif

		private void LoadSceneCanvasCallback(object canvas)
		{
			canvasCache.LoadSceneNodeCanvas((string)canvas);
			sceneCanvasName = canvasCache.nodeCanvas.name;
		}

		private void SaveSceneCanvasCallback()
		{
			modalPanelContent = SaveSceneCanvasPanel;
			showModalPanel = true;
		}

		private void ImportCanvasCallback(string formatID)
		{
			IOFormat = ImportExportManager.ParseFormat(formatID);
			if (IOFormat.RequiresLocationGUI)
			{
				ImportLocationGUI = IOFormat.ImportLocationArgsGUI;
				modalPanelContent = ImportCanvasGUI;
				showModalPanel = true;
			}
			else if (IOFormat.ImportLocationArgsSelection(out IOLocationArgs))
				canvasCache.SetCanvas(ImportExportManager.ImportCanvas(IOFormat, IOLocationArgs));
		}

		private void ImportCanvasGUI()
		{
			if (ImportLocationGUI != null)
			{
				bool? state = ImportLocationGUI(ref IOLocationArgs);
				if (state == null)
					return;

				if (state == true)
					canvasCache.SetCanvas(ImportExportManager.ImportCanvas(IOFormat, IOLocationArgs));

				ImportLocationGUI = null;
				modalPanelContent = null;
				showModalPanel = false;
			}
			else
				showModalPanel = false;
		}

		private void ExportCanvasCallback(string formatID)
		{
			IOFormat = ImportExportManager.ParseFormat(formatID);
			if (IOFormat.RequiresLocationGUI)
			{
				ExportLocationGUI = IOFormat.ExportLocationArgsGUI;
				modalPanelContent = ExportCanvasGUI;
				showModalPanel = true;
			}
			else if (IOFormat.ExportLocationArgsSelection(canvasCache.nodeCanvas.saveName, out IOLocationArgs))
				ImportExportManager.ExportCanvas(canvasCache.nodeCanvas, IOFormat, IOLocationArgs);
		}

		private void ExportCanvasGUI()
		{
			if (ExportLocationGUI != null)
			{
				bool? state = ExportLocationGUI(canvasCache.nodeCanvas.saveName, ref IOLocationArgs);
				if (state == null)
					return;

				if (state == true)
					ImportExportManager.ExportCanvas(canvasCache.nodeCanvas, IOFormat, IOLocationArgs);

				ImportLocationGUI = null;
				modalPanelContent = null;
				showModalPanel = false;
			}
			else
				showModalPanel = false;
		}

#endregion
	}
}
