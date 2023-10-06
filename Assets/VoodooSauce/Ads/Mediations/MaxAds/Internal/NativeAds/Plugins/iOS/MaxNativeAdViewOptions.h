//
//  MaxNativeAdViewOptions.h
//  Unity-iPhone
//
//  Created by Cyril Gy on 16/05/2022.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface MaxNativeAdViewOptions : NSObject

@property (nonatomic, strong) NSNumber *x;
@property (nonatomic, strong) NSNumber *y;
@property (nonatomic, strong) NSNumber *width;
@property (nonatomic, strong) NSNumber *height;
@property (nonatomic, strong) NSString *backgroundColor;
@property (nonatomic, strong) NSString *titleColor;
@property (nonatomic, strong) NSString *bodyColor;
@property (nonatomic, strong) NSString *buttonBackgroundColor;
@property (nonatomic, strong) NSString *buttonTextColor;
@property (nonatomic, strong) NSString *advertiserColor;

- (id)initWithData:(NSData *)data;

@end

NS_ASSUME_NONNULL_END
