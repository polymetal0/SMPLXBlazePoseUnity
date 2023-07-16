using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.BlazePose;

public class PoseVisuallizer : MonoBehaviour
{
    [SerializeField] WebCamInput webCamInput;
    [SerializeField] RawImage inputImageUI;
    [SerializeField] Shader shader;
    [SerializeField, Range(0, 1)] float humanExistThreshold = 0.5f;

    Material material;
    BlazePoseDetecter detecter;
    GameObject landMarks;
    GameObject lines;

    // Lines count of body's topology.
    const int BODY_LINE_NUM = 35;
    // Pairs of vertex indices of the lines that make up body's topology.
    // Defined by the figure in https://google.github.io/mediapipe/solutions/pose.
    readonly List<Vector4> linePair = new List<Vector4>{
        new Vector4(0, 1), new Vector4(1, 2), new Vector4(2, 3), new Vector4(3, 7), new Vector4(0, 4), 
        new Vector4(4, 5), new Vector4(5, 6), new Vector4(6, 8), new Vector4(9, 10), new Vector4(11, 12), 
        new Vector4(11, 13), new Vector4(13, 15), new Vector4(15, 17), new Vector4(17, 19), new Vector4(19, 15), 
        new Vector4(15, 21), new Vector4(12, 14), new Vector4(14, 16), new Vector4(16, 18), new Vector4(18, 20), 
        new Vector4(20, 16), new Vector4(16, 22), new Vector4(11, 23), new Vector4(12, 24), new Vector4(23, 24), 
        new Vector4(23, 25), new Vector4(25, 27), new Vector4(27, 29), new Vector4(29, 31), new Vector4(31, 27), 
        new Vector4(24, 26), new Vector4(26, 28), new Vector4(28, 30), new Vector4(30, 32), new Vector4(32, 28)
    };


    void Start(){
        material = new Material(shader);
        detecter = new BlazePoseDetecter();
        landMarks = new GameObject();
        lines = new GameObject();
    }

    void LateUpdate(){
        //inputImageUI.texture = webCamInput.inputImageTexture;

        // Predict pose by neural network model.
        detecter.ProcessImage(inputImageUI.texture);// webCamInput.inputImageTexture);
        for (int i = 0; i < landMarks.transform.childCount; i++)
        {
            Destroy(landMarks.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < lines.transform.childCount; i++)
        {
            Destroy(lines.transform.GetChild(i).gameObject);
        }
        // Output landmark values(33 values) and the score whether human pose is visible (1 values).
        for (int i = 0; i < detecter.vertexCount + 1; i++){
            /*
            0~32 index datas are pose landmark.
            Check below Mediapipe document about relation between index and landmark position.
            https://google.github.io/mediapipe/solutions/pose#pose-landmark-model-blazepose-ghum-3d
            Each data factors are
            x: x cordinate value of pose landmark ([0, 1]).
            y: y cordinate value of pose landmark ([0, 1]).
            z: Landmark depth with the depth at the midpoint of hips being the origin.
               The smaller the value the closer the landmark is to the camera. ([0, 1]).
               This value is full body mode only.
               **The use of this value is not recommended beacuse in development.**
            w: The score of whether the landmark position is visible ([0, 1]).
        
            33 index data is the score whether human pose is visible ([0, 1]).
            This data is (score, 0, 0, 0).
            */
            Debug.LogFormat("{0}: {1}", i, detecter.GetPoseLandmark(i));
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.transform.parent = landMarks.transform;
            marker.transform.localScale = Vector3.one * 0.01f;
            marker.transform.position = detecter.GetPoseLandmark(i) + new Vector4(inputImageUI.transform.position.x, inputImageUI.transform.position.y, inputImageUI.transform.position.z);
        }
        AddSkeleton(landMarks, lines);

        Debug.Log("---");
    }

    private void AddSkeleton(GameObject lm, GameObject l)
    {
        foreach (var line in linePair)
        {
            var renderer = new GameObject().AddComponent<LineRenderer>();// lm.transform.GetChild((int)line[0]).gameObject.AddComponent<LineRenderer>();
            renderer.sortingOrder = 1;
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.transform.SetParent(l.transform);
            renderer.startWidth = 0.01f;
            renderer.endWidth = 0.01f;
            renderer.startColor = Color.green; 
            renderer.endColor = Color.red;
            renderer.SetPosition(0, lm.transform.GetChild((int)line[0]).position);
            renderer.SetPosition(1, lm.transform.GetChild((int)line[1]).position);
        }

    }

    void OnRenderObject(){
        var w = inputImageUI.rectTransform.rect.width;
        var h = inputImageUI.rectTransform.rect.height;
        var d = Camera.main.transform.position.z - inputImageUI.transform.position.z;
        Debug.Log(d);
        // Use predicted pose landmark results on the ComputeBuffer (GPU) memory.
        material.SetBuffer("_vertices", detecter.outputBuffer);
        // Set pose landmark counts.
        material.SetInt("_keypointCount", detecter.vertexCount);
        material.SetFloat("_humanExistThreshold", humanExistThreshold);
        material.SetVector("_uiScale", new Vector2(w, h));
        material.SetVectorArray("_linePair", linePair);

        // Draw 35 body topology lines.
        material.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, BODY_LINE_NUM);

        // Draw 33 landmark points.
        material.SetPass(1);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, detecter.vertexCount);
    }

    void OnApplicationQuit(){
        // Must call Dispose method when no longer in use.
        detecter.Dispose();
    }
}
