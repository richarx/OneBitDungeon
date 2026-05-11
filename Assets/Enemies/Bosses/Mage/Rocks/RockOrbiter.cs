using System.Collections;
using System.Collections.Generic;
using Game_Manager;
using UnityEngine;
using static SpinRock;

public class RockOrbiter : MonoBehaviour
{
    [SerializeField] private List<SpinRock> rockPrefabs;
    [SerializeField] private GameObject rockDebrisPrefab;

    private List<SpinRock> rocks = new List<SpinRock>();

    public static RockOrbiter instance;

    private RockState globalState = RockState.Spinning;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SetupRockList();

        GameManager.OnUnlockLevel.AddListener(DestroyRocks);
    }

    private void SetupRockList()
    {
        for (int i = 0; i < 50; i++)
        {
            int index = Random.Range(0, rockPrefabs.Count);
            float distance = Random.Range(15.0f, 25.0f);

            SpinRock rock = Instantiate(rockPrefabs[index], Vector3.right * distance, Quaternion.identity, transform);
            rock.SetupRockAtStartOfLevel();
            rocks.Add(rock);
        }
    }

    private void SpawnDebris(Vector3 position)
    {
        Instantiate(rockDebrisPrefab, position, Quaternion.identity);

        MageSFX.instance.PlayRockBreak();
    }

    private void DestroyRocks()
    {
        StartCoroutine(DestroyRocksCoroutine());
    }

    private IEnumerator DestroyRocksCoroutine()
    {
        yield return new WaitForSeconds(0.3f);

        int rockCount = rocks.Count;

        for (int i = rockCount - 1; i >= 0; i--)
        {
            SpawnDebris(rocks[i].transform.position);
            Destroy(rocks[i].gameObject);
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void HideRocks()
    {
        globalState = RockState.Hidden;

        foreach (SpinRock rock in rocks)
            rock.SetState(RockState.Hidden);
    }

    public void BounceRocks()
    {
        globalState = RockState.Bouncing;

        foreach (SpinRock rock in rocks)
            rock.SetState(RockState.Bouncing);
    }

    public void DisplayRocks()
    {
        globalState = RockState.Spinning;

        foreach (SpinRock rock in rocks)
            rock.SetState(RockState.Spinning);
    }

    public void SetRockSpeed(float boost)
    {
        foreach (SpinRock rock in rocks)
            rock.SetMoveSpeedBoost(boost);
    }
}
