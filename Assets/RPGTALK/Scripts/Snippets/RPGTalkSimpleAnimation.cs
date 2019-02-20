using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RPGTALK.Texts;

public class RPGTalkSimpleAnimation : MonoBehaviour
{

    public int textSpeed = 30;
    public UnityEvent OnAnimationEnd;
    public bool startOnAwake = true;

    TMP_Translator text;
    string originalText;
    bool animating;
    float currentChar;

    // Start is called before the first frame update
    void Start()
    {
        text = new TMP_Translator(gameObject);
        if (startOnAwake)
        {
            StartAnimating();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!animating)
        {
            return;
        }

        // if there's still text left to show
        if (currentChar < originalText.Length)
        {

            //ensure that we don't accidentally blow past the end of the string
            currentChar = Mathf.Min(currentChar + textSpeed * Time.deltaTime,
                originalText.Length);

            //Do what we have to do if the the text just ended
            if (currentChar >= originalText.Length)
            {
                OnAnimationEnd.Invoke();
                return;
            }

            //Get the current char and the text and put it into the U
            text.ChangeTextTo(originalText.Substring(0, (int)currentChar));


        }
    }

    public void StartAnimating()
    {
        originalText = text.GetCurrentText();
        animating = true;
    }
}
