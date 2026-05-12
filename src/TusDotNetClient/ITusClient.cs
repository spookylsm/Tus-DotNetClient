namespace TusDotNetClient;

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public interface ITusClient
{
    Dictionary<string, string> AdditionalHeaders { get; }
    
    IWebProxy Proxy { get; set; }

    Task<string> CreateAsync(string url, FileInfo fileInfo, params (string key, string value)[] metadata);
    
    Task<string> CreateAsync(string url, long uploadLength, params (string key, string value)[] metadata);

    Task<bool> Delete(string url);
    
    TusOperation<TusHttpResponse> DownloadAsync(string url, CancellationToken cancellationToken = default);
    
    Task<TusServerInfo> GetServerInfo(string url);
    
    Task<TusHttpResponse> HeadAsync(string url);

    TusOperation<List<TusHttpResponse>> UploadAsync(string url, FileInfo file, double chunkSize = 5.0, CancellationToken cancellationToken = default);
    
    TusOperation<List<TusHttpResponse>> UploadAsync(string url, Stream fileStream, double chunkSize = 5.0, CancellationToken cancellationToken = default);
}
