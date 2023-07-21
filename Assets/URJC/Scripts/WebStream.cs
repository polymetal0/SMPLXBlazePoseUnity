using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.IO;
using UnityEngine.UI;

public class WebStream : MonoBehaviour
{

    public RawImage frame;  

    public string sourceURL = "http://192.168.18.130:4747/video";
    public Text ipText, countdown;
    private Texture2D texture;
    private Stream stream;
    private string uri = "https://urjc.serveo.net";

    void Start()
    {
        //StartStream();
    }

    public void StartStream()
    {
        StartCoroutine(Stream());
    }

    public IEnumerator Stream()
    {
        texture = new Texture2D(2, 2);

        sourceURL = "http://" + ipText.text + ":4747/video";
        // create HTTP request
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(sourceURL);
        yield return req.GetResponse();

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
                { 
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