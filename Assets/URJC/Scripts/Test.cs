using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Linq;
using SimpleJSON;
using System.IO;
using System;

public class Test : MonoBehaviour
{
    public static Test instance;
    // Start is called before the first frame update
    //public PoseVisuallizer3D pose;
    public GameObject origin;
    public GameObject origin2;
    private GameObject _origin;
    public GameObject mirror;
    public GameObject virtualMirror;
    private SMPLX sample;
    public SMPLX[] bodies;
    public Slider[] sliders;
    public Dropdown bodyType;
    public GameObject jsonItem;
    public Transform scrollView;
    private string uri = "https://urjc.serveo.net";//"http://127.0.0.1:5000";

    void Start()
    {
        instance = this;
        _origin = origin;
        //SpawnAvatar();
        //StartCoroutine(GetRequest(uri));
        //sample = FindObjectOfType<SMPLX>();
        SetPaths();
        ListSaved();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Show()
    {
        mirror.SetActive(false);
        virtualMirror.SetActive(true);
    }
    
    public void Hide()
    {
        mirror.SetActive(true);
        virtualMirror.SetActive(false);
    }

    public void SpawnAvatar()
    {
        if (sample != null)
        {
            Destroy(sample.gameObject);
            Debug.Log("avatar destroyed");
        }
        sample = Instantiate(bodies[bodyType.value], _origin.transform.position, Quaternion.identity, _origin.transform);
        sample.transform.Rotate(Vector3.up * 180);
        sample.SnapToGroundPlane();
        for (int i = 0; i < 10; i++)
        {
            sample.betas[i] = sliders[i].value;
        }
        sample.SetBetaShapes();
        //Debug.Log("avatar Spawned");

        PoseVisuallizer3D.instance.gameObject.SetActive(true);
        PoseVisuallizer3D.instance.model = sample.GetComponent<BlazePoseModel>();
        PoseVisuallizer3D.instance.jointPoints = PoseVisuallizer3D.instance.model.Init();
        //pose.gameObject.SetActive(true);
        //pose.model = sample.GetComponent<BlazePoseModel>();
        //pose.model.Init();
        Show();
    }

    private void SetToGround()
    {
        var BB = sample.transform.GetComponentInChildren<SkinnedMeshRenderer>().bounds.size.y;
        sample.transform.position += new Vector3(0, BB * 0.75f, 0);
    }

    public void ChangeOrigin() 
    {
        if (sample != null)
        {
            if (virtualMirror.activeSelf)
            {
                PoseVisuallizer3D.instance.rotation = 0f;
                _origin = origin2;
                SpawnAvatar();
                SetToGround();
                Hide();
            }
            else
            {
                PoseVisuallizer3D.instance.rotation = 180f;
                _origin = origin;
                SpawnAvatar();
                SetToGround();
                Show();
            }
        }     
    }

    public void SetShapes()
    {

        for (int i = 0; i < 10; i++)
        {
            sample.betas[i] = 0.0f;
            sliders[i].value = 0.0f;
        }

        sample.SetBetaShapes();
        //sample.SnapToGroundPlane();
    }
    public void SubmitSliderSetting(int i)
    {
        //Debug.Log(sliders[i].value);
        sample.betas[i] = sliders[i].value;
        sample.SetBetaShapes();
        //sample.SnapToGroundPlane();
    }

    public void Request()
    {
        StartCoroutine(GetRequest(uri));
    }

    internal IEnumerator GetRequest(string url)
    {
        SpawnAvatar();
        float[] betas = new float[sample.betas.Length];
        UnityWebRequest webReq = UnityWebRequest.Get(url);
        {
            yield return webReq.SendWebRequest();
            if (webReq.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error: " + webReq.error);
            }
            else
            {
                var text = webReq.downloadHandler.text;

                var json = JSON.Parse(text).Children;
                int index = 0;
                foreach (var j in json)
                {
                    sample.betas[index] = j.AsFloat;
                    sliders[index].value = j.AsFloat;
                    Debug.Log(sample.betas[index]);
                    index++;
                }
                //sample.gameObject.AddComponent<RASCALSkinnedMeshCollider>();
                SetToGround();
                SaveData(text);
            }
        }
    }

    private string path = "";
    private string persistentPath = "";

    private void SetPaths()
    {
        path = Application.dataPath + Path.AltDirectorySeparatorChar + "/JSON/SaveData" + DateTime.Now.TimeOfDay.TotalMilliseconds + ".json";
        persistentPath = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "SaveData.json";
    }

    public void SaveData(string data)
    {
        string savePath = path;

        Debug.Log("Saving Data at " + savePath);
        Debug.Log(data);

        using StreamWriter writer = new StreamWriter(savePath);
        writer.Write(data);

        ListSaved();
    }

    public void LoadData(string name)
    {
        SpawnAvatar();
        using StreamReader reader = new StreamReader(Application.dataPath + Path.AltDirectorySeparatorChar + "/JSON/" + name);
        string data = reader.ReadToEnd();

        var json = JSON.Parse(data).Children;
        int index = 0;
        foreach (var j in json)
        {
            sample.betas[index] = j.AsFloat;
            sliders[index].value = j.AsFloat;
            Debug.Log(sample.betas[index]);
            index++;
        }
        SetToGround();
    }

    public void ListSaved()
    {
        for (int i = 0; i < scrollView.childCount; i++)
        {
            Destroy(scrollView.GetChild(i).gameObject);
        }
        var info = new DirectoryInfo(Application.dataPath + Path.AltDirectorySeparatorChar + "/JSON");
        var files = info.GetFiles();
        foreach ( var file in files ) 
        {
            if (file.Extension == ".json")
            {
                var json = Instantiate(jsonItem, scrollView);
                json.SetActive(true);
                json.GetComponentInChildren<Text>().text = file.Name;
            }         
        }
    }

    public Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
    {
        Color32[] original = originalTexture.GetPixels32();
        Color32[] rotated = new Color32[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels32(rotated);
        rotatedTexture.Apply();
        return rotatedTexture;
    }

    public Texture2D FlipTexture(Texture2D originalTexture)
    {
        Color32[] original = originalTexture.GetPixels32();
        Color32[] flipped = new Color32[original.Length];

        int w = originalTexture.width;
        int h = originalTexture.height;

        int iFlipped, iOriginal;

        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                iOriginal = ((i + 1) * w - 1) - j;
                iFlipped = i * w + j;
                flipped[iFlipped] = original[iOriginal];
            }
        }

        Texture2D flippedTexture = new Texture2D(w, h);
        flippedTexture.SetPixels32(flipped);
        flippedTexture.Apply();

        return flippedTexture;
    }

