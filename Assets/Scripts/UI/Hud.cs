using System.Collections;
using Saiyan.Boss;
using Saiyan.Core;
using Saiyan.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Saiyan.UI
{
    /// <summary>
    /// In-game UI: hearts, Super meter, boss name and health bar with phase ticks, control hints, banner text, and the Pause / Game Over / Victory panels.
    /// Built from code. Panels are plain GameObjects that the level flow shows and hides.
    /// </summary>
    public sealed class Hud : MonoBehaviour
    {
        public System.Action OnResume, OnRetry, OnMenu, OnPlayAgain;
        Canvas m_Canvas; Image[] m_Hearts; Image m_SuperFill, m_SuperGlow; Text m_SuperLabel; GameObject m_BossBar; Image m_BossFill; Text m_Banner, m_Hint;
        GameObject m_Pause, m_Dead, m_Win; Toggle m_AssistT, m_ShakeT; PlayerHealth m_Health; SuperMeter m_Meter; BossHealth m_Boss;
        Sprite m_HeartFull, m_HeartEmpty; float m_BannerT; float m_SuperPulse;

        public bool PauseOpen => m_Pause && m_Pause.activeSelf;
        public bool AnyPanelOpen => PauseOpen || (m_Dead && m_Dead.activeSelf) || (m_Win && m_Win.activeSelf);

        public static Hud Create(PlayerHealth health, SuperMeter meter, BossHealth boss)
        {
            var go = new GameObject("HUD"); var h = go.AddComponent<Hud>(); h.Build(health, meter, boss); return h;
        }

        void Build(PlayerHealth health, SuperMeter meter, BossHealth boss)
        {
            UiKit.EnsureEventSystem();
            m_Health = health; m_Meter = meter; m_Boss = boss;
            m_Canvas = UiKit.CreateCanvas("HudCanvas", 10); m_Canvas.transform.SetParent(transform, false);
            var root = m_Canvas.transform;
            m_HeartFull = PlaceholderArt.Heart(0.9f, new Color(1f, 0.25f, 0.35f)); m_HeartEmpty = PlaceholderArt.Heart(0.9f, new Color(0.45f, 0.4f, 0.45f, 0.7f));
            m_Hearts = new Image[5];
            for (int i = 0; i < 5; i++)
                m_Hearts[i] = UiKit.Panel(root, "Heart" + i, new Vector2(0f, 1f), new Vector2(70f + i * 78f, -70f), new Vector2(70f, 70f), Color.white, m_HeartFull);
            // super meter
            var frame = UiKit.Panel(root, "SuperFrame", new Vector2(0f, 1f), new Vector2(210f, -150f), new Vector2(340f, 38f), Color.white, PlaceholderArt.RoundRect(340f / 64f, 38f / 64f, new Color(0.15f, 0.12f, 0.3f), 0.3f, 3f));
            m_SuperGlow = UiKit.Panel(frame.transform, "Glow", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(356f, 52f), new Color(1f, 0.9f, 0.3f, 0f), PlaceholderArt.RoundRect(356f / 64f, 52f / 64f, Color.white, 0.4f, 0f));
            m_SuperGlow.transform.SetAsFirstSibling();
            var fill = UiKit.Panel(frame.transform, "Fill", new Vector2(0f, 0.5f), new Vector2(170f, 0f), new Vector2(330f, 28f), new Color(1f, 0.82f, 0.2f), PlaceholderArt.RoundRect(330f / 64f, 28f / 64f, Color.white, 0.3f, 0f));
            fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = 0; fill.fillAmount = 0f; m_SuperFill = fill;
            m_SuperLabel = UiKit.Label(frame.transform, "L", "SUPER", 26, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(340f, 38f), Color.white);
            // boss bar
            m_BossBar = new GameObject("BossBar", typeof(RectTransform)); m_BossBar.transform.SetParent(root, false);
            var bb = (RectTransform)m_BossBar.transform; bb.anchorMin = bb.anchorMax = new Vector2(0.5f, 1f); bb.anchoredPosition = new Vector2(0f, -60f); bb.sizeDelta = new Vector2(900f, 90f);
            UiKit.Label(m_BossBar.transform, "Name", "KING CAKEZILLA", 40, new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(900f, 50f), new Color(1f, 0.85f, 0.3f));
            var bf = UiKit.Panel(m_BossBar.transform, "Frame", new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(900f, 34f), Color.white, PlaceholderArt.RoundRect(900f / 64f, 34f / 64f, new Color(0.18f, 0.08f, 0.15f), 0.4f, 3f));
            m_BossFill = UiKit.Panel(bf.transform, "Fill", new Vector2(0f, 0.5f), new Vector2(450f, 0f), new Vector2(884f, 24f), Color.white, PlaceholderArt.RoundRect(884f / 64f, 24f / 64f, new Color(1f, 0.4f, 0.6f), 0.3f, 0f));
            m_BossFill.type = Image.Type.Filled; m_BossFill.fillMethod = Image.FillMethod.Horizontal; m_BossFill.fillOrigin = 0;
            foreach (float f in new[] { 0.70f, 0.35f }) UiKit.Panel(bf.transform, "tick", new Vector2(0f, 0.5f), new Vector2(8f + 884f * f, 0f), new Vector2(4f, 34f), Color.white);
            m_BossBar.SetActive(false);
            m_Banner = UiKit.Label(root, "Banner", "", 110, new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1600f, 200f), new Color(1f, 0.85f, 0.2f));
            m_Hint = UiKit.Label(root, "Hint", "A/D move   Space jump   J shoot   K dash   L super      Pad: A jump  X shoot  B/RB dash  Y super", 24, new Vector2(0f, 0f), new Vector2(800f, 28f), new Vector2(1500f, 40f), new Color(1f, 1f, 1f, 0.9f), TextAnchor.MiddleLeft);
            BuildPause(root); BuildDead(root); BuildWin(root);

            if (m_Health) { m_Health.Changed += OnHearts; OnHearts(m_Health.Current, m_Health.Max); }
            if (m_Meter) m_Meter.Changed += OnMeter;
            if (m_Boss) m_Boss.Changed += OnBoss;
        }

        void OnDestroy()
        {
            if (m_Health) m_Health.Changed -= OnHearts;
            if (m_Meter) m_Meter.Changed -= OnMeter;
            if (m_Boss) m_Boss.Changed -= OnBoss;
        }

        void OnHearts(int cur, int max)
        {
            for (int i = 0; i < m_Hearts.Length; i++) { m_Hearts[i].gameObject.SetActive(i < max); m_Hearts[i].sprite = i < cur ? m_HeartFull : m_HeartEmpty; }
        }
        void OnMeter(float f) { m_SuperFill.fillAmount = f; }
        void OnBoss(float f) { m_BossFill.fillAmount = f; }
        public void ShowBossBar(bool on) { m_BossBar.SetActive(on); if (on && m_Boss) m_BossFill.fillAmount = m_Boss.Fraction; }

        public void Banner(string text, float seconds = 2f) { m_Banner.text = text; m_BannerT = seconds; }

        void Update()
        {
            if (m_BannerT > 0f)
            {
                m_BannerT -= Time.unscaledDeltaTime;
                float a = Mathf.Clamp01(Mathf.Min(m_BannerT * 3f, 1f)); var c = m_Banner.color; c.a = a; m_Banner.color = c;
                if (m_BannerT <= 0f) m_Banner.text = "";
            }
            bool full = m_Meter && m_Meter.Full;
            m_SuperPulse += Time.unscaledDeltaTime * (full ? 8f : 0f);
            var g = m_SuperGlow.color; g.a = full ? 0.5f + 0.5f * Mathf.Sin(m_SuperPulse) : 0f; m_SuperGlow.color = g;
            m_SuperLabel.text = full ? "SUPER READY! (L / Y)" : "SUPER";
        }

        // ---- panels ----
        GameObject MakePanel(Transform root, string name, string title, Color tint)
        {
            var p = new GameObject(name, typeof(RectTransform)); p.transform.SetParent(root, false);
            var r = (RectTransform)p.transform; r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
            var dim = p.AddComponent<Image>(); dim.color = new Color(0.05f, 0.03f, 0.12f, 0.7f);
            UiKit.Panel(p.transform, "Card", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 760f), Color.white, PlaceholderArt.RoundRect(900f / 64f, 640f / 64f, tint, 0.5f, 5f));
            UiKit.Label(p.transform, "Title", title, 90, new Vector2(0.5f, 0.5f), new Vector2(0f, 250f), new Vector2(880f, 140f), new Color(1f, 0.85f, 0.2f));
            p.SetActive(false); return p;
        }

        void BuildPause(Transform root)
        {
            m_Pause = MakePanel(root, "PausePanel", "PAUSED", new Color(0.35f, 0.5f, 0.95f));
            UiKit.Button(m_Pause.transform, "Resume", new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(520f, 90f), () => OnResume?.Invoke());
            UiKit.Button(m_Pause.transform, "Retry Fight", new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(520f, 90f), () => OnRetry?.Invoke());
            m_AssistT = Toggle(m_Pause.transform, "Assist Mode (5 hearts, easier boss)", new Vector2(0f, -100f), GameSettings.AssistMode, v => GameSettings.AssistMode = v);
            m_ShakeT = Toggle(m_Pause.transform, "Reduced screen shake", new Vector2(0f, -170f), GameSettings.ReducedShake, v => GameSettings.ReducedShake = v);
            UiKit.Button(m_Pause.transform, "Main Menu", new Vector2(0.5f, 0.5f), new Vector2(0f, -270f), new Vector2(520f, 90f), () => OnMenu?.Invoke());
        }

        Toggle Toggle(Transform parent, string label, Vector2 pos, bool value, UnityEngine.Events.UnityAction<bool> changed)
        {
            var bg = UiKit.Panel(parent, "T_" + label, new Vector2(0.5f, 0.5f), pos, new Vector2(740f, 56f), Color.white, PlaceholderArt.RoundRect(740f / 64f, 56f / 64f, new Color(1f, 0.95f, 0.85f), 0.4f, 3f));
            var box = UiKit.Panel(bg.transform, "Box", new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(38f, 38f), Color.white, PlaceholderArt.RoundRect(38f / 64f, 38f / 64f, Color.white, 0.2f, 3f));
            var tick = UiKit.Panel(box.transform, "Tick", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24f, 24f), new Color(0.2f, 0.7f, 0.3f), PlaceholderArt.RoundRect(24f / 64f, 24f / 64f, Color.white, 0.2f, 0f));
            UiKit.Label(bg.transform, "Label", label, 30, new Vector2(0.5f, 0.5f), new Vector2(30f, 0f), new Vector2(640f, 50f), new Color(0.2f, 0.1f, 0.1f), TextAnchor.MiddleLeft, false);
            var t = bg.gameObject.AddComponent<Toggle>(); t.targetGraphic = bg; t.graphic = tick; t.isOn = value; t.onValueChanged.AddListener(v => { AudioHooks.Play(Cue.UiClick); changed(v); });
            var cb = t.colors; cb.selectedColor = new Color(0.75f, 1f, 0.8f); t.colors = cb;
            return t;
        }

        void BuildDead(Transform root)
        {
            m_Dead = MakePanel(root, "GameOverPanel", "OH NO!", new Color(0.95f, 0.5f, 0.6f));
            UiKit.Label(m_Dead.transform, "Sub", "Cakezilla won this round.\nTry again!", 40, new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(800f, 140f), Color.white);
            UiKit.Button(m_Dead.transform, "Retry", new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(520f, 100f), () => OnRetry?.Invoke(), 52);
            UiKit.Button(m_Dead.transform, "Main Menu", new Vector2(0.5f, 0.5f), new Vector2(0f, -160f), new Vector2(520f, 90f), () => OnMenu?.Invoke());
        }

        void BuildWin(Transform root)
        {
            m_Win = MakePanel(root, "VictoryPanel", "SWEET VICTORY!", new Color(1f, 0.6f, 0.8f));
            UiKit.Panel(m_Win.transform, "RewardStar", new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(190f, 190f), Color.white, PlaceholderArt.Star(3f, new Color(1f, 0.88f, 0.25f), 5, 0.5f, 4f));
            UiKit.Label(m_Win.transform, "Reward", "Golden Candle Star", 52, new Vector2(0.5f, 0.5f), new Vector2(0f, -50f), new Vector2(800f, 80f), Color.white);
            UiKit.Button(m_Win.transform, "Play Again", new Vector2(0.5f, 0.5f), new Vector2(0f, -150f), new Vector2(520f, 90f), () => OnPlayAgain?.Invoke());
            UiKit.Button(m_Win.transform, "Main Menu", new Vector2(0.5f, 0.5f), new Vector2(0f, -260f), new Vector2(520f, 90f), () => OnMenu?.Invoke());
        }

        public void ShowPause(bool on)
        {
            if (on) { m_AssistT.SetIsOnWithoutNotify(GameSettings.AssistMode); m_ShakeT.SetIsOnWithoutNotify(GameSettings.ReducedShake); }
            m_Pause.SetActive(on);
            if (on) UiKit.Select(m_Pause.transform.Find("Btn_Resume").gameObject);
        }
        public void ShowDead() { m_Dead.SetActive(true); UiKit.Select(m_Dead.transform.Find("Btn_Retry").gameObject); }
        public void ShowWin() { m_Win.SetActive(true); UiKit.Select(m_Win.transform.Find("Btn_Play Again").gameObject); }
        public void HideHint() { if (m_Hint) m_Hint.gameObject.SetActive(false); }
    }
}
