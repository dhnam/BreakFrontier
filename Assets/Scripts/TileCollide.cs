using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


class Fronteer
{
    public List<Vector3Int> fronteer = new List<Vector3Int>();
    public List<int> timer = new List<int>();
    public int Count
    {
        get => timer.Count;
    }
    public void Remove(Vector3Int value)
    {
        int index = fronteer.FindIndex(x => x == value);
        if (index < 0)
        {
            return;
        }
        fronteer.RemoveAt(index);
        timer.RemoveAt(index);

    }

    public void Add(Vector3Int value)
    {
        fronteer.Add(value);
        timer.Add(0);
    }

    public void Update()
    {
        for (int i = 0; i < timer.Count; i++)
        {
            timer[i]++;
        }
    }

    public bool checkRegenBorder(int index, int regenBorder)
    {
        return timer[index] == regenBorder;
    }

    public bool Contains(Vector3Int value)
    {
        return fronteer.Contains(value);
    }

    public void RemoveAt(int index)
    {
        fronteer.RemoveAt(index);
        timer.RemoveAt(index);
    }

    public Vector3Int At(int index)
    {
        try
        {
            return fronteer[index];
        }
        catch (System.IndexOutOfRangeException e)
        {
            throw e;
        }
    }

    public int TimeAt(int index)
    {
        try
        {
            return timer[index];
        }
        catch (System.IndexOutOfRangeException e)
        {
            throw e;
        }
    }
    public void TimeResetAt(int index)
    {
        try
        {
            timer[index] = 0;
        }
        catch (System.IndexOutOfRangeException e)
        {
            throw e;
        }
    }
}
public class TileCollide : MonoBehaviour
{
    public TileBase block;
    public TileBase undiscovered;
    public TileBase border;
    public TileBase ball_add;
    public TileBase danger;
    public TileBase danger_ball;

    List<Vector3Int> discovered = new List<Vector3Int>();
    List<Vector3Int> add_list = new List<Vector3Int>();
    List<Vector3Int> gameover_border = new List<Vector3Int>();
    List<GameObject> gameover_border_obj = new List<GameObject>();

    public GameObject ball;
    public GameObject redline;
    public GameObject progressBarEmpty;
    public GameObject progressBarFilled;

    public int bullettimeFrame = 10;
    public int fronteerRegenBorder = 500;
    public int firstBallFrame = 10;

    Fronteer fronteer = new Fronteer();
    public float bullettimeSpeed = 0.05f;
    Tilemap tilemap;
    float fixedDeltaTime;
    int ball_count = 0;
    int gameOverRadiusCurr;

    public int initialRadius = 3;
    public int gameOverRadius = 1;
    public int gameOverRadiusMax = 4;
    public float ballAddProbability = 0.08f;

    public AudioClip ballAdd;
    public AudioClip ballAddSlow;
    public AudioClip blockBreak;
    public AudioClip blockBreakSlow;
    AudioSource source;
    enum SoundType
    {
        ballAdd, blockBreak
    }


