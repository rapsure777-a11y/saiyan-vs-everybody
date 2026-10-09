using Saiyan.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Saiyan.UI
{
    /// <summary>Small helpers that build uGUI elements from code (no prefabs yet). Colors follow the concept art: candy pink, gold, deep blue.</summary>
    public static class UiKit
    {
        public static Font Font => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        public static readonly Color Pink = new Color(1f, 0.45f, 0.65f), DeepBlue = new Color(0.1f, 0.2f, 0.55f), Cream = new Color(1f, 0.95f, 0.82f);

        public static Canvas CreateCanvas(string name, int order)
        {
            var go = new GameObject(name);
            var c = go.AddComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = order;
            var s = go.AddComponent<CanvasScaler>(); s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; s.referenceResolution = new Vector2(1920, 1080); s.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return c;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>()) return;
            var go = new GameObject("EventSystem"); go.AddComponent<EventSystem>(); go.AddComponent<InputSystemUIInputModule>();
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform; r.anchorMin = anchorMin; r.anchorMax = anchorMax; r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = pos; r.sizeDelta = size; return r;
        }

        public static Image Panel(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color color, Sprite sprite = null)
        {
            var r = Rect(parent, name, anchor, anchor, pos, size); var img = r.gameObject.AddComponent<Image>(); img.color = color;
            if (sprite) { img.sprite = sprite; img.type = Image.Type.Simple; }
            return img;
        }

        public static Text Label(Transform parent, string name, string text, int size, Vector2 anchor, Vector2 pos, Vector2 boxSize, Color color, TextAnchor align = TextAnchor.MiddleCenter, bool outline = true)
        {
            var r = Rect(parent, name, anchor, anchor, pos, boxSize); var t = r.gameObject.AddComponent<Text>();
            t.font = Font; t.text = text; t.fontSize = size; t.color = color; t.alignment = align; t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow; t.raycastTarget = false;
            if (outline) { var o = r.gameObject.AddComponent<Outline>(); o.effectColor = new Color(0.16f, 0.09f, 0.12f, 1f); o.effectDistance = new Vector2(3f, -3f); }
            return t;
        }

        public static Button Button(Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction onClick, int fontSize = 44)
        {
            // the sprite is generated at the button's own pixel size (64 px per unit), so the rounded corners are not stretched
            var img = Panel(parent, "Btn_" + label, anchor, pos, size, Color.white, PlaceholderArt.RoundRect(size.x / PlaceholderArt.Ppu, size.y / PlaceholderArt.Ppu, new Color(1f, 0.8f, 0.3f), 0.4f, 4f));
            var b = img.gameObject.AddComponent<Button>(); b.targetGraphic = img;
            var cb = b.colors; cb.normalColor = Color.white; cb.highlightedColor = new Color(1f, 0.95f, 0.75f); cb.selectedColor = new Color(0.75f, 1f, 0.8f); cb.pressedColor = new Color(0.8f, 0.7f, 0.4f); b.colors = cb;
            var nav = b.navigation; nav.mode = Navigation.Mode.Automatic; b.navigation = nav;
            b.onClick.AddListener(() => AudioHooks.Play(Cue.UiClick));
            b.onClick.AddListener(onClick);
            var t = Label(img.transform, "Label", label, fontSize, new Vector2(0.5f, 0.5f), Vector2.zero, size, new Color(0.2f, 0.1f, 0.1f), TextAnchor.MiddleCenter, false);
            return b;
        }

        public static void Select(GameObject go) { if (EventSystem.current) { EventSystem.current.SetSelectedGameObject(null); EventSystem.current.SetSelectedGameObject(go); } }
    }
}
