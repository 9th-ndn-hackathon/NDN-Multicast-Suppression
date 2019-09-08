
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

    private float radiusGrowthPerGrowthInterval;
    private float growthInterval;
    
    public LineRenderer line;

    public void setParameters(Transform parentTransform, float growthRate, float lifeTime) 
    {
        this.gameObject.transform.SetParent(parentTransform, false);

        this.growthRate = growthRate;
        this.lifeTime = lifeTime;
       
        line.useWorldSpace = false;
        line.SetVertexCount(segments + 1);

        // TODO: calculate radiusGrowthPerGrowthInterval from growthRate
        // TODO: calculate growthInteral from growthRate
        radiusGrowthPerGrowthInterval = .5f;
        growthInterval = 1.0f / growthRate / Time.timeScale;
    
    }

    public void startGrowth() {
        StartCoroutine(GrowthRoutine());
        StartCoroutine(LifetimeCountdownRoutine());
    }

    IEnumerator GrowthRoutine()
    {
        CreatePoints();

        while (radius < maxRadius)
        {
            radius += radiusGrowthPerGrowthInterval;
            CreatePoints();
            yield return new WaitForSeconds(growthInterval);
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

