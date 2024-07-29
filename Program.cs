using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using rkllm_sharp;

namespace pudding4
{
    internal class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static Rkllm? llm;
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

            var llmOpt = new LLMsetting();
            if (File.Exists("llm.json"))
            {
                llmOpt = JsonConvert.DeserializeObject<LLMsetting>(File.ReadAllText("llm.json"));
            }
            else
            {
                Console.WriteLine("No ollama.json,creating one.");
                File.WriteAllText("llm.json", JsonConvert.SerializeObject(llmOpt));
                return;
            }

            CqWsSession session = new CqWsSession(options);
            var random = new Random(DateTime.Now.Millisecond);
            await session.StartAsync();                               // 开始连接 (你也可以使用它的异步版本)
            Console.WriteLine("已启动");
            //AI
            if (System.Environment.OSVersion.Platform == PlatformID.Unix)
            {
                Console.WriteLine("Init RKLLM");
                llm = new Rkllm(llmOpt.ModelPath, llmOpt.RKLLMsetting);
                session.UseGroupMessage(async (context, next) =>
                {
                    if (context.RawMessage.StartsWith("[CQ:at,qq=2674713993]") || context.Message.Text.Contains("布丁"))
                    {
                        var sendstr = String.Format(llmOpt.PromptTemplate, llmOpt.SystemPrompt, context.Message.Text);
                        //Console.WriteLine($"Sending {sendstr}");
                        var recvstr = await llm.RunAsync(sendstr);
                        context.QuickOperation.Reply = new CqMessage(recvstr);
                        return;
                    }
                    await next.Invoke();
                    return;
                });
            }
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