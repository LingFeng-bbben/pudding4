using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pudding4
{
    internal class Rbdx
    {
        static Random random = new Random(new Guid().GetHashCode() + (int)DateTime.Now.Ticks);
        public static async Task<string> GetRbdxSongs(string search = "")
        {
            var list = (await DownloadObject<RbdxSongResponse>("http://45.32.255.62:8080/api/bot/songs")).Data;

            if (list == null) throw new FileNotFoundException("rbdx.json err");
            if (search != "")
            {
                list = list.FindAll(list => list.Title.ToLower().Contains(search.ToLower()) ||
                                            list.Artist.ToLower().Contains(search.ToLower()) ||
                                            list.ChartAuthor.ToLower().Contains(search.ToLower()) ||
                                            list.DiffB.Contains(search) || list.DiffM.Contains(search) ||
                                            list.DiffH.Contains(search) || list.DiffSp.Contains(search));
                if (list.Count == 0)
                {
                    return "则不能neutral热爆挖鼻";
                }
            }
            int songid = random.Next(0, list.Count);
            var song = list[songid];
            var reply = String.Format("{0}\n{1}\n" +
                                    "🟢[{2}]🟡[{3}]🔴[{4}]🔵[{5}]\n" +
                                    "{6}",
                                       song.Title, song.Artist,
                                       song.DiffB, song.DiffM, song.DiffH, song.DiffSp,
                                       song.ChartAuthor);
            var imgpath = "http://45.32.255.62/data/rbdx/image/song/" + song.Id.ToString() + ".png";
            reply = String.Format("[CQ:image,file={0},subType=1]{1}", imgpath, reply);
            //insert image
            return reply;
        }

        public static async Task<string> ReadImage(string id)
        {
            //id = id.Replace("500", "").Replace("600", "");
            string targetpath = "rbdximages/";
            try
            {
                Console.Write("Searching existing image:" + id + "...");
                var dir = new DirectoryInfo(targetpath);
                dir.Create();
                var files = dir.GetFiles();
                if (files.Length > 0)
                {
                    if (files.Any(x => x.Name.Contains(id)))
                    {
                        var pic = files.First(x => x.Name.Contains(id));
                        if (pic != null)
                        {
                            Console.WriteLine("Already downloaded.");
                            return pic.FullName;
                        }
                    }
                }
                Console.WriteLine("nope.");
                Console.WriteLine("nope\nDownloading image:\n" + id);
                HttpClient Client = new HttpClient();
                var resp = await Client.GetByteArrayAsync("http://45.32.255.62/data/rbdx/image/song/" + id + ".png");
                targetpath = targetpath + id + ".png";
                File.WriteAllBytes(targetpath, resp);
                return targetpath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to download image:\n" + ex.InnerException);
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public static async Task<T> DownloadObject<T>(string url)
        {
            HttpClient Client = new HttpClient();
            var resp = await Client.GetAsync(url);
            var text = await resp.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<T>(text);
            return obj;
        }

    }

    public partial class RbdxSongResponse
    {
        [JsonProperty("data")]
        public List<RbdxSong> Data { get; set; }
    }

    public partial class RbdxSong
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("chartAuthor")]
        public string ChartAuthor { get; set; }

        [JsonProperty("diffB")]
        public string DiffB { get; set; }

        [JsonProperty("diffM")]
        public string DiffM { get; set; }

        [JsonProperty("diffH")]
        public string DiffH { get; set; }

        [JsonProperty("diffSP")]
        public string DiffSp { get; set; }
    }
}
