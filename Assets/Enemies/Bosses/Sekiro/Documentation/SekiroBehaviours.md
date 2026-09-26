# Sekiro — comportements et configuration

Cette étape ajoute quatre comportements configurables par ScriptableObject, sans modifier de scène ni de prefab existant.

- `SekiroComboAttackBehaviour` / **SekiroComboAttackData** : deux attaques rapides, puis un choix pondéré : **A** prépare immédiatement son cône long (`Variant A — long cone`), **B** prépare immédiatement son cône moyen (`Variant B — medium cone then rapid`) puis exécute son attaque rapide finale.
- `SekiroCombatObservationBehaviour` / **SekiroCombatObservationData** : s'oriente, s'approche jusqu'à la distance souhaitée et termine après sa durée.
- `SekiroPursuitBehaviour` / **SekiroPursuitData** : dash borné via `EnemyMovementUtility`, puis attaque conique télégraphiée.
- `SekiroParryBehaviour` / **SekiroParryData** : poids normal `0`, donc réservé à `TryReplaceCurrentBehaviour`; il ne réussit qu'au contact réel du sabre avec son `Damageable` pendant la fenêtre. La revanche est alors facultative et conique.
- `SekiroSpawnBehaviour` / **SekiroSpawnData** : transition initiale avec cache du Rigidbody, cercle télégraphié, chute vers la position de scène, puis réactivation de la hitbox.

Les presets sont dans `Assets/Enemies/Bosses/Sekiro/Presets`. Ils référencent tous le vrai `Assets/Enemies/Damage Zones/ConeDamageZone.prefab`. Chaque comportement crée ses propres zones et son état runtime : deux ennemis peuvent partager les mêmes Data sans partager séquence, abonnement ou zone.

## Phase à configurer

Assigner `SekiroSpawnBehaviour` → `SekiroSpawn.asset` dans **Phases / transitionBehaviour** de la première phase. Il est rejoué pour chaque transition de phase à laquelle il est assigné; ne pas l'ajouter à la sélection pondérée. Il conserve la position XZ/Y configurée en scène comme sol de référence et place temporairement le Rigidbody au-dessus de celle-ci.

Dans la liste de comportements d'une phase, créer les trois objets inline suivants et assigner leurs Data :

1. `SekiroComboAttackBehaviour` → `SekiroComboAttack.asset`.
2. `SekiroCombatObservationBehaviour` → `SekiroCombatObservation.asset`.
3. `SekiroPursuitBehaviour` → `SekiroPursuit.asset`.

Ne pas mettre `SekiroParryBehaviour` dans la sélection normale : son preset a **Normal weight = 0**. `SekiroComboAttackData` et `SekiroCombatObservationData` doivent obligatoirement recevoir les références **Parry data = SekiroParry.asset** et **Pursuit data = SekiroPursuit.asset**; ils instancient une réaction par ennemi quand elle est valable. Le combo ouvre la réaction uniquement pendant ses deux rapides. Observation est prête pendant sa durée. Les deux exigent que `PlayerTargeting.Target` soit l'ennemi, sa racine ou un de ses enfants, que le joueur soit assez proche et regarde suffisamment vers lui. Le ciblage courant du projet est le plus proche ennemi, même sans verrou manuel.

Pour forcer un essai de branche, mettre **Variant A — long cone / Selection weight** à `1` et **Variant B — medium cone then rapid / Selection weight** à `0`; inverser les valeurs pour B. Le preset A a un `Fill` de `1.3 s`; B a `0.6 s` puis une rapide à `0.25 s`.

## Tempo et zones des presets

Les noms d'animation et les valeurs `Spawn`/`Fill` restent inchangés. Les pauses utilisent les `Recovery` existants : rapide 1 `0.15 → 0.35 s`, rapide 2 `0.15 → 0.40 s`, A `0.45 → 0.55 s`, cône moyen B `0.20 → 0.45 s`, finale B `0.25 → 0.40 s`, dash `0.25 → 0.35 s`. La revanche conserve son tempo rapide.

Les rayons montent d'environ 30 % : rapides/finale `2.5 → 3.25 m`, A `4 → 5.2 m`, B `3.5 → 4.55 m`, dash `2.8 → 3.65 m`, revanche `2.4 → 3.1 m`. Angles, seuils de réaction et distances de poursuite restent inchangés.

Les trois frappes rapides du Combo (rapide 1, rapide 2 et finale B) utilisent désormais une **Rectangle zone prefab** assignable dans chaque étape, avec **Width** et **Length**. Le preset pointe vers `Assets/Enemies/Bosses/Biscotto/Dummy/RotateRectangleDamageZone 1.prefab`, longueur `3.25 m` et largeur `1.8 m`; remplacer librement ce prefab par un autre qui contient `RectangleDamageZone`. Les cônes A/B, le cône de poursuite et la revanche restent coniques. Les récupérations du preset sont plus posées : rapide 1 `0.35 → 0.55 s`, rapide 2 `0.40 → 0.60 s`, A `0.55 → 0.70 s`, B `0.45 → 0.60 s`, finale B `0.40 → 0.60 s`.

