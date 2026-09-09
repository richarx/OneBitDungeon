# Rebinding Rewired — plan d’implémentation

Statut : plan uniquement, aucune implémentation lancée.
Date de l’audit : 9 septembre 2026.
Exécuteur de l’implémentation : **Terra (`gpt-5.6-terra`)**, à la demande de l’utilisateur.

## 1. Contrat de travail

- Terra écrit et modifie les scripts C# ; l’utilisateur effectue les manipulations Unity.
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

## 4. Étape 1 — façade et conservation du comportement

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

## 5. Étape 2 — menu et sauvegarde

- Utiliser Control Mapper pour la capture, les conflits, l’annulation d’une capture et le retour aux commandes par défaut. Ne pas réécrire un écran de rebinding.
- Choix de persistance : composant Rewired **UserDataStore_File**, ajouté et configuré par l’utilisateur. Chargement au démarrage, sauvegarde à la fermeture normale du menu. Ne pas ajouter une deuxième sauvegarde maison.
- Dans la version installée, `ControlMapper.Close(true)` sauvegarde via `ReInput.userDataStore`. `Close(false)` ferme sans sauvegarder mais **n’annule pas** les changements appliqués en mémoire. La V1 applique les changements ; « annuler » concerne la capture en cours, pas un rollback de toute la session.
- Brancher le contrôleur sur les événements réels `ScreenOpenedEvent` / `ScreenClosedEvent`, y compris si le menu est ouvert ou fermé par ses propres commandes. Gérer aussi désactivation/destruction pour ne jamais laisser le verrou d’entrées bloqué.
- Tant que le menu est ouvert, la façade renvoie des commandes neutres, y compris les champs physiques de continuation de l’intro/menu. L’UI Rewired reste utilisable. Il faut notamment empêcher un clic sur « Contrôles » de démarrer le jeu via `MainMenu.WaitForInput`.
- Purger les buffers à l’entrée et à la sortie. Attendre le relâchement des commandes concernées avant de réactiver le gameplay : la touche de fermeture ou la touche capturée ne doit pas provoquer une roulade, une attaque ou une poursuite d’intro.
- Préserver/restaurer l’état du curseur si ce contrôleur en prend la responsabilité. Exposer des méthodes et événements simples à raccorder dans l’inspecteur.
- Cette suspension d’entrées ne constitue pas une pause de la simulation. La V1 doit être accessible depuis le menu principal, ou depuis un écran dont l’utilisateur gère déjà la pause. Ne pas ajouter un gestionnaire de pause global ni modifier les effets de ralentissement/gel existants.
- Notifier l’affichage après changement de binding, chargement, restauration des valeurs par défaut et fermeture. Rafraîchir dès que l’écran redevient visible ; ne pas simuler un changement de périphérique pour signaler un changement de touche.

## 6. Étape 3 — indications cohérentes après rebinding

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

Après les scripts, Terra fournit une checklist avec les noms exacts réellement utilisés :

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

## 9. Validation proportionnée

Terra : compilation si un environnement exploitable est disponible, contrôle du diff et tests C# ciblés sur la logique fragile. Utiliser la structure de tests Editor déjà présente ; ne pas installer un framework supplémentaire ni modifier les réglages pour les tests.

Tests prioritaires : buffer consommé une seule fois, expiration/reset, alias TagCritical, indépendance entre deux InputPacker, absence de réarmement dans une frame, état neutre pendant le rebinding et reprise après relâchement. Une petite entrée de test contrôlable suffit ; éviter une architecture de mocks disproportionnée.

Utilisateur dans Unity :

- Sans rebind, vérifier déplacement et diagonales, roulade, saut, attaque, parade, tag/critique, arrogance, assise/relevé et dialogues.
- Rebind clavier, souris et manette : la nouvelle commande agit, l’ancienne cesse d’agir si aucun binding alternatif ne la conserve.
- Se relever à la manette suit la nouvelle touche Jump ; menus d’intro continuent avec leurs commandes physiques fixes.
- Ouvrir/rebinder/fermer sans déclencher gameplay ni continuation du menu ; navigation de l’écran utilisable même après changement des commandes de combat.
- Annuler une capture, gérer un conflit, restaurer les valeurs par défaut ; vérifier les alias et recouvrements intentionnels.
- Vérifier icônes et libellés après rebinding, reset et changement clavier/manette, y compris glyphe absent.
- Fermer, relancer le jeu et changer de scène : bindings conservés ; pas de deuxième manager.
- Débrancher/rebrancher la manette ; démarrer sans manette ; vérifier absence d’erreur, de commande bloquée et de clignotement des glyphes.

Terra distingue dans sa livraison ce qui a été vérifié par les outils, ce qui reste à configurer par l’utilisateur et ce qui exige une vérification Play Mode. Ne pas présenter l’intégration Unity comme terminée sur la seule base de scripts compilables.

## 10. Références

- Sources locales examinées : InputPacker, ses consommateurs, ControlMapper.cs et les composants UserDataStore de l’import existant.
- [Control Mapper — installation, configuration UI et sauvegarde](https://guavaman.com/projects/rewired/docs/ControlMapper.html).
- [User Data Store — composants de persistance fournis](https://guavaman.com/projects/rewired/docs/UserDataStore.html).

La version locale de Rewired fait foi pour les signatures API lors de l’implémentation.
