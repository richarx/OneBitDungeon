using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public sealed class SekiroAttackStep
{
    [BoxGroup("Animation")]
    [SerializeField, LabelText("Preparation animation")]
    private string _preparationAnimation;

    [BoxGroup("Animation")]
    [SerializeField, LabelText("Impact animation")]
    private string _impactAnimation;

    [BoxGroup("Telegraph")]
    [SerializeField, MinValue(0.0f), LabelText("Radius"), SuffixLabel("meters")]
    private float _radius = 3.0f;

    [BoxGroup("Telegraph")]
    [SerializeField, Range(0.0f, 360.0f), LabelText("Opening angle"), SuffixLabel("degrees")]
    private float _openingAngle = 90.0f;

    [BoxGroup("Rectangle zone")]
    [SerializeField, LabelText("Rectangle zone prefab")]
    private GameObject _rectangleZonePrefab;

    [BoxGroup("Rectangle zone")]
    [SerializeField, ShowIf(nameof(HasRectangleZonePrefab)), MinValue(0.01f), LabelText("Width"), SuffixLabel("meters")]
    private float _rectangleWidth = 1.8f;

    [BoxGroup("Rectangle zone")]
    [SerializeField, ShowIf(nameof(HasRectangleZonePrefab)), MinValue(0.01f), LabelText("Length"), SuffixLabel("meters")]
    private float _rectangleLength = 3.25f;

    [BoxGroup("Timing")]
    [SerializeField, MinValue(0.0f), LabelText("Spawn"), SuffixLabel("seconds")]
    private float _spawnDuration = 0.15f;

    [BoxGroup("Timing")]
    [SerializeField, MinValue(0.0f), LabelText("Fill"), SuffixLabel("seconds")]
    private float _fillDuration = 0.35f;

    [BoxGroup("Timing")]
    [SerializeField, MinValue(0.0f), LabelText("Recovery"), SuffixLabel("seconds")]
    private float _recoveryDuration = 0.4f;

    [BoxGroup("Damage")]
    [SerializeField, MinValue(0), LabelText("Damage")]
    private int _damage = 1;

    [BoxGroup("Damage")]
    [SerializeField, LabelText("Player can parry")]
    private bool _canBeParried = true;

    [BoxGroup("Damage")]
    [SerializeField, LabelText("Player can jump")]
    private bool _canBeJumped;

    [BoxGroup("Damage")]
    [SerializeField, MinValue(-1.0f), LabelText("Stagger power (-1 uses zone default)")]
    private float _staggerPower = -1.0f;

    public string PreparationAnimation => _preparationAnimation;
    public string ImpactAnimation => _impactAnimation;
    public float Radius => _radius;
    public float OpeningAngle => _openingAngle;
    public GameObject RectangleZonePrefab => _rectangleZonePrefab;
    public float RectangleWidth => _rectangleWidth;
    public float RectangleLength => _rectangleLength;
    public float SpawnDuration => _spawnDuration;
    public float FillDuration => _fillDuration;
    public float RecoveryDuration => _recoveryDuration;
    public int Damage => _damage;
    public bool CanBeParried => _canBeParried;
    public bool CanBeJumped => _canBeJumped;
    public float StaggerPower => _staggerPower;
    public float TotalDuration => _spawnDuration + _fillDuration + _recoveryDuration;

    private bool HasRectangleZonePrefab() => _rectangleZonePrefab != null;
}
