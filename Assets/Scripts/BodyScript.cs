using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyScript : MonoBehaviour
{
    public float spawnDistance;
    public GameObject linkPrefab;
    public List<GameObject> links;
    private GameObject playerParent;
    Rigidbody2D headBody;
    
    [Range(0.1f, 7f)] 
    public float moveSpeed = 3f;

    [Range(50f, 200f)] 
    public float rotationSpeed = 100f;

    public KeyCode up;
    public KeyCode down;
    public KeyCode right;
    public KeyCode left;

    // Start is called before the first frame update
    void Start()
    {
        links = new List<GameObject>();
        headBody = GetComponent<Rigidbody2D>();  // TODO It might be better to just drag it in the inspector
        playerParent = transform.parent.gameObject;
        for (int i = 0; i < 5; i++) // TODO get rid of this
        {
            addLink();
        }
    }

    void addLink()
    {
        Vector2 lastPos;
        Vector2 spawnPos;
        GameObject last;
        Quaternion spawnRot;
        if (links.Count == 0)
        {
            last = this.gameObject;
            if (headBody.velocity.magnitude > 0)
            {
                spawnPos = headBody.velocity;
                spawnPos.Normalize();
            }
            else spawnPos = Vector2.left;
            spawnPos = new Vector2(transform.position.x, transform.position.y) - (spawnPos * spawnDistance);
            spawnRot = transform.rotation;
        }
        else
        {
            last = links[links.Count - 1];
            lastPos = last.transform.position;
            Vector2 beforeLastPos;
            if (links.Count > 1) beforeLastPos = links[links.Count - 2].transform.position;
            else beforeLastPos = transform.position;
            spawnPos = 2* lastPos - beforeLastPos;
            spawnRot = links[links.Count - 1].transform.rotation;
        }
        GameObject newLink = Instantiate(linkPrefab, spawnPos, spawnRot);
        newLink.GetComponent<HingeJoint2D>().enabled = false;
        last.GetComponent<HingeJoint2D>().enabled = true;
        last.GetComponent<HingeJoint2D>().connectedBody = newLink.GetComponent<Rigidbody2D>();
        newLink.transform.parent = playerParent.transform;
        links.Add(newLink);
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKey(up)) {
        //     headBody.MovePosition(transform.forward);
        // } else if (Input.GetKey(right)) {
        //     headBody.MoveRotation(
        //         Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0,90f,0), 
        //             Time.deltaTime * rotationSpeed));
        // } else if (Input.GetKey(left)) {
        //     headBody.MoveRotation(
        //         Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0,-90f,0), 
        //             Time.deltaTime * rotationSpeed));
        // }
        
        if (Input.GetKeyDown(KeyCode.Space))  // TODO: detect circle closure instead
        {
            addLink();
        }
        if (headBody.velocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(headBody.velocity.y, headBody.velocity.x) * Mathf.Rad2Deg + 180;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void FixedUpdate() {
        Vector3 playerPos = transform.eulerAngles;
     
        if (Input.GetKey(left)) {
            if (playerPos.y > 93 || playerPos.y < 87) {
                transform.Rotate(0, 0 ,1 * rotationSpeed * Time.deltaTime);
            }
        }
      
        if (Input.GetKey(right))
        { 
            if (playerPos.y > 273 || playerPos.y < 267) {
                transform.Rotate(0, 0 ,-1 * rotationSpeed
                                          * Time.deltaTime);
            }
        }
        if (Input.GetKey(up)) {
            Vector2 direction = -transform.right;
            headBody.MovePosition(headBody.position + moveSpeed * Time.deltaTime * direction);
        }
    }

    void checkFoodPickup(GameObject link)
    {
        List<Vector3> vertices = new List<Vector3>();
        vertices.Add(transform.position);
        int i = 1;
        while(links[i - 1] != link)
        {
            vertices.Add(links[i - 1].transform.position);
            i++;
        }
        int[] triangles = new int[vertices.Count * 3];
        for (i = 0; i < vertices.Count - 2; i++)
        {
            triangles[i * 3] = i + 2;
            triangles[i * 3 + 1] = 0;
            triangles[i * 3 + 2] = i + 1;
        }
        Mesh container = new Mesh();
        container.vertices = vertices.ToArray();
        container.triangles = triangles;

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "link")
        {
            checkFoodPickup(collision.gameObject);
        }
    }
}
