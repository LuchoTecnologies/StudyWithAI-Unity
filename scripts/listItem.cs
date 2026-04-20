using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class listItem : MonoBehaviour
{
    public string titleText;
    public TMP_Text title;
    public SimpleListManager manager;
    [HideInInspector]
    public int listIndex = -1;
    // Start is called before the first frame update
    void Start()
    {
        updateText();
    }

    void OnValidate()
    {
        updateText();
    }

    public void updateText()
    {
        title.text = titleText;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onItemClick()
    {
        manager.clickFromItem(listIndex);
    }
}
