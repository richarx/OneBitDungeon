# Rebinding Rewired — plan d’implémentation

Statut : phase 1 autorisée et en cours d’implémentation par Terra ; phases 2 et 3 non lancées.
Date de l’audit : 9 septembre 2026.
Exécuteur de l’implémentation : **Terra (`gpt-5.6-terra`)**, à la demande de l’utilisateur.
Déroulement : **trois phases d’implémentation successives, avec revue du code par lecture et tests manuels à chaque phase**. Ne pas implémenter les trois phases en une seule livraison.

## 1. Contrat de travail

- Terra écrit et modifie les scripts C# ; l’utilisateur effectue les manipulations Unity.
- À chaque fin de phase, livrer les scripts, la revue du code et la checklist de tests, puis attendre le retour et la validation de l’utilisateur avant de commencer la phase suivante. Corriger les problèmes de la phase courante sans demander une nouvelle autorisation pour chaque correction.
- Ne pas modifier les scènes, prefabs, assets, ProjectSettings, packages, sprites, fichiers de sauvegarde ou sources Rewired. Ne pas générer ces modifications par un script Editor. Laisser Unity générer les `.meta` des nouveaux scripts.
- Conserver au maximum les scripts consommateurs. La modification principale porte sur `Assets/Tools and Scripts/InputPacker.cs`, qui contient aussi `InputPackage`, `InputData` et `InputType`.
- Les quelques exceptions identifiées ci-dessous sont prévues parce qu’une façade ne peut pas corriger les touches et images codées directement dans ses consommateurs.
- Respecter AGENTS.md : champs privés préfixés `_`, `[SerializeField]`, Odin pour l’inspecteur, dépendances mises en cache. Préserver les noms publics existants pour la compatibilité. Préserver les données sérialisées des consommateurs ; si un champ existant est renommé, utiliser `FormerlySerializedAs`.
- Préserver les changements locaux de l’utilisateur, notamment le travail en cours dans les tutoriels et l’import Rewired. Refaire un état des lieux avant de coder, sans rétablir ni nettoyer ces changements.
- Ne pas lancer l’implémentation en réalisant ce plan. Lors de l’exécution, utiliser réellement Terra ; ne pas simplement annoncer son nom en faisant exécuter le code par un autre modèle.

## 2. Constat et faisabilité

Le projet utilise Unity 2022.3.62f2 et Unity Input System 1.14.2. Rewired, Control Mapper, les composants UserDataStore et les outils de glyphes sont présents dans `Assets/Rewired`.

`PlayerStateMachine`, `MainMenu` et `Intro` construisent leur propre `new InputPacker()`. Le constructeur doit donc rester sans paramètre et ne pas supposer que les objets Unity ou Rewired sont déjà initialisés.

La majorité du gameplay passe par `GetMove`, `GetAttack`, `GetRoll`, etc. Une façade conservant ces propriétés peut éviter de modifier la machine à états, les comportements de combat et `DialogueManager`.

Exceptions constatées :

- `PlayerSit` utilise directement `southButton` pour se relever à la manette. Remplacer cette lecture par `GetJump`, en conservant sa condition manette, pour que ce geste suive le rebinding sans élargir le comportement clavier.
- `MainMenu` et `Intro` lisent `southButton`, `westButton`, `leftMouse` et `spaceKey` pour continuer. Conserver ces commandes physiques fixes en V1 et leurs champs de compatibilité ; ne pas modifier ces scripts.
- `InputIconDisplay` et `DialogueActionButton` choisissent entre deux sprites fixes. `TutorialPresenter` utilise une table action/type de périphérique avec des glyphes fixes. Leurs indications doivent être adaptées pour afficher les bindings effectifs.
- Les lectures directes trouvées dans `PlayerVfx` sont commentées ; celles de `PlayerHealth` sont dans un bloc désactivé. Ne pas les migrer dans cette tâche. `CameraScreenPosition` lit encore la souris via Unity Input System : conserver le package.

## 3. Architecture retenue

Flux principal : Rewired → service d’entrées → façade InputPacker → consommateurs actuels.

### Couche réutilisable, indépendante de OneBitDungeon

