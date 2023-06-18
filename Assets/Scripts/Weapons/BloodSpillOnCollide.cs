using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider))]
public class BloodSpillOnCollide : MonoBehaviour
{
    private static readonly float BLOODSPILL_LIFETIME = 10;
    [SerializeField] private GameObject bloodSpillPrefab;

    private void OnCollisionEnter(Collision collision)
    {
        //only bloodsplat on default surfaces
        if (collision.gameObject.layer != 0) { return; }

        Transform BSP = Instantiate(bloodSpillPrefab,collision.contacts[0].point, Quaternion.identity).transform;
        BSP.rotation = Quaternion.FromToRotation(-BSP.forward, collision.GetContact(0).normal);
        BSP.position += -BSP.forward * 0.02f;
        Destroy(BSP.gameObject, BLOODSPILL_LIFETIME);

        enabled = false;
    }
}
