﻿using Raider.Game.Cameras;
using Raider.Game.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace Raider.Game.Weapons
{
    public class WeaponController : NetworkBehaviour
    {
        [SyncVar]
        public int ownerId;

        public LayerMask dontShoot;

        protected virtual void Start()
        {
            transform.SetParent(NetworkGameManager.instance.GetPlayerDataById(ownerId).transform, false);
        }

        public WeaponSettings weaponCustomization;

        public GameObject weaponFirePoint; //Assigned in inspector, where the bullet raycast begins.

        public int clipAmmo; //The ammo in the clip.
        public int totalAmmo; //Backpack ammo.

        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
                Reload();
            else if (Input.GetKey(KeyCode.Mouse0))
                Shoot();
        }


        float lastFired = 0;
        public virtual void Shoot()
        {
            if (Time.time - lastFired >= weaponCustomization.fireRate && !IsReloading)
            {
                if (clipAmmo <= 0)
                {
                    Reload();
                    return;
                }

                Vector3 firePointPosition;


                RaycastHit raycastHit;

#if DEBUG
                firePointPosition = CameraModeController.singleton.cam.transform.position + CameraModeController.singleton.cam.transform.forward * weaponCustomization.range;
                Debug.DrawLine(CameraModeController.singleton.cam.transform.position, firePointPosition, Color.red);

#endif

                for (int projectileCount = 0; projectileCount < weaponCustomization.projectileCount; ++projectileCount)
                {
                    Vector2 bulletSpread = Random.insideUnitCircle * weaponCustomization.bulletSpread;

                    firePointPosition = CameraModeController.singleton.cam.transform.position + CameraModeController.singleton.cam.transform.forward * weaponCustomization.range + CameraModeController.singleton.cam.transform.right * bulletSpread.x + CameraModeController.singleton.cam.transform.up * bulletSpread.y;

                    if (Physics.Linecast(CameraModeController.singleton.cam.transform.position, firePointPosition, out raycastHit, ~dontShoot))
                    {
                        firePointPosition = raycastHit.point;
                    }

#if DEBUG
                    Debug.DrawLine(CameraModeController.singleton.cam.transform.position, firePointPosition, Color.magenta);
#endif

                    Debug.DrawLine(weaponFirePoint.transform.position, firePointPosition, Color.green);
                }

                clipAmmo--;

                lastFired = Time.time;
            }
        }

        public virtual void Recoil()
        {
            //Apply recoil...
        }
        
        float lastReload = 0;
        public virtual void Reload()
        {
            if (totalAmmo <= 0)
                return;

            lastReload = Time.time;
            if (totalAmmo < weaponCustomization.clipSize)
                clipAmmo = totalAmmo;
            else
                clipAmmo = weaponCustomization.clipSize;
            totalAmmo -= clipAmmo;
        }

        bool IsReloading
        {
            get { if (Time.time - lastReload >= weaponCustomization.reloadTime) return false; else return true; }
        }
    }
}