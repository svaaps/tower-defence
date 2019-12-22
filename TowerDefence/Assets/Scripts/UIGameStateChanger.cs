using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameStateChanger : MonoBehaviour
{
    private static UIGameStateChanger instance;
    public static UIGameStateChanger Instance => instance;

    [SerializeField]
    private RectTransform
        stateStart,
        stateBuild,
        statePlay,
        statePause,
        stateWin,
        stateLose,
        stateSave;

    private void Awake()
    {
        instance = this;
    }

    public void SetState(Game.State state)
    {
        stateStart.gameObject.SetActive(false);
        stateBuild.gameObject.SetActive(false);
        statePlay.gameObject.SetActive(false);
        statePause.gameObject.SetActive(false);
        stateWin.gameObject.SetActive(false);
        stateLose.gameObject.SetActive(false);
        stateSave.gameObject.SetActive(false);

        switch (state)
        {
            case Game.State.Start:
                stateStart.gameObject.SetActive(true);
                break;
            case Game.State.Build:
                stateBuild.gameObject.SetActive(true);
                break;
            case Game.State.Play:
                statePlay.gameObject.SetActive(true);
                break;
            case Game.State.Pause:
                statePause.gameObject.SetActive(true);
                break;
            case Game.State.Win:
                stateWin.gameObject.SetActive(true);
                break;
            case Game.State.Lose:
                stateLose.gameObject.SetActive(true);
                break;
            case Game.State.Save:
                stateSave.gameObject.SetActive(true);
                break;
        }
    }
}
