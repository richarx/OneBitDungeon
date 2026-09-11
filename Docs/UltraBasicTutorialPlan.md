# Plan — tutoriel temporaire dans TutorialRoom

Statut : proposition d’implémentation, fondée sur les scripts et la scène actuels. Ce document ne modifie pas le gameplay.

## Intention

Huit étapes courtes, dans l’ordre demandé. Une notion et un objectif visibles à la fois. Pas d’étape d’attaque normale, de roulade ou de contre-attaque.

`UltraBasicTutorial` orchestre la salle et enchaîne huit `TutorialData` avec `TutorialRunner.RunAsync`. Le runner conserve la validation des objectifs ; `TutorialBasicPresenter` conserve leur affichage et les glyphes de commandes.

Le mannequin reste invincible. Le joueur ne perd aucun point de vie pendant tout le tutoriel, mais reçoit normalement les coups : réaction, recul et perte d’arrogance. Un échec relance uniquement l’attaque de l’exercice, sans recommencer le tutoriel. Une réussite suffit par étape.

## Déroulé et textes

Les textes ci-dessous sont les textes complets des objectifs. Les retours à la ligne séparent l’explication de l’action. Les balises entre accolades correspondent aux actions Rewired existantes ; elles seront remplacées par les touches ou boutons adaptés.

### 1. Barre d’arrogance et taunt

**Asset :** `UBT_01_ArroganceEtTaunt.asset`

> Provoquer remplit ta barre d’arrogance.
> Maintiens {ArroganceMode} devant le mannequin pour passer en mode arrogant et provoquer.

- Mannequin passif, enregistré comme cible ; jauge initialement vide.
- Mettre brièvement la barre en évidence avec un repère sur le HUD.
- Valider après une seconde continue de taunt : `PlayerTauntedForOneSecond`, cible 1.
- La jauge augmente réellement pendant la démonstration. Son remplissage complet n’est pas nécessaire.
- Réinitialiser le suivi de durée du taunt à l’entrée de l’étape, pour ne pas compter un maintien commencé avant l’affichage.

### 2. Attaque impertinente

**Asset :** `UBT_02_AttaqueImpertinente.asset`

> Une barre pleine permet une attaque impertinente. Elle utilise toute ton arrogance.
> Appuie sur {Critical} près du mannequin.

- Garder le mannequin passif et remplir une seule fois la jauge avec `FillArrogance()`.
- Garantir une cible principale vivante, une épée disponible et une distance compatible avec `insolenceRange`.
- Prévoir assez d’espace derrière le mannequin pour le déplacement de l’attaque.
- Valider sur `PlayerCriticalAttackStarted`, cible 1.
- Attendre ensuite la fin de l’attaque avant de préparer l’étape suivante. Ne pas remplir la jauge pendant sa consommation : le joueur doit la voir se vider.
- La commande est bien `{Critical}`, pas `{Attack}` ni `{CriticalAttack}`.

### 3. Perdre son arrogance en prenant un coup

**Asset :** `UBT_03_PerteArrogance.asset`

> Un coup reçu vide ta barre d’arrogance.
> Laisse le mannequin te toucher. Ici, tu ne perds pas de vie.

- Remplir de nouveau la jauge, puis lancer une attaque lente et annoncée.
- Utiliser une zone qui ne peut être ni parée ni sautée pour cette démonstration ; ne pas afficher encore de consigne sur les couleurs défensives.
- Valider sur `PlayerTookDamage`, cible 1, après un coup réellement accepté par `PlayerHealth`.
- Vérifier la jauge vide avant de poursuivre et garder le résultat visible brièvement. Ne pas la recharger aussitôt.
- Si le joueur évite le coup, recommencer l’attaque. Attendre la fin des éventuelles protections d’une action précédente avant de lancer la première tentative.

### 4. Taunt dans la zone de danger

**Asset :** `UBT_04_TauntDangerZone.asset`

> La zone au sol annonce une attaque. Y provoquer remplit ta barre plus vite.
> Entre dans la zone et maintiens {ArroganceMode}.

