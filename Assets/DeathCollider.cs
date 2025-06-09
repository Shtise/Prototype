using JUTPS;
using UnityEngine;

public class DeathCollider : MonoBehaviour
{
    private Transform _target;

    private void Start()
    {
        var player = GameObject.Find("TPS Character");
        if (player != null)
        {
            _target = player.transform;
        }
        else
        {
            Debug.LogError("Игрок с именем 'TPS Character' не найден!");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform != _target)
            return;

        var playerHealth = other.GetComponent<JUHealth>();
        playerHealth.DoDamage(99999);
    }
}
