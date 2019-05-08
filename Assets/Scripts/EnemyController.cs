using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody[] rbs;
    void Start()
    {
        rbs = GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rbs)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        // rbs[0].isKinematic = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ActivateRagdoll()
    {
        GetComponent<Animator>().enabled = false;
        foreach (var rb in rbs)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        // rbs[0].isKinematic = true;
    }
}
