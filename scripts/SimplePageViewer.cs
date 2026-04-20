using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePageViewer : MonoBehaviour
{
    public GameObject[] pages;
    public int pageSelected = 0;
    // Start is called before the first frame update
    void Start()
    {
        openPage(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) && pageSelected > 0)
        {
            pageSelected--;
            openPage(pageSelected);
        }
        if(Input.GetKeyDown(KeyCode.RightArrow) && pageSelected < pages.Length - 1)
        {
            pageSelected++;
            openPage(pageSelected);
        }
    }

    public void openPage(int idx)
    {
        foreach (var item in pages)
        {
            item.SetActive(false);
        }
        pages[idx].SetActive(true);
    }
}
