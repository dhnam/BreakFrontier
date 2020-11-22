using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BallMove : MonoBehaviour
{

    public float speed = 10.0f;
    public int afterCollideBorder = 100;
    public int afterNotclickedBorder = 200;
    Rigidbody2D body;
    SpriteRenderer spriteRenderer;
    public Sprite ready;
    public Sprite moving;
    public Sprite warning;
    public GameObject line;
    GameObject lineCurr;

    public AudioClip breakAudio;
    public AudioClip breakSlow;
    public AudioClip shoot;
    public AudioClip shootSlow;
    AudioSource source;

    enum SoundType
    {
        breaking, shoot
    }

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        source = GetComponent<AudioSource>();
    }

    bool clicked = false;
    bool collided = false;
    int afterCollideFrame = 0;
    int afterNotClickedFrame = 0;

    private void OnEnable()
    {
        GameManager.instance.ballList.Add(gameObject);
        clicked = false;
        collided = false;
        afterCollideFrame = 0;
        afterNotClickedFrame = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (clicked)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos = new Vector3(mousePos.x, mousePos.y, -0.1f);
            DrawLine(new Vector3(transform.position.x - 0.5f, transform.position.y + 0.5f, 0.1f), mousePos);
            //lineRenderer.SetPosition(1, mousePos);
            //lineRenderer.SetPosition(0, new Vector3(transform.position.x - 0.5f, transform.position.y + 0.5f, 0.1f));
        }
        if (!IsInvoking(nameof(FrameUpdate)))
        {
            Invoke(nameof(FrameUpdate), GameManager.instance.frameDelta);
        }
    }

    void PlaySound(SoundType type)
    {
        if (Time.timeScale == 1.0f) //normal speed
        {
            if (type == SoundType.breaking)
            {
                AudioSource.PlayClipAtPoint(breakAudio, Camera.main.transform.position);
            }
            else if (type == SoundType.shoot)
            {
                source.PlayOneShot(shoot);
            }
        }
        else
        {
            if (type == SoundType.breaking)
            {
                AudioSource.PlayClipAtPoint(breakSlow, Camera.main.transform.position);
            }
            else if (type == SoundType.shoot)
            {
                source.PlayOneShot(shootSlow);
            }
        }
    }

    void FrameUpdate()
    {
        if (collided)
        {
            afterCollideFrame += 1;
            if (afterCollideFrame >= afterCollideBorder && spriteRenderer.sprite == moving)
            {
                collided = false;
                body.velocity = new Vector2(0, 0);
                transform.position = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
                afterCollideFrame = 0;
                afterNotClickedFrame = 0;
                spriteRenderer.sprite = ready;
            }
        }
        if (spriteRenderer.sprite != moving)
        {
            afterNotClickedFrame++;
        }
        if (afterNotClickedFrame == afterNotclickedBorder * 6 / 10)
        {
            spriteRenderer.sprite = warning;
        }
        if (afterNotClickedFrame > afterNotclickedBorder)
        {
            PlaySound(SoundType.breaking);
            GameManager.instance.ballList.Remove(gameObject);
            if (lineCurr != null)
            {
                EraseLine();
            }  
            Destroy(gameObject);
        }
    }

    void OnMouseDown()
    {
        if (clicked == false)
        {
            clicked = true;

        }
        if (spriteRenderer.sprite != moving)
        {
            //lineRenderer.positionCount = 2;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos = new Vector3(mousePos.x, mousePos.y, -0.1f);
            DrawLine(new Vector3(transform.position.x - 0.5f, transform.position.y + 0.5f, 0.1f), mousePos);
            //lineRenderer.SetPosition(0, new Vector3(transform.position.x - 0.5f, transform.position.y + 0.5f, 0.1f));
            //lineRenderer.SetPosition(1, mousePos);
        }
    }

    void OnMouseUp()
    {
        if (clicked)
        {
            clicked = false;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos = new Vector3(mousePos.x, mousePos.y, 0);
            Vector3 direction = Vector3.Normalize(transform.position - mousePos);
            body.velocity = new Vector2(direction.x * speed, direction.y * speed);
            spriteRenderer.sprite = moving;
            afterNotClickedFrame = 0;
            //lineRenderer.positionCount = 0;
            EraseLine();
            PlaySound(SoundType.shoot);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.name == "Tilemap" && spriteRenderer.sprite == moving)
        {
            collided = true;
        }
    }

    void DrawLine(Vector3 from, Vector3 to)
    {
        float dist = Vector3.Distance(from, to);
        float angle = Vector2.SignedAngle(new Vector2(1, 0), new Vector2(from.x - to.x, from.y - to.y));
        Quaternion direction = Quaternion.Euler(0, 0, angle);
        if (lineCurr == null)
        {
            lineCurr = Instantiate(line, from, direction);
        }
        else
        {
            lineCurr.transform.eulerAngles = new Vector3(0, 0, angle);
            lineCurr.transform.position = from;
        }
        Vector3 scale = lineCurr.transform.localScale;
        scale.x = dist;
        lineCurr.transform.localScale = scale;
    }


    void EraseLine()
    {
        Destroy(lineCurr);
    }
}
