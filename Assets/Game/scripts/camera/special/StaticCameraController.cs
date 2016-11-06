﻿using UnityEngine;
using System.Collections;

namespace Raider.Game.Cameras
{

    public class StaticCameraController : ThirdPersonCameraController
    {
        Vector3 cameraPosition;
        Quaternion cameraRotation;

        StaticCameraController()
        {
            camStartingPos = new Vector3(0, 1.8f, 0);
            pointStartingPos = Vector3.zero;
            pointStartingRot = Vector3.zero;
            camStartingRot = Vector3.zero;
            overrideWalking = false;
        }

        new void Start()
        {
            base.Start();

            //I shouldn't have to change the camera parent here, I should change the parentTransform variable.
            CameraModeController.singleton.RemoveCameraParent();
        }

        void Update()
        {
            RotatePlayer();
        }
    }
}
