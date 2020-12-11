using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBack : MonoBehaviour {

    void OnTriggerStay2D (Collider2D other)
    {
        Debug.Log("Entered");
        var magnitude = 10;
 
        var force = other.transform.position - transform.position;
 
        force.Normalize ();
        other.gameObject.GetComponent<Rigidbody2D>().AddForce(-force * magnitude);
    }
}
