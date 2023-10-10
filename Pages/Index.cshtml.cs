using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Video_Analyzer.Models;
using System.Diagnostics;



namespace Video_Analyzer.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public IFormFile VideoFile { get; set; } = null!;
        public VideoInfos VideoInfo { get; set; } = new VideoInfos();

        public void OnGet()
        {
        }

        public async Task OnPostAsync()
        {
            if (VideoFile != null && VideoFile.Length > 0)
            {
                using (var streamReader = new StreamReader(VideoFile.OpenReadStream()))
                {
                    // Processar o arquivo de vídeo e coletar informações
                    VideoInfo = await GetVideoInfoAsync(streamReader.BaseStream);
                }
            }
        }

        private async Task<VideoInfos> GetVideoInfoAsync(Stream videoStream)
        {
            var videoInfo = new VideoInfos();

            using (var memoryStream = new MemoryStream())
            {
                await videoStream.CopyToAsync(memoryStream);

                using (var process = new Process())
                {
                    var ffmpegPath = "ffmpeg"; 
                    var inputOptions = "-i -"; 
                    var outputOptions = "-f json -";

                    process.StartInfo.FileName = ffmpegPath;
                    process.StartInfo.Arguments = $"{inputOptions} {outputOptions}";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();

                    // Enviar o conteúdo do vídeo para o processo FFmpeg
                    await memoryStream.CopyToAsync(process.StandardInput.BaseStream);
                    process.StandardInput.Close();

                    // Ler a saída JSON do processo FFmpeg e converter para VideoInfos
                    var outputJson = await process.StandardOutput.ReadToEndAsync();
                    videoInfo = JsonConvert.DeserializeObject<VideoInfos>(outputJson);

                    process.WaitForExit();  
                }
            }

        return videoInfo;
        }
    }
}