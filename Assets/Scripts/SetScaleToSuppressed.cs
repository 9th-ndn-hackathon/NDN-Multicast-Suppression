using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetScaleToSuppressed : MonoBehaviour
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
        transform.localScale = new Vector3(1f, component.interestsSuppressed * scaleFactor, 1f);
    }
}
