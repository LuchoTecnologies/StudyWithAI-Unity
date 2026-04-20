using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleListManager : MonoBehaviour
{
    public List<listItem> spawnedItems;
    public List<string> listContents;

    public bool testMode = false;

    [Header("List Config")]
    public Transform ViewportParent;
    public GameObject prefab;
    public GameObject firstPrefab; //if this is not null, the first list item will be like this
    public Vector3 startPos;
    public Vector3 offset;

    [System.Serializable]
    public class ListEvent : UnityEvent<int> { }
    public ListEvent onItemClick;

    void Start()
    {
        StartCoroutine(lateStart());
    }

    IEnumerator lateStart()
    {
        yield return new WaitForSeconds(0.1f);
        refreshSpawned();
    }

    void Update()
    {
        if (testMode)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                addToList("dummy");
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                resetList();
            }
        }

    }

    public void addToList(string s)
    {
        listContents.Add(s);
        refreshSpawned();
    }

    public void resetList()
    {
        foreach (var item in spawnedItems)
        {
            Destroy(item.gameObject);
        }

        spawnedItems = new();
        listContents = new();
    }

    public void refreshSpawned()
    {
        for (int i = spawnedItems.Count; i < listContents.Count; i++)
        {
            //spawn new item
            GameObject _pref = prefab;
            if (i == 0 && firstPrefab != null)
            {
                _pref = firstPrefab;
            }

            GameObject spawn = Instantiate(_pref, ViewportParent);
            spawn.transform.localPosition = startPos + offset * i;

            listItem item = spawn.GetComponent<listItem>();
            item.titleText = listContents[i];
            item.updateText();
            item.manager = this;
            item.listIndex = i;
            spawnedItems.Add(item);

            
        }
    }

    public void clickFromItem(int idx)
    {
        if(testMode)print("List pressed at index " + idx);
        onItemClick.Invoke(idx);
    }
}
