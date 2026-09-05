using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GladiatorSkippingRopeData", menuName = "ScriptableObjects/Gladiator/Skipping Rope")]
public class GladiatorSkippingRopeData : ScriptableObject
{
    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de skipping rope")]
    public SkippingRopeController ropeControllerPrefab;

    [field: SerializeField]
    [field: LabelText("Offset de position de la rope par rapport au boss")]
    [field: SuffixLabel("mètres")]
    public Vector3 ThrowOffset;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Distance parcourue par la rope")]
    [field: SuffixLabel("mètres")]
    public float FlyDistance;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'extension de la rope")]
    [field: SuffixLabel("mètres")]
    public float ExtensionDuration;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de retractation de la rope")]
    [field: SuffixLabel("mètres")]
    public float RetractionDuration;

    [field: SerializeField]
    [field: LabelText("Vitesse de rotation")]
    [field: SuffixLabel("angle/secondes")]
    public float RotationSpeed { get; private set; } = 0.25f;

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Lissage de la rotation")]
    public float RotationDampening;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de la rotation")]
    [field: SuffixLabel("secondes")]
    public float SkippingDuration { get; private set; } = 0.25f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée du déplacement")]
    [field: SuffixLabel("secondes")]
    public float MoveDuration { get; private set; } = 0.25f;

    [field: SerializeField]
    [field: LabelText("Animation d'anticipation")]
    public string AnticipationAnimation { get; private set; }

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de l'animation de lancé de hook")]
    [field: SuffixLabel("secondes")]
    public float AnticipationAnimationDuration;

    [field: SerializeField]
    [field: LabelText("Animation de lancé")]
    public string RopeThrowAnimation { get; private set; }

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de l'animation de lancé de hook")]
    [field: SuffixLabel("secondes")]
    public float RopeThrowAnimationDuration;

    [field: SerializeField]
    [field: LabelText("Animation de recovery")]
    public string RecoveryAnimation { get; private set; }

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de l'animation de recovery")]
    [field: SuffixLabel("secondes")]
    public float RecoveryAnimationDuration;

    [field: SerializeField]
    [field: LabelText("After-image pendant le déplacement")]
    public bool TriggerAfterImageOnSideMove { get; private set; } = true;
}
