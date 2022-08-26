using System.Collections;
using UnityEngine;
using System.Threading.Tasks;
using Cinemachine;

[CreateAssetMenu(fileName = "New Default Gun", menuName = "Gun/Default", order = 0)]
public class DefaultGun : BaseGun
{
    [Header("Default Gun Settings")]
    public float spreadAmm = 0.05f;
    public float maxHitRange = 10.0f;



    private Vector3 targetPos;
    protected override IEnumerator BulletDefinition(Gun gun)
    {
        if (gun.gunBarrel == null) yield break;

        GameObject obj = Instantiate(bulletEffect);
        obj.transform.position = gun.gunBarrel.position;
        obj.transform.forward = gun.gunBarrel.forward;


        LineRenderer line = obj.GetComponentInChildren<LineRenderer>();

        if (!line) yield break;


        line.SetPosition(0, gun.gunBarrel.position);
        line.SetPosition(1, targetPos);



        float width = line.startWidth;

        while (width > 0)
        {
            width -= Time.deltaTime * 2.0f;
            line.startWidth = width;
            line.endWidth = width;
            yield return null;

        }

        Destroy(obj);
    }

    protected override void Fire(Gun gun)
    {

        //weaponManager.recoilCM.GenerateImpulse(Camera.main.transform.forward.normalized * recoilForce);

        string printOut = $"[Log]<DefaultGun/{gun.gunBarrel.parent.name}>: Firing weapon ";



        printOut += $" on layer <{LayerMask.LayerToName(hitMask)}>. [Result]: ";

        Vector3 spread = Random.insideUnitSphere * (gun.isAimingDownTheSights ? spreadAmm / 2.0f : spreadAmm);
        spread.z = 0;

        Ray firingDirection = FiringDirection(gun);

        bool intersecting = InteractionUtilities.Raycast(firingDirection, spread, maxHitRange, hitMask, out var hitInfo, true);



        if (intersecting && hitInfo.collider.GetComponent<IDamageable>() is IDamageable damageable)
        {
            printOut += $"Dealing Damage ({damage})!";
            targetPos = hitInfo.point;
            damageable.OnDamageTaken(damage);
        }
        else
        {
            printOut += "Missed!";
            targetPos = gun.gunBarrel.position + (gun.gunBarrel.forward.normalized + spread) * maxHitRange;
        }

        Debug.Log(printOut);
    }

    protected override IEnumerator MuzzleDefinition(Gun gun)
    {

        if (gun.gunBarrel == null || !muzzleEffect) yield break;
        GameObject obj = Instantiate(muzzleEffect, gun.gunBarrel);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        ParticleSystem particleS = obj.GetComponentInChildren<ParticleSystem>();

        if (!particleS) yield break;

        yield return new WaitWhile(() => { return particleS && particleS.isEmitting; });



        Destroy(obj);
    }
}
