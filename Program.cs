using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace pudding4
{
    internal class Program
    {
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
            CqWsSession session = new CqWsSession(options);

            await session.StartAsync();                               // 开始连接 (你也可以使用它的异步版本)
            Console.WriteLine("已启动");
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
            Console.ReadLine();
        }
    }
}