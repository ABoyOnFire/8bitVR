using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneLight : MonoBehaviour
{
    public Light trackedLight;
    public Light trackingLight;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        trackingLight.enabled = trackedLight.enabled;
    }
}
