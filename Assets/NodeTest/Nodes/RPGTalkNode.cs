using System;
using System.Collections.Generic;
using NodeEditorFramework;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;

namespace RPGTALK.Nodes
{

    /// <summary>
    /// basic node class for a dialog line
    /// </summary>
    [Node(false, "Rpgtalk/Dialog Node")]
    public class RPGTalkNode : Node
    {

        public override string Title
        {
            get
            {
                if (startOfCutscene)
                {
                    return CutsceneTitle + " - Line: " + lineInTxt.ToString();
                }
                else if (attachedTo != null)
                {
                    string nameToGo = "";
                    RPGTalkNode lookingInto = attachedTo;
                    while (lookingInto != null)
                    {
                        nameToGo = lookingInto.CutsceneTitle;
                        lookingInto = lookingInto.attachedTo;
                    }
                    return "Attached to: " + nameToGo + " - Line: " + lineInTxt.ToString();
                }
                else
                {
                    return "Not start but not attached to anything";
                }
            }
        }

        public override bool AllowRecursion { get { return true; } }
        public Type GetObjectType { get { return typeof(RPGTalkNode); } }

        public override Vector2 MinSize { get { return new Vector2(350, 200); } }
        public override bool AutoLayout { get { return true; } }  //resizable renamed to autolayout?

        private const string Id = "rpgtalkNode";
        public override string GetID { get { return Id; } }

        [FormerlySerializedAs("SayingCharacterPotrait")]
        public Sprite CharacterPotrait;
        [FormerlySerializedAs("WhatTheCharacterSays")]
        public string DialogLine;

        public string CutsceneTitle;
        public bool startOfCutscene;
        public RPGTalkNode attachedTo;
        string[] characters;
        public int characterID;
        int oldCharacterID = -1;

        public string[] expressions;
        public int expressionID;
        int oldExpressionID;

        public List<RPGTalkChoiceNode> choices = new List<RPGTalkChoiceNode>();
        public string questionID = "Type your question ID here";
        public RPGTalkChoiceNode attachedToChoice;

        public int lineInTxt;

        public List<RPGTalkSaveNode> saves = new List<RPGTalkSaveNode>();
        public RPGTalkSaveNode attachedToSave;


        //Next Node to go to
        [ValueConnectionKnob("To Where", Direction.Out, "RPGTalkForward", NodeSide.Right, MaxConnectionCount = ConnectionCount.Multi)]
        public ValueConnectionKnob toWhereOUT;
        [ConnectionKnob("From Where", Direction.In, "RPGTalkForward", NodeSide.Left, MaxConnectionCount = ConnectionCount.Multi)]
        public ConnectionKnob fromWhereIN;

        private Vector2 scroll;

        protected override void OnCreate()
        {
            DialogLine = "This is what will be said";
            CharacterPotrait = null; 
            CutsceneTitle = "Cutscene_" + (NodeEditor.curNodeCanvas as RPGTalkNodeCanvas).GetCorrectCutsceneAutoTitle().ToString();
            startOfCutscene = true;
            characters = (NodeEditor.curNodeCanvas as RPGTalkNodeCanvas).GetallCharactersNames();
            lineInTxt = (NodeEditor.curNodeCanvas as RPGTalkNodeCanvas).GetNextLineInTxt() + 1; //when create, it doesn't count with myself.
        }

        public override void NodeGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            if (startOfCutscene)
            {
                CutsceneTitle = EditorGUILayout.TextField("", CutsceneTitle);
            }
            GUILayout.BeginHorizontal();
            CharacterPotrait = (Sprite)EditorGUILayout.ObjectField(CharacterPotrait, typeof(Sprite), false, GUILayout.Width(65f), GUILayout.Height(65f));
            EditorGUILayout.BeginVertical();
            if (characters != null)
            {
                characterID = EditorGUILayout.Popup(characterID, characters);
                if (characterID != oldCharacterID)
                {
                    oldCharacterID = characterID;
                    if (characterID == 0)
                    {
                        CharacterPotrait = null;
                        expressions = null;
                        return;
                    }
                    CharacterPotrait = (NodeEditor.curNodeCanvas as RPGTalkNodeCanvas).GetAllCharactersInGame()[characterID - 1].photo;
                    expressions = (NodeEditor.curNodeCanvas as RPGTalkNodeCanvas).GetExpressionsNamesByCharacterName(characters[characterID - 1]);
                }
            }
            if (expressions != null && expressions.Length > 1)
            {
                expressionID = EditorGUILayout.Popup(expressionID, expressions);
                if (expressionID != oldExpressionID)
                {
                    oldExpressionID = expressionID;
                    if (expressionID == 0)
                    {
                        CharacterPotrait = (NodeEditor.curNodeCanvas as RPGTalkNodeCanvas).GetAllCharactersInGame()[characterID - 1].photo;
                        return;
                    }
                    CharacterPotrait = (NodeEditor.curNodeCanvas as RPGTalkNodeCanvas).expressions[expressionID - 1].photo;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            if (choices.Count > 0)
            {
                questionID = EditorGUILayout.TextField("Question ID:", questionID);
            }
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();

            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(100));
            EditorStyles.textField.wordWrap = true;
            DialogLine = EditorGUILayout.TextArea(DialogLine, GUILayout.ExpandHeight(true), GUILayout.Width(340));
            EditorStyles.textField.wordWrap = false;
            EditorGUILayout.EndScrollView();
            GUILayout.EndHorizontal();

        }


