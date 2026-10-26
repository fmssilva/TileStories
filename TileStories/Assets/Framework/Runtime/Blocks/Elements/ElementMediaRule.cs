using UnityEngine;

namespace TileStories
{
    // The shape of a media element's frame (_3.3 R14.2, the prototype's magazine), pure: the frame follows its picture's own proportion
    // (width / height) so a wide engraving is not cropped to a square and a portrait print is not cut to a strip -- kept between TallestAspect
    // (a portrait no taller than 4:5, which would push the text off the phone) and WidestAspect (a panorama no flatter than 2:1). A picture
    // pair's two frames share PairAspect (4:3), so the two read as one comparison whatever their files. An arch frame rounds its top into a
    // half circle as wide as the frame.
    public static class ElementMediaRule
    {
        public const float TallestAspect = 0.8f;
        public const float WidestAspect = 2f;
        public const float PairAspect = 4f / 3f;
        // A carousel's items share one proportion too (the prototype's 280 x 190 strip, 3:2): a row of equal frames reads as one strip
        public const float CarouselAspect = 3f / 2f;
        // A picture that is not loaded (or has no size) is framed at the pair's proportion
        public const float FallbackAspect = PairAspect;

        // The frame's width / height for a picture whose own is `pictureAspect`
        public static float FrameAspect(float pictureAspect)
        {
            if (float.IsNaN(pictureAspect) || float.IsInfinity(pictureAspect) || pictureAspect <= 0f) return FallbackAspect;
            return Mathf.Clamp(pictureAspect, TallestAspect, WidestAspect);
        }

        // An arch's top corners' radius for a frame `width` x `height`: half the width (a half circle), never past the frame's height
        public static float ArchRadius(float width, float height)
        {
            if (float.IsNaN(width) || float.IsNaN(height) || width <= 0f || height <= 0f) return 0f;
            return Mathf.Min(width * 0.5f, height);
        }
    }
}
