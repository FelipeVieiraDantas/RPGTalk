using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTALK.Localization
{
    [CreateAssetMenu(fileName = "NewLanguage", menuName = "RPGTalk/Language", order = 12)]
    public class RPGTalkLanguage : ScriptableObject
    {
        public string identifier;
        public bool mainLanguage;
        public TextAsset[] txts;
    }
}