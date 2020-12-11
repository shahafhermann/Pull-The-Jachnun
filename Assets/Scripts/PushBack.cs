using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBack : MonoBehaviour {

    [Range(1000f, 3000f)]
    public float pushBackMagnitude = 1500f;

    void OnTriggerStay2D (Collider2D other)
    {
        var force = other.transform.position - transform.position;
 
        force.Normalize ();
        other.gameObject.GetComponent<Rigidbody2D>().AddForce(-force * pushBackMagnitude);
    }
}