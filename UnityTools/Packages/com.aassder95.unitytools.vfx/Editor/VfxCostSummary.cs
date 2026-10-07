using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Vfx.Editor
{
    public class VfxCostSummary
    {
        //============================================================
        // Fields
        //============================================================
        private int _systemCnt;
        private int _rendererCnt;
        private int _materialSlotCnt;
        private int _uniqueMaterialCnt;
        private long _maxParticleCnt;
        private int _peakParticleCnt;
        private float _sampleDurationSec;
        private int _sampleCnt;

        //============================================================
        // Properties
        //============================================================
        public int SystemCnt => _systemCnt;
        public int RendererCnt => _rendererCnt;
        public int MaterialSlotCnt => _materialSlotCnt;
        public int UniqueMaterialCnt => _uniqueMaterialCnt;
        public long MaxParticleCnt => _maxParticleCnt;
        public int PeakParticleCnt => _peakParticleCnt;
        public float SampleDurationSec => _sampleDurationSec;
        public int SampleCnt => _sampleCnt;

        //============================================================
        // Logic
        //============================================================
        public static bool TryAnalyze(GameObject prefab, float durationSec, int sampleCnt, out VfxCostSummary summary)
        {
            summary = null;
            if (prefab == null || !UnityEditor.PrefabUtility.IsPartOfPrefabAsset(prefab) || float.IsNaN(durationSec) || float.IsInfinity(durationSec) || durationSec <= 0.0f || durationSec > VfxPreviewSession.MAX_PREVIEW_SEC || sampleCnt < 2 || sampleCnt > 301)
                return false;

            ParticleSystem[] systems = prefab.GetComponentsInChildren<ParticleSystem>(true);
            if (systems.Length == 0)
                return false;

            VfxCostSummary result = new VfxCostSummary();
            result._systemCnt = systems.Length;
            for (int idx = 0; idx < systems.Length; idx++)
            {
                result._maxParticleCnt += systems[idx].main.maxParticles;
            }

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            HashSet<Material> materials = new HashSet<Material>();
            for (int idx = 0; idx < renderers.Length; idx++)
            {
                Renderer renderer = renderers[idx];
                if (!(renderer is ParticleSystemRenderer) && !(renderer is MeshRenderer) && !(renderer is SpriteRenderer))
                    continue;

                result._rendererCnt++;
                Material[] slots = renderer.sharedMaterials;
                result._materialSlotCnt += slots.Length;
                for (int slotIdx = 0; slotIdx < slots.Length; slotIdx++)
                {
                    if (slots[slotIdx] != null)
                        materials.Add(slots[slotIdx]);
                }
            }

            result._uniqueMaterialCnt = materials.Count;
            result._sampleDurationSec = durationSec;
            result._sampleCnt = sampleCnt;
            using (VfxPreviewSession preview = new VfxPreviewSession(prefab))
            {
                if (!preview.TrySeek(0.0f))
                    return false;

                float stepSec = durationSec / (sampleCnt - 1);
                for (int idx = 0; idx < sampleCnt; idx++)
                {
                    if (idx > 0)
                        preview.Advance(stepSec);

                    result._peakParticleCnt = Mathf.Max(result._peakParticleCnt, preview.ParticleCnt);
                }
            }

            summary = result;
            return true;
        }
    }
}
