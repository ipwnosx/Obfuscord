using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
namespace AutoObfDiscord
{
    class Program
    {
        #region Utils
        public static Random random = new Random();
            public static string GetCurrentTime()
            {
                return DateTime.Now.ToString("[hh:mm:ss]");
            }
            public static string GetRandomAlphaNumeric()
            {
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                return new string(chars.Select(c => chars[random.Next(chars.Length)]).Take(4).ToArray());
            }
        #endregion

        #region Zip
        public static void ZipFiles(string rarPackagePath, List<string> tozip, string single, string randomworkplace, string randomoutput)
            {
                try
                {
                    using (var archive = SharpCompress.Archives.Zip.ZipArchive.Create())
                    {
                        foreach (string lol in tozip)
                        {
                            FileInfo file = new FileInfo(Environment.CurrentDirectory + "\\" + randomworkplace + "\\" + lol);
                            archive.AddEntry(lol, file);
                        }
                        archive.AddAllFromDirectory(Environment.CurrentDirectory + "\\" + randomoutput + "\\");
                        archive.SaveTo(rarPackagePath, CompressionType.Deflate);
                    }
                }
                catch
                {
                }
            }
        #endregion

        #region RarFile
        public static string RarFiles(string rarPackagePath, Dictionary<int, string> accFiles)
            {
                string error = "";
                try
                {
                    string[] files = new string[accFiles.Count];
                    int i = 0;
                    foreach (var fList_item in accFiles)
                    {
                        files[i] = "\"" + fList_item.Value;
                        i++;
                    }
                    string fileList = string.Join("\" ", files);
                    fileList += "\"";
                    System.Diagnostics.ProcessStartInfo sdp = new System.Diagnostics.ProcessStartInfo();
                    string cmdArgs = string.Format("A {0} {1} -ep1 -r",
                    String.Format("\"{0}\"", rarPackagePath),
                    fileList);
                    sdp.ErrorDialog = false;
                    sdp.UseShellExecute = true;
                    sdp.Arguments = cmdArgs;
                    sdp.FileName = "C:\\Program Files\\WinRAR\\WinRAR.exe";//Winrar.exe path
                    sdp.CreateNoWindow = false;
                    sdp.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    System.Diagnostics.Process process = System.Diagnostics.Process.Start(sdp);
                    process.WaitForExit();
                    error = "OK";
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
                return error;
            }
        #endregion

        #region DeleteFile
        public void DeleteFile(string location)
            {
                try
                {
                    File.Delete(location);
                }
                catch
                {
                }
        }
        #endregion

        #region Unrar
        public Tuple<List<string>, string> Unrar(string location, string temp)
            {
                try
                {
                    using (RarArchive archive = SharpCompress.Archives.Rar.RarArchive.Open(location))
                    {
                        string specific = "";
                        List<string> filestodelete = new List<string>();
                        foreach (var entry in archive.Entries)
                        {
                            entry.WriteToFile(Path.Combine(temp + "\\", entry.Key), new ExtractionOptions()
                            {
                                Overwrite = true,
                                ExtractFullPath = true

                            });
                            if (entry.Key.ToUpper().Contains("EXE"))
                            {
                                if (Path.GetFileNameWithoutExtension(entry.Key) == Path.GetFileNameWithoutExtension(location))
                                {
                                    continue;
                                }
                                specific = entry.Key;
                            }
                            else
                                filestodelete.Add(entry.Key);
                        }
                        return new Tuple<List<string>, string>(filestodelete, specific);
                    }
                }
                catch
                {
                    return new Tuple<List<string>, string>(null, null);
                }

        }
        #endregion

        #region Unzip
        public Tuple<List<string>, string> Unzip(string location, string temp)
            {
                try
                {
                    using (SharpCompress.Archives.Zip.ZipArchive archive = SharpCompress.Archives.Zip.ZipArchive.Open(location))
                    {
                        string specific = "";
                        List<string> filestodelete = new List<string>();
                        foreach (var entry in archive.Entries)
                        {
                            entry.WriteToFile(Path.Combine(temp + "\\", entry.Key), new ExtractionOptions()
                            {
                                Overwrite = true,
                                ExtractFullPath = true

                            });
                            if (entry.Key.ToUpper().Contains("EXE"))
                            {
                                if (Path.GetFileNameWithoutExtension(entry.Key) == Path.GetFileNameWithoutExtension(location))
                                {
                                    continue;
                                }
                                specific = entry.Key;
                            }
                            else
                                filestodelete.Add(entry.Key);
                        }
                        return new Tuple<List<string>, string>(filestodelete, specific);
                    }
                }
                catch
                {
                    return new Tuple<List<string>, string>(null, null);
                }
            }
        #endregion

        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
            Console.ReadLine();
        }
        private readonly DiscordSocketClient _client;
        public Program()
        {
            _client = new DiscordSocketClient();
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
        }
        public async Task MainAsync()
        {
            //can be obtained from https://discordapp.com/developers/applications
            await _client.LoginAsync(Discord.TokenType.Bot, "<Discord Bot Token>");
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");
            return Task.CompletedTask;
        }
       
