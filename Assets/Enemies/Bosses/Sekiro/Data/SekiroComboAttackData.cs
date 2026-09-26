using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SekiroComboAttackData", menuName = "ScriptableObjects/Sekiro/Combo Attack Data")]
public sealed class SekiroComboAttackData : ScriptableObject
{
    [TitleGroup("Selection")]
    [SerializeField, MinValue(0.0f), LabelText("Normal weight")]
    private float _normalWeight = 50.0f;

    [TitleGroup("Selection")]
    [SerializeField, MinValue(0.0f), LabelText("Maximum starting distance"), SuffixLabel("meters")]
    private float _maximumStartingDistance = 4.0f;

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
    [SerializeField, MinValue(0.0f), LabelText("Rapid parry window"), SuffixLabel("seconds")]
    private float _rapidParryWindowDuration = 0.45f;

    [TitleGroup("Reaction references")]
    [SerializeField, MinValue(0.0f), LabelText("Replace with pursuit beyond"), SuffixLabel("meters")]
    private float _pursuitReplaceDistance = 5.0f;

    [TitleGroup("Zones")]
    [SerializeField, Required, LabelText("Cone zone prefab")]
    private ConeDamageZone _coneZonePrefab;

    [TitleGroup("Rapid attacks")]
    [SerializeField, InlineProperty, LabelText("Rapid attack 1")]
    private SekiroAttackStep _rapidAttack1 = new SekiroAttackStep();

    [TitleGroup("Rapid attacks")]
    [SerializeField, InlineProperty, LabelText("Rapid attack 2")]
    private SekiroAttackStep _rapidAttack2 = new SekiroAttackStep();

    [TitleGroup("Variant A — long cone")]
    [SerializeField, MinValue(0.0f), LabelText("Selection weight")]
    private float _variantAWeight = 1.0f;

    [TitleGroup("Variant A — long cone")]
    [SerializeField, InlineProperty, LabelText("Long cone")]
    private SekiroAttackStep _variantALongCone = new SekiroAttackStep();

    [TitleGroup("Variant B — medium cone then rapid")]
    [SerializeField, MinValue(0.0f), LabelText("Selection weight")]
    private float _variantBWeight = 1.0f;

    [TitleGroup("Variant B — medium cone then rapid")]
    [SerializeField, InlineProperty, LabelText("Medium cone")]
    private SekiroAttackStep _variantBMediumCone = new SekiroAttackStep();

    [TitleGroup("Variant B — medium cone then rapid")]
    [SerializeField, InlineProperty, LabelText("Final rapid attack")]
    private SekiroAttackStep _variantBFinalRapid = new SekiroAttackStep();

    public float NormalWeight => _normalWeight;
    public float MaximumStartingDistance => _maximumStartingDistance;
    public SekiroParryData ParryData => _parryData;
    public SekiroPursuitData PursuitData => _pursuitData;
    public float ParryReactionRange => _parryReactionRange;
    public float MinimumPlayerFacingDot => _minimumPlayerFacingDot;
    public float RapidParryWindowDuration => _rapidParryWindowDuration;
    public float PursuitReplaceDistance => _pursuitReplaceDistance;
    public ConeDamageZone ConeZonePrefab => _coneZonePrefab;
    public SekiroAttackStep RapidAttack1 => _rapidAttack1;
    public SekiroAttackStep RapidAttack2 => _rapidAttack2;
    public float VariantAWeight => _variantAWeight;
    public SekiroAttackStep VariantALongCone => _variantALongCone;
    public float VariantBWeight => _variantBWeight;
    public SekiroAttackStep VariantBMediumCone => _variantBMediumCone;
    public SekiroAttackStep VariantBFinalRapid => _variantBFinalRapid;
}
