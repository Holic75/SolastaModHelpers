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
            try
            {
                Sprite sprite = Gui.LoadAssetSync<Sprite>(sprite_reference);

                var texture = textureFromSprite(sprite);
                saveTextureAsPNG(texture, path);
            }
            catch (Exception e)
            {
                Main.Logger.Log(e.ToString());
            }
        }

        //stacks horizontaly images from files from left to right, resizes resulting image to final scale and stores it to a file
        public static void combineImages(string[] files, (int, int) final_scale, string final_image_filename)
        {
            List<System.Drawing.Bitmap> images = new List<System.Drawing.Bitmap>();
            System.Drawing.Bitmap finalImage = null;

            try
            {
                int width = 0;
                int height = 0;

                foreach (string image in files)
                {
                    //create a Bitmap from the file and add it to the list
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(image);

                    //update the size of the final bitmap
                    width += bitmap.Width;
                    height = bitmap.Height > height ? bitmap.Height : height;

                    images.Add(bitmap);
                }

                //create a bitmap to hold the combined image
                finalImage = new System.Drawing.Bitmap(width, height);

                //get a graphics object from the image so we can draw on it
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(finalImage))
                {
                    //set background color
                    g.Clear(System.Drawing.Color.Black);

                    //go through each image and draw it on the final image
                    int offset = 0;
                    foreach (System.Drawing.Bitmap image in images)
                    {
                        g.DrawImage(image,
                          new System.Drawing.Rectangle(offset, 0, image.Width, image.Height));
                        offset += image.Width;
                    }
                }
                finalImage = new System.Drawing.Bitmap(finalImage, new System.Drawing.Size(final_scale.Item1, final_scale.Item2));
                finalImage.Save(final_image_filename);
            }
            catch (Exception)
            {
                if (finalImage != null)
                    finalImage.Dispose();
                //throw ex;
                throw;
            }
            finally
            {
                //clean up memory
                foreach (System.Drawing.Bitmap image in images)
                {
                    image.Dispose();
                }
            }
        }


        //puts scaled inner image into specified postion of base image and stores it to a file
        public static void merge2Images(string base_image_file, string inner_image_file, (int, int) inner_image_scale, (int, int) inner_image_position, 
                                          string final_image_filename)
        {
            System.Drawing.Bitmap base_image = null;
            System.Drawing.Bitmap inner_image = null;

            try
            {
                base_image = new System.Drawing.Bitmap(base_image_file);
                inner_image = new System.Drawing.Bitmap(new System.Drawing.Bitmap(inner_image_file), new System.Drawing.Size(inner_image_scale.Item1, inner_image_scale.Item2));

                var src_region = new System.Drawing.Rectangle(0, 0, inner_image_scale.Item1, inner_image_scale.Item2);
                var dst_region = new System.Drawing.Rectangle(inner_image_position.Item1, inner_image_position.Item2, inner_image_scale.Item1, inner_image_scale.Item2);
                using (System.Drawing.Graphics grD = System.Drawing.Graphics.FromImage(base_image))
                {
                    grD.DrawImage(inner_image, dst_region, src_region, System.Drawing.GraphicsUnit.Pixel);
                }

                base_image.Save(final_image_filename);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //clean up memory
                if (base_image != null)
                    base_image.Dispose();
                if (inner_image != null)
                    inner_image.Dispose();
            }
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


        static public bool isCustomIcon(UnityEngine.Sprite sprite)
        {
            if (sprite == null)
            {
                return false;
            }
            return loaded_icons.ContainsValue(sprite);
        }
    }
}
