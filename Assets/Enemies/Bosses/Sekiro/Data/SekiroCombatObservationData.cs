using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SekiroCombatObservationData", menuName = "ScriptableObjects/Sekiro/Combat Observation Data")]
public sealed class SekiroCombatObservationData : ScriptableObject
{
    [TitleGroup("Selection")]
    [SerializeField, MinValue(0.0f), LabelText("Normal weight")]
    private float _normalWeight = 20.0f;

    [TitleGroup("Selection")]
    [SerializeField, MinValue(0.0f), LabelText("Maximum distance"), SuffixLabel("meters")]
    private float _maximumDistance = 100.0f;

    [TitleGroup("Movement")]
    [SerializeField, MinValue(0.0f), LabelText("Desired distance"), SuffixLabel("meters")]
    private float _desiredDistance = 2.5f;

    [TitleGroup("Movement")]
    [SerializeField, MinValue(0.0f), LabelText("Approach speed"), SuffixLabel("meters/second")]
    private float _approachSpeed = 2.0f;

    [TitleGroup("Movement")]
    [SerializeField, MinValue(0.0f), LabelText("Duration"), SuffixLabel("seconds")]
    private float _duration = 1.25f;

    [TitleGroup("Animation")]
    [SerializeField, LabelText("Observation")]
    private string _observationAnimation;

    [TitleGroup("Reaction references")]
    [SerializeField, Required, LabelText("Parry data")]
    private SekiroParryData _parryData;

    [TitleGroup("Reaction references")]
    [SerializeField, Required, LabelText("Pursuit data")]
    private SekiroPursuitData _pursuitData;

    [TitleGroup("Reaction references")]
    [SerializeField, MinValue(0.0f), LabelText("Parry reaction range"), SuffixLabel("meters")]
    private float _parryReactionRange = 3.0f;

    [TitleGroup("Reaction references")]
    [SerializeField, Range(-1.0f, 1.0f), LabelText("Minimum player facing dot")]
    private float _minimumPlayerFacingDot = 0.2f;

    [TitleGroup("Reaction references")]
    [SerializeField, MinValue(0.0f), LabelText("Replace with pursuit beyond"), SuffixLabel("meters")]
    private float _pursuitReplaceDistance = 5.0f;

    public float NormalWeight => _normalWeight;
    public float MaximumDistance => _maximumDistance;
    public float DesiredDistance => _desiredDistance;
    public float ApproachSpeed => _approachSpeed;
    public float Duration => _duration;
    public string ObservationAnimation => _observationAnimation;
    public SekiroParryData ParryData => _parryData;
    public SekiroPursuitData PursuitData => _pursuitData;
    public float ParryReactionRange => _parryReactionRange;
    public float MinimumPlayerFacingDot => _minimumPlayerFacingDot;
    public float PursuitReplaceDistance => _pursuitReplaceDistance;
}
