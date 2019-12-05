using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class embeddedBridge : MonoBehaviour
{
    public SerialController serialController;
    public GameObject moveObject;
    public GameObject xObject;
    public GameObject yObject;
    public GameObject zObject;
    public Light LED0;
    public Light roomTemp;
    public Light roomHot;
    public Light roomCool;
    public Text xText;
    public Text yText;
    public Text zText;
    public Text tempText;

    private bool vrButtonPressed;
    private bool buttonMessageDone;

    private const float xOstart = -1.5f;
    private const float zOstart = -0.25f;

    private const float xYOstart = 2.0f;
    private const float yYstart = 1.5f;
    private const float zYstart = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        LED0.enabled = false;
        vrButtonPressed = false;
        buttonMessageDone = false;
        xText.text = "";
        yText.text = "";
        zText.text = "";
        tempText.text = "";
        roomTemp.enabled = true;
        roomHot.enabled = false;
        roomCool.enabled = false;
        moveObject.transform.position = new Vector3(0.0f, 3.0f, 0.0f);
        xObject.transform.position = new Vector3(xOstart, xYOstart, zOstart);
        yObject.transform.position = new Vector3(xOstart, yYstart, zOstart);
        zObject.transform.position = new Vector3(xOstart, zYstart, zOstart);
    }

    // Update is called once per frame
    void Update()
    {
        //---------------------------------------------------------------------
        // Send data
        //---------------------------------------------------------------------
        if (buttonMessageDone)
        {
            if (vrButtonPressed == true)
            {
                Debug.Log("Sent led0 on");
            }
            if (vrButtonPressed == false)
            {
                Debug.Log("Sent led0 off");
            }
            buttonMessageDone = false;
        }
    }

    public bool getButtonState()
    {
        return buttonMessageDone;
    }

    public void setButtonState(bool state)
    {
        buttonMessageDone = state;
    }
    public bool getVRButtonState()
    {
        return vrButtonPressed;
    }
    public bool processLegacyFormat(string message)
    {
        bool validMessage = false;
        string[] splitMessage = message.Split(' ');
        /*
        int counter = 0;
        foreach (string msg in splitMessage)
        {
            Debug.Log(counter++);
            Debug.Log(msg);
        }
        */
        // Push Button Actions
        if (splitMessage[11] == "P")
        {
            if (splitMessage[12] == "0")
            {
                LED0.enabled = true;
            }
            else if (splitMessage[12] == "1")
            {
                LED0.enabled = false;
            }
            else
            {
                LED0.enabled = false;
            }
            validMessage = true;
        }
        else
        {
            validMessage = false;
        }

        // LED Actions
        if (splitMessage[13] == "L")
        {
            if (splitMessage[14] == "0")
            {
                //LED0.enabled = true;
            }
            else if (splitMessage[1] == "1")
            {
                //LED0.enabled = false;
            }
            else
            {
                //LED0.enabled = false;
            }
            validMessage = true;
        }
        else
        {
            validMessage = false;
        }
        // Temperature Sensor
        if (splitMessage[0] == "T")
        {
            tempText.text = splitMessage[1].Replace("+", "");

            tempText.text += splitMessage[2] + "." + splitMessage[3];

            int tempValue = (int.Parse(splitMessage[2]));

            if (tempValue >= 29)
            {
                roomTemp.enabled = false;
                roomHot.enabled = true;
                roomCool.enabled = false;
            }
            else if (tempValue <= 25)
            {
                roomTemp.enabled = false;
                roomHot.enabled = false;
                roomCool.enabled = true;
            }
            else
            {
                roomTemp.enabled = true;
                roomHot.enabled = false;
                roomCool.enabled = false;
            }
            /*
            string tempString = "";
            tempString = tempText.text.ToString();
            int tempValue = int.Parse(tempString);
            */
            validMessage = true;
        }
        else
        {
            validMessage = false;
        }

        // Accelerometer
        if (splitMessage[4] == "A")
        {
            xText.text = splitMessage[6];
            yText.text = splitMessage[8];
            zText.text = splitMessage[10];

            string xString = "";
            string yString = "";
            string zString = "";

            xString = xText.text.ToString();
            yString = yText.text.ToString();
            zString = zText.text.ToString();

            float xValue = (float.Parse(xString) / 4000) + 0.0f;
            float yValue = (float.Parse(yString) / 4000) + 3.0f;
            float zValue = (float.Parse(zString) / 4000) + 0.0f;

            float xAdjust = (float.Parse(xString) / 4000) + zOstart;
            float yAdjust = (float.Parse(yString) / 4000) + zOstart;
            float zAdjust = (float.Parse(zString) / 4000) + zOstart;

            moveObject.transform.position = new Vector3(xValue, yValue, zValue);

            xObject.transform.position = new Vector3(xOstart, xYOstart, xAdjust);
            yObject.transform.position = new Vector3(xOstart, yYstart, yAdjust);
            zObject.transform.position = new Vector3(xOstart, zYstart, zAdjust);

            //Debug.Log(moveObject.transform.position);
            validMessage = true;
        }
        else
        {
            validMessage = false;
        }
        return validMessage;
    }

    public bool processMessageData(string message)
    {
        string[] splitMessage = message.Split(' ');
        Debug.Log(message);
        // Prototype Data Message Format
        if ((splitMessage[0] == "T") && (splitMessage[1] == "+")
                || (splitMessage[0] == "T") && (splitMessage[1] == "-"))
        {
            return processLegacyFormat(message);
        }
        else if (splitMessage[0] == "D:")
        {
            // Parse new data format
        }
        else
        {
            if (message.Length < 32)
            {
                // Echo String to Terminal
                serialController.SendSerialMessage(message);
                return true;
            }
        }
        return false;
    }
}
