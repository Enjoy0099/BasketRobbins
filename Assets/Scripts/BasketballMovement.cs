using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketballMovement : MonoBehaviour
{
    public Transform ballTransform;
    // Start is called before the first frame update

    private void Awake()
    {
        ballTransform = GetComponent<Transform>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKey(KeyCode.W)) {
        //    Vector3 motion = ballTransform.position;
        //    motion.x += (1 * Time.deltaTime);
        //    ballTransform.position = motion;
        //}

        //if (Input.GetKey(KeyCode.S))
        //{
        //    Vector3 motion = ballTransform.position;
        //    motion.x -= (1 * Time.deltaTime);
        //    ballTransform.position = motion;
        //}

        //if (Input.GetKey(KeyCode.A))
        //{
        //    Vector3 motion = ballTransform.position;
        //    motion.z -= (1 * Time.deltaTime);
        //    ballTransform.position = motion;
        //}

        //if (Input.GetKey(KeyCode.D))
        //{
        //    Vector3 motion = ballTransform.position;
        //    motion.z += (1 * Time.deltaTime);
        //    ballTransform.position = motion;
        //}

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        ballTransform.position += new Vector3(x, 0f, y) * Time.deltaTime * 10;
    }
}