- Partir avec une jauge vide pour rendre le gain visible.
- Faire apparaître une vraie zone de dégâts du jeu, avec environ trois secondes d’annonce. Elle doit laisser le temps d’entrer puis de provoquer une seconde.
- Valider sur `PlayerTauntedInDangerZoneForOneSecond`, cible 1 ; un taunt à l’extérieur ne compte pas.
- Annuler immédiatement la zone après validation afin de préserver le gain observé. La fenêtre d’affichage de réussite ne doit pas laisser l’attaque toucher le joueur.
- Si le joueur se fait toucher avant la réussite, recommencer l’annonce.
- Ne pas afficher de multiplicateur chiffré : le gain peut dépendre des réglages de progression existants.

### 5. Spin

**Asset :** `UBT_05_Spin.asset`

> En mode arrogant, l’esquive devient un spin.
> Maintiens {ArroganceMode}, choisis une direction et appuie sur {Roll}.

- Mannequin passif, aucune zone active : apprendre d’abord le déplacement.
- Valider sur `PlayerSpinStarted`, cible 1.
- Attendre la fin du spin avant de lancer l’exercice suivant.
- `{Roll}` est la commande existante du spin ; cela n’ajoute pas d’apprentissage de la roulade.

### 6. Close dodge

**Asset :** `UBT_06_CloseDodge.asset`

> Quitter la zone au dernier moment avec un spin donne de l’arrogance : c’est un close dodge.
> Maintiens {ArroganceMode} et sors de la zone avec {Roll} juste avant l’impact.

- Vider la jauge pour que la récompense soit visible.
- Répéter une attaque fixe, étroite et annoncée, avec une pause entre les tentatives. Ne pas faire suivre le joueur par la zone une fois l’annonce commencée.
- Placer le point d’exercice près du bord de la zone pour qu’un spin puisse réellement en sortir.
- Valider uniquement sur `PlayerCloseDodgePerformed`, cible 1 ; le simple départ d’un spin ne compte pas.
- Conserver la fenêtre réelle de close dodge. Régler la géométrie et le rythme de l’attaque, plutôt que changer les règles du joueur pour le tutoriel.
- Le détecteur actuel exige que le joueur soit hors de la zone et encore en spin lors de la résolution. Le réglage de la zone doit permettre cette combinaison.

### 7. Parade sur attaque jaune

**Asset :** `UBT_07_Parade.asset`

> Une attaque jaune peut être parée avec {Parry} juste avant l’impact.
> Relâche {ArroganceMode} : impossible de parer en mode arrogant.

- Arrêter les zones précédentes et annoncer uniquement des attaques jaunes.
- Zone configurée avec `canBeParried = true`, `canBeJumped = false`, et les couleurs correspondantes pendant l’annonce.
- Attendre que le joueur relâche le mode arrogant avant la première attaque. Il doit effectuer lui-même cette sortie de mode.
- Valider sur `PlayerParrySucceeded`, cible 1. Un blocage maintenu ou un appui sans attaque ne compte pas.
- Répéter l’attaque si nécessaire ; ne pas ajouter de contre-attaque à apprendre.

### 8. Saut sur attaque bleue

**Asset :** `UBT_08_Saut.asset`

> Une attaque bleue s’évite en sautant avec {Jump} juste avant l’impact.
> Impossible de sauter en mode arrogant : garde {ArroganceMode} relâché.

- Arrêter les attaques jaunes, puis annoncer uniquement des attaques bleues.
- Zone configurée avec `canBeJumped = true`, `canBeParried = false`, et les couleurs correspondantes pendant l’annonce.
- Valider sur un nouveau signal `PlayerJumpAvoidedAttack`, cible 1 : le joueur doit être dans la zone au moment de l’impact et le saut doit effectivement empêcher le coup.
- Un saut à côté, trop tôt ou trop tard ne valide pas. Répéter l’attaque jusqu’à réussite.
- Après réussite : annuler les attaques restantes, masquer les objectifs, vider la jauge d’exercice, restaurer l’invincibilité précédente et ouvrir les sorties avec le mécanisme de portes existant.

## Réutilisation et ajouts

### `UltraBasicTutorial.cs` — nouveau, dans `Assets/Tutorial/Runtime/TutorialsScript`

Un `MonoBehaviour` volontairement simple, avec huit références explicites aux `TutorialData`. Chaque méthode d’étape prépare la salle, attend le runner puis nettoie l’exercice. Pas de nouveau système générique de quêtes ou de graphe de progression.

Références privées sérialisées : `_runner`, `_attackEmitter`, les huit données, le mannequin, les points de placement et le repère de barre. Références du joueur résolues et mises en cache à l’initialisation, après sa création.

