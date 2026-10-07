using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace UnityTools.Ui.Editor
{
    public class UiImageUsage
    {
        //============================================================
        // Properties
        //============================================================
        public string PrefabPath { get; }
        public string ImagePath { get; }
        public Sprite Sprite { get; }
        public Texture Texture => Sprite == null ? null : Sprite.texture;
        public Material Material { get; }
        public IReadOnlyList<SpriteAtlas> Atlases { get; }
        public bool HasSprite => Sprite != null;
        public bool HasAtlas => Atlases.Count > 0;

        //============================================================
        // Constructors
        //============================================================
        public UiImageUsage(string prefabPath, string imagePath, Sprite sprite, Material material, IReadOnlyList<SpriteAtlas> atlases)
        {
            PrefabPath = prefabPath;
            ImagePath = imagePath;
            Sprite = sprite;
            Material = material;
            Atlases = atlases;
        }
    }
}
