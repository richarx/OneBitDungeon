using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SekiroPursuitData", menuName = "ScriptableObjects/Sekiro/Pursuit Data")]
public sealed class SekiroPursuitData : ScriptableObject
{
    [TitleGroup("Selection")]
    [SerializeField, MinValue(0.0f), LabelText("Base weight")]
    private float _baseWeight = 10.0f;

    [TitleGroup("Selection")]
    [SerializeField, MinValue(0.0f), LabelText("Weight per meter beyond desired range")]
    private float _weightPerMeter = 12.0f;

    [TitleGroup("Selection")]
    [SerializeField, MinValue(0.0f), LabelText("Activation distance"), SuffixLabel("meters")]
    private float _activationDistance = 4.0f;

    [TitleGroup("Selection")]
    [SerializeField, MinValue(0.0f), LabelText("Recent distant-roll bonus")]
    private float _distantRollWeightBonus = 20.0f;

    [TitleGroup("Selection")]
    [SerializeField, MinValue(0.0f), LabelText("Distant-roll memory"), SuffixLabel("seconds")]
    private float _distantRollMemoryDuration = 0.75f;

    [TitleGroup("Movement")]
    [SerializeField, MinValue(0.0f), LabelText("Desired distance"), SuffixLabel("meters")]
    private float _desiredDistance = 2.0f;

    [TitleGroup("Movement")]
    [SerializeField, MinValue(0.0f), LabelText("Maximum dash distance"), SuffixLabel("meters")]
    private float _maximumDashDistance = 5.0f;

    [TitleGroup("Movement")]
    [SerializeField, MinValue(0.01f), LabelText("Dash duration"), SuffixLabel("seconds")]
    private float _dashDuration = 0.35f;

    [TitleGroup("Animation")]
    [SerializeField, LabelText("Dash")]
    private string _dashAnimation;

    [TitleGroup("Attack")]
    [SerializeField, Required, LabelText("Cone zone prefab")]
    private ConeDamageZone _dashConePrefab;

    [TitleGroup("Attack")]
    [SerializeField, LabelText("Enabled")]
    private bool _dashAttackEnabled = true;

    [TitleGroup("Attack")]
    [SerializeField, MinValue(0.0f), LabelText("Delay after dash"), SuffixLabel("seconds")]
    private float _attackDelay;

    [TitleGroup("Attack")]
    [SerializeField, InlineProperty, LabelText("Dash attack")]
    private SekiroAttackStep _dashAttack = new SekiroAttackStep();

    public float BaseWeight => _baseWeight;
    public float WeightPerMeter => _weightPerMeter;
    public float ActivationDistance => _activationDistance;
    public float DistantRollWeightBonus => _distantRollWeightBonus;
    public float DistantRollMemoryDuration => _distantRollMemoryDuration;
    public float DesiredDistance => _desiredDistance;
    public float MaximumDashDistance => _maximumDashDistance;
    public float DashDuration => _dashDuration;
    public string DashAnimation => _dashAnimation;
    public ConeDamageZone DashConePrefab => _dashConePrefab;
    public bool DashAttackEnabled => _dashAttackEnabled;
    public float AttackDelay => _attackDelay;
    public SekiroAttackStep DashAttack => _dashAttack;
}
