using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SekiroSpawnData", menuName = "ScriptableObjects/Sekiro/Spawn Data")]
public sealed class SekiroSpawnData : ScriptableObject
{
    [TitleGroup("Intro")]
    [SerializeField, LabelText("Disable hitbox during intro")]
    private bool _disableHitboxDuringIntro = true;

    [TitleGroup("Intro")]
    [SerializeField, LabelText("Hide sprite during delay")]
    private bool _hideSpriteDuringDelay = true;

    [TitleGroup("Entrance")]
    [SerializeField, LabelText("Use vertical entrance")]
    private bool _useVerticalEntrance = true;

    [TitleGroup("Entrance")]
    [SerializeField, ShowIf(nameof(_useVerticalEntrance)), MinValue(0.0f), LabelText("Entrance height"), SuffixLabel("meters")]
    private float _entranceHeight = 8.0f;

    [TitleGroup("Entrance")]
    [SerializeField, ShowIf(nameof(_useVerticalEntrance)), MinValue(0.01f), LabelText("Fall duration"), SuffixLabel("seconds")]
    private float _fallDuration = 0.35f;

    [TitleGroup("Spawn zone")]
    [SerializeField, LabelText("Spawn circle enabled")]
    private bool _spawnCircleEnabled = true;

    [TitleGroup("Spawn zone")]
    [SerializeField, ShowIf(nameof(_spawnCircleEnabled)), Required, LabelText("Circle zone prefab")]
    private CircleDamageZone _spawnCirclePrefab;

    [TitleGroup("Spawn zone")]
    [SerializeField, ShowIf(nameof(_spawnCircleEnabled)), MinValue(0.0f), LabelText("Radius"), SuffixLabel("meters")]
    private float _spawnCircleRadius = 4.0f;

    [TitleGroup("Spawn zone")]
    [SerializeField, ShowIf(nameof(_spawnCircleEnabled)), MinValue(0.0f), LabelText("Spawn duration"), SuffixLabel("seconds")]
    private float _spawnCircleDuration = 0.25f;

    [TitleGroup("Spawn zone")]
    [SerializeField, ShowIf(nameof(_spawnCircleEnabled)), MinValue(0.0f), LabelText("Fill duration"), SuffixLabel("seconds")]
    private float _spawnCircleFillDuration = 0.5f;

    [TitleGroup("Timing")]
    [SerializeField, MinValue(0.0f), LabelText("Hidden delay"), SuffixLabel("seconds")]
    private float _hiddenDelay = 0.5f;

    [TitleGroup("Timing")]
    [SerializeField, MinValue(0.0f), LabelText("Intro duration without circle"), SuffixLabel("seconds")]
    private float _introDuration = 1.0f;

    [TitleGroup("Timing")]
    [SerializeField, MinValue(0.0f), LabelText("Recovery duration"), SuffixLabel("seconds")]
    private float _recoveryDuration = 0.35f;

    [TitleGroup("Animation")]
    [SerializeField, LabelText("Spawn")]
    private string _spawnAnimation;

    [TitleGroup("Animation")]
    [SerializeField, LabelText("Fall")]
    private string _fallAnimation;

    [TitleGroup("Animation")]
    [SerializeField, LabelText("Impact")]
    private string _impactAnimation;

    [TitleGroup("Animation")]
    [SerializeField, LabelText("Recovery")]
    private string _recoveryAnimation;

    public bool DisableHitboxDuringIntro => _disableHitboxDuringIntro;
    public bool HideSpriteDuringDelay => _hideSpriteDuringDelay;
    public bool UseVerticalEntrance => _useVerticalEntrance;
    public float EntranceHeight => _entranceHeight;
    public float FallDuration => _fallDuration;
    public bool SpawnCircleEnabled => _spawnCircleEnabled;
    public CircleDamageZone SpawnCirclePrefab => _spawnCirclePrefab;
    public float SpawnCircleRadius => _spawnCircleRadius;
    public float SpawnCircleDuration => _spawnCircleDuration;
    public float SpawnCircleFillDuration => _spawnCircleFillDuration;
    public float HiddenDelay => _hiddenDelay;
    public float IntroDuration => _introDuration;
    public float RecoveryDuration => _recoveryDuration;
    public string SpawnAnimation => _spawnAnimation;
    public string FallAnimation => _fallAnimation;
    public string ImpactAnimation => _impactAnimation;
    public string RecoveryAnimation => _recoveryAnimation;
}
