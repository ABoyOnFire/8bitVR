using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlindsDirection : MonoBehaviour
{
    public BlindManager Blind_1;
    public BlindManager Blind_2;
    public BlindManager Blind_3;
    public BlindManager Blind_4;
    public BlindManager Blind_5;
    public BlindManager bottomBlind;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (bottomBlind.BlindsAreAtLimit())
        {
            int bottomDirection = bottomBlind.GetBlindDirection();
            if (bottomDirection == 0)
            {
                bottomDirection = 1;
            }
            else
            {
                bottomDirection = 0;
            }
            Blind_1.SetBlindDirection(bottomDirection);
            Blind_2.SetBlindDirection(bottomDirection);
            Blind_3.SetBlindDirection(bottomDirection);
            Blind_4.SetBlindDirection(bottomDirection);
            Blind_5.SetBlindDirection(bottomDirection);
            bottomBlind.SetBlindDirection(bottomDirection);
        }
    }
    
}
