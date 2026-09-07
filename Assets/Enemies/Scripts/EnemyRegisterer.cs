using Enemies.Scripts;
using Enemies.Spawner;
using UnityEngine;

public class EnemyRegisterer : MonoBehaviour
{
    [SerializeField] private bool isMainEnemy;

    private void Start()
    {
        EnemyHolder.instance.RegisterEnemy(gameObject, isMainEnemy);

        GetComponent<Damageable>().OnDie.AddListener(() =>
        {
            EnemyHolder.instance.UnRegisterEnemy(gameObject);
        });
    }
}