Prévoir un petit ensemble de nouveaux scripts hors de `Assets/Rewired`, par exemple dans `Assets/Tools and Scripts/Input/Rewired/`. Les noms précis pourront être simplifiés par Terra :

- **RewiredInputRuntime** : composant initialisant le Player Rewired configuré, mettant en cache les références et identifiants d’actions, exposant leurs valeurs et le périphérique actif.
- **RewiredRebindMenu** : composant avec référence sérialisée au Control Mapper, méthodes publiques d’ouverture/fermeture pour les boutons Unity, suspension des commandes du jeu et notifications de changement des bindings.
- **RewiredBindingDisplay** : résolution de la touche ou du bouton actuellement associé à une action, de son libellé et de son glyphe. Réutiliser les outils de glyphes déjà fournis par Rewired lorsque leur sortie convient.
- Un catalogue/configuration sérialisable uniquement si les correspondances de sprites/TMP ou les réglages le nécessitent. Terra crée la classe ; l’utilisateur crée et remplit l’asset.

Cette couche ne connaît ni `PlayerStateMachine`, ni `TutorialInputAction`, ni les actions spécifiques au combat. La configuration et la façade assurent cette adaptation. Ne pas construire de framework multijoueur ou d’abstraction multi-bibliothèques dans cette V1.

### Façade propre au jeu

Conserver dans `InputPacker.cs` :

- le namespace `Tools_and_Scripts` et les quatre types publics existants ;
- `new InputPacker()`, `ComputeInputPackage()`, `ResetBuffers()` ;
- `OnChangeInputType`, avec son type `UnityEvent<InputType>` ;
- toutes les propriétés `InputPackage.Get…` et les champs publics existants ;
- `InputData.wasPressedThisFrame`, `isPressed`, `lastPressTimestamp` et `WasPressedWithBuffer(float bufferDuration = 0.2f)`.

Les propriétés `Get…` lisent les **actions Rewired**, et cessent de déduire une action d’un bouton physique choisi selon `lastInputType`. Les champs physiques historiques restent distincts. Ne pas donner au champ `spaceKey` le sens « roulade après rebinding ».

Conserver la lecture physique nécessaire aux commandes fixes de l’intro/menu dans une portion de compatibilité limitée. Garder leurs valeurs correctes et null-safe avec les périphériques absents. Réutiliser les lectures Unity Input System existantes est acceptable pour cette portion ; aucun `Get…` rebindable ne doit en dépendre.

## 4. Phase 1 — façade et conservation du comportement

Livraison de cette phase : service d’entrées, façade, correction ciblée de `PlayerSit` et tests du buffer. Ne pas encore coder le contrôleur de menu ni adapter les trois affichages. Cette phase doit être compilable et testable avec les bindings par défaut, après configuration manuelle du runtime et des maps par l’utilisateur.

1. Définir et centraliser les noms d’actions attendus. Les résoudre en identifiants une seule fois après initialisation Rewired ; ne pas imposer des IDs numériques devinés.
2. Initialiser le service via son cycle Unity/Rewired, jamais depuis le constructeur de `InputPacker`. Avant qu’il soit prêt, renvoyer un état neutre et un diagnostic utile, sans spam. Gérer le cas Rewired déjà prêt lors de l’activation, et le cas où il devient prêt ensuite.
3. Fournir à la façade un état déjà initialisé, sans `Find`, `GetComponent`, `ReInput.players.GetPlayer`, résolution par nom ou service locator répété par frame. Définir l’ordre de lecture/publication pour éviter une frame de retard.
4. Préserver les fronts d’appui, le maintien et les buffers de combat. Un buffer consommé ne doit pas réapparaître à la frame suivante. Des lectures répétées dans la même frame ne doivent pas réarmer un appui.
5. Conserver des buffers indépendants entre instances d’InputPacker : la consommation de l’intro/menu ne consomme pas celle du joueur. Le périphérique actif, lui, peut être partagé ; son événement ne doit pas être émis plusieurs fois pour la même transition.
6. `ResetBuffers()` invalide les commandes en attente, y compris les alias. Garder la durée par défaut et l’horloge `Time.time` du gameplay actuel.
7. Préserver la vitesse de déplacement : clavier normalisé, diagonales limitées, amplitude analogique conservée. Vérifier la dead zone et ne pas l’appliquer deux fois entre Rewired et l’adaptateur.
8. Garder le contrat `GetLook` et ses réglages existants même si aucun consommateur actif n’a été trouvé lors de l’audit. Distinguer delta souris et axe du stick ; vérifier les échelles et l’usage de `deltaTime` pour éviter une double conversion.
9. Détecter clavier/souris et manette à partir d’une activité pertinente. Une souris au repos ou le bruit d’un stick ne doit pas faire clignoter les indications. Ne pas conditionner toutes les actions au dernier périphérique d’affichage : une action liée doit fonctionner depuis ses périphériques autorisés.
10. Appliquer l’exception locale dans `PlayerSit`, puis vérifier que les autres scripts de gameplay compilent sans modification.

