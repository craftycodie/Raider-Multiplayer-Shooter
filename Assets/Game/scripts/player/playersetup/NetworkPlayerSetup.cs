﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Raider.Game.Player;
using Raider.Game.Saves;
using Raider;
using Raider.Game.Cameras;
using Raider.Game.Networking;

namespace Raider.Game.Player
{
    [RequireComponent(typeof(PlayerChatManager))]
    [RequireComponent(typeof(PlayerData))]
    [RequireComponent(typeof(PlayerResourceReferences))]
    public class NetworkPlayerSetup : NetworkBehaviour
    {
        public static NetworkPlayerSetup localPlayer;
        private PlayerData playerData;

        // Use this for initialization

        void Start()
        {
            playerData = GetComponent<PlayerData>();

            //If the player is a client, or is playing alone, add the moving mechanics.
            if (isLocalPlayer)
            {
                PlayerData.localPlayerData = playerData;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                localPlayer = this;
            }

            playerData.appearenceController.ReplacePlayerModel(playerData);

            if(isLocalPlayer)
            {
                playerData.appearenceController.ChangePerspectiveModel(Session.userSaveDataHandler.GetSettings().perspective);
            }
        }

        [TargetRpc]
        public void TargetSetupLocalControl(NetworkConnection conn)
        {
            gameObject.AddComponent<MovementController>();
            playerData.animationController = gameObject.AddComponent<AnimationParametersController>();
            playerData.gamePlayerController = gameObject.AddComponent<LocalPlayerController>();
            CameraModeController.singleton.playerGameObject = gameObject;
            //CameraModeController.singleton.SetCameraMode(Session.saveDataHandler.GetSettings().perspective);
            playerData.gamePlayerController.UpdatePerspective(Session.userSaveDataHandler.GetSettings().perspective);
        }

        //Detatch Camera, Prototype.
        private void OnDestroy()
        {
            if (CameraModeController.singleton.GetCameraController() is PlayerCameraController)
                CameraModeController.singleton.gameObject.transform.SetParent(null, false);
        }
    }
}
