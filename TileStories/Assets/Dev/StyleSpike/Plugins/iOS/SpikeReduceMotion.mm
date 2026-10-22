// DEV-ONLY (_3.3 P0 S8): the iOS side of SpikeReduceMotion.Read(). The importer of this file is switched OFF for every platform,
// so no build includes it; to try it on an iPhone: enable it for iOS in its Inspector AND add TILESTORIES_SPIKE_IOS_REDUCE_MOTION to
// the iOS Scripting Define Symbols (both, or the C# side calls a symbol that is not linked).
#import <UIKit/UIKit.h>

extern "C" bool _TileStoriesSpikeIsReduceMotionEnabled()
{
    return UIAccessibilityIsReduceMotionEnabled();
}
