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
    public Light generalAction_0;
    public Light generalAction_1;
    public Text xText;
    public Text yText;
    public Text zText;
    public Text tempText;
    public TextMesh screenText;

    private int pushRetainValue;
    private int ledRetainValue;
    private int gpioRetainValue;
    private int tempRetainValue;

    private string displayText;

    private const float xOstart = (-1.5f);
    private const float zOstart = (0.5f);

    private const float xYOstart = (2.0f);
    private const float yYstart = (1.5f);
    private const float zYstart = (1.0f);

    private float[] defaultMovePosition = { 0.0f, 1.5f, 4.0f };
    private SerialController upstreamSerialBus;

    const bool defaultLedStartState = false;

    const string DEFAULT_MESSAGE = "Hello World";

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
        moveObject.transform.position = new Vector3(defaultMovePosition[0], defaultMovePosition[1], defaultMovePosition[2]);
        xObject.transform.position = new Vector3(xOstart, xYOstart, zOstart);
        yObject.transform.position = new Vector3(xOstart, yYstart, zOstart);
        zObject.transform.position = new Vector3(xOstart, zYstart, zOstart);
        upstreamSerialBus = GameObject.Find("SerialController").GetComponent<SerialController>();
        screenText.text = DEFAULT_MESSAGE;
        displayText = screenText.text;
        ledRetainValue = 0;
        gpioRetainValue = 0;
        tempRetainValue = 0;
    }

    void UpstreamSerialCommunication(SerialController upstreamBus)
    {
        if ((upstreamSerialBus == null) && (upstreamBus != null))
        {
            upstreamSerialBus = upstreamBus;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update Display
        string printString;
        printString = displayText + "\r\n"
                        + "Temp: " + tempText.text + "\r\n"
                        + "X: " + xText.text + "\r\n"
                        + "Y: " + yText.text + "\r\n"
                        + "Z: " + zText.text;
        screenText.text = printString;

        // Keyboard Test
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Test Key Press");
        }

        // Keyboard Actions
        if (Input.GetKeyDown(KeyCode.F12))
        {
            SceneManager.LoadScene("ces2020");
        }
    }

    public int GetPushValue()
    {
        return pushRetainValue;
    }
    public int GetLedValue()
    {
        return ledRetainValue;
    }

    public int GetGPIOValue()
    {
        return gpioRetainValue;
    }

    public int GetTempValue()
    {
        return tempRetainValue;
    }

    public void SendVrMessage(string message)
    {
        upstreamSerialBus.SendSerialMessage(message);
    }
    public bool ProcessMessageData(string message)
    {
        if (message.StartsWith("{"))
            {
            // Parse new data format
            return ProcessJSONFormat(message);
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

    private void ProcessTemperatureTask(string passedValue)
    {
        // Temperature
        mcp9844_reg_t registerSetting;
        string fixedOrder = passedValue.Substring(2, 2) + passedValue.Substring(0, 2);
        registerSetting.mcp9844_flags = fixedOrder.Substring(0, 1);
        registerSetting.mcp9844_whole = fixedOrder.Substring(1, 2);
        registerSetting.mcp9844_decimal = fixedOrder.Substring(3, 1);
        int tempValue = int.Parse(registerSetting.mcp9844_whole, System.Globalization.NumberStyles.HexNumber);
        int tempDecimalHex = int.Parse(registerSetting.mcp9844_decimal, System.Globalization.NumberStyles.HexNumber);
        int tempFlags = int.Parse(registerSetting.mcp9844_flags, System.Globalization.NumberStyles.HexNumber);
        float convertMath = tempDecimalHex;
        convertMath = (convertMath * 62.5f);
        tempDecimalHex = (int)convertMath;

        tempRetainValue = tempValue;

        if ((tempFlags & 0x01) == 0x01)
        {
            tempText.text = "-";
        }
        else
        {
            tempText.text = "+";
        }
        tempText.text = tempText.text + tempValue + "." + tempDecimalHex;
    }

    private void ProcessPushButtonTask(string passedValue)
    {
        int pushhexValue = int.Parse(passedValue);
        if ((pushhexValue & 0x1) == 0x1)
        {
            vrLight.enabled = true;
        }
        else if ((pushhexValue & 0xFF) == 0x00)
        {   // Check For 0x00
            vrLight.enabled = false;
        }
        else
        {   // Failure Condition
            vrLight.enabled = false;
        }
        pushRetainValue = pushhexValue;
    }

    private void ProcessLedTask(string passedValue)
    {
        int ledhexValue = int.Parse(passedValue);
        boardLED_0.enabled = false;
        boardLED_1.enabled = false;
        boardLED_2.enabled = false;
        if ((ledhexValue & 0x1) == 0x1)
        {
            boardLED_0.enabled = true;
        }
        if ((ledhexValue & 0x2) == 0x2)
        {
            boardLED_1.enabled = true;
        }
        if ((ledhexValue & 0x3) == 0x3)
        {
            boardLED_2.enabled = true;
        }

        ledRetainValue = ledhexValue;
    }

    private void ProcessXaccelTask(string passedValue)
    {
        string axString = string.Copy(passedValue);
        bool xNegative = false;
        // Fix Order
        axString = axString.Substring(2, 2) + axString.Substring(0, 2);
        int axValue = int.Parse(axString, System.Globalization.NumberStyles.HexNumber);

        if (axValue > 0x800)
        {
            axValue = 0xFFF - axValue;
            xNegative = true;
        }

        float xfloat = (float)(axValue / 2000.00f);
        float axAdjust;
        float xObjectPos;

        if (xNegative == true)
        {
            axAdjust = xfloat + zOstart;
            xObjectPos = xfloat + defaultMovePosition[0];
        }
        else
        {
            axAdjust = zOstart - xfloat;
            xObjectPos = defaultMovePosition[0] - xfloat;
        }

        // Update Text on Screen
        if (xNegative)
        {
            xText.text = "-" + axValue.ToString();
        }
        else
        {
            xText.text = axValue.ToString();
        }

        // Move Things
        moveObject.transform.position = new Vector3(xObjectPos, moveObject.transform.position.y, moveObject.transform.position.z);
        xObject.transform.position = new Vector3(xOstart, xYOstart, axAdjust);
    }

    private void ProcessYaccelTask(string passedValue)
    {
        string ayString = string.Copy(passedValue);
        bool yNegative = false;
        // Fix Order
        ayString = ayString.Substring(2, 2) + ayString.Substring(0, 2);
        int ayValue = int.Parse(ayString, System.Globalization.NumberStyles.HexNumber);

        if (ayValue > 0x800)
        {
            ayValue = 0xFFF - ayValue;
            yNegative = true;
        }

        float yfloat = (float)(ayValue / 2000.00f);
        float ayAdjust = 0.0f;
        float yObjectPos = 0.0f;

        if (yNegative == true)
        {
            ayAdjust = yfloat + zOstart;
            yObjectPos = yfloat + defaultMovePosition[1];
        }
        else
        {
            ayAdjust = zOstart - yfloat;
            yObjectPos = defaultMovePosition[1] - yfloat;
        }

        // Update Text on Screen
        if (yNegative)
        {
            yText.text = "-" + ayValue.ToString();
        }
        else
        {
            yText.text = ayValue.ToString();
        }
        // Move Things
        moveObject.transform.position = new Vector3(moveObject.transform.position.x, yObjectPos, moveObject.transform.position.z);
        yObject.transform.position = new Vector3(xOstart, yYstart, ayAdjust);
    }

    private void ProcessZaccelTask(string passedValue)
    {
        string azString = string.Copy(passedValue);
        bool zNegative = false;
        // Fix Order
        azString = azString.Substring(2, 2) + azString.Substring(0, 2);
        int azValue = int.Parse(azString, System.Globalization.NumberStyles.HexNumber);
        if (azValue > 0x800)
        {
            azValue = 0xFFF - azValue;
            zNegative = true;
        }
        float zfloat = (float)(azValue / 2000.00f);
        float azAdjust = 0.0f;
        float zObjectPos = 0.0f;

        if (zNegative == true)
        {
            azAdjust = zfloat + zOstart;
            zObjectPos = zfloat + defaultMovePosition[2];
        }
        else
        {
            azAdjust = zOstart - zfloat;
            zObjectPos = defaultMovePosition[2] - zfloat;
        }

        // Update Text on Screen
        if (zNegative)
        {
            zText.text = "-" + azValue.ToString();
        }
        else
        {
            zText.text = azValue.ToString();
        }
        // Move Things
        moveObject.transform.position = new Vector3(moveObject.transform.position.x, moveObject.transform.position.y, zObjectPos);
        zObject.transform.position = new Vector3(xOstart, zYstart, azAdjust);
    }

    private void ProcessGeneralTask(string passedValue)
    {
        // General
        int genhexValue = int.Parse(passedValue);
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

        gpioRetainValue = genhexValue;
    }

    private void ProcessSerialTask(string passedMessage)
    {
        Debug.Log("-> " + passedMessage);
        displayText = passedMessage;
        upstreamSerialBus.SendSerialMessage(passedMessage);
    }
    private bool ProcessJSONFormat(string message)
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
                    ProcessTemperatureTask(pair.Value);
                    break;
                case ("P"):
                    ProcessPushButtonTask(pair.Value);
                    break;
                case ("L"):
                    ProcessLedTask(pair.Value);

                    break;
                case ("A"):
                    // Accelerometer
                    string axString = pair.Value.Substring(0, 2) + pair.Value.Substring(2, 2);
                    string ayString = pair.Value.Substring(4, 2) + pair.Value.Substring(6, 2);
                    string azString = pair.Value.Substring(8, 2) + pair.Value.Substring(10, 2);
                    ProcessXaccelTask(axString);
                    ProcessYaccelTask(ayString);
                    ProcessZaccelTask(azString);
                    break;
                case ("X"):
                    ProcessXaccelTask(pair.Value);
                    break;
                case ("Y"):
                    ProcessYaccelTask(pair.Value);
                    break;
                case ("Z"):
                    ProcessZaccelTask(pair.Value);
                    break;
                case ("G"):
                    ProcessGeneralTask(pair.Value);
                    break;
                case ("S"):
                    ProcessSerialTask(pair.Value);
                    break;
                default:
                    Debug.Log("JSON like GOT: " + pair.Key + " w/ " + pair.Value);
                    validMessage = false;
                    break;
            }
        }
        return validMessage;
    }
}
