using System.Collections;
using System.Collections.Generic;
using Game_Manager;
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

        GameManager.OnUnlockLevel.AddListener(DestroyRocks);
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

    private void SetupRockList()
    {
        for (int i = 0; i < 50; i++)
        {
            int index = Random.Range(0, rockPrefabs.Count);
            float distance = Random.Range(15.0f, 25.0f);

            SpinRock rock = Instantiate(rockPrefabs[index], Vector3.right * distance, Quaternion.identity, transform);
            rocks.Add(rock);
        }
    }

    public SpinRock GetRandomRock()
    {
        if (rocks.Count < 1)
            return Instantiate(rockPrefabs[Random.Range(0, rockPrefabs.Count)], Vector3.right * 20.0f, Quaternion.identity, transform);

        int index = 0;
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
