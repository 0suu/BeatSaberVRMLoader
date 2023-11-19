using RootMotion.FinalIK;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UniGLTF;
using UnityEngine;
using VRM;

namespace VRMLoader
{
    public class AvatarData
    {
        public GameObject AvatarGameObject = null;
        public GameObject RightHand = null;
        public GameObject LeftHand = null;
        public VRIK VRIK = null;
        
        //コンストラクタ
        public AvatarData(GameObject avatarGameObject, GameObject rightHand, GameObject leftHand, VRIK vrik)
        {
            AvatarGameObject = avatarGameObject;
            RightHand = rightHand;
            LeftHand = leftHand;
            VRIK = vrik;
        }
    }
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
    public class VRMLoaderController : MonoBehaviour
    {
        public static VRMLoaderController Instance { get; private set; }

        public Shader forVrmShader = null;

        public AvatarData CurrentAvatarData = null;

        public Camera MainCamera = null;

        public event Action OnAvatarLoaded;

        readonly Quaternion leftRotDefOffset = Quaternion.Euler(-40, 60, 30);
        readonly Quaternion rightRotDefOffset = Quaternion.Euler(-40, -60, -30);
        readonly Vector3 leftPosDefOffset = new Vector3(-0.016f, 0.024f, -0.13f);
        readonly Vector3 rightPosDefOffset = new Vector3(0.016f, 0.024f, -0.13f);

        // These methods are automatically called by Unity, you should remove any you aren't using.
        #region Monobehaviour Messages
        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        private void Awake()
        {
            // For this particular MonoBehaviour, we only want one instance to exist at any time, so store a reference to it in a static property
            //   and destroy any that are created while one already exists.
            if (Instance != null)
            {
                Plugin.Log?.Warn($"Instance of {GetType().Name} already exists, destroying.");
                GameObject.DestroyImmediate(this);
                return;
            }
            GameObject.DontDestroyOnLoad(this); // Don't destroy this object on scene changes
            Instance = this;
            Plugin.Log?.Debug($"{name}: Awake()");
        }

        /// <summary>
        /// Only ever called once on the first frame the script is Enabled. Start is called after any other script's Awake() and before Update().
        /// </summary>
        private void Start()
        {
            SetEvent();
            StartCoroutine(Init());
        }

        /// <summary>
        /// イベント登録
        /// </summary>
        void SetEvent()
        {

        }

        IEnumerator Init()
        {
            yield return new WaitForSeconds(4);
            StartCoroutine(LoadAssetBundleAndVRM());
        }

        /// <summary>
        /// VRM読み込み
        /// </summary>
        /// <param name="path"></param>
        public void LoadVRM(string path)
        {
            // 前回のアバターを削除
            if (CurrentAvatarData?.AvatarGameObject)
            {
                Destroy(CurrentAvatarData.AvatarGameObject);
            }

            // 手のオブジェクトをアバターがない間に取得(名前被り)
            var hands = GetHandsObject();

            // VRMロード
            var i = new GlbFileParser(path).Parse();
            VRMData vrmData = new VRMData(i);

            var instance = Load(vrmData);

            instance.EnableUpdateWhenOffscreen();
            instance.ShowMeshes();

            CurrentAvatarData = new AvatarData(instance.gameObject, null, null, null);

            // メッシュを自分視点非表示にする
            {
                foreach (var m in instance.SkinnedMeshRenderers)
                {
                    m.gameObject.layer = 3;
                }

                foreach (var m in instance.MeshRenderers)
                {
                    m.gameObject.layer = 3;
                }
            }

            Debug.Log(Properties.Settings.Default.LeftHandRotation);

            //Avatarの初期サイズ設定
            var scale = VRMAvatarSettingUiViewController._scale;
            instance.gameObject.transform.localScale = new Vector3(scale, scale, scale);

            //VRIKセットアップ
            {
                VRIKSetUp(CurrentAvatarData);

                // 手の割り当て
                if (CurrentAvatarData?.LeftHand == null || CurrentAvatarData?.RightHand == null)
                {
                    SetHandTargetObjects(hands[0].transform, hands[1].transform);
                }
                else
                {
                    CurrentAvatarData.AvatarGameObject = instance.gameObject;

                    CurrentAvatarData.VRIK.solver.leftArm.target = CurrentAvatarData.LeftHand.transform;
                    CurrentAvatarData.VRIK.solver.rightArm.target = CurrentAvatarData.RightHand.transform;
                }
            }

            // 手をグーに
            SetHandGesture(CurrentAvatarData);

            // アバターロード完了イベント発火
            UiManager.ActiveMenuButton();

            //DontDestoryにする
            DontDestroyOnLoad(instance.gameObject);
        }

