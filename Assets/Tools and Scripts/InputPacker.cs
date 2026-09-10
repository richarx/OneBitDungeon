using System;
using System.Collections.Generic;
using Tools_and_Scripts.RewiredInput;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Tools_and_Scripts
{
    public enum InputType { Gamepad, Keyboard }

    public class InputData
    {
        public bool wasPressedThisFrame;
        public bool isPressed;
        public float lastPressTimestamp = -1.0f;

        public bool WasPressedWithBuffer(float bufferDuration = 0.2f)
        {
            if (lastPressTimestamp < 0.0f) return false;
            if (Time.time - lastPressTimestamp > bufferDuration) return false;
            lastPressTimestamp = -1.0f;
            return true;
        }
    }

    public class InputPackage
    {
        public InputType lastInputType = InputType.Keyboard;
        public Vector2 GetMove => _move;
        public Vector2 GetLook => _look;
        public InputData GetRoll => _roll;
        public InputData GetJump => _jump;
        public InputData GetAttack => _attack;
        public InputData GetParry => _parry;
        public InputData GetInteraction => _interact;
        public InputData GetSitDown => _sitDown;
        public InputData GetTag => _tagCritical;
        public InputData GetCriticalAttack => _tagCritical;
        public InputData GetArroganceMode => _arroganceMode;
        public InputData GetMenuLeft => _menuLeft;
        public InputData GetMenuUp => _menuUp;
        public InputData GetMenuRight => _menuRight;
        public InputData GetMenuDown => _menuDown;
        public InputData GetDialogueConfirm => _interact;
        public InputData GetDialogueQuit => _dialogueQuit;

        // Fixed physical controls retained for MainMenu and Intro compatibility.
        public Vector2 gamepadMove;
        public Vector2 gamepadLook;
        public InputData eastButton = new InputData();
        public InputData northButton = new InputData();
        public InputData westButton = new InputData();
        public InputData southButton = new InputData();
        public InputData leftArrowButton = new InputData();
        public InputData upArrowButton = new InputData();
        public InputData rightArrowButton = new InputData();
        public InputData downArrowButton = new InputData();
        public InputData leftStickButton = new InputData();
        public InputData rightStickButton = new InputData();
        public InputData leftShoulder = new InputData();
        public InputData rightShoulder = new InputData();
        public InputData leftTrigger = new InputData();
        public InputData rightTrigger = new InputData();
        public InputData startButton = new InputData();
        public InputData selectButton = new InputData();
        public Vector2 keyboardMove;
        public Vector2 keyboardLook;
        public InputData spaceKey = new InputData();
        public InputData leftMouse = new InputData();
        public InputData rightMouse = new InputData();
        public InputData middleMouse = new InputData();
        public InputData leftKey = new InputData();
        public InputData upKey = new InputData();
        public InputData rightKey = new InputData();
        public InputData downKey = new InputData();
        public InputData shiftKey = new InputData();
        public InputData tabKey = new InputData();
        public InputData ctrlKey = new InputData();
        public InputData key_1 = new InputData();
        public InputData key_2 = new InputData();
        public InputData key_3 = new InputData();
        public InputData key_4 = new InputData();
        public InputData bKey = new InputData();
        public InputData cKey = new InputData();
        public InputData eKey = new InputData();
        public InputData fKey = new InputData();
        public InputData gKey = new InputData();
        public InputData iKey = new InputData();
        public InputData rKey = new InputData();
        public InputData xKey = new InputData();

        private Vector2 _move;
        private Vector2 _look;
        private InputData _roll = new InputData();
        private InputData _jump = new InputData();
        private InputData _attack = new InputData();
        private InputData _parry = new InputData();
        private InputData _interact = new InputData();
        private InputData _sitDown = new InputData();
        private InputData _tagCritical = new InputData();
        private InputData _arroganceMode = new InputData();
        private InputData _menuLeft = new InputData();
        private InputData _menuUp = new InputData();
        private InputData _menuRight = new InputData();
        private InputData _menuDown = new InputData();
        private InputData _dialogueQuit = new InputData();

        internal void SetActions(Vector2 move, InputData roll, InputData jump, InputData attack, InputData parry, InputData interact, InputData sitDown, InputData tagCritical, InputData arroganceMode, InputData menuLeft, InputData menuUp, InputData menuRight, InputData menuDown, InputData dialogueQuit)
        {
            _move = move; _roll = roll; _jump = jump; _attack = attack; _parry = parry;
            _interact = interact; _sitDown = sitDown; _tagCritical = tagCritical; _arroganceMode = arroganceMode;
            _menuLeft = menuLeft; _menuUp = menuUp; _menuRight = menuRight; _menuDown = menuDown; _dialogueQuit = dialogueQuit;
        }

        internal void Clear()
        {
            lastInputType = InputType.Keyboard;
            _move = Vector2.zero; _look = Vector2.zero;
            Clear(_roll); Clear(_jump); Clear(_attack); Clear(_parry); Clear(_interact); Clear(_sitDown); Clear(_tagCritical); Clear(_arroganceMode); Clear(_menuLeft); Clear(_menuUp); Clear(_menuRight); Clear(_menuDown); Clear(_dialogueQuit);
            gamepadMove = Vector2.zero; gamepadLook = Vector2.zero; keyboardMove = Vector2.zero; keyboardLook = Vector2.zero;
            Clear(eastButton); Clear(northButton); Clear(westButton); Clear(southButton); Clear(leftArrowButton); Clear(upArrowButton); Clear(rightArrowButton); Clear(downArrowButton); Clear(leftStickButton); Clear(rightStickButton); Clear(leftShoulder); Clear(rightShoulder); Clear(leftTrigger); Clear(rightTrigger); Clear(startButton); Clear(selectButton);
            Clear(spaceKey); Clear(leftMouse); Clear(rightMouse); Clear(middleMouse); Clear(leftKey); Clear(upKey); Clear(rightKey); Clear(downKey); Clear(shiftKey); Clear(tabKey); Clear(ctrlKey); Clear(key_1); Clear(key_2); Clear(key_3); Clear(key_4); Clear(bKey); Clear(cKey); Clear(eKey); Clear(fKey); Clear(gKey); Clear(iKey); Clear(rKey); Clear(xKey);
        }

        private static void Clear(InputData input)
        {
            input.wasPressedThisFrame = false;
            input.isPressed = false;
            input.lastPressTimestamp = -1.0f;
        }
    }

    public class InputPacker
    {
        private static readonly string[] _actionNames =
        {
            RewiredActionNames.MoveHorizontal,
            RewiredActionNames.MoveVertical,
            RewiredActionNames.Roll,
            RewiredActionNames.Jump,
            RewiredActionNames.Attack,
            RewiredActionNames.Parry,
            RewiredActionNames.Interact,
            RewiredActionNames.SitDown,
            RewiredActionNames.Critical,
            RewiredActionNames.ArroganceMode,
            RewiredActionNames.MenuHorizontal,
            RewiredActionNames.MenuVertical,
            RewiredActionNames.DialogueQuit
        };
        public static UnityEvent<InputType> OnChangeInputType = new UnityEvent<InputType>();
        private InputPackage _previousPackage = new InputPackage();
        private InputPackage _currentPackage = new InputPackage();
        private int _lastComputedFrame = -1;
        private RewiredInputRuntime _cachedRuntime;
        private int _cachedRuntimeGeneration = -1;
        private bool _ignoreMenuLeftUntilReleased;
        private bool _ignoreMenuUpUntilReleased;
        private bool _ignoreMenuRightUntilReleased;
        private bool _ignoreMenuDownUntilReleased;
        private static bool _hasPublishedInputType;
        private static InputType _lastPublishedInputType;
        private static readonly List<WeakReference<InputPacker>> _instances = new List<WeakReference<InputPacker>>();
        private static int _suspensionCount;

        public InputPacker()
        {
            _instances.Add(new WeakReference<InputPacker>(this));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            OnChangeInputType = new UnityEvent<InputType>();
            _hasPublishedInputType = false;
            _lastPublishedInputType = InputType.Keyboard;
            _instances.Clear();
            _suspensionCount = 0;
        }

        /// <summary>Stops every legacy input package until the returned handle is disposed.</summary>
        public static IDisposable Suspend()
        {
            _suspensionCount++;
            InvalidateLivePackages();
            return new SuspensionHandle();
        }

        /// <summary>Returns true when the gameplay controls sampled by this facade are no longer held.</summary>
        public static bool AreGameplayControlsReleased()
        {
            RewiredInputRuntime runtime = RewiredInputRuntime.Instance;
            if (runtime != null && runtime.IsReady)
            {
                for (int i = 2; i < _actionNames.Length; i++)
                {
                    if (runtime.GetButton(_actionNames[i])) return false;
                }
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.spaceKey.isPressed || keyboard.aKey.isPressed || keyboard.qKey.isPressed || keyboard.zKey.isPressed || keyboard.wKey.isPressed || keyboard.dKey.isPressed || keyboard.sKey.isPressed || keyboard.leftShiftKey.isPressed || keyboard.tabKey.isPressed || keyboard.leftCtrlKey.isPressed || keyboard.digit1Key.isPressed || keyboard.numpad1Key.isPressed || keyboard.digit2Key.isPressed || keyboard.numpad2Key.isPressed || keyboard.digit3Key.isPressed || keyboard.numpad3Key.isPressed || keyboard.digit4Key.isPressed || keyboard.numpad4Key.isPressed || keyboard.bKey.isPressed || keyboard.cKey.isPressed || keyboard.eKey.isPressed || keyboard.fKey.isPressed || keyboard.gKey.isPressed || keyboard.iKey.isPressed || keyboard.rKey.isPressed || keyboard.xKey.isPressed)) return false;

            Mouse mouse = Mouse.current;
            if (mouse != null && (mouse.leftButton.isPressed || mouse.rightButton.isPressed || mouse.middleButton.isPressed)) return false;

            Gamepad gamepad = Gamepad.current;
            return gamepad == null || !(gamepad.buttonEast.isPressed || gamepad.buttonNorth.isPressed || gamepad.buttonWest.isPressed || gamepad.buttonSouth.isPressed || gamepad.dpad.left.isPressed || gamepad.dpad.up.isPressed || gamepad.dpad.right.isPressed || gamepad.dpad.down.isPressed || gamepad.leftStickButton.isPressed || gamepad.rightStickButton.isPressed || gamepad.leftShoulder.isPressed || gamepad.rightShoulder.isPressed || gamepad.leftTrigger.isPressed || gamepad.rightTrigger.isPressed || gamepad.startButton.isPressed || gamepad.selectButton.isPressed);
        }

        public void ResetBuffers()
        {
            _previousPackage.Clear();
            _currentPackage.Clear();
            _lastComputedFrame = Time.frameCount;
            _ignoreMenuLeftUntilReleased = true;
            _ignoreMenuUpUntilReleased = true;
            _ignoreMenuRightUntilReleased = true;
            _ignoreMenuDownUntilReleased = true;
        }

        public InputPackage ComputeInputPackage()
        {
            if (_suspensionCount > 0)
            {
                ResetBuffers();
                return _currentPackage;
            }

            if (_lastComputedFrame == Time.frameCount)
                return _currentPackage;

            InputPackage inputs = new InputPackage();
            RewiredInputRuntime runtime = RewiredInputRuntime.Instance;

            if (runtime != null && runtime.IsReady)
            {
                if (_cachedRuntime != runtime || _cachedRuntimeGeneration != runtime.ActionCacheGeneration)
                {
                    runtime.CacheActions(_actionNames);
                    _cachedRuntime = runtime;
                    _cachedRuntimeGeneration = runtime.ActionCacheGeneration;
                }
                ComputeRewiredInput(inputs, runtime);
            }

            ComputePhysicalCompatibilityInput(inputs);
            if (!_hasPublishedInputType || inputs.lastInputType != _lastPublishedInputType)
            {
                _hasPublishedInputType = true;
                _lastPublishedInputType = inputs.lastInputType;
                OnChangeInputType?.Invoke(inputs.lastInputType);
            }
            _previousPackage = inputs;
            _currentPackage = inputs;
            _lastComputedFrame = Time.frameCount;
            return inputs;
        }

        private void ComputeRewiredInput(InputPackage inputs, RewiredInputRuntime runtime)
        {
            Vector2 move = Vector3.ClampMagnitude(
                new Vector2(
                    runtime.GetAxis(RewiredActionNames.MoveHorizontal),
                    runtime.GetAxis(RewiredActionNames.MoveVertical)),
                1.0f);
            inputs.lastInputType = runtime.GetLastInputType();
            inputs.SetActions(move,
                ReadAction(runtime, RewiredActionNames.Roll, _previousPackage.GetRoll),
                ReadAction(runtime, RewiredActionNames.Jump, _previousPackage.GetJump),
                ReadAction(runtime, RewiredActionNames.Attack, _previousPackage.GetAttack),
                ReadAction(runtime, RewiredActionNames.Parry, _previousPackage.GetParry),
                ReadAction(runtime, RewiredActionNames.Interact, _previousPackage.GetInteraction),
                ReadAction(runtime, RewiredActionNames.SitDown, _previousPackage.GetSitDown),
                ReadAction(runtime, RewiredActionNames.Critical, _previousPackage.GetTag),
                ReadAction(runtime, RewiredActionNames.ArroganceMode, _previousPackage.GetArroganceMode),
                ReadAxisButton(runtime, RewiredActionNames.MenuHorizontal, -1.0f, _previousPackage.GetMenuLeft, ref _ignoreMenuLeftUntilReleased),
                ReadAxisButton(runtime, RewiredActionNames.MenuVertical, 1.0f, _previousPackage.GetMenuUp, ref _ignoreMenuUpUntilReleased),
                ReadAxisButton(runtime, RewiredActionNames.MenuHorizontal, 1.0f, _previousPackage.GetMenuRight, ref _ignoreMenuRightUntilReleased),
                ReadAxisButton(runtime, RewiredActionNames.MenuVertical, -1.0f, _previousPackage.GetMenuDown, ref _ignoreMenuDownUntilReleased),
                ReadAction(runtime, RewiredActionNames.DialogueQuit, _previousPackage.GetDialogueQuit));
        }

        private static InputData ReadAction(RewiredInputRuntime runtime, string name, InputData previous)
        {
            bool down = runtime.GetButtonDown(name);
            return new InputData { wasPressedThisFrame = down, isPressed = runtime.GetButton(name), lastPressTimestamp = down ? Time.time : previous.lastPressTimestamp };
        }

        private static InputData ReadAxisButton(RewiredInputRuntime runtime, string name, float direction, InputData previous, ref bool ignoreUntilReleased)
        {
            bool pressed = runtime.GetAxis(name) * direction > 0.5f;
            bool down = pressed && !previous.isPressed && !ignoreUntilReleased;
            if (!pressed) ignoreUntilReleased = false;
            return new InputData { wasPressedThisFrame = down, isPressed = pressed, lastPressTimestamp = down ? Time.time : previous.lastPressTimestamp };
        }

        private void ComputePhysicalCompatibilityInput(InputPackage inputs)
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            Gamepad gamepad = Gamepad.current;
            if (keyboard != null)
            {
                inputs.keyboardMove = new Vector2(Held(keyboard.dKey) - Held(keyboard.aKey, keyboard.qKey), Held(keyboard.zKey, keyboard.wKey) - Held(keyboard.sKey)).normalized;
                inputs.keyboardLook = mouse == null ? Vector2.zero : mouse.delta.ReadValue() * (Application.isEditor ? 5.0f : 1.0f) * Time.deltaTime;
                inputs.spaceKey = Physical(keyboard.spaceKey, _previousPackage.spaceKey); inputs.leftKey = Physical(keyboard.aKey, keyboard.qKey, _previousPackage.leftKey); inputs.upKey = Physical(keyboard.zKey, keyboard.wKey, _previousPackage.upKey); inputs.rightKey = Physical(keyboard.dKey, _previousPackage.rightKey); inputs.downKey = Physical(keyboard.sKey, _previousPackage.downKey);
                inputs.shiftKey = Physical(keyboard.leftShiftKey, _previousPackage.shiftKey); inputs.tabKey = Physical(keyboard.tabKey, _previousPackage.tabKey); inputs.ctrlKey = Physical(keyboard.leftCtrlKey, _previousPackage.ctrlKey);
                inputs.key_1 = Physical(keyboard.digit1Key, keyboard.numpad1Key, _previousPackage.key_1); inputs.key_2 = Physical(keyboard.digit2Key, keyboard.numpad2Key, _previousPackage.key_2); inputs.key_3 = Physical(keyboard.digit3Key, keyboard.numpad3Key, _previousPackage.key_3); inputs.key_4 = Physical(keyboard.digit4Key, keyboard.numpad4Key, _previousPackage.key_4);
                inputs.bKey = Physical(keyboard.bKey, _previousPackage.bKey); inputs.cKey = Physical(keyboard.cKey, _previousPackage.cKey); inputs.eKey = Physical(keyboard.eKey, _previousPackage.eKey); inputs.fKey = Physical(keyboard.fKey, _previousPackage.fKey); inputs.gKey = Physical(keyboard.gKey, _previousPackage.gKey); inputs.iKey = Physical(keyboard.iKey, _previousPackage.iKey); inputs.rKey = Physical(keyboard.rKey, _previousPackage.rKey); inputs.xKey = Physical(keyboard.xKey, _previousPackage.xKey);
            }
            if (mouse != null) { inputs.leftMouse = Physical(mouse.leftButton, _previousPackage.leftMouse); inputs.rightMouse = Physical(mouse.rightButton, _previousPackage.rightMouse); inputs.middleMouse = Physical(mouse.middleButton, _previousPackage.middleMouse); }
            if (gamepad == null) return;
            inputs.gamepadMove = Vector3.ClampMagnitude(gamepad.leftStick.ReadValue(), 1.0f); inputs.gamepadLook = gamepad.rightStick.ReadValue() * (Application.isEditor ? 5.0f : 1.0f) * Time.deltaTime;
            inputs.eastButton = Physical(gamepad.buttonEast, _previousPackage.eastButton); inputs.northButton = Physical(gamepad.buttonNorth, _previousPackage.northButton); inputs.westButton = Physical(gamepad.buttonWest, _previousPackage.westButton); inputs.southButton = Physical(gamepad.buttonSouth, _previousPackage.southButton);
            inputs.leftArrowButton = Physical(gamepad.dpad.left, _previousPackage.leftArrowButton); inputs.upArrowButton = Physical(gamepad.dpad.up, _previousPackage.upArrowButton); inputs.rightArrowButton = Physical(gamepad.dpad.right, _previousPackage.rightArrowButton); inputs.downArrowButton = Physical(gamepad.dpad.down, _previousPackage.downArrowButton);
            inputs.leftStickButton = Physical(gamepad.leftStickButton, _previousPackage.leftStickButton); inputs.rightStickButton = Physical(gamepad.rightStickButton, _previousPackage.rightStickButton); inputs.leftShoulder = Physical(gamepad.leftShoulder, _previousPackage.leftShoulder); inputs.rightShoulder = Physical(gamepad.rightShoulder, _previousPackage.rightShoulder); inputs.leftTrigger = Physical(gamepad.leftTrigger, _previousPackage.leftTrigger); inputs.rightTrigger = Physical(gamepad.rightTrigger, _previousPackage.rightTrigger); inputs.startButton = Physical(gamepad.startButton, _previousPackage.startButton); inputs.selectButton = Physical(gamepad.selectButton, _previousPackage.selectButton);
        }

        private static float Held(ButtonControl primary, ButtonControl secondary = null) => primary != null && primary.isPressed || secondary != null && secondary.isPressed ? 1.0f : 0.0f;

        private static InputData Physical(ButtonControl button, InputData previous) => Physical(button, null, previous);

        private static InputData Physical(ButtonControl primary, ButtonControl secondary, InputData previous)
        {
            bool down = primary != null && primary.wasPressedThisFrame || secondary != null && secondary.wasPressedThisFrame;
            return new InputData { wasPressedThisFrame = down, isPressed = primary != null && primary.isPressed || secondary != null && secondary.isPressed, lastPressTimestamp = down ? Time.time : previous.lastPressTimestamp };
        }

        private static void InvalidateLivePackages()
        {
            for (int i = _instances.Count - 1; i >= 0; i--)
            {
                if (!_instances[i].TryGetTarget(out InputPacker inputPacker))
                {
                    _instances.RemoveAt(i);
                    continue;
                }
                inputPacker.ResetBuffers();
            }
        }

        private sealed class SuspensionHandle : IDisposable
        {
            private bool _disposed;

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _suspensionCount = Mathf.Max(0, _suspensionCount - 1);
                InvalidateLivePackages();
            }
        }
    }
}
