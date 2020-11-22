using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(position.x, position.y);
    }
}
