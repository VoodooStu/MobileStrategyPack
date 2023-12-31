﻿#region copyright
// --------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [http://codestage.net]
// --------------------------------------------------------------
#endregion

namespace Voodoo.Sauce.Internal.DebugScreen.CodeStage.AdvancedFPSCounter.CountersData
{
	public abstract class StaticCounterData : BaseCounterData
	{
		internal override void Activate()
		{
			base.Activate();
			
			if (inited)
				UpdateValue();
		}
	}
}