        protected internal override void OnAddConnection(ConnectionPort port, ConnectionPort connection)
        {
            base.OnAddConnection(port, connection);
            if (port.direction == Direction.In)
            {

                if (connection.body.isChildOf(this))
                {
                    return;
                }

                if (connection.body is RPGTalkNode)
                {
                    startOfCutscene = false;
                    if (attachedTo == null)
                    {
                        attachedTo = connection.body as RPGTalkNode;
                    }
                }
                else if (connection.body is RPGTalkChoiceNode)
                {
                    if (attachedToChoice == null)
                    {
                        attachedToChoice = connection.body as RPGTalkChoiceNode;
                    }
                    //I am the result of a choice
                    if (attachedToChoice.attachedTo != null)
                    {
                        CutsceneTitle = "FollowUp_" + attachedToChoice.attachedTo.CutsceneTitle + "_" + attachedToChoice.Title;
                    }
                    else
                    {
                        CutsceneTitle = "FollowUp_" + attachedToChoice.Title;
                    }
                }
                else
                {
                    if (attachedToSave == null)
                    {
                        attachedToSave = connection.body as RPGTalkSaveNode;
                    }
                    //I am the result of a choice
                    if (attachedToSave.attachedTo != null)
                    {
                        CutsceneTitle = "FollowUp_" + attachedToSave.attachedTo.CutsceneTitle + "_" + attachedToSave.Title;
                    }
                    else
                    {
                        CutsceneTitle = "FollowUp_" + attachedToSave.Title;
                    }
                }
            }
            else
            {

                //make sure that will be only one connection to RPGTalkNode
                List<ConnectionPort> toDelete = new List<ConnectionPort>();
                foreach (ConnectionPort connected in outputPorts[0].connections)
                {
                    if (connection.body is RPGTalkNode)
                    {
                        if (connected != connection)
                        {
                            toDelete.Add(connected);
                        }
                    }
                    else if (connection.body is RPGTalkChoiceNode)
                    {
                        if (connected != connection && (connected.body is RPGTalkNode || connected.body is RPGTalkSaveNode))
                        {
                            toDelete.Add(connected);
                        }
                    }
                    else
                    {
                        if (connected != connection && (connected.body is RPGTalkNode || connected.body is RPGTalkChoiceNode))
                        {
                            toDelete.Add(connected);
                        }
                    }
                }


                foreach (ConnectionPort deleteMe in toDelete)
                {
                    outputPorts[0].RemoveConnection(deleteMe);
                }




                //Add choice
                if (connection.body is RPGTalkChoiceNode)
                {
                    choices.Add(connection.body as RPGTalkChoiceNode);
                }

                //Add save
                if (connection.body is RPGTalkSaveNode)
                {
                    saves.Add(connection.body as RPGTalkSaveNode);
                }


            }


            (NodeEditor.curNodeCanvas as RPGTalkNodeCanvas).GetNextLineInTxt();

        }

        protected internal override void OnRemoveConnection(ConnectionPort port, ConnectionPort connection)
        {
            base.OnRemoveConnection(port, connection);
            if (port.direction == Direction.In)
            {
                if (attachedTo == connection.body as RPGTalkNode)
                {
                    if (port.connections.Count == 0)
                    {
                        startOfCutscene = true;
                        attachedTo = null;
                        CutsceneTitle = "Cutscene_" + (NodeEditor.curNodeCanvas as RPGTalkNodeCanvas).GetCorrectCutsceneAutoTitle().ToString();
                    }
                    else
                    {
                        attachedTo = port.connections[0].body as RPGTalkNode;
                    }
                }
                else if (attachedToChoice == connection.body as RPGTalkChoiceNode)
                {

                    //removed choice I came from
                    attachedToChoice = null;
                    CutsceneTitle = "Cutscene_" + (NodeEditor.curNodeCanvas as RPGTalkNodeCanvas).GetCorrectCutsceneAutoTitle().ToString();

                }
                else if (attachedToChoice == connection.body as RPGTalkSaveNode)
                {
                    //removed choice I came from
                    attachedToSave = null;
                    CutsceneTitle = "Cutscene_" + (NodeEditor.curNodeCanvas as RPGTalkNodeCanvas).GetCorrectCutsceneAutoTitle().ToString();
                }

            }
            else
            {

                //Remove choice
                if (connection.body is RPGTalkChoiceNode)
                {
                    choices.Remove(connection.body as RPGTalkChoiceNode);
                }

                //Remove choice
                if (connection.body is RPGTalkSaveNode)
                {
                    saves.Remove(connection.body as RPGTalkSaveNode);
                }
            }


            (NodeEditor.curNodeCanvas as RPGTalkNodeCanvas).GetNextLineInTxt();

        }

