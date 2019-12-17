using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameClock : MonoBehaviour
{
    [SerializeField]
    private float tickInterval;

    private float counter;

    public void FixedUpdate()
    {
        counter += Time.deltaTime;
        while(counter >= tickInterval)
        {
            counter -= tickInterval;
            Map.Instance.Tick();
        }
        Map.Instance.InterTick(counter / tickInterval);
    }
}
