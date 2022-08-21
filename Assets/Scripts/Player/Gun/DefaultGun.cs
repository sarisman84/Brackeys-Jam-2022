using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "New Default Gun", menuName = "Gun/Default", order = 0)]
public class DefaultGun : BaseGun
{
    public GameObject muzzleEffect;
    public GameObject bulletEffect;
    public override void OnGunUse(RaycastHit hitInfo, GameObject gunModel, MonoBehaviour coroutineStarter)
    {
        Transform barrel = null;
        for (int i = 0; i < gunModel.transform.childCount; i++)
        {
            Transform child = gunModel.transform.GetChild(i);
            if (child.tag.ToLower().Equals("barrel"))
            {
                barrel = child;
                break;
            }
        }




        coroutineStarter.StartCoroutine(SpawnBulletEffect(barrel, hitInfo.collider ? hitInfo.point : barrel.transform.position + Camera.main.transform.forward.normalized * 500));
        coroutineStarter.StartCoroutine(SpawnMuzzleEffect(barrel));
        if (hitInfo.collider && hitInfo.collider.GetComponent<IDamageable>() is IDamageable damageable)
        {
            damageable.OnDamageTaken(5.0f);
        }


    }

    private IEnumerator SpawnBulletEffect(Transform barrel, Vector3 targetPos)
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
    private IEnumerator SpawnMuzzleEffect(Transform barrel)
    {


        if (barrel == null) yield break;
        GameObject obj = Instantiate(muzzleEffect, barrel);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        ParticleSystem particleS = obj.GetComponentInChildren<ParticleSystem>();

        if (!particleS) yield break;

        yield return new WaitWhile(() => { return particleS && particleS.isEmitting; });



        Destroy(obj);


    }
}
