using System.Diagnostics;
using Newtonsoft.Json;

public class VideoProcessor
{
    public string ProcessVideo(string videoPath)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-i \"{videoPath}\" -vf showinfo -f null -",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using (var process = new Process { StartInfo = processStartInfo })
        {
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return JsonConvert.SerializeObject(new { VideoInfo = output });
        }
    }
}
