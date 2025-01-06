using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace pudding4
{
    public class BiliSearchSettings
    {
        public string KeyWord = "原神";
    }

    public partial class BiliSearch
    {
        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("message")]
        public long Message { get; set; }

        [JsonProperty("ttl")]
        public long Ttl { get; set; }

        [JsonProperty("data")]
        public BiliData Data { get; set; }

        private static CookieContainer _cookieContainer = new CookieContainer();
        private static HttpClient _httpClient = new HttpClient(
            new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = _cookieContainer,
            }
            );

        public static async Task<BiliSearch> Get(BiliSearchSettings searchSettings)
        {
            Console.WriteLine("Searching Video for " + searchSettings.KeyWord);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36");
            var cookies = _cookieContainer.GetCookies(new Uri("https://bilibili.com"));
            if (cookies == null || cookies.Count==0 || cookies.Any(o=>o.Expired))
            {
                Console.WriteLine("Cookie Null or Expired, getting new one");
                var home_result = await _httpClient.GetAsync(new Uri("https://bilibili.com"));
            }
            else
            {
                Console.WriteLine("Already have cookie, skip homepage");
            }
            //Console.WriteLine(await home_result.Content.ReadAsStringAsync());
            string url = "https://api.bilibili.com/x/web-interface/wbi/search/type?";
            var (imgKey, subKey) = await BiliContentGetter.GetWbiKeys();
            var par = new Dictionary<string, string>
                {
                { "search_type", "video" },
                { "order", "pubdate" },
                { "page", "1" },
                { "page_size", "10" },
                { "platform","pc"},
                { "highlight","0"},
                { "keyword", searchSettings.KeyWord },
                };
            Dictionary<string, string> signedParams = BiliContentGetter.EncWbi(
                parameters: par,
                imgKey: imgKey,
                subKey: subKey
            );
            string query = await new FormUrlEncodedContent(signedParams).ReadAsStringAsync();
            url += query;
            Console.WriteLine( "Get Webapi" );
            var resp = await _httpClient.GetAsync(new Uri(url));
            var json =  await resp.Content.ReadAsStringAsync() ;
            Console.WriteLine("OK.." + json.Substring(0,50));
            return JsonConvert.DeserializeObject<BiliSearch>(json);
        }
    }

    public class BiliContentGetter
    {
        

        private static readonly int[] MixinKeyEncTab =
        {
        46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35, 27, 43, 5, 49, 33, 9, 42, 19, 29, 28, 14, 39,
        12, 38, 41, 13, 37, 48, 7, 16, 24, 55, 40, 61, 26, 17, 0, 1, 60, 51, 30, 4, 22, 25, 54, 21, 56, 59, 6, 63,
        57, 62, 11, 36, 20, 34, 44, 52
    };

        //对 imgKey 和 subKey 进行字符顺序打乱编码
        private static string GetMixinKey(string orig)
        {
            return MixinKeyEncTab.Aggregate("", (s, i) => s + orig[i])[..32];
        }

        public static Dictionary<string, string> EncWbi(Dictionary<string, string> parameters, string imgKey,
            string subKey)
        {
            string mixinKey = GetMixinKey(imgKey + subKey);
            string currTime = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            //添加 wts 字段
            parameters["wts"] = currTime;
            // 按照 key 重排参数
            parameters = parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
            //过滤 value 中的 "!'()*" 字符
            parameters = parameters.ToDictionary(
                kvp => kvp.Key,
                kvp => new string(kvp.Value.Where(chr => !"!'()*".Contains(chr)).ToArray())
            );
            // 序列化参数
            string query = new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result;
            //计算 w_rid
            using MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(query + mixinKey));
            string wbiSign = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            parameters["w_rid"] = wbiSign;

            return parameters;
        }

        // 获取最新的 img_key 和 sub_key
        public static async Task<(string, string)> GetWbiKeys()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36");
            //httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.bilibili.com/");

            HttpResponseMessage responseMessage = await httpClient.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://api.bilibili.com/x/web-interface/nav"),
            });

            JsonNode response = JsonNode.Parse(await responseMessage.Content.ReadAsStringAsync())!;

            string imgUrl = (string)response["data"]!["wbi_img"]!["img_url"]!;
            imgUrl = imgUrl.Split("/")[^1].Split(".")[0];

            string subUrl = (string)response["data"]!["wbi_img"]!["sub_url"]!;
            subUrl = subUrl.Split("/")[^1].Split(".")[0];
            return (imgUrl, subUrl);
        }
    }


    public partial class BiliData
    {
        [JsonProperty("seid")]
        public string Seid { get; set; }

        [JsonProperty("page")]
        public long Page { get; set; }

        [JsonProperty("pagesize")]
        public long Pagesize { get; set; }

        [JsonProperty("numResults")]
        public long NumResults { get; set; }

        [JsonProperty("numPages")]
        public long NumPages { get; set; }


        [JsonProperty("result")]
        public List<BiliResult> Result { get; set; }

    }

    public partial class BiliResult
    {

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("mid")]
        public long Mid { get; set; }

        [JsonProperty("arcurl")]
        public Uri Arcurl { get; set; }

        [JsonProperty("aid")]
        public long Aid { get; set; }

        [JsonProperty("bvid")]
        public string Bvid { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("pic")]
        public string Pic { get; set; }

        [JsonProperty("play")]
        public long Play { get; set; }

        [JsonProperty("video_review")]
        public long VideoReview { get; set; }

        [JsonProperty("favorites")]
        public long Favorites { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("review")]
        public long Review { get; set; }

        [JsonProperty("pubdate")]
        public long Pubdate { get; set; }

        [JsonProperty("senddate")]
        public long Senddate { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("is_pay")]
        public long IsPay { get; set; }

        [JsonProperty("is_union_video")]
        public long IsUnionVideo { get; set; }

        [JsonProperty("rec_tags")]
        public object RecTags { get; set; }

        [JsonProperty("new_rec_tags")]
        public List<object> NewRecTags { get; set; }

        [JsonProperty("rank_score")]
        public long RankScore { get; set; }

        [JsonProperty("like")]
        public long Like { get; set; }

        [JsonProperty("upic")]
        public Uri Upic { get; set; }

        [JsonProperty("danmaku")]
        public long Danmaku { get; set; }

    }
}
