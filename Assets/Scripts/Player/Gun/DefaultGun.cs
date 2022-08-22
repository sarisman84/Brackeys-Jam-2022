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
    protected override IEnumerator BulletDefinition(WeaponManager weaponManager, Transform barrel)
    {
        if (barrel == null) yield break;

        GameObject obj = Instantiate(bulletEffect);
        obj.transform.position = barrel.position;
        obj.transform.forward = barrel.forward;


        LineRenderer line = obj.GetComponentInChildren<LineRenderer>();

        if (!line) yield break;


        line.SetPosition(0, barrel.position);
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

    protected override void Fire(WeaponManager weaponManager, Transform barrel)
    {

        //weaponManager.recoilCM.GenerateImpulse(Camera.main.transform.forward.normalized * recoilForce);

        LayerMask mask = LayerMask.NameToLayer("Default");

        Vector3 spread = Random.insideUnitSphere * spreadAmm;
        spread.z = 0;

        bool intersecting = InteractionUtilities.IntersectFromCamera(Camera.main, spread, maxHitRange, mask, out var hitInfo);

        if (intersecting && hitInfo.collider.GetComponent<IDamageable>() is IDamageable damageable)
        {
            targetPos = hitInfo.point;
            damageable.OnDamageTaken(damage);
        }
        else
        {
            targetPos = barrel.position + (barrel.forward.normalized + spread) * maxHitRange;
        }
    }

    protected override IEnumerator MuzzleDefinition(WeaponManager weaponManager, Transform barrel)
    {
        if (barrel == null || !muzzleEffect) yield break;
        GameObject obj = Instantiate(muzzleEffect, barrel);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        ParticleSystem particleS = obj.GetComponentInChildren<ParticleSystem>();

        if (!particleS) yield break;

        yield return new WaitWhile(() => { return particleS && particleS.isEmitting; });



        Destroy(obj);
    }
}
