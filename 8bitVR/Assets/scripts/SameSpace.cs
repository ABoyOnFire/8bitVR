using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SameSpace : MonoBehaviour
{
    public UserPolling_VR switchControl;

    private bool switchActive;

    // Start is called before the first frame update
    void Start()
    {
        switchActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "switch")
        {
            if (switchActive == false)
            {
                switchActive = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "switch")
        {
            switchActive = false;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
