using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageHouseLights : MonoBehaviour
{
    public Light HouseLight_G;
    public Light HouseLight_R;
    public Light HouseLight_P;
    public Light HouseLight_B;
    public Light HouseLight_1;
    public Light HouseLight_2;

    public Light roomTemp;
    public Light roomHot;
    public Light roomCool;

    public Light boardData;
    public Light boardError;
    public Light boardConnected;

    public int HighTemp;
    public int LowTemp;

    public EmbeddedBridge embeddedData;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void UpdateLedLights()
    {
        int ledUpdate = embeddedData.GetLedValue();

        if ((ledUpdate & 0x1) == 0x1)
        {
            boardData.enabled = true;
        }
        else
        {
            boardData.enabled = false;
        }
        if ((ledUpdate & 0x2) == 0x2)
        {
            boardError.enabled = true;
        }
        else
        {
            boardError.enabled = false;
        }
        if ((ledUpdate & 0x4) == 0x4)
        {
            boardConnected.enabled = true;
        }
        else
        {
            boardConnected.enabled = false;
        }
    }

    private int UpdateGPIOLights()
    {
        int activeCount = 0;
        int gpioUpdate = embeddedData.GetGPIOValue();
        if ((gpioUpdate & 0x1) == 0x1)
        {
            HouseLight_G.enabled = false;
            HouseLight_R.enabled = false;
            HouseLight_P.enabled = false;
            HouseLight_B.enabled = false;
        }
        else
        {
            HouseLight_G.enabled = true;
            HouseLight_R.enabled = true;
            HouseLight_P.enabled = true;
            HouseLight_B.enabled = true;
            activeCount++;
        }
        if ((gpioUpdate & 0x2) == 0x2)
        {
            HouseLight_1.enabled = false;
            HouseLight_2.enabled = false;
        }
        else
        {
            HouseLight_1.enabled = true;
            HouseLight_2.enabled = true;
            activeCount++;
        }
        return activeCount;
    }

    private void UpdateRoomLight(int count)
    {
        int tempUpdate = embeddedData.GetTempValue();
        if (tempUpdate >= HighTemp)
        {
            roomTemp.enabled = false;
            roomHot.enabled = true;
            roomCool.enabled = false;
        }
        else if (tempUpdate <= LowTemp)
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
    }
    // Update is called once per frame
    void Update()
    {
        UpdateLedLights();
        UpdateRoomLight(UpdateGPIOLights());
    }
}
