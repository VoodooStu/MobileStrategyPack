namespace Voodoo.Sauce.Core
{
	public static class StatusCodeVerifier
	{
		public static ValidationStatus VerifyStatus(int statusCode)
		{
			switch (statusCode)
			{
				case 0: //Only success case both for Android & iOS
					return ValidationStatus.Accepted;
				case 1: //Android Deny answer
				case 400:
				case 21000:
				case 21001:
				case 21003:
				case 21004:
				case 21006:
				case 21007:
				case 21008:
				case 21010:
					return ValidationStatus.Denied;
				case 2: //Android Retry answer
				case 21002:
				case 21005:
				case 21009:
					return ValidationStatus.InProgress;
				default:
					return ValidationStatus.Denied;
			}
		}
	}
}