using NUnit.Framework;
using UnityEngine;

namespace UnityTools.Vfx.Editor.Tests
{
    public class VfxFrameSelectorTests
    {
        //============================================================
        // Logic
        //============================================================
        [Test]
        public void UniformFramesHaveNoForeground()
        {
            var pixels = new Color[64];
            for (int idx = 0; idx < pixels.Length; idx++)
            {
                pixels[idx] = Color.gray;
            }

            Assert.That(VfxFrameSelector.Score(pixels, 8, 8), Is.EqualTo(0.0f));
            pixels[27] = Color.white;
            pixels[28] = Color.white;
            Assert.That(VfxFrameSelector.Score(pixels, 8, 8), Is.GreaterThan(0.0f));
        }

        [Test]
        public void CenteredForegroundScoresHigher()
        {
            var centered = new Color[64];
            var edge = new Color[64];
            centered[27] = Color.white;
            edge[9] = Color.white;
            Assert.That(VfxFrameSelector.Score(centered, 8, 8), Is.GreaterThan(VfxFrameSelector.Score(edge, 8, 8)));
        }

        [Test]
        public void InvalidRequestsFailWithoutCapture()
        {
            Assert.That(VfxFrameSelector.TrySelect(null, 1.0f, 36, out _), Is.False);
            Assert.That(VfxFrameSelector.Score(new Color[4], int.MaxValue, 2), Is.Zero);
        }
    }
}
