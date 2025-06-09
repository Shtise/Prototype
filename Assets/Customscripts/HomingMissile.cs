using JUTPS;
using UnityEngine;

public class HomingMissile : MonoBehaviour
{
    [Header("Targeting Settings")]
    public float homingStopDistance = 5f; // расстояние, на котором ракета перестаёт наводиться
    public float speed = 10f; // скорость ракеты
    public float rotationSpeed = 200f; // скорость поворота при самонаведении
    
    [Header("Lifetime Settings")]
    public float selfDestructTime = 5f; // время до самоуничтожения
    
    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public float damage = 50f;
    public GameObject explosionEffect;
    public LayerMask damageLayers; 
    
    private Transform _target;
    private bool _isHoming = true;
    private float _timer;

    public float _waitBeforeStart = 0f;
    private float _currentTimerBeforeStart = 0f;
    
    private void Start()
    {
        _currentTimerBeforeStart = _waitBeforeStart;
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

        _timer = selfDestructTime;
        OnStart();
    }

    protected virtual void OnStart()
    {
    }

    private void Update()
    {
        if (_currentTimerBeforeStart > 0)
        {
            _currentTimerBeforeStart -= Time.deltaTime;
            return;
        }
        
        if (_target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, _target.position);

            if (distanceToTarget <= homingStopDistance)
                _isHoming = false;

            if (_isHoming)
            {
                Vector3 direction = (_target.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }
        }

        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            Explode();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & damageLayers) != 0)
        {
            Explode();
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
}