`SekiroSpawn.asset` configure **Intro / Disable hitbox during intro**, **Intro / Hide sprite during delay**, **Entrance / Use vertical entrance**, **Entrance / Entrance height**, **Entrance / Fall duration**, **Spawn zone / Spawn circle enabled**, **Circle zone prefab**, **Radius**, **Spawn duration**, **Fill duration**, puis **Timing** et **Animation**. Le preset utilise `Biscotto/Dummy/DummyCircleDamageZone.prefab` (GUID `80e2045c1640a1d40917d4872dbd38d7`, composant `5892917051957619975`), hauteur `8 m`, cercle `4 m`, `0.25 + 0.5 s`, chute `0.35 s`. **Intro duration without circle** (`0.75 s`) ne sert que si le cercle est désactivé ou absent; avec un cercle, sa durée `Spawn + Fill + 0.05 s` (transition visuelle interne du cercle avant collision) règle l’instant d’atterrissage pour que les dégâts ne précèdent jamais l’impact visible. Ses chaînes d'animation restent vides à assigner.

## Animations

Le pipeline ennemi constaté est `enemy.animator.Play(string)`. Il n'y a ni trigger Animator dédié ni clips/contrôleur Sekiro fournis. Les chaînes suivantes doivent être renseignées avec les **noms d'états réels** du futur Animator Sekiro :

- Parry Data : **Start**, **Success**, **Recovery**, et l'animation de préparation de **Revenge / Attack**.
- Pursuit Data : **Dash**, puis **Dash attack / Preparation animation** et **Impact animation**.
- Combo Data : **Rapid attack 1 / Preparation animation**, **Rapid attack 2 / Preparation animation**, `Variant A — long cone / Preparation animation`, `Variant B — medium cone then rapid / Preparation animation`, et `Final rapid attack / Preparation animation`. Les deux préparations de branches sont jouées dès la séparation, avant le télégraphe conique. Chaque étape peut aussi jouer son **Impact animation** quand son remplissage atteint l'impact.
- Observation Data : **Observation**.

Les champs sont volontairement vides dans les presets : aucun nom Sekiro n'est inventé. Les timings sont indépendants des Animation Events : `Spawn` + `Fill` déclenchent l'impact du cône et l'`Impact animation` si renseignée.

## Chronologie exacte

`ConeDamageZone` rend le télégraphe pendant **Spawn**, remplit pendant **Fill**, puis seulement après ces deux durées appelle sa vérification de collision et `DealDamageToPlayer.TryDealDamage`. Les comportements jouent l'animation de préparation au début de **Spawn** et l'**Impact animation** au même seuil `Spawn + Fill` : elle coïncide donc avec l'activation réelle de dégâts, et non avec le début du télégraphe.

- Rapide Combo : préparation, `Spawn 0.1 s + Fill 0.25 s`, impact, récupération `0.35 s` puis `0.40 s`; la fenêtre de remplacement vers Parade dure `Rapid parry window = 0.45 s` au départ de chaque rapide.
- Variante A : préparation A dès la séparation, `Spawn 0.2 s + Fill 1.3 s`, impact, récupération `0.55 s`.
- Variante B : préparation B dès la séparation, `Spawn 0.15 s + Fill 0.6 s`, impact, récupération `0.45 s`, puis rapide finale `0.1 s + 0.25 s`, impact et récupération `0.40 s`.
- Parade : fenêtre `0.35 s`. Après un contact réellement dévié, attente de revanche `0.05 s`, préparation de revanche, `0.1 s + 0.2 s` avant impact, récupération de l'étape `0.15 s`, puis récupération finale `0.25 s`.
- Poursuite : active à `4 m`, vise `2 m`, se limite à `5 m` de dash durant `0.35 s`, puis son cône suit `Spawn 0.1 s + Fill 0.25 s` avant impact et récupération `0.35 s`.
- Spawn : désactive hitbox, place le Rigidbody à `8 m`, masque optionnellement pendant `0.5 s`, joue **Spawn** et crée le cercle. Le cercle télégraphie `0.25 s`, remplit `0.5 s`, effectue sa transition pleine `0.05 s`, puis active ses dégâts à `0.80 s`; la chute commence à `0.45 s` et rejoint le sol après `0.35 s`, exactement au même instant. Le comportement joue alors **Impact**, réactive la hitbox, joue **Recovery** puis attend `0.35 s` avant `Complete`. Sans cercle, **Intro duration without circle** définit cette durée de préparation.

