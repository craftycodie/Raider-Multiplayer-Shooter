﻿using UnityEngine;
using System.Collections.Generic;

//if/when different HUDs are available, inherit the basicas.
public class PlayerHUD : MonoBehaviour {

    public static PlayerHUD instance;
    public HUDWidget[] widgets
    {
        get { return GetComponentsInChildren<HUDWidget>(true); }
    }

	void Awake () {
        if (instance != null)
        {
            Debug.LogError("Multiple Player HUDs!");
            UserFeedback.LogError("Multiple Player HUDs!");
        }
        instance = this;
	}

    void OnDestroy()
    {
        instance = null;
    }
}