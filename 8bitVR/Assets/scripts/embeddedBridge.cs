using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private const float xOstart =   (-1.5f);
    private const float zOstart =   (-0.25f);

    private const float xYOstart =  (2.0f);
    private const float yYstart =   (1.5f);
    private const float zYstart =   (1.0f);

    private float [] defaultMovePosition = { 0.0f, 3.0f, 0.0f };
    private SerialController upstreamSerialBus = null;

    // Start is called before the first frame update
    void Start()
    {
        LED0.enabled = false;
        xText.text = "";
        yText.text = "";
        zText.text = "";
        tempText.text = "";
        roomTemp.enabled = true;
        roomHot.enabled = false;
        roomCool.enabled = false;
        moveObject.transform.position = new Vector3(defaultMovePosition[0], defaultMovePosition[1], defaultMovePosition[2]);
        xObject.transform.position = new Vector3(xOstart, xYOstart, zOstart);
        yObject.transform.position = new Vector3(xOstart, yYstart, zOstart);
        zObject.transform.position = new Vector3(xOstart, zYstart, zOstart);
    }

    void UpstreamSerialCommunication(SerialController upstreamBus)
    {
        if ((upstreamSerialBus == null) && (upstreamBus != null))
        {
            upstreamSerialBus = upstreamBus;
        }
    }

    private bool isResetting = false;
    // Update is called once per frame
    void Update()
    {
        // Keyboard LED Test
        if (Input.GetKeyDown(KeyCode.Q))
        {
            upstreamSerialBus.SendSerialMessage("led0 on");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            upstreamSerialBus.SendSerialMessage("led0 off");
        }
        // Keyboard Actions
        if (Input.GetKeyDown(KeyCode.F12) && (isResetting == false))
        {
            isResetting = true;
            SceneManager.LoadScene("ces2020");
            isResetting = false;
        }
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
        else if ((splitMessage[0] == "{") && (splitMessage[1] != "?"))
        {
            // Parse new data format
            return processJSONFormat(message);
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

    private bool processJSONFormat(string message)
    {
        bool validMessage = false;
        var map = new Dictionary<string, string>();
        string[] splitMessage = message.Split(',');

        // How many 'pairs' of data are there
        foreach (string split in splitMessage)
        {
            // Get the Key | Value 'Pair'
            string[] splitMap = split.Split(':');
            if (splitMap.Length > 2)
            {
                // Error; there should only be (2) values in every splitMap instance
                return false;
            }
            else
            {
                map.Add(splitMap[0], splitMap[1]);
            }
        }

        foreach (KeyValuePair<string, string> pair in map)
        {
            // For all Unless ELSE is hit
            validMessage = true;

            if (pair.Key.Equals("T") == true)
            {   // Temperature
                tempText.text = pair.Value.Replace("+", "");
                int tempValue = (int.Parse(tempText.text));
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
            }
            else if (pair.Key.Equals("P") == true)
            {   // Push Button
                if (pair.Value.Equals("0"))
                {
                    LED0.enabled = true;
                }
                else if (pair.Value.Equals("1"))
                {
                    LED0.enabled = false;
                }
                else
                {   // Error Condition
                    LED0.enabled = false;
                }
            }
            else if (pair.Key.Equals("L") == true)
            {   // LEDs
                if (pair.Value.Equals("0"))
                {
                    // LED0.enabled = true;
                }
                else if (pair.Value.Equals("1"))
                {
                    // LED0.enabled = false;
                }
                else
                {
                    // LED0.enabled = false;
                }
            }
            else if (pair.Key.Equals("X") == true)
            {   // X Accelerometer
                xText.text = splitMessage[6];
                string xString = "";
                xString = xText.text.ToString();
                float xValue = (float.Parse(xString) / 4000) + defaultMovePosition[0];
                float xAdjust = (float.Parse(xString) / 4000) + zOstart;

                moveObject.transform.position = new Vector3 (xValue, transform.position.y, transform.position.z);
                xObject.transform.position = new Vector3(xOstart, xYOstart, xAdjust);
            }
            else if (pair.Key.Equals("Y") == true)
            {   // Y Accelerometer
                yText.text = splitMessage[8];
                string yString = "";
                yString = yText.text.ToString();
                float yValue = (float.Parse(yString) / 4000) + defaultMovePosition[1];
                float yAdjust = (float.Parse(yString) / 4000) + zOstart;

                moveObject.transform.position = new Vector3(transform.position.x, yValue, transform.position.z);
                xObject.transform.position = new Vector3(xOstart, yAdjust, zOstart);
            }
            else if (pair.Key.Equals("Z") == true)
            {   // Z Accelerometer
                zText.text = splitMessage[10];
                string zString = "";
                zString = zText.text.ToString();
                float zValue = (float.Parse(zString) / 4000) + defaultMovePosition[2];
                float zAdjust = (float.Parse(zString) / 4000) + zOstart;

                moveObject.transform.position = new Vector3(transform.position.x, transform.position.y, zValue);
                xObject.transform.position = new Vector3(xOstart, xYOstart, zAdjust);
            }
            else
            {
                validMessage = false;
            }
        }
        return validMessage;
    }
    private bool processLegacyFormat(string message)
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

            float xValue = (float.Parse(xString) / 4000) + defaultMovePosition[0];
            float yValue = (float.Parse(yString) / 4000) + defaultMovePosition[1];
            float zValue = (float.Parse(zString) / 4000) + defaultMovePosition[2];

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
}
