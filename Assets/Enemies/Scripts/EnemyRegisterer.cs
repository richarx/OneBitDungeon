using Enemies.Scripts;
using Enemies.Spawner;
using UnityEngine;

public class EnemyRegisterer : MonoBehaviour
{
    private void Start()
    {
        EnemyHolder.instance.RegisterEnemy(gameObject);

        GetComponent<Damageable>().OnDie.AddListener(() =>
        {
            EnemyHolder.instance.UnRegisterEnemy(gameObject);
        });
    }
}
