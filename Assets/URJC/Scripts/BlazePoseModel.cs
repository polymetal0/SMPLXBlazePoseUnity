using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum PositionIndex : int
{
    Nose = 0,
    lEyeInner,
    lEye,
    lEyeOuter,
    rEyeInner,
    rEye,
    rEyeOuter,
    lEar,
    rEar,
    mouthL,
    mouthR,
    lShoulder,
    rShoulder,
    lElbow,
    rElbow,
    lWrist,
    rWrist,
    lPinky,
    rPinky,
    lIndex,
    rIndex,
    lThumb,
    rThumb,
    lHip,
    rHip,
    lKnee,
    rKnee,
    lAnkle,
    rAnkle,
    lHeel,
    rHeel,
    lFootIndex,
    rFootIndex,
    humanVisible,

    //Calculated coordinates
    head,
    neck,
    chest,
    spine,
    hips,

    Count,
    None,
}

public static partial class EnumExtend
{
    public static int Int(this PositionIndex i)
    {
        return (int)i;
    }
}

public class BlazePoseModel : MonoBehaviour
{
    public class JointPoint
    {
        public Vector3 Pos3D = new Vector3();
        public float score3D;

        // Bones
        public Transform Transform = null;
        public Quaternion InitRotation;
        public Quaternion Inverse;
        public Quaternion InverseRotation;

        public JointPoint Child = null;
        public JointPoint Parent = null;

        public int boneIndex = -1;
    }

    // Joint position and bone
    private JointPoint[] jointPoints;
    public JointPoint[] JointPoints { get { return jointPoints; } }
    private Vector3 initPosition; // Initial center position
    private Vector3 jointPositionOffset = Vector3.zero;

    // Avatar
    public GameObject ModelObject;
    public GameObject Nose;
    public Animator anim;

    public PoseVisuallizer3D poseVisualizer3D;

    /*private XRHandSubsystem _subsystem;
    private OpenXRDevice _hmd;
    private XRHand rHand;
    private XRHand lHand;*/


    void Start()
    {
        poseVisualizer3D = FindObjectOfType<PoseVisuallizer3D>();

        /*_subsystem =
            XRGeneralSettings.Instance?
            .Manager?
            .activeLoader?
            .GetLoadedSubsystem<XRHandSubsystem>();

        _hmd = InputSystem.GetDevice<OpenXRDevice>(CommonUsages.Position);
        lHand = _subsystem.leftHand;//InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rHand = _subsystem.rightHand;//InputDevices.GetDeviceAtXRNode(XRNode.RightHand);*/
    }

    void Update()
    {
        if (jointPoints != null)
            PoseUpdate();
    }