        /// <summary>
        /// 手の位置を追加
        /// </summary>
        /// <param name="pos"></param>
        public void AddHandPosition(Vector3 pos)
        {
            if (CurrentAvatarData.LeftHand == null || CurrentAvatarData.RightHand == null)
            {
                Debug.Log("SetHandRotation : Hand Null");
                return;
            }

            Transform LeftHand = CurrentAvatarData.LeftHand.transform;
            Transform RightHand = CurrentAvatarData.RightHand.transform;

            var leftPos = pos;
            var rightPos = new Vector3(pos.x * -1, pos.y, pos.z);

            LeftHand.localPosition += leftPos;
            RightHand.localPosition += rightPos;

            SaveHandOffset(leftPos: LeftHand.localPosition, rightPos: RightHand.localPosition);
        }

        /// <summary>
        /// 手の回転を追加
        /// </summary>
        /// <param name="rot"></param>
        public void AddHandRotation(Vector3 rot)
        {
            if (CurrentAvatarData.LeftHand == null || CurrentAvatarData.RightHand == null)
            {
                Debug.Log("SetHandRotation : Hand Null");
                return;
            }

            Quaternion leftQuat = Quaternion.Euler(rot);
            Quaternion rightQuat = Quaternion.Euler(rot.x, rot.y * -1, rot.z * -1);

            Transform LeftHand = CurrentAvatarData.LeftHand.transform;
            Transform RightHand = CurrentAvatarData.RightHand.transform;

            LeftHand.localRotation *= leftQuat;
            RightHand.localRotation *= rightQuat;

            SaveHandOffset(null, null, LeftHand.localEulerAngles, RightHand.localEulerAngles);
        }

        /// <summary>
        /// 手の位置を指定
        /// </summary>
        /// <param name="leftPos"></param>
        /// <param name="rightPos"></param>
        public void SetHandPosition(Vector3 leftPos, Vector3 rightPos)
        {
            if (CurrentAvatarData.LeftHand == null || CurrentAvatarData.RightHand == null)
            {
                Debug.Log("SetHandRotation : Hand Null");
                return;
            }

            Transform LeftHand = CurrentAvatarData.LeftHand.transform;
            Transform RightHand = CurrentAvatarData.RightHand.transform;

            LeftHand.localPosition = leftPos;
            RightHand.localPosition = rightPos;

            Debug.Log("SetHandPosition: Left: " + leftPos + "Right: " + rightPos);

            SaveHandOffset(leftPos:LeftHand.localPosition, rightPos:RightHand.localPosition);
        }

        /// <summary>
        /// 手の回転を指定
        /// </summary>
        /// <param name="leftQuat"></param>
        /// <param name="rightQuat"></param>
        public void SetHandRotation(Quaternion leftQuat, Quaternion rightQuat)
        {
            if (CurrentAvatarData.LeftHand == null || CurrentAvatarData.RightHand == null)
            {
                Debug.Log("SetHandRotation : Hand Null");
                return;
            }

            Transform LeftHand = CurrentAvatarData.LeftHand.transform;
            Transform RightHand = CurrentAvatarData.RightHand.transform;

            LeftHand.localRotation = leftQuat;
            RightHand.localRotation = rightQuat;

            SaveHandOffset(leftRot:LeftHand.localEulerAngles, rightRot:RightHand.localEulerAngles);
        }

        public void ResetAvatarOffsetAll()
        {
            Transform LeftHand = CurrentAvatarData.LeftHand.transform;
            Transform RightHand = CurrentAvatarData.RightHand.transform;

            //デフォルトのオフセットを指定
            {
                LeftHand.localPosition = leftPosDefOffset;
                RightHand.localPosition = rightPosDefOffset;

                LeftHand.localRotation = leftRotDefOffset;
                RightHand.localRotation = rightRotDefOffset;
            }

            SaveHandOffset(LeftHand.localPosition, RightHand.localPosition, LeftHand.localEulerAngles, RightHand.localEulerAngles);
        }

        void SaveHandOffset(Vector3? leftPos = null, Vector3? rightPos = null, Vector3? leftRot = null, Vector3? rightRot = null)
        {
            if (leftPos != null && rightPos != null)
            {
                var leftPosStr = leftPos.Value.x + "," + leftPos.Value.y + "," + leftPos.Value.z;
                var rightPosStr = rightPos.Value.x + "," + rightPos.Value.y + "," + rightPos.Value.z;

                Debug.Log("Save Position:" + leftPosStr + " " + rightPosStr);

                Properties.Settings.Default.LeftHandPosition = leftPosStr;
                Properties.Settings.Default.RightHandPosition = rightPosStr;
            }

            if (leftRot != null && rightRot != null)
            {
                Debug.Log("Save Rotation:" + leftRot.Value.ToString() + " " + rightRot.Value.ToString());

                Properties.Settings.Default.LeftHandRotation = leftRot.ToString();
                Properties.Settings.Default.RightHandRotation = rightRot.ToString();
            }

            Properties.Settings.Default.Save();
        }

