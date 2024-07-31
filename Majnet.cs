using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pudding4
{
    public class SongDetail
    {
        public string Id { get; set; }
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public string? Designer { get; set; }
        public string? Description { get; set; }
        public string[]? Levels { get; set; } = new string[6];
        public string? Uploader { get; set; }
        public long Timestamp { get; set; }
    }

    public class MajNet
    {
        static async Task<T> DownloadObject<T>(string url)
        {
            HttpClient Client = new HttpClient();
            var resp = await Client.GetAsync(url);
            var text = await resp.Content.ReadAsStringAsync();
            var obj = JsonConvert.DeserializeObject<T>(text);
            return obj;
        }

        public static async Task<List<SongDetail>> GetSongs(string api)
        {
            var obj = await DownloadObject<List<SongDetail>>(api);
            return obj;
        }
        static Random random = new Random(new Guid().GetHashCode() + (int)DateTime.Now.Ticks);
        public static async Task<string> GetRandomSong(bool isMmfc=false)
        {
            var songs = isMmfc?
                await MajNet.GetSongs("https://majdata.net/api1/api/SongList") : 
                await MajNet.GetSongs("https://majdata.net/api3/api/SongList");
            var i = random.Next(0, songs.Count);
            var song = songs[i];
            var levels = song.Levels[0] == null ? "" : "🟦" + song.Levels[0];
            levels += song.Levels[1] == null ? "" : "🟩" + song.Levels[1];
            levels += song.Levels[2] == null ? "" : "🟨" + song.Levels[2];
            levels += song.Levels[3] == null ? "" : "🟥" + song.Levels[3];
            levels += song.Levels[4] == null ? "" : "🟪" + song.Levels[4];
            levels += song.Levels[5] == null ? "" : "🟧" + song.Levels[5];
            var message = isMmfc? 
                String.Format("[CQ:image,file=https://majdata.net/api1/api/Image/{0},subType=1]" +
                "{1}\n{2}\n{0}号选手:{3}\n{4}",
                song.Id, song.Title, song.Artist, song.Designer, levels) : 
                String.Format("[CQ:image,file=https://majdata.net/api3/api/Image/{0},subType=1]" +
                "{1}\n{2}\n{3}\n{4}\nhttps://majdata.net/song?id={0}",
                song.Id, song.Title, song.Artist, song.Uploader, levels)
                ;
            return message;
        }

        public static async Task<string> GetRandomComment()
        {
            var comms = await DownloadObject<Dictionary<string, List<string>>>("https://www.maimaimfc.ink/_functions/comments");
            var commids = comms.Keys.ToList();
            var comminner = comms.Values.ToList();
            var rand  = random.Next(0, commids.Count);
            var randcoom = random.Next(0, comminner[rand].Count);
            var comment = comminner[rand][randcoom];
            var ret = String.Format("给{0}号的评论:\n{1}", commids[rand], comment);
            return ret;
        }
    }

}
