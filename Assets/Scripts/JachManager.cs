using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Start is called before the first frame update

    private void Awake()
    {
        foods = new List<GameObject>();
        links = new List<GameObject>();
        foods.Add(GameObject.Find("Egg"));
        links.Add(GameObject.Find("mid_link"));
    }

    void Start()
    {
        startPos = camera.transform.position;
        zoomCurr = zoomTime;
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