        public static bool RunObfuscate(string location)
        {
            try
            {
                Process P = Process.Start("KOIVM\\Confuser.CLI.exe", $"-n {location}");
                P.WaitForExit();
                int result = P.ExitCode;
                if (result == 0) {
                    Obfuscations++;
                    return true;
                }
                return false;
            }
            catch {
                return false;
            }
        }
        public static int Obfuscations = 0;
        private async void Obfuscate(SocketMessage message)
        {
            try
            {
                #region Files
                string file = message.Attachments.ElementAt(0).Filename;
                string url = message.Attachments.ElementAt(0).Url;
                string randomoutput = "Output" + GetRandomAlphaNumeric();
                string randomworkplace = "WorkPlace" + GetRandomAlphaNumeric();
                Directory.CreateDirectory(randomoutput);
                Directory.CreateDirectory(randomworkplace);
                #endregion

                Console.WriteLine(message.Author + $" is obfuscating {file}");
                if (file.ToUpper().Contains("RAR"))
                {
                    #region DownloadingFile
                    WebClient client = new WebClient();
                    byte[] buffer = client.DownloadData(url);
                    #endregion

                    #region UnRar
                    string location = Path.GetFileNameWithoutExtension(file) + GetRandomAlphaNumeric() + ".rar";
                    File.WriteAllBytes(Environment.CurrentDirectory + "\\" + randomworkplace + "\\" + location, buffer);
                    Tuple<List<string>, string> tuple = Unrar(Environment.CurrentDirectory + "\\" + randomworkplace + "\\" + location, Environment.CurrentDirectory + "\\" + randomworkplace);
                    #endregion

                    #region Project
                    string project = File.ReadAllText("Project.crproj");
                    string startingproject = "";
                    startingproject = project;
                    project = project.Replace("<NAME>", Environment.CurrentDirectory + "\\" + randomworkplace + "\\" + tuple.Item2);
                    project = project.Replace("<OUTPUT>", Environment.CurrentDirectory + "\\" + randomoutput);
                    project = project.Replace("<WORKPLACE>", Environment.CurrentDirectory + "\\" + randomworkplace);
                    project = project.Replace("<location>", Environment.CurrentDirectory);
                    File.WriteAllText(Environment.CurrentDirectory + "\\" + randomworkplace + "\\Project.crproj", project);
                    #endregion

                    #region Obfuscate
                    if (!(RunObfuscate(Environment.CurrentDirectory + "\\" + randomworkplace + "\\Project.crproj")))
                    {
                        await message.Channel.SendMessageAsync($"{message.Author.Mention} Failed to obfuscate :(");
                        foreach (string lol in tuple.Item1)
                        {
                            DeleteFile(Environment.CurrentDirectory + "\\" + randomworkplace + lol);
                        }
                        Directory.Delete(randomworkplace, true);
                        Directory.Delete(randomoutput, true);
                        return;
                    }
                    #endregion

                    #region RarFiles
                    string rarPackage = Environment.CurrentDirectory + "\\" + randomoutput + "\\" + "Obfuscated.rar";
                    Dictionary<int, string> accFiles = new Dictionary<int, string>();
                    int thing = 1;
                    foreach (string lol in tuple.Item1)
                    {
                        accFiles.Add(thing, Environment.CurrentDirectory + "\\" + randomworkplace + "\\" + lol);
                        thing++;
                    }
                    accFiles.Add(thing, Environment.CurrentDirectory + "\\" + randomoutput + "\\" + tuple.Item2);
                    RarFiles(rarPackage, accFiles);

                    #endregion

                    #region SendFile
                    await message.Author.SendFileAsync(rarPackage);
                    await message.Channel.SendMessageAsync($"{message.Author.Mention} Sucessfully Obfuscated, Please check ur pm's :) - Total Obfuscations : {Obfuscations}");
                    #endregion

                    #region DeleteFiles
                    foreach (string lol in tuple.Item1)
                    {
                        DeleteFile(Environment.CurrentDirectory + "\\" + randomworkplace + lol);

                    }
                    DeleteFile(rarPackage);
                    Directory.Delete(randomworkplace, true);
                    Directory.Delete(randomoutput, true);

                    #endregion
                }
                else if (file.ToUpper().Contains("ZIP"))
                {
                    #region DownloadingFile
                    WebClient client = new WebClient();
                    byte[] buffer = client.DownloadData(url);
                    #endregion

                    #region UnZip
                    string location = Path.GetFileNameWithoutExtension(file) + GetRandomAlphaNumeric() + ".zip";
                    File.WriteAllBytes(Environment.CurrentDirectory + "\\" + randomworkplace + "\\" + location, buffer);
                    Tuple<List<string>, string> tuple = Unzip(Environment.CurrentDirectory + "\\" + randomworkplace + "\\" + location, Environment.CurrentDirectory + "\\" + randomworkplace);
                    #endregion

                    #region Project
                    string project = File.ReadAllText("Project.crproj");
                    string startingproject = "";
                    startingproject = project;
                    project = project.Replace("<NAME>", Environment.CurrentDirectory + "\\" + randomworkplace + "\\" + tuple.Item2);
                    project = project.Replace("<OUTPUT>", Environment.CurrentDirectory + "\\" + randomoutput);
                    project = project.Replace("<WORKPLACE>", Environment.CurrentDirectory + "\\" + randomworkplace);
                    project = project.Replace("<location>", Environment.CurrentDirectory);
                    File.WriteAllText(Environment.CurrentDirectory + "\\" + randomworkplace + "\\Project.crproj", project);
                    #endregion

                    #region Obfuscate
                    if (!(RunObfuscate(Environment.CurrentDirectory + "\\" + randomworkplace + "\\Project.crproj")))
                    {
                        await message.Channel.SendMessageAsync($"{message.Author.Mention} Failed to obfuscate :(");
                        foreach (string lol in tuple.Item1)
                        {
                            DeleteFile(Environment.CurrentDirectory + "\\" + randomworkplace + lol);
                        }
                        Directory.Delete(randomworkplace, true);
                        Directory.Delete(randomoutput, true);
                        return;
                    }
                    #endregion

                    #region ZipFiles
                    string zipPackage = Environment.CurrentDirectory + "\\" + randomoutput + "\\" + "Obfuscated.zip";
                    Dictionary<int, string> accFiles = new Dictionary<int, string>();
                    int thing = 1;
                    /*  foreach (string lol in tuple.Item1)
                      {
                          accFiles.Add(thing, Environment.CurrentDirectory + "\\" + randomworkplace + "\\" + lol);
                          thing++;
                      }
                      accFiles.Add(thing, Environment.CurrentDirectory + "\\" + randomoutput + "\\" + tuple.Item2);*/
                    ZipFiles(zipPackage, tuple.Item1, Environment.CurrentDirectory + "\\" + randomoutput + "\\" + tuple.Item2, randomworkplace, randomoutput);

                    #endregion

                    #region SendFile
                    await message.Author.SendFileAsync(zipPackage);
                    await message.Channel.SendMessageAsync($"{message.Author.Mention} Sucessfully Obfuscated, Please check ur pm's :) - Total Obfuscations : {Obfuscations}");
                    #endregion

                    #region DeleteFiles
                    foreach (string lol in tuple.Item1)
                    {
                        DeleteFile(Environment.CurrentDirectory + "\\" + randomworkplace + lol);

                    }
                    DeleteFile(zipPackage);
                    Directory.Delete(randomworkplace, true);
                    Directory.Delete(randomoutput, true);
                    #endregion
                }
            }
            catch {
            }
        }
        private async Task MessageReceivedAsync(SocketMessage message)
        {
            #region Checks
            if (message.Author.Id == _client.CurrentUser.Id)
                return;
            if (message.Author.IsBot)
                return;
            if (message.Author.IsWebhook)
                return;
            #endregion

            try
            {
                if (message.Attachments.Count() != 0)
                {
                    if (message.Attachments.Where(q => q.Filename.ToString().ToUpper().Contains("RAR")).Count() > 0 || message.Attachments.Where(q => q.Filename.ToString().ToUpper().Contains("ZIP")).Count() > 0)
                    {
                        Obfuscate(message);
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("We only accept .RAR and .ZIP files (for now)");
                    }
                }
            }
            catch {
            }
        }
    }
}
