using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

public class DataLoader : MonoBehaviour
{
    private Toggle toggle;
    // Start is called before the first frame update
    void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.group = GetComponentInParent<ToggleGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadData()//FileInfo file)
    {
        if (toggle.isOn)
        {
            Test.instance.LoadData(GetComponentInChildren<Text>().text);
        }
    }
}
