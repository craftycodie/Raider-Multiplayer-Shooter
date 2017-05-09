﻿using Raider.Game.Gametypes;
using Raider.Game.Networking;
using Raider.Game.Saves.User;
using Raider.Game.Weapons;
using Raider.Game.Cameras;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Raider.Game.GUI.Scoreboard;

namespace Raider.Game.Player
{
    public class NetworkPlayerController : NetworkBehaviour
    {
        public delegate void OnServerPlayerKilledPlayer(int idKilled, int idKilledBy);
        public delegate void OnServerPlayerRespawned(int idRespawned);
        public delegate void OnClientLocalPlayerHealthChange();

        public static OnServerPlayerKilledPlayer onServerPlayerKilledPlayer;
        public static OnServerPlayerRespawned onServerPlayerRespawned;
        public static OnClientLocalPlayerHealthChange onClientLocalPlayerHealthChange;

        private PlayerData playerData;
		public PlayerData PlayerData
		{
			get
			{
                if (playerData != null)
                    return playerData;
                else return playerData = GetComponent<PlayerData>();
			}
		}

		public override void OnStartClient()
		{
			base.OnStartClient();
			ScoreboardHandler.InvalidateScoreboard();
		}

		public override void OnNetworkDestroy()
		{
			if (GametypeController.singleton == null && !GametypeController.singleton.isGameEnding)
				GametypeController.singleton.InactivateScoreboardPlayer(PlayerData.syncData.id, PlayerData.syncData.team);
			base.OnNetworkDestroy();
		}

        void HealthSync(int value)
        {
            Health = value;

            if (isLocalPlayer && onClientLocalPlayerHealthChange != null)
                onClientLocalPlayerHealthChange();
        }

        [SyncVar(hook = "HealthSync")]
        private int health = MAX_HEALTH;
		public int Health
        {
            get { return health; }
            set {
                health = value; if (isLocalPlayer && onClientLocalPlayerHealthChange != null)
                    onClientLocalPlayerHealthChange();
            }
        }

        public const int MAX_HEALTH = 100;

