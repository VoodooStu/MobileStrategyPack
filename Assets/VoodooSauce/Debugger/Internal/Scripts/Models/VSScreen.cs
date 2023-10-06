using UnityEngine;
using Voodoo.Sauce.Internal;

namespace Voodoo.Sauce.Debugger
{
    public class VSScreen : Screen, IConditionalScreen
    {
        public bool CanDisplay => Application.identifier == VoodooConstants.TEST_APP_BUNDLE;
    }
}
