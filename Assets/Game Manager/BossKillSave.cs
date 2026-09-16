using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossKillSave : MonoBehaviour
{
    public enum BossName
    {
        Biscotto,
        Mage,
        Peacock,
        Gladiator
    }

    public static BossKillSave instance;

    private List<BossName> bossKilled = new List<BossName>();

    private void Awake()
    {
        instance = this;
    }

    public void RegisterBossKilled(BossName boss)
    {
        if (!bossKilled.Contains(boss))
            bossKilled.Add(boss);
    }

    public bool IsBossKilled(BossName boss)
    {
        return bossKilled.Contains(boss);
    }
}
