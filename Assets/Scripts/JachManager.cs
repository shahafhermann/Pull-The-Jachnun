using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class JachManager : MonoBehaviour
{
    public GameObject p1;
    public GameObject p2;
    public GameObject camera;
    public float maxCameraDistance;
    public float viewMargin;
    public float zoomTime;
    private float zoomCurr;
    public List<GameObject> links;
    public List<GameObject> foods;
    Vector3 startPos;

    public GameObject eggPrefab;
    private GameObject curEgg;

    private void Awake()
    {
        foods = new List<GameObject>();
        links = new List<GameObject>();
        links.Add(GameObject.Find("mid_link"));
    }

    void Start()
    {
        startPos = camera.transform.position;
        zoomCurr = zoomTime;
        
        spawnEgg(false);
    }

    /**
     * Instantiate the first egg at a position different than the player's
     */
    public void spawnEgg(bool destroyOld) {
        if (destroyOld) {
            Destroy(curEgg);
        }
        
        int xPos, yPos;
        bool notValid = true;
        while (notValid) {
            xPos = Random.Range(-14, 15);
            yPos = Random.Range(-8, 6);
            // Check proximity to heads
            if (math.abs(xPos - p1.transform.position.x) < 2 &&
                math.abs(yPos - p1.transform.position.y) < 2 ||
                math.abs(xPos - p2.transform.position.x) < 2 &&
                math.abs(yPos - p2.transform.position.y) < 2) {
                continue;
            }
            
            // Check proximity to body
            foreach (GameObject link in links) {
                if (math.abs(xPos - link.transform.position.x) < 2 &&
                    math.abs(yPos - link.transform.position.y) < 2) {
                    continue;
                }
            }
            
            // Passed the tests, instantiate
            curEgg = Instantiate(eggPrefab, new Vector3(xPos, yPos, 0f), Quaternion.identity);
            foods.Add(GameObject.Find("Egg"));
            notValid = false;
        }
    }

    Vector3 moveCamera()
    {
        float camX = (p1.transform.position.x + p2.transform.position.x) / 2;
        float camY = (p1.transform.position.y + p2.transform.position.y) / 2;
        float f = camera.GetComponent<Camera>().fieldOfView;
        float distance = (p1.transform.position - p2.transform.position).magnitude;
        float camZ = -Mathf.Sqrt(3) * ((distance / 2) + viewMargin);
        if (zoomCurr <= 0) return new Vector3(camX, camY, camZ);
        zoomCurr -= Time.deltaTime;
        return Vector3.Lerp(new Vector3(camX, camY, camZ), startPos, zoomCurr / zoomTime);
    }



    // Update is called once per frame
    void Update()
    {
        camera.transform.position = moveCamera();
    }
}
