﻿using UnityEngine;
using System.Collections;

namespace Raider.Game.Cameras
{

    public class FirstPersonCameraController : PlayerCameraController
    {
        public FirstPersonCameraController()
        {
            pointStartingPos = new Vector3(0, 1.7f, 0);
        }

        new void Start()
        {
            //if (CameraModeController.instance.firstPersonCamSettings.moveWithBody)
            //{
            //    base.parent = GameObject.FindGameObjectWithTag("localPlayer").transform.Find("Graphics");
            //}
            base.Start();
        }

        // Update is called once per frame
        void Update()
        {
            RotatePlayer();
            RotateCamera();
            LockCamPointZRotation();
            LockCamPointYRotation();
        }

        void RotateCamera()
        {
            //Looking up and down, needs to be inverted for some reason...
            float _xRot = -Input.GetAxisRaw("Mouse Y");

            //If the camera is set to inverted mode, invert the rotation.
            if (CameraModeController.instance.firstPersonCamSettings.inverted)
            {
                _xRot = -_xRot;
            }

            Vector3 _rotation = new Vector3(_xRot, 0f, 0f) * CameraModeController.instance.firstPersonCamSettings.lookSensitivity;

            _rotation = ApplyXBufferToRotation(cam.transform.eulerAngles, _rotation);

            //Apply rotation
            camPoint.transform.Rotate(_rotation);
        }
    }
}