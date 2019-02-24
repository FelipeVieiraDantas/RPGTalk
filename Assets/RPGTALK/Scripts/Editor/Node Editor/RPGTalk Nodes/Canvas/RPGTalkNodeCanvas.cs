using System.Collections.Generic;
using System.Linq;
using NodeEditorFramework;
using UnityEditor;
using RPGTALK.Helper;

namespace RPGTALK.Nodes
{

    [NodeCanvasType("RPGTalk Canvas")]
    public class RPGTalkNodeCanvas : NodeCanvas
    {

        public override string canvasName { get { return "RPGTalk"; } }
        public string Name = "RPGTalk";

        public RPGTalkCharacter[] characters;
        public string[] charactersNames;

        public Expression[] expressions;

        public string[] GetExpressionsNamesByCharacterName(string name)
        {
            List<Expression> returnExpressions = new List<Expression>();
            List<string> _expressions = new List<string>();
            _expressions.Add("None");
            foreach (RPGTalkCharacter character in characters)
            {
                if (name == character.dialoger)
                {
                    foreach (Expression exp in character.expressions)
                    {
                        returnExpressions.Add(exp);
                        _expressions.Add(exp.name);
                    }
                }
            }

            expressions = returnExpressions.ToArray();
            return _expressions.ToArray();
        }

        public RPGTalkNode[] GetStartCutsceneNodes()
        {
            List<RPGTalkNode> startNodes = new List<RPGTalkNode>();
            foreach (Node node in this.nodes)
            {
                if (node is RPGTalkNode && (node as RPGTalkNode).startOfCutscene)
                {
                    startNodes.Add(node as RPGTalkNode);
                }

            }

            return startNodes.ToArray();
        }

        public int GetCorrectCutsceneAutoTitle()
        {
            RPGTalkNode[] startNodes = GetStartCutsceneNodes();
            List<int> contains = new List<int>();
            for (int i = 0; i < startNodes.Length; i++)
            {
                if (startNodes[i].CutsceneTitle.IndexOf("Cutscene_") != -1 && startNodes[i].CutsceneTitle.IndexOf("FollowUp_") == -1)
                {
                    contains.Add(int.Parse(startNodes[i].CutsceneTitle.Substring(startNodes[i].CutsceneTitle.IndexOf("Cutscene_") + 9)));
                }
            }
            for (int i = 0; i < contains.Count; i++)
            {
                if (!contains.Contains(i))
                {
                    return i;
                }
            }
            return startNodes.Length;
        }

        public RPGTalkCharacter[] GetAllCharactersInGame()
        {
            if (characters != null && characters.Length > 0)
            {
                return characters;
            }

            string[] assetPath = AssetDatabase.FindAssets("t:RPGTalkCharacter");
            if (assetPath.Length == 0)
            {
                return null;
            }

            List<RPGTalkCharacter> list = new List<RPGTalkCharacter>();
            List<string> names = new List<string>();
            names.Add("None");
            foreach (string path in assetPath)
            {
                RPGTalkCharacter character = AssetDatabase.LoadAssetAtPath<RPGTalkCharacter>(AssetDatabase.GUIDToAssetPath(path));
                list.Add(character);
                names.Add(character.dialoger);
            }

            charactersNames = names.ToArray();
            characters = list.ToArray();
            return list.ToArray();

        }

        public string[] GetallCharactersNames()
        {
            if (characters == null || characters.Length > 0)
            {
                GetAllCharactersInGame();
            }

            return charactersNames;
        }

        public int GetNextLineInTxt()
        {
            string file = "";
            foreach (RPGTalkNode node in GetStartCutsceneNodes())
            {
                file += "[title=" + node.CutsceneTitle + "_begin]\n";
                int actualLine = file.Split('\n').Length;
                file += node.GetText(actualLine);
                file += "[title=" + node.CutsceneTitle + "_end]\n";
            }
            int numLines = file.Split('\n').Length;
            return numLines;
        }





    }
}