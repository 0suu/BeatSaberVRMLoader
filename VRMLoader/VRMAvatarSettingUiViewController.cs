using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using HMUI;
using TMPro;
using System.IO;
using UnityEngine;
using IPA;
using static RootMotion.FinalIK.VRIKCalibrator;
using UnityEngine.UI;
using Unity.Mathematics;
using System.Windows.Forms;
using IPA.Config.Data;
using UnityEngine.UIElements;

namespace VRMLoader
{
    /// <summary>
    /// 右のパネル
    /// </summary>
    internal class VRMAvatarSettingUiViewController : BSMLResourceViewController
    {
        public override string ResourceName => "VRMLoader.BSML.vrmSettingView.bsml";

        public static float _scale = 1.62f;

        Camera mirrorCamera = null;

        RenderTexture renderTexture = null;

        GameObject quad = null;

        Vector3 cameraPosition = new Vector3(3.6f, 1.5f, 2.5f);

        VRMLoaderController _vRMLoaderController;
        VRMLoaderController vRMLoaderController
        {
            get
            {
                if (_vRMLoaderController == null)
                {
                    _vRMLoaderController = VRMLoaderController.Instance;
                }
                return _vRMLoaderController;
            }
        }

        [UIComponent("scaleValueText")]
        public TextMeshProUGUI ScaleText;

        void Start()
        {
            _scale = LoadAvatarSize();
            ScaleText.text = "Scale: " + _scale.ToString();
        }

        [UIAction("IncreaseScale1")]
        private void IncreaseScale1()
        {
            _scale += 0.01f;
            UpdateScale(_scale);
        }

        [UIAction("IncreaseScale2")]
        private void IncreaseScale2()
        {
            _scale += 0.1f;
            UpdateScale(_scale);
        }

        [UIAction("DecreaseScale1")]
        private void DecreaseScale1()
        {
            if (_scale >= 0.2f)
            {
                _scale -= 0.01f;
                UpdateScale(_scale);
            }
        }

        [UIAction("DecreaseScale2")]
        private void DecreaseScale2()
        {
            if (_scale >= 0.2f)
            {
                _scale -= 0.1f;
                UpdateScale(_scale);
            }
        }

        // Rotation X+
        [UIAction("RotXPlus")]
        void RotXPlus()
        {
            vRMLoaderController.AddHandRotation(new Vector3(10, 0, 0));
        }

        // Rotation X-
        [UIAction("RotXMinus")]
        void RotXMinus()
        {
            vRMLoaderController.AddHandRotation(new Vector3(-10, 0, 0));
        }

        // Rotation Y+
        [UIAction("RotYPlus")]
        void RotYPlus()
        {
            vRMLoaderController.AddHandRotation(new Vector3(0, 10, 0));
        }

        // Rotation Y-
        [UIAction("RotYMinus")]
        void RotYMinus()
        {
            vRMLoaderController.AddHandRotation(new Vector3(0, -10, 0));
        }

        // Rotation Z+
        [UIAction("RotZPlus")]
        void RotZPlus()
        {
            vRMLoaderController.AddHandRotation(new Vector3(0, 0, 10));
        }

        // Rotation Z-
        [UIAction("RotZMinus")]
        void RotZMinus()
        {
            vRMLoaderController.AddHandRotation(new Vector3(0, 0, -10));
        }

        // Position X+
        [UIAction("PosXPlus")]
        void PosXPlus()
        {
            vRMLoaderController.CurrentAvatarData.LeftHand.transform.Translate(0.01f, 0, 0);
            vRMLoaderController.AddHandPosition(new Vector3(0.01f, 0, 0));
        }

        // Position X-
        [UIAction("PosXMinus")]
        void PosXMinus()
        {
            vRMLoaderController.AddHandPosition(new Vector3(-0.01f, 0, 0));
        }

        // Position Y+
        [UIAction("PosYPlus")]
        void PosYPlus()
        {
            vRMLoaderController.AddHandPosition(new Vector3(0, 0.01f,0));
        }

        // Position Y-
        [UIAction("PosYMinus")]
        void PosYMinus()
        {
            vRMLoaderController.AddHandPosition(new Vector3(0, -0.01f, 0));
        }

        // Position Z+
        [UIAction("PosZPlus")]
        void PosZPlus()
        {
            vRMLoaderController.AddHandPosition(new Vector3(0, 0, 0.01f));
        }

        // Position Z-
        [UIAction("PosZMinus")]
        void PosZMinus()
        {
            vRMLoaderController.AddHandPosition(new Vector3(0, 0, -0.01f));
        }

        [UIAction("Reset")]
        void Reset()
        {
            vRMLoaderController.ResetAvatarOffsetAll();
        }

        [OnEnable]
        void OnEnable()
        {
            // 鏡用カメラを作る
            if (mirrorCamera == null)
            {
                mirrorCamera = new GameObject("VRMAvatar Mirror Camera").AddComponent<Camera>();
                mirrorCamera.transform.position = cameraPosition;
                mirrorCamera.transform.LookAt(new Vector3(0, 1.1f, 0));
                mirrorCamera.cullingMask = 1 << 3 | 1 << 12;
                mirrorCamera.orthographic = true;
                mirrorCamera.orthographicSize = 1.2f;

                // レンダリングテクスチャの設定
                renderTexture = new RenderTexture(512, 512, 24);

                mirrorCamera.targetTexture = renderTexture;
                
                // Quadを作る
                quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.position = cameraPosition;
                quad.transform.rotation = Quaternion.Euler(0, 60, 0);
                quad.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                Debug.Log(quad.layer + " quadlayer");
                var renderer = quad.GetComponent<Renderer>();

                //shader materialの設定
                {
                    renderer.material.shader = vRMLoaderController.forVrmShader;

                    renderer.material.SetColor("_AmbientLight", new Color(0.77f, 0.77f, 0.77f, 1f));
                    renderer.material.SetFloat("_CustomColorsAlbedo", 1f);
                    renderer.material.SetFloat("_Glossiness", 0f);
                    renderer.material.SetVector("_LightDir", new Vector4(0, 0, 0, 0));
                }

                renderer.material.mainTexture = renderTexture;
            }
            else
            {
                mirrorCamera.gameObject.SetActive(true);
                quad.SetActive(true);

                vRMLoaderController.CurrentAvatarData.VRIK.solver.spine.headTarget = Camera.main.transform;
            }
        }

        [OnDisable]
        void OnDisable()
        {
            mirrorCamera.gameObject.SetActive(false);
            quad.SetActive(false);
        }

        void UpdateScale(float value)
        {
            //アバターサイズ更新
            vRMLoaderController.CurrentAvatarData.AvatarGameObject.transform.localScale = new Vector3(value, value, value);

            // テキスト更新
            ScaleText.text = "Scale: " + value.ToString();

            mirrorCamera.Render();

            // 値の保存
            SaveAvatarSize(value);
        }

        void SaveAvatarSize(float size)
        {
            Properties.Settings.Default.AvatarSize = size;
            Properties.Settings.Default.Save();
        }

        float LoadAvatarSize()
        {
            return Properties.Settings.Default.AvatarSize;
        }
    }
}