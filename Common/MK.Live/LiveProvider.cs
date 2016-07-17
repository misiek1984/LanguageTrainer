using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Live;

using MK.Data;
using MK.Utilities;

namespace MK.Live
{
    public class LiveProvider : ILiveProvider, IRefreshTokenHandler
    {
        #region Fields & Properties

        private readonly string[] _scopes = new[] { "wl.skydrive", "wl.skydrive_update", "wl.signin", "wl.offline_access" };

        private IPersister<LiveToken> _persister;

        public string SingInUrl { get; private set; }

        private LiveAuthClient _authClient;
        private LiveConnectClient _liveConnectClient;
        private RefreshTokenInfo _refreshTokenInfo;

        public bool IsAuthorised
        {
            get { return _liveConnectClient != null; }
        }

        public string ErrorCode { get; private set; }
        public string ErrorDescription { get; private set; }

        #endregion

        #region Constructor

        public LiveProvider(IPersister<LiveToken> persister)
        {
            persister.NotNull("persister");
            _persister = persister;

            var token = _persister.Load();
            if (token != null)
            {
                _refreshTokenInfo = new RefreshTokenInfo(token.Value);
            }
        }

        #endregion

        #region Public Methods

        public async Task<bool> Init(string clientId)
        {
            clientId.NotNull("clientId");

            _authClient = new LiveAuthClient(clientId, this);

            var loginResult = await _authClient.InitializeAsync(_scopes);
            if (loginResult.Session != null)
                _liveConnectClient = new LiveConnectClient(loginResult.Session);

            SingInUrl = _authClient.GetLoginUrl(_scopes);
            return true;
        }

        public async Task<bool> LoginCallback(Uri url)
        {
            if (!url.AbsoluteUri.StartsWith("https://login.live.com/oauth20_desktop.srf"))
                return false;

            var result = new AuthResult(url);

            if (!String.IsNullOrEmpty(result.ErrorCode))
            {
                _liveConnectClient = null;

                ErrorCode = result.ErrorCode;
                ErrorDescription = result.ErrorDescription;

                return false;
            }

            var session = await _authClient.ExchangeAuthCodeAsync(result.AuthorizeCode);
            _liveConnectClient = new LiveConnectClient(session);

            ErrorCode = null;
            ErrorDescription = null;

            return true;
        }

        public async Task<Stream> GetFile(string id, CancellationToken? ct = null)
        {
            LiveDownloadOperationResult res;
            if (ct != null)
                res = await _liveConnectClient.DownloadAsync(id + "/content", ct.Value, null);
            else
                res = await _liveConnectClient.DownloadAsync(id + "/content");

            return res.Stream;
        }

        public async Task<Stream> GetFileByName(string targetPath, string targetFileName, CancellationToken? ct = null)
        {
            LiveOperationResult folderDetails;
            if(ct != null)
                folderDetails  = await _liveConnectClient.GetAsync(targetPath, ct.Value);
            else
                folderDetails  = await _liveConnectClient.GetAsync(targetPath);

            var files = (List<object>)folderDetails.Result["data"];
            string fileId = null;

            foreach (IDictionary<string, object> dict in files)
            {
                var name = (string)dict["name"];
                var type = (string)dict["type"];
                if (name == targetFileName && type == "file")
                {
                    fileId = (string)dict["id"];
                    break;
                }
            }

            if(fileId != null)
            {
                var res = await _liveConnectClient.DownloadAsync(fileId + "/content");
                return res.Stream;
            }

            return null;
        }

        public async Task<string> Upload(string targetPath, string targetFileName, Stream stream, CancellationToken? ct = null)
        {
            LiveOperationResult res;
            if (ct != null)
                res = await _liveConnectClient.UploadAsync("me/skydrive/" + targetPath, targetFileName, stream, OverwriteOption.Overwrite, ct.Value, null);
            else
                res = await _liveConnectClient.UploadAsync("me/skydrive/" + targetPath, targetFileName, stream, OverwriteOption.Overwrite);

            return res.Result["id"].ToString();
        }

        #endregion

        #region IRefreshTokenHandler

        Task IRefreshTokenHandler.SaveRefreshTokenAsync(RefreshTokenInfo tokenInfo)
        {
            return Task.Factory.StartNew(() =>
            {
                _refreshTokenInfo = tokenInfo;
                _persister.Save(new LiveToken { Value = _refreshTokenInfo.RefreshToken });
            });
        }

        Task<RefreshTokenInfo> IRefreshTokenHandler.RetrieveRefreshTokenAsync()
        {
            return Task.Factory.StartNew(() => _refreshTokenInfo);
        }

        #endregion
    }

    public class AuthResult
    {
        public AuthResult(Uri resultUri)
        {
            string[] queryParams = resultUri.Query.TrimStart('?').Split('&');
            foreach (string param in queryParams)
            {
                string[] kvp = param.Split('=');
                switch (kvp[0])
                {
                    case "code":
                        this.AuthorizeCode = kvp[1];
                        break;
                    case "error":
                        this.ErrorCode = kvp[1];
                        break;
                    case "error_description":
                        this.ErrorDescription = Uri.UnescapeDataString(kvp[1]);
                        break;
                }
            }
        }

        public string AuthorizeCode { get; private set; }
        public string ErrorCode { get; private set; }
        public string ErrorDescription { get; private set; }
    }

    public class LiveToken
    {
        public string Value { get; set; }
    }
}