    public JointPoint[] Init()
    {
        jointPoints = new JointPoint[PositionIndex.Count.Int()];
        for (var i = 0; i < PositionIndex.Count.Int(); i++)
            jointPoints[i] = new JointPoint();

        anim = ModelObject.GetComponent<Animator>();

        //TRANSFORM
        // Right Arm
        jointPoints[PositionIndex.rShoulder.Int()].Transform = SMPLX.instance._transformFromName["right_shoulder"];
        //anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
        jointPoints[PositionIndex.rElbow.Int()].Transform = SMPLX.instance._transformFromName["right_elbow"];
        //anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
        jointPoints[PositionIndex.rWrist.Int()].Transform = SMPLX.instance._transformFromName["right_wrist"];
        //anim.GetBoneTransform(HumanBodyBones.RightHand);
        jointPoints[PositionIndex.rThumb.Int()].Transform = SMPLX.instance._transformFromName["right_thumb1"];
        //anim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        jointPoints[PositionIndex.rPinky.Int()].Transform = SMPLX.instance._transformFromName["right_pinky1"];
        //anim.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
        jointPoints[PositionIndex.rIndex.Int()].Transform = SMPLX.instance._transformFromName["right_index1"];
        //anim.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);

        // Left Arm
        jointPoints[PositionIndex.lShoulder.Int()].Transform = SMPLX.instance._transformFromName["left_shoulder"];
        //anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        jointPoints[PositionIndex.lElbow.Int()].Transform = SMPLX.instance._transformFromName["left_elbow"];
        //anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        jointPoints[PositionIndex.lWrist.Int()].Transform = SMPLX.instance._transformFromName["left_wrist"];
        //anim.GetBoneTransform(HumanBodyBones.LeftHand);
        jointPoints[PositionIndex.lThumb.Int()].Transform = SMPLX.instance._transformFromName["left_thumb1"];
        //anim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        jointPoints[PositionIndex.lPinky.Int()].Transform = SMPLX.instance._transformFromName["left_pinky1"];
        //anim.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
        jointPoints[PositionIndex.lIndex.Int()].Transform = SMPLX.instance._transformFromName["left_index1"];
        //anim.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);

        //jointPoints[PositionIndex.lEar.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.lEye.Int()].Transform = SMPLX.instance._transformFromName["left_eye_smplhf"];
        //anim.GetBoneTransform(HumanBodyBones.LeftEye);
        //jointPoints[PositionIndex.rEar.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.rEye.Int()].Transform = SMPLX.instance._transformFromName["right_eye_smplhf"];
        //anim.GetBoneTransform(HumanBodyBones.RightEye);
        jointPoints[PositionIndex.Nose.Int()].Transform = Nose.transform;

        // Right Leg
        jointPoints[PositionIndex.rHip.Int()].Transform = SMPLX.instance._transformFromName["right_hip"];
        //anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        jointPoints[PositionIndex.rKnee.Int()].Transform = SMPLX.instance._transformFromName["right_knee"];
        //anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        jointPoints[PositionIndex.rAnkle.Int()].Transform = SMPLX.instance._transformFromName["right_ankle"];
        //anim.GetBoneTransform(HumanBodyBones.RightFoot);
        jointPoints[PositionIndex.rFootIndex.Int()].Transform = SMPLX.instance._transformFromName["right_foot"];
        //anim.GetBoneTransform(HumanBodyBones.RightFoot);
        // Left Leg
        jointPoints[PositionIndex.lHip.Int()].Transform = SMPLX.instance._transformFromName["left_hip"];
        //anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        jointPoints[PositionIndex.lKnee.Int()].Transform = SMPLX.instance._transformFromName["left_knee"];
        //anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        jointPoints[PositionIndex.lAnkle.Int()].Transform = SMPLX.instance._transformFromName["left_ankle"];
        //anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        jointPoints[PositionIndex.lFootIndex.Int()].Transform = SMPLX.instance._transformFromName["left_foot"];
        //anim.GetBoneTransform(HumanBodyBones.LeftFoot);

        // Spine
        jointPoints[PositionIndex.head.Int()].Transform = SMPLX.instance._transformFromName["head"];
        //anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.neck.Int()].Transform = SMPLX.instance._transformFromName["neck"];
        //anim.GetBoneTransform(HumanBodyBones.Neck);
        jointPoints[PositionIndex.chest.Int()].Transform = SMPLX.instance._transformFromName["spine2"];
        //anim.GetBoneTransform(HumanBodyBones.Chest);
        jointPoints[PositionIndex.spine.Int()].Transform = SMPLX.instance._transformFromName["spine1"];
        //anim.GetBoneTransform(HumanBodyBones.Spine);
        jointPoints[PositionIndex.hips.Int()].Transform = SMPLX.instance._transformFromName["pelvis"];
        //anim.GetBoneTransform(HumanBodyBones.Hips);

        // Parent-Child Setup
        // Right Arm

        jointPoints[PositionIndex.rShoulder.Int()].Child = jointPoints[PositionIndex.rElbow.Int()];
        jointPoints[PositionIndex.rElbow.Int()].Child = jointPoints[PositionIndex.rWrist.Int()];
        jointPoints[PositionIndex.rElbow.Int()].Parent = jointPoints[PositionIndex.rShoulder.Int()];
        // Left Arm
        jointPoints[PositionIndex.lShoulder.Int()].Child = jointPoints[PositionIndex.lElbow.Int()];
        jointPoints[PositionIndex.lElbow.Int()].Child = jointPoints[PositionIndex.lWrist.Int()];
        jointPoints[PositionIndex.lElbow.Int()].Parent = jointPoints[PositionIndex.lShoulder.Int()];
        // Right Leg
        jointPoints[PositionIndex.rHip.Int()].Child = jointPoints[PositionIndex.rKnee.Int()];
        jointPoints[PositionIndex.rKnee.Int()].Child = jointPoints[PositionIndex.rAnkle.Int()];
        jointPoints[PositionIndex.rAnkle.Int()].Child = jointPoints[PositionIndex.rFootIndex.Int()];
        jointPoints[PositionIndex.rAnkle.Int()].Parent = jointPoints[PositionIndex.rKnee.Int()];
        // Left Leg
        jointPoints[PositionIndex.lHip.Int()].Child = jointPoints[PositionIndex.lKnee.Int()];
        jointPoints[PositionIndex.lKnee.Int()].Child = jointPoints[PositionIndex.lAnkle.Int()];
        jointPoints[PositionIndex.lAnkle.Int()].Child = jointPoints[PositionIndex.lFootIndex.Int()];
        jointPoints[PositionIndex.lAnkle.Int()].Parent = jointPoints[PositionIndex.lKnee.Int()];
        // Spine
        jointPoints[PositionIndex.spine.Int()].Child = jointPoints[PositionIndex.chest.Int()];
        jointPoints[PositionIndex.chest.Int()].Child = jointPoints[PositionIndex.neck.Int()];
        jointPoints[PositionIndex.neck.Int()].Child = jointPoints[PositionIndex.head.Int()];

        // Set Inverse
        var forward = TriangleNormal(jointPoints[PositionIndex.hips.Int()].Transform.position, jointPoints[PositionIndex.lHip.Int()].Transform.position, jointPoints[PositionIndex.rHip.Int()].Transform.position);
        foreach (var jointPoint in jointPoints)
        {
            if (jointPoint != null)
            {
                if (jointPoint.Transform != null)
                {
                    jointPoint.InitRotation = jointPoint.Transform.rotation;
                }
                if (jointPoint.Child != null && jointPoint.Child.Transform != null && jointPoint.Child.Transform.position != null)
                {
                    jointPoint.Inverse = GetInverse(jointPoint, jointPoint.Child, forward);
                    jointPoint.InverseRotation = jointPoint.Inverse * jointPoint.InitRotation;
                }
            }
        }

        // Hips Rotation
        var hips = jointPoints[PositionIndex.hips.Int()];
        initPosition = jointPoints[PositionIndex.hips.Int()].Transform.position;
        hips.Inverse = Quaternion.Inverse(Quaternion.LookRotation(forward));
        hips.InverseRotation = hips.Inverse * hips.InitRotation;

        // Head Rotation
        var head = jointPoints[PositionIndex.head.Int()];
        head.InitRotation = jointPoints[PositionIndex.head.Int()].Transform.rotation;
       
        {
            var gaze = jointPoints[PositionIndex.Nose.Int()].Transform.position - jointPoints[PositionIndex.head.Int()].Transform.position;
            head.Inverse = Quaternion.Inverse(Quaternion.LookRotation(gaze));
            var f = TriangleNormal(jointPoints[PositionIndex.Nose.Int()].Pos3D, jointPoints[PositionIndex.rEye.Int()].Pos3D, jointPoints[PositionIndex.lEye.Int()].Pos3D);
            head.InverseRotation = head.Inverse * head.InitRotation;
            head.Transform.rotation = Quaternion.LookRotation(gaze, f) * head.InverseRotation;

        }

        {
            // Wrists rotation
            var lWrist = jointPoints[PositionIndex.lWrist.Int()];
            var lf = TriangleNormal(lWrist.Pos3D, jointPoints[PositionIndex.lPinky.Int()].Pos3D, jointPoints[PositionIndex.lThumb.Int()].Pos3D);
            lWrist.Transform.rotation = Quaternion.LookRotation(jointPoints[PositionIndex.lThumb.Int()].Pos3D - jointPoints[PositionIndex.lPinky.Int()].Pos3D, lf) * lWrist.InverseRotation;

            var rWrist = jointPoints[PositionIndex.rWrist.Int()];
            var rf = TriangleNormal(rWrist.Pos3D, jointPoints[PositionIndex.rThumb.Int()].Pos3D, jointPoints[PositionIndex.rPinky.Int()].Pos3D);
            rWrist.Transform.rotation = Quaternion.LookRotation(jointPoints[PositionIndex.rThumb.Int()].Pos3D - jointPoints[PositionIndex.rPinky.Int()].Pos3D, rf) * rWrist.InverseRotation;
        }


        return JointPoints;
    }
    public void PoseUpdate()
    {
        // movement and rotatation of the center
        var forward = TriangleNormal(jointPoints[PositionIndex.hips.Int()].Pos3D, jointPoints[PositionIndex.lHip.Int()].Pos3D, jointPoints[PositionIndex.rHip.Int()].Pos3D);

            jointPoints[PositionIndex.hips.Int()].Transform.position = jointPoints[PositionIndex.hips.Int()].Pos3D + initPosition - jointPositionOffset;

        jointPoints[PositionIndex.hips.Int()].Transform.rotation = Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hips.Int()].InverseRotation;

