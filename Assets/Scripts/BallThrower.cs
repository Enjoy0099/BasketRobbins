using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BallThrower : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Rigidbody ballBody;
    [SerializeField] private float minForce = 5f;
    [SerializeField] private float maxForce = 10f;

    private Vector2 startDrag;
    private Vector2 endDrag;

    private void Awake()
    {
        ballBody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        
    }
    // Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetKey(KeyCode.Space))
    //    {
    //        currentForce += minForce * Time.deltaTime;
    //        currentForce = Mathf.Clamp(currentForce, minForce, maxForce);
    //    }

    //    if (Input.GetKeyUp(KeyCode.Space))
    //    {
    //        BallThrow();
    //        //currentForce = minForce;
    //    }
    //}
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startDrag = Input.mousePosition;
        }

        // Equivalent to OnDrag
        if (Input.GetMouseButton(0))
        {
            endDrag = Input.mousePosition;
        }

        // Equivalent to OnPointerUp
        if (Input.GetMouseButtonUp(0))
        {
            endDrag = Input.mousePosition;
            BallThrow();
        }
    }

    void BallThrow()
    {
        Vector2 dragVector = endDrag - startDrag;

        float force = Mathf.Clamp(dragVector.magnitude * 0.5f,minForce,maxForce);

        Debug.Log("Force = " + force);

        Vector3 throwDirection = transform.forward + transform.up * 0.5f;
        ballBody.AddForce(force * throwDirection, ForceMode.Impulse);
    }
}
