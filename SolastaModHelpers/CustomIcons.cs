using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SolastaModHelpers.CustomIcons
{
    static public class Tools
    {
        static Dictionary<string, Sprite> loaded_icons = new Dictionary<string, Sprite>();
        static string CUSTOM_ICON_PREFIX => "CUSTOM_ICON_PREFIX_";

        // Loosely based on https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/
        public static Sprite imageToSprite(string filePath, int size_x, int size_y)
        {
            var bytes = File.ReadAllBytes(filePath);
            var texture = new Texture2D(size_x, size_y, TextureFormat.DXT5, false);
            texture.LoadImage(bytes);
            return Sprite.Create(texture, new Rect(0, 0, size_x, size_y), new Vector2(0, 0));
        }


        public static Texture2D textureFromSprite(Sprite sprite)
        {
            if (sprite.rect.width != sprite.texture.width)
            {
                Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                             (int)sprite.textureRect.y,
                                                             (int)sprite.textureRect.width,
                                                             (int)sprite.textureRect.height);
                newText.SetPixels(newColors);
                newText.Apply();
                return newText;
            }
            else
                return sprite.texture;
        }


        static Texture2D duplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }


        public static void saveTextureAsPNG(Texture2D texture, string path)
        {
            byte[] _bytes = duplicateTexture(texture).EncodeToPNG();
            Main.Logger.Log("Saving to " + path.ToString());
            System.IO.File.WriteAllBytes(path, _bytes);
        }


        public static void saveSpriteFromAssetReferenceAsPNG(AssetReferenceSprite sprite_reference, string path)
        {
            Sprite sprite = Gui.LoadAssetSync<Sprite>(sprite_reference);

            var texture = textureFromSprite(sprite);
            saveTextureAsPNG(texture, path);
        }


        static public AssetReferenceSprite storeCustomIcon(string name, string file_path, int size_x, int size_y)
        {
            var sprite = imageToSprite(file_path, size_x, size_y);
            loaded_icons.Add(CUSTOM_ICON_PREFIX + name, sprite);

            return new AssetReferenceSprite(CUSTOM_ICON_PREFIX + name);
        }


        static public UnityEngine.Sprite loadStoredCustomIcon(string guid)
        {
            if (!loaded_icons.ContainsKey(guid))
            {
                return null;
            }

            return loaded_icons[guid];
        }
    }
}
