using System.Collections;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float jumpForce;

    [Header("Movement")]
    public Transform startPoint;
    public Transform endPoint;
    public float moveSpeed = 0.2f;
    public float waitTime = 2f;
    private bool isMove = true;

    private void Start()
    {
        StartCoroutine(moveCoroutine());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    IEnumerator moveCoroutine()
    {
        while (true)
        {
            Vector3 pos = isMove ? endPoint.position : startPoint.position;

            while (Vector3.Distance(transform.position, pos) > 0.1f)
            {
                transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * moveSpeed);
                yield return null;
            }

            yield return new WaitForSeconds(waitTime);
            isMove = !isMove;
        }
    }
}