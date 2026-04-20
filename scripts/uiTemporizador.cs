using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class uiTemporizador : MonoBehaviour
{
    public string currentText = "";
    public TMP_Text displayText;
    float tempStart;
    float tempLen;

    public AudioSource alarm;

    bool tempRunning = false;
    // Start is called before the first frame update
    void Awake()
    {
        resetTemp();
    }

    void OnEnable()
    {
        resetTemp();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (tempRunning)
        {
            float temptime = tempLen - (Time.time - tempStart);
            //print(temptime);
            if (temptime > 0)
            {
                TimeSpan t = TimeSpan.FromSeconds(temptime);
                updateText(t.ToString(@"mm\:ss"));
            }
            else
            {
                //temp over
                alarm.Play();
                tempRunning = false;
            }

        }
    }

    void resetTemp()
    {
        updateText("");
        tempRunning = false;
    }

    public void StartTemp(int t)
    {
        tempRunning = true;
        tempLen = t;
        tempStart = Time.time;
    }

    public void updateText(string s)
    {
        if (s != currentText)
        {
            displayText.text = s;
            currentText = s;
        }
    }
}