        void LoadHandOffset()
        {
            var leftPos = Utility.StringToVector3(Properties.Settings.Default.LeftHandPosition);
            var rightPos = Utility.StringToVector3(Properties.Settings.Default.RightHandPosition);
            SetHandPosition(leftPos, rightPos);

            var leftRot = Utility.StringToQuaternion(Properties.Settings.Default.LeftHandRotation);
            var rightRot = Utility.StringToQuaternion(Properties.Settings.Default.RightHandRotation);
            SetHandRotation(leftRot, rightRot);
        }

        RuntimeGltfInstance Load(VRMData vrm)
        {
            // 使用後に Dispose で VRMImporterContext を破棄してください。
            using (var loader = new VRMImporterContext(vrm))
            {
                var instance = loader.Load();
                return instance;
            }
        }

        IEnumerator LoadAssetBundleAndVRM()
        {
            // アセットバンドルのダウンロードと読み込み
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream("VRMLoader.AssetBundleData.shaders"))
                {
                    AssetBundleCreateRequest bsuberBundleLoadRequest = AssetBundle.LoadFromStreamAsync(stream);
                    yield return bsuberBundleLoadRequest;

                    AssetBundle bsuberLoadedAssetBundle = bsuberBundleLoadRequest.assetBundle;

                    if (bsuberLoadedAssetBundle == null)
                    {
                        yield break;
                    }

                    // シェーダーロード
                    Shader[] shaders = bsuberLoadedAssetBundle.LoadAllAssets<Shader>();
                    forVrmShader = shaders.FirstOrDefault(s => s.name == "BeatSaber/MToon");

                    bsuberLoadedAssetBundle.Unload(false);
                }
            }

