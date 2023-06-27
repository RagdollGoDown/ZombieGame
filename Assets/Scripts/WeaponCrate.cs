using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class WeaponnCrate : MonoBehaviour
{
    [SerializeField] private string weaponName;

    private Interactable _inter;

    private void Awake()
    {
        _inter = GetComponent<Interactable>();
        _inter.SetInteractionText("pick up " + weaponName);
    }

    /*
     * Gives the player the weapon in the crate identified by it's name
     */
    public void GiveGun()
    {
        _inter.GetPlayerInArea().PickUpWeapon(weaponName);
    }
}
