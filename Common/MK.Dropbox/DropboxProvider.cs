using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using DropNet;
using DropNet.Models;

using MK.Data;
using MK.Utilities;

namespace MK.Dropbox
{
    public class DropboxProvider : IDropboxProvider
    {
        private DropNetClient _client;
        private UserLogin _finalLogin;
        private IPersister<DropboxToken> _persister;

        public bool IsAuthorised
        {
            get { return _finalLogin != null; }
        }

        public string SingInUrl { get; private set; }

        public bool IsFinalUrl(Uri address)
        {
            return address.AbsoluteUri.Contains("authorize_submit");
        }

        public DropboxProvider(IPersister<DropboxToken> persiter)
        {
            persiter.NotNull("persister");
            _persister = persiter;

            var token = _persister.Load();

            if (token != null)
                _finalLogin = token.Value;
        }

        public Task<bool> Init(string key, string secret, bool useSandbox)
        {
            if (_finalLogin == null)
                _client = new DropNetClient(key, secret)
                    {
                        UseSandbox = useSandbox
                    };
            else
            {
                _client = new DropNetClient(key, secret, _finalLogin.Token, _finalLogin.Secret)
                    {
                        UseSandbox = useSandbox
                    };

                return Task.Factory.StartNew(() => true);
            }

            var tcs = new TaskCompletionSource<bool>();

            _client.GetTokenAsync(login =>
                {
                    SingInUrl = _client.BuildAuthorizeUrl();
                    tcs.SetResult(true);
                }, tcs.SetException);

            return tcs.Task;
        }

        public Task<bool> LoginCallback(Uri url)
        {
            var tcs = new TaskCompletionSource<bool>();

            _client.GetAccessTokenAsync(login =>
                {
                    _finalLogin = login;
                    _persister.Save(new DropboxToken { Value = _finalLogin });
                    tcs.SetResult(true);
                }, tcs.SetException);

            return tcs.Task;
        }

        public Task<Stream> GetFile(string path, CancellationToken? ct = null)
        {
            var tcs = new TaskCompletionSource<Stream>();

            if (ct != null)
                ct.Value.Register(tcs.SetCanceled);

            _client.GetFileAsync(path, response => tcs.TrySetResult(new MemoryStream(response.RawBytes)), ex => tcs.TrySetException(ex));

            return tcs.Task;
        }

        public Task<bool> Upload(string targetPath, string targetFileName, Stream stream, CancellationToken? ct = null)
        {
            if (ct != null)
                return Task.Factory.StartNew(() =>
                    {
                        ct.Value.ThrowIfCancellationRequested();

                        var m = _client.UploadFile(targetPath, targetFileName, stream);

                        return true;
                    }, ct.Value);

            return Task.Factory.StartNew(() =>
                {
                    var m = _client.UploadFile(targetPath, targetFileName, stream);
                    return true;
                });

            //MK 2015-07-05: Nie wiem dlaczego, ale UploadFileAsync rzuca wyjątkiem 
            //var tcs = new TaskCompletionSource<bool>();

            //if (ct != null)
            //    ct.Value.Register(tcs.SetCanceled);

            //_client.UploadFileAsync(targetPath, targetFileName, stream, data => tcs.TrySetResult(true), ex => tcs.TrySetException(ex));

            //return tcs.Task;
        }
    }

    public class DropboxToken
    {
        public UserLogin Value { get; set; }
    }
}
