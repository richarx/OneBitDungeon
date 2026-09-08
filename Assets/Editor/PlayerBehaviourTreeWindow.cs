using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Player.Scripts;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Visualise les transitions effectivement déclarées dans les scripts de behaviour du joueur.
/// Ouvrir depuis le menu contextuel du composant PlayerStateMachine.
/// </summary>
public sealed class PlayerBehaviourTreeWindow : EditorWindow
{
    private const float NodeWidth = 170f;
    private const float NodeHeight = 82f;
    private const float DetailsWidth = 365f;

    private static readonly Dictionary<BehaviourType, string> ScriptNames = new Dictionary<BehaviourType, string>
    {
        { BehaviourType.Idle, "PlayerIdle" },
        { BehaviourType.ArrogantIdle, "PlayerArrogantIdle" },
        { BehaviourType.Run, "PlayerRun" },
        { BehaviourType.ArrogantRun, "PlayerArrogantRun" },
        { BehaviourType.Roll, "PlayerRoll" },
        { BehaviourType.ArrogantSpin, "PlayerArrogantSpin" },
        { BehaviourType.Jump, "PlayerJump" },
        { BehaviourType.JumpTag, "PlayerJumpTag" },
        { BehaviourType.Attack, "PlayerAttack" },
        { BehaviourType.CriticalAttack, "PlayerCriticalAttack" },
        { BehaviourType.Stagger, "PlayerStagger" },
        { BehaviourType.Parry, "PlayerParry" },
        { BehaviourType.Sit, "PlayerSit" },
        { BehaviourType.Dead, "PlayerDead" },
        { BehaviourType.Locked, "PlayerLocked" },
        { BehaviourType.Tag, "PlayerTag" },
        { BehaviourType.JumpAttack, "PlayerJumpAttack" },
        { BehaviourType.CounterAttack, "PlayerCounterAttack" },
        { BehaviourType.Taunt, "PlayerTaunt" }
    };

    private static readonly Dictionary<string, BehaviourType> BehaviourFields = new Dictionary<string, BehaviourType>
    {
        { "playerIdle", BehaviourType.Idle },
        { "playerArrogantIdle", BehaviourType.ArrogantIdle },
        { "playerRun", BehaviourType.Run },
        { "playerArrogantRun", BehaviourType.ArrogantRun },
        { "playerRoll", BehaviourType.Roll },
        { "playerArrogantSpin", BehaviourType.ArrogantSpin },
        { "playerJump", BehaviourType.Jump },
        { "playerJumpTag", BehaviourType.JumpTag },
        { "playerAttack", BehaviourType.Attack },
        { "playerCriticalAttack", BehaviourType.CriticalAttack },
        { "playerStagger", BehaviourType.Stagger },
        { "playerParry", BehaviourType.Parry },
        { "playerSit", BehaviourType.Sit },
        { "playerDead", BehaviourType.Dead },
        { "playerLocked", BehaviourType.Locked },
        { "playerTag", BehaviourType.Tag },
        { "playerJumpAttack", BehaviourType.JumpAttack },
        { "playerCounterAttack", BehaviourType.CounterAttack },
        { "playerTaunt", BehaviourType.Taunt }
    };

    private readonly Dictionary<BehaviourType, Rect> nodeRects = new Dictionary<BehaviourType, Rect>();
    private readonly List<Transition> transitions = new List<Transition>();
    private Vector2 graphScroll;
    private Vector2 detailsScroll;
    private BehaviourType selectedBehaviour = BehaviourType.Idle;
    private PlayerStateMachine player;
    private BehaviourType? draggedBehaviour;
    private Vector2 dragOffset;

    [MenuItem("CONTEXT/PlayerStateMachine/Show Player Behaviour Tree")]
    private static void ShowFromComponent(MenuCommand command)
    {
        ShowWindow(command.context as PlayerStateMachine);
    }

    [MenuItem("Tools/Player/Show Player Behaviour Tree")]
    private static void ShowFromToolsMenu()
    {
        ShowWindow(Selection.activeGameObject == null ? null : Selection.activeGameObject.GetComponent<PlayerStateMachine>());
    }

    private static void ShowWindow(PlayerStateMachine stateMachine)
    {
        PlayerBehaviourTreeWindow window = GetWindow<PlayerBehaviourTreeWindow>("Player Behaviour Tree");
        window.player = stateMachine;
        window.minSize = new Vector2(1000f, 650f);
        window.Refresh();
        window.Show();
    }

    private void OnEnable()
    {
        BuildNodeLayout();
        Refresh();
    }

