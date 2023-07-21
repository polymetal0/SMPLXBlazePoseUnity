using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CamMirror : MonoBehaviour
{
    public static CamMirror instance;

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
        instance = this;

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
        StartCoroutine(Test.instance.UploadImage(countdown, tex, false));
    }
}