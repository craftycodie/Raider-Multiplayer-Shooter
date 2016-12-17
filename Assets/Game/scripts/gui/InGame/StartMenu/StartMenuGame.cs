﻿using Raider.Game.GUI.Screens;
using Raider.Game.Networking;
using Raider.Game.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Raider.Game.GUI.StartMenu
{
    public class StartMenuGame : StartMenuPane
    {
        [Header("Option Buttons")]
        public Button leaveGameButton;
        public Button endGameButton;
        public Button changeCharacterButton;
        public Button logOutButton;
        public Button debugButton;

        public Image optionImage;
        public Text optionText;

        public Sprite leaveGameSprite;
        public Sprite endGameSprite;
        public Sprite changeCharacterSprite;
        public Sprite logOutSprite;

        protected override void SetupPaneData()
        {
            NoHover();

            leaveGameButton.gameObject.SetActive(false);
            endGameButton.gameObject.SetActive(false);
            changeCharacterButton.gameObject.SetActive(false);
            logOutButton.gameObject.SetActive(false);

            leaveGameButton.onClick.RemoveAllListeners();
            endGameButton.onClick.RemoveAllListeners();
            changeCharacterButton.onClick.RemoveAllListeners();
            logOutButton.onClick.RemoveAllListeners();

            if (Scenario.InLobby)
            {
                changeCharacterButton.gameObject.SetActive(true);
                logOutButton.gameObject.SetActive(true);
                changeCharacterButton.onClick.AddListener(() => StartMenuHandler.instance.CloseStartMenu());
                changeCharacterButton.onClick.AddListener(() => MainmenuHandler.instance.ChangeCharacter());
                logOutButton.onClick.AddListener(() => StartMenuHandler.instance.CloseStartMenu());
                logOutButton.onClick.AddListener(() => MainmenuHandler.instance.Logout());
            }
            else
            {
                if (NetworkManager.instance.CurrentNetworkState == NetworkManager.NetworkState.Client)
                {
                    leaveGameButton.onClick.AddListener(() => StartMenuHandler.instance.CloseStartMenu());
                    leaveGameButton.onClick.AddListener(() => Scenario.instance.LeaveGame());
                    leaveGameButton.gameObject.SetActive(true);
                }
                else
                {
                    endGameButton.onClick.AddListener(() => StartMenuHandler.instance.CloseStartMenu());
                    endGameButton.onClick.AddListener(() => Scenario.instance.LeaveGame());
                    endGameButton.gameObject.SetActive(true);
                }
            }

        }

        public void NoHover()
        {
            optionImage.sprite = Scenario.GetMapImage(Scenario.instance.currentScene);
            optionText.text = Scenario.instance.currentGametype + " on " + Scenario.instance.currentScene;
        }

        public void LeaveGameHover()
        {
            optionImage.sprite = leaveGameSprite;
            optionText.text =
                "Leave the game and return to the main menu.";
        }

        public void EndGameHover()
        {
            optionImage.sprite = endGameSprite;
            optionText.text =
                "End the game and take your lobby back to the main menu.";
;        }

        public void ChangeCharacterHover()
        {
            optionImage.sprite = changeCharacterSprite;
            optionText.text =
                "Go back to the character selection screen.";
        }

        public void LogOutHover()
        {
            optionImage.sprite = logOutSprite;
            optionText.text =
                "Return to the login screen.";
        }

        //Debug Button

#if !UNITY_EDITOR
        private void Start()
        {
            debugButton.gameObject.SetActive(false);
        }
#else

        public void DebugHover()
        {
            optionImage.sprite = changeCharacterSprite;
            optionText.text =
                "No Function.";
        }

        public void DebugClick()
        {
            //Components.LobbyHandler.PlayerNameplate newPlate = new Components.LobbyHandler.PlayerNameplate("Test Nameplate", false, false, false, Session.activeCharacter);
            //Components.LobbyHandler.AddPlayer(newPlate);
        }
#endif
    }
}
