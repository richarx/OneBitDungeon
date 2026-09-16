using Enemies.Scripts;
using UnityEngine;

public class RegisterBossKill : MonoBehaviour
{
    [SerializeField] private BossKillSave.BossName bossName;

    private void Start()
    {
        GetComponent<Damageable>().OnDie.AddListener(() => BossKillSave.instance.RegisterBossKilled(bossName));
    }
}