Responsabilités :

- Se lancer une seule fois quand l’entrée de scène est terminée. S’appuyer sur `GameManager.OnChangeScene` et gérer aussi le lancement manuel après cet événement ; ne pas concurrencer les téléportations et le verrouillage du `GameManager` au démarrage.
- Valider les dépendances et les prérequis du joueur avant le lancement : cible principale, épée, perte d’arrogance sur coup activée.
- Enchaîner strictement les étapes, préparer les jauges et piloter les attaques.
- Remettre le joueur et le mannequin à leurs points d’exercice quand nécessaire, entre les actions, avec `TeleportPlayer` pour le joueur. Une tentative ratée ne doit pas provoquer une téléportation systématique.
- Activer la protection des points de vie pour toute la séance et restaurer la valeur antérieure à la fin, à l’arrêt, à une erreur ou à une sortie de scène.
- Annuler l’étape, les attentes et les attaques avant un redémarrage. Attendre le nettoyage de l’exécution précédente avant tout nouvel appel au runner.
- Gérer une durée de lecture minimale réglable et une courte confirmation visuelle, sans mettre le jeu entier en pause.
- Exposer via Odin : lancer, arrêter, recommencer et démarrer à une étape pour les essais. Le saut de debug applique tous les prérequis de l’étape choisie.

Les champs privés suivent la convention `_`, avec `[SerializeField]`, `[Required]` et des groupes Odin séparant références, réglages et debug.

### `TutorialAttackEmitter.cs` — nouveau petit composant de scène

Il instancie et configure les zones existantes, garde les instances actives et permet de les annuler. Il propose une attaque unique ou une répétition contrôlée, avec durée d’annonce, intervalle, direction et dimensions réglables.

Utiliser `ConeDamageZone` et `DealDamageToPlayer` existants, avec une direction figée au début de chaque annonce. Des profils sérialisés dans ce composant suffisent ; aucun nouveau type de ScriptableObject n’est nécessaire pour les attaques temporaires.

Prévoir trois variantes de prefab fondées sur les zones actuelles : démonstration/close dodge, jaune parable, bleue sautable. Réutiliser les matériaux existants quand adaptés, et régler aussi les couleurs de remplissage/flash pour conserver une annonce cohérente. Les couleurs seules ne déterminent pas le comportement : contrôler également les deux propriétés défensives.

Le mannequin reste passif ; ce composant est l’unique source d’attaques de la séance. Le `DummyConeBehaviour` actuel suit le joueur et ne propose pas de commande publique de séance : il n’est pas adapté tel quel à l’exercice de sortie de zone.

### Adaptations limitées aux scripts existants

| Élément | Ajustement prévu | Motif |
| --- | --- | --- |
| `PlayerHealth` | Accès contrôlé à la valeur d’invincibilité, permettant lecture, activation et restauration. | L’implémentation actuelle protège déjà la vie sans supprimer le signal de coup ni le stagger. |
| `TutorialSignalBridge` | Ajouter le désabonnement symétrique de `SubToBridge` et le nouveau signal de saut réussi. | Éviter les objectifs encore abonnés après annulation et reconnaître un saut utile. |
| `TutorialObjectiveRuntime` | Libérer son abonnement lors de la fin ou de l’annulation. | Les abonnements actuels ne sont jamais retirés individuellement. |
| `TutorialRunner` | Libérer tous les objectifs dans son nettoyage ; conserver brièvement leur validation à l’écran. Prévoir une notification de réussite avant ce délai pour arrêter les attaques immédiatement. | Le runner masque aujourd’hui les lignes dès que les objectifs sont terminés. |
| `DealDamageToPlayer` | Émettre un événement de saut ayant évité le coup, sur la branche `canBeJumped`, une seule fois par attaque. L’émetteur relaie uniquement les zones qu’il possède vers le bridge pendant l’exercice. | `PlayerJumpStarted` ne prouve pas qu’une attaque a été évitée. |
| `TutorialSignalId` | Ajouter `PlayerJumpAvoidedAttack` avec une nouvelle valeur à la fin de l’énumération. | Préserver les valeurs déjà sérialisées dans les assets. |
| `PlayerJump.CanJump` / `PlayerParry.CanParry` | Centraliser l’interdiction pendant le mode arrogant et couvrir l’appui simultané avec la commande de mode. | `ArrogantIdle` bloque ces transitions, mais `ArrogantRun` les propose encore ; les gardes actuelles ne contrôlent pas ce mode. |
| `ArroganceProcessor` | Permettre de remettre à zéro le suivi temporel du taunt à l’entrée de son exercice. | Compter une seconde après l’affichage, sans reprendre un ancien maintien. |

