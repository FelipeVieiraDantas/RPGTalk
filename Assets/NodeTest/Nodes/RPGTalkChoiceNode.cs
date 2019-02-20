using System;
using NodeEditorFramework;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;


namespace RPGTALK.Nodes
{
    /// <summary>
    /// Node for when the current text should be an question
    /// </summary>
    [Node(false, "Rpgtalk/Choice Node")]
    public class RPGTalkChoiceNode : Node
    {

        public override string Title
        {
            get
            {
                if (attachedTo == null)
                {
                    return "Choice";
                }
                else
                {
                    return "Choice_" + attachedTo.choices.IndexOf(this).ToString();
                }
            }
        }

        public override bool AllowRecursion { get { return true; } }
        public Type GetObjectType { get { return typeof(RPGTalkChoiceNode); } }

        public override Vector2 MinSize { get { return new Vector2(150, 100); } }
        public override bool AutoLayout { get { return true; } }

        private const string Id = "rpgtalkChoiceNode";
        public override string GetID { get { return Id; } }

        [FormerlySerializedAs("WhatTheCharacterSays")]
        public string DialogLine;

        public RPGTalkNode attachedTo;

        public virtual RPGTalkChoiceNode PassAhead(int inputValue)
        {
            return this;
        }



        //Next Node to go to
        [ValueConnectionKnob("To Where", Direction.Out, "RPGTalkChoiceForward", NodeSide.Right, MaxConnectionCount = ConnectionCount.Single)]
        public ValueConnectionKnob toWhereOUT;
        [ConnectionKnob("From Where", Direction.In, "RPGTalkChoiceForward", NodeSide.Left, MaxConnectionCount = ConnectionCount.Single)]
        public ConnectionKnob fromWhereIN;

        private Vector2 scroll;

        protected override void OnCreate()
        {
            DialogLine = "This is the text of the answer!";
        }

        public override void NodeGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(100));
            EditorStyles.textField.wordWrap = true;
            DialogLine = EditorGUILayout.TextArea(DialogLine, GUILayout.ExpandHeight(true), GUILayout.Width(140));
            EditorStyles.textField.wordWrap = false;
            EditorGUILayout.EndScrollView();


        }


        protected internal override void OnAddConnection(ConnectionPort port, ConnectionPort connection)
        {
            base.OnAddConnection(port, connection);

            if(!(connection.body is RPGTalkNode)){
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


    public class RPGTalkChoiceForwardType : ValueConnectionType // : IConnectionTypeDeclaration
    {
        public override string Identifier { get { return "RPGTalkChoiceForward"; } }
        public override Type Type { get { return typeof(float); } }
        public override Color Color { get { return Color.yellow; } }
    }

}