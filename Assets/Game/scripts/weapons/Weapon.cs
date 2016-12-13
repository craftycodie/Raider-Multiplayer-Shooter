﻿using UnityEngine;

namespace Raider.Game.Weapons
{
    //Since weapons might have special abilities, they need seperate classes.
    //But all inherit some similarities, like the crosshair.
    public abstract class Weapon : MonoBehaviour
    {
        //Assigned in Editor.
        public Sprite crosshair;
        public Sprite blueprints;
        public GameObject graphics;

        protected abstract class WeaponSettings
        {
            public readonly float reloadTime;
            public readonly float damagePerShot;
            public readonly int clipSize;
            public readonly int maxAmmo;
        }

        //Three tiers should be available. Slow and powerful, mid, fast and weak.
        protected WeaponSettings chosenSettings;
    }
}