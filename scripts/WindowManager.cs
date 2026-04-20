using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Proyect
{
    public string name;
    public float proggress;
}

public class WindowManager : MonoBehaviour
{
    [Header("Intro")]
    public GameObject introPanel;
    public Image introCover;
    Color introCoverColor;
    bool onIntro = true;
    float introStart = 0;

    [Header("Main Page")]
    public GameObject mainPageWindow;
    public GameObject mainPageCreateProyectPanel;

    public TMP_InputField mainPageNewInput;

    public List<Proyect> proyectsList;

    public SimpleListManager proyectListUI;

    [Header("Proyect Page")]
    public GameObject proyectPageWindow;

    public RankIndicator proyectRank;

    public TMP_Text[] proyectNameIndicators;

    public ProyectManager proyectManager;

    [Header("Apuntes Page")]
    public GameObject apuntesPageWindow;

    [Header("History Page")]
    public GameObject historyPageWindow;

    // Start is called before the first frame update
    
    void Start()
    {
        //moveToWindow(0);
        //mainPageCreateProyectPanel.SetActive(false);
        introPanel.SetActive(true);

        introStart = Time.time;
        introCoverColor = introCover.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (onIntro)
        {
            float t = Time.time - introStart;
            if(t < 1)
            {
                introCoverColor.a = 1 - t;
                introCover.color= introCoverColor;
            }
            if(t > 4)
            {
                introCoverColor.a = t - 4;
                introCover.color= introCoverColor;
            }
            if(t >= 5.5)
            {
                onIntro = false;
                introPanel.SetActive(false);
                moveToWindow(0);
                mainPageCreateProyectPanel.SetActive(false);
            }
        }
    }

    public void moveToWindow(int idx)
    {
        GameObject[] pages = { mainPageWindow, proyectPageWindow, apuntesPageWindow, historyPageWindow};
        foreach (var item in pages)
        {
            item.SetActive(false);
        }
        if(idx != -1)pages[idx].SetActive(true);

        if(idx == 0)
        {
            mainPageUpdateList();
        }

        if(idx == 1)
        {
            proyectPageStartup();
        }
    }

    public void mainPageCreateButton()
    {
        mainPageCreateProyectPanel.SetActive(!mainPageCreateProyectPanel.activeInHierarchy);
    }
    public void mainPageConfirmNewProyect()
    {
        string pname = mainPageNewInput.text;
        Proyect p = new();
        p.name = pname;
        p.proggress = float.NaN;
        proyectsList.Insert(0, p);

        mainPageUpdateList();

        mainPageNewInput.text = "";
        mainPageCreateButton(); //hide panel

    }
    public void mainPageSelectProyect(int idx)
    {
        Proyect p = proyectsList[idx];
        proyectsList.Remove(p);
        proyectsList.Insert(0, p);
        mainPageUpdateList();
    }
    public void mainPageUpdateList()
    {
        proyectListUI.resetList();
        //proyectListUI.listContents = proyectsList;
        foreach (var item in proyectsList)
        {
            proyectListUI.listContents.Add(item.name);
        }
        proyectListUI.refreshSpawned();

        int i = 0;
        foreach (var item in proyectListUI.spawnedItems)
        {
            RankIndicator rank = item.GetComponentInChildren<RankIndicator>(); //real men of genius (yeah this is very, well hella inefficient but naaah there wont be lots of proyects i guess)    
            rank.Progress = proyectsList[i].proggress;
            i++;
        }

    }

    public void proyectPageStartup()
    {
        foreach (var item in proyectNameIndicators)
        {
            item.text = proyectsList[0].name;
        }
        
        proyectRank.Progress = proyectsList[0].proggress;
    }

    public void proyectKeepLearning()
    {
        //dar control al proyect manager
        proyectManager.proyect = proyectsList[0];
        proyectManager.nextQuiz();
        moveToWindow(-1);
    }



}