## Parade réelle

`Damageable.TryTakeDamage` appelle les intercepteurs locaux avant santé, `OnTakeDamage` ou `OnDie`. Il renvoie `false` lorsqu'un intercepteur consomme le coup ; `WeaponDamageTrigger` ajoute tout de même la cible au swing courant, mais ne publie pas `OnHitEnemy`. Les effets de hit, l'humilité et les dégâts sont donc absents pour ce coup paré. `SekiroParryBehaviour` enregistre alors `EnemyExchangeResult.PlayerDeflected`, qui signifie ici **coup du joueur dévié par l'ennemi**, pas le succès de la parade du joueur.

La revanche n'est jamais déclenchée au signal anticipé `PlayerAttack.OnPlayerAttack` seul : ce signal ne sert qu'à remplacer Combo/Observation par une parade. Il faut ensuite que le coup atteigne réellement l'ennemi pendant la fenêtre. Un coup qui ne touche pas laisse la parade finir sans revanche.

Les attaques coniques sont configurées par étape avec **Player can parry** et **Player can jump**, transmis à `DealDamageToPlayer.Configure` avant leur impact. La parade ennemie accepte toutes les attaques joueur `AttackPayload` sauf `Critical` par défaut; **Can parry critical attacks** l'autorise explicitement.

## Vérifications manuelles

1. Assigner des états Animator réels, puis configurer la phase ci-dessus. Vérifier les deux rapides, puis A : cône à remplissage long; B : cône moyen puis rapide finale. Vérifier que les préparations A/B se distinguent dès le début de leurs cônes.
2. Pendant une rapide, attaquer l'ennemi ciblé à portée : Combo est annulé, Parry démarre. Manquer le coup : aucune revanche. Toucher durant la fenêtre : ni santé ni effet de coup subi ne changent, puis la revanche apparaît.
3. Attaquer un autre ennemi ou attaquer hors portée / dans le dos : aucune parade Sekiro ne démarre.
4. Éloigner le joueur au point autorisé entre étapes : Combo ou Observation est remplacé par Pursuit; vérifier déplacement borné, cône de dash, puis retour à la sélection normale.
5. Laisser Observation s'exécuter sans événement : il avance jusqu'à la distance et termine après **Duration**.
6. Annuler, changer de phase, tuer ou désactiver l'ennemi pendant chaque comportement : zones disparaissent, dash/séquences et abonnements s'arrêtent. Avec deux ennemis, vérifier que seule la cible pertinente réagit et que leurs zones restent séparées.
7. Configurer `SekiroSpawnBehaviour` comme transition initiale. Vérifier que le boss tombe sur sa position placée au même instant que l’impact réel du cercle, puis que sa hitbox revient avant la sélection normale. Annuler l'intro, changer de phase ou désactiver le boss : le Rigidbody revient au sol, le cercle est annulé et les renderers reviennent exactement à leur état initial; la hitbox reste sous le contrôle de l’état suivant, afin de ne pas réactiver le combat pendant une mort ou transition.

## Recul du joueur après une attaque déviée

Une interception réelle du coup déclenche `PlayerDeflected` : l'attaque est interrompue,
sa hitbox est désactivée immédiatement et le joueur recule légèrement. Aucun dégât ni
invulnérabilité de blessure ne sont appliqués. Une attaque dans le vide ne déclenche rien.

Dans `KnightData` et `ThiefData`, le groupe **Attack deflected** règle :

- **Deflected Defence Recovery** : `0.10 s` avant de pouvoir parer, rouler, sauter ou faire un spin (mode arrogance + roulade).
- **Deflected Attack Recovery** : `0.55 s` avant de pouvoir attaquer à nouveau, même après une sortie anticipée vers une défense.
- **Deflected Recoil Speed / Deceleration** : `2 / 20`, pour un déplacement très court.
- **Deflected Animation** : `Hurt`, utilisant les sprites avec épée en main du personnage actif, dans la direction de l'adversaire. Les planches Knight et Thief contiennent déjà trois images de recul par direction.

Les commandes d'attaque reçues pendant la récupération sont oubliées. Les commandes
défensives conservent leur buffer habituel. Le stagger de dégâts reste un état distinct.

`PlayerVfx` réutilise **Spark Prefab** pour afficher une petite étincelle entre les
combattants, avec la courte pause de parade existante. Sa taille se règle dans
**Parry / Deflected spark scale** (défaut `0.8`).

Le preset `SekiroParry` active la revanche. Son animation d'attaque est déclenchée à
l'impact prévu par la zone, en incluant la fenêtre d'esquive et la transition de couleur.
La fenêtre de spin du cône utilise désormais ce même instant. La préparation ne relance
plus une première animation d'attaque prématurée. Une revanche désactivée ne verrouille
plus à tort le réarmement de la parade.