### Configuration proposée à créer manuellement

Un Player Rewired pour la V1. Les noms ci-dessous sont le contrat proposé entre les scripts et la configuration Unity, pas des actions déjà présentes ou créées par ce plan.

| Action Rewired | Type | API historique | Clavier/souris actuels | Manette actuelle |
|---|---|---|---|---|
| MoveHorizontal | Axis | GetMove.x | A/Q négatif, D positif | Stick gauche X |
| MoveVertical | Axis | GetMove.y | S négatif, W/Z positif | Stick gauche Y |
| LookHorizontal | Axis | GetLook.x | Delta souris X | Stick droit X |
| LookVertical | Axis | GetLook.y | Delta souris Y | Stick droit Y |
| Roll | Button | GetRoll | Espace | Bouton est |
| Jump | Button | GetJump | Maj gauche | Bouton sud |
| Attack | Button | GetAttack | Clic gauche | Bouton ouest |
| Parry | Button | GetParry | Clic droit | Épaule gauche |
| Interact | Button | GetInteraction et GetDialogueConfirm | E | Bouton nord |
| SitDown | Button | GetSitDown | C | Croix bas |
| TagCritical | Button | GetTag et GetCriticalAttack | Tab | Épaule droite |
| ArroganceMode | Button | GetArroganceMode | Clic molette maintenu | Gâchette droite |
| MenuHorizontal | Axis | GetMenuLeft / GetMenuRight | A/Q et D | Croix gauche/droite |
| MenuVertical | Axis | GetMenuDown / GetMenuUp | S et W/Z | Croix bas/haut |
| DialogueQuit | Button | GetDialogueQuit | R | Bouton est |

Les entrées clavier ci-dessus reflètent le code actuel. L’utilisateur vérifiera les touches physiques et leurs libellés AZERTY/QWERTY lors de la création des maps ; ne pas supposer que noms de touches et positions physiques sont interchangeables.

**Alias intentionnels** : `GetTag` et `GetCriticalAttack` restent une même action et une même instance de buffer consommable par package. Même principe pour `GetInteraction` et `GetDialogueConfirm`. Ne pas exposer deux lignes de rebinding indépendantes pour ces alias en V1.

Prévoir des actions de navigation **propres à Control Mapper**, distinctes des getters de menu hérités : `UIHorizontal`, `UIVertical`, `UISubmit`, `UICancel`. L’utilisateur les configure dans une catégorie UI protégée, avec flèches, Entrée, Échap et une navigation manette. Les clics pointeur sont gérés par le module UI, sans dupliquer leur association dans UISubmit.

Séparer les catégories/maps pour les commandes du jeu, celles de dialogue si nécessaire et l’UI protégée. Autoriser les recouvrements contextuels existants, notamment Roll/DialogueQuit à la manette. Ne pas introduire un nouveau routeur gameplay/dialogue exigeant de modifier `DialogueManager` pour cette V1.

## 5. Phase 2 — menu et sauvegarde

Prérequis : phase 1 validée par l’utilisateur. Livraison : contrôleur Control Mapper, suspension/reprise des entrées et raccordement à la sauvegarde fournie par Rewired. L’utilisateur peut maintenant tester un véritable rebinding. Les anciennes icônes sont une limitation connue de cette livraison intermédiaire, à corriger en phase 3 ; ne pas annoncer le système complet à ce stade.

