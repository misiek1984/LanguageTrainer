using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MK.Live
{
    public interface ILiveProvider
    {
        bool IsAuthorised { get; }

        string ErrorCode { get; }

        string ErrorDescription { get; }

        string SingInUrl { get; }

        Task<bool> Init(string clientId);

        Task<bool> LoginCallback(Uri url);

        Task<Stream> GetFile(string id, CancellationToken? ct = null);

        Task<Stream> GetFileByName(string targetPath, string targetFileName, CancellationToken? ct = null);

        Task<string> Upload(string targetPath, string targetFileName, Stream stream, CancellationToken? ct = null);
    }
}
