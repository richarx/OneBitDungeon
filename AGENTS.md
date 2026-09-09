# OneBitDungeon — conventions Unity

## Inspecteur

- Odin Inspector est disponible dans le projet. Pour les inspecteurs de classes et
  composants, privilégier ses attributs (`[BoxGroup]`, `[FoldoutGroup]`,
  `[ShowIf]`, `[Button]`, `[Required]`, etc.) plutôt que de créer un inspecteur
  Unity personnalisé.
- Ne créer un `CustomEditor` ou un `PropertyDrawer` que si Odin ne permet pas
  d'obtenir le comportement ou l'ergonomie voulus.
- Structurer l'inspecteur pour que les références, réglages et actions de debug
  soient faciles à distinguer. Garder les champs d'implémentation privés et
  sérialisés lorsque c'est approprié.

## Références et performances

- Préférer les dépendances assignées dans l'inspecteur (`[SerializeField]`,
  idéalement avec `[Required]`) ou résolues une seule fois à l'initialisation
  (`Awake`, `Start`, injection, ou au moment du spawn).
- Mettre en cache les composants, services, transforms et autres dépendances
  utilisés régulièrement.
- Ne pas appeler `GetComponent`, `Find`, `FindObjectOfType`, un service locator
  ou une méthode `Resolve` dans `Update`, `FixedUpdate`, `LateUpdate` ou toute
  boucle exécutée à chaque frame.
- Une résolution dynamique par frame n'est acceptable que lorsqu'elle est
  réellement nécessaire au comportement ; dans ce cas, documenter brièvement
  pourquoi le cache ou une référence initialisée ne convient pas.
