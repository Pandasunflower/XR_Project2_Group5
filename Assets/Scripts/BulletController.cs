using System.Collections;
using System.Collections.Generic;
// using System.Numerics;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float maxDistance = 30;
    Vector3 initPos;
    
    void Start()
    {
        initPos = transform.position;
    }

    void Update()
    {
        float diffX = Mathf.Abs(transform.position.x - initPos.x);
        float diffY = Mathf.Abs(transform.position.y - initPos.y);
        float diffZ = Mathf.Abs(transform.position.z - initPos.z);

        if (diffX > maxDistance || diffY > maxDistance || diffZ > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // if (other.CompareTag("Enemy"))
        // {
        //     Destroy(gameObject);
        //     Destroy(other.gameObject);
        // }
    }
}
