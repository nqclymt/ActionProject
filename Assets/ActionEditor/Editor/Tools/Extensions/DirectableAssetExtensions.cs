using UnityEngine;

namespace PKC.ActionEditor
{
    public static class DirectableAssetExtensions
    {
        public static float SnapTime(this IDirector asset, float time)
        {
            if (asset is Asset actionAsset)
            {
                return SkillFrameUtility.QuantizeTime(time, actionAsset.EvaluationFrameRate,
                    actionAsset.Length, SkillFrameRounding.Nearest);
            }

            return Mathf.Max(0f, time);
        }

        public static float TimeToPos(this IDirector asset, float time, float width)
        {
            return (time - asset.ViewTimeMin) / asset.ViewTime * width;
        }

        public static float PosToTime(this IDirector asset, float pos, float width)
        {
            return pos / width * asset.ViewTime + asset.ViewTimeMin;
        }
        
        public static float WidthToTime(this IDirector asset, float pos, float width)
        {
            return pos / width * asset.ViewTime;
        }
    }
}
