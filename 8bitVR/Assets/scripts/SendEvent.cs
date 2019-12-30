using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendEvent : MonoBehaviour
{
    public EmbeddedBridge embeddedData;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("DataLed") == true)
        {
            embeddedData.SendVrMessage("1");
        }
        else if (other.tag.Equals("ErrorLed") == true)
        {
            embeddedData.SendVrMessage("4");
        }
        else
        {
            //
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("DataLed") == true)
        {
            embeddedData.SendVrMessage("0");
        }
        else if (other.tag.Equals("ErrorLed") == true)
        {
            embeddedData.SendVrMessage("3");
        }
        else
        {
            //
        }
    }
}
