using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Photino.NET;

namespace DocxDiffTool;

static class Program
{
    private const int Port = 5000;

    [STAThread]
    static void Main()
    {
        var url = $"http://127.0.0.1:{Port}";

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Production,
        });
        builder.WebHost.UseUrls(url);

        var app = builder.Build();

        app.MapPost("/api/compare", async (HttpRequest request) =>
        {
            var form = await request.ReadFormAsync();
            var f1 = form.Files.GetFile("file1");
            var f2 = form.Files.GetFile("file2");
            if (f1 is null || f2 is null)
                return Results.BadRequest(new { error = "请上传两个文件（支持 .docx / .doc / .pdf / .txt / .md）" });

            List<string> paras1, paras2;
            try
            {
                paras1 = DiffService.ExtractText(f1.OpenReadStream());
                paras2 = DiffService.ExtractText(f2.OpenReadStream());
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = $"文件解析失败: {ex.Message}" });
            }

            var diffs = DiffService.BuildDiffs(paras1, paras2);
            var result = new CompareResult
            {
                Name1 = f1.FileName,
                Name2 = f2.FileName,
                Text1 = string.Join("\n", paras1),
                Text2 = string.Join("\n", paras2),
                Diffs = diffs,
                Stats = new DiffStats
                {
                    Equal = diffs.Count(d => d.Type == "equal"),
                    Deleted = diffs.Count(d => d.Type == "delete"),
                    Inserted = diffs.Count(d => d.Type == "insert"),
                    Replaced = diffs.Count(d => d.Type == "replace"),
                },
            };

            return Results.Json(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            });
        });

        var embeddedProvider = new ManifestEmbeddedFileProvider(
            Assembly.GetExecutingAssembly(), "wwwroot");
        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = embeddedProvider });
        app.UseStaticFiles(new StaticFileOptions { FileProvider = embeddedProvider });

        app.MapGet("/api/open-external", (string url) =>
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch { /* ignore */ }
            return Results.Ok();
        });

        var serverReady = new ManualResetEventSlim();
        var serverThread = new Thread(() =>
        {
            serverReady.Set();
            app.Run();
        })
        { IsBackground = true };
        serverThread.Start();
        serverReady.Wait();

        var window = new PhotinoWindow()
            .SetTitle("DocxDiffTool")
            .SetSize(1400, 900)
            .Center()
            .Load(url);

        window.WaitForClose();
        Environment.Exit(0);
    }
}