    private void Refresh()
    {
        BuildNodeLayout();
        transitions.Clear();

        foreach (KeyValuePair<BehaviourType, string> behaviour in ScriptNames)
            ReadTransitions(behaviour.Key, behaviour.Value);

        // Les deux helpers de PlayerStateMachine cachent une transition derrière un booléen.
        AddHelperTransitions();
        AddInitialTransition();
        Repaint();
    }

    private void BuildNodeLayout()
    {
        nodeRects.Clear();
        AddNode(BehaviourType.Idle, 50f, 80f);
        AddNode(BehaviourType.Run, 50f, 225f);
        AddNode(BehaviourType.ArrogantIdle, 280f, 80f);
        AddNode(BehaviourType.ArrogantRun, 280f, 225f);
        AddNode(BehaviourType.Roll, 510f, 20f);
        AddNode(BehaviourType.ArrogantSpin, 510f, 145f);
        AddNode(BehaviourType.Jump, 510f, 270f);
        AddNode(BehaviourType.Parry, 740f, 20f);
        AddNode(BehaviourType.Attack, 740f, 145f);
        AddNode(BehaviourType.CriticalAttack, 740f, 270f);
        AddNode(BehaviourType.JumpAttack, 970f, 75f);
        AddNode(BehaviourType.CounterAttack, 970f, 200f);
        AddNode(BehaviourType.Sit, 50f, 430f);
        AddNode(BehaviourType.Taunt, 280f, 430f);
        AddNode(BehaviourType.Locked, 510f, 430f);
        AddNode(BehaviourType.Stagger, 740f, 430f);
        AddNode(BehaviourType.Dead, 970f, 400f);
        AddNode(BehaviourType.Tag, 970f, 525f);
        AddNode(BehaviourType.JumpTag, 1200f, 525f);
    }

    private void AddNode(BehaviourType type, float x, float y)
    {
        string key = GetNodeKey(type);
        Vector2 position = new Vector2(
            EditorPrefs.GetFloat(key + ".x", x),
            EditorPrefs.GetFloat(key + ".y", y));
        nodeRects[type] = new Rect(position.x, position.y, NodeWidth, NodeHeight);
    }

    private void ReadTransitions(BehaviourType source, string scriptName)
    {
        string assetPath = FindScriptPath(scriptName);
        if (string.IsNullOrEmpty(assetPath))
            return;

        string sourceText = StripComments(File.ReadAllText(ToFullPath(assetPath)));
        MatchCollection calls = Regex.Matches(sourceText, @"player\.ChangeBehaviour\((?<argument>.*?)\);", RegexOptions.Singleline);

        foreach (Match call in calls)
        {
            string argument = call.Groups["argument"].Value;
            MatchCollection fields = Regex.Matches(argument, @"player\.(?<field>player\w+)");
            foreach (Match field in fields)
            {
                BehaviourType destination;
                if (!BehaviourFields.TryGetValue(field.Groups["field"].Value, out destination))
                    continue;

                string rawCondition = GetContainingConditions(sourceText, call.Index);
                string helperConditions = GetHelperCallConditions(sourceText, call.Index);
                if (!string.IsNullOrEmpty(helperConditions))
                {
                    rawCondition = rawCondition == "Déclenché par le code"
                        ? helperConditions
                        : rawCondition + " && (" + helperConditions + ")";
                }

                string condition = DescribeCondition(rawCondition);
                TransitionTrigger trigger = string.IsNullOrEmpty(helperConditions)
                    ? GetTransitionTrigger(rawCondition)
                    : GetTransitionTrigger(helperConditions);
                AddTransition(source, destination, condition, assetPath, trigger);
            }
        }
    }

    private void AddHelperTransitions()
    {
        foreach (KeyValuePair<BehaviourType, string> behaviour in ScriptNames)
        {
            string path = FindScriptPath(behaviour.Value);
            if (string.IsNullOrEmpty(path))
                continue;

            string sourceText = StripComments(File.ReadAllText(ToFullPath(path)));
            if (sourceText.Contains("player.TryStartAttack()"))
                AddTransition(behaviour.Key, BehaviourType.Attack, "Clic gauche + attaque autorisée", path, TransitionTrigger.Input);

            if (sourceText.Contains("player.TryStartCriticalAttack()"))
                AddTransition(behaviour.Key, BehaviourType.CriticalAttack, "Tab + attaque critique autorisée", path, TransitionTrigger.Input);
        }
    }

    private void AddInitialTransition()
    {
        AddTransition(BehaviourType.Idle, BehaviourType.Idle, "État initial au démarrage", "Assets/Player/Scripts/Systems/PlayerStateMachine.cs", TransitionTrigger.Code);
    }

