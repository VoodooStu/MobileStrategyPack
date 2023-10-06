#import "NativeTime.h"

@implementation NativeTime

#pragma mark - Publicly available C methods

/*
 * This object is assigned to a global variable.
 * So it will be created as soon as this code will be loaded by the system,
 * at the application launch.
 *
 * For the details: https://stackoverflow.com/questions/889380/how-can-i-get-a-precise-time-for-example-in-milliseconds-in-objective-c/30363702
 */
    NSDate *startDate = [NSDate date];
    
extern "C"
{
    // This function returns the time in milliseconds elapsed since the launch of the application.
    double _getElapsedTime() {
        return [startDate timeIntervalSinceNow] * -1000.0;
    }
    
    // This function returns the timestamp in milliseconds since the launch of the application.
    long _getStartTimestamp() {
        return [startDate timeIntervalSince1970] * 1000;
    }
}

@end
