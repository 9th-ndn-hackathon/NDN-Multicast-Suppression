
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleGrowth : MonoBehaviour
{
    private int segments = 50;
    private float radius = 0;
    private float maxRadius = 100000;
    private float growthRate;
    private float lifeTime;
    
    public LineRenderer line;

    private bool started = false;

    public void setParameters(float growthRate, float lifeTime) 
    {
        this.growthRate = growthRate;
        this.lifeTime = lifeTime;
       
        line.useWorldSpace = false;
        line.SetVertexCount(segments + 1);
        line.startWidth = 1.0f;
        line.endWidth = 1.0f;
    }

    public void startGrowth() {
        StartCoroutine(LifetimeCountdownRoutine());
        started = true;
    }

    public void Update() 
    {
        if (!started) return;
        if (radius < maxRadius)
        {
            radius += (growthRate * Time.deltaTime) ;
            CreatePoints();
        }
    }

    IEnumerator LifetimeCountdownRoutine()
    {
        yield return new WaitForSeconds(lifeTime);
        Object.Destroy(this.gameObject);
    }
   
    void CreatePoints ()
    {
        float x;
        float y;
        float z = 0f;
       
        float angle = 0f;
       
        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin (Mathf.Deg2Rad * angle);
                    y = Mathf.Cos (Mathf.Deg2Rad * angle);
                    line.SetPosition (i,new Vector3(x,y,z) * radius);
                    angle += (360f / segments);
        }
    }
}
