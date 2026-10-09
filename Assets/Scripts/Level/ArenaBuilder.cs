using Saiyan.Boss;
using Saiyan.Core;
using Saiyan.Player;
using UnityEngine;

namespace Saiyan.Level
{
    /// <summary>
    /// Builds Frosting Fields from placeholder shapes: sky, parallax candy castles, the cake-slab ground, the tutorial (signs, obstacles, dummies, pickups, checkpoint) and the
    /// boss arena (two low one-way platforms, an invisible wall in front of the boss, a left wall that closes when the fight starts).
    /// </summary>
    public sealed class ArenaBuilder
    {
        public Transform Root; public GameObject LeftWall, BossWall;
        public readonly System.Collections.Generic.List<Transform> Platforms = new System.Collections.Generic.List<Transform>();

        public static ArenaBuilder Build(CameraRig rig, SuperMeter meter)
        {
            var b = new ArenaBuilder { Root = new GameObject("Level01_FrostingFields").transform };
            b.Sky(rig); b.Castles(rig); b.Ground(); b.TutorialProps(meter); b.Arena();
            return b;
        }

        void Sky(CameraRig rig)
        {
            var sky = new GameObject("Sky"); sky.transform.SetParent(Root, false);
            var sr = sky.AddComponent<SpriteRenderer>(); sr.sprite = PlaceholderArt.Gradient(Palette.SkyTop, Palette.SkyBottom); sr.sortingOrder = -100;
            // gradient sprite is 4x64 px at 4 ppu: 1 x 16 units; scale to the screen
            sky.transform.localScale = new Vector3(LevelLayout.CameraWidth * 1.15f, LevelLayout.CameraSize * 2f * 1.15f / 16f, 1f);
            rig.SetSky(sky.transform);
        }

        void Castles(CameraRig rig)
        {
            // far layer: pale candy castles and clouds; near layer: gumdrop hills
            var far = new GameObject("FarLayer"); far.transform.SetParent(Root, false);
            for (int i = 0; i < 4; i++)
            {
                float x = i * 10f + 5f;
                var sp = far.transform;
                float h = 3.5f + (i % 2) * 1.3f;
                var body = PlaceholderArt.Part(sp, "castle" + i, PlaceholderArt.RoundRect(3.2f, h, new Color(1f, 0.93f, 0.88f), 0.2f, 3f), new Vector2(x, 1.4f + h * 0.5f), -90);
                PlaceholderArt.Part(sp, "roofA" + i, PlaceholderArt.Star(1.4f, Palette.Frosting, 3, 0.5f, 3f), new Vector2(x - 0.9f, 1.4f + h + 0.45f), -89);
                PlaceholderArt.Part(sp, "roofB" + i, PlaceholderArt.Star(1.2f, Palette.Frosting, 3, 0.5f, 3f), new Vector2(x + 0.9f, 1.4f + h + 0.3f), -89);
                PlaceholderArt.Part(sp, "door" + i, PlaceholderArt.RoundRect(0.8f, 1.2f, Palette.Sponge, 0.35f, 3f), new Vector2(x, 2.0f), -89);
                PlaceholderArt.Part(sp, "cloud" + i, PlaceholderArt.Ellipse(3.2f, 1.1f, new Color(1f, 1f, 1f, 0.95f), 2f), new Vector2(x + 2.5f, 7.6f + (i % 3) * 0.7f), -95);
            }
            far.transform.position = new Vector3(0, 0, 0);
            rig.AddLayer(far.transform, 0.85f, 10f);
            var near = new GameObject("NearLayer"); near.transform.SetParent(Root, false);
            for (int i = 0; i < 8; i++)
            {
                float x = i * 5f + 2f; float w = 3.6f + (i % 2);
                PlaceholderArt.Part(near.transform, "hill" + i, PlaceholderArt.Ellipse(w, 2.6f, i % 2 == 0 ? new Color(1f, 0.7f, 0.82f) : new Color(0.98f, 0.82f, 0.62f), 3f), new Vector2(x, 0.4f), -70);
            }
            rig.AddLayer(near.transform, 0.6f, 5f * 4f / 2f);
        }

        void Ground()
        {
            float x0 = LevelLayout.LevelStartX, x1 = LevelLayout.LevelEndX; float w = x1 - x0;
            var g = new GameObject("Ground"); g.transform.SetParent(Root, false); g.transform.position = new Vector3((x0 + x1) * 0.5f, LevelLayout.FloorY - 1.5f, 0f);
            var col = g.AddComponent<BoxCollider2D>(); col.size = new Vector2(w, 3f);
            for (float x = x0; x < x1; x += 4f)
            {
                var seg = new GameObject("Slab"); seg.transform.SetParent(Root, false); seg.transform.position = new Vector3(x + 2f, LevelLayout.FloorY - 1.5f, 0f);
                PlaceholderArt.Part(seg.transform, "sponge", PlaceholderArt.RoundRect(4.02f, 3f, Palette.Sponge, 0.1f, 3f), Vector2.zero, 0);
                PlaceholderArt.Part(seg.transform, "wafer", PlaceholderArt.RoundRect(3.5f, 0.35f, new Color(0.95f, 0.78f, 0.5f), 0.1f, 2f), new Vector2(0f, -0.7f), 1);
                PlaceholderArt.Part(seg.transform, "frost", PlaceholderArt.RoundRect(4.05f, 0.7f, Palette.Frosting, 0.3f, 3f), new Vector2(0f, 1.3f), 2);
                int k = Mathf.RoundToInt(x * 0.25f);
                PlaceholderArt.Part(seg.transform, "drip", PlaceholderArt.Ellipse(0.5f, 0.9f, Palette.Frosting, 3f), new Vector2(-1.2f + (k % 3) * 0.7f, 0.9f), 2);
                PlaceholderArt.Part(seg.transform, "spr1", PlaceholderArt.RoundRect(0.22f, 0.08f, new Color(1f, 0.9f, 0.3f), 0.03f, 0f), new Vector2(-0.8f, 1.35f), 3, 20f * k);
                PlaceholderArt.Part(seg.transform, "spr2", PlaceholderArt.RoundRect(0.22f, 0.08f, new Color(0.4f, 0.7f, 1f), 0.03f, 0f), new Vector2(0.4f, 1.28f), 3, -30f * k);
                PlaceholderArt.Part(seg.transform, "spr3", PlaceholderArt.RoundRect(0.22f, 0.08f, Color.white, 0.03f, 0f), new Vector2(1.4f, 1.4f), 3, 50f);
            }
        }