- Utiliser Control Mapper pour la capture, les conflits, l’annulation d’une capture et le retour aux commandes par défaut. Ne pas réécrire un écran de rebinding.
- Choix de persistance : composant Rewired **UserDataStore_File**, ajouté et configuré par l’utilisateur. Chargement au démarrage, sauvegarde à la fermeture normale du menu. Ne pas ajouter une deuxième sauvegarde maison.
- Dans la version installée, `ControlMapper.Close(true)` sauvegarde via `ReInput.userDataStore`. `Close(false)` ferme sans sauvegarder mais **n’annule pas** les changements appliqués en mémoire. La V1 applique les changements ; « annuler » concerne la capture en cours, pas un rollback de toute la session.
- Brancher le contrôleur sur les événements réels `ScreenOpenedEvent` / `ScreenClosedEvent`, y compris si le menu est ouvert ou fermé par ses propres commandes. Gérer aussi désactivation/destruction pour ne jamais laisser le verrou d’entrées bloqué.
- Tant que le menu est ouvert, la façade renvoie des commandes neutres, y compris les champs physiques de continuation de l’intro/menu. L’UI Rewired reste utilisable. Il faut notamment empêcher un clic sur « Contrôles » de démarrer le jeu via `MainMenu.WaitForInput`.
- Purger les buffers à l’entrée et à la sortie. Attendre le relâchement des commandes concernées avant de réactiver le gameplay : la touche de fermeture ou la touche capturée ne doit pas provoquer une roulade, une attaque ou une poursuite d’intro.
- Préserver/restaurer l’état du curseur si ce contrôleur en prend la responsabilité. Exposer des méthodes et événements simples à raccorder dans l’inspecteur.
- Cette suspension d’entrées ne constitue pas une pause de la simulation. La V1 doit être accessible depuis le menu principal, ou depuis un écran dont l’utilisateur gère déjà la pause. Ne pas ajouter un gestionnaire de pause global ni modifier les effets de ralentissement/gel existants.
- Notifier l’affichage après changement de binding, chargement, restauration des valeurs par défaut et fermeture. Rafraîchir dès que l’écran redevient visible ; ne pas simuler un changement de périphérique pour signaler un changement de touche.

## 6. Phase 3 — indications cohérentes après rebinding

Prérequis : phase 2 validée par l’utilisateur. Livraison : résolution commune des bindings/glyphes, adaptations d’affichage ciblées et vérification complète du parcours. Ne pas profiter de cette phase pour modifier le gameplay déjà validé.

Adapter seulement les points où l’image ou le texte de commande est choisi :

| Script existant | Modification limitée |
|---|---|
| InputIconDisplay.cs | Résoudre l’indication à partir de l’action Interact et du binding effectif ; conserver animation et détection. |
| DialogueActionButton.cs | Résoudre la commande de confirmation depuis Interact ; conserver les animations et événements du dialogue. |
| TutorialPresenter.cs | Associer TutorialInputAction aux actions du jeu, résoudre le binding et rafraîchir sur changement ; conserver formatage, localisation et progression. |

Le service commun fournit des informations d’action/binding ; les correspondances `TutorialInputAction` restent côté jeu. Réutiliser le catalogue et les assets TMP déjà disponibles lorsque possible, sans les modifier automatiquement.

Prévoir le clavier, la souris et les familles de manettes configurées. Quand plusieurs touches sont affectées, appliquer une règle stable : binding principal valide du périphérique affiché. Pour Move, représenter les directions utiles plutôt qu’une seule touche arbitraire.

Si un glyphe manque, afficher le libellé réel du bouton/de la touche lorsque la cible permet du texte. Sur une cible SpriteRenderer/Image seule, prévoir une référence texte optionnelle à brancher manuellement ou masquer l’image incorrecte avec un diagnostic unique. Ne jamais réafficher le sprite de l’ancienne touche après rebinding par simple fallback.

Conserver les signatures et champs sérialisés existants autant que possible. Toute modification de catalogue supplémentaire doit avoir une raison concrète ; ne pas réorganiser l’ensemble du système de tutoriels.

## 7. Périmètre de fichiers attendu

Scripts existants à modifier :

