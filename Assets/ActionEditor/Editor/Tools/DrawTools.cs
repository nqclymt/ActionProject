using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PKC.ActionEditor
{
    public static class DrawTools
    {
        private sealed class AudioTextureCache
        {
            public int Width;
            public int Height;
            public Texture2D Texture;
        }

        static DrawTools()
        {
            AssemblyReloadEvents.beforeAssemblyReload += ClearAudioTextures;
            EditorApplication.quitting += ClearAudioTextures;
        }

        public static void DrawDashedLine(float x, float startY, float endY, Color color)
        {
            Handles.BeginGUI();
            Handles.color = color;

            var totalLength = Mathf.Abs(endY - startY);
            var dashes = Mathf.FloorToInt(totalLength / 10); // 每段长度为10

            for (var i = 0; i < dashes; i++)
            {
                var t1 = (float)i / dashes;
                var t2 = (i + 0.5f) / dashes;
                var point1Y = Mathf.Lerp(startY, endY, t1);
                var point2Y = Mathf.Lerp(startY, endY, t2);

                Handles.DrawLine(new Vector2(x, point1Y), new Vector2(x, point2Y));
            }

            Handles.EndGUI();
        }


        #region 绘制贴图

        private static readonly Dictionary<AudioClip, AudioTextureCache> AudioTextures =
            new Dictionary<AudioClip, AudioTextureCache>();

        public static Texture2D GetAudioClipTexture(AudioClip clip, int width, int height)
        {
            if (clip == null)
            {
                return null;
            }

            width = Mathf.Clamp(width, 1, 4096);
            height = Mathf.Clamp(height, 1, 256);

            if (AudioTextures.TryGetValue(clip, out var cached))
            {
                if (cached.Texture != null && cached.Width == width && cached.Height == height)
                {
                    return cached.Texture;
                }

                DestroyTexture(cached.Texture);
                AudioTextures.Remove(clip);
            }

            if (clip.loadType != AudioClipLoadType.DecompressOnLoad)
            {
                return null;
            }

            var sampleCount = (long)clip.samples * clip.channels;
            if (sampleCount <= 0 || sampleCount > int.MaxValue)
            {
                return null;
            }

            var samples = new float[(int)sampleCount];
            if (!clip.GetData(samples, 0))
            {
                return null;
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            var pixels = new Color[width * height];
            var center = height / 2;
            for (var x = 0; x < width; x++)
            {
                var startFrame = Mathf.FloorToInt((float)x * clip.samples / width);
                var endFrame = Mathf.Max(startFrame + 1,
                    Mathf.CeilToInt((float)(x + 1) * clip.samples / width));
                endFrame = Mathf.Min(endFrame, clip.samples);

                var peak = 0f;
                for (var frame = startFrame; frame < endFrame; frame++)
                {
                    var channelOffset = frame * clip.channels;
                    for (var channel = 0; channel < clip.channels; channel++)
                    {
                        peak = Mathf.Max(peak, Mathf.Abs(samples[channelOffset + channel]));
                    }
                }

                var halfHeight = Mathf.CeilToInt(peak * Mathf.Max(1, height - 1) * 0.5f);
                var minY = Mathf.Max(0, center - halfHeight);
                var maxY = Mathf.Min(height - 1, center + halfHeight);
                for (var y = minY; y <= maxY; y++)
                {
                    pixels[y * width + x] = Color.white;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            AudioTextures[clip] = new AudioTextureCache
            {
                Width = width,
                Height = height,
                Texture = texture
            };
            return texture;
        }

        private static void ClearAudioTextures()
        {
            foreach (var cached in AudioTextures.Values)
            {
                DestroyTexture(cached.Texture);
            }

            AudioTextures.Clear();
        }

        private static void DestroyTexture(Texture2D texture)
        {
            if (texture != null)
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        /// <summary>
        /// 绘制循环音频剪辑纹理
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="audioClip"></param>
        /// <param name="maxLength"></param>
        /// <param name="offset"></param>
        public static void DrawLoopedAudioTexture(Rect rect, AudioClip audioClip, float maxLength, float offset)
        {
            if (audioClip == null || audioClip.length <= Mathf.Epsilon || maxLength <= Mathf.Epsilon ||
                rect.width <= 0 || rect.height <= 0)
            {
                return;
            }

            var audioRect = rect;
            audioRect.width = (audioClip.length / maxLength) * rect.width;
            var t = GetAudioClipTexture(audioClip, (int)audioRect.width, (int)audioRect.height);
            if (t != null)
            {
                var oldHandlesColor = Handles.color;
                var oldGuiColor = GUI.color;
                Handles.color = new Color(0, 0, 0, 0.2f);
                GUI.color = new Color(0.4f, 0.435f, 0.576f);
                audioRect.yMin += 2;
                audioRect.yMax -= 2;
                for (var f = offset; f < maxLength; f += audioClip.length)
                {
                    audioRect.x = rect.x + (f / maxLength) * rect.width;
                    GUI.DrawTexture(audioRect, t);
                }

                Handles.color = oldHandlesColor;
                GUI.color = oldGuiColor;
            }
        }

        /// <summary>
        /// 在 Rect 内绘制环形垂直线，并提供最大长度（带可选偏移量）
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="length"></param>
        /// <param name="maxLength"></param>
        /// <param name="offset"></param>
        public static void DrawLoopedLines(Rect rect, float length, float maxLength, float offset)
        {
            if (Mathf.Abs(length) > Mathf.Epsilon && Mathf.Abs(maxLength) > Mathf.Epsilon)
            {
                length = Mathf.Abs(length);
                maxLength = Mathf.Abs(maxLength);
                var oldHandlesColor = Handles.color;
                Handles.color = new Color(0, 0, 0, 0.2f);
                for (var f = offset; f < maxLength; f += length)
                {
                    var posX = rect.x + (f / maxLength) * rect.width;
                    Handles.DrawLine(new Vector2(posX, rect.yMin), new Vector2(posX, rect.yMax));
                }

                Handles.color = oldHandlesColor;
            }
        }

        #endregion
    }
}
