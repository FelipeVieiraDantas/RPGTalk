using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTALK.Helper
{

    [System.Serializable]
    public class Expression
    {
        public string name;
        public Sprite photo;
        public string boolInAnimator;
        public AudioClip audio;
    }

    [CreateAssetMenu(fileName = "New Character", menuName = "RPGTalk/Character", order = 12)]
    public class RPGTalkCharacter : ScriptableObject
    {
        public string dialoger;
        public Sprite photo;

        public Expression[] expressions;
    }
}