		public bool IsAlive
		{
			get
			{
				if (Health > 0)
					return true;
				else
					return false;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="damageDealtBy">The ID of the player who dealt damage. -1 for no player.</param>
		[Server]
		public void TakeDamage(int damage, int damageDealtBy)
		{
			if (PlayerData.syncData.id == damageDealtBy)
				return;
			//Don't let players shoot themselves. Not sure how that would happen anyway.

			if (Health <= 0)
				return;
				//If they're already dead, stop killing them!

			Health -= damage;
			if (Health < 1)
			{ 
				KillPlayer();

				PlayerChatManager chatManager = PlayerData.localPlayerData.PlayerChatManager;
				chatManager.CmdSendNotificationMessage(PlayerChatManager.GetFormattedUsername(damageDealtBy) + " killed " + PlayerChatManager.GetFormattedUsername(PlayerData.syncData.id), -1);

				if (damageDealtBy > -1)
				{
					PlayerData player = NetworkGameManager.instance.GetPlayerDataById(damageDealtBy);

					Debug.Log("I was killed by " + player.name);

                    if (onServerPlayerKilledPlayer != null)
                        onServerPlayerKilledPlayer(PlayerData.syncData.id, player.syncData.id);
				}
			}
		}

		[Server]
		public void KillPlayer()
		{
			Debug.Log("This player died.");
			TargetKillPlayer(connectionToClient);
			RpcKillPlayer();

			GameObject ragDoll = Instantiate(PlayerResourceReferences.instance.GetRagdollByRace(PlayerData.PlayerSyncData.Character.Race));
			ragDoll.transform.position = this.transform.position;
			ragDoll.GetComponent<PlayerRagdoll>().UpdatePlayerAppearence(PlayerData.PlayerSyncData);
			NetworkServer.Spawn(ragDoll);
			RpcSetupRagdoll(ragDoll);

			StartCoroutine(WaitAndRespawn());
		}

		[ClientRpc]
		public void RpcSetupRagdoll(GameObject ragDoll)
		{
			ragDoll.transform.position = this.transform.position;
			ragDoll.GetComponent<PlayerRagdoll>().UpdatePlayerAppearence(PlayerData.PlayerSyncData);
		}

		[TargetRpc]
		public void TargetKillPlayer(NetworkConnection conn)
		{
			//When the player dies, switch their camera to spectate.
			for (int i = 0; i < NetworkGameManager.instance.Players.Count; i++)
				if(NetworkGameManager.instance.Players[i].PlayerSyncData.id == PlayerData.localPlayerData.PlayerSyncData.id)
				{
					CameraModeController.singleton.spectatingPlayerIndex = i;
					break;
				}

			GetComponent<MovementController>().canMove = false;

			CameraModeController.singleton.SetCameraMode(CameraModeController.CameraModes.SpectatorThirdPerson);
		}

		[ClientRpc]
		public void RpcKillPlayer()
		{
			//Hide the dead player.
			ScoreboardHandler.InvalidateScoreboard();
			GetComponent<PlayerData>().appearenceController.HidePlayer(true);
			ToggleWeapons(false);
		}

		public IEnumerator WaitAndRespawn()
		{
			yield return new WaitForSeconds(NetworkGameManager.instance.lobbySetup.syncData.gameOptions.generalOptions.respawnTimeSeconds);
			RespawnPlayer();
		}

		[Server]
		public void RespawnPlayer()
		{
			Health = MAX_HEALTH;
			Debug.Log("This player respawned.");

            if (onServerPlayerRespawned != null)
                onServerPlayerRespawned(PlayerData.syncData.id);

			TargetRespawnPlayer(connectionToClient);
			RpcRespawnPlayer();
		}

		[TargetRpc]
		public void TargetRespawnPlayer(NetworkConnection conn)
		{
			//Switch back to the normal camera.
			NetworkStartPosition[] startPositions = FindObjectsOfType<NetworkStartPosition>();
			int randomElement = Random.Range(0, startPositions.Length);
			gameObject.transform.position = startPositions[randomElement].transform.position;
			CameraModeController.singleton.SetCameraMode(Session.userSaveDataHandler.GetSettings().Perspective);

			if (!PlayerData.localPlayerData.paused)
				GetComponent<MovementController>().canMove = true;
		}

		[ClientRpc]
		public void RpcRespawnPlayer()
		{
			ScoreboardHandler.InvalidateScoreboard();
			GetComponent<PlayerData>().appearenceController.HidePlayer(false);
			ToggleWeapons(true);
		}

		public void ToggleWeapons(bool active)
		{
			PlayerData playerData = GetComponent<PlayerData>();

			if (active)
			{
				playerData.primaryWeaponController.gameObject.SetActive(true);
				playerData.secondaryWeaponController.gameObject.SetActive(true);
				playerData.tertiaryWeaponController.gameObject.SetActive(true);
			}
			else
			{
				playerData.primaryWeaponController.gameObject.SetActive(false);
				playerData.secondaryWeaponController.gameObject.SetActive(false);
				playerData.tertiaryWeaponController.gameObject.SetActive(false);
			}
		}

		public void SpawnWeapon(Armory.Weapons weapon)
        {
            if(!isLocalPlayer)
            {
                Debug.LogWarning("Attempted to spawn weapon on non local!");
                return;
            }

            WeaponSettings weaponCustomization = Armory.GetWeaponSettingsByWeaponAndVariation(weapon, Session.ActiveCharacter.GetWeaponChosenVariation(weapon));
            if (weaponCustomization == null)
            {
                weaponCustomization = Armory.GetWeaponMidSettings(weapon);
                UserSaveDataStructure.Character character = Session.ActiveCharacter;
                character.weaponCustomizations.Add(new Armory.WeaponAndVariation(weapon, Armory.WeaponVariation.Mid));
                Session.UpdateActiveCharacter(character);
            }
            CmdSpawnWeapon(weapon, weaponCustomization, PlayerData.localPlayerData.PlayerSyncData.id);
        }

        [Command]
        public void CmdSpawnWeapon(Armory.Weapons weapon, WeaponSettings customization, int ownerID)
        {
            GameObject newWeapon = Instantiate(Armory.GetWeaponPrefab(weapon), NetworkGameManager.instance.GetPlayerDataById(ownerID).gameObject.transform, false);

            newWeapon.GetComponent<WeaponController>().weaponCustomization = customization;
            newWeapon.GetComponent<WeaponController>().ownerId = ownerID;
			newWeapon.name = weapon.ToString() + ownerID;

            NetworkServer.SpawnWithClientAuthority(newWeapon, connectionToClient);

			WeaponController weaponController = newWeapon.GetComponent<WeaponController>();
			Armory.WeaponType weaponType = Armory.GetWeaponType(weapon);

			if (weaponController == null || weaponType == Armory.WeaponType.Special)
				return;
			else
			{
				switch (weaponType)
				{
					case Armory.WeaponType.Primary:
						GetComponent<PlayerData>().primaryWeaponController = weaponController;
						break;
					case Armory.WeaponType.Secondary:
						GetComponent<PlayerData>().secondaryWeaponController = weaponController;
						break;
					case Armory.WeaponType.Tertiary:
						GetComponent<PlayerData>().tertiaryWeaponController = weaponController;
						break;
				}
			}

			GetComponent<NetworkPlayerSetup>().TargetSpawnedWeapon(connectionToClient, newWeapon, weapon);
        }
    }
}