Ne pas créer d’objectifs d’information avec `SignalId.None` : le runner les refuse actuellement. Les courtes explications restent dans le texte de l’objectif actif, ce que le presenter sait déjà afficher. Ne pas modifier la structure de `TutorialData` pour ce besoin.

La notification de saut doit provenir d’un véritable contrôle de collision de la zone pendant sa fenêtre de dégâts. Elle doit être liée à la tentative active, dédupliquée et désabonnée avec la zone ; une action ancienne ne doit jamais valider l’étape suivante.

## Branchement de TutorialRoom

La scène contient déjà `Testtuto` avec le runner et le bridge, le prefab de presenter et un `DummyFighter` invincible. Remplacer le composant `TutorialTestSequencer` de cet objet par `UltraBasicTutorial`, en conservant le runner, le bridge et le presenter.

- Ajouter les huit assets sous `Assets/Tutorial/Data/UltraBasic/`, avec des identifiants stables, des textes français dans `fallbackText` et une cible de 1 sans compteur affiché.
- Ajouter l’émetteur au mannequin et assigner les trois variantes de zones.
- Garder le mannequin passif dès l’entrée de scène et explicitement enregistré comme ennemi principal. Le taunt exige une cible et l’attaque impertinente exige une cible principale.
- Retirer du montage du tutoriel les lanceurs de test concurrents, dont l’ancien composant `CircleAttack` et le lancement manuel du comportement de cône.
- Ajouter des transforms repères pour le mannequin, le joueur et l’exercice près du bord de zone. Ils restent invisibles en jeu.
- Ajouter au HUD un simple repère activable pour la barre, piloté par `UltraBasicTutorial`.
- Activer le verrouillage de la salle sur `DoorsHolder` : `isRoomLocking` vaut actuellement `false`. Les portes sont déverrouillées à la réussite via `GameManager.OnUnlockLevel` ; conserver leurs destinations existantes.
- Vérifier le profil réellement chargé. `KnightData` active déjà `loseAllArroganceOnHit`, contrairement à `ThiefData`. Utiliser un profil compatible dans cette salle ; ne pas altérer un asset global à l’exécution.

Pour cette version temporaire, la progression reste locale à la séance : une nouvelle entrée dans TutorialRoom recommence le tutoriel. Aucune sauvegarde de progression supplémentaire.

## Ordre de réalisation et validation

1. Fiabiliser les abonnements du runner et les commandes d’invincibilité ; ajouter le signal de saut réellement réussi et les gardes du mode arrogant.
2. Ajouter l’émetteur et vérifier ses trois profils indépendamment, notamment leur annulation et leurs couleurs.
3. Créer `UltraBasicTutorial`, ses huit données et brancher la scène.
4. Régler en Play Mode la position des exercices, le rythme des attaques, la lisibilité des textes et les délais entre étapes.

Vérifications ciblées :

- Parcours complet au clavier et à la manette, avec glyphes corrects ; entrée par une porte et lancement direct de la scène.
- Pas de vie perdue ; coup réel et barre vidée à l’étape 3 ; invincibilité antérieure restaurée après fin, arrêt ou sortie de scène.
- Taunt extérieur refusé à l’étape 4 ; spin ordinaire refusé à l’étape 6 ; blocage maintenu refusé à l’étape 7 ; saut hors zone ou mal synchronisé refusé à l’étape 8.
- Saut et parade impossibles en mode arrogant, y compris depuis les états de déplacement et avec des commandes simultanées ; leur fonctionnement normal reste disponible après relâchement.
- Échecs répétés sans blocage de progression ; sortie de portée récupérable ; aucune attaque restante lors d’un changement d’étape.
- Arrêt/reprise et lancement de debug sans double abonnement, objectif validé par une ancienne tentative ou erreur de runner déjà occupé.
- Les sorties restent fermées pendant la séance et s’ouvrent après le saut réussi.

Ajouter des tests ciblés pour les abonnements après annulation, les gardes de saut/parade et le signal de saut évité. Les réglages de scène et la compréhension des annonces nécessitent une vérification en jeu.