            // シェーダーが読み込めていたらVRMload
            if (forVrmShader != null)
            {
                var path = Directory.GetFiles("VRMAvatars", "*.vrm");

                if (path.Length != 0)
                {
                    LoadVRM(path[0]);
                }
                else
                {
                    Debug.Log("vrm not found");
                }
            }
            else
            {
                Debug.Log("sahder null;");
            }
        }

        void VRIKSetUp(AvatarData currentAvatarData)
        {
            var vrik = currentAvatarData.AvatarGameObject.AddComponent<VRIK>();
            CurrentAvatarData.VRIK = vrik;

            // 全体
            {
                vrik.solver.plantFeet = false;
                vrik.solver.locomotion.weight = 1F;
                vrik.solver.IKPositionWeight = 1f;
                vrik.solver.LOD = 1;
                vrik.solver.spine.positionWeight = 1f;
            }

            // 頭
            {
                vrik.solver.spine.headTarget = Camera.main.transform;
            }

            // 腰
            {
                vrik.solver.spine.pelvisPositionWeight = 0f;

                //vrik.solver.spine.pelvisTarget = pelvisTarget;
            }

            // 腕
            {
                vrik.solver.leftArm.positionWeight = 1f;
                vrik.solver.leftArm.rotationWeight = 1f;

                vrik.solver.rightArm.positionWeight = 1f;
                vrik.solver.rightArm.rotationWeight = 1f;
            }

            // 足
            {
                vrik.solver.leftLeg.positionWeight = 0f;
                vrik.solver.rightLeg.positionWeight = 0f;

                vrik.solver.locomotion.footDistance = 0.1f;
                vrik.solver.locomotion.stepThreshold = 0.3f;
            }

            Debug.Log("VRIK setting completed");
        }

        void SetHandGesture(AvatarData avatarData)
        {
            var animator = avatarData.AvatarGameObject.GetComponent<Animator>();

            if (animator != null)
            {
                animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal).localRotation = Quaternion.Euler(0, 0, 80);
                animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate).localRotation = Quaternion.Euler(0, 0, 60);
                animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal).localRotation = Quaternion.Euler(0, 0, 60);

                animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal).localRotation = Quaternion.Euler(0, 0, 80);
                animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate).localRotation = Quaternion.Euler(0, 0, 60);
                animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal).localRotation = Quaternion.Euler(0, 0, 60);

                animator.GetBoneTransform(HumanBodyBones.LeftRingProximal).localRotation = Quaternion.Euler(0, 0, 80);
                animator.GetBoneTransform(HumanBodyBones.LeftRingIntermediate).localRotation = Quaternion.Euler(0, 0, 60);
                animator.GetBoneTransform(HumanBodyBones.LeftRingDistal).localRotation = Quaternion.Euler(0, 0, 60);

                animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal).localRotation = Quaternion.Euler(0, 0, 80);
                animator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate).localRotation = Quaternion.Euler(0, 0, 60);
                animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal).localRotation = Quaternion.Euler(0, 0, 60);

                animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate).localRotation = Quaternion.Euler(62, -17, 37);
                animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal).localRotation = Quaternion.Euler(0, -90, 0);

                animator.GetBoneTransform(HumanBodyBones.RightIndexProximal).localRotation = Quaternion.Euler(0, 0, -80);
                animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate).localRotation = Quaternion.Euler(0, 0, -60);
                animator.GetBoneTransform(HumanBodyBones.RightIndexDistal).localRotation = Quaternion.Euler(0, 0, -60);

                animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal).localRotation = Quaternion.Euler(0, 0, -80);
                animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate).localRotation = Quaternion.Euler(0, 0, -60);
                animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal).localRotation = Quaternion.Euler(0, 0, -60);

                animator.GetBoneTransform(HumanBodyBones.RightRingProximal).localRotation = Quaternion.Euler(0, 0, -80);
                animator.GetBoneTransform(HumanBodyBones.RightRingIntermediate).localRotation = Quaternion.Euler(0, 0, -60);
                animator.GetBoneTransform(HumanBodyBones.RightRingDistal).localRotation = Quaternion.Euler(0, 0, -60);

                animator.GetBoneTransform(HumanBodyBones.RightLittleProximal).localRotation = Quaternion.Euler(0, 0, -80);
                animator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate).localRotation = Quaternion.Euler(0, 0, -60);
                animator.GetBoneTransform(HumanBodyBones.RightLittleDistal).localRotation = Quaternion.Euler(0, 0, -60);

                animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate).localRotation = Quaternion.Euler(62, 17, -37);
                animator.GetBoneTransform(HumanBodyBones.RightThumbDistal).localRotation = Quaternion.Euler(0, 90, 0);
            }
        }

        /// <summary>
        /// Called every frame if the script is enabled.
        /// </summary>
        void Update()
        {
            if (MainCamera != Camera.main)
            {
                //メインカメラを更新
                MainCamera = Camera.main;

                // VRIKが存在したら更新
                if (CurrentAvatarData != null && CurrentAvatarData.VRIK != null)
                {
                    CurrentAvatarData.VRIK.solver.spine.headTarget = MainCamera.transform;

                    var hands = GetHandsObject();
                    if (hands != null && CurrentAvatarData?.VRIK != null)
                    {
                        SetHandTargetObjects(hands[0].transform, hands[1].transform);
                    }
                }
            }
            /*
            if(CurrentAvatarData?.VRIK?.solver.rightArm.target == null || CurrentAvatarData?.VRIK?.solver.leftArm.target == null)
            {
                Debug.Log("sertch hand");
                // 手がなかったら
                var hands = GetHandsObject();
                if (hands != null && CurrentAvatarData?.VRIK != null)
                {
                    //SetHandTargetObjects(hands[0].transform, hands[1].transform);
                }
            }
            */
        }

        void SetHandTargetObjects(Transform rightHand, Transform leftHand)
        {
            // 右
            GameObject rightHandOffset = new GameObject("RightHandOffset");
            rightHandOffset.transform.parent = rightHand.transform;

            CurrentAvatarData.VRIK.solver.rightArm.target = rightHandOffset.transform;
            
            // 左
            GameObject leftHandOffset = new GameObject("LeftHandOffset");
            leftHandOffset.transform.parent = leftHand.transform;

            CurrentAvatarData.VRIK.solver.leftArm.target = leftHandOffset.transform;

            CurrentAvatarData.RightHand = rightHandOffset;
            CurrentAvatarData.LeftHand = leftHandOffset;

            LoadHandOffset();

            Debug.Log("set handtargetobje");
        }

        GameObject[] GetHandsObject()
        {
            GameObject[] gameObjects = FindObjectsOfType(typeof(GameObject)) as GameObject[];

            GameObject[] returnObjects = new GameObject[2];

            foreach (var gameObject in gameObjects)
            {
                if (gameObject.name == "RightHand" || gameObject.name == "ControllerRight")
                {
                    returnObjects[0] = gameObject;
                }
                if (gameObject.name.Contains("LeftHand") || gameObject.name == "ControllerLeft")
                {
                    returnObjects[1] = gameObject;
                }
            }

            if (returnObjects[0] == null && returnObjects[1] == null)
            {
                return null;
            }

            return returnObjects;
        }

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Plugin.Log?.Debug($"{name}: OnDestroy()");
            if (Instance == this)
                Instance = null; // This MonoBehaviour is being destroyed, so set the static instance property to null.

        }
        #endregion
    }
}
