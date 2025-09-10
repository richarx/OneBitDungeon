using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Tools_and_Scripts
{
    public enum InputType
    {
        Gamepad,
        Keyboard
    }

    public class InputData
    {
        public bool wasPressedThisFrame;
        public bool isPressed;
        public float lastPressTimestamp = -1.0f;

        public bool WasPressedWithBuffer()
        {
            if (Time.time - lastPressTimestamp <= 0.2f)
            {
                lastPressTimestamp = -1.0f;
                return true;
            }

            return false;
        }
    }

    public class InputPackage
    {
        public InputType lastInputType = InputType.Keyboard;
        
        //Game specific gamepad/keyboard bindings
        public Vector2 GetMove => lastInputType == InputType.Gamepad ? gamepadMove : keyboardMove;
        public Vector2 GetLook => lastInputType == InputType.Gamepad ? gamepadLook : keyboardLook;

        public InputData GetRoll => lastInputType == InputType.Gamepad ? eastButton : spaceKey;
        public InputData GetCrouch => lastInputType == InputType.Gamepad ? eastButton : ctrlKey;
        
        public InputData GetShoot => lastInputType == InputType.Gamepad ? rightTrigger : leftMouse;
        public InputData GetAim => lastInputType == InputType.Gamepad ? leftTrigger : rightMouse;
        public InputData GetReload => lastInputType == InputType.Gamepad ? westButton : rKey;
        public InputData GetSwapWeapon => lastInputType == InputType.Gamepad ? northButton : middleMouse;
        
        public InputData GetBackpack => lastInputType == InputType.Gamepad ? startButton : bKey;
        
        public InputData GetMenuLeft => lastInputType == InputType.Gamepad ? leftArrowButton : leftKey;
        public InputData GetMenuUp => lastInputType == InputType.Gamepad ? upArrowButton : upKey;
        public InputData GetMenuRight => lastInputType == InputType.Gamepad ? rightArrowButton : rightKey;
        public InputData GetMenuDown => lastInputType == InputType.Gamepad ? downArrowButton : downKey;
        
        public InputData GetToolLeft => lastInputType == InputType.Gamepad ? leftArrowButton : key_1;
        public InputData GetToolUp => lastInputType == InputType.Gamepad ? upArrowButton : key_2;
        public InputData GetToolRight => lastInputType == InputType.Gamepad ? rightArrowButton : key_3;
        public InputData GetToolDown => lastInputType == InputType.Gamepad ? downArrowButton : key_4;

        public InputData GetSettingsMenu => lastInputType == InputType.Gamepad ? selectButton : tabKey;
        public InputData GetInteract => lastInputType == InputType.Gamepad ? rightShoulder : eKey;

        //Gamepad raw
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
        
        //Keyboard raw
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
        public InputData eKey = new InputData();
        public InputData fKey = new InputData();
        public InputData gKey = new InputData();
        public InputData iKey = new InputData();
        public InputData rKey = new InputData();
        public InputData xKey = new InputData();
    }
    
    public class InputPacker
    {
        public static UnityEvent<InputType> OnChangeInputType = new UnityEvent<InputType>();
        
        private InputPackage previousPackage = new InputPackage();

        private bool wasGamepadUsed;
        private bool wasKeyboardUsed;

        public void ResetBuffers()
        {
            previousPackage = new InputPackage();
        }
        
        public InputPackage ComputeInputPackage()
        {
            wasGamepadUsed = false;
            wasKeyboardUsed = false;

            InputPackage inputs = new InputPackage();
            
            if (Gamepad.current != null)
                inputs = ComputeGamepadInput(inputs);
            inputs = ComputeKeyboardInput(inputs);

            inputs.lastInputType = ComputeLastInputTypeUsed();

            if (inputs.lastInputType != previousPackage.lastInputType)
                OnChangeInputType?.Invoke(inputs.lastInputType);
            
            previousPackage = inputs;

            return inputs;
        }
        
        private InputPackage ComputeKeyboardInput(InputPackage inputs)
        {
            inputs.keyboardMove = ComputeKeyboardMove();
            inputs.keyboardLook = ComputeKeyboardLook();
            
            inputs.spaceKey = ComputeKeyboardInput(Keyboard.current.spaceKey, previousPackage.spaceKey.lastPressTimestamp);
            
            inputs.leftMouse = ComputeKeyboardInput(Mouse.current.leftButton, previousPackage.leftMouse.lastPressTimestamp);
            inputs.rightMouse = ComputeKeyboardInput(Mouse.current.rightButton, previousPackage.rightMouse.lastPressTimestamp);
            inputs.middleMouse = ComputeMiddleMouse(previousPackage.middleMouse.lastPressTimestamp);
            
            inputs.leftKey = ComputeDualKeyboardInput(Keyboard.current.aKey, Keyboard.current.qKey, previousPackage.leftKey.lastPressTimestamp);
            inputs.upKey = ComputeDualKeyboardInput(Keyboard.current.zKey, Keyboard.current.wKey, previousPackage.upKey.lastPressTimestamp);
            inputs.rightKey = ComputeKeyboardInput(Keyboard.current.dKey, previousPackage.rightKey.lastPressTimestamp);
            inputs.downKey = ComputeKeyboardInput(Keyboard.current.sKey, previousPackage.downKey.lastPressTimestamp);
            
            inputs.shiftKey = ComputeKeyboardInput(Keyboard.current.leftShiftKey, previousPackage.shiftKey.lastPressTimestamp);
            inputs.tabKey = ComputeKeyboardInput(Keyboard.current.tabKey, previousPackage.tabKey.lastPressTimestamp);
            inputs.ctrlKey = ComputeKeyboardInput(Keyboard.current.leftCtrlKey, previousPackage.ctrlKey.lastPressTimestamp);

            inputs.key_1 = ComputeDualKeyboardInput(Keyboard.current.digit1Key, Keyboard.current.numpad1Key, previousPackage.key_1.lastPressTimestamp);
            inputs.key_2 = ComputeDualKeyboardInput(Keyboard.current.digit2Key, Keyboard.current.numpad2Key, previousPackage.key_2.lastPressTimestamp);
            inputs.key_3 = ComputeDualKeyboardInput(Keyboard.current.digit3Key, Keyboard.current.numpad3Key, previousPackage.key_3.lastPressTimestamp);
            inputs.key_4 = ComputeDualKeyboardInput(Keyboard.current.digit4Key, Keyboard.current.numpad4Key, previousPackage.key_4.lastPressTimestamp);
            
            inputs.bKey = ComputeKeyboardInput(Keyboard.current.bKey, previousPackage.bKey.lastPressTimestamp);
            inputs.eKey = ComputeKeyboardInput(Keyboard.current.eKey, previousPackage.eKey.lastPressTimestamp);
            inputs.fKey = ComputeKeyboardInput(Keyboard.current.fKey, previousPackage.fKey.lastPressTimestamp);
            inputs.gKey = ComputeKeyboardInput(Keyboard.current.gKey, previousPackage.gKey.lastPressTimestamp);
            inputs.iKey = ComputeKeyboardInput(Keyboard.current.iKey, previousPackage.iKey.lastPressTimestamp);
            inputs.rKey = ComputeKeyboardInput(Keyboard.current.rKey, previousPackage.rKey.lastPressTimestamp);
            inputs.xKey = ComputeKeyboardInput(Keyboard.current.xKey, previousPackage.xKey.lastPressTimestamp);
            
            return inputs;
        }
        
        private InputPackage ComputeGamepadInput(InputPackage inputs)
        {
            inputs.gamepadMove = ComputeLeftJoystick();
            inputs.gamepadLook = ComputeRightJoystick();
            
            inputs.eastButton = ComputeGamepadButtonInput(Gamepad.current.buttonEast, previousPackage.eastButton.lastPressTimestamp);
            inputs.northButton = ComputeGamepadButtonInput(Gamepad.current.buttonNorth, previousPackage.northButton.lastPressTimestamp);
            inputs.westButton = ComputeGamepadButtonInput(Gamepad.current.buttonWest, previousPackage.westButton.lastPressTimestamp);
            inputs.southButton = ComputeGamepadButtonInput(Gamepad.current.buttonSouth, previousPackage.southButton.lastPressTimestamp);

            inputs.leftArrowButton = ComputeGamepadButtonInput(Gamepad.current.dpad.left, previousPackage.leftArrowButton.lastPressTimestamp);
            inputs.upArrowButton = ComputeGamepadButtonInput(Gamepad.current.dpad.up, previousPackage.upArrowButton.lastPressTimestamp);
            inputs.rightArrowButton = ComputeGamepadButtonInput(Gamepad.current.dpad.right, previousPackage.rightArrowButton.lastPressTimestamp);
            inputs.downArrowButton = ComputeGamepadButtonInput(Gamepad.current.dpad.down, previousPackage.downArrowButton.lastPressTimestamp);
            
            inputs.leftStickButton = ComputeGamepadButtonInput(Gamepad.current.leftStickButton, previousPackage.leftStickButton.lastPressTimestamp);
            inputs.rightStickButton = ComputeGamepadButtonInput(Gamepad.current.rightStickButton, previousPackage.rightStickButton.lastPressTimestamp);
            
            inputs.leftShoulder = ComputeGamepadButtonInput(Gamepad.current.leftShoulder, previousPackage.leftShoulder.lastPressTimestamp);
            inputs.rightShoulder = ComputeGamepadButtonInput(Gamepad.current.rightShoulder, previousPackage.rightShoulder.lastPressTimestamp);
            
            inputs.leftTrigger = ComputeGamepadButtonInput(Gamepad.current.leftTrigger, previousPackage.leftTrigger.lastPressTimestamp);
            inputs.rightTrigger = ComputeGamepadButtonInput(Gamepad.current.rightTrigger, previousPackage.rightTrigger.lastPressTimestamp);
            
            inputs.startButton = ComputeGamepadButtonInput(Gamepad.current.startButton, previousPackage.startButton.lastPressTimestamp);
            inputs.selectButton = ComputeGamepadButtonInput(Gamepad.current.selectButton, previousPackage.selectButton.lastPressTimestamp);
            
            return inputs;
        }

        private Vector2 ComputeKeyboardMove()
        {
            Vector2 keyBoardInput = Vector2.zero;
            
            if (Keyboard.current.zKey.isPressed || Keyboard.current.wKey.isPressed)
                keyBoardInput.y += 1;
            
            if (Keyboard.current.sKey.isPressed)
                keyBoardInput.y -= 1;
            
            if (Keyboard.current.qKey.isPressed || Keyboard.current.aKey.isPressed)
                keyBoardInput.x -= 1;
            
            if (Keyboard.current.dKey.isPressed)
                keyBoardInput.x += 1;

            return keyBoardInput.normalized;
        }

        private Vector2 ComputeKeyboardLook()
        {
            Vector2 mouse = Vector2.zero;
            float sensibilityMultiplier = Application.isEditor ? 5.0f : 1.0f;
            
            mouse.x = Input.GetAxisRaw("Mouse X");
            mouse.y = Input.GetAxisRaw("Mouse Y");

            if (mouse.magnitude > 0.0f)
                mouse *= sensibilityMultiplier * Time.deltaTime;

            return mouse;
        }

        private Vector2 ComputeLeftJoystick()
        {
            Vector2 gamepadInput = new Vector2(Gamepad.current.leftStick.x.ReadValue(), Gamepad.current.leftStick.y.ReadValue());

            if (Mathf.Abs(gamepadInput.x) <= 0.15f)
                gamepadInput.x = 0.0f;
        
            if (Mathf.Abs(gamepadInput.y) <= 0.15f)
                gamepadInput.y = 0.0f;
                
            if (gamepadInput.magnitude <= 0.15f)
                return Vector2.zero;
            
            RegisterInputType(InputType.Gamepad);

            return gamepadInput;
        }

        private Vector2 ComputeRightJoystick()
        {
            float sensibilityMultiplier = Application.isEditor ? 5.0f : 1.0f;
            
            Vector2 gamepad = new Vector2(Gamepad.current.rightStick.x.ReadValue(), Gamepad.current.rightStick.y.ReadValue());
                
            if (Mathf.Abs(gamepad.x) <= 0.15f)
                gamepad.x = 0.0f;
            
            if (Mathf.Abs(gamepad.y) <= 0.15f)
                gamepad.y = 0.0f;

            if (gamepad.magnitude <= 0.15f)
                return Vector2.zero;
            
            gamepad.x *= sensibilityMultiplier * Time.deltaTime;
            gamepad.y *= sensibilityMultiplier * Time.deltaTime;
            
            RegisterInputType(InputType.Gamepad);
            
            return gamepad;
        }
        
        private InputData ComputeMiddleMouse(float lastPressTimestamp)
        {
            bool isPressed = Mouse.current.scroll.up.magnitude > 0.0f || Mouse.current.scroll.down.magnitude > 0.0f;
            
            InputData input = new InputData();
            
            input.wasPressedThisFrame = isPressed;
            input.isPressed = isPressed;

            input.lastPressTimestamp = input.wasPressedThisFrame ? Time.time : lastPressTimestamp;
            
            if (input.wasPressedThisFrame)
                RegisterInputType(InputType.Keyboard);

            return input;
        }
        
        private InputData ComputeGamepadButtonInput(ButtonControl button, float lastPressTimestamp)
        {
            return ComputeButtonInput(button, lastPressTimestamp, InputType.Gamepad);
        }
        
        private InputData ComputeKeyboardInput(ButtonControl button, float lastPressTimestamp)
        {
            return ComputeButtonInput(button, lastPressTimestamp, InputType.Keyboard);
        }
        
        private InputData ComputeDualKeyboardInput(ButtonControl button_1, ButtonControl button_2, float lastPressTimestamp)
        {
            InputData input_1 = ComputeButtonInput(button_1, lastPressTimestamp, InputType.Keyboard);
            InputData input_2 = ComputeButtonInput(button_2, lastPressTimestamp, InputType.Keyboard);

            InputData input = new InputData();

            input.wasPressedThisFrame = input_1.wasPressedThisFrame || input_2.wasPressedThisFrame;
            input.isPressed = input_1.isPressed || input_2.isPressed;
            input.lastPressTimestamp = Mathf.Max(input_1.lastPressTimestamp, input_2.lastPressTimestamp);

            return input;
        }

        private InputData ComputeButtonInput(ButtonControl button, float lastPressTimestamp, InputType inputType)
        {
            InputData input = new InputData();
            
            input.wasPressedThisFrame = button.wasPressedThisFrame;
            input.isPressed = button.isPressed;

            input.lastPressTimestamp = input.wasPressedThisFrame ? Time.time : lastPressTimestamp;

            if (input.wasPressedThisFrame)
                RegisterInputType(inputType);
            
            return input;
        }

        private void RegisterInputType(InputType inputType)
        {
            if (inputType == InputType.Gamepad)
                wasGamepadUsed = true;
            else if (inputType == InputType.Keyboard)
                wasKeyboardUsed = true;
        }
        
        private InputType ComputeLastInputTypeUsed()
        {
            if (wasGamepadUsed)
                return InputType.Gamepad;
            if (wasKeyboardUsed)
                return InputType.Keyboard;
            
            return previousPackage.lastInputType;
        }
    }
}