    public IEnumerator UploadImage(Text countdown, Texture2D texture, bool clockwise)
    {
        int count = 3;

        countdown.gameObject.SetActive(true);

        while (count > 0)
        {
            countdown.text = count.ToString();
            count -= 1;
            yield return new WaitForSeconds(1);
        }

        countdown.text = "wait...";

        yield return new WaitForEndOfFrame();

        int width = texture.width;
        int height = texture.height;
        Texture2D tex = new Texture2D(width, height);

        if (!clockwise)
        {
            tex.SetPixels32(CamMirror.instance.webcamTexture.GetPixels32());
        }
        else
        {
            tex.SetPixels32(texture.GetPixels32());
        }

        tex.Apply();
        tex = RotateTexture(tex, clockwise);

        byte[] bytes = tex.EncodeToJPG();
        Destroy(tex);

        WWWForm form = new WWWForm();

        form.AddBinaryData("fileUpload", bytes);

        UnityWebRequest w = UnityWebRequest.Post(uri, form);
        yield return w.SendWebRequest();

        countdown.gameObject.SetActive(false);

        if (w.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(w.error);
        }
        else
        {
            Debug.Log("Finished Uploading Screenshot");
            StartCoroutine(GetRequest(uri));
        }
        countdown.gameObject.SetActive(false);
    }
}
