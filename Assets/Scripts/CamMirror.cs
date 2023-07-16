using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using System.Net;
using static TreeEditor.TreeEditorHelper;
using UnityEditor.PackageManager.UI;
using UnityEngine.UI;
using SimpleJSON;

public class CamMirror : MonoBehaviour
{
    public Material mirror;
    public RawImage espej;
    public TextMeshProUGUI debug;

    public int camNr = 0;
    public Text countdown;
    internal WebCamTexture webcamTexture;
    private WebCamDevice[] cams;
    private Texture2D texture;
    private string uri = "https://urjc.serveo.net";


    void Start()
    {
        cams = WebCamTexture.devices;
        webcamTexture = new WebCamTexture();
        //mat = mirror;
        //espej.texture = RotateTexture( webcamTexture, true);
        //mat.mainTexture = webcamTexture;
        webcamTexture.deviceName = cams[camNr].name;
        webcamTexture.Play();
        texture = new Texture2D(webcamTexture.width, webcamTexture.height);

        if (cams.Length <= 0)
        {
            debug.text = "No cams";
        }

        for (int i = 0; i < cams.Length; i++)
        {
            debug.text += cams[i].name + "\n";
        }

    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            SendImage();
            //TakeSnapshot();
        }

        texture.SetPixels32(webcamTexture.GetPixels32());
        espej.texture = Test.instance.RotateTexture(texture, false);
        espej.texture = Test.instance.FlipTexture((Texture2D)espej.texture);
        texture.Apply();
    }

    public void NextDevice()
    {
        if (camNr < cams.Length - 1)
        {
            camNr++;
        }

        else
        {
            camNr = 0;
        }

        webcamTexture.Stop();


        webcamTexture.deviceName = cams[camNr].name;
        espej.texture = webcamTexture;
        webcamTexture.Play();

    }

    public void SendImage()
    {
        Texture2D tex = new Texture2D(webcamTexture.width, webcamTexture.height);
        tex.SetPixels32(webcamTexture.GetPixels32());
        StartCoroutine(Test.instance.UploadImage(countdown, tex));
    }
    /*
    IEnumerator UploadImage()
    {
        int count = 3;

        countdown.gameObject.SetActive(true);

        while (count > 0)
        {
            countdown.text = count.ToString();
            count -= 1;
            //Debug.Log(countdown.text);
            yield return new WaitForSeconds(1);
        }

        countdown.text = "wait...";
        // We should only read the screen buffer after rendering is complete
        yield return new WaitForEndOfFrame();

        // Create a texture the size of the screen, RGB24 format
        int width = webcamTexture.width;
        int height = webcamTexture.height;
        Texture2D tex = new Texture2D(width, height);
        tex.SetPixels32(webcamTexture.GetPixels32());
        tex.Apply();
        tex = Test.instance.RotateTexture(tex, false);
        //tex = FlipTexture(tex);
        // Read screen contents into the texture
        //tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        //tex.Apply();

        // Encode texture into JPG
        byte[] bytes = tex.EncodeToJPG();
        Destroy(tex);
        //string path = _SavePath + "img_" + _CaptureCounter.ToString("00") + ".jpg";   
        //File.WriteAllBytes(path, bytes);
        //Debug.Log("CAPTURA   " + _CaptureCounter);

        // Create a Web Form
        WWWForm form = new WWWForm();

        form.AddBinaryData("fileUpload", bytes);

        UnityWebRequest w = UnityWebRequest.Post(uri, form);
        yield return w.SendWebRequest();

        if (w.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(w.error);
        }
        else
        {
            Debug.Log("Finished Uploading Screenshot");
            StartCoroutine(Test.instance.GetRequest(uri));
        }
        countdown.gameObject.SetActive(false);
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
        }*/
}