namespace Voodoo.Sauce.Internal.Analytics
{
    public class MixpanelParameters
    {
        public readonly string MixpanelProdToken;
        public readonly bool UseVoodooTune;
        public readonly bool UseMixpanel;

        public MixpanelParameters(string mixpanelProdToken, bool useMixpanel, bool useVoodooTune)
        {
            MixpanelProdToken = mixpanelProdToken;
            UseVoodooTune = useVoodooTune;
            UseMixpanel = useMixpanel;
        }

        public bool IsMixpanelRemotelyActivated() => !UseVoodooTune && UseMixpanel && !string.IsNullOrEmpty(MixpanelProdToken);
    }
}