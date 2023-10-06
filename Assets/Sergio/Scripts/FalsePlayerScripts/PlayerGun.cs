
using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    public float damage = 10f;
    public float damageTower = 1f;
    public float range = 100f;

    //public Transform muzzle;

    public CharacterAiming characterAiming;

    public Transform muzzle; // Transform del punto de inicio de la línea
    public Transform endPoint;   // Transform del punto final de la línea

    private LineRenderer lineRenderer; // Referencia al componente LineRenderer

    public ParticleSystem muzzleFlash;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();


    }

    void FixedUpdate()
    {
        //Debug.DrawRay(muzzle.position, muzzle.transform.forward * range, Color.red);
        if (Input.GetMouseButton(0) && characterAiming.canShoot == true)
        {
            if (lineRenderer == null)
            {
                // Si no hay LineRenderer, lo agregamos
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            // Configurar propiedades de la línea
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.positionCount = 2; // Dos puntos (inicio y fin)

            // Establecer los puntos de inicio y fin de la línea
            lineRenderer.SetPosition(0, muzzle.position);
            lineRenderer.SetPosition(1, endPoint.position);
            shoot();
        }
    }

    void shoot()
    {
        muzzleFlash.Play();
        RaycastHit hit;
        if (Physics.Raycast(muzzle.position, muzzle.transform.forward, out hit, range))
        {
            // Debug.Log(hit.transform.name);
            if (hit.collider.gameObject.tag== "IA")
            {
                Debug.Log("Daño a la AI");
                EnemyLife target = hit.transform.GetComponent<EnemyLife>();
                if (target != null)
                {
                    target.TakeDamage(damage);
                }
                EnemyLifeTower target2 = hit.transform.GetComponent<EnemyLifeTower>();
                if (target2 != null)
                {
                    target2.TakeDamage(damageTower);
                }
            }
        }
    }
}
