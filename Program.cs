using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace pudding4
{
    internal class Program
    {
        private static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {
            var options = new CqWsSessionOptions();
            if (File.Exists("settings.json"))
            {
                options = JsonConvert.DeserializeObject<CqWsSessionOptions>(File.ReadAllText("settings.json"));
            }
            else
            {
                Console.WriteLine("No settings.json,creating one.");
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(options));
                return;
            }

            var ollamaOpt = new Ollama.OllamaSettings();
            if (File.Exists("ollama.json"))
            {
                ollamaOpt = JsonConvert.DeserializeObject<Ollama.OllamaSettings>(File.ReadAllText("ollama.json"));
            }
            else
            {
                Console.WriteLine("No ollama.json,creating one.");
                File.WriteAllText("ollama.json", JsonConvert.SerializeObject(ollamaOpt));
                return;
            }

            CqWsSession session = new CqWsSession(options);
            var random = new Random(DateTime.Now.Millisecond);
            await session.StartAsync();                               // 开始连接 (你也可以使用它的异步版本)
            Console.WriteLine("已启动");
            //AI
            session.UseGroupMessage(async (context, next) => {
                if (!context.RawMessage.StartsWith("[CQ:at,qq=2674713993]")){
                    await next.Invoke();
                    return;
                }
                var sendstr = JsonConvert.SerializeObject(new Ollama.OllamaSend(context.Message.Text.Trim(), ollamaOpt.systemMessage));
                //Console.WriteLine($"Sending {sendstr}");
                var response = await client.PostAsync(ollamaOpt.chatAddress,new StringContent(sendstr));
                var recvstr = await response.Content.ReadAsStringAsync();
                var recv = JsonConvert.DeserializeObject<Ollama.OllamaReply>(recvstr);
                //Console.WriteLine(recv.Message.Content);
                context.QuickOperation.Reply = new CqMessage(recv.Message.Content);
            });
            session.UseGroupMessage(async (context, next) => {
                var text = context.Message.Text;
                if (text.StartsWith("#csm"))
                {
                    var system = String.Format("现在的时间是{0}，考虑当前时间随机给出食物的建议。省去繁文缛节，只列出3个菜品。省去菜品描述，只返回菜品名称。", DateTime.Now.ToString("tt hh:mm"));
                    var prompt = text.Length > 4 ? context.Message.Text.Substring(4).Trim() : "吃什么";
                    var sendstr = JsonConvert.SerializeObject(new Ollama.OllamaSend(prompt, system, "qwen2:7b-instruct-q4_0", 1.3));
                    //Console.WriteLine($"Sending {sendstr}");
                    var response = await client.PostAsync(ollamaOpt.chatAddress, new StringContent(sendstr));
                    var recvstr = await response.Content.ReadAsStringAsync();
                    var recv = JsonConvert.DeserializeObject<Ollama.OllamaReply>(recvstr);
                    //Console.WriteLine(recv.Message.Content);
                    context.QuickOperation.Reply = new CqMessage(recv.Message.Content);
                    return;
                }
                await next.Invoke();
            });
            //关键词
            session.UseGroupMessage(async (context, next) =>
            {
                var text = context.Message.Text;
                if (text.Contains("njmlp"))
                    await session.SendGroupMessageAsync(context.GroupId, new CqMessage("にじゃまれぴ！"));
                if (text.Contains("🍮"))
                {
                    if (text.Contains("💩"))
                        await session.SendGroupMessageAsync(context.GroupId, new CqMessage("味道有点怪哟"));
                    else
                        await session.SendGroupMessageAsync(context.GroupId, new CqMessage("全部吃掉了哟"));
                }
                if (text.Contains("布") && text.Contains("丁"))
                    await session.SendGroupMessageAsync(context.GroupId, new CqMessage("喊你祖宗干嘛"));
                if (text.Contains("尼") && text.Contains("莫"))
                    await session.SendGroupMessageAsync(context.GroupId, new CqMessage("呕啊"));
                if (text.Contains("公"))
                    await session.SendGroupMessageAsync(context.GroupId, new CqMessage(text.Replace("公", "母")));
                if (text.Contains("如何评价"))
                    await session.SendGroupMessageAsync(context.GroupId, new CqMessage("豆瓣拒绝评分"));
                if (text.Contains("对吗"))
                    await session.SendGroupMessageAsync(context.GroupId, new CqMessage("不对吧"));
                if (text.Contains("不对"))
                    await session.SendGroupMessageAsync(context.GroupId, new CqMessage("对的对的"));
                if (text.Contains("冰！"))
                    await session.SendGroupMessageAsync(context.GroupId, new CqMessage("大家好啊，我是说的布丁"));
                if (text.Contains("明日方舟"))
                    await session.SendGroupMessageAsync(context.GroupId, new CqMessage("脚臭吧"));
                if (text.Contains("入院") || text.Contains("出院"))
                    await session.SendGroupMessageAsync(context.GroupId, new CqMessage("已批准"));
                if (text.StartsWith("玩") && text.EndsWith("玩的"))
                    await session.SendGroupMessageAsync(context.GroupId, new CqMessage("活该"));
                await next.Invoke();    // 执行下一个中间件
            });
            //dydy
            session.UseGroupMessage(async (context, next) => {
                var text = context.RawMessage;
                if (text.StartsWith("#dydy-add"))
                {
                    var dycontent = text.Substring(9).Trim();
                    if (dycontent == "")
                    {
                        await session.SendGroupMessageAsync(context.GroupId, new CqMessage("空的加你吗呢"));
                        return;
                    }
                    if (File.Exists("dydy3.json"))
                    {
                        var dydys = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("dydy3.json"));
                        if (dydys != null)
                        {
                            dydys.Add(dycontent);
                            File.WriteAllText("dydy3.json", JsonConvert.SerializeObject(dydys));
                        }
                    }
                    else
                    {
                        var dydys = new List<string>();
                        dydys.Add(dycontent);
                        File.WriteAllText("dydy3.json", JsonConvert.SerializeObject(dydys));
                    }
                    await session.SendGroupMessageAsync(context.GroupId, new CqMessage("已添加"));
                }
                else if (text.StartsWith("#dydy")){
                    var dydys = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("dydy3.json"));
                    var dy = CqMessage.FromCqCode(dydys[random.Next(dydys.Count)]);
                    await session.SendGroupMessageAsync(context.GroupId, new CqMessage(dy));
                }
                await next.Invoke();
            });
            //jrrp
            session.UseGroupMessage(async (context, next) => {
                var text = context.Message.Text;
                if (text.StartsWith("#jrrp"))
                {
                    var qid = context.Sender.UserId;
                    Random random = new Random(System.DateTime.Today.DayOfYear + (int)qid);
                    var rp = random.Next(0, 100);
                    var message = CqMessage.FromCqCode(String.Format("[CQ:at,qq={0}] 的今日人品是{1}哟", qid, rp));
                    await session.SendGroupMessageAsync(context.GroupId, message);
                }
                await next.Invoke();
            });
            //rbdx
            session.UseGroupMessage(async (context, next) => {
                var text = context.Message.Text;
                if (text.StartsWith("#rbdx"))
                {
                    var argument = "";
                    if (text.Length > 5)
                    {
                        argument = text.Substring(6); 
                    }
                    var info = await Rbdx.GetRbdxSongs(argument);
                    var message = CqMessage.FromCqCode(info);
                    await session.SendGroupMessageAsync(context.GroupId, message);
                }
                await next.Invoke();
            });
            Console.ReadLine();
        }
    }
}