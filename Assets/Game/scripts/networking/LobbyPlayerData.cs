﻿using Raider.Game.Saves;
using UnityEngine.Networking;
using UnityEngine;
using Raider.Game.Scene;

namespace Raider.Game.Networking
{
    [System.Serializable]
    public class LobbyPlayerData : NetworkBehaviour
    {
        /// <summary>
        /// Is the PlayerData up to date?
        /// </summary>
        public bool gotData = false;
        void Start()
        {
            transform.SetParent(NetworkManager.instance.lobbyGameObject.transform);

            if (isLocalPlayer)
            {
                //If the player is hosting (if networkserver is active), isLeader will be true.
                UpdateLocalData(Session.saveDataHandler.GetUsername(), Session.activeCharacter, NetworkServer.active);

                if (NetworkManager.instance.currentNetworkState == NetworkManager.NetworkState.Host)
                    RpcRecieveUpdateFromServer(username, serializedCharacter, isLeader);
                else
                    CmdUpdateServer(username, serializedCharacter, isLeader);

                //If the player is not the host, they're automatically set to ready.
                //This means the host's ready flag starts the game.
                if (NetworkManager.instance.currentNetworkState == NetworkManager.NetworkState.Client)
                {
                    GetComponent<NetworkLobbyPlayer>().SendReadyToBeginMessage();
                    Debug.LogError("Requesting Lobby Update.");
                    CmdRequestLobbySetupUpdate();
                }
            }
        }

        //Player DATA
        public string username
        {
            get { return gameObject.name; }
            set { gameObject.name = value; }
        }
        public SaveDataStructure.Character character;
        public bool isLeader;

        #region serialization and player data syncing

        public string serializedCharacter
        {
            get { return Serialization.Serialize(character); }
            set { character = Serialization.Deserialize<SaveDataStructure.Character>(value); }
        }

        void UpdateLocalData(string _username, SaveDataStructure.Character _character, bool _isLeader)
        {
            this.username = _username;
            this.character = _character;
            this.isLeader = _isLeader;
            this.gotData = true;
            NetworkManager.instance.UpdateLobbyNameplates();
        }

        [Command]
        void CmdUpdateServer(string _username, string _serializedCharacter, bool _isHost)
        {
            RpcRecieveUpdateFromServer(_username, _serializedCharacter, _isHost);
            UpdateLocalData(_username, Serialization.Deserialize<SaveDataStructure.Character>(_serializedCharacter), _isHost);

            //If the client has sent their player data to the server, 
            //that means it's spawned and ready to recieve data regarding other players.
            //So now the server sends that data using the Client RPC.
            UpdateClientPlayerDataObjects();
        }

        [ClientRpc]
        public void RpcRecieveUpdateFromServer(string _username, string _serializedCharacter, bool _isHost)
        {
            UpdateLocalData(_username, Serialization.Deserialize<SaveDataStructure.Character>(_serializedCharacter), _isHost);
        }

        [Server]
        public static void UpdateClientPlayerDataObjects()
        {
            foreach (LobbyPlayerData player in NetworkManager.instance.players)
            {
                if (player.gotData)
                    player.RpcRecieveUpdateFromServer(player.username, player.serializedCharacter, player.isLeader);
            }
        }

        //If a player is remove from the scene, update the lobby!
        public override void OnNetworkDestroy()
        {
            base.OnNetworkDestroy();
            NetworkManager.instance.actionQueue.Enqueue(NetworkManager.instance.UpdateLobbyNameplates);
        }

        #endregion

        #region LobbySetup Syncing (Refactor Me!)

        [Command]
        void CmdRequestLobbySetupUpdate()
        {
            Debug.LogError("Sending Lobby Data.");
            TargetSendLobbySetup(connectionToClient, NetworkManager.instance.lobbySetup.Gametype, NetworkManager.instance.lobbySetup.Network, NetworkManager.instance.lobbySetup.SelectedScene);
        }

        //If a new player joins the lobby, this is used to send them the details.
        //I can't get this to work so instead I'm just updating all clients.
        //This should be refactored.
        [TargetRpc]
        public void TargetSendLobbySetup(NetworkConnection conn, string gametype, string network, string selectedScene)
        {
            Debug.LogError("Recieved lobby data");
            NetworkManager.instance.lobbySetup.Gametype = gametype;
            NetworkManager.instance.lobbySetup.Network = network;
            NetworkManager.instance.lobbySetup.SelectedScene = selectedScene;
        }

        //If the host changes the lobby setup, this sends the new details to the clients.
        [ClientRpc]
        public void RpcSendLobbySetup(string gametype, string network, string selectedScene)
        {
            //Hosts have a client and a server, but they don't need updating.
            //NetworkState.Client represents clients only.
            if (NetworkManager.instance.currentNetworkState == NetworkManager.NetworkState.Client)
            {
                Debug.Log("Recieved lobby data");
                NetworkManager.instance.lobbySetup.Gametype = gametype;
                NetworkManager.instance.lobbySetup.Network = network;
                NetworkManager.instance.lobbySetup.SelectedScene = selectedScene;
            }
        }

        [ClientRpc]
        public void RpcUpdateScenarioGametype()
        {
            Scenario.instance.currentGametype = NetworkManager.instance.lobbySetup.scenarioGametype;
        }

        #endregion
    }
}