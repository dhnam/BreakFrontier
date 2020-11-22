using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public bool isGameOver = false;
    public float frameDelta;
    public int score;
    public List<GameObject> blinkList;
    public List<GameObject> ballList;
    public AudioClip gameover;
    public GameObject tilemap;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        frameDelta = Time.fixedDeltaTime;
    }

    bool invoked = false;
    bool started = false;
    // Update is called once per frame
    void Update()
    {
        if (!started && ballList.Count != 0)
        {
            started = true;
        }
        if (started && ballList.Count == 0)
        {
            isGameOver = true;
        }
        if (isGameOver)
        {
            if (!invoked)
            {
                AudioSource.PlayClipAtPoint(gameover, Camera.main.transform.position);
                StartCoroutine(WaitAndInvoke(GameOver, 2));
                StartCoroutine(Blink());
            }
            invoked = true;
            Time.timeScale = 0.0f;
        }
    }

    IEnumerator WaitAndInvoke(System.Action action, float t)
    {
        yield return new WaitForSecondsRealtime(t);
        action();
    }
    
    IEnumerator Blink()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        for (int i = 0; i < blinkList.Count; i++)
        {
            blinkList[i].SetActive(false);
        }
        yield return new WaitForSecondsRealtime(0.5f);
        for (int i = 0; i < blinkList.Count; i++)
        {
            blinkList[i].SetActive(true);
        }
        yield return new WaitForSecondsRealtime(0.5f);
        for (int i = 0; i < blinkList.Count; i++)
        {
            blinkList[i].SetActive(false);
        }
        yield return new WaitForSecondsRealtime(0.5f);
        for (int i = 0; i < blinkList.Count; i++)
        {
            blinkList[i].SetActive(true);
        }
    }
    void GameOver()
    {
        Time.timeScale = 1;
        isGameOver = false;
        invoked = false;
        started = false;
        score = 0;
        for (int i = 0; i < ballList.Count; i++)
        {
            Destroy(ballList[i]);
        }
        ballList.Clear();
        tilemap.SendMessage("Restart");
    }
}
