using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using Player;

[RequireComponent(typeof(Interactable))]
public class PlayerInteractions : MonoBehaviour
{
    [SerializeField] private PlayerUI.Menu menu;

    public void Open(){
        Interactable interactable = GetComponent<Interactable>();

        PlayerController player = (PlayerController)interactable.GetInteractorInArea();

        player.OpenMenu(menu);
    }

    public void GiveWeapon(){
        Interactable interactable = GetComponent<Interactable>();

        PlayerController player = (PlayerController)interactable.GetInteractorInArea();

        string weaponName;
        int tries = 10;

        do {
            weaponName = player._weaponsNamesWhoCanBeEquiped[Random.Range(0, player._weaponsNamesWhoCanBeEquiped.Count)];
        } while (player._weaponsNamesWhoCanBeEquiped.Contains(weaponName) && tries-- > 0);

        player.PickUpWeapon(weaponName);
        Destroy(gameObject);
    }
}
