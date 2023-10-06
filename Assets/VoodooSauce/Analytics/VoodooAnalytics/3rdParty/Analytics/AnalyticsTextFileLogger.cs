using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using Voodoo.Sauce.Internal;

namespace Voodoo.Analytics
{
    internal static class AnalyticsTextFileLogger
    {
        private static string _apiRequestResponseLoggerPath = "";
        private static bool _isDebugMode = false;
        private const string TAG = "Analytics - TextLogger";
        private const string BEGIN_API = "-----BEGIN API LOG-----";
        private const string END_API = "-----END API LOG------";
        private const string BEGIN_REQUEST_HEADER = "-----BEGIN REQUEST HEADER-----";
        private const string END_REQUEST_HEADER = "-----END REQUEST HEADER-----";
        private const string BEGIN_REQUEST_BODY = "-----BEGIN REQUEST BODY-----";
        private const string END_REQUEST_BODY = "-----END REQUEST BODY-----";
        private const string BEGIN_RESPONSE_BODY = "-----BEGIN RESPONSE BODY-----";
        private const string END_RESPONSE_BODY = "-----END RESPONSE BODY-----";
        private const string BEGIN_RESPONSE_STATUS = "-----BEGIN RESPONSE STATUS-----";
        private const string END_RESPONSE_STATUS = "-----END RESPONSE STATUS-----";
        private const string BEGIN_EXCEPTION = "-----BEGIN API EXCEPTION-----";
        private const string END_EXCEPTION = "-----END API EXCEPTION-----";
        private const string FILE_NAME = "analytics_export_logs.txt";

        internal static void ConfigureAutomatedTextLogger(string path, bool isDebugMode)
        {
            _apiRequestResponseLoggerPath = path;
            _isDebugMode = isDebugMode;
        }
        
        private static void WriteRequestHeader(List<string> stringToWrite, string requestHeaderString)
        {
            stringToWrite.Add(BEGIN_REQUEST_HEADER);
            stringToWrite.Add(requestHeaderString);
            stringToWrite.Add(END_REQUEST_HEADER);
        }

        private static void WriteRequestBody(List<string> stringToWrite, string requestBodyString)
        {
            stringToWrite.Add(BEGIN_REQUEST_BODY);
            stringToWrite.Add(requestBodyString);
            stringToWrite.Add(END_REQUEST_BODY);
        }

        private static void WriteResponseStatusCode(List<string> stringToWrite, HttpStatusCode statusCode)
        {
            stringToWrite.Add(BEGIN_RESPONSE_STATUS);
            stringToWrite.Add($"{(int)statusCode} - {statusCode.ToString()}");
            stringToWrite.Add(END_RESPONSE_STATUS);
        }

        private static void WriteResponseBody(List<string> stringToWrite, string responseBody)
        {
            stringToWrite.Add(BEGIN_RESPONSE_BODY);
            stringToWrite.Add(responseBody);
            stringToWrite.Add(END_RESPONSE_BODY);
        }

        public static async void LogToText(HttpResponseMessage responseMessage)
        {
            if(_isDebugMode) 
                VoodooLog.LogWarning(Module.ANALYTICS, TAG,"LogToText called with response "+responseMessage);
            var stringsToWrite = new List<string>();
            
            try {
                stringsToWrite.Add(BEGIN_API);
                WriteRequestHeader(stringsToWrite, responseMessage.RequestMessage.Content.Headers.ToString());
                WriteRequestBody(stringsToWrite, await responseMessage.RequestMessage.Content.ReadAsStringAsync());
                WriteResponseStatusCode(stringsToWrite, responseMessage.StatusCode);
                WriteResponseBody(stringsToWrite, await responseMessage.Content.ReadAsStringAsync());
                stringsToWrite.Add(END_API);
                WriteAnalyticsLogsToFile(stringsToWrite);
            } catch (Exception exception) {
                VoodooSauce.LogException(exception);
            }
        }

        public static void LogExceptionToText(Exception exception)
        {
            if (_isDebugMode)
                VoodooLog.LogWarning(Module.ANALYTICS, TAG, "LogExceptionToText called with exception: " + exception);

            var stringsToWrite = new List<string> {
                BEGIN_EXCEPTION,
                exception.ToString(),
                END_EXCEPTION
            };
            WriteAnalyticsLogsToFile(stringsToWrite);
        }

        private static void WriteAnalyticsLogsToFile(List<string> stringsToWrite)
        {
            string filePath = _apiRequestResponseLoggerPath + "/" + FILE_NAME;
            try {
                File.AppendAllLines(filePath, stringsToWrite);
            } catch (Exception localException) {
                if (localException is UnauthorizedAccessException || localException is DirectoryNotFoundException
                    || localException is IOException) {
                    AnalyticsLog.CustomLoggerLogE(TAG, localException);
                } else {
                    VoodooSauce.LogException(localException);
                }
            }
        }
    }
}