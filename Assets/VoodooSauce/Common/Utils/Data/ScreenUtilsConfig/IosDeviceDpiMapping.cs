using UnityEngine;

namespace Voodoo.Sauce.Common.Utils.Data.ScreenUtilsConfig
{
    public static class IosDeviceDpiMapping
    {
        /*
         * Copied from https://github.com/Clafou/DevicePpi/blob/master/Sources/DevicePpi/Ppi.swift
         */
        public static float GetIosDpiFromIosMachineName(string iosMachineName)
        {
            switch (iosMachineName) {
                // iPhone 13 mini
                case "iPhone14,4": 
                // iPhone 12 mini
                case "iPhone13,1":
                    return 476;
                
                // iPhone 15
                case "iPhone15,4": 
                // iPhone 15 Plus
                case "iPhone15,5": 
                // iPhone 15 Pro
                case "iPhone16,1": 
                // iPhone 15 Pro Max
                case "iPhone16,2":
                // iPhone 14
                case "iPhone14,7":
                // iPhone 14 Pro
                case "iPhone15,2":
                // iPhone 14 Pro Max
                case "iPhone15,3":
                // iPhone 13
                case "iPhone14,5":
                // iPhone 13 Pro
                case "iPhone14,2":
                // iPhone 12
                case "iPhone13,2":
                // iPhone 12 Pro
                case "iPhone13,3":
                    return 460;
                
                // iPhone 14 Plus
                case "iPhone14,8":
                // iPhone 13 Pro Max
                case "iPhone14,3":
                // iPhone 12 Pro Max
                case "iPhone13,4":
                // iPhone 11 Pro
                case "iPhone12,3":
                // iPhone 11 Pro Max
                case "iPhone12,5":
                // iPhone XS
                case "iPhone11,2":
                // iPhone XS Max
                case "iPhone11,4": case "iPhone11,6":
                // iPhone X
                case "iPhone10,3": case "iPhone10,6":
                    return 458;
                
                // iPhone 8 Plus
                case "iPhone10,2": case "iPhone10,5":
                // iPhone 7 Plus
                case "iPhone9,2": case "iPhone9,4":
                // iPhone 6S Plus
                case "iPhone8,2":
                // iPhone 6 Plus
                case "iPhone7,1":
                    return 401;
                
                // iPhone 11
                case "iPhone12,1":
                // iPhone XR
                case "iPhone11,8":
                // iPhone SE (3rd generation)
                case "iPhone14,6":
                // iPhone SE (2nd generation)
                case "iPhone12,8":
                // iPhone 8
                case "iPhone10,1": case "iPhone10,4":
                // iPhone 7
                case "iPhone9,1": case "iPhone9,3":
                // iPhone 6S
                case "iPhone8,1":
                // iPhone 6
                case "iPhone7,2":
                // iPhone SE
                case "iPhone8,4":
                // iPhone 5S
                case "iPhone6,1": case "iPhone6,2":
                // iPhone 5C
                case "iPhone5,3": case "iPhone5,4":
                // iPhone 5
                case "iPhone5,1": case "iPhone5,2":
                // iPod touch (7th generation)
                case "iPod9,1":
                // iPod touch (6th generation)
                case "iPod7,1":
                // iPod touch (5th generation)
                case "iPod5,1":
                // iPhone 4S
                case "iPhone4,1":
                // iPad mini (6th generation)
                case "iPad14,1": case "iPad14,2":
                // iPad mini (5th generation)
                case "iPad11,1": case "iPad11,2":
                // iPad mini 4
                case "iPad5,1": case "iPad5,2":
                // iPad mini 3
                case "iPad4,7": case "iPad4,8": case "iPad4,9":
                // iPad mini 2
                case "iPad4,4": case "iPad4,5": case "iPad4,6":
                    return 326;
                
                // iPad (10th generation)
                case "iPad13,18": case "iPad13,19":
                // iPad Pro (11″, 4th generation)
                case "iPad14,3": case "iPad14,4":
                case "iPad14,3-A": case "iPad14,3-B": case "iPad14,4-A": case "iPad14,4-B":
                // iPad Pro (12.9″, 6th generation)
                case "iPad14,5": case "iPad14,6":
                case "iPad14,5-A": case "iPad14,5-B": case "iPad14,6-A": case "iPad14,6-B":
                // iPad Air (5th generation)
                case "iPad13,16": case "iPad13,17":
                // iPad (9th generation)
                case "iPad12,1": case "iPad12,2":
                // iPad Pro (12.9″, 5th generation)
                case "iPad13,8": case "iPad13,9": case "iPad13,10": case "iPad13,11":
                // iPad Pro (11″, 3rd generation)
                case "iPad13,4": case "iPad13,5": case "iPad13,6": case "iPad13,7":
                // iPad Air (4th generation)
                case "iPad13,1": case "iPad13,2":
                // iPad (8th generation)
                case "iPad11,6": case "iPad11,7":
                // iPad Pro (12.9″, 4th generation)
                case "iPad8,11": case "iPad8,12":
                // iPad Pro (11″, 2nd generation)
                case "iPad8,9": case "iPad8,10":
                // iPad (7th generation)
                case "iPad7,11": case "iPad7,12":
                // iPad Air (3rd generation)
                case "iPad11,3": case "iPad11,4":
                // iPad Pro (12.9″, 3rd generation)
                case "iPad8,5": case "iPad8,6": case "iPad8,7": case "iPad8,8":
                // iPad Pro (11″)
                case "iPad8,1": case "iPad8,2": case "iPad8,3": case "iPad8,4":
                // iPad (6th generation)
                case "iPad7,5": case "iPad7,6":
                // iPad Pro (10.5″)
                case "iPad7,3": case "iPad7,4":
                // iPad Pro (12.9″, 2nd generation)
                case "iPad7,1": case "iPad7,2":
                // iPad (5th generation)
                case "iPad6,11": case "iPad6,12":
                // iPad Pro (12.9″)
                case "iPad6,7": case "iPad6,8":
                // iPad Pro (9.7″)
                case "iPad6,3": case "iPad6,4":
                // iPad Air 2
                case "iPad5,3": case "iPad5,4":
                // iPad Air
                case "iPad4,1": case "iPad4,2": case "iPad4,3":
                // iPad (4th generation)
                case "iPad3,4": case "iPad3,5": case "iPad3,6":
                // iPad (3rd generation)
                case "iPad3,1": case "iPad3,2": case "iPad3,3":
                    return 264;
                // iPad mini
                case "iPad2,5": case "iPad2,6": case "iPad2,7":
                    return 163;
                // iPad 2
                case "iPad2,1": case "iPad2,2": case "iPad2,3": case "iPad2,4":
                    return 132;
            }
            
            //Use Screen.dpi as fallback
            return Screen.dpi;
        }
    }
}