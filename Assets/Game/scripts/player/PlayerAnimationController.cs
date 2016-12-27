﻿using UnityEngine;
using Raider.Game.Cameras;
using System.Collections;
using UnityEditor.Animations;
using Raider.Game.Saves;

namespace Raider.Game.Player
{

    public class PlayerAnimationController : MonoBehaviour
    {

        private Animator attachedAnimator;

        public void SetupAnimationControllerForPerspective(CameraModeController.CameraModes perspective)
        {
            attachedAnimator.runtimeAnimatorController = null;
            attachedAnimator.runtimeAnimatorController = PlayerResourceReferences.instance.animatorControllers.GetControllerByPerspective(perspective);
        }

        // Use this for initialization
        void Start()
        {
            attachedAnimator = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            //Only update the animator of the camera controller allows movement.
            if (!CameraModeController.ControllerInstance.preventMovement)
            {
                attachedAnimator.SetFloat("verticalSpeed", Input.GetAxis("Vertical"));
                attachedAnimator.SetFloat("horizontalSpeed", Input.GetAxis("Horizontal"));

                //if (Input.GetButton("Jump"))
                //{
                //    attachedAnimator.SetBool("jumping", true);
                //    Invoke("StopJumping", 0.1f);
                //}

                if (Input.GetButton("Run") && Input.GetAxis("Vertical") > 0.25)
                {
                    attachedAnimator.SetBool("running", true);
                }
                else
                {
                    attachedAnimator.SetBool("running", false);
                }
            }
            else
            {
                StopAnimations();
            }
        }

        public void StopAnimations()
        {
            attachedAnimator.SetFloat("verticalSpeed", 0f);
            attachedAnimator.SetFloat("horizontalSpeed", 0f);
            attachedAnimator.SetBool("running", false);
            attachedAnimator.SetBool("jumping", false);
        }

        void StopJumping()
        {
            attachedAnimator.SetBool("jumping", false);
        }
    }
}