/**
 * Ardity (Serial Communication for Arduino + Unity)
 * Author: Daniel Wilches <dwilches@gmail.com>
 *
 * This work is released under the Creative Commons Attributions license.
 * https://creativecommons.org/licenses/by/2.0/
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/**
 * Sample for reading using polling by yourself, and writing too.
 */
public class UserPolling_VR : MonoBehaviour
{
    public embeddedBridge embeddedData;
    public SerialController serialController;

    // Initialization
    void Start()
    {
        serialController = GameObject.Find("SerialController").GetComponent<SerialController>();
        Debug.Log("Press A or Z to execute some actions");
    }

    // Executed each frame
    void Update()
    {
        //---------------------------------------------------------------------
        // Send data
        //---------------------------------------------------------------------
        if (embeddedData.getButtonState())
        {
            if (embeddedData.getVRButtonState() == true)
            {
                Debug.Log("Sent led0 on");
            }
            if (embeddedData.getVRButtonState() == false)
            {
                Debug.Log("Sent led0 off");
            }
            embeddedData.setButtonState(false);
        }
        // If you press one of these keys send it to the serial device. A
        // sample serial device that accepts this input is given in the README.
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("led0 on");
            serialController.SendSerialMessage("led0 on");
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("led0 off");
            serialController.SendSerialMessage("led0 off");
        }

        //---------------------------------------------------------------------
        // Receive data
        //---------------------------------------------------------------------

        string message = serialController.ReadSerialMessage();

        if (message == null)
        { 
            return;
        }

        // Check if the message is plain data or a connect/disconnect event.
        if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_CONNECTED))
        {
            Debug.Log("Connection established");
        }

        else if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_DISCONNECTED))
        {
            Debug.Log("Connection attempt failed or disconnection detected");
        }

        else
        {
            Debug.Log("Message arrived: " + message);
            embeddedData.processMessageData(message);
        }
    }
}