1. `Assets/Tools and Scripts/InputPacker.cs` — façade et types de compatibilité.
2. `Assets/Player/Scripts/Behaviours/PlayerSit.cs` — une lecture d’entrée à remplacer.
3. `Assets/Interactable/InputIconDisplay.cs` — résolution de l’indication.
4. `Assets/Npcs/Dialogues/DialogueActionButton.cs` — résolution de l’indication.
5. `Assets/Tutorial/Runtime/Presentation/TutorialPresenter.cs` — résolution et rafraîchissement des indications.

Ajouter les scripts du module et les tests nécessaires. Objectif : aucune modification de `PlayerStateMachine`, des autres comportements de combat, de `MainMenu`, d’`Intro` et de `DialogueManager`. Une découverte ultérieure peut justifier une exception minimale ; Terra devra l’expliquer avec le comportement concerné, sans refonte opportuniste.

## 8. Manipulations réservées à l’utilisateur

À chaque livraison, Terra fournit uniquement les manipulations nécessaires à la phase courante, avec les noms exacts réellement utilisés. Répartition : étapes 1, 2, 7 et 9 pour la phase 1 ; étapes 3 à 6 pour la phase 2 ; étape 8 pour la phase 3. Les validations de l’étape 10 sont progressives selon la section 9.

1. Créer/configurer le Rewired Input Manager et le Player de la V1, avec clavier, souris et attribution de la manette.
2. Créer les actions, catégories, layouts et maps de la table ; reproduire les bindings actuels et régler les conflits contextuels.
3. Ajouter UserDataStore_File, choisir un nom de fichier propre au jeu et activer le chargement au démarrage.
4. Installer/configurer le prefab Control Mapper, sa référence Input Manager, les actions affichées et l’UI protégée. Adapter sa présentation.
5. Configurer l’EventSystem avec RewiredStandaloneInputModule, sans modules concurrents gérant les mêmes événements.
6. Ajouter les nouveaux composants, assigner leurs références et brancher le bouton « Contrôles » aux méthodes exposées.
7. Prévoir un bootstrap persistant unique, accessible dès le menu, et la présence de ce bootstrap pour les scènes lancées directement en test. Éviter les doublons de managers entre scènes.
8. Assigner les glyphes/catalogues et éventuels champs de texte de fallback. Mettre à jour les préfabs d’affichage selon la checklist de Terra.
9. Conserver les packages et réglages d’input actuels pendant cette migration partielle.
10. Effectuer les vérifications Play Mode ci-dessous. Avant cette configuration, les scripts peuvent compiler sans que le rebinding soit utilisable en jeu.

## 9. Déroulement, revues et tests manuels

### Référence avant la phase 1

Avant le remplacement d’InputPacker, l’utilisateur relève le comportement actuel du déplacement, des combats, de l’assise et des dialogues sur clavier et manette. Noter les problèmes déjà présents pour éviter de les confondre avec une régression de la migration. Le relevé peut être une checklist courte ; aucune scène de test ni vidéo n’est exigée.

### Cycle obligatoire de chaque phase

1. **Implémentation par Terra** : uniquement le périmètre de la phase courante, en conservant les modifications locales de l’utilisateur. Ne pas commencer les fonctionnalités de la phase suivante en anticipation.
2. **Vérifications techniques disponibles** : compilation et tests C# ciblés, sans modifier de réglages Unity. Si leur exécution n’est pas possible, le signaler comme non exécuté ; ne pas l’assimiler à une réussite.
3. **Revue du code par lecture** : effectuer une passe distincte après l’écriture, lire le diff de la phase et les appels/cycles de vie concernés. Cette revue ne se réduit ni à la compilation ni à `git diff --check`. Fournir les constats avec fichier, emplacement, conséquence et correction. Ne pas présenter cette lecture par l’agent comme une revue humaine indépendante.
4. **Correction des constats** : Terra corrige les anomalies de la phase et relit les corrections. Fournir un diff final limité que l’utilisateur peut lui-même examiner, en distinguant le code préexistant de celui de la phase.
5. **Manipulations et tests manuels par l’utilisateur** : fournir des étapes numérotées, chacune avec le résultat attendu, et uniquement les branchements nécessaires à cette livraison.
6. **Retour et validation** : attendre explicitement le retour de l’utilisateur. Un test non effectué reste en attente ; le silence ne vaut pas validation. En cas d’échec, corriger la phase, relire le code modifié et demander de rejouer les scénarios affectés. Ne passer à la phase suivante qu’après validation de la phase courante, revue incluse.

