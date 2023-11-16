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

        [UIComponent("scaleValueText")]
        public TextMeshProUGUI ScaleText;

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
            VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.Rotate(10, 0, 0);
        }

        // Rotation X-
        [UIAction("RotXMinus")]
        void RotXMinus()
        {
            VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.Rotate(-10, 0, 0);
        }

        // Rotation Y+
        [UIAction("RotYPlus")]
        void RotYPlus()
        {
            VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.Rotate(0, 10, 0);
        }

        // Rotation Y-
        [UIAction("RotYMinus")]
        void RotYMinus()
        {
            VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.Rotate(0, -10, 0);
        }

        // Rotation Z+
        [UIAction("RotZPlus")]
        void RotZPlus()
        {
            VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.Rotate(0, 0, 10);
        }

        // Rotation Z-
        [UIAction("RotZMinus")]
        void RotZMinus()
        {
            VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.Rotate(0, 0, -10);
        }

        // Position X+
        [UIAction("PosXPlus")]
        void PosXPlus()
        {
            VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.Translate(0.01f, 0, 0);
            Debug.Log(VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.x + " " + VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.y + " " + VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.z);

        }

        // Position X-
        [UIAction("PosXMinus")]
        void PosXMinus()
        {
            VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.Translate(-0.01f, 0, 0);
            Debug.Log(VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.x + " " + VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.y + " " + VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.z);

        }

        // Position Y+
        [UIAction("PosYPlus")]
        void PosYPlus()
        {
            VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.Translate(0, 0.01f, 0);
            Debug.Log(VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.x + " " + VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.y + " "+ VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.z);
        }

        // Position Y-
        [UIAction("PosYMinus")]
        void PosYMinus()
        {
            VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.Translate(0, -0.01f, 0);
            Debug.Log(VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.x + " " + VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.y + " " + VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.z);

        }

        // Position Z+
        [UIAction("PosZPlus")]
        void PosZPlus()
        {
            VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.Translate(0, 0, 0.01f);
            Debug.Log(VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.x + " " + VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.y + " " + VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.z);

        }

        // Position Z-
        [UIAction("PosZMinus")]
        void PosZMinus()
        {
            VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.Translate(0, 0, -0.01f);
            Debug.Log(VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.x + " " + VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.y + " " + VRMLoaderController.Instance.CurrentAvatarData.LeftHand.transform.localPosition.z);

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
                //mirrorCamera.fieldOfView = 10;

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
                    renderer.material.shader = VRMLoaderController.Instance.forVrmShader;

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

                VRMLoaderController.Instance.CurrentAvatarData.VRIK.solver.spine.headTarget = Camera.main.transform;
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
            VRMLoaderController.Instance.CurrentAvatarData.AvatarGameObject.transform.localScale = new Vector3(value, value, value);

            // テキスト更新
            ScaleText.text = "Scale: " + value.ToString();

            mirrorCamera.Render();
        }
    }
}