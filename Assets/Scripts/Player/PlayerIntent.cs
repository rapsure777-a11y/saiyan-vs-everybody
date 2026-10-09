using UnityEngine;
using UnityEngine.InputSystem;

namespace Saiyan.Player
{
    /// <summary>What the player wants to do this frame, independent of the device. Press flags are true for exactly the frame they happen.</summary>
    public struct PlayerIntent
    {
        public float move;                  // -1..1
        public bool jumpPressed, jumpHeld, shootHeld, dashPressed, superPressed, pausePressed, anyPressed;
    }

    public interface IIntentSource { PlayerIntent Read(); }

    /// <summary>Reads the Input Actions asset (Resources/Input/SaiyanControls). Rebind it in the asset: keyboard and gamepad are both set up there.</summary>
    public sealed class InputActionsIntent : MonoBehaviour, IIntentSource
    {
        public const string AssetPath = "Input/SaiyanControls";
        InputActionAsset m_Asset;
        InputAction m_Move, m_Jump, m_Shoot, m_Dash, m_Super, m_Pause, m_Any;
        int m_ReadFrame = -1; PlayerIntent m_Cached;

        void Awake()
        {
            var src = Resources.Load<InputActionAsset>(AssetPath);
            m_Asset = src ? Instantiate(src) : BuildDefaultAsset();
            var map = m_Asset.FindActionMap("Player", true);
            m_Move = map.FindAction("Move", true); m_Jump = map.FindAction("Jump", true); m_Shoot = map.FindAction("Shoot", true);
            m_Dash = map.FindAction("Dash", true); m_Super = map.FindAction("Super", true); m_Pause = map.FindAction("Pause", true);
            m_Any = new InputAction("Any", InputActionType.Button); m_Any.AddBinding("<Keyboard>/anyKey"); m_Any.AddBinding("<Gamepad>/buttonSouth"); m_Any.AddBinding("<Gamepad>/start");
            m_Any.Enable();
        }
        void OnEnable() { m_Asset?.Enable(); }
        void OnDisable() { m_Asset?.Disable(); }
        void OnDestroy() { m_Any?.Dispose(); if (m_Asset) Destroy(m_Asset); }

        /// <summary>If the asset is missing (should not happen) the game still works with built-in default bindings.</summary>
        public static InputActionAsset BuildDefaultAsset()
        {
            var a = ScriptableObject.CreateInstance<InputActionAsset>(); a.name = "SaiyanControls"; var map = a.AddActionMap("Player");
            var mv = map.AddAction("Move", InputActionType.Value); mv.expectedControlType = "Vector2";
            mv.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            mv.AddBinding("<Gamepad>/leftStick"); mv.AddBinding("<Gamepad>/dpad");
            map.AddAction("Jump", InputActionType.Button, "<Keyboard>/space").AddBinding("<Gamepad>/buttonSouth");
            map.AddAction("Shoot", InputActionType.Button, "<Keyboard>/j").AddBinding("<Gamepad>/buttonWest");
            map.AddAction("Dash", InputActionType.Button, "<Keyboard>/k").AddBinding("<Gamepad>/buttonEast");
            map.AddAction("Super", InputActionType.Button, "<Keyboard>/l").AddBinding("<Gamepad>/buttonNorth");
            map.AddAction("Pause", InputActionType.Button, "<Keyboard>/escape").AddBinding("<Gamepad>/start");
            return a;
        }

        public PlayerIntent Read()
        {
            if (m_ReadFrame == Time.frameCount) return m_Cached;       // several readers per frame see the same press
            m_ReadFrame = Time.frameCount;
            var v = m_Move.ReadValue<Vector2>();
            float mv = Mathf.Abs(v.x) < 0.3f ? 0f : Mathf.Sign(v.x) * Mathf.Clamp01((Mathf.Abs(v.x) - 0.3f) / 0.7f + 0.2f);
            m_Cached = new PlayerIntent
            {
                move = mv,
                jumpPressed = m_Jump.WasPressedThisFrame(), jumpHeld = m_Jump.IsPressed(), shootHeld = m_Shoot.IsPressed(),
                dashPressed = m_Dash.WasPressedThisFrame() || (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame),
                superPressed = m_Super.WasPressedThisFrame(), pausePressed = m_Pause.WasPressedThisFrame(), anyPressed = m_Any.WasPressedThisFrame(),
            };
            return m_Cached;
        }
    }

    /// <summary>Test and replay input: set the fields from code.</summary>
    public sealed class ScriptedIntent : MonoBehaviour, IIntentSource
    {
        public PlayerIntent Current;
        int m_Frame = -1;
        public PlayerIntent Read()
        {
            var r = Current;
            if (m_Frame != Time.frameCount) { m_Frame = Time.frameCount; }
            return r;
        }
        void LateUpdate() { Current.jumpPressed = false; Current.dashPressed = false; Current.superPressed = false; Current.pausePressed = false; Current.anyPressed = false; }
    }
}
