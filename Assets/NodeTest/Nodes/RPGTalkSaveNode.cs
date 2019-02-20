using System;
using NodeEditorFramework;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;

namespace RPGTALK.Nodes
{

    /// <summary>
    /// Node to deal with something that could be different if some value is saved
    /// </summary>
    [Node(false, "Rpgtalk/Save Node")]
    public class RPGTalkSaveNode : Node
    {

        public override string Title
        {
            get
            {
                if (attachedTo == null)
                {
                    return "Save Node";
                }
                else
                {
                    return "SaveNode_" + attachedTo.saves.IndexOf(this).ToString();
                }
            }
        }

        public override bool AllowRecursion { get { return true; } }
        public Type GetObjectType { get { return typeof(RPGTalkChoiceNode); } }

        public override Vector2 MinSize { get { return new Vector2(200, 100); } }
        public override bool AutoLayout { get { return true; } }  //resizable renamed to autolayout?

        private const string Id = "rpgtalkSaveNode";
        public override string GetID { get { return Id; } }

        [FormerlySerializedAs("WhatTheCharacterSays")]
        public string savedData;
        public int modifier;

        public RPGTalkNode attachedTo;

        public virtual RPGTalkSaveNode PassAhead(int inputValue)
        {
            return this;
        }



        //Next Node to go to
        [ValueConnectionKnob("To Where", Direction.Out, "RPGTalkSaveForward", NodeSide.Right, MaxConnectionCount = ConnectionCount.Single)]
        public ValueConnectionKnob toWhereOUT;
        [ConnectionKnob("From Where", Direction.In, "RPGTalkSaveForward", NodeSide.Left, MaxConnectionCount = ConnectionCount.Single)]
        public ConnectionKnob fromWhereIN;

        private Vector2 scroll;

        protected override void OnCreate()
        {
            savedData = "IDOfQuestion";
        }

        public override void NodeGUI()
        {
            EditorGUILayout.HelpBox("Write below the ID of the saved statement. For questions answered, this is the id of the question.", MessageType.Info);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(50));
            EditorStyles.textField.wordWrap = true;
            savedData = EditorGUILayout.TextArea(savedData, GUILayout.ExpandHeight(true), GUILayout.Width(190));
            EditorStyles.textField.wordWrap = false;
            EditorGUILayout.EndScrollView();
            EditorGUILayout.HelpBox("Write below the value that the saved statement should have to continue this node. For questions answered, this is the id of the choice made", MessageType.Info);
            modifier = EditorGUILayout.IntField(modifier);

        }


        protected internal override void OnAddConnection(ConnectionPort port, ConnectionPort connection)
        {
            base.OnAddConnection(port, connection);
            if (!(connection.body is RPGTalkNode))
            {
                port.RemoveConnection(connection);
                return;
            }


            if (port.direction == Direction.In)
            {
                attachedTo = connection.body as RPGTalkNode;
            }

        }

        protected internal override void OnRemoveConnection(ConnectionPort port, ConnectionPort connection)
        {
            base.OnRemoveConnection(port, connection);
            if (port.direction == Direction.In)
            {
                attachedTo = null;

            }
        }

        //Return the title from the rpgtalknode after it
        public string GetFollowUpTalkTile()
        {
            foreach (ConnectionPort port in connectionPorts)
            {
                if (port.direction == Direction.Out && port.connections.Count > 0)
                {
                    RPGTalkNode node = (port.connections[0].body as RPGTalkNode);
                    if (node.attachedToChoice == null || node.attachedToChoice == this)
                    {
                        return node.CutsceneTitle;
                    }
                    else
                    {
                        return node.lineInTxt.ToString();
                    }
                }
            }

            return "";
        }

        //Return the linetobreak from the rpgtalknode after it
        public string GetFollowUpTalkBreak()
        {
            foreach (ConnectionPort port in connectionPorts)
            {
                if (port.direction == Direction.Out && port.connections.Count > 0)
                {
                    RPGTalkNode node = (port.connections[0].body as RPGTalkNode);
                    if (node.attachedToChoice == null || node.attachedToChoice == this)
                    {
                        return node.CutsceneTitle;
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
                        return initialNode.CutsceneTitle;
                    }
                }
            }

            return "";
        }

    }


    public class RPGTalkSaveForwardType : ValueConnectionType // : IConnectionTypeDeclaration
    {
        public override string Identifier { get { return "RPGTalkSaveForward"; } }
        public override Type Type { get { return typeof(float); } }
        public override Color Color { get { return Color.red; } }
    }

}