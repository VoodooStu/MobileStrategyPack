#import "NativeAudio.h"
#import <AVFoundation/AVFoundation.h>

@implementation NativeAudio

#pragma mark - Publicly available C methods

extern "C"
{
    // This function returns the time in milliseconds elapsed since the launch of the application.
    int _getVolume() {
        [[AVAudioSession sharedInstance] setActive:YES error:nil];
        float volume = [[AVAudioSession sharedInstance] outputVolume];
        return (volume * 100.0)/1;
    }
}

@end
