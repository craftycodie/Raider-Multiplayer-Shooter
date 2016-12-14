﻿using Raider.Game.GUI.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Raider.Game.Scene;
using Raider.Game.Networking;
using Raider.Game.GUI.CharacterPreviews;
using UnityEngine.UI;

namespace Raider.Game.GUI.StartMenu
{
    public class StartMenuPlayer : StartMenuPane
    {
        public GridSelectionSlider perspectiveSelection;

        public GameObject characterPreviewImage;
        RawImage characterPreviewRawImage;

        const string PREVIEW_CHARACTER_NAME = "StartMenu";
        const CharacterPreviewHandler.PreviewType PREVIEW_TYPE = CharacterPreviewHandler.PreviewType.Full;

        void Start()
        {
            characterPreviewRawImage = characterPreviewImage.GetComponent<RawImage>();
        }

        protected override void SetupPaneData()
        {
            perspectiveSelection.onSelectionChanged = UpdatePerspectiveSelection;
            perspectiveSelection.title.text = "Perspective: " + Session.activeCharacter.chosenPlayerPerspective.ToString();

            switch (Session.activeCharacter.chosenPlayerPerspective)
            {
                case Cameras.CameraModeController.CameraModes.FirstPerson:
                    perspectiveSelection.SelectedObject = perspectiveSelection.gridLayout.transform.Find("FirstPerson").gameObject;
                    break;
                case Cameras.CameraModeController.CameraModes.ThirdPerson:
                    perspectiveSelection.SelectedObject = perspectiveSelection.gridLayout.transform.Find("ThirdPerson").gameObject;
                    break;
                case Cameras.CameraModeController.CameraModes.Shoulder:
                    perspectiveSelection.SelectedObject = perspectiveSelection.gridLayout.transform.Find("Shoulder").gameObject;
                    break;
            }

            StartCoroutine(SetupPlayerPreviewAfterAFrame());
        }

        IEnumerator SetupPlayerPreviewAfterAFrame()
        {
            yield return 0;
            CharacterPreviewHandler.instance.NewPreview(PREVIEW_CHARACTER_NAME, Session.activeCharacter, PREVIEW_TYPE, characterPreviewRawImage, characterPreviewImage.GetComponent<CharacterPreviewDisplayHandler>());
        }

        void UpdatePerspectiveSelection(GameObject newObject)
        {
            switch(newObject.name)
            {
                case "FirstPerson":
                    Session.activeCharacter.chosenPlayerPerspective = Cameras.CameraModeController.CameraModes.FirstPerson;
                    break;
                case "ThirdPerson":
                    Session.activeCharacter.chosenPlayerPerspective = Cameras.CameraModeController.CameraModes.ThirdPerson;
                    break;
                case "Shoulder":
                    Session.activeCharacter.chosenPlayerPerspective = Cameras.CameraModeController.CameraModes.Shoulder;
                    break;
            }
            if (!Scenario.InLobby)
                Player.Player.localPlayer.UpdatePerspective(Session.activeCharacter.chosenPlayerPerspective);
            else if (LobbyPlayerData.localPlayer != null)
                LobbyPlayerData.localPlayer.character.chosenPlayerPerspective = Session.activeCharacter.chosenPlayerPerspective;

            Session.SaveActiveCharacter();
            perspectiveSelection.title.text = "Perspective: " + Session.activeCharacter.chosenPlayerPerspective.ToString();
        }

        public override void ClosePane()
        {
            //Destroy the player preview...
            CharacterPreviewHandler.instance.DestroyPreviewObject(PREVIEW_CHARACTER_NAME);
            base.ClosePane();
        }

    }
}