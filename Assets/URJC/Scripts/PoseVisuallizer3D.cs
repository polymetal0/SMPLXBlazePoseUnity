using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.BlazePose;
using System.Linq;

public class PoseVisuallizer3D : MonoBehaviour
{
    public static PoseVisuallizer3D instance;

    [SerializeField] Camera mainCamera;
    [SerializeField] WebCamInput webCamInput;
    [SerializeField] RawImage inputImageUI;
    [SerializeField] Shader shader;
    [SerializeField, Range(0, 1)] float humanExistThreshold = 0.5f;

    Material material;
    BlazePoseDetecter detecter;
    public BlazePoseModel model;
    public RawImage mirror;

    internal BlazePoseModel.JointPoint[] jointPoints;
    internal float rotation = 180f;
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
        instance = this;
        material = new Material(shader);
        detecter = new BlazePoseDetecter();
        jointPoints = model.Init();
    }

    void Update(){
        //mainCamera.transform.RotateAround(Vector3.zero, Vector3.up, 0.1f);
    }

    void LateUpdate(){
        //inputImageUI.texture = camMirror.webcamTexture;//.inputImageTexture;

        // Predict pose by neural network model.
        detecter.ProcessImage(mirror.texture);// webCamInput.inputImageTexture);

        //var cubo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //cubo.transform.position = detecter.GetPoseLandmark(0);
        //cubo.transform.localScale = Vector3.one * 0.1f;
        if (jointPoints != null)
        {
            // Output landmark values(33 values) and the score whether human pose is visible (1 values).
            for (int i = 0; i < detecter.vertexCount + 1; i++)
            {
                /*
                0~32 index datas are pose world landmark.
                Check below Mediapipe document about relation between index and landmark position.
                https://google.github.io/mediapipe/solutions/pose#pose-landmark-model-blazepose-ghum-3d
                Each data factors are
                x, y and z: Real-world 3D coordinates in meters with the origin at the center between hips.
                w: The score of whether the world landmark position is visible ([0, 1]).

                33 index data is the score whether human pose is visible ([0, 1]).
                This data is (score, 0, 0, 0).
                */
                // Debug.LogFormat("{0}: {1}", i, detecter.GetPoseWorldLandmark(i));
                //if (detecter.GetPoseWorldLandmark(i).w > humanExistThreshold)
                {
                    //jointPoints[i].Pos3D.x = -1f * detecter.GetPoseWorldLandmark(i).x * scaling.x;
                    jointPoints[i].Pos3D = detecter.GetPoseWorldLandmark(i);//.x;// * scaling.x;
                    //jointPoints[i].Pos3D.y = detecter.GetPoseWorldLandmark(i).y;// * scaling.y;
                    //jointPoints[i].Pos3D.z = -1f * detecter.GetPoseWorldLandmark(i).z * scaling.z;
                    //jointPoints[i].Pos3D.z = detecter.GetPoseWorldLandmark(i).z;// * scaling.z;
                    jointPoints[i].score3D = detecter.GetPoseWorldLandmark(i).w;

                    
                }
                //else
                {
                    //jointPoints[i].Transform = model.anim.GetBoneTransform((HumanBodyBones)jointPoints[i].boneIndex);
                }
            }

            // Calculate head position
            Vector3 earCenter = Vector3.Lerp(jointPoints[PositionIndex.rEar.Int()].Pos3D, jointPoints[PositionIndex.lEar.Int()].Pos3D, 0.5f);
            Vector3 eyeCenter = Vector3.Lerp(jointPoints[PositionIndex.rEye.Int()].Pos3D, jointPoints[PositionIndex.lEye.Int()].Pos3D, 0.5f);
            Vector3 earCenterEyeCenter = eyeCenter - earCenter;
            Vector3 leftEarRightEar = jointPoints[PositionIndex.rEar.Int()].Pos3D - jointPoints[PositionIndex.lEar.Int()].Pos3D;
            Vector3 earCenterHead = Vector3.Cross(leftEarRightEar, earCenterEyeCenter);
            Vector3 normalizedEarCenterHead = Vector3.Normalize(earCenterHead);
            earCenterHead = normalizedEarCenterHead * 0.1f;
            jointPoints[PositionIndex.head.Int()].Pos3D = earCenter + earCenterHead;
            // Calculate head score
            float[] headScores3D = { jointPoints[PositionIndex.rEar.Int()].score3D, jointPoints[PositionIndex.lEar.Int()].score3D,
                jointPoints[PositionIndex.rEye.Int()].score3D, jointPoints[PositionIndex.lEye.Int()].score3D };
            jointPoints[PositionIndex.head.Int()].score3D = headScores3D.Min();


            // Calculate neck position
            Vector3 shoulderCenter = Vector3.Lerp(jointPoints[PositionIndex.rShoulder.Int()].Pos3D, jointPoints[PositionIndex.lShoulder.Int()].Pos3D, 0.5f);
            jointPoints[PositionIndex.neck.Int()].Pos3D = Vector3.Lerp(shoulderCenter, jointPoints[PositionIndex.head.Int()].Pos3D, 0.3f);
            // Calculate neck score
            float[] neckScores3D = { jointPoints[PositionIndex.rShoulder.Int()].score3D, jointPoints[PositionIndex.lShoulder.Int()].score3D, jointPoints[PositionIndex.head.Int()].score3D };
            jointPoints[PositionIndex.neck.Int()].score3D = neckScores3D.Min();

            // Calculate hips position
            Vector3 hipCenter = Vector3.Lerp(jointPoints[PositionIndex.rHip.Int()].Pos3D, jointPoints[PositionIndex.lHip.Int()].Pos3D, 0.5f);
            jointPoints[PositionIndex.hips.Int()].Pos3D = Vector3.Lerp(hipCenter, shoulderCenter, 0.125f);
            // Calculate hips score
            float[] hipsScores3D = { jointPoints[PositionIndex.rShoulder.Int()].score3D, jointPoints[PositionIndex.lShoulder.Int()].score3D,
                jointPoints[PositionIndex.rHip.Int()].score3D, jointPoints[PositionIndex.lHip.Int()].score3D};
            jointPoints[PositionIndex.hips.Int()].score3D = hipsScores3D.Min();

            // Calculate spine position
            jointPoints[PositionIndex.spine.Int()].Pos3D = Vector3.Lerp(hipCenter, shoulderCenter, 0.28f);
            // Calculate spine score
            jointPoints[PositionIndex.spine.Int()].score3D = hipsScores3D.Min();

            // Calculate chest position
            jointPoints[PositionIndex.chest.Int()].Pos3D = Vector3.Lerp(hipCenter, shoulderCenter, 0.7f);
            // Calculate chest score
            jointPoints[PositionIndex.chest.Int()].score3D = hipsScores3D.Min();

            //SMPLX joint offset
            jointPoints[PositionIndex.rKnee.Int()].Pos3D.x -= 0.06f;
            jointPoints[PositionIndex.lKnee.Int()].Pos3D.x += 0.06f;
            
            jointPoints[PositionIndex.rAnkle.Int()].Transform.Rotate(0, 0, 45);
            jointPoints[PositionIndex.lAnkle.Int()].Transform.Rotate(0, 0, -45);

            jointPoints[PositionIndex.hips.Int()].Transform.Rotate(-15, rotation, 0);
            //jointPoints[PositionIndex.neck.Int()].Transform.Rotate(25, 0, 0);

            jointPoints[PositionIndex.head.Int()].Pos3D.x -= 0.02f;
            jointPoints[PositionIndex.head.Int()].Pos3D.y += 0.05f;
            jointPoints[PositionIndex.head.Int()].Pos3D.z -= 0.05f;
        }
    } 

    void OnRenderObject(){
        /*// Use predicted pose world landmark results on the ComputeBuffer (GPU) memory.
        material.SetBuffer("_worldVertices", detecter.outputBuffer);
        // Set pose landmark counts.
        material.SetInt("_keypointCount", detecter.vertexCount);
        material.SetFloat("_humanExistThreshold", humanExistThreshold);
        material.SetVectorArray("_linePair", linePair);
        material.SetMatrix("_invViewMatrix", mainCamera.worldToCameraMatrix.inverse) ;
        material.SetPass(2);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, BODY_LINE_NUM);

        // Draw 33 world landmark points.
        material.SetPass(3);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, detecter.vertexCount);*/
    }

    void OnApplicationQuit(){
        // Must call Dispose method when no longer in use.
        detecter.Dispose();
    }
}
