using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlindManager : MonoBehaviour
{
    public EmbeddedBridge embeddedData;
    public GameObject BlindObject;
    public GameObject TopBlind;
    public float StopOffset;
    public int moveDirection;   // 0 is up; 1 is down. 

    private bool blindDone = false;
    private float blindsSpeed = 0.1f;
    private float adjustSpeed;
    private float blindEndPoint;
    private float topBlind_Y;
    private float xstart;
    private float ystart;
    private float zstart;
    // Start is called before the first frame update
    void Start()
    {
        xstart = BlindObject.transform.position.x;
        ystart = BlindObject.transform.position.y;
        zstart = BlindObject.transform.position.z;

        topBlind_Y = TopBlind.transform.position.y;

        blindEndPoint = topBlind_Y - StopOffset;
        adjustSpeed = (blindsSpeed / 4.0f);
        moveDirection = 0;
    }

    public bool BlindsAreAtLimit()
    {
        return blindDone;
    }
    public int GetBlindDirection()
    {
        return moveDirection;
    }
    public bool SetBlindDirection(int direction)
    {
        if (moveDirection == direction)
        {
            // Request to keep moving same direction
        }
        else
        {   // New Direction
            if (blindDone == true)
            {   // We were Maxed out; Reset Rate
                blindDone = false;
                moveDirection = direction;
            }
            else
            {   // We were Mid Use. Just Change Direction
                moveDirection = direction;
            }
        }
        return blindDone;
    }

    private bool BlindShouldBeMoving()
    {
        bool blindsMoving = false;
        int pushUpdate = embeddedData.GetPushValue();

        if ((pushUpdate & 0x1) == 0x1)
        {   // No other condition moves blinds
            blindsMoving = true;
        }

        return blindsMoving;
    }
    // Update is called once per frame
    void Update()
    {
        bool blindsMoving = BlindShouldBeMoving();

        if (blindsMoving == true)
        {
            if (moveDirection == 0) // Up
            {
                if (blindDone == false)
                {
                    if (BlindObject.transform.position.y < blindEndPoint)
                    {
                        BlindObject.transform.Translate((Vector3.up * blindsSpeed) * Time.deltaTime);
                        BlindObject.transform.Translate((Vector3.right * (blindsSpeed - (adjustSpeed)) * Time.deltaTime));
                    }
                    else
                    {
                        blindDone = true;
                    }
                }
            }
            else if (moveDirection == 1)    // Down
            {
                if (blindDone == false)
                {
                    if (BlindObject.transform.position.y > ystart)
                    {
                        BlindObject.transform.Translate((Vector3.down * blindsSpeed) * Time.deltaTime);
                        BlindObject.transform.Translate((Vector3.left * (blindsSpeed - (adjustSpeed)) * Time.deltaTime));
                    }
                    else
                    {
                        blindDone = true;
                    }
                }
            }
        }
    }
}
