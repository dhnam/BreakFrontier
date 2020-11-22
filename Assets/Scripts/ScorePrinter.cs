using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePrinter : MonoBehaviour
{
    public List<Sprite> numbers;
    public GameObject num0;
    public GameObject num1;
    public GameObject num2;
    public GameObject num3;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        int score = GameManager.instance.score;
        if (score > 10000)
        {
            score = 9999;
        }
        int digit0 = score / 1000; score %= 1000;
        int digit1 = score / 100; score %= 100;
        int digit2 = score / 10; score %= 10;
        int digit3 = score / 1;
        num0.GetComponent<SpriteRenderer>().sprite = numbers[digit0];
        num1.GetComponent<SpriteRenderer>().sprite = numbers[digit1];
        num2.GetComponent<SpriteRenderer>().sprite = numbers[digit2];
        num3.GetComponent<SpriteRenderer>().sprite = numbers[digit3];
    }
}
