using System.Collections.Generic;
using Saiyan.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Saiyan.Art
{
    /// <summary>
    /// Animation preview scene (open from the main menu with F10): pick a character and animation, loop or play once, pause and step frames, change speed and scale,
    /// switch backgrounds, and show the gameplay hitbox and the pivot so you can judge how the art sits on the real collider. Esc returns to the menu.
    /// </summary>
    public sealed class AnimationPreview : MonoBehaviour
    {
        public const string SceneName = "AnimationPreview";
        readonly string[] m_Characters = { "Saiyan", "KingCakezilla" };
        int m_CharIndex; string[] m_Anims = new string[0]; string m_Selected = "";
        SpriteRenderer m_Sr; SpriteFlipbook m_Flip; LineRenderer m_Hitbox, m_Pivot; Camera m_Cam;
        float m_Scale = 1f, m_Speed = 1f; bool m_Loop = true, m_Paused, m_ShowHitbox = true, m_ShowPivot = true, m_Mirror; int m_Bg;
        static readonly Color[] s_Backgrounds = { new Color(0.47f, 0.69f, 0.96f), new Color(0.12f, 0.12f, 0.16f), new Color(0.75f, 0.75f, 0.75f), new Color(1f, 0.52f, 0.7f) };
        static readonly string[] s_BgNames = { "Sky", "Dark", "Grey", "Pink" };
        GUIStyle m_Btn, m_Label, m_Sel; Vector2 m_Scroll;

        void Awake()
        {
            Time.timeScale = 1f;
            m_Cam = Camera.main;
            if (!m_Cam) { var g = new GameObject("Main Camera") { tag = "MainCamera" }; m_Cam = g.AddComponent<Camera>(); g.AddComponent<AudioListener>(); }
            m_Cam.orthographic = true; m_Cam.orthographicSize = 2.2f; m_Cam.transform.position = new Vector3(1.2f, 1.0f, -10f); m_Cam.clearFlags = CameraClearFlags.SolidColor;
            var go = new GameObject("PreviewSprite"); m_Sr = go.AddComponent<SpriteRenderer>(); m_Sr.sortingOrder = 10; m_Flip = go.AddComponent<SpriteFlipbook>(); m_Flip.Target = m_Sr;
            var floor = new GameObject("Floor"); var fr = floor.AddComponent<SpriteRenderer>(); fr.sprite = PlaceholderArt.RoundRect(14f, 0.08f, new Color(0.2f, 0.12f, 0.15f), 0.03f, 0f); fr.sortingOrder = 0;
            m_Hitbox = NewLine("Hitbox", new Color(0.2f, 1f, 0.4f, 1f)); m_Pivot = NewLine("Pivot", new Color(1f, 0.3f, 0.3f, 1f));
            SelectCharacter(0);
        }

        LineRenderer NewLine(string n, Color c)
        {
            var g = new GameObject(n); var l = g.AddComponent<LineRenderer>(); l.useWorldSpace = true; l.widthMultiplier = 0.02f; l.sortingOrder = 20;
            l.material = new Material(Shader.Find("Sprites/Default")); l.startColor = l.endColor = c; return l;
        }

        void SelectCharacter(int i)
        {
            m_CharIndex = i; string ch = m_Characters[i]; m_Flip.Character = ch; m_Anims = SpriteLibrary.Animations(ch);
            m_Sr.sprite = null; m_Selected = "";
            if (m_Anims.Length > 0) SelectAnim(m_Anims[0]);
        }

        void SelectAnim(string a)
        {
            if (m_Flip.Play(a, true)) { m_Selected = a; m_Flip.Player.Loop = m_Loop; m_Flip.Speed = m_Speed; m_Flip.Paused = m_Paused; }
        }

        void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame) SceneManager.LoadScene("MainMenu");
            m_Cam.backgroundColor = s_Backgrounds[m_Bg];
            m_Sr.transform.localScale = new Vector3(m_Mirror ? -m_Scale : m_Scale, m_Scale, 1f);
            if (m_Flip.Player != null) { m_Flip.Player.Loop = m_Loop; m_Flip.Speed = m_Speed; m_Flip.Paused = m_Paused; if (!m_Loop && m_Flip.Player.Finished && !m_Paused) { /* hold last frame */ } }
            // gameplay hitbox: the player capsule is 0.6 x 1.3 with its centre 0.68 above the feet; the pivot is the feet
            float x0 = -0.3f, x1 = 0.3f, y0 = 0.03f, y1 = 1.33f;
            m_Hitbox.enabled = m_ShowHitbox; m_Hitbox.positionCount = 5;
            m_Hitbox.SetPositions(new[] { new Vector3(x0, y0), new Vector3(x1, y0), new Vector3(x1, y1), new Vector3(x0, y1), new Vector3(x0, y0) });
            m_Pivot.enabled = m_ShowPivot; m_Pivot.positionCount = 4;
            m_Pivot.SetPositions(new[] { new Vector3(-0.2f, 0), new Vector3(0.2f, 0), new Vector3(0, 0), new Vector3(0, 0.2f) });
        }

        void OnGUI()
        {
            if (m_Btn == null)
            {
                m_Btn = new GUIStyle(GUI.skin.button) { fontSize = 18 }; m_Label = new GUIStyle(GUI.skin.label) { fontSize = 18, normal = { textColor = Color.white } };
                m_Sel = new GUIStyle(m_Btn) { fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.9f, 0.3f) } };
            }
            float w = 330f;
            GUILayout.BeginArea(new Rect(10, 10, w, Screen.height - 20), GUI.skin.box);
            GUILayout.Label("ANIMATION PREVIEW", m_Label);
            GUILayout.Label("Character", m_Label);
            for (int i = 0; i < m_Characters.Length; i++)
            {
                bool has = SpriteLibrary.HasCharacter(m_Characters[i]);
                GUI.enabled = has;
                if (GUILayout.Button(has ? m_Characters[i] : m_Characters[i] + " (no art yet)", i == m_CharIndex ? m_Sel : m_Btn)) SelectCharacter(i);
                GUI.enabled = true;
            }
            GUILayout.Label("Animation", m_Label);
            m_Scroll = GUILayout.BeginScrollView(m_Scroll, GUILayout.Height(Mathf.Min(180, 36 * Mathf.Max(1, m_Anims.Length))));
            foreach (var a in m_Anims) if (GUILayout.Button(a, a == m_Selected ? m_Sel : m_Btn)) SelectAnim(a);
            GUILayout.EndScrollView();
            if (m_Anims.Length == 0) GUILayout.Label("No animations found for this character.", m_Label);
            m_Loop = GUILayout.Toggle(m_Loop, " Loop", m_Label);
            m_Paused = GUILayout.Toggle(m_Paused, " Pause", m_Label);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("< frame", m_Btn)) { m_Paused = true; m_Flip.Paused = true; m_Flip.StepFrame(-1); }
            if (GUILayout.Button("frame >", m_Btn)) { m_Paused = true; m_Flip.Paused = true; m_Flip.StepFrame(1); }
            if (GUILayout.Button("Restart", m_Btn)) { SelectAnim(m_Selected); }
            GUILayout.EndHorizontal();
            GUILayout.Label($"Speed {m_Speed:0.00}x", m_Label); m_Speed = GUILayout.HorizontalSlider(m_Speed, 0.1f, 3f);
            GUILayout.Label($"Scale {m_Scale:0.00}x", m_Label); m_Scale = GUILayout.HorizontalSlider(m_Scale, 0.4f, 2.5f);
            m_Mirror = GUILayout.Toggle(m_Mirror, " Face left", m_Label);
            m_ShowHitbox = GUILayout.Toggle(m_ShowHitbox, " Show gameplay hitbox (green)", m_Label);
            m_ShowPivot = GUILayout.Toggle(m_ShowPivot, " Show pivot / feet (red)", m_Label);
            GUILayout.Label("Background", m_Label);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < s_BgNames.Length; i++) if (GUILayout.Button(s_BgNames[i], i == m_Bg ? m_Sel : m_Btn)) m_Bg = i;
            GUILayout.EndHorizontal();
            if (m_Flip.Current != null) GUILayout.Label($"{m_Flip.Character}/{m_Flip.CurrentName}: frame {m_Flip.Frame + 1}/{m_Flip.Current.Frames.Length} at {m_Flip.Current.Fps} fps", m_Label);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Back to menu (Esc)", m_Btn)) SceneManager.LoadScene("MainMenu");
            GUILayout.EndArea();
        }

        // for tests
        public SpriteFlipbook Flipbook => m_Flip;
        public bool Paused { get => m_Paused; set => m_Paused = value; }
        public IReadOnlyList<string> Animations => m_Anims;
        public void Select(string anim) { SelectAnim(anim); }
    }
}
