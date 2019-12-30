using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class textVisible : MonoBehaviour
{
    public TextMesh visibleText;

    // Start is called before the first frame update
    void Start()
    {
        visibleText.gameObject.SetActive(false);   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player") == true)
        {
            visibleText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Player") == true)
        {
            visibleText.gameObject.SetActive(false);
        }
    }
}
