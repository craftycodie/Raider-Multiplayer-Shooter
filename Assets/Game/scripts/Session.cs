﻿using UnityEngine;
using System.Collections;

public static class Session {
    public static ISaveDataHandler saveDataHandler;
    public static bool online = false;
    public static SaveDataStructure.Character character;

    public static void Login(string _username)
    {
        InitializeSaveDataHandler();
        saveDataHandler.SetUsername(_username);
    }

    public static void SelectCharacter(int slot)
    {
        character = saveDataHandler.GetCharacter(slot);
    }

    public static void InitializeSaveDataHandler()
    {
        //if (online)
        //{
        //    saveDataHandler = new MongoSaveDataHandler();
        //}
        //else
        //{
            saveDataHandler = new LocalSerializedSaveDataHandler();
            //saveDataHandler = GameObjectSaveDataHandler.CreateGameObjectSaveDataHandler();
        //}
    }
}
