using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MK.Data;

namespace MK.Dropbox
{
    public interface IDropboxProvider
    {
        bool IsAuthorised { get; }

        string SingInUrl { get; }

        bool IsFinalUrl(Uri address);

        Task<bool> Init(string key, string secret, bool useSandbox);

        Task<bool> LoginCallback(Uri address);
       
        Task<Stream> GetFile(string path, CancellationToken? ct = null);

        Task<bool> Upload(string targetPath, string targetFileName, Stream stream, CancellationToken? ct = null);
    }
}
