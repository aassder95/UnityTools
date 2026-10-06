using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace UnityTools.Vfx.Editor.Tests
{
    public class VfxBrowserTests
    {
        //============================================================
        // Fields
        //============================================================
        private string _folderPath;
        private GameObject _prefab;
        private VfxPreviewSession _preview;

        //============================================================
        // Init/Register
        //============================================================
        [SetUp]
        public void SetUp()
        {
            string folderName = "UnityToolsVfxTests-" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", folderName);
            _folderPath = "Assets/" + folderName;
            GameObject go = new GameObject("불꽃 Spark", typeof(ParticleSystem), typeof(AudioSource), typeof(Animator));
            ParticleSystem system = go.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = system.main;
            main.startLifetime = 2.0f;
            main.startSpeed = 1.0f;
            main.startSize = 0.4f;
            main.startColor = new Color(1.0f, 0.3f, 0.04f);
            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = false;
            Material material = new Material(Shader.Find("Particles/Standard Unlit"));
            AssetDatabase.CreateAsset(material, _folderPath + "/Spark.mat");
            system.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
            _prefab = PrefabUtility.SaveAsPrefabAsset(go, _folderPath + "/불꽃 Spark.prefab");
            Object.DestroyImmediate(go);
            AssetDatabase.SaveAssets();
        }

        [TearDown]
        public void TearDown()
        {
            _preview?.Dispose();
            _preview = null;
            AssetDatabase.DeleteAsset(_folderPath);
        }

        //============================================================
        // Logic
        //============================================================
        [Test]
        public void CatalogIncludesOnlyParticlePrefabs()
        {
            GameObject go = new GameObject("Plain");
            PrefabUtility.SaveAsPrefabAsset(go, _folderPath + "/Plain.prefab");
            Object.DestroyImmediate(go);
            VfxPrefabCatalog catalog = new VfxPrefabCatalog();
            Assert.That(catalog.TryRefresh(_folderPath), Is.True);
            Assert.That(catalog.Items.Count, Is.EqualTo(1));
            Assert.That(catalog.Items[0].ParticleSystemCnt, Is.EqualTo(1));
            Assert.That(catalog.Items[0].IsLooping, Is.True);
        }

        [Test]
        public void InvalidFolderKeepsPreviousIndex()
        {
            VfxPrefabCatalog catalog = new VfxPrefabCatalog();
            Assert.That(catalog.TryRefresh(_folderPath), Is.True);
            Assert.That(catalog.TryRefresh("Assets/ThisFolderDoesNotExist"), Is.False);
            Assert.That(catalog.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void FilterRequiresEveryTokenAndSupportsKorean()
        {
            VfxPrefabCatalog catalog = new VfxPrefabCatalog();
            Assert.That(catalog.TryRefresh(_folderPath), Is.True);
            List<VfxPrefabInfo> results = new List<VfxPrefabInfo>();
            HashSet<string> favorites = new HashSet<string>();
            catalog.Filter("불꽃 SPARK", EVfxLoopFilter.All, favorites, false, results);
            Assert.That(results.Count, Is.EqualTo(1));
            catalog.Filter("불꽃 Water", EVfxLoopFilter.All, favorites, false, results);
            Assert.That(results, Is.Empty);
            catalog.Filter("Spark", EVfxLoopFilter.OneShot, favorites, false, results);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void FavoriteGuidSurvivesPrefabRename()
        {
            VfxPrefabCatalog catalog = new VfxPrefabCatalog();
            Assert.That(catalog.TryRefresh(_folderPath), Is.True);
            HashSet<string> favorites = new HashSet<string> { catalog.Items[0].Guid };
            string error = AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(_prefab), _folderPath + "/Renamed.prefab");
            Assert.That(error, Is.Empty);
            Assert.That(catalog.TryRefresh(_folderPath), Is.True);
            List<VfxPrefabInfo> results = new List<VfxPrefabInfo>();
            catalog.Filter(string.Empty, EVfxLoopFilter.Looping, favorites, true, results);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].AssetPath, Does.EndWith("Renamed.prefab"));
        }

        [Test]
        public void PreviewContainsVisualsWithoutScriptsAudioOrAnimator()
        {
            _preview = new VfxPreviewSession(_prefab);
            GameObject root = PreviewRoot();
            Assert.That(root.GetComponentsInChildren<ParticleSystem>(true).Length, Is.EqualTo(1));
            Assert.That(root.GetComponentsInChildren<AudioSource>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Animator>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
            Assert.That(root.scene, Is.Not.EqualTo(UnityEngine.SceneManagement.SceneManager.GetActiveScene()));
        }

        [Test]
        public void SimulationDoesNotChangeSourcePrefabOrMaterial()
        {
            string path = AssetDatabase.GetAssetPath(_prefab);
            string materialPath = _folderPath + "/Spark.mat";
            byte[] prefabBytes = File.ReadAllBytes(path);
            byte[] materialBytes = File.ReadAllBytes(materialPath);
            bool wasDirty = EditorUtility.IsDirty(_prefab);
            _preview = new VfxPreviewSession(_prefab);
            Assert.That(_preview.TrySeek(1.0f), Is.True);
            _preview.Advance(0.5f);
            _preview.Dispose();
            Assert.That(File.ReadAllBytes(path), Is.EqualTo(prefabBytes));
            Assert.That(File.ReadAllBytes(materialPath), Is.EqualTo(materialBytes));
            Assert.That(EditorUtility.IsDirty(_prefab), Is.EqualTo(wasDirty));
            Assert.That(EditorUtility.IsDirty(AssetDatabase.LoadAssetAtPath<Material>(materialPath)), Is.False);
            Assert.That(_prefab.GetComponent<ParticleSystem>().main.playOnAwake, Is.True);
            Assert.That(_prefab.GetComponent<ParticleSystem>().particleCount, Is.Zero);
        }

        [Test]
        public void SeekingUsesRepeatableSimulationAndRejectsInvalidTime()
        {
            _preview = new VfxPreviewSession(_prefab);
            Assert.That(_preview.TrySeek(1.0f), Is.True);
            int cnt = _preview.ParticleCnt;
            Assert.That(cnt, Is.GreaterThan(0));
            _preview.Advance(0.3f);
            Assert.That(_preview.TrySeek(1.0f), Is.True);
            Assert.That(_preview.ParticleCnt, Is.EqualTo(cnt));
            Assert.That(_preview.TrySeek(float.NaN), Is.False);
            Assert.That(_preview.TrySeek(-1.0f), Is.False);
            Assert.That(_preview.TrySeek(VfxPreviewSession.MAX_PREVIEW_SEC + 1.0f), Is.False);
            Assert.That(_preview.TimeSec, Is.EqualTo(1.0f));
        }

        [Test]
        public void DisposalDestroysOwnedObjectsAndIsRepeatable()
        {
            _preview = new VfxPreviewSession(_prefab);
            GameObject root = PreviewRoot();
            _preview.Dispose();
            _preview.Dispose();
            Assert.That(root == null, Is.True);
            Assert.That(_prefab == null, Is.False);
            Assert.That(_preview.TrySeek(0.0f), Is.False);
        }

        [Test]
        public void InactivePrefabRootCanBePreviewed()
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(_prefab));
            contents.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(contents, AssetDatabase.GetAssetPath(_prefab));
            PrefabUtility.UnloadPrefabContents(contents);
            _preview = new VfxPreviewSession(_prefab);
            Assert.That(_preview.TrySeek(1.0f), Is.True);
            Assert.That(_preview.ParticleCnt, Is.GreaterThan(0));
            Assert.That(_prefab.activeSelf, Is.False);
        }

        [Test]
        public void SubEmitterAndCustomSpaceReferencesUsePreviewCopies()
        {
            string path = AssetDatabase.GetAssetPath(_prefab);
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            GameObject child = new GameObject("Sub Emitter", typeof(ParticleSystem));
            child.transform.SetParent(contents.transform, false);
            ParticleSystem parent = contents.GetComponent<ParticleSystem>();
            ParticleSystem subSystem = child.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = parent.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Custom;
            main.customSimulationSpace = child.transform;
            ParticleSystem.SubEmittersModule emitters = parent.subEmitters;
            emitters.enabled = true;
            emitters.AddSubEmitter(subSystem, ParticleSystemSubEmitterType.Birth, ParticleSystemSubEmitterProperties.InheritNothing);
            PrefabUtility.SaveAsPrefabAsset(contents, path);
            PrefabUtility.UnloadPrefabContents(contents);
            _preview = new VfxPreviewSession(_prefab);
            ParticleSystem copiedParent = PreviewRoot().transform.GetChild(0).GetComponent<ParticleSystem>();
            ParticleSystem copiedSub = copiedParent.transform.GetChild(0).GetComponent<ParticleSystem>();
            Assert.That(copiedParent.subEmitters.GetSubEmitterSystem(0), Is.SameAs(copiedSub));
            Assert.That(copiedParent.main.customSimulationSpace, Is.SameAs(copiedSub.transform));
            Assert.That(_prefab.GetComponent<ParticleSystem>().subEmitters.GetSubEmitterSystem(0), Is.Not.SameAs(copiedSub));
        }

        [Test]
        public void ClosingBrowserReleasesPreviewAndFavoritesPersist()
        {
            string key = "UnityTools.Vfx.Favorites." + Hash128.Compute(Application.dataPath);
            bool hadPreferences = EditorPrefs.HasKey(key);
            string previous = EditorPrefs.GetString(key, string.Empty);
            VfxBrowserWindow window = ScriptableObject.CreateInstance<VfxBrowserWindow>();
            window.Show();
            VfxBrowserWindow reopened = null;
            try
            {
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_prefab));
                HashSet<string> favorites = (HashSet<string>)typeof(VfxBrowserWindow).GetField("_favoriteGuids", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window);
                favorites.Add(guid);
                typeof(VfxBrowserWindow).GetMethod("SaveFavorites", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(window, null);
                VfxPrefabInfo item = new VfxPrefabInfo(guid, AssetDatabase.GetAssetPath(_prefab), _prefab.name, 1, true);
                typeof(VfxBrowserWindow).GetMethod("SelectItem", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(window, new object[] { item });
                VfxPreviewSession session = (VfxPreviewSession)typeof(VfxBrowserWindow).GetField("_preview", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window);
                GameObject root = (GameObject)typeof(VfxPreviewSession).GetField("_goRoot", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
                window.Close();
                Assert.That(root == null, Is.True);
                reopened = ScriptableObject.CreateInstance<VfxBrowserWindow>();
                reopened.Show();
                HashSet<string> restored = (HashSet<string>)typeof(VfxBrowserWindow).GetField("_favoriteGuids", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(reopened);
                Assert.That(restored.Contains(guid), Is.True);
            }
            finally
            {
                if (window != null)
                    window.Close();

                if (reopened != null)
                    reopened.Close();

                if (hadPreferences)
                {
                    EditorPrefs.SetString(key, previous);
                }
                else
                {
                    EditorPrefs.DeleteKey(key);
                }
            }
        }

        [Test]
        public void RenderProducesVisibleParticles()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                Assert.Ignore("그래픽 장치가 없는 실행에서는 실제 렌더링을 검증할 수 없습니다.");

            _preview = new VfxPreviewSession(_prefab);
            Assert.That(_preview.TrySeek(1.0f), Is.True);
            _preview.Fit();
            Texture2D texture = _preview.Capture(256, 256);
            try
            {
                Color[] pixels = texture.GetPixels();
                int brightCnt = 0;
                for (int idx = 0; idx < pixels.Length; idx++)
                {
                    if (pixels[idx].r > 0.3f)
                        brightCnt++;
                }

                Assert.That(brightCnt, Is.GreaterThan(15));
                string outputPath = Path.Combine(Path.GetTempPath(), "UnityTools-Vfx-Preview-" + Hash128.Compute(Application.dataPath) + ".png");
                File.WriteAllBytes(outputPath, texture.EncodeToPNG());
                TestContext.WriteLine("Preview image: " + outputPath);
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private GameObject PreviewRoot()
        {
            return (GameObject)typeof(VfxPreviewSession).GetField("_goRoot", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_preview);
        }
    }
}