Pour chaque livraison, indiquer : fichiers modifiés et raisons, manipulations Unity, résultats de la revue, vérifications exécutées/non exécutées, checklist manuelle et limitations temporaires. Ne pas demander un commit, un merge ou un changement de tâche pour valider une phase.

### Phase 1 — points de revue

- Compatibilité de toutes les signatures publiques et de `new InputPacker()` ; absence de changement des autres comportements de combat.
- Initialisation avant/après Rewired, références mises en cache, ordre d’exécution et absence d’abonnement dupliqué ou conservé après destruction.
- Front d’appui, maintien, consommation/expiration/reset des buffers, alias TagCritical et indépendance entre instances.
- Valeurs analogiques, diagonales, dead zones, souris et sélection du périphérique d’affichage.
- Distinction entre actions rebindables et champs physiques de compatibilité ; correction locale de `PlayerSit`.

### Phase 1 — tests manuels et résultats attendus

| Manipulation | Résultat attendu |
|---|---|
| Après configuration des actions et du runtime, lancer le menu puis l’intro au clavier et à la manette. | Les commandes fixes permettent toujours de continuer, sans exception dans la console. |
| Tester déplacement cardinal/diagonal, petites amplitudes du stick et retour au repos. | Vitesse comparable à la référence, amplitude analogique conservée, absence de dérive visible. |
| Tester roulade, saut, attaque, parade, tag/critique et maintien de l’arrogance. | Actions déclenchées comme auparavant ; aucun déclenchement double ni maintien bloqué. |
| Presser une commande juste avant la fin d’une action, puis la même commande trop tôt. | Le buffer conserve la réactivité attendue sans rejouer une commande expirée ou déjà consommée. |
| S’asseoir/se relever puis confirmer et quitter un dialogue. | Les commandes par défaut restent opérationnelles ; les images correspondent encore aux bindings par défaut. |
| Alterner clavier/manette, débrancher/rebrancher la manette et relancer une scène configurée directement. | Pas d’erreur, de blocage d’entrée, de frame d’initialisation provoquant une action ou de doublon de manager. |

Critère de passage : comportement de référence préservé, revue sans anomalie connue affectant la phase et retour utilisateur positif sur les scénarios exécutés. Tout scénario non exécuté doit être identifié et traité avant validation, ou explicitement accepté comme reporté par l’utilisateur.

### Phase 2 — points de revue

- Événements réels de Control Mapper, nettoyage des abonnements et libération des verrous à la fermeture/désactivation/destruction.
- Neutralisation des commandes du jeu et des entrées de continuation du menu, tout en conservant la navigation de Control Mapper.
- Purge des buffers et reprise après relâchement ; absence de clic de menu réinterprété comme démarrage du jeu ou attaque.
- Appels de sauvegarde/chargement/restauration, portée de ces appels et distinction entre annuler une capture et fermer sans sauvegarder.
- Catégories UI protégées, recouvrements contextuels et alias ; absence de gestionnaire de pause ou de sauvegarde parallèle ajouté.

### Phase 2 — tests manuels et résultats attendus

| Manipulation | Résultat attendu |
|---|---|
| Ouvrir « Contrôles » depuis le menu principal, naviguer et fermer avec clavier/souris puis manette. | Le menu fonctionne ; ni le clic d’ouverture ni la fermeture ne font continuer l’écran de démarrage. |
| Rebind Roll au clavier, Attack à la souris et Jump à la manette, puis essayer dans le jeu. | Chaque nouvelle commande fonctionne ; l’ancienne cesse d’agir si aucun binding alternatif ne la conserve. Se relever à la manette suit Jump. |
| Lancer une capture puis l’annuler ; essayer une touche déjà utilisée. | Capture annulée sans perte du binding précédent ; conflit géré selon les règles prévues, sans rendre la navigation UI inutilisable. |
| Tester TagCritical, confirmation de dialogue et Roll/DialogueQuit après modification. | Alias et partages intentionnels conservés ; pas de commande jumelle désynchronisée. |
| Fermer en maintenant une touche, puis la relâcher et presser à nouveau. | Pas d’action parasite à la fermeture ; les nouvelles entrées fonctionnent après relâchement. |
| Fermer normalement, changer de scène puis quitter et relancer le jeu. | Les bindings choisis sont retrouvés, sans exception ni manager supplémentaire. |
| Restaurer les valeurs par défaut, fermer puis relancer. | Les valeurs par défaut sont rétablies et persistent. |
| Débrancher/rebrancher la manette pendant le menu, puis fermer. | L’écran reste récupérable au clavier/souris et les entrées du jeu ne restent pas verrouillées. |

