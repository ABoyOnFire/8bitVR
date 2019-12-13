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
    public SerialController serialController;

    // This is incase I wish to share COM BUS with multiple Game Object
    // Currently my Bridge manages everything
    public EmbeddedBridge embeddedData;

    // Initialization
    void Start()
    {
        // Get the COM port instance
        serialController = GameObject.Find("SerialController").GetComponent<SerialController>();
    }

    // Executed each frame
    void Update()
    {
        //---------------------------------------------------------------------
        // Send data
        //---------------------------------------------------------------------
        
        // Application Sends Data through embeddedBridge <embeddedData>

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
            // Pass to Object for inspection
            embeddedData.ProcessMessageData(message);
            // Debug.Log("Message arrived: " + message);
        }
    }
}
