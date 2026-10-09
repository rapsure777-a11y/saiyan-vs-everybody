using Saiyan.Core;
using Saiyan.Level;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Saiyan.UI
{
    /// <summary>Main menu: title, Play, Controls (with the accessibility options), Quit. Built from code over a placeholder candy sky.</summary>
    public sealed class MainMenu : MonoBehaviour
    {
        GameObject m_Main, m_Controls; Transform m_Star1, m_Star2;

        void Awake()
        {
            Time.timeScale = 1f; GameSession.Reset();
            var cam = Camera.main;
            if (!cam) { var cg = new GameObject("Main Camera") { tag = "MainCamera" }; cam = cg.AddComponent<Camera>(); cg.AddComponent<AudioListener>(); }
            cam.orthographic = true; cam.orthographicSize = 5.6f; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Palette.SkyBottom; cam.transform.position = new Vector3(0, 0, -10);
            var sky = new GameObject("Sky"); var sr = sky.AddComponent<SpriteRenderer>(); sr.sprite = PlaceholderArt.Gradient(Palette.SkyTop, Palette.SkyBottom); sr.sortingOrder = -100;
            sky.transform.localScale = new Vector3(22f, 12f / 16f * 1.2f, 1f); sky.transform.position = new Vector3(0, 0, 5);
            // decoration: candy hills and bobbing stars
            for (int i = 0; i < 6; i++) PlaceholderArt.Part(null, "hill", PlaceholderArt.Ellipse(5f + i % 2, 3f, i % 2 == 0 ? new Color(1f, 0.7f, 0.82f) : new Color(0.98f, 0.82f, 0.62f), 3f), new Vector2(-10f + i * 4f, -5f), -10 + i % 2);
            m_Star1 = PlaceholderArt.Part(null, "star1", PlaceholderArt.Star(1.6f, Palette.Gold), new Vector2(-7f, 3f), 5).transform;
            m_Star2 = PlaceholderArt.Part(null, "star2", PlaceholderArt.Star(1.1f, Palette.Gold), new Vector2(7.5f, 1.5f), 5).transform;

            UiKit.EnsureEventSystem();
            var canvas = UiKit.CreateCanvas("MenuCanvas", 5); canvas.transform.SetParent(transform, false);
            m_Main = new GameObject("Main", typeof(RectTransform)); Stretch(m_Main, canvas.transform);
            UiKit.Label(m_Main.transform, "Title1", "SAIYAN", 190, new Vector2(0.5f, 0.5f), new Vector2(0f, 330f), new Vector2(1500f, 220f), new Color(1f, 0.85f, 0.15f));
            UiKit.Label(m_Main.transform, "Title2", "vs. EVERYBODY!", 120, new Vector2(0.5f, 0.5f), new Vector2(0f, 180f), new Vector2(1500f, 160f), new Color(1f, 0.35f, 0.25f));
            var play = UiKit.Button(m_Main.transform, "Play", new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(520f, 110f), () => SceneManager.LoadScene(LevelFlow.SceneName), 56);
            UiKit.Button(m_Main.transform, "Controls", new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(520f, 100f), () => Show(false), 48);
            UiKit.Button(m_Main.transform, "Quit", new Vector2(0.5f, 0.5f), new Vector2(0f, -230f), new Vector2(520f, 100f), Quit, 48);
            UiKit.Label(m_Main.transform, "Note", "Vertical slice: Frosting Fields and King Cakezilla (placeholder art)", 26, new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(1500f, 40f), new Color(1f, 1f, 1f, 0.9f));

            m_Controls = new GameObject("Controls", typeof(RectTransform)); Stretch(m_Controls, canvas.transform);
            UiKit.Panel(m_Controls.transform, "Card", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1300f, 880f), Color.white, PlaceholderArt.RoundRect(1300f / 64f, 880f / 64f, new Color(0.3f, 0.45f, 0.9f), 0.5f, 5f));
            UiKit.Label(m_Controls.transform, "T", "CONTROLS", 80, new Vector2(0.5f, 0.5f), new Vector2(0f, 360f), new Vector2(1200f, 110f), new Color(1f, 0.85f, 0.2f));
            UiKit.Label(m_Controls.transform, "Keys",
                "KEYBOARD\nMove: A / D or arrows     Jump: Space (hold = higher)\nShoot: J     Dash: K     Super: L     Pause: Esc\n\nGAMEPAD\nMove: left stick / d-pad     Jump: A     Shoot: X\nDash: B or RB     Super: Y     Pause: Start",
                36, new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), new Vector2(1200f, 420f), Color.white);
            UiKit.Label(m_Controls.transform, "OptT", "OPTIONS", 44, new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(600f, 60f), new Color(1f, 0.85f, 0.2f));
            MakeToggle(m_Controls.transform, "Assist Mode (5 hearts, easier boss)", new Vector2(0f, -180f), GameSettings.AssistMode, v => GameSettings.AssistMode = v);
            MakeToggle(m_Controls.transform, "Reduced screen shake", new Vector2(0f, -250f), GameSettings.ReducedShake, v => GameSettings.ReducedShake = v);
            UiKit.Button(m_Controls.transform, "Back", new Vector2(0.5f, 0.5f), new Vector2(0f, -350f), new Vector2(400f, 90f), () => Show(true), 46);
            m_Controls.SetActive(false);
            UiKit.Select(play.gameObject);
        }

        static void Stretch(GameObject go, Transform parent)
        {
            go.transform.SetParent(parent, false); var r = (RectTransform)go.transform; r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
        }

        void Show(bool main)
        {
            m_Main.SetActive(main); m_Controls.SetActive(!main);
            UiKit.Select((main ? m_Main : m_Controls).transform.Find(main ? "Btn_Play" : "Btn_Back").gameObject);
        }

        void MakeToggle(Transform parent, string label, Vector2 pos, bool value, UnityEngine.Events.UnityAction<bool> changed)
        {
            var bg = UiKit.Panel(parent, "T_" + label, new Vector2(0.5f, 0.5f), pos, new Vector2(760f, 58f), Color.white, PlaceholderArt.RoundRect(760f / 64f, 58f / 64f, new Color(1f, 0.95f, 0.85f), 0.4f, 3f));
            var box = UiKit.Panel(bg.transform, "Box", new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(38f, 38f), Color.white, PlaceholderArt.RoundRect(38f / 64f, 38f / 64f, Color.white, 0.2f, 3f));
            var tick = UiKit.Panel(box.transform, "Tick", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24f, 24f), new Color(0.2f, 0.7f, 0.3f), PlaceholderArt.RoundRect(24f / 64f, 24f / 64f, Color.white, 0.2f, 0f));
            UiKit.Label(bg.transform, "Label", label, 30, new Vector2(0.5f, 0.5f), new Vector2(30f, 0f), new Vector2(660f, 50f), new Color(0.2f, 0.1f, 0.1f), TextAnchor.MiddleLeft, false);
            var t = bg.gameObject.AddComponent<Toggle>(); t.targetGraphic = bg; t.graphic = tick; t.isOn = value; t.onValueChanged.AddListener(v => { AudioHooks.Play(Cue.UiClick); changed(v); });
        }

        void Update()
        {
            m_Star1.localPosition = new Vector3(-7f, 3f + Mathf.Sin(Time.time * 1.6f) * 0.3f, 0f); m_Star1.localRotation = Quaternion.Euler(0, 0, Time.time * 20f);
            m_Star2.localPosition = new Vector3(7.5f, 1.5f + Mathf.Sin(Time.time * 2f + 1f) * 0.25f, 0f); m_Star2.localRotation = Quaternion.Euler(0, 0, -Time.time * 25f);
        }

        void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
