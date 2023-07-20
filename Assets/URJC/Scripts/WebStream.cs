using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Networking;

public class WebStream : MonoBehaviour
{

    public RawImage frame;  //Mesh for displaying video

    public string sourceURL = "http://192.168.18.130:4747/video";
    public Text ipText, countdown;
    private Texture2D texture;
    private Stream stream;
    private string uri = "https://urjc.serveo.net";

    void Start()
    {
        StartStream();
    }

    public void StartStream()
    {
        StartCoroutine(Stream());
    }

    public IEnumerator Stream()
    {
        texture = new Texture2D(2, 2);

        //sourceURL = "http://" + ipText.text + ":4747/video";
        // create HTTP request
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(sourceURL);
        yield return req.GetResponse();

        //Optional (if authorization is Digest)
        //req.Credentials = new NetworkCredential("username", "password");
        // get response
        WebResponse resp = req.GetResponse();
        // get response stream
        stream = resp.GetResponseStream();

        StartCoroutine(GetFrame());
    }

    IEnumerator GetFrame()
    {
        Byte[] JpegData = new Byte[65536];

        while (true)
        {
            int bytesToRead = FindLength(stream);
            //print(bytesToRead);
            if (bytesToRead == -1)
            {
                print("End of stream");
                yield break;
            }

            int leftToRead = bytesToRead;

            while (leftToRead > 0)
            {
                leftToRead -= stream.Read(JpegData, bytesToRead - leftToRead, leftToRead);
                yield return null;
            }

            MemoryStream ms = new MemoryStream(JpegData, 0, bytesToRead, false, true);

            texture.LoadImage(ms.GetBuffer());
            frame.texture = Test.instance.RotateTexture(texture, true);
            frame.texture = Test.instance.FlipTexture((Texture2D)frame.texture);
            stream.ReadByte(); // CR after bytes
            stream.ReadByte(); // LF after bytes
        }
    }

    int FindLength(Stream stream)
    {
        int b;
        string line = "";
        int result = -1;
        bool atEOL = false;

        while ((b = stream.ReadByte()) != -1)
        {
            if (b == 10) continue; // ignore LF char
            if (b == 13)
            { // CR
                if (atEOL)
                {  // two blank lines means end of header
                    stream.ReadByte(); // eat last LF
                    return result;
                }
                if (line.StartsWith("Content-Length:"))
                {
                    result = Convert.ToInt32(line.Substring("Content-Length:".Length).Trim());
                }
                else
                {
                    line = "";
                }
                atEOL = true;
            }
            else
            {
                atEOL = false;
                line += (char)b;
            }
        }
        return -1;
    }
    public void SendImage()
    {
        StartCoroutine(Test.instance.UploadImage(countdown, texture, true));
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
        int width = texture.width;
        int height = texture.height;
        Texture2D tex = new Texture2D(width, height);
        tex.SetPixels32(texture.GetPixels32());
        tex.Apply();
        tex = Test.instance.RotateTexture(tex, true);
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
        //("http://127.0.0.1:5000", form);
        //w.SetRequestHeader("content-type", "image/jpeg");
        yield return w.SendWebRequest();

        countdown.gameObject.SetActive(false);

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

    public void AddText(string s)
    {
        if (s == "")
        {
            ipText.text = ipText.text.Substring(0, ipText.text.Length - 1);
        }
        else
        {
            ipText.text += s;
        }
    }
}