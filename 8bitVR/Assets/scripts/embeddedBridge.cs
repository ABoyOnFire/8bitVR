using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EmbeddedBridge : MonoBehaviour
{
    public SerialController serialController;
    public GameObject moveObject;
    public GameObject xObject;
    public GameObject yObject;
    public GameObject zObject;
    public Light vrLight;
    public Light boardLED_0;
    public Light boardLED_1;
    public Light boardLED_2;
    public Light roomTemp;
    public Light roomHot;
    public Light roomCool;
    public Light generalAction_0;
    public Light generalAction_1;
    public Text xText;
    public Text yText;
    public Text zText;
    public Text tempText;

    private const float xOstart =   (-1.5f);
    private const float zOstart =   (-1.85f);

    private const float xYOstart =  (0.0f);
    private const float yYstart =   (-0.5f);
    private const float zYstart =   (-1.0f);

    private float [] defaultMovePosition = {0.0f, 3.0f, 0.0f };
    private SerialController upstreamSerialBus = null;

    const bool defaultLedStartState = false;

    // Start is called before the first frame update
    void Start()
    {
        vrLight.enabled = defaultLedStartState;
        boardLED_0.enabled = defaultLedStartState;
        boardLED_1.enabled = defaultLedStartState;
        boardLED_2.enabled = defaultLedStartState;
        generalAction_0.enabled = defaultLedStartState;
        generalAction_1.enabled = defaultLedStartState;
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
            upstreamSerialBus.SendSerialMessage("led 0 on");
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
        // Prototype Data Message Format
        /*
        if (message.StartsWith("T"))
        {
            return processLegacyFormat(message);
        }
        */
        if (message.StartsWith("{"))
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

    public struct mcp9844_reg_t
    {
        public string mcp9844_flags;   // 4 bits
        public string mcp9844_whole;   // 8 bits
        public string mcp9844_decimal; // 4 bits
    };   
    private bool processJSONFormat(string message)
    {
        bool validMessage = false;
        var map = new Dictionary<string, string>();
        message = message.Replace("{", "").Replace("}", "");
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

            switch (pair.Key)
            {
                case ("T"):
                    // Temperature
                    mcp9844_reg_t registerSetting;
                    tempText.text = pair.Value;
                    string fixedOrder = tempText.text.Substring(2, 2) + tempText.text.Substring(0, 2);
                    registerSetting.mcp9844_flags = fixedOrder.Substring(0, 1);
                    registerSetting.mcp9844_whole = fixedOrder.Substring(1, 2);
                    registerSetting.mcp9844_decimal = fixedOrder.Substring(3, 1);
                    int tempValue = int.Parse(registerSetting.mcp9844_whole, System.Globalization.NumberStyles.HexNumber);
                    int tempDecimalHex = int.Parse(registerSetting.mcp9844_decimal, System.Globalization.NumberStyles.HexNumber);
                    int tempFlags = int.Parse(registerSetting.mcp9844_flags, System.Globalization.NumberStyles.HexNumber);
                    float convertMath = 0;
                    convertMath = tempDecimalHex;
                    convertMath = convertMath * 62.5f;
                    tempDecimalHex = (int)convertMath;
                    if ((tempFlags & 0x01) == 0x01)
                    {
                        tempText.text = "-";
                    }
                    else
                    {
                        tempText.text = "+";
                    }
                    tempText.text = tempText.text + tempValue + "." + tempDecimalHex;
                    Debug.Log(tempText.text);
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
                    break;
                case ("P"):
                    // Push Button
                    if (pair.Value.Equals("01"))
                    {
                        vrLight.enabled = true;
                    }
                    else if (pair.Value.Equals("00"))
                    {
                        vrLight.enabled = false;
                    }
                    else
                    {   // Error Condition
                        vrLight.enabled = false;
                    }
                    break;
                case ("L"):
                    // LEDs
                    int ledhexValue = int.Parse(pair.Value);
                    int stateLED_0 = 0x0;
                    int stateLED_1 = 0x0;
                    int stateLED_2 = 0x0;
                    boardLED_0.enabled = false;
                    boardLED_1.enabled = false;
                    boardLED_2.enabled = false;
                    stateLED_0 = (ledhexValue & 0x1);
                    stateLED_1 = (ledhexValue & 0x2);
                    stateLED_2 = (ledhexValue & 0x3);
                    if (stateLED_0 == 0x1)
                    {
                        boardLED_0.enabled = true;
                    }
                    if (stateLED_1 == 0x2)
                    {
                        boardLED_1.enabled = true;
                    }
                    if (stateLED_2 == 0x3)
                    {
                        boardLED_2.enabled = true;
                    }
                    break;
                case ("A"):
                    // Accelerometer
                    string axString = string.Copy(pair.Value);
                    string ayString = string.Copy(pair.Value);
                    string azString = string.Copy(pair.Value);

                    Debug.Log(pair.Value);

                    axString = axString.Substring(2, 2) + axString.Substring(0, 2);
                    ayString = ayString.Substring(6, 2) + ayString.Substring(4, 2);
                    azString = azString.Substring(10, 2) + azString.Substring(8, 2);

                    Debug.Log(axString);
                    Debug.Log(ayString);
                    Debug.Log(azString);
                    int axValue = int.Parse(axString, System.Globalization.NumberStyles.HexNumber);
                    int ayValue = int.Parse(ayString, System.Globalization.NumberStyles.HexNumber);
                    int azValue = int.Parse(azString, System.Globalization.NumberStyles.HexNumber);
 
                    Debug.Log(axValue);
                    Debug.Log(ayValue);
                    Debug.Log(azValue);
                    
                    float xfloat = (axValue/2000) + defaultMovePosition[0];
                    float yfloat = (ayValue / 2000) + defaultMovePosition[1];
                    float zfloat = (azValue / 2000) + defaultMovePosition[2];

                    float axAdjust = (axValue / 2000) + zOstart;
                    float ayAdjust = (axValue / 2000) + zOstart;
                    float azAdjust = (axValue / 2000) + zOstart;
                    
                    moveObject.transform.position = new Vector3(axValue, ayValue, azValue);
                    xObject.transform.position = new Vector3(xOstart, xYOstart, axAdjust);
                    yObject.transform.position = new Vector3(xOstart, xYOstart, ayAdjust);
                    zObject.transform.position = new Vector3(xOstart, xYOstart, azAdjust);
                    break;
                case ("X"):
                    // X Accelerometer
                    xText.text = pair.Value;
                    string xString = "";
                    xString = xText.text.ToString();
                    float xValue = (float.Parse(xString) / 4000) + defaultMovePosition[0];
                    float xAdjust = (float.Parse(xString) / 4000) + zOstart;

                    moveObject.transform.position = new Vector3(xValue, transform.position.y, transform.position.z);
                    xObject.transform.position = new Vector3(xOstart, xYOstart, xAdjust);
                    break;
                case ("Y"):
                    // Y Accelerometer
                    yText.text = pair.Value;
                    string yString = "";
                    yString = yText.text.ToString();
                    float yValue = (float.Parse(yString) / 4000) + defaultMovePosition[1];
                    float yAdjust = (float.Parse(yString) / 4000) + zOstart;

                    moveObject.transform.position = new Vector3(transform.position.x, yValue, transform.position.z);
                    xObject.transform.position = new Vector3(xOstart, yAdjust, zOstart);
                    break;
                case ("Z"):
                    // Z Accelerometer
                    zText.text = pair.Value;
                    string zString = "";
                    zString = zText.text.ToString();
                    float zValue = (float.Parse(zString) / 4000) + defaultMovePosition[2];
                    float zAdjust = (float.Parse(zString) / 4000) + zOstart;

                    moveObject.transform.position = new Vector3(transform.position.x, transform.position.y, zValue);
                    xObject.transform.position = new Vector3(xOstart, xYOstart, zAdjust);
                    break;
                case ("G"):
                    // General
                    int genhexValue = int.Parse(pair.Value);
                    int genhexValue_0 = 0x0;
                    int genhexValue_1 = 0x0;
                    generalAction_0.enabled = false;
                    generalAction_1.enabled = false;
                    genhexValue_0 = (genhexValue & 0x1);
                    genhexValue_1 = (genhexValue & 0x2);
                    if (genhexValue_0 == 0x1)
                    {
                        generalAction_0.enabled = true;
                    }
                    if (genhexValue_1 == 0x2)
                    {
                        generalAction_1.enabled = true;
                    }
                    break;
                case ("S"):
                    // Echo out upStream
                    Debug.Log("-> " + pair.Value);
                    upstreamSerialBus.SendSerialMessage(message);
                    break;
                default:
                    Debug.Log("JSON like GOT: " + pair.Key + " w/ " + pair.Value);
                    validMessage = false;
                    break;
            }
        }
        return validMessage;
    }
    private bool processLegacyFormat(string message)
    {
        bool validMessage = false;
        string[] splitMessage = message.Split(' ');
        int counter = 0;
        foreach (string msg in splitMessage)
        {
            Debug.Log(counter++);
            Debug.Log(msg);
        }
        // Push Button Actions
        if (splitMessage[11] == "P")
        {
            if (splitMessage[12] == "0")
            {
                vrLight.enabled = true;
            }
            else if (splitMessage[12] == "1")
            {
                vrLight.enabled = false;
            }
            else
            {
                vrLight.enabled = false;
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
                // 
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
