using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

[RequireComponent(typeof(Interactable))]
public class WeaponCrate : MonoBehaviour
{
    [SerializeField] private string weaponName;

    private Interactable _inter;

    private void Awake()
    {
        _inter = GetComponent<Interactable>();
        _inter.SetInteractionText("pick up " + weaponName);
    }

    /*
     * used by the rewardmanager to make the crate actual have an existing gun name
     */
    public void SetWeaponName(string newName)
    {
        weaponName = newName;
        _inter.SetInteractionText("pick up " + weaponName);
    }

    /*
     * Gives the player the weapon in the crate identified by it's name
     */
    public void GiveGun()
    {
        Debug.Log("Give Gun");
        if (((PlayerController)_inter.GetInteractorInArea()).PickUpWeapon(weaponName)) Destroy(gameObject);
    }
}
