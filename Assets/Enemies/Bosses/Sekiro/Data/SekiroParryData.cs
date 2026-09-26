using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SekiroParryData", menuName = "ScriptableObjects/Sekiro/Parry Data")]
public sealed class SekiroParryData : ScriptableObject
{
    [TitleGroup("Selection")]
    [SerializeField, MinValue(0.0f), LabelText("Normal weight")]
    private float _normalWeight;

    [TitleGroup("Defence")]
    [SerializeField, MinValue(0.0f), LabelText("Parry window"), SuffixLabel("seconds")]
    private float _parryWindowDuration = 0.35f;

    [TitleGroup("Defence")]
    [SerializeField, LabelText("Can parry critical attacks")]
    private bool _canParryCriticalAttacks;

    [TitleGroup("Defence")]
    [SerializeField, MinValue(0.0f), LabelText("Rearm reaction range"), SuffixLabel("meters")]
    private float _rearmReactionRange = 3.0f;

    [TitleGroup("Defence")]
    [SerializeField, Range(-1.0f, 1.0f), LabelText("Rearm minimum player facing dot")]
    private float _rearmMinimumPlayerFacingDot = 0.2f;

    [TitleGroup("Animation")]
    [SerializeField, LabelText("Start")]
    private string _startAnimation;

    [TitleGroup("Animation")]
    [SerializeField, LabelText("Success")]
    private string _successAnimation;

    [TitleGroup("Animation")]
    [SerializeField, LabelText("Recovery")]
    private string _recoveryAnimation;

    [TitleGroup("Revenge")]
    [SerializeField, Required, LabelText("Cone zone prefab")]
    private ConeDamageZone _revengeConePrefab;

    [TitleGroup("Revenge")]
    [SerializeField, LabelText("Enabled after a successful parry")]
    private bool _revengeEnabled = true;

    [TitleGroup("Revenge")]
    [SerializeField, MinValue(0.0f), LabelText("Delay"), SuffixLabel("seconds")]
    private float _revengeDelay = 0.05f;

    [TitleGroup("Revenge")]
    [SerializeField, InlineProperty, LabelText("Attack")]
    private SekiroAttackStep _revengeAttack = new SekiroAttackStep();

    [TitleGroup("Revenge")]
    [SerializeField, MinValue(0.0f), LabelText("Final recovery"), SuffixLabel("seconds")]
    private float _finalRecoveryDuration = 0.25f;

    public float NormalWeight => _normalWeight;
    public float ParryWindowDuration => _parryWindowDuration;
    public bool CanParryCriticalAttacks => _canParryCriticalAttacks;
    public float RearmReactionRange => _rearmReactionRange;
    public float RearmMinimumPlayerFacingDot => _rearmMinimumPlayerFacingDot;
    public string StartAnimation => _startAnimation;
    public string SuccessAnimation => _successAnimation;
    public string RecoveryAnimation => _recoveryAnimation;
    public ConeDamageZone RevengeConePrefab => _revengeConePrefab;
    public bool RevengeEnabled => _revengeEnabled;
    public float RevengeDelay => _revengeDelay;
    public SekiroAttackStep RevengeAttack => _revengeAttack;
    public float FinalRecoveryDuration => _finalRecoveryDuration;
}