        public string GetText(int currentLine = -1)
        {

            if (currentLine != -1)
            {
                lineInTxt = currentLine;
            }

            string toReturn = "";
            if (characterID != 0)
            {
                toReturn += characters[characterID] + ":";
            }
            if (expressionID != 0 && expressions.Length > expressionID)
            {

                toReturn += "[expression=" + expressions[expressionID] + "]";
            }
            if (choices.Count > 0)
            {
                //remove empty spaces
                questionID = questionID.Replace(" ", string.Empty);
                toReturn += "[question=" + questionID + "]";
            }
            DialogLine = DialogLine.Replace("\n", "\\n");
            toReturn += DialogLine;


            foreach (ConnectionPort port in connectionPorts)
            {
                if (port.direction != Direction.Out)
                {
                    break;
                }

                if (port.connections.Count > 0)
                {
                    if (port.connections[0].body is RPGTalkNode)
                    {
                        RPGTalkNode node = (port.connections[0].body as RPGTalkNode);
                        if (node.attachedTo == this)
                        {
                            toReturn += "\n";
                            if (currentLine != -1)
                            {
                                currentLine++;
                            }
                            toReturn += node.GetText(currentLine);
                        }
                        else
                        {
                            if (node.startOfCutscene)
                            {
                                toReturn += "[newtalk start=" + node.CutsceneTitle + "_begin break=" + node.CutsceneTitle + "_end]";
                            }
                            else
                            {
                                RPGTalkNode initialNode = node;
                                while (true)
                                {
                                    if (initialNode.attachedTo == null)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        initialNode = initialNode.attachedTo;
                                    }

                                }
                                toReturn += "[newtalk start=" + node.lineInTxt + " break=" + initialNode.CutsceneTitle + "_end]";

                            }

                            toReturn += "\n";
                        }

                    }//is rpgtalknode
                    else if (port.connections[0].body is RPGTalkChoiceNode)
                    {
                        toReturn += "\n";
                        foreach (RPGTalkChoiceNode choice in choices)
                        {
                            toReturn += "[choice]" + choice.DialogLine;

                            if (choice.GetFollowUpTalkTile() != "")
                            {
                                int startchoice;
                                if (int.TryParse(choice.GetFollowUpTalkTile(), out startchoice))
                                {
                                    toReturn += "[newtalk start=" + startchoice.ToString() + " break=" + choice.GetFollowUpTalkBreak() + "_end]";
                                }
                                else
                                {
                                    toReturn += "[newtalk start=" + choice.GetFollowUpTalkTile() + "_begin break=" + choice.GetFollowUpTalkBreak() + "_end]";
                                }

                            }
                            toReturn += "\n";
                        }
                    }// is rpgtalkchoice
                    else
                    {
                        toReturn += "\n";
                        foreach (RPGTalkSaveNode save in saves)
                        {
                            toReturn += "[save ";

                            if (save.GetFollowUpTalkTile() != "")
                            {
                                int startchoice;
                                if (int.TryParse(save.GetFollowUpTalkTile(), out startchoice))
                                {
                                    toReturn += " start=" + startchoice.ToString() + " break=" + save.GetFollowUpTalkBreak() + "_end";
                                }
                                else
                                {
                                    toReturn += " start=" + save.GetFollowUpTalkTile() + "_begin break=" + save.GetFollowUpTalkBreak() + "_end";
                                }

                            }

                            toReturn += " data=" + save.savedData + " mod=" + save.modifier + "]";

                            toReturn += "\n";
                        }
                    }// is save
                }// connection coun > 0
                else
                {
                    //This is the final node
                    toReturn += "\n";
                }
            }//foreach node
            return toReturn;
        }



    }


    public class RPGTalkForwardType : ValueConnectionType // : IConnectionTypeDeclaration
    {
        public override string Identifier { get { return "RPGTalkForward"; } }
        public override Type Type { get { return typeof(float); } }
        public override Color Color { get { return Color.cyan; } }
    }

}