# IA ennemis — vague 1

`EnemyController` possède un `EnemyContext` propre à chaque ennemi. Il contient la cible mise en cache, la distance et la direction dans le plan XZ, ainsi que la phase et le comportement courants. Si le joueur apparaît après l'ennemi, le contrôleur adopte l'instance disponible au prochain point de décision, puis la conserve ; un spawner peut aussi fournir la cible avec `SetContextTarget`, y compris avant `Start`.

Avant chaque sélection, le contrôleur actualise les données spatiales et expire la mémoire d'échange. Il sélectionne ensuite les comportements de la phase active dont le poids est strictement positif et fini, avec un tirage proportionnel à leur poids. En l'absence de candidat, il attend la cadence configurée dans **Sélection IA / Retry interval without candidate** au lieu de relancer une sélection immédiatement.

Les comportements existants ne changent pas : leur poids est implicitement `100`, et leur filtre `IConditionalEnemyBehaviour` historique reste appliqué. La file `EnqueueBehaviour` reste prioritaire et ne passe pas par le tirage pondéré. Si une phase n'a pas de comportement de transition, le contrôleur lance directement sa sélection normale.

Un comportement qui souhaite adapter sa sélection implémente optionnellement `IContextualEnemyBehaviour` : `CanExecute(EnemyContext)` décide son éligibilité et `GetWeight(EnemyContext)` retourne son poids. Il n'existe ni labels, ni continuations, ni interruption automatique.

```csharp
public bool CanExecute(EnemyContext context) => context.HasTarget;

public float GetWeight(EnemyContext context)
{
    return context.DistanceToTarget <= 3.0f ? 200.0f : 25.0f;
}
```

`CommonReactionTestBehaviour` reste présent parce que `DummyRoom` le référence déjà. Son Inspector ne contient que **Data** : la configuration est portée par `CommonReactionTestData`. Les deux instances de `DummyRoom` référencent respectivement :

- `Assets/Enemies/CommonBehaviours/Data/CommonReactionTestShortData.asset` — poids normal/proche `100`/`100`, distance `3 m`, éligible, durée `1 s`.
- `Assets/Enemies/CommonBehaviours/Data/CommonReactionTestLongData.asset` — poids normal/proche `100`/`100`, distance `3 m`, éligible, durée `5 s`.

Modifier ces assets dans leur Inspector change les valeurs sans dupliquer les réglages sur le comportement. Si **Data** est vide, le comportement est inéligible et le message Odin explique la référence manquante, sans journaliser à répétition.

Vérifications manuelles :

1. Ouvrir `DummyRoom`, `GladiatorRoom` et `BiscottoRoom` : les phases et comportements existants doivent être toujours visibles dans l'Inspector.
2. Jouer plusieurs cycles pour confirmer que les chaînes existantes continuent de s'enchaîner et que les comportements de phase restent sélectionnés.
3. Retirer temporairement le comportement de transition d'une phase : la sélection normale doit démarrer sans bloquer le boss.
4. Créer temporairement une phase vide ou dont tous les comportements conditionnels sont indisponibles : l'ennemi attend, un seul avertissement est produit, puis il réessaie à la cadence affichée.
5. Avec deux ennemis actifs, vérifier que leurs informations de contexte affichées en débogage restent séparées.
6. Dans `DummyRoom` en mode debug, exécuter le comportement de transition immédiat : au plus 32 complétions synchrones sont traitées dans la même requête, puis l'exécution s'arrête avec un avertissement au lieu de récursiver.

La mémoire d'échange de `EnemyContext` reste une donnée passive accessible via `RecordExchangeResult`; aucun événement combat ne l'alimente et la sélection ne l'interprète pas. La réaction immédiate explicite et les données de roulade/proximité sont décrites dans `AiImmediateReactions.md`. Aucune logique ne modifie les dégâts, la parade, l'humilité ou l'arrogance.