        // rotation of each of the bones
        foreach (var jointPoint in jointPoints)
        {
            if (jointPoint.Parent != null)
            {
                var fv = jointPoint.Parent.Pos3D - jointPoint.Pos3D;
                var finalRotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, fv) * jointPoint.InverseRotation;

                jointPoint.Transform.rotation = finalRotation;
            }
            else if (jointPoint.Child != null)
            {
                jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, forward) * jointPoint.InverseRotation;
            }
        }

        {
            // Head Rotation
            var gaze = jointPoints[PositionIndex.Nose.Int()].Pos3D - jointPoints[PositionIndex.head.Int()].Pos3D;
            var f = TriangleNormal(jointPoints[PositionIndex.Nose.Int()].Pos3D, jointPoints[PositionIndex.rEar.Int()].Pos3D, jointPoints[PositionIndex.lEar.Int()].Pos3D);
            var head = jointPoints[PositionIndex.head.Int()];
            head.Transform.rotation = Quaternion.LookRotation(gaze, f) * head.InverseRotation;

        }

        {
            // Wrist rotation
            var lWrist = jointPoints[PositionIndex.lWrist.Int()];
            var lf = TriangleNormal(lWrist.Pos3D, jointPoints[PositionIndex.lPinky.Int()].Pos3D, jointPoints[PositionIndex.lIndex.Int()].Pos3D);
            lWrist.Transform.rotation = Quaternion.LookRotation(jointPoints[PositionIndex.lThumb.Int()].Pos3D - jointPoints[PositionIndex.lPinky.Int()].Pos3D, lf) * lWrist.InverseRotation;

            var rWrist = jointPoints[PositionIndex.rWrist.Int()];
            var rf = TriangleNormal(rWrist.Pos3D, jointPoints[PositionIndex.rThumb.Int()].Pos3D, jointPoints[PositionIndex.rPinky.Int()].Pos3D);
            rWrist.Transform.rotation = Quaternion.LookRotation(jointPoints[PositionIndex.rThumb.Int()].Pos3D - jointPoints[PositionIndex.rPinky.Int()].Pos3D, rf) * rWrist.InverseRotation;
        }

    }
    Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 d1 = a - b;
        Vector3 d2 = a - c;

        Vector3 dd = Vector3.Cross(d1, d2);
        dd.Normalize();

        return dd;
    }

    private Quaternion GetInverse(JointPoint p1, JointPoint p2, Vector3 forward)
    {
        return Quaternion.Inverse(Quaternion.LookRotation(p1.Transform.position - p2.Transform.position, forward));
    }

    private Vector3 GetCenter(GameObject obj)
    {
        Vector3 sumVector = Vector3.zero;

        foreach (Transform child in obj.transform)
        {
            sumVector += child.position;
        }

        Vector3 groupCenter = sumVector / obj.transform.childCount;
        return sumVector;
    }

    float RoundAngle(float angle)
    {
        return angle > 180 ? angle - 360 : angle;
    }
    Vector3 RoundAngles(Vector3 angles)
    {
        if (angles.x > 180)
        {
            angles.x -= 360;
        }
        if (angles.y > 180)
        {
            angles.y -= 360;
        }
        if (angles.z > 180)
        {
            angles.z -= 360;
        }

        return angles;
    }
}
