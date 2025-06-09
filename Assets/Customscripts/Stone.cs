using System;
using System.Collections;
using JUTPS;
using UnityEngine;

public class Stone : MonoBehaviour
{
    public Rigidbody _rigidbody;
    public Collider _collider;
    public LayerMask damageLayers; 
    public GameObject explosionEffect;
    public GameObject ShockWavePrefab;
    public float ForceToPlayer = 500f;
    public float UpForce = 200f;
    public float explosionRadius = 5f;
    public float damage = 50f;
    public float selfDestructTime = 15f; // время до самоуничтожения
    private float _timer;
    private bool _throwed;
    private bool _exploded;

    private Transform _target;

    private void Start()
    {
        _timer = selfDestructTime;
        var player = GameObject.Find("CharacterTargetTransform");
        if (player != null)
        {
            _target = player.transform;
            transform.LookAt(_target);
        }
        else
        {
            Debug.LogError("Игрок с именем 'TPS Character' не найден!");
        }
        _collider.enabled = false;
        _rigidbody.isKinematic = true;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!_exploded &&_throwed && ((1 << other.gameObject.layer) & damageLayers) != 0)
        {
            _exploded = true;
            Explode();
            
            Vector3 approxContactPoint = other.ClosestPoint(transform.position);
            Vector3 abovePoint = approxContactPoint + Vector3.up * 30f;
            LayerMask mask = LayerMask.GetMask("Terrain");
            if (Physics.Raycast(abovePoint, Vector3.down, out RaycastHit groundHit, 300f, mask))
            {
                var shockwave = Instantiate(ShockWavePrefab);
                shockwave.transform.position = groundHit.point;
            }
        }
    }
    
    private void Explode()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, damageLayers);
        foreach (Collider hit in hits)
        {
            JUHealth health = hit.GetComponent<JUHealth>();
            if (health != null)
            {
                health.DoDamage(damage);
            }
        }
        
        Destroy(gameObject);
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            Explode();
        }
    }
    
    public void Throw()
    {
        _throwed = true;
        _rigidbody.isKinematic = false;
        StartCoroutine(AddForce());
        _collider.enabled = true;
        transform.parent = null;
        StartCoroutine(MoveToPlayer());
    }

    private IEnumerator MoveToPlayer()
    {
        yield return new WaitForSeconds(2f);
        Vector3 direction = (_target.position - transform.position).normalized;
        _rigidbody.AddForce(direction * ForceToPlayer);
    }

    private IEnumerator AddForce()
    {
        yield return new WaitForEndOfFrame();
        Vector3 direction = (_target.position - transform.position).normalized + Vector3.up * 10;
        _rigidbody.AddForce(direction * UpForce);
    }
}