using System.Collections.Generic;
using NUnit.Framework;
using Saiyan.Art;

namespace Saiyan.Tests
{
    public class ClipPlayerTests
    {
        [Test]
        public void Loops_AtTheClipFrameRate_AndWraps()
        {
            var p = new ClipPlayer(8, 8f, true);
            p.Tick(0.13f); Assert.AreEqual(1, p.Frame);          // 8 fps: a frame every 0.125 s
            p.Tick(0.13f); Assert.AreEqual(2, p.Frame);          // (values chosen away from exact frame boundaries: float rounding)
            p.Tick(0.76f); Assert.AreEqual(0, p.Frame);          // a full second: back at the start
            Assert.IsFalse(p.Finished);
        }

        [Test]
        public void EveryFrameEntered_IsReported_EvenAfterALongFrame()
        {
            var p = new ClipPlayer(8, 8f, true); var seen = new List<int>(); p.FrameEntered = seen.Add;
            p.Tick(0.52f);                                        // 4 frames in one step
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, seen);
        }

        [Test]
        public void NonLooping_HoldsTheLastFrame_AndFinishesOnce()
        {
            var p = new ClipPlayer(4, 8f, false); var seen = new List<int>(); p.FrameEntered = seen.Add;
            for (int i = 0; i < 30; i++) p.Tick(0.1f);
            Assert.IsTrue(p.Finished); Assert.AreEqual(3, p.Frame);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, seen);
        }

        [Test]
        public void Speed_Pause_AndManualStepping()
        {
            var p = new ClipPlayer(8, 8f, true) { Speed = 2f };
            p.Tick(0.13f); Assert.AreEqual(2, p.Frame);           // double speed
            p.Paused = true; p.Tick(1f); Assert.AreEqual(2, p.Frame);
            p.SetFrame(6); Assert.AreEqual(6, p.Frame);
            p.Paused = false; p.Tick(0.13f); Assert.AreEqual(0, p.Frame);   // continues from the stepped frame: 2 frames at double speed (7, then wraps to 0)
        }

        [Test]
        public void ClipEventMeta_And_AnimMeta_ParseFromJson()
        {
            var m = UnityEngine.JsonUtility.FromJson<AnimMeta>("{\"character\":\"Saiyan\",\"name\":\"shoot\",\"fps\":12,\"loop\":false,\"ppu\":200,\"frames\":6,\"pivot\":[0.51,0.03],\"events\":[{\"frame\":2,\"name\":\"fire\"}]}");
            Assert.AreEqual(12f, m.fps); Assert.IsFalse(m.loop); Assert.AreEqual(2, m.events[0].frame); Assert.AreEqual("fire", m.events[0].name); Assert.AreEqual(0.51f, m.pivot[0], 1e-4f);
        }
    }
}
