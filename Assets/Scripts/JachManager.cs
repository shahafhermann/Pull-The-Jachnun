using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class JachManager : MonoBehaviour
{
    public GameObject p1;
    public GameObject p2;
    public new GameObject camera;
    public AudioSource soundSource;
    public List<AudioClip> sounds;
    public GameObject bar1;
    public GameObject bar2;
    public GameObject mask1;
    public GameObject mask2;
    public Canvas uiCanvas;
    public GameObject endScreenPrefab;
    public GameObject shotPrefab;
    public Sprite winner2;
    public Sprite tie;
    public float maxCameraDistance;
    public float viewMargin;
    public float zoomTime;
    private float zoomCurr;
    public List<GameObject> links;
    public List<GameObject> foods;
    public GameObject eggPrefab;
    public Image wayPoint;
    public Vector3 wayPointOffset;
    Vector3 startPos;
    private GameObject curEgg;
    private float staminaDrainFactor;
    private bool timeUp;
    private int p1Score = 0;
    private int p2Score = 0;
    
    public Image wayPoint;
    
    private GameObject[] shotPool;
    int currShotIdx;

    private void Awake()
    {
        foods = new List<GameObject>();
        links = new List<GameObject>();
        links.Add(GameObject.Find("mid_link"));
    }

    void Start()
    {
        timeUp = false;
        startPos = camera.transform.position;
        soundSource = camera.GetComponent<AudioSource>();
        zoomCurr = zoomTime;
        spawnEgg(false);
        shotPool = new GameObject[30];
        staminaDrainFactor = 0;
        for (int i = 0; i < 20; i++)
        {
            shotPool[i] = Instantiate(shotPrefab);
            shotPool[i].GetComponent<ShotScript>().setId(i);
            shotPool[i].SetActive(false);
        }
        currShotIdx = 0;
    }

    public GameObject getShot()
    {
        GameObject shot = shotPool[currShotIdx];
        currShotIdx = (currShotIdx == 29) ? 0 : currShotIdx + 1;
        return shot;
    }

    public void addPoint(int playerNum) {
        switch (playerNum) {
            case 1:
                p1Score++;
                break;
            case 2:
                p2Score++;
                break;
        }
    }

    public GameObject getCurrentEgg() {
        return curEgg;
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
            bool shouldCont = false;
            foreach (GameObject link in links) {
                if (math.abs(xPos - link.transform.position.x) < 2 &&
                    math.abs(yPos - link.transform.position.y) < 2) {
                    shouldCont = true;
                }
            }
            if (shouldCont) {
                continue; }
            
            // Passed the tests, instantiate
            curEgg = Instantiate(eggPrefab, new Vector3(xPos, yPos, 0f), Quaternion.identity);
            // foods.Add(curEgg);
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

    public void changeStaminaPercent(int player, float percent)
    {
        GameObject stamina = (player == 1) ? bar1 : bar2;
        GameObject mask = (player == 1) ? mask1 : mask2;
        if (staminaDrainFactor == 0) staminaDrainFactor =
                stamina.transform.GetComponent<RectTransform>().rect.height * uiCanvas.scaleFactor;
        Vector3 move = Vector3.up * staminaDrainFactor * (percent / 100);
        stamina.transform.position -= move;
        mask.transform.position += move;
    }

    // Update is called once per frame
    void Update()
    {
        camera.transform.position = moveCamera();

        // Food Waypoints
        if(!curEgg.GetComponent<Renderer>().isVisible){
            if (!wayPoint.enabled) {
                wayPoint.enabled = true;
            }
            float minX = wayPoint.GetPixelAdjustedRect().width / 2;
            float maxX = Screen.width - minX;
            float minY = wayPoint.GetPixelAdjustedRect().height / 2;
            float maxY = Screen.height - minY;

            Vector2 pos = Camera.main.WorldToScreenPoint(curEgg.transform.position);
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            wayPoint.transform.position = pos;
        }
        else if (wayPoint.enabled){
            wayPoint.enabled = false;
        }
        
        if (Input.GetKeyDown(KeyCode.R)) // TODO: delete
        {
            timeUp = true;
        }
        if (timeUp)
        {
            endGame();
            timeUp = false;
        }
        float minX = wayPoint.GetPixelAdjustedRect().width / 2;
        float maxX = Screen.width - minX;
        float minY = wayPoint.GetPixelAdjustedRect().height / 2;
        float maxY = Screen.height - minY;
        Vector2 pos = Camera.main.WorldToScreenPoint(curEgg.transform.position + wayPointOffset);
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        wayPoint.transform.position = pos;
    }

    void endGame()
    {
        Time.timeScale = 0;
        GameObject endScreen = Instantiate(endScreenPrefab);
        Image winner = endScreen.transform.Find("playerWinner").GetComponent<Image>();
        if (p1Score < p2Score)
        {
            winner.sprite = winner2;
        }
        // TODO: else if (p1Score == p2Score)  winner.sprite = tie;
        Button playAgain = endScreen.transform.Find("PlayAgainButton").GetComponent<Button>();
        playAgain.onClick.AddListener(newGame);
        soundSource.clip = sounds[0];
        soundSource.Play();
    }
    void newGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
