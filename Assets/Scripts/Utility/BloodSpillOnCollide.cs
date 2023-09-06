using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BloodSpillOnCollide : MonoBehaviour
{
    private static readonly float BLOODSPILL_LIFETIME = 10;

    private static int BLOOD_SPILLS_INDEX;

    [SerializeField] private GameObject[] bloodSpillPrefabs;
    [SerializeField] private bool destroyOnCollide;
    [SerializeField] private float scale;

    private void OnCollisionEnter(Collision collision)
    {
        //only bloodsplat on default surfaces
        if (collision.gameObject.layer != 0) { return; }

        if (bloodSpillPrefabs == null || bloodSpillPrefabs.Length == 0) { return; }

        GameObject bloodSpill = bloodSpillPrefabs[BLOOD_SPILLS_INDEX++ % bloodSpillPrefabs.Length];

        if (bloodSpill)
        {
            Transform BSP = Instantiate(bloodSpill, collision.contacts[0].point, Quaternion.identity).transform;
            BSP.rotation = Quaternion.FromToRotation(-BSP.forward, collision.GetContact(0).normal);
            BSP.position += -BSP.forward * 0.02f;
            BSP.localScale *= scale;
            Destroy(BSP.gameObject, BLOODSPILL_LIFETIME);
        }

        if (destroyOnCollide) Destroy(gameObject);
    }
}