Critère de passage : rebinding, sauvegarde et reprise des commandes validés. Les indications encore fixes sont explicitement admises comme travail de phase 3, pas comme une fonctionnalité terminée. Les tests en jeu pendant l’ouverture du menu ne sont effectués que dans un contexte de pause géré par l’utilisateur ; la neutralisation des entrées n’arrête pas la simulation.

### Phase 3 — points de revue

- Résolution depuis les bindings effectifs, pas depuis les touches par défaut ; traitement de plusieurs bindings, directions et périphériques.
- Rafraîchissement après rebinding, reset, chargement, activation de l’affichage et changement de périphérique, sans faux événement de changement d’InputType.
- Conservation des champs/références sérialisés, animations, localisation et progression des tutoriels.
- Comportement quand le glyphe ou la référence manque ; aucun ancien sprite trompeur après remapping.
- Module commun indépendant du joueur et des tutoriels ; modifications limitées aux consommateurs prévus.

### Phase 3 — tests manuels et résultats attendus

| Manipulation | Résultat attendu |
|---|---|
| Rebind Interact et plusieurs actions représentées dans les tutoriels, puis afficher interaction, dialogue et tutoriel. | Les indications correspondent aux nouvelles commandes dans les trois usages. |
| Alterner clavier/souris et manette, y compris à proximité d’une interaction ou pendant un dialogue. | Les indications suivent le périphérique pertinent sans clignotement au repos. |
| Modifier les directions de déplacement ou conserver plusieurs bindings. | L’indication suit la règle documentée, avec les directions utiles et un choix stable. |
| Tester un bouton sans glyphe disponible. | Le vrai libellé apparaît si du texte est branché ; sinon l’ancienne image incorrecte est masquée et le diagnostic reste limité. |
| Restaurer les valeurs par défaut puis relancer le jeu avec une configuration sauvegardée. | Texte et icônes suivent les bindings réellement chargés dans les deux cas. |
| Rejouer le parcours menu → intro → niveau → combat → dialogue, avec un rebind clavier et un rebind manette. | Les scénarios précédemment validés restent fonctionnels et les indications sont désormais cohérentes. |

Critère de fin : les trois phases ont une revue documentée et une validation manuelle explicite ; aucun problème bloquant connu n’est laissé dans le parcours. Une configuration Unity manquante ou un test non réalisé reste indiqué comme tel.

### Tests automatisés complémentaires

Ils complètent la lecture du code et les essais manuels sans les remplacer. Utiliser la structure de tests Editor existante ; ne pas installer un framework supplémentaire ni modifier les réglages pour les tests.

Phase 1 : buffer consommé une seule fois, expiration/reset, alias TagCritical, indépendance entre deux InputPacker et absence de réarmement dans une frame. Phase 2 : état neutre pendant le rebinding et reprise après relâchement. Une petite entrée de test contrôlable suffit ; éviter une architecture de mocks disproportionnée. En phase 3, n’ajouter de tests que pour une logique nouvelle présentant un risque concret, sans dupliquer l’implémentation.

Terra distingue les vérifications réellement exécutées, la revue par lecture et les résultats rapportés par l’utilisateur. Ne pas présenter l’intégration Unity comme terminée sur la seule base de scripts compilables.

## 10. Références

- Sources locales examinées : InputPacker, ses consommateurs, ControlMapper.cs et les composants UserDataStore de l’import existant.
- [Control Mapper — installation, configuration UI et sauvegarde](https://guavaman.com/projects/rewired/docs/ControlMapper.html).
- [User Data Store — composants de persistance fournis](https://guavaman.com/projects/rewired/docs/UserDataStore.html).

La version locale de Rewired fait foi pour les signatures API lors de l’implémentation.