    private void AddTransition(BehaviourType source, BehaviourType destination, string condition, string assetPath, TransitionTrigger trigger = TransitionTrigger.Code)
    {
        if (transitions.Any(t => t.source == source && t.destination == destination && t.condition == condition))
            return;

        transitions.Add(new Transition(source, destination, condition, assetPath, trigger));
    }

    private static string FindScriptPath(string scriptName)
    {
        string[] guids = AssetDatabase.FindAssets(scriptName + " t:MonoScript");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == scriptName)
                return path;
        }

        return string.Empty;
    }

    private static string ToFullPath(string assetPath)
    {
        string projectPath = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(projectPath, assetPath);
    }

    private static string StripComments(string text)
    {
        text = Regex.Replace(text, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
        return Regex.Replace(text, @"//.*?$", string.Empty, RegexOptions.Multiline);
    }

    private static string GetContainingConditions(string sourceText, int index)
    {
        List<string> conditions = new List<string>();
        MatchCollection ifStatements = Regex.Matches(sourceText.Substring(0, index), @"\bif\s*\(");

        foreach (Match ifStatement in ifStatements)
        {
            int openParenthesis = sourceText.IndexOf('(', ifStatement.Index);
            int closeParenthesis = FindMatchingCharacter(sourceText, openParenthesis, '(', ')');
            if (closeParenthesis < 0)
                continue;

            int bodyStart = closeParenthesis + 1;
            while (bodyStart < sourceText.Length && char.IsWhiteSpace(sourceText[bodyStart]))
                bodyStart++;

            int bodyEnd;
            if (bodyStart < sourceText.Length && sourceText[bodyStart] == '{')
            {
                bodyEnd = FindMatchingCharacter(sourceText, bodyStart, '{', '}');
            }
            else
            {
                // Un if sans accolades ne contient que son instruction immédiate,
                // pas son éventuel bloc else.
                bodyEnd = sourceText.IndexOf(';', bodyStart);
            }

            if (bodyEnd >= index)
                conditions.Add(sourceText.Substring(openParenthesis + 1, closeParenthesis - openParenthesis - 1));
        }

        return conditions.Count > 0 ? string.Join(" && ", conditions) : "Déclenché par le code";
    }

    private static int FindMatchingCharacter(string text, int openingIndex, char openingCharacter, char closingCharacter)
    {
        int depth = 0;
        for (int i = openingIndex; i < text.Length; i++)
        {
            if (text[i] == openingCharacter)
                depth++;
            else if (text[i] == closingCharacter)
            {
                depth--;
                if (depth == 0)
                    return i;
            }
        }

        return -1;
    }

    private static string GetHelperCallConditions(string sourceText, int transitionIndex)
    {
        string methodName = GetEnclosingMethodName(sourceText, transitionIndex);
        if (string.IsNullOrEmpty(methodName))
            return string.Empty;

        List<string> conditions = new List<string>();
        MatchCollection calls = Regex.Matches(sourceText, @"\b" + Regex.Escape(methodName) + @"\s*\(\s*player\s*\)");
        foreach (Match call in calls)
        {
            string condition = GetContainingConditions(sourceText, call.Index);
            if (condition != "Déclenché par le code")
                conditions.Add(condition);
        }

        return conditions.Count > 0 ? string.Join(" || ", conditions.Distinct()) : string.Empty;
    }

    private static string GetEnclosingMethodName(string sourceText, int index)
    {
        const string MethodPattern = @"\b(?:public|private|protected|internal)\s+(?:static\s+)?[\w<>\[\],]+\s+(?<name>\w+)\s*\([^)]*\)\s*\{";
        MatchCollection methods = Regex.Matches(sourceText, MethodPattern);
        string result = string.Empty;

        foreach (Match method in methods)
        {
            int bodyStart = method.Index + method.Length - 1;
            int bodyEnd = FindMatchingCharacter(sourceText, bodyStart, '{', '}');
            if (bodyStart <= index && bodyEnd >= index)
                result = method.Groups["name"].Value;
        }

        return result;
    }

    private static string DescribeCondition(string condition)
    {
        string description = Regex.Replace(condition, @"\s+", " ").Trim();
        if (description == "Déclenché par le code")
            return description;

        description = description
            .Replace("player.inputPackage.GetRoll.WasPressedWithBuffer()", "Espace")
            .Replace("player.inputPackage.GetJump.WasPressedWithBuffer()", "Maj gauche")
            .Replace("player.inputPackage.GetAttack.WasPressedWithBuffer()", "Clic gauche")
            .Replace("player.inputPackage.GetAttack.wasPressedThisFrame", "Clic gauche")
            .Replace("player.inputPackage.GetCriticalAttack.WasPressedWithBuffer()", "Tab")
            .Replace("player.inputPackage.GetParry.WasPressedWithBuffer()", "Clic droit")
            .Replace("player.inputPackage.GetParry.wasPressedThisFrame", "Clic droit")
            .Replace("player.inputPackage.GetSitDown.wasPressedThisFrame", "C (s'asseoir)")
            .Replace("player.inputPackage.GetTaunt.wasPressedThisFrame", "Q / A")
            .Replace("player.inputPackage.GetArroganceMode.isPressed", "molette maintenue")
            .Replace("player.moveInput.magnitude >= 0.15f", "ZQSD / WASD")
            .Replace("player.moveInput.magnitude < 0.15f", "aucune direction")
            .Replace("player.playerRoll.CanRoll(player)", "roulade disponible")
            .Replace("player.playerJump.CanJump(player)", "saut disponible")
            .Replace("player.playerParry.CanParry(player)", "parade disponible")
            .Replace("player.playerArrogantSpin.CanSpin(player)", "spin disponible")
            .Replace("Time.time - rollStartTimestamp >= player.playerData.rollMaxDuration", "durée max de roulade atteinte")
            .Replace("Vector3.Distance(rollStartPosition, player.position) >= player.playerData.rollMaxDistance", "distance max de roulade atteinte")
            .Replace("Time.time - jumpStartTimestamp >= player.playerData.jumpMaxDuration", "durée max de saut atteinte")
            .Replace("Vector3.Distance(spinStartPosition, player.position) >= player.playerData.spinMaxDistance", "distance max de spin atteinte")
            .Replace("Time.time - attackStartTimestamp >= player.playerData.attackDuration", "attaque terminée")
            .Replace("Time.time - attackStartTimestamp >= player.playerData.jumpAttackDuration", "attaque sautée terminée")
            .Replace("Time.time - attackStartTimestamp >= player.playerData.counterAttackDuration", "contre terminé")
            .Replace("Time.time - startStaggerTimestamp >= player.playerData.staggerDuration", "stagger terminé");

        return description;
    }

    private static TransitionTrigger GetTransitionTrigger(string rawCondition)
    {
        bool requiresTime = rawCondition.Contains("Time.time") || rawCondition.Contains("isRecoveryOver");
        bool requiresInput = rawCondition.Contains("inputPackage") || rawCondition.Contains("moveInput");

        if (requiresTime && requiresInput)
            return TransitionTrigger.TimeAndInput;

        if (requiresTime)
            return TransitionTrigger.Time;

        if (requiresInput)
            return TransitionTrigger.Input;

        return TransitionTrigger.Code;
    }

    private void OnGUI()
    {
        DrawToolbar();

        Rect graphArea = new Rect(0f, 22f, Mathf.Max(300f, position.width - DetailsWidth), position.height - 22f);
        Rect detailsArea = new Rect(graphArea.xMax, 22f, DetailsWidth, position.height - 22f);
        DrawGraph(graphArea);
        DrawDetails(detailsArea);
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Player Behaviour Tree", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Actualiser depuis les scripts", EditorStyles.toolbarButton, GUILayout.Width(180f)))
            Refresh();
        if (GUILayout.Button("Réinitialiser le visuel", EditorStyles.toolbarButton, GUILayout.Width(145f)))
            ResetLayout();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawGraph(Rect area)
    {
        GUI.Box(area, GUIContent.none, EditorStyles.helpBox);
        GUILayout.BeginArea(area);
        graphScroll = GUI.BeginScrollView(new Rect(0f, 0f, area.width, area.height), graphScroll, new Rect(0f, 0f, 1420f, 660f));
        DrawConnections();

        HandleNodeDrag();
        foreach (KeyValuePair<BehaviourType, Rect> node in nodeRects)
        {
            GUIStyle style = new GUIStyle(EditorStyles.helpBox)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                fontStyle = node.Key == selectedBehaviour ? FontStyle.Bold : FontStyle.Normal
            };

            GUI.Box(node.Value, BuildNodeLabel(node.Key), style);
        }

        GUI.EndScrollView();
        GUILayout.EndArea();
    }

    private string BuildNodeLabel(BehaviourType behaviour)
    {
        IEnumerable<string> keys = transitions
            .Where(t => t.destination == behaviour)
            .SelectMany(t => ExtractKeys(t.condition))
            .Distinct();

        string keyText = string.Join(", ", keys);
        if (string.IsNullOrEmpty(keyText))
            keyText = "automatique / code";

        int timedTransitions = transitions.Count(t => t.source == behaviour && t.trigger == TransitionTrigger.Time);
        int combinedTransitions = transitions.Count(t => t.source == behaviour && t.trigger == TransitionTrigger.TimeAndInput);
        string timeText = timedTransitions > 0 ? "\nFin auto : temps (" + timedTransitions + ")" : string.Empty;
        if (combinedTransitions > 0)
            timeText += "\nTemps + input : " + combinedTransitions;
        return behaviour + "\nEntrée : " + keyText + timeText;
    }

    private static IEnumerable<string> ExtractKeys(string condition)
    {
        string[] knownKeys = { "Espace", "Maj gauche", "Clic gauche", "Clic droit", "Tab", "C (s'asseoir)", "Q / A", "molette maintenue", "ZQSD / WASD" };
        return knownKeys.Where(condition.Contains);
    }

    private void DrawConnections()
    {
        Handles.BeginGUI();

        List<Transition> highlightedTransitions = transitions.Where(t => t.source == selectedBehaviour).ToList();
        List<Transition> incomingTransitions = transitions.Where(t => t.destination == selectedBehaviour).ToList();

        // Les sorties du nœud sélectionné sont dessinées en dernier afin qu'aucune
        // liaison en arrière-plan ne puisse les recouvrir.
        foreach (Transition transition in transitions.OrderBy(t => t.source == selectedBehaviour ? 1 : 0))
        {
            Rect source;
            Rect destination;
            if (!nodeRects.TryGetValue(transition.source, out source) || !nodeRects.TryGetValue(transition.destination, out destination))
                continue;

            // La sélection ne met en avant que ce que l'état peut déclencher,
            // pas les flèches qui arrivent vers lui.
            bool isHighlighted = highlightedTransitions.Contains(transition);
            bool isIncoming = incomingTransitions.Contains(transition);
            if (transition.source == selectedBehaviour || transition.destination == selectedBehaviour)
                Debug.Log("Transition from " + transition.source + " to " + transition.destination + " is highlighted: " + isHighlighted);
            Color color = GetTransitionColor(transition.trigger, isHighlighted, isIncoming);
            Color glowColor = GetGlowColor(transition.trigger);
            float lineWidth = isHighlighted ? 3.5f : 2f;

            if (transition.source == transition.destination)
            {
                Vector3 loopStart = new Vector3(source.xMax - 20f, source.yMin + 8f);
                Vector3 loopEnd = new Vector3(source.xMax - 8f, source.yMin + 26f);
                if (isHighlighted)
                    Handles.DrawBezier(loopStart, loopEnd, loopStart + Vector3.right * 48f, loopEnd + Vector3.right * 48f, glowColor, null, 8f);
                Handles.DrawBezier(loopStart, loopEnd, loopStart + Vector3.right * 48f, loopEnd + Vector3.right * 48f, color, null, lineWidth);
                continue;
            }

            Endpoint startEndpoint = GetEndpoint(transition, "out", source, destination.center);
            Endpoint endEndpoint = GetEndpoint(transition, "in", destination, source.center);
            Vector3 start = startEndpoint.position + GetSharedEndpointOffset(transition, "out", startEndpoint);
            Vector3 end = endEndpoint.position + GetSharedEndpointOffset(transition, "in", endEndpoint);
            bool hasReverseTransition = HasReverseTransition(transition);
            if (hasReverseTransition)
            {
                Vector3 endpointOffset = GetReciprocalLaneOffset(start, end).normalized * 8f;
                start += endpointOffset;
                end += endpointOffset;
            }

            float handleLength = Mathf.Max(45f, Vector3.Distance(start, end) * 0.35f);

            Vector3 startTangent;
            Vector3 endTangent;

            if (startEndpoint.side == AnchorSide.Left || startEndpoint.side == AnchorSide.Right)
            {
                startTangent = new Vector3(startEndpoint.normal.x * handleLength, startEndpoint.normal.y * handleLength);
                endTangent = new Vector3(endEndpoint.normal.x * handleLength, endEndpoint.normal.y * handleLength);
            }
            else
            {
                startTangent = new Vector3(-startEndpoint.normal.x * handleLength, -startEndpoint.normal.y * handleLength);
                endTangent = new Vector3(-endEndpoint.normal.x * handleLength, -endEndpoint.normal.y * handleLength);
            }

            Vector3 laneOffset = hasReverseTransition ? GetReciprocalLaneOffset(start, end) : Vector3.zero;
            if (isHighlighted)
                Handles.DrawBezier(start, end, start + startTangent + laneOffset, end + endTangent + laneOffset, glowColor, null, 8f);
            Handles.DrawBezier(start, end, start + startTangent + laneOffset, end + endTangent + laneOffset, color, null, lineWidth);
            DrawArrow(end, -endTangent - laneOffset, color, isHighlighted ? 12f : 8f);
        }
        Handles.EndGUI();
    }

    private bool HasReverseTransition(Transition transition)
    {
        return transitions.Any(t =>
            t.source == transition.destination &&
            t.destination == transition.source &&
            t.source != t.destination);
    }

    private static Vector3 GetReciprocalLaneOffset(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized;
        return new Vector3(-direction.y, direction.x) * 26f;
    }

    private Vector3 GetSharedEndpointOffset(Transition transition, string endpointName, Endpoint endpoint)
    {
        List<Transition> sharedTransitions = new List<Transition>();
        foreach (Transition candidate in transitions)
        {
            BehaviourType nodeBehaviour = endpointName == "out" ? candidate.source : candidate.destination;
            BehaviourType otherBehaviour = endpointName == "out" ? candidate.destination : candidate.source;
            BehaviourType expectedNode = endpointName == "out" ? transition.source : transition.destination;
            if (nodeBehaviour != expectedNode || candidate.source == candidate.destination)
                continue;

            Rect nodeRect = nodeRects[nodeBehaviour];
            Endpoint candidateEndpoint = GetEndpoint(candidate, endpointName, nodeRect, nodeRects[otherBehaviour].center);
            if (Vector3.Distance(candidateEndpoint.position, endpoint.position) < 0.01f)
                sharedTransitions.Add(candidate);
        }

        if (sharedTransitions.Count <= 1)
            return Vector3.zero;

        sharedTransitions.Sort(CompareTransitions);
        int index = sharedTransitions.FindIndex(t => IsSameTransition(t, transition));
        float centeredIndex = index - (sharedTransitions.Count - 1) * 0.5f;
        Vector3 alongNodeEdge = new Vector3(-endpoint.normal.y, endpoint.normal.x).normalized;
        return alongNodeEdge * centeredIndex * 9f;
    }

    private static int CompareTransitions(Transition first, Transition second)
    {
        int sourceComparison = first.source.CompareTo(second.source);
        if (sourceComparison != 0)
            return sourceComparison;

        int destinationComparison = first.destination.CompareTo(second.destination);
        if (destinationComparison != 0)
            return destinationComparison;

        return string.CompareOrdinal(first.condition, second.condition);
    }

    private static bool IsSameTransition(Transition first, Transition second)
    {
        return first.source == second.source &&
            first.destination == second.destination &&
            first.condition == second.condition;
    }

    private static Color GetTransitionColor(TransitionTrigger trigger, bool isHighlighted, bool isIncoming)
    {
        Color baseColor = GetTransitionBaseColor(trigger);
        return new Color(baseColor.r, baseColor.g, baseColor.b, isHighlighted ? 1f : (isIncoming ? 0.4f : 0.1f));
    }

    private static Color GetGlowColor(TransitionTrigger trigger)
    {
        Color baseColor = GetTransitionBaseColor(trigger);
        return new Color(baseColor.r, baseColor.g, baseColor.b, 0.22f);
    }

    private static Color GetTransitionBaseColor(TransitionTrigger trigger)
    {
        switch (trigger)
        {
            case TransitionTrigger.Time:
                return new Color(1f, 0.55f, 0.15f);
            case TransitionTrigger.TimeAndInput:
                return new Color(0.25f, 0.9f, 0.35f);
            case TransitionTrigger.Input:
                return new Color(0.25f, 0.75f, 1f);
            default:
                return new Color(0.6f, 0.6f, 0.6f);
        }
    }

    private Endpoint GetEndpoint(Transition transition, string endpointName, Rect rect, Vector2 other)
    {
        string key = GetTransitionKey(transition, endpointName);
        AnchorSide savedSide = (AnchorSide)EditorPrefs.GetInt(key + ".side", (int)AnchorSide.Auto);
        float offset = EditorPrefs.GetFloat(key + ".offset", 0.5f);
        AnchorSide side = savedSide == AnchorSide.Auto ? GetAutomaticSide(rect, other) : savedSide;
        return BuildEndpoint(rect, side, offset);
    }

    private static AnchorSide GetAutomaticSide(Rect rect, Vector2 other)
    {
        float xDistance = Mathf.Abs(other.x - rect.center.x);
        float yDistance = Mathf.Abs(other.y - rect.center.y);
        if (xDistance >= yDistance)
            return other.x >= rect.center.x ? AnchorSide.Right : AnchorSide.Left;

        return other.y >= rect.center.y ? AnchorSide.Bottom : AnchorSide.Top;
    }

    private static Endpoint BuildEndpoint(Rect rect, AnchorSide side, float offset)
    {
        offset = Mathf.Clamp01(offset);
        switch (side)
        {
            case AnchorSide.Left:
                return new Endpoint(new Vector3(rect.xMin, Mathf.Lerp(rect.yMin, rect.yMax, offset)), Vector3.left, side);
            case AnchorSide.Right:
                return new Endpoint(new Vector3(rect.xMax, Mathf.Lerp(rect.yMin, rect.yMax, offset)), Vector3.right, side);
            case AnchorSide.Top:
                return new Endpoint(new Vector3(Mathf.Lerp(rect.xMin, rect.xMax, offset), rect.yMin), Vector3.up, side);
            case AnchorSide.Bottom:
                return new Endpoint(new Vector3(Mathf.Lerp(rect.xMin, rect.xMax, offset), rect.yMax), Vector3.down, side);
            default:
                return new Endpoint(rect.center, Vector3.right, side);
        }
    }

    private static void DrawArrow(Vector3 end, Vector3 direction, Color color, float size)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        Vector3 perpendicular = new Vector3(-direction.y, direction.x).normalized;
        Vector3 back = end - direction.normalized * size;
        using (new Handles.DrawingScope(color))
        {
            Handles.DrawAAConvexPolygon(end, back + perpendicular * size * 0.45f, back - perpendicular * size * 0.45f);
        }
    }

    private void DrawDetails(Rect area)
    {
        GUI.Box(area, GUIContent.none, EditorStyles.helpBox);
        GUILayout.BeginArea(new Rect(area.x + 8f, area.y + 8f, area.width - 16f, area.height - 16f));
        GUILayout.Label(selectedBehaviour.ToString(), EditorStyles.boldLabel);
        GUILayout.Label("Touches et conditions des passages sortants.", EditorStyles.miniLabel);
        GUILayout.Space(6f);

        List<Transition> outgoing = transitions.Where(t => t.source == selectedBehaviour).ToList();
        detailsScroll = EditorGUILayout.BeginScrollView(detailsScroll);
        if (outgoing.Count == 0)
        {
            EditorGUILayout.HelpBox("Aucune transition trouvée dans le script de ce behaviour.", MessageType.Info);
        }
        else
        {
            foreach (Transition transition in outgoing)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label("→ " + transition.destination, EditorStyles.boldLabel);
                GUILayout.Label(GetTriggerLabel(transition.trigger), EditorStyles.miniBoldLabel);
                GUILayout.Label(transition.condition, EditorStyles.wordWrappedLabel);
                if (transition.source != transition.destination)
                    DrawEndpointControls(transition);
                if (GUILayout.Button("Ouvrir le script", EditorStyles.miniButton))
                    OpenScript(transition.assetPath);
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Space(6f);
        EditorGUILayout.HelpBox("Bleu : input. Orange : temps. Vert : temps + input. Les sorties de l'état sélectionné deviennent vives, épaisses et sont dessinées au-dessus des autres. Les flèches qui partagent un même point sont automatiquement décalées.", MessageType.None);
        GUILayout.EndArea();
    }

    private static string GetTriggerLabel(TransitionTrigger trigger)
    {
        switch (trigger)
        {
            case TransitionTrigger.Time:
                return "TEMPS — fin automatique";
            case TransitionTrigger.Input:
                return "INPUT — action du joueur";
            case TransitionTrigger.TimeAndInput:
                return "TEMPS + INPUT — délai atteint puis action du joueur";
            default:
                return "CODE / ÉVÉNEMENT";
        }
    }

    private void DrawEndpointControls(Transition transition)
    {
        EditorGUILayout.Space(3f);
        EditorGUI.BeginChangeCheck();
        AnchorSide outputSide = (AnchorSide)EditorGUILayout.EnumPopup("Sortie", GetSavedSide(transition, "out"));
        EditorGUI.BeginDisabledGroup(outputSide == AnchorSide.Auto);
        float outputOffset = EditorGUILayout.Slider("Position sortie", GetSavedOffset(transition, "out"), 0f, 1f);
        EditorGUI.EndDisabledGroup();
        AnchorSide inputSide = (AnchorSide)EditorGUILayout.EnumPopup("Entrée", GetSavedSide(transition, "in"));
        EditorGUI.BeginDisabledGroup(inputSide == AnchorSide.Auto);
        float inputOffset = EditorGUILayout.Slider("Position entrée", GetSavedOffset(transition, "in"), 0f, 1f);
        EditorGUI.EndDisabledGroup();
        if (EditorGUI.EndChangeCheck())
        {
            SaveEndpoint(transition, "out", outputSide, outputOffset);
            SaveEndpoint(transition, "in", inputSide, inputOffset);
            Repaint();
        }
    }

    private static AnchorSide GetSavedSide(Transition transition, string endpointName)
    {
        return (AnchorSide)EditorPrefs.GetInt(GetTransitionKey(transition, endpointName) + ".side", (int)AnchorSide.Auto);
    }

    private static float GetSavedOffset(Transition transition, string endpointName)
    {
        return EditorPrefs.GetFloat(GetTransitionKey(transition, endpointName) + ".offset", 0.5f);
    }

    private static void SaveEndpoint(Transition transition, string endpointName, AnchorSide side, float offset)
    {
        string key = GetTransitionKey(transition, endpointName);
        EditorPrefs.SetInt(key + ".side", (int)side);
        EditorPrefs.SetFloat(key + ".offset", offset);
    }

    private void HandleNodeDrag()
    {
        Event currentEvent = Event.current;
        if (currentEvent.type == EventType.MouseUp && draggedBehaviour.HasValue)
        {
            draggedBehaviour = null;
            currentEvent.Use();
            return;
        }

        if (currentEvent.type == EventType.MouseDrag && draggedBehaviour.HasValue)
        {
            Rect rect = nodeRects[draggedBehaviour.Value];
            rect.position = currentEvent.mousePosition - dragOffset;
            nodeRects[draggedBehaviour.Value] = rect;
            SaveNodePosition(draggedBehaviour.Value, rect.position);
            currentEvent.Use();
            Repaint();
            return;
        }

        if (currentEvent.type != EventType.MouseDown || currentEvent.button != 0)
            return;

        foreach (KeyValuePair<BehaviourType, Rect> node in nodeRects)
        {
            if (!node.Value.Contains(currentEvent.mousePosition))
                continue;

            selectedBehaviour = node.Key;
            draggedBehaviour = node.Key;
            dragOffset = currentEvent.mousePosition - node.Value.position;
            detailsScroll = Vector2.zero;
            currentEvent.Use();
            Repaint();
            return;
        }
    }

    private void ResetLayout()
    {
        foreach (BehaviourType behaviour in ScriptNames.Keys)
        {
            EditorPrefs.DeleteKey(GetNodeKey(behaviour) + ".x");
            EditorPrefs.DeleteKey(GetNodeKey(behaviour) + ".y");
        }

        foreach (Transition transition in transitions)
        {
            ResetEndpoint(transition, "out");
            ResetEndpoint(transition, "in");
        }

        BuildNodeLayout();
        Repaint();
    }

    private static void ResetEndpoint(Transition transition, string endpointName)
    {
        string key = GetTransitionKey(transition, endpointName);
        EditorPrefs.DeleteKey(key + ".side");
        EditorPrefs.DeleteKey(key + ".offset");
    }

    private void SaveNodePosition(BehaviourType behaviour, Vector2 position)
    {
        EditorPrefs.SetFloat(GetNodeKey(behaviour) + ".x", position.x);
        EditorPrefs.SetFloat(GetNodeKey(behaviour) + ".y", position.y);
    }

    private static string GetNodeKey(BehaviourType behaviour)
    {
        return "OneBitDungeon.PlayerBehaviourTree.Node." + behaviour;
    }

    private static string GetTransitionKey(Transition transition, string endpointName)
    {
        return "OneBitDungeon.PlayerBehaviourTree.Edge." + transition.source + "." + transition.destination + "." + StableHash(transition.condition) + "." + endpointName;
    }

    private static string StableHash(string value)
    {
        unchecked
        {
            uint hash = 2166136261;
            for (int i = 0; i < value.Length; i++)
                hash = (hash ^ value[i]) * 16777619;
            return hash.ToString("X8");
        }
    }

    private static void OpenScript(string assetPath)
    {
        MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
        if (script != null)
            AssetDatabase.OpenAsset(script);
    }

    private struct Transition
    {
        public readonly BehaviourType source;
        public readonly BehaviourType destination;
        public readonly string condition;
        public readonly string assetPath;
        public readonly TransitionTrigger trigger;

        public Transition(BehaviourType source, BehaviourType destination, string condition, string assetPath, TransitionTrigger trigger)
        {
            this.source = source;
            this.destination = destination;
            this.condition = condition;
            this.assetPath = assetPath;
            this.trigger = trigger;
        }
    }

    private enum TransitionTrigger
    {
        Code,
        Input,
        Time,
        TimeAndInput
    }

    private enum AnchorSide
    {
        Auto,
        Left,
        Right,
        Top,
        Bottom
    }

    private struct Endpoint
    {
        public readonly Vector3 position;
        public readonly Vector3 normal;
        public readonly AnchorSide side;

        public Endpoint(Vector3 position, Vector3 normal, AnchorSide side)
        {
            this.position = position;
            this.normal = normal;
            this.side = side;
        }
    }
}
