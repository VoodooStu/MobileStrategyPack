using System.Collections.Generic;
using System.Linq;
using Voodoo.Sauce.Common.Utils;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal.Analytics;
using Voodoo.Sauce.Internal.IntegrationCheck;

namespace Voodoo.Sauce.Internal.Editor
{
    public class ConversionEventsIntegrationCheck : IIntegrationCheck
    {
        private const string CONVERSION_EVENT_ERROR = "\nInvalid conversion events detected in voodooSettings\n\n";
        private const string CONVERSION_EXPIRATION_DATE_ERROR =
            "\nThe number of days until the expiration after the first app launch must be (int) > 0\n\n";
        private const string CONVERSION_EVENT_FORMAT_ERROR = "Conversion Event format must be EventName(fs,rv,...)/EventTriggerNumber/EventToken.\n\n"
            + "The following Conversion Events are not valid {0}\n\n\n";

        private const string CONVERSION_DUPLICATED_EVENT_WARNING = "\nDuplicated conversion events detected in voodooSettings\n\n"
            + "The following Conversion Events are duplicated {0}\n\n";

        public List<IntegrationCheckMessage> IntegrationCheck(VoodooSettings settings)
        {
            var errors = new List<IntegrationCheckMessage>();

            ConversionEventsSettings eventsSettings = null;
            if (PlatformUtils.UNITY_IOS) {
                eventsSettings = settings.ConversionIosEvents;
            } else if (PlatformUtils.UNITY_ANDROID) {
                eventsSettings = settings.ConversionAndroidEvents;
            }

            //Feature not used or no events so no error checks
            if (eventsSettings == null || !eventsSettings.IsEnabled()) return errors;

            var errorDescription = "";
            //Check events expiration date
            if (!eventsSettings.HasValidExpirationDate()) {
                errorDescription = CONVERSION_EVENT_ERROR + CONVERSION_EXPIRATION_DATE_ERROR;
            }

            //Check for invalid events
            List<string> invalidEvents = eventsSettings.GetInvalidEventsData();
            if (invalidEvents.Any()) {
                if (errorDescription.Length == 0) errorDescription = CONVERSION_EVENT_ERROR;
                errorDescription += string.Format(CONVERSION_EVENT_FORMAT_ERROR, string.Join(", ", invalidEvents));
            }

            if (errorDescription.Length > 0) {
                errors.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.ERROR, errorDescription));
            }

            //Check for duplicated events
            List<string> duplicatedEvents = eventsSettings.GetDuplicatedEventsData();
            if (duplicatedEvents.Any()) {
                errors.Add(new IntegrationCheckMessage(IntegrationCheckMessage.Type.WARNING,
                    string.Format(CONVERSION_DUPLICATED_EVENT_WARNING, string.Join(", ", duplicatedEvents))));
            }

            return errors;
        }
    }
}