using System.Collections.Generic;
using Tools_and_Scripts;
using UnityEngine;

public class RockOrbiter : MonoBehaviour
{
    [SerializeField] private List<SpinRock> rockPrefabs;
    [SerializeField] private GameObject rockDebrisPrefab;

    private List<SpinRock> rocks = new List<SpinRock>();

    public static RockOrbiter instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SetupRockList();
    }

    private void SetupRockList()
    {
        for (int i = 0; i < 15; i++)
        {
            int index = Random.Range(0, rockPrefabs.Count);
            float distance = Random.Range(15.0f, 25.0f);

            SpinRock rock = Instantiate(rockPrefabs[index], Vector3.right * distance, Quaternion.identity, transform);
            rocks.Add(rock);
        }
    }

    public SpinRock GetRandomRock()
    {
        int index = Random.Range(0, rocks.Count);
        SpinRock rock = rocks[index];

        rocks.RemoveAt(index);

        Debug.Log($"Get Random Rock : {rock != null} / {index}");

        return rock;
    }

    public void AddBackRock(SpinRock rock)
    {
        float distance = Random.Range(15.0f, 25.0f);
        rock.transform.position = Random.insideUnitCircle.normalized.ToVector3() * distance;

        rocks.Add(rock);
        rock.SetLockState(false);
    }

    public void SpawnDebris(Vector3 position)
    {
        Instantiate(rockDebrisPrefab, position, Quaternion.identity);
    }
}
