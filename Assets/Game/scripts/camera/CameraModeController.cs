﻿using UnityEngine;
using System;
using Raider.Game.GUI;

namespace Raider.Game.Cameras
{

    public class CameraModeController : MonoBehaviour
    {
        #region Singleton Setup

        public static CameraModeController singleton;
        public static CameraController controllerInstance
        {
            get { return singleton.GetComponent<CameraController>(); }
        }

        public void Awake()
        {
            if (singleton != null)
                Debug.LogAssertion("It seems that multiple Camera Mode Controllers are active, breaking the singleton instance.");
            singleton = this;
        }

        public void OnDestroy()
        {
            singleton = null;
        }

        #endregion

        public GameObject camPoint;

        //how close the camera can be to directly overhead or underfoot.
        public float xAxisBuffer = 27f;

        public FirstPersonCameraSettings firstPersonCamSettings;
        public ThirdPersonCameraSettings thirdPersonCamSettings;

        public GameObject sceneOverviewGameObject;
        public GameObject cameraPathGameObject;

        public enum CameraModes
        {
            FirstPerson = 0,
            ThirdPerson = 1,
            Shoulder = 2,
            FlyCam = 3,
            Static = 4,
            Follow = 5,
            SceneOverview = 6,
            FreeCam = 7,
            FollowPath = 8
        }

        public CameraModes selectedCameraMode = CameraModes.ThirdPerson;
        private CameraModes activeCamera;

        [System.Serializable]
        public class FirstPersonCameraSettings
        {
            public float lookSensitivity = 3f;
            public bool inverted = false;
            public bool moveWithBody = true;
        }

        [System.Serializable]
        public class ThirdPersonCameraSettings
        {
            public LayerMask transparent;
            public float lookSensetivity = 3f;
            public float minDistance = 5f;
            public float maxDistance = 15f;
            public float distanceMoveSpeed = 3f;
            public float cameraPaddingPercent = 0.3f;
            public bool inverted = false;
        }

        Vector3 cameraStartingPoint;

        // Use this for initialization
        void Start()
        {
            if (cameraPathGameObject == null || sceneOverviewGameObject == null)
            {
                Debug.LogWarning("The camera mode controller is missing an object reference.");
                Debug.LogWarning("A scene probably has an overview or path cam.");
            }

            camPoint = this.gameObject;

            activeCamera = selectedCameraMode;

            SwitchCameraMode();
        }

        //The method the player uses to change mode.
        void ChangeCameraMode()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                selectedCameraMode++;

                if ((int)selectedCameraMode == Enum.GetNames(typeof(CameraModes)).Length)
                {
                    selectedCameraMode = 0;
                }

                UserFeedback.LogError("Changed Camera Mode to " + selectedCameraMode.ToString());
            }
        }

        //Activates the camera specified in selectedCameraMode.
        void SwitchCameraMode()
        {
            RemoveCameraController();

            activeCamera = selectedCameraMode;

            //Might be a better way to do this, but it beats the old one.
            switch (selectedCameraMode)
            {
                case CameraModes.FirstPerson:
                    gameObject.AddComponent<FirstPersonCameraController>();
                    break;
                case CameraModes.ThirdPerson:
                    gameObject.AddComponent<ThirdPersonCameraController>();
                    break;
                case CameraModes.Shoulder:
                    gameObject.AddComponent<ShoulderCameraController>();
                    break;
                case CameraModes.SceneOverview:
                    gameObject.AddComponent<SceneOverviewCameraController>();
                    break;
                case CameraModes.FreeCam:
                    gameObject.AddComponent<FreeCameraController>();
                    break;
                case CameraModes.FlyCam:
                    gameObject.AddComponent<FlyCameraController>();
                    break;
                case CameraModes.Static:
                    gameObject.AddComponent<StaticCameraController>();
                    break;
                case CameraModes.Follow:
                    gameObject.AddComponent<FollowCameraController>();
                    break;
                case CameraModes.FollowPath:
                    gameObject.AddComponent<FollowPathCameraController>();
                    break;
            }
        }

        public void ChangeCameraParent(Transform _newParent)
        {
            if (_newParent == null)
                Debug.LogWarning("[CameraModeController] Attempted to switch camera parent to null.");
            gameObject.transform.parent = _newParent;
        }

        public void RemoveCameraParent()
        {
            gameObject.transform.parent = null;
        }

        void RemoveCameraController()
        {
            //Remove script of type CameraController
            Destroy(GetComponent<CameraController>());
        }

        void FixedUpdate()
        {

#if UNITY_EDITOR
            ChangeCameraMode();
#endif

            if (activeCamera != selectedCameraMode)
            {
                SwitchCameraMode();
            }
        }
    }
}