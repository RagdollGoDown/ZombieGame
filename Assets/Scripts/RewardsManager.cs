using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

class RewardManager : MonoBehaviour
{
    private readonly static string[] WEAPON_NAMES = { "AK-47", "Shotgun", "AK-47 Rafale","SubMachineGun" };
    
    [SerializeField] private GameObject weaponCratePrefab;

    private Transform[] _rewardCrateSpawns;

    private void Awake()
    {
        if (!weaponCratePrefab) throw new System.ArgumentException("You must set the weapons crate");

        List<Transform> rewardCrateSpawnList = new();

        foreach(Transform t in transform)
        {
            rewardCrateSpawnList.Add(t);
        }

        _rewardCrateSpawns = rewardCrateSpawnList.ToArray();

        if (_rewardCrateSpawns.Length == 0) 
            throw new System.ArgumentException("No children transforms to spawn the rewards");
    }

    /*
     * used by the objectives manage to tell the reward manager to give a reward to the player
     */
    public void GiveReward()
    {
        WeaponCrate wc = Instantiate(weaponCratePrefab,
            _rewardCrateSpawns[Random.Range(0, _rewardCrateSpawns.Length)]).GetComponent<WeaponCrate>();

        wc.SetWeaponName(SelectRandomWeaponName());
    }

    private string SelectRandomWeaponName()
    {
        return WEAPON_NAMES[Random.Range(0, WEAPON_NAMES.Length)];
    }
}