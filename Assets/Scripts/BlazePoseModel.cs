using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using static BlazePoseModel;

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
    lController,
    rController,
    lPhantomElbow,
    rPhantomElbow,
    centerHead,
    phantomNose,

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
    internal Animator anim;
    //private Vector3 avatarDimensions;
    //private Vector3 avatarCenter;

    public PoseVisuallizer3D poseVisualizer3D;

    // Start is called before the first frame update
    void Start()
    {
        poseVisualizer3D = FindObjectOfType<PoseVisuallizer3D>();

        string[] boneName = HumanTrait.BoneName;
        for (int i = 0; i < HumanTrait.BoneCount; ++i)
        {
            //Debug.Log(i + " : " + boneName[i]);
            //Debug.Log(new Vector3(HumanTrait.MuscleFromBone(i, 0),
                 //HumanTrait.MuscleFromBone(i, 1),
                 //HumanTrait.MuscleFromBone(i, 2)));
            //Debug.Log(new Vector3(HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(i, 0)),
                 //HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(i, 1)),
                 //HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(i, 2))));
        }
    }

    // Update is called once per frame
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

        //avatarDimensions.x = Vector3.Distance(anim.GetBoneTransform(HumanBodyBones.RightHand).position, anim.GetBoneTransform(HumanBodyBones.LeftHand).position);
        //avatarDimensions.y = Nose.transform.position.y;
        //avatarCenter = GetCenter(gameObject);

        //BONE INDEXING
        // Right Arm
        jointPoints[PositionIndex.rShoulder.Int()].boneIndex = (int)HumanBodyBones.RightUpperArm;
        jointPoints[PositionIndex.rElbow.Int()].boneIndex = (int)HumanBodyBones.RightLowerArm;
        jointPoints[PositionIndex.rWrist.Int()].boneIndex = (int)HumanBodyBones.RightHand;
        jointPoints[PositionIndex.rThumb.Int()].boneIndex = (int)HumanBodyBones.RightThumbIntermediate;
        jointPoints[PositionIndex.rPinky.Int()].boneIndex = (int)HumanBodyBones.RightLittleIntermediate;
        jointPoints[PositionIndex.rIndex.Int()].boneIndex = (int)HumanBodyBones.RightIndexIntermediate;

        // Left Arm
        jointPoints[PositionIndex.lShoulder.Int()].boneIndex = (int)HumanBodyBones.LeftUpperArm;
        jointPoints[PositionIndex.lElbow.Int()].boneIndex = (int)HumanBodyBones.LeftLowerArm;
        jointPoints[PositionIndex.lWrist.Int()].boneIndex = (int)HumanBodyBones.LeftHand;
        jointPoints[PositionIndex.lThumb.Int()].boneIndex = (int)HumanBodyBones.LeftThumbIntermediate;
        jointPoints[PositionIndex.lPinky.Int()].boneIndex = (int)HumanBodyBones.LeftLittleIntermediate;
        jointPoints[PositionIndex.lIndex.Int()].boneIndex = (int)HumanBodyBones.LeftIndexIntermediate;

        jointPoints[PositionIndex.lEar.Int()].boneIndex = (int)HumanBodyBones.Head;
        jointPoints[PositionIndex.lEye.Int()].boneIndex = (int)HumanBodyBones.LeftEye;
        jointPoints[PositionIndex.rEar.Int()].boneIndex = (int)HumanBodyBones.Head;
        jointPoints[PositionIndex.rEye.Int()].boneIndex = (int)HumanBodyBones.RightEye;
        //jointPoints[PositionIndex.Nose.Int()].boneIndex = Nose.transform;

        // Right Leg
        jointPoints[PositionIndex.rHip.Int()].boneIndex = (int)HumanBodyBones.RightUpperLeg;
        jointPoints[PositionIndex.rKnee.Int()].boneIndex = (int)HumanBodyBones.RightLowerLeg;
        jointPoints[PositionIndex.rAnkle.Int()].boneIndex = (int)HumanBodyBones.RightFoot;
        jointPoints[PositionIndex.rFootIndex.Int()].boneIndex = (int)HumanBodyBones.RightToes;
        // Left Leg
        jointPoints[PositionIndex.lHip.Int()].boneIndex = (int)HumanBodyBones.LeftUpperLeg;
        jointPoints[PositionIndex.lKnee.Int()].boneIndex = (int)HumanBodyBones.LeftLowerLeg;
        jointPoints[PositionIndex.lAnkle.Int()].boneIndex = (int)HumanBodyBones.LeftFoot;
        jointPoints[PositionIndex.lFootIndex.Int()].boneIndex = (int)HumanBodyBones.LeftToes;

        // Spine
        jointPoints[PositionIndex.head.Int()].boneIndex = (int)HumanBodyBones.Head;
        jointPoints[PositionIndex.neck.Int()].boneIndex = (int)HumanBodyBones.Neck;
        jointPoints[PositionIndex.chest.Int()].boneIndex = (int)HumanBodyBones.Chest;
        jointPoints[PositionIndex.spine.Int()].boneIndex = (int)HumanBodyBones.Spine;
        jointPoints[PositionIndex.hips.Int()].boneIndex = (int)HumanBodyBones.Hips;

        //TRANSFORM
        // Right Arm
        jointPoints[PositionIndex.rShoulder.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);

        jointPoints[PositionIndex.rElbow.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
        jointPoints[PositionIndex.rWrist.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightHand);
        jointPoints[PositionIndex.rThumb.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
        jointPoints[PositionIndex.rPinky.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
        jointPoints[PositionIndex.rIndex.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);

        // Left Arm
        jointPoints[PositionIndex.lShoulder.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        //Debug.LogWarning("COSA:   " + jointPoints[PositionIndex.lShoulder.Int()].Transform.rotation.eulerAngles);
        jointPoints[PositionIndex.lElbow.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        //Debug.LogWarning("COSA:   " + jointPoints[PositionIndex.lElbow.Int()].Transform.rotation.eulerAngles);
        jointPoints[PositionIndex.lWrist.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftHand);
        jointPoints[PositionIndex.lThumb.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
        jointPoints[PositionIndex.lPinky.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
        jointPoints[PositionIndex.lIndex.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);

        jointPoints[PositionIndex.lEar.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.lEye.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftEye);
        jointPoints[PositionIndex.rEar.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.rEye.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightEye);
        jointPoints[PositionIndex.Nose.Int()].Transform = Nose.transform;

        // Right Leg
        jointPoints[PositionIndex.rHip.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightUpperLeg);

        var puta = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg).position;
        //anim.GetBoneTransform(HumanBodyBones.RightLowerLeg).position = new Vector3(0, puta.y, puta.z);

        jointPoints[PositionIndex.rKnee.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        jointPoints[PositionIndex.rAnkle.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        jointPoints[PositionIndex.rFootIndex.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        // Left Leg
        jointPoints[PositionIndex.lHip.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        jointPoints[PositionIndex.lKnee.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        jointPoints[PositionIndex.lAnkle.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        jointPoints[PositionIndex.lFootIndex.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.LeftFoot);

        // Spine
        jointPoints[PositionIndex.head.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Head);
        jointPoints[PositionIndex.neck.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Neck);
        jointPoints[PositionIndex.chest.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Chest);
        jointPoints[PositionIndex.spine.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Spine);
        jointPoints[PositionIndex.hips.Int()].Transform = anim.GetBoneTransform(HumanBodyBones.Hips);

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
            head.InverseRotation = head.Inverse * head.InitRotation;
        }

        {
            // Wrists rotation
            var lWrist = jointPoints[PositionIndex.lWrist.Int()];
            var lf = TriangleNormal(lWrist.Pos3D, jointPoints[PositionIndex.lPinky.Int()].Pos3D, jointPoints[PositionIndex.lThumb.Int()].Pos3D);
            lWrist.InitRotation = lWrist.Transform.rotation;
            lWrist.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoints[PositionIndex.lThumb.Int()].Transform.position - jointPoints[PositionIndex.lPinky.Int()].Transform.position, lf));
            lWrist.InverseRotation = lWrist.Inverse * lWrist.InitRotation;

            var rWrist = jointPoints[PositionIndex.rWrist.Int()];
            var rf = TriangleNormal(rWrist.Pos3D, jointPoints[PositionIndex.rThumb.Int()].Pos3D, jointPoints[PositionIndex.rPinky.Int()].Pos3D);
            rWrist.InitRotation = jointPoints[PositionIndex.rWrist.Int()].Transform.rotation;
            rWrist.Inverse = Quaternion.Inverse(Quaternion.LookRotation(jointPoints[PositionIndex.rThumb.Int()].Transform.position - jointPoints[PositionIndex.rPinky.Int()].Transform.position, rf));
            rWrist.InverseRotation = rWrist.Inverse * rWrist.InitRotation;
        }


        return JointPoints;
    }
    public void PoseUpdate()
    {
        // movement and rotatation of the center
        var forward = TriangleNormal(jointPoints[PositionIndex.hips.Int()].Pos3D, jointPoints[PositionIndex.lHip.Int()].Pos3D, jointPoints[PositionIndex.rHip.Int()].Pos3D);

            jointPoints[PositionIndex.hips.Int()].Transform.position = jointPoints[PositionIndex.hips.Int()].Pos3D + initPosition - jointPositionOffset;

        var rot = (Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hips.Int()].InverseRotation).eulerAngles;
        //Debug.Log("ANTES   " + rot);

        var max = new Vector3(HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone((int)HumanBodyBones.Spine, 0)),
            HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone((int)HumanBodyBones.Spine, 1)),
            HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone((int)HumanBodyBones.Spine, 2)));//anim.avatar.humanDescription.human[PositionIndex.lKnee.Int()].limit.max;
        var min = new Vector3(HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone((int)HumanBodyBones.Spine, 0)),
            HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone((int)HumanBodyBones.Spine, 1)),
            HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone((int)HumanBodyBones.Spine, 2)));//anim.avatar.humanDescription.human[PositionIndex.rKnee.Int()].limit.max;

        rot.x = Mathf.Clamp(rot.x, jointPoints[PositionIndex.hips.Int()].InitRotation.eulerAngles.x + min.x, jointPoints[PositionIndex.hips.Int()].InitRotation.eulerAngles.x + max.x);
        rot.y = Mathf.Clamp(rot.y, jointPoints[PositionIndex.hips.Int()].InitRotation.eulerAngles.y + min.y, jointPoints[PositionIndex.hips.Int()].InitRotation.eulerAngles.y + max.y);
        rot.z = Mathf.Clamp(rot.z, jointPoints[PositionIndex.hips.Int()].InitRotation.eulerAngles.z + min.z, jointPoints[PositionIndex.hips.Int()].InitRotation.eulerAngles.z + max.z);

        var finalRot = (Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hips.Int()].InverseRotation);
        finalRot.eulerAngles = rot;

        for (int i = 0; i < 3; i++)
        {
            switch (min[i])
            {
                case -1.0f:
                    min[i] = 0.0f;
                    break;
                default:
                    break;
            }
            switch (max[i])
            {
                case -1.0f:
                    max[i] = 0.0f;
                    break;
                default:
                    break;
            }
        }
        jointPoints[PositionIndex.hips.Int()].Transform.rotation = Quaternion.LookRotation(forward) * jointPoints[PositionIndex.hips.Int()].InverseRotation;

        //Debug.Log("DESPUES:   " + rot);
        //Debug.Log("MIN:   " + min);
        //Debug.Log("MAX:   " + max);

        // rotation of each of the bones
        foreach (var jointPoint in jointPoints)
        {
            if (jointPoint.Parent != null)
            {
                var fv = jointPoint.Parent.Pos3D - jointPoint.Pos3D;
                var detectedRotation = (Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, fv) * jointPoint.InverseRotation).eulerAngles;

                detectedRotation = RoundAngles(detectedRotation);

                var minRotation = new Vector3(HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(jointPoint.boneIndex, 0)),
                    HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(jointPoint.boneIndex, 1)), 
                    HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(jointPoint.boneIndex, 2)));
                var maxRotation = new Vector3(HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(jointPoint.boneIndex, 0)), 
                    HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(jointPoint.boneIndex, 1)), 
                    HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(jointPoint.boneIndex, 2)));

                detectedRotation.x = Mathf.Clamp(detectedRotation.x, RoundAngle(jointPoint.InitRotation.eulerAngles.x) + minRotation.x, RoundAngle(jointPoint.InitRotation.eulerAngles.x) + maxRotation.x);
                detectedRotation.y = Mathf.Clamp(detectedRotation.y, RoundAngle(jointPoint.InitRotation.eulerAngles.y) + minRotation.y, RoundAngle(jointPoint.InitRotation.eulerAngles.y) + maxRotation.y);
                detectedRotation.z = Mathf.Clamp(detectedRotation.z, RoundAngle(jointPoint.InitRotation.eulerAngles.z) + minRotation.z, RoundAngle(jointPoint.InitRotation.eulerAngles.z) + maxRotation.z);

                var finalRotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, fv) * jointPoint.InverseRotation;
                //finalRotation.eulerAngles = detectedRotation;

                jointPoint.Transform.rotation = finalRotation;
                //Debug.Log("Final 0 - 360   " + detectedRotation);
                //Debug.Log("-180 - 180   " + new Vector3(RoundAngle(detectedRotation.x), RoundAngle(detectedRotation.y), RoundAngle(detectedRotation.z)));
            }
            else if (jointPoint.Child != null)
            {
                var detectedRotation = (Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, forward) * jointPoint.InverseRotation).eulerAngles;

                detectedRotation = RoundAngles(detectedRotation);

                var minRotation = new Vector3(HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(jointPoint.boneIndex, 0)), 
                    HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(jointPoint.boneIndex, 1)), 
                    HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone(jointPoint.boneIndex, 2)));
                var maxRotation = new Vector3(HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(jointPoint.boneIndex, 0)), 
                    HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(jointPoint.boneIndex, 1)), 
                    HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone(jointPoint.boneIndex, 2)));//anim.avatar.humanDescription.human[PositionIndex.rKnee.Int()].limit.max;

                detectedRotation.x = Mathf.Clamp(detectedRotation.x, RoundAngle(jointPoint.InitRotation.eulerAngles.x) + minRotation.x, RoundAngle(jointPoint.InitRotation.eulerAngles.x) + maxRotation.x);
                detectedRotation.y = Mathf.Clamp(detectedRotation.y, RoundAngle(jointPoint.InitRotation.eulerAngles.y) + minRotation.y, RoundAngle(jointPoint.InitRotation.eulerAngles.y) + maxRotation.y);
                detectedRotation.z = Mathf.Clamp(detectedRotation.z, RoundAngle(jointPoint.InitRotation.eulerAngles.z) + minRotation.z, RoundAngle(jointPoint.InitRotation.eulerAngles.z) + maxRotation.z);

                var finalRotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, forward) * jointPoint.InverseRotation;
                finalRotation.eulerAngles = detectedRotation;

                jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, forward) * jointPoint.InverseRotation;
                //Debug.Log(detectedRotation);
            }
        }

        {
            // Head Rotation
            var gaze = jointPoints[PositionIndex.Nose.Int()].Pos3D - jointPoints[PositionIndex.head.Int()].Pos3D;
            var f = TriangleNormal(jointPoints[PositionIndex.Nose.Int()].Pos3D, jointPoints[PositionIndex.rEar.Int()].Pos3D, jointPoints[PositionIndex.lEar.Int()].Pos3D);
            var head = jointPoints[PositionIndex.head.Int()]; 

            /*var rot = (Quaternion.LookRotation(gaze, f) * head.InverseRotation).eulerAngles;
            var max = new Vector3(HumanTrait.MuscleFromBone((int)HumanBodyBones.Head, 0), 
                HumanTrait.MuscleFromBone((int)HumanBodyBones.Head, 1), 
                HumanTrait.MuscleFromBone((int)HumanBodyBones.Head, 2));//anim.avatar.humanDescription.human[PositionIndex.lKnee.Int()].limit.max;
            var min = new Vector3(HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone((int)HumanBodyBones.Head, 0)), 
                HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone((int)HumanBodyBones.Head, 1)), 
                HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone((int)HumanBodyBones.Head, 2)));//anim.avatar.humanDescription.human[PositionIndex.rKnee.Int()].limit.max;

            rot.x = Mathf.Clamp(rot.x, min.x, max.x);
            rot.y = Mathf.Clamp(rot.y, min.y, max.y);
            rot.z = Mathf.Clamp(rot.z, min.z, max.z);

            var finalRotation = new Quaternion();
            finalRotation.eulerAngles = rot;*/

            head.Transform.rotation = Quaternion.LookRotation(gaze, f) * head.InverseRotation;

            //Debug.Log(min + "\n" + max);

            // movement and rotatation of the center
            //var fv = jointPoints[PositionIndex.rElbow.Int()].Parent.Pos3D - jointPoints[PositionIndex.rElbow.Int()].Pos3D;
            var detectedRotation = (Quaternion.LookRotation(jointPoints[PositionIndex.rShoulder.Int()].Pos3D - jointPoints[PositionIndex.rShoulder.Int()].Child.Pos3D, forward) * jointPoints[PositionIndex.rShoulder.Int()].InverseRotation).eulerAngles;
            //jointPoints[PositionIndex.rShoulder.Int()].Transform.position = jointPoints[PositionIndex.rShoulder.Int()].Pos3D + initPosition - jointPositionOffset;

            //Debug.Log("ANTES   " + rot);

            var maxPuta = new Vector3(HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone((int)HumanBodyBones.RightUpperArm, 0)),
                HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone((int)HumanBodyBones.RightUpperArm, 1)),
                HumanTrait.GetMuscleDefaultMax(HumanTrait.MuscleFromBone((int)HumanBodyBones.RightUpperArm, 2)));//anim.avatar.humanDescription.human[PositionIndex.lKnee.Int()].limit.max;
            var minPuta = new Vector3(HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone((int)HumanBodyBones.RightUpperArm, 0)),
                HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone((int)HumanBodyBones.RightUpperArm, 1)),
                HumanTrait.GetMuscleDefaultMin(HumanTrait.MuscleFromBone((int)HumanBodyBones.RightUpperArm, 2)));//anim.avatar.humanDescription.human[PositionIndex.rKnee.Int()].limit.max;

            detectedRotation = RoundAngles(detectedRotation);

            detectedRotation.x = Mathf.Clamp(detectedRotation.x, RoundAngle(jointPoints[PositionIndex.rShoulder.Int()].InitRotation.eulerAngles.x) + minPuta.x, RoundAngle(jointPoints[PositionIndex.rShoulder.Int()].InitRotation.eulerAngles.x) + maxPuta.x);
            detectedRotation.y = Mathf.Clamp(detectedRotation.y, RoundAngle(jointPoints[PositionIndex.rShoulder.Int()].InitRotation.eulerAngles.y) + minPuta.y, RoundAngle(jointPoints[PositionIndex.rShoulder.Int()].InitRotation.eulerAngles.y) + maxPuta.y);
            detectedRotation.z = Mathf.Clamp(detectedRotation.z, RoundAngle(jointPoints[PositionIndex.rShoulder.Int()].InitRotation.eulerAngles.z) + minPuta.z, RoundAngle(jointPoints[PositionIndex.rShoulder.Int()].InitRotation.eulerAngles.z) + maxPuta.z);

            var finalRotPuta = Quaternion.LookRotation(jointPoints[PositionIndex.rShoulder.Int()].Pos3D - jointPoints[PositionIndex.rShoulder.Int()].Child.Pos3D, forward) * jointPoints[PositionIndex.rShoulder.Int()].InverseRotation;

            finalRotPuta.eulerAngles = detectedRotation;// rotPuta;
         
            //jointPoints[PositionIndex.rShoulder.Int()].Transform.rotation = finalRotPuta;// Quaternion.LookRotation(forward) * jointPoints[PositionIndex.rShoulder.Int()].InverseRotation;
            //Debug.LogWarning(finalRotPuta.eulerAngles);
            //Debug.LogWarning("-180 - 180   " + new Vector3(RoundAngle(detectedRotation.x), RoundAngle(detectedRotation.y), RoundAngle(detectedRotation.z)));
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
        return angle > 180 ? angle - 360 : angle;// < -180 ? angle + 360 : angle;
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

        return angles;//angle > 180 ? angle - 360 : angle;// < -180 ? angle + 360 : angle;
    }
}
