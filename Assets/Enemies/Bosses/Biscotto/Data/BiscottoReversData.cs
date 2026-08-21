using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BiscottoReversData", menuName = "ScriptableObjects/Biscotto/Revers Data")]
public sealed class BiscottoReversData : ScriptableObject
{
    [field: SerializeField]
    [field: Required]
    [field: LabelText("Prefab de zone Revers")]
    public GameObject RectangularDamageZonePrefab { get; private set; }

    [Title("Télégraphe")]
    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée d'apparition")]
    public float SpawnDuration { get; private set; } = 0.35f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Durée de remplissage")]
    public float FillDuration { get; private set; } = 1.0f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Verrouillage avant impact")]
    public float LockBeforeImpact { get; private set; } = 0.3f;

    [field: SerializeField]
    [field: MinValue(0.001f)]
    [field: LabelText("Lissage de la visée")]
    public float RotationDampening { get; private set; } = 0.08f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Anticipation animation d'impact")]
    public float ImpactAnimationLeadTime { get; private set; } = 0.1f;

    [Title("Résultats")]
    [field: SerializeField]
    [field: MinValue(0)]
    [field: LabelText("Dégâts retournés à Biscotto")]
    public int SelfDamage { get; private set; } = 50;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Perte d'arrogance sur esquive précoce")]
    public float EarlyDodgeArroganceLoss { get; private set; }

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Récupération si le joueur est touché")]
    public float HitRecoveryDuration { get; private set; } = 0.8f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Récupération sur esquive précoce")]
    public float EarlyDodgeRecoveryDuration { get; private set; } = 0.45f;

    [field: SerializeField]
    [field: MinValue(0.0f)]
    [field: LabelText("Stun après retour")]
    public float ReflectedStunDuration { get; private set; } = 1.6f;

    [Title("Animations")]
    [field: SerializeField]
    [field: LabelText("Invitation")]
    public string InvitationAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Revers")]
    public string ImpactAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Réussite de Biscotto")]
    public string HitPlayerAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Provocation esquive précoce")]
    public string EarlyDodgeAnimation { get; private set; }

    [field: SerializeField]
    [field: LabelText("Revers retourné")]
    public string ReflectedAnimation { get; private set; }
}
