using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetScaleToVar : MonoBehaviour
{
    public NFDConsumer component;
    public float scaleFactor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = new Vector3(1f, component.m_supress * scaleFactor, 1f);
    }
}