    // Start is called before the first frame update
    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        source = GetComponent<AudioSource>();
        fixedDeltaTime = Time.fixedDeltaTime;
        Init();
    }

    void Init()
    {
        for (int i = -initialRadius; i <= initialRadius; i++)
        {
            for (int j = -initialRadius + Mathf.Abs(i); j <= initialRadius - Mathf.Abs(i); j++)
            {
                Vector3Int cell = new Vector3Int(i, j, 0);
                if (j == -initialRadius + Mathf.Abs(i) || j == initialRadius - Mathf.Abs(i))
                {
                    fronteer.Add(cell);
                    tilemap.SetTile(cell, block);
                }
                else
                {
                    discovered.Add(cell);
                    tilemap.SetTile(cell, null);

                }
            }
        }
        gameOverRadiusCurr = gameOverRadius;
        SetGameoverBorder();

        Invoke(nameof(MakeBall), firstBallFrame * fixedDeltaTime);
    }

    private void Restart()
    {
        ball_count = 0;
        discovered = new List<Vector3Int>();
        add_list = new List<Vector3Int>();
        for (int i = 0; i < gameover_border_obj.Count; i++)
        {
            Destroy(gameover_border_obj[i]);
        }
        gameover_border = new List<Vector3Int>();
        gameover_border_obj = new List<GameObject>();
        fronteer = new Fronteer();
        tilemap = GetComponent<Tilemap>();
        afterBullettime = 0;
        BoundsInt bound = tilemap.cellBounds;
        for (int i = bound.xMin; i < bound.xMax; i++)
        {
            for (int j = bound.yMin; j < bound.yMax; j++)
            {
                Vector3Int cell = new Vector3Int(i, j, 0);
                if (tilemap.GetTile(cell) != border)
                {
                    tilemap.SetTile(cell, null);
                }
            }
        }
        tilemap.FloodFill(new Vector3Int(0, 0, 0), undiscovered);
        Init();
    }

    void SetGameoverBorder()
    {
        int radius = gameOverRadiusCurr;
        for (int i = 0; i < gameover_border_obj.Count; i++)
        {
            Destroy(gameover_border_obj[i]);
        }
        gameover_border_obj.Clear();
        gameover_border.Clear();
        for (int i = -radius; i <= radius; i++)
        {
            int border = -radius + Mathf.Abs(i);
            if (border == 0)
            {
                GameObject border_obj = Instantiate(redline, tilemap.CellToWorld(new Vector3Int(i, 0, 0)), Quaternion.identity);
                gameover_border.Add(new Vector3Int(i, 0, 0));
                gameover_border_obj.Add(border_obj);
            }
            else
            {
                GameObject border_obj1 = Instantiate(redline, tilemap.CellToWorld(new Vector3Int(i, border, 0)), Quaternion.identity);
                gameover_border.Add(new Vector3Int(i, border, 0));
                GameObject border_obj2 = Instantiate(redline, tilemap.CellToWorld(new Vector3Int(i, -border, 0)), Quaternion.identity);
                gameover_border.Add(new Vector3Int(i, -border, 0));
                gameover_border_obj.Add(border_obj1);
                gameover_border_obj.Add(border_obj2);
            }
        }
        for (int i = 0; i < gameover_border.Count; i++)
        {
            Vector3Int cell = gameover_border[i];
            Vector3Int left = new Vector3Int(cell.x - 1, cell.y, cell.z);
            Vector3Int right = new Vector3Int(cell.x + 1, cell.y, cell.z);
            Vector3Int up = new Vector3Int(cell.x, cell.y + 1, cell.z);
            Vector3Int down = new Vector3Int(cell.x, cell.y - 1, cell.z);
            BreakBlock(cell, playSound:false);
            BreakBlock(left, playSound: false);
            BreakBlock(right, playSound: false);
            BreakBlock(up, playSound: false);
            BreakBlock(down, playSound: false);
        }
    }
    void MakeBall()
    {
        Instantiate(ball, new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f)), Quaternion.identity);
        ball_count++;
        if (ball_count == 3)
        {
            ball_count = 0;
            gameOverRadiusCurr++;
            if (gameOverRadiusCurr > gameOverRadiusMax)
            {
                gameOverRadiusCurr = gameOverRadiusMax;
            }
            SetGameoverBorder();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!IsInvoking(nameof(FrameUpdate)))
        {
            Invoke(nameof(FrameUpdate), GameManager.instance.frameDelta);
        }
        ProgressBar();
    }


    void ProgressBar()
    {
        if (Time.timeScale == 1.0f || Time.timeScale == 0.0f)
        {
            //progressBarEmpty.SetActive(false);
            progressBarFilled.SetActive(false);
        }
        else
        {
            //progressBarEmpty.SetActive(true);
            progressBarFilled.SetActive(true);
            progressBarFilled.transform.localScale = new Vector3(1, (float)afterBullettime / bullettimeFrame);
        }
    }

    int afterBullettime;
    private void FrameUpdate()
    {
        fronteer.Update();
        int regenBorder = fronteerRegenBorder - 50 * (gameOverRadiusCurr - gameOverRadius);
        //process regenerate
        for (int i = fronteer.Count - 1; i > -1; i--)
        {
            Vector3Int cell;
            try
            {
                cell = fronteer.At(i);
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                break;
            }
            if (fronteer.checkRegenBorder(i, regenBorder))
            {
                BreakBlock(cell, true);
                if (tilemap.GetTile(cell) == danger_ball)
                {
                    tilemap.SetTile(cell, ball_add);
                }
                else if (tilemap.GetTile(cell) == danger)
                {
                    tilemap.SetTile(cell, block);
                }

            }
            if (fronteer.checkRegenBorder(i, regenBorder * 8 / 10))
            {
                if (tilemap.GetTile(cell) == ball_add)
                {
                    tilemap.SetTile(cell, danger_ball);
                }
                else if (tilemap.GetTile(cell) == block)
                {
                    tilemap.SetTile(cell, danger);
                }
            }
        }

        //remove inclosed from fronteer
        for (int i = fronteer.Count - 1; i > -1; i--)
        {
            Vector3Int cell;
            try
            {
                cell = fronteer.At(i);
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                break;
            }
            Vector3Int left = new Vector3Int(cell.x - 1, cell.y, cell.z);
            Vector3Int right = new Vector3Int(cell.x + 1, cell.y, cell.z);
            Vector3Int up = new Vector3Int(cell.x, cell.y + 1, cell.z);
            Vector3Int down = new Vector3Int(cell.x, cell.y - 1, cell.z);
            if (!(tilemap.GetTile(left) == null || tilemap.GetTile(right) == null || tilemap.GetTile(up) == null || tilemap.GetTile(down) == null))
            {
                if (tilemap.GetTile(cell) == danger)
                {
                    tilemap.SetTile(cell, block);
                }
                if (tilemap.GetTile(cell) == danger_ball)
                {
                    tilemap.SetTile(cell, ball_add);
                }
                fronteer.RemoveAt(i);
            }
        }
        if (Time.timeScale == bullettimeSpeed)
        {
            afterBullettime++;
        }
        if (afterBullettime > bullettimeFrame)
        {
            afterBullettime = 0;
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = fixedDeltaTime * bullettimeSpeed;
            GameManager.instance.frameDelta /= 1 + bullettimeSpeed;
        }
    }

    void PlaySound(SoundType type)
    {
        /*
         * if (source.isPlaying && type == SoundType.blockBreak)
        {
            return;
        }
        */
        if (Time.timeScale == 1.0f) //normal speed
        {
            if (type == SoundType.ballAdd)
            {
                source.PlayOneShot(ballAdd);
            }
            else if (type == SoundType.blockBreak)
            {
                source.PlayOneShot(blockBreak);
            }
        }
        else
        {
            if (type == SoundType.ballAdd)
            {
                source.PlayOneShot(ballAddSlow);
            }
            else if (type == SoundType.blockBreak)
            {
                source.PlayOneShot(blockBreakSlow);
            }
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        Grid grid = tilemap.layoutGrid;
        List<ContactPoint2D> contactList = new List<ContactPoint2D>();
        List<Vector3Int> processed = new List<Vector3Int>();
        collision.GetContacts(contactList);
        foreach (ContactPoint2D contactPoint in contactList)
        {
            Vector3Int cell = grid.WorldToCell(contactPoint.point);
            if (cell == new Vector3Int(0, 0, 0))
            {
                continue;
            }
            if (tilemap.GetTile(cell) == null)
            {
                continue;
            } 
            if (processed.Contains(cell))
            {
                continue;
            }
            Debug.Log(cell);
            Debug.Log(Vector3.Distance(contactPoint.point, collision.collider.transform.position));
            if (Mathf.Round(Vector3.Distance(contactPoint.point, collision.collider.transform.position)) > 1)
            {
                continue;
            }
            processed.Add(cell);
            BreakBlock(cell);
            GameManager.instance.score += 5 * (gameOverRadiusCurr - gameOverRadius + 1);
        }
    }

    void BreakBlock(Vector3Int cell, bool regen=false, bool playSound=true)
    {
        if (tilemap.GetTile(cell) != border || regen)
        {
            if (!regen)
            {
                tilemap.SetTile(cell, null);
            }
            if (!discovered.Contains(cell))
            {
                discovered.Add(cell);
            }
            fronteer.Remove(cell);
            if (add_list.Contains(cell) && !regen)
            {
                if (playSound)
                {
                    PlaySound(SoundType.ballAdd);
                }
                add_list.Remove(cell);
                MakeBall();
                Time.timeScale = bullettimeSpeed;
                Time.fixedDeltaTime = fixedDeltaTime * bullettimeSpeed;
                GameManager.instance.frameDelta *= 1 + bullettimeSpeed;
                afterBullettime = 0;
            }
            else if (!regen)
            {
                if (playSound)
                {
                    PlaySound(SoundType.blockBreak);
                }
            }
            DiscoverNearby(cell, regen);
        }
        else
        {
            if (!fronteer.Contains(cell))
            {
                fronteer.Remove(cell);
            }
            fronteer.Add(cell);
        }
    }

    private void DiscoverNearby(Vector3Int cell, bool regen = false)
    {
        Vector3Int left = new Vector3Int(cell.x - 1, cell.y, cell.z);
        Vector3Int right = new Vector3Int(cell.x + 1, cell.y, cell.z);
        Vector3Int up = new Vector3Int(cell.x, cell.y + 1, cell.z);
        Vector3Int down = new Vector3Int(cell.x, cell.y - 1, cell.z);
        if (IsNewTile(left, regen))
        {
            NewTile(left, regen);
        }
        if (IsNewTile(right, regen))
        {
            NewTile(right, regen);
        }
        if (IsNewTile(up, regen))
        {
            NewTile(up, regen);
        }
        if (IsNewTile(down, regen))
        {
            NewTile(down, regen);
        }
    }

    private bool IsNewTile(Vector3Int cell, bool regen)
    {
        if (regen)
        {
            return !fronteer.Contains(cell) && tilemap.GetTile(cell) != border;
        }
        return !fronteer.Contains(cell) && tilemap.GetTile(cell) != border && tilemap.GetTile(cell) != null;
    }

    private void NewTile(Vector3Int cell, bool regen)
    {
        if (gameover_border.Contains(cell))
        {
            GameManager.instance.isGameOver = true;
        }
        if (!regen)
        {
            if (Random.Range(0f, 1f) < ballAddProbability && tilemap.GetTile(cell) == undiscovered)
            {
                tilemap.SetTile(cell, ball_add);
                add_list.Add(cell);
            }
            else if (tilemap.GetTile(cell) == ball_add)
            {

            }
            else
            {
                tilemap.SetTile(cell, block);
            }
        }
        else
        {
            if (tilemap.GetTile(cell) == null)
            {
                tilemap.SetTile(cell, block);
            }
        }
        fronteer.Add(cell);
    }

    private void OnGUI()
    {
        /*for (int i = 0; i < fronteer.Count; i++)
        {
            Vector3 pos = Camera.main.WorldToScreenPoint(tilemap.GetCellCenterWorld(fronteer.At(i)));
            Rect rect = new Rect(pos.x, Screen.height - pos.y, 150, 150);
            GUI.Label(rect, fronteer.TimeAt(i).ToString());
        }*/
    }

}