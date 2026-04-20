using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankIndicator : MonoBehaviour
{
    public Sprite[] ranks;
    public Sprite noRank;

    public Sprite finalRank;
    public string finalRankName = "Élite";
    public Image sRenderer;
    public Image progressCircle;
    [Header("Text")]
    public string[] rankNames;
    public TMP_Text text;

    [Range(0,100)]
    [SerializeField]
    private float _progress = 0;

    public float Progress
    {
        get => _progress;
        set
        {
            if (_progress == value) return; // Evita ejecuciones innecesarias si el valor es el mismo
            _progress = value;
            UpdateIcon(); // El método que quieres ejecutar
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnValidate()
    {
        UpdateIcon();
    }

    public void UpdateIcon()
    {
        if (float.IsNaN(_progress) || float.IsInfinity(_progress))
        {
            sRenderer.sprite = noRank;
            progressCircle.fillAmount = 0;
            if(text)text.text = "Sin Rango";

        }
        else
        {
            //print(_progress);
            float p = _progress / 100f;

            if(p == 1)
            {
                sRenderer.sprite = finalRank;
                if(text)text.text = finalRankName;
                progressCircle.fillAmount = 0;
                return;
            }

            int iconIdx = Mathf.Min((int)(p * ranks.Length), ranks.Length - 1); ;
            sRenderer.sprite = ranks[iconIdx];

            progressCircle.fillAmount = (p % (1f / (float)ranks.Length)) * (float)ranks.Length;
            if(text)text.text = rankNames[iconIdx];
        }

    }
}
