namespace TusDotNetClientTests;

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using TusDotNetClient;
using Xunit;
using static TusDotNetClientTests.Utils;

public class TusClientTests : IClassFixture<Fixture>
{
    private const string TusEndpoint = @"http://localhost:1080/files/";
    private readonly string _dataDirectoryPath;

    public TusClientTests()
    {
        _dataDirectoryPath = Fixture.DataDirectory.FullName;
    }

    [Theory]
    [MemberData(nameof(Fixture.TestFiles), MemberType = typeof(Fixture))]
    public async Task AfterCallingCreate_DataShouldContainAFile(FileInfo file)
    {
        var sut = new TusClient();

        var url = await sut.CreateAsync(
            TusEndpoint,
            file.Length);

        var upload = new FileInfo(Path.Combine(_dataDirectoryPath, $"{url.Split('/').Last()}"));
        upload.Exists.Should().Be(true);
        upload.Length.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(Fixture.TestFiles), MemberType = typeof(Fixture))]
    public async Task AfterCallingCreate_WithFileInfo_DataShouldContainAFile(FileInfo file)
    {
        var sut = new TusClient();

        var url = await sut.CreateAsync(
            TusEndpoint,
            file);

        var upload = new FileInfo(Path.Combine(_dataDirectoryPath, $"{url.Split('/').Last()}"));
        upload.Exists.Should().Be(true);
        upload.Length.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(Fixture.TestFiles), MemberType = typeof(Fixture))]
    public async Task AfterCallingCreateAndUpload_UploadedFileShouldBeTheSameAsTheOriginalFile(FileInfo file)
    {
        var sut = new TusClient();

        var url = await sut.CreateAsync(TusEndpoint, file.Length);

        await sut.UploadAsync(url, file);

        var upload = new FileInfo(Path.Combine(_dataDirectoryPath, $"{url.Split('/').Last()}"));
        upload.Exists.Should().Be(true);
        upload.Length.Should().Be(file.Length);
        using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
        using (var uploadStream = new FileStream(upload.FullName, FileMode.Open, FileAccess.Read))
        {
            var fileBytes = new byte[fileStream.Length];
            fileStream.Read(fileBytes, 0, fileBytes.Length);
            var uploadBytes = new byte[uploadStream.Length];
            uploadStream.Read(uploadBytes, 0, uploadBytes.Length);
            SHA1(uploadBytes).Should().Be(SHA1(fileBytes));
        }
    }

    [Theory]
    [MemberData(nameof(Fixture.TestFiles), MemberType = typeof(Fixture))]
    public async Task AfterCallingCreateAndUpload_WithFileUpload_UploadedFileShouldBeTheSameAsTheOriginalFile(
        FileInfo file)
    {
        var sut = new TusClient();

        var url = await sut.CreateAsync(TusEndpoint, file);

        await sut.UploadAsync(url, file);

        var upload = new FileInfo(Path.Combine(_dataDirectoryPath, $"{url.Split('/').Last()}"));
        upload.Exists.Should().Be(true);
        upload.Length.Should().Be(file.Length);
        using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
        using (var uploadStream = new FileStream(upload.FullName, FileMode.Open, FileAccess.Read))
        {
            var fileBytes = new byte[fileStream.Length];
            fileStream.Read(fileBytes, 0, fileBytes.Length);
            var uploadBytes = new byte[uploadStream.Length];
            uploadStream.Read(uploadBytes, 0, uploadBytes.Length);
            SHA1(uploadBytes).Should().Be(SHA1(fileBytes));
        }
    }

    [Theory]
    [MemberData(nameof(Fixture.TestFiles), MemberType = typeof(Fixture))]
    public async Task AfterCallingDownload_DownloadedFileShouldBeTheSameAsTheOriginalFile(FileInfo file)
    {
        var sut = new TusClient();

        var url = await sut.CreateAsync(TusEndpoint, file.Length);
        await sut.UploadAsync(url, file);
        var response = await sut.DownloadAsync(url);

        using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
        {
            var fileBytes = new byte[fileStream.Length];
            fileStream.Read(fileBytes, 0, fileBytes.Length);
            SHA1(response.ResponseBytes).Should().Be(SHA1(fileBytes));
        }
    }

    [Theory]
    [MemberData(nameof(Fixture.TestFiles), MemberType = typeof(Fixture))]
    public async Task CallingDelete_ShouldRemoveUploadedFile(FileInfo file)
    {
        var sut = new TusClient();

        var url = await sut.CreateAsync(TusEndpoint, file.Length);
        await sut.UploadAsync(url, file);
        var uploadHeadResponse = await sut.HeadAsync(url);
        var deleteResult = await sut.Delete(url);

        deleteResult.Should().Be(true);
        uploadHeadResponse.Headers.Keys.Should().Contain("Upload-Offset");
        uploadHeadResponse.Headers["Upload-Offset"].Should().Be(file.Length.ToString());
        File.Exists(Path.Combine(_dataDirectoryPath, $"url.Split('/').Last()")).Should().Be(false);
    }

    [Fact]
    public async Task CallingGetServerInfo_ShouldReturnServerInfo()
    {
        var sut = new TusClient();

        var response = await sut.GetServerInfo(TusEndpoint);

        response.Version.Should().NotBeNullOrWhiteSpace();
        response.Extensions.Should().NotBeEmpty();
        response.SupportedVersions.Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(Fixture.TestFiles), MemberType = typeof(Fixture))]
    public async Task CallingHead_ShouldReturnProgressOfUploadedFile(FileInfo file)
    {
        var sut = new TusClient();

        var url = await sut.CreateAsync(TusEndpoint, file.Length);
        var headBeforeUpload = await sut.HeadAsync(url);
        await sut.UploadAsync(url, file);
        var headAfterUpload = await sut.HeadAsync(url);

        headBeforeUpload.Headers.Keys.Should().Contain("Upload-Offset");
        headBeforeUpload.Headers["Upload-Offset"].Should().Be("0");
        headAfterUpload.Headers.Keys.Should().Contain("Upload-Offset");
        headAfterUpload.Headers["Upload-Offset"].Should().Be(file.Length.ToString());
    }
}