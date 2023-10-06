namespace Voodoo.Sauce.Privacy
{
    /*
     * This interface defines the SDK properties requiring declaring a GDPR link.
     */
    public interface IPrivacyLink
    {
        /*
         * Returns the name of the SDK.
         */
        string SDKName { get; }

        /*
         * Returns the privacy policy url concerning the SDK.
         * This is needed for the GDPR pop-up for example.
         */
        string PrivacyPolicyUrl { get; }

        /*
         * Returns the kind of SDK.
         */
        PrivacySDKType SDKType { get; }
    }

    /*
     * The different kinds of SDK requiring displaying a GDPR link to the users. 
     */
    public enum PrivacySDKType
    {
     AdNetworkMaxAds, 
     Analytics,
     AdNetworkIronSource,
     AdNetwork
    }
}