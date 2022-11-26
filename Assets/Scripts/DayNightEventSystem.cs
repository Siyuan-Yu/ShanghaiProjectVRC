
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DayNightEventSystem : UdonSharpBehaviour
{
    public int dayHourTime;
    public int nightHourTime;

    public int virtualTimeHour;

    void Start()
    {
        
    }

    void update()
    {
       if(virtualTimeHour > dayHourTime)
        {

        }
    }
}
