using System;
using System.Threading.Tasks;
using UnityEngine;
using Voodoo.Sauce.Core;
using Voodoo.Sauce.Internal;

namespace Voodoo.Sauce.Privacy
{
    public class PrivacyAPI : IPrivacyAPI
    {
        private static readonly string BaseUrl = "https://api-gdpr.voodoo-tech.io/";
        private const string NEED_CONSENT = "need_consent";
        private const string CONSENT_INSIGHTS = "consent_insights";
        private const string PRIVACY_REQUEST = "privacy_request";

        private IRequest _request;

        public void Initialize(IRequest request)
        {
            _request = request;
        }

        public Task<NeedConsent> NeedConsent(NeedConsentParams data)
        {
            var tcs = new TaskCompletionSource<NeedConsent>();
            var url = BaseUrl + NEED_CONSENT + data;
           _request.Get(url, null,
                request => {
                    NeedConsent needConsent = null;
                    try {
                        needConsent = JsonUtility.FromJson<NeedConsent>(request.downloadHandler.text);
                    } catch {
                        VoodooLog.LogError(Module.PRIVACY,"PrivacyAPI", "can't deserialize JSON");
                    }
                    tcs.TrySetResult(needConsent);
                },
                request => { tcs.TrySetResult(null); });
            return tcs.Task;
        }

        public Task<bool> ConsentInsights(ConsentInsightsParams data)
        {
            var tcs = new TaskCompletionSource<bool>();
            var url = BaseUrl + CONSENT_INSIGHTS + data;
            _request.Get(url, null,
                request => { tcs.TrySetResult(true); },
                request => { tcs.TrySetResult(false); });
            return tcs.Task;
        }

        public Task<PrivacyRequest> DeleteDataRequest(DeleteDataParameters p)
        {
            var tcs = new TaskCompletionSource<PrivacyRequest>();
            var url = BaseUrl + PRIVACY_REQUEST;
            string json = JsonUtility.ToJson(p);
            json = JsonUtils.AddToJson(json, "action", "delete");
            _request.PostJson(url,json, null,
                request => {
                    PrivacyRequest privacyRequest = JsonUtility.FromJson<PrivacyRequest>(request.downloadHandler.text);
                    tcs.TrySetResult(privacyRequest);
                },
                request => {
                    PrivacyRequest privacyRequest = JsonUtility.FromJson<PrivacyRequest>(request.downloadHandler.text);
                    tcs.TrySetResult(privacyRequest);
                });
            return tcs.Task;
        }
        
        public Task<PrivacyRequest> AccessDataRequest(AccessDataParameters p)
        {
            var tcs = new TaskCompletionSource<PrivacyRequest>();
            var url = BaseUrl + PRIVACY_REQUEST;
            string json = JsonUtility.ToJson(p);
            json = JsonUtils.AddToJson(json, "action", "access");
            _request.PostJson(url,json, null,
                request => {
                    PrivacyRequest privacyRequest = JsonUtility.FromJson<PrivacyRequest>(request.downloadHandler.text);
                    tcs.TrySetResult(privacyRequest);
                },
                request => {
                    PrivacyRequest privacyRequest = JsonUtility.FromJson<PrivacyRequest>(request.downloadHandler.text);
                    tcs.TrySetResult(privacyRequest);
                });
            return tcs.Task;
        }
    }
}