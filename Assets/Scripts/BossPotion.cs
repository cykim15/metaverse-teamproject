using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPotion : MonoBehaviour
{
    [SerializeField]
    private GameObject effectPrefab;
    [SerializeField]
    private float radius = 3f;
    [SerializeField]
    private float speed = 4f;
    [SerializeField]
    private float rotationSpeed = 200f;

    [HideInInspector]
    public float damage;

    private bool preventDoubleDamage = false;

    private void Start()
    {
        Vector3 targetPosition = Player.Instance.transform.position + new Vector3(0f, 1f, 0f);

        Vector3 direction = (targetPosition - transform.position).normalized;
        GetComponent<Rigidbody>().velocity = direction * speed;

        Vector3 randomRotation = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
        GetComponent<Rigidbody>().angularVelocity = randomRotation * rotationSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (preventDoubleDamage) return;
        preventDoubleDamage = true;
        Instantiate(effectPrefab, collision.contacts[0].point, Quaternion.identity);
        float distanceToPlayer = Vector3.Distance(transform.position, Player.Instance.transform.position);

        if (distanceToPlayer <= radius)
        {
            Player.Instance.GetHit(damage);
        }

        Destroy(gameObject);
    }



}
