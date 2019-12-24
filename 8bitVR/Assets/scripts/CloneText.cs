using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneText : MonoBehaviour
{
    public TextMesh masterText;
    public TextMesh screenText;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        screenText.text = masterText.text;
    }
}
