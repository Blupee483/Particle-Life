using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    private Camera myCam;
    [SerializeField] private float speed = 0.8f;
    private Vector2 movement;
    // Start is called before the first frame update
    void Start()
    {
        myCam = gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        movement = Vector2.zero;
        if(Input.GetKey(KeyCode.W)) movement += Vector2.up;
        if(Input.GetKey(KeyCode.A)) movement += Vector2.left;
        if(Input.GetKey(KeyCode.S)) movement += Vector2.down;
        if(Input.GetKey(KeyCode.D)) movement += Vector2.right;

        transform.position += (Vector3)movement * speed * Time.deltaTime;

        if(Input.GetKeyDown(KeyCode.Z)) myCam.orthographicSize *= 1.05f;
        else if(Input.GetKeyDown(KeyCode.X)) myCam.orthographicSize *= 0.95f;
    }
}
