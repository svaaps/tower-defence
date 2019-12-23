using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    private static Game instance;

    public static Game Instance => instance;

    public enum State
    {
        Start,
        Build,
        Play,
        Pause,
        Win,
        Lose,
        Save,
    }

    private State gameState = State.Start;

    public State GameState
    {
        get => gameState;
        private set
        {
            gameState = value;
            UIGameStateChanger.Instance.SetState(value);
        }
    }

    [SerializeField]
    private float tickInterval;

    private float counter;

    [SerializeField]
    private Mob mobPrefab;

    private List<Mob> mobs = new List<Mob>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        GameState = State.Start;
    }

    public void PressNewGame()
    {
        if (GameState != State.Start)
            return;

        GameState = State.Build;
        NewGame();
    }

    public void PressPlay()
    {
        if (GameState != State.Build)
            return;

        GameState = State.Play;
        Play();
    }

    public void PressPause()
    {
        if (GameState != State.Play)
            return;

        GameState = State.Pause;
        Pause();
    }

    public void PressResume()
    {
        if (GameState != State.Pause)
            return;

        GameState = State.Play;
        Resume();
    }

    public void PressGiveUp()
    {
        if (GameState != State.Pause)
            return;

        GameState = State.Lose;
        GiveUp();
    }

    public void PressSaveAndExit()
    {
        if (GameState != State.Build)
            return;

        GameState = State.Save;
        SaveAndExit();
    }

    private void NewGame()
    {
        Map.Instance.Generate();
    }

    private void Play()
    {
        Time.timeScale = 1;
    }

    private void Pause()
    {
        Time.timeScale = 0;
    }

    private void Resume()
    {
        Time.timeScale = 1;
    }

    private void GiveUp()
    {
        
    }

    private void SaveAndExit()
    {

    }

    private void Win()
    {

    }

    private void Lose()
    {

    }

    private void Update()
    {
        if (GameState == State.Play)
        {
            counter += Time.deltaTime;
            while (counter >= tickInterval)
            {
                counter -= tickInterval;
                Map.Instance.Tick();
                AudioManager.Instance.Tick();
            }
            Map.Instance.InterTick(counter / tickInterval);
            AudioManager.Instance.InterTick(counter / tickInterval);
        }

        else if (GameState == State.Build)
        {
            Map.Instance.BuildModeUpdate();
        }
    }

    public void ClearMobs()
    {
        foreach (Mob mob in mobs)
        {
            Destroy(mob.gameObject);
        }
        mobs.Clear();
    }

    public Mob ClosestMob(Vector3 position)
    {
        Mob closest = null;
        float sqDistance = float.MaxValue;
        foreach (Mob mob in mobs)
        {
            float d = Map.SquareDistance(position, mob.transform.position);
            if (closest == null || sqDistance > d)
            {
                closest = mob;
                sqDistance = d;
            }
        }
        return closest;
    }

    public List<Mob> MobsInRange(Vector3 position, float range)
    {
        List<Mob> inRange = new List<Mob>();

        foreach (Mob mob in mobs)
            if (Map.SquareDistance(position, mob.transform.position) <= range * range)
                inRange.Add(mob);

        return inRange;
    }

    public void AddDamage(Vector3 position, float damage, float range)
    {
        foreach (Mob mob in MobsInRange(position, range))
        {
            Vector3 delta = mob.transform.position - position;
            float distance = delta.magnitude;
            delta /= distance;
            float rangeMultiplier = 1f - distance / range;
            mob.AddDamage(damage * rangeMultiplier);
        }
    }

    public void AddForce(Vector3 position, float force, float range)
    {
        foreach (Mob mob in MobsInRange(position, range))
        {
            Vector3 delta = mob.transform.position - position;
            float distance = delta.magnitude;
            delta /= distance;
            float rangeMultiplier = 1f - distance / range;
            mob.AddForce(delta * force * rangeMultiplier);
        }
    }

    public void AddMob(Mob prefab, Vector3 position)
    {
        if (prefab == null)
            return;
        mobs.Add(Instantiate(prefab, position, Quaternion.identity));
    }

    public void RemoveMob(Mob instance)
    {
        mobs.Remove(instance);
    }
}
