using System.Collections;
using JUTPS;
using UnityEngine;

public class Shockwave : MonoBehaviour
{
    public float maxRadius = 20f;
    public float expandSpeed = 10f;
    public float damage = 20f;
    public Collider SafeCollider;
    private JUHealth _target;
    private JUCharacterController _characterController;
    private float _current;

    void Start()
    {
        var player = GameObject.Find("TPS Character");
        if (player != null)
        {
            _target = player.GetComponent<JUHealth>();
            _characterController = _target.GetComponent<JUCharacterController>();
        }
        else
        {
            Debug.LogError("Игрок с именем 'TPS Character' не найден!");
        }
        StartCoroutine(Expand());
    }

    IEnumerator Expand()
    {
        _current = 0f;
        while (_current < maxRadius)
        {
            _current += expandSpeed * Time.deltaTime;
            transform.localScale = new Vector3(_current, 0.1f, _current);
            yield return null;
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == _target.gameObject)
        {
            bool isInsideSafeCollider = SafeCollider.bounds.Contains(_characterController.transform.position);

            
            if (!_characterController.IsDead && _characterController.IsGrounded && !isInsideSafeCollider) // реализовать метод
            {
                // var position = transform.position;
                // var spherePosition = new Vector3(position.x, _target.transform.position.y, position.z);
                // var distance = (spherePosition - _target.transform.position).sqrMagnitude;
                // Debug.LogError(Mathf.Sqrt(distance));
                // var safeZone = Mathf.Max(_current - 1f);
                // if(distance > safeZone * safeZone)
                    _target.DoDamage(damage);
            }
        }
    }
}