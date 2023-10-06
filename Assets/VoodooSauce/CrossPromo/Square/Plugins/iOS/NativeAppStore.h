
#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>

extern UIViewController *UnityGetGLViewController(void);
extern void UnitySendMessage(const char *, const char *, const char *);


@interface NativeAppStore : NSObject <SKStoreProductViewControllerDelegate>

    @property(nonatomic, strong) SKStoreProductViewController *productViewController;
    @property(nonatomic) NSInteger productId;
    @property(nonatomic) BOOL error;

- (void)loadStoreProduct:(NSInteger)itemId;
- (void)openStoreProduct:(NSInteger)itemId;
@end
