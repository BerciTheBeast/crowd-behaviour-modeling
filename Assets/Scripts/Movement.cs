using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Movement : MonoBehaviour
{

    public float speed;
    public GameObject destination;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        this.Move();
    }

    public void Move()
    {
        gameObject.transform.LookAt(this.destination.transform.position);
        gameObject.transform.position += gameObject.transform.forward * speed;
    }
}
