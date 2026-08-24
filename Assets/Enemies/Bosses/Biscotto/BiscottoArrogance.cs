using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BiscottoArrogance : MonoBehaviour
{
    [SerializeField]
    [Required]
    [LabelText("Sprites des niveaux")]
    private SpriteRenderer[] arroganceLevelSprites = new SpriteRenderer[3];

    [ShowInInspector]
    [ReadOnly]
    [LabelText("Niveau d'arrogance")]
    public int CurrentArroganceLevel => currentArroganceLevel;


    private Color[] emptyLevelColors;
    private int currentArroganceLevel;
    public bool IsFull => MaxArroganceLevel > 0 && currentArroganceLevel >= MaxArroganceLevel;

    private int MaxArroganceLevel => arroganceLevelSprites?.Length ?? 0;

    private void Awake()
    {
        CacheEmptyLevelColors();
        RefreshLevelSprites();
    }

    public void AddArroganceLevel()
    {
        currentArroganceLevel = Mathf.Min(currentArroganceLevel + 1, MaxArroganceLevel);
        RefreshLevelSprites();
    }

    public bool ConsumeFullArrogance()
    {
        if (!IsFull)
            return false;

        currentArroganceLevel = 0;
        RefreshLevelSprites();
        return true;
    }

    private void CacheEmptyLevelColors()
    {
        emptyLevelColors = new Color[MaxArroganceLevel];

        for (int i = 0; i < MaxArroganceLevel; i++)
        {
            if (arroganceLevelSprites[i] != null)
                emptyLevelColors[i] = arroganceLevelSprites[i].color;
        }
    }

    private void RefreshLevelSprites()
    {
        if (emptyLevelColors == null || emptyLevelColors.Length != MaxArroganceLevel)
            CacheEmptyLevelColors();

        for (int i = 0; i < MaxArroganceLevel; i++)
        {
            SpriteRenderer levelSprite = arroganceLevelSprites[i];
            if (levelSprite == null)
                continue;

            levelSprite.color = i < currentArroganceLevel ? Color.white : emptyLevelColors[i];
        }
    }
}
