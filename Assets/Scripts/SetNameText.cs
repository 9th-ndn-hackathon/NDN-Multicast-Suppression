using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetNameText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NFDConsumer nodeName = GetComponentInParent<NFDConsumer>();
        GetComponent<TMPro.TextMeshProUGUI>().text = nodeName.name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