        void TutorialProps(SuperMeter meter)
        {
            // signs
            SignFactory.Create(Root, 3f, "RUN\nA / D or left stick");
            SignFactory.Create(Root, 9f, "JUMP\nSpace or A\n(hold for higher)", 3.8f);
            Block(14.5f, 2.2f, 1.1f);
            Block(20f, 2.8f, 2.0f);
            SignFactory.Create(Root, 17.2f, "Tall one?\nHold jump!", 3.2f);
            SignFactory.Create(Root, 27f, "DASH\nK or B/RB\n(works in the air too)", 3.9f);
            for (int i = 0; i < 5; i++) StarPickup.Create(Root, new Vector2(31f + i * 2.6f, 1.2f + (i % 2) * 2.4f));
            SignFactory.Create(Root, 44f, "SHOOT\nJ or X\n(hold to keep firing)", 3.9f);
            for (int i = 0; i < 3; i++) TargetDummy.Create(Root, new Vector2(49f + i * 3f, 0.7f + (i == 1 ? 2.6f : 0f)), meter);
            SignFactory.Create(Root, 58f, "SUPER\nL or Y when the\nmeter is full", 3.9f);
            CheckpointFlag.Reached = false;
            CheckpointFlag.Create(Root, LevelLayout.CheckpointX);
            SignFactory.Create(Root, LevelLayout.CheckpointX + 3f, "BOSS AHEAD!", 3f);
        }

        void Block(float x, float w, float h)
        {
            var go = new GameObject("CakeBlock"); go.transform.SetParent(Root, false); go.transform.position = new Vector3(x, LevelLayout.FloorY + h * 0.5f, 0f);
            var col = go.AddComponent<BoxCollider2D>(); col.size = new Vector2(w, h);
            PlaceholderArt.Part(go.transform, "sponge", PlaceholderArt.RoundRect(w, h, Palette.Sponge, 0.18f, 3f), Vector2.zero, 4);
            PlaceholderArt.Part(go.transform, "frost", PlaceholderArt.RoundRect(w + 0.1f, 0.45f, Palette.Frosting, 0.2f, 3f), new Vector2(0f, h * 0.5f - 0.1f), 5);
            PlaceholderArt.Part(go.transform, "drip", PlaceholderArt.Ellipse(0.38f, 0.6f, Palette.Frosting, 3f), new Vector2(-w * 0.25f, h * 0.5f - 0.45f), 5);
        }

        void Arena()
        {
            float L = LevelLayout.ArenaLeft;
            float[] cx = { L + 4.2f, L + 9.4f };
            for (int i = 0; i < 2; i++)
            {
                var p = new GameObject("Platform" + i); p.transform.SetParent(Root, false); p.transform.position = new Vector3(cx[i], 2.0f, 0f);
                var col = p.AddComponent<BoxCollider2D>(); col.size = new Vector2(3.2f, 0.45f); col.offset = new Vector2(0f, -0.2f); col.usedByEffector = true;
                var eff = p.AddComponent<PlatformEffector2D>(); eff.useOneWay = true; eff.surfaceArc = 160f; eff.useSideFriction = false;
                PlaceholderArt.Part(p.transform, "wafer", PlaceholderArt.RoundRect(3.2f, 0.55f, new Color(0.95f, 0.78f, 0.5f), 0.18f, 3f), new Vector2(0f, -0.2f), 5);
                PlaceholderArt.Part(p.transform, "frost", PlaceholderArt.RoundRect(3.3f, 0.28f, Palette.Frosting, 0.12f, 3f), new Vector2(0f, 0.02f), 6);
                Platforms.Add(p.transform);
            }
            BossWall = Wall("BossWall", LevelLayout.BossWallX + 0.5f, true);
            LeftWall = Wall("ArenaLeftWall", L - 0.5f, false); LeftWall.SetActive(false);
        }

        GameObject Wall(string name, float x, bool passShots)
        {
            var w = new GameObject(name); w.transform.SetParent(Root, false); w.transform.position = new Vector3(x, 8f, 0f);
            var col = w.AddComponent<BoxCollider2D>(); col.size = new Vector2(1f, 24f);
            col.sharedMaterial = new PhysicsMaterial2D("WallNoFriction") { friction = 0f, bounciness = 0f };
            if (passShots) w.AddComponent<ShotPassThrough>();
            return w;
        }
    }
}
