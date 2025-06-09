using UnityEngine;
using System.Collections;
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float dodgeSpeed = 10f;
    public float dodgeDuration = 0.5f;
    private bool isDodging = false;

    void Update()
    {
        if (!isDodging)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;
            transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Dodge());
        }
    }

    private IEnumerator Dodge()
    {
        isDodging = true;
        Vector3 dodgeDirection = transform.forward * dodgeSpeed;
        transform.Translate(dodgeDirection * Time.deltaTime, Space.World);
        yield return new WaitForSeconds(dodgeDuration);
        isDodging = false;
    }
}

