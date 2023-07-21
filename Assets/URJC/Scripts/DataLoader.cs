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

    public void LoadData()
    {
        if (toggle.isOn)
        {
            Test.instance.LoadData(GetComponentInChildren<Text>().text);
        }
    }
}
