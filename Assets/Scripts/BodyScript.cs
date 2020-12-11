﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyScript : MonoBehaviour
{
    public float spawnDistance;
    public GameObject linkPrefab;
    public JachManager manager;
    private List<GameObject> links;
    public static GameObject midLink;
    public static GameObject lastLink1;
    public static GameObject lastLink2;
    public int playerNum;
    public float moveForce;
    private GameObject playerParent;
    Rigidbody2D headBody;
    [Range(1f, 10f)] public float moveSpeed = 11f;
    public KeyCode up;
    public KeyCode down;
    public KeyCode right;
    public KeyCode left;


    // Start is called before the first frame update
    void Start()
    {
        headBody = GetComponent<Rigidbody2D>();
        links = manager.links;
        moveForce = 30f;
        playerParent = transform.parent.gameObject;
        links.Add(GameObject.Find("mid_link"));
    }

    void addLink()
    {
        //Vector2 lastPos;
        //Vector2 spawnPos;
        //GameObject last;
        //Quaternion spawnRot;
        //if (links.Count == 0)
        //{
        //    last = this.gameObject;
        //    if (headBody.velocity.magnitude > 0)
        //    {
        //        spawnPos = headBody.velocity;
        //        spawnPos.Normalize();
        //    }
        //    else spawnPos = Vector2.left;
        //    spawnPos = new Vector2(transform.position.x, transform.position.y) - (spawnPos * spawnDistance);
        //    spawnRot = transform.rotation;
        //}
        //else
        //{
        //    last = links[links.Count - 1];
        //    lastPos = last.transform.position;
        //    Vector2 beforeLastPos;
        //    if (links.Count > 1) beforeLastPos = links[links.Count - 2].transform.position;
        //    else beforeLastPos = transform.position;
        //    spawnPos = 2* lastPos - beforeLastPos;
        //    spawnRot = links[links.Count - 1].transform.rotation;
        //}
        //GameObject newLink = Instantiate(linkPrefab, spawnPos, spawnRot);
        //newLink.GetComponent<HingeJoint2D>().enabled = false;
        //last.GetComponent<HingeJoint2D>().enabled = true;
        //last.GetComponent<HingeJoint2D>().connectedBody = newLink.GetComponent<Rigidbody2D>();
        //newLink.transform.parent = playerParent.transform;
        //links.Add(newLink);
        GameObject lastLink = (playerNum == 1) ? lastLink = links[0] : links[links.Count - 1];
        Vector2 push = (transform.position - lastLink.transform.position).normalized * spawnDistance;
        Vector2 spawnPos = new Vector2(lastLink.transform.position.x, lastLink.transform.position.y) + push;
        headBody.transform.position = (spawnPos + 1.8f * push);
        GameObject newLink = Instantiate(linkPrefab, spawnPos, Quaternion.Slerp(transform.rotation, lastLink.transform.rotation, 0.5f));
        newLink.transform.parent = playerParent.transform;
        GetComponent<HingeJoint2D>().connectedBody = newLink.GetComponent<Rigidbody2D>();
        newLink.GetComponent<HingeJoint2D>().connectedBody = lastLink.GetComponent<Rigidbody2D>();
        newLink.transform.parent = playerParent.transform;
        if (playerNum == 1) links.Insert(0, newLink);
        else links.Add(newLink);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(up)) {
            headBody.velocity = new Vector2(headBody.velocity.x, moveSpeed);
        } else if (Input.GetKey(down)) {
            headBody.velocity = new Vector2(headBody.velocity.x, -moveSpeed);
        } else if (Input.GetKey(right)) {
            headBody.velocity = new Vector2(moveSpeed, headBody.velocity.y);
        } else if (Input.GetKey(left)) {
            headBody.velocity = new Vector2(-moveSpeed, headBody.velocity.y);
        }
        if (Input.GetKeyDown(KeyCode.Space))  // TODO: detect on circle closure
        {
            addLink();
        }
        //if (headBody.velocity.magnitude > 0.1f)
        //{
        //    float angle = Mathf.Atan2(headBody.velocity.y, headBody.velocity.x) * Mathf.Rad2Deg + 180;
        //    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        //}
    }

    void checkFoodPickup(GameObject link)
    {
        //List<Vector2> vertices = new List<Vector2>();
        //vertices.Add(transform.position);
        //int i = 1;
        //while (links[i - 1] != link)
        //{
        //    vertices.Add(links[i - 1].transform.position);
        //    i++;
        //}
        //Vector2[] verArr = vertices.ToArray();
        //foreach (GameObject food in manager.foods)
        //{
        //    if (Poly.ContainsPoint(verArr, food.transform.position))
        //    {
        //        Debug.Log("ate!"); // TODO delete and respawn
        //        addLink();
        //    }
        //}
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "link")
        {
            checkFoodPickup(collision.gameObject);
        }
    }

    public static class Poly
    {
        public static bool ContainsPoint(Vector2[] polyPoints, Vector2 p)
        {
            var j = polyPoints.Length - 1;
            var inside = false;
            for (int i = 0; i < polyPoints.Length; j = i++)
            {
                var pi = polyPoints[i];
                var pj = polyPoints[j];
                if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
                    (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x))
                    inside = !inside;
            }
            return inside;
        }
    }
}
