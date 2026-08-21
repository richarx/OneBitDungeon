using Player.Scripts;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Aligne la séparation du matériau SplitScreen entre deux positions monde.
/// La droite est calculée dans l'espace viewport : y = _Pente * x + _Origine.
/// </summary>
public class SplitScreenMaterialController : MonoBehaviour
{
    private static readonly int PenteId = Shader.PropertyToID("_Pente");
    private static readonly int OrigineId = Shader.PropertyToID("_Origine");
    private static readonly int PowerDecalageId = Shader.PropertyToID("_PowerDecalage");

    [TitleGroup("Références")]
    [SerializeField, Required] private Material splitScreenMaterial;

    [TitleGroup("Références")]
    [Tooltip("Caméra qui rend l'effet. Laisse vide pour utiliser CamerasHolder puis Camera.main.")]
    [SerializeField] private Camera effectCamera;

    [TitleGroup("Références")]
    [Tooltip("Optionnel : laisse vide pour utiliser PlayerStateMachine.instance.")]
    [SerializeField] private PlayerStateMachine player;

    [Tooltip("Évite une division par zéro lorsque la ligne est verticale.")]
    [SerializeField, MinValue(0.0001f)] private float minimumHorizontalSeparation = 0.001f;

    [TitleGroup("Test")]
    [SerializeField, MinValue(0.01f)] private float testDuration = 0.2f;

    [TitleGroup("Test")]
    [Tooltip("Amplitude avant normalisation. Le déplacement est normalisé selon la pente de la séparation.")]
    [SerializeField] private float testPowerDecalage = 1.0f;

    [TitleGroup("Debug"), ShowInInspector, ReadOnly, LabelText("Pente")]
    private float CurrentPente => splitScreenMaterial != null ? splitScreenMaterial.GetFloat(PenteId) : 0.0f;

    [TitleGroup("Debug"), ShowInInspector, ReadOnly, LabelText("Ordonnée à l'origine")]
    private float CurrentOrigine => splitScreenMaterial != null ? splitScreenMaterial.GetFloat(OrigineId) : 0.0f;

    private Sequence testSequence;

    public void Start()
    {
        if (splitScreenMaterial == null)
            Debug.LogWarning("SplitScreenMaterialController : aucun matériau SplitScreen n'est assigné.", this);
        if (effectCamera == null)
        {
            if (CamerasHolder.instance != null && CamerasHolder.instance.mainCamera != null)
                effectCamera = CamerasHolder.instance.mainCamera;
            else
                effectCamera = Camera.main;
        }
        if (player == null)
        {
            player = PlayerStateMachine.instance;
        }
    }

    /// <summary>
    /// Convertit les deux positions monde en viewport puis écrit les coefficients de la droite dans le matériau.
    /// </summary>
    public void SetSplitLine(Vector3 playerWorldPosition, Vector3 bossWorldPosition)
    {
        Vector3 playerViewportPosition = effectCamera.WorldToViewportPoint(playerWorldPosition);
        Vector3 bossViewportPosition = effectCamera.WorldToViewportPoint(bossWorldPosition);
        SetSplitLineFromViewport(playerViewportPosition, bossViewportPosition);
    }

    /// <summary>
    /// Grab la camera dans la scene
    /// </summary>
    public void SetNewCamera()
    {
        if (CamerasHolder.instance != null && CamerasHolder.instance.mainCamera != null)
            effectCamera = CamerasHolder.instance.mainCamera;
    }

    public void SetSplitLineFromWorldPositions(Vector3 playerPosition, Vector3 bossPosition)
    {
        if (effectCamera == null)
            SetNewCamera();

        SetSplitLineFromViewport(effectCamera.WorldToViewportPoint(playerPosition), effectCamera.WorldToViewportPoint(bossPosition));
    }

    /// <summary>
    /// Écrit une droite à partir de deux positions viewport (x et y entre 0 et 1).
    /// Utile si les positions ont déjà été projetées à l'écran.
    /// </summary>
    public void SetSplitLineFromViewport(Vector2 playerViewportPosition, Vector2 bossViewportPosition)
    {
        if (splitScreenMaterial == null)
        {
            Debug.LogWarning("SplitScreenMaterialController : aucun matériau SplitScreen n'est assigné.", this);
            return;
        }

        if (effectCamera == null)
            SetNewCamera();

        float deltaX = bossViewportPosition.x - playerViewportPosition.x;
        float safeDeltaX = Mathf.Abs(deltaX) < minimumHorizontalSeparation
            ? (deltaX < 0.0f ? -minimumHorizontalSeparation : minimumHorizontalSeparation)
            : deltaX;

        float pente = (bossViewportPosition.y - playerViewportPosition.y) / safeDeltaX;
        float origine = playerViewportPosition.y - (pente * playerViewportPosition.x);

        splitScreenMaterial.SetFloat(PenteId, pente);
        splitScreenMaterial.SetFloat(OrigineId, origine);
    }

    [TitleGroup("Test"), Button("Tester joueur → boss ciblé"), EnableIf(nameof(CanTestCurrentTarget))]
    private void TestCurrentTarget()
    {
        GameObject currentTarget = player.playerTargeting.Target;

        SetSplitLine(player.position, currentTarget.transform.position);
        PlayTestPowerDecalageAnimation(testPowerDecalage);
    }

    private void PlayTestPowerDecalageAnimation(float powerDecalage = 1.0f)
    {
        if (splitScreenMaterial == null)
            return;

        if (testSequence.isAlive)
            testSequence.Stop();

        float pente = splitScreenMaterial.GetFloat(PenteId);
        float normalizedPower = powerDecalage / Mathf.Sqrt(1.0f + (pente * pente));

        splitScreenMaterial.SetFloat(PowerDecalageId, 0.0f);
        testSequence = Sequence.Create()
            .Chain(Tween.MaterialProperty(splitScreenMaterial, PowerDecalageId, normalizedPower, testDuration, Ease.OutQuad))
            .Chain(Tween.MaterialProperty(splitScreenMaterial, PowerDecalageId, 0.0f, testDuration, Ease.InQuad));
    }

    public void PlayPowerDecalageAnimation(float powerDecalage, float durationIn, float durationOut)
    {
        if (splitScreenMaterial == null)
            return;

        if (testSequence.isAlive)
            testSequence.Stop();

        float pente = splitScreenMaterial.GetFloat(PenteId);
        float normalizedPower = powerDecalage / Mathf.Sqrt(1.0f + (pente * pente));

        splitScreenMaterial.SetFloat(PowerDecalageId, 0.0f);
        testSequence = Sequence.Create()
            .Chain(Tween.MaterialProperty(splitScreenMaterial, PowerDecalageId, normalizedPower, durationIn, Ease.OutExpo))
            .Chain(Tween.MaterialProperty(splitScreenMaterial, PowerDecalageId, 0.0f, durationOut, Ease.OutBack));
    }

    private bool CanTestCurrentTarget()
    {
        return Application.isPlaying
            && splitScreenMaterial != null
            && effectCamera != null
            && player != null
            && player.playerTargeting != null
            && player.playerTargeting.hasTarget;
    }

    private void OnDisable()
    {
        if (testSequence.isAlive)
            testSequence.Stop();
    }

    private void OnDestroy()
    {
        splitScreenMaterial.SetFloat(PenteId, 0.0f);
        splitScreenMaterial.SetFloat(OrigineId, 0.0f);
    }
}
