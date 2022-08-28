using System.Collections;
using UnityEngine;
using System.Collections.Generic;

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

        GameObject obj = Instantiate(bulletEffect, PollingStation.Instance.gameManager.GetEntityParent());
        obj.transform.position = gun.gunBarrel.position;
        obj.transform.forward = gun.gunBarrel.forward;
        gun.activeBulletEffects.Add(obj);

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
        gun.activeBulletEffects.Remove(obj);
    }

    protected override void Fire(Gun gun)
    {
        PollingStation.Instance.audioManager.Play("GunFire", gun.gunBarrel.position);

        //weaponManager.recoilCM.GenerateImpulse(Camera.main.transform.forward.normalized * recoilForce);

        string printOut = $"[Log]<DefaultGun/{gun.gunBarrel.parent.name}>: Firing weapon ";



        printOut += $" on layer <*no one knows*>. [Result]: ";

        Vector3 spread = Random.insideUnitSphere * (gun.isAimingDownTheSights ? spreadAmm / 2.0f : spreadAmm);
        spread.z = 0;

        Ray firingDirection = FiringDirection(gun);

        bool intersecting = InteractionUtilities.Raycast(firingDirection, spread, maxHitRange, hitMask, out var hitInfo, true);



        if (intersecting)
        {
            if(hitInfo.collider.GetComponent<IDamageable>() is IDamageable damageable) {
                printOut += $"Dealing Damage ({damage})!";
                targetPos = hitInfo.point;
                damageable.OnDamageTaken(damage);
                PollingStation.Instance.audioManager.Play("BulletImpact Organic", hitInfo.point);
            }
            else {
                targetPos = hitInfo.point;
                CreateImpactDecal(hitInfo);//create bullet hole for not damagable objects
                PollingStation.Instance.audioManager.Play("BulletImpact General", hitInfo.point);
            }
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
