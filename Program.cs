using EleCho.GoCqHttpSdk;
using EleCho.GoCqHttpSdk.Message;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using rkllm_sharp;
using System.Collections.Generic;
using System.Diagnostics;

namespace pudding4
{
    internal class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static Rkllm? llm;
        private static bool isDuicheng = false;
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
                        Task.Run(async () =>
                        {
                            var recvstr = await llm.RunAsync(sendstr);
                            await session.SendGroupMessageAsync(context.GroupId, new CqMessage(recvstr));
                        });
                        
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
            session.UseGroupMessage(async (context, next) => {
                var text = context.Message.Text.ToLower();
                if (text.StartsWith("随机谱面"))
                {
                    var message = await MajNet.GetRandomSong();
                    await session.SendGroupMessageAsync(context.GroupId, CqMessage.FromCqCode(message));
                }
                if (text.StartsWith("随机mmfc"))
                {
                    var message = await MajNet.GetRandomSong(true);
                    await session.SendGroupMessageAsync(context.GroupId, CqMessage.FromCqCode(message));
                }
                await next.Invoke();
            });
            session.UseGroupMessage(async (context, next) =>
            {

                if (context.Message.First().MsgType == "reply")
                {
                    try
                    {
                        if (context.Message.Text.Contains("对称"))
                        {
                            if (isDuicheng)
                            {
                                await session.SendGroupMessageAsync(context.GroupId, new CqMessage("别急"));
                                return;
                            }
                            isDuicheng = true;
                            Task.Run(async () => {
                                var orig = (CqReplyMsg)context.Message.First();
                                var replyorig = (CqImageMsg)session.GetMessage((long)orig.Id).Message.First();
                                var imgurl = replyorig.Url;
                                HttpClient Client = new HttpClient();
                                Console.WriteLine("Getting Image");
                                Client.DefaultRequestHeaders.Add("Accept", "*/*");
                                var cancel = new CancellationTokenSource();
                                cancel.CancelAfter(5000);
                                var resp = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, imgurl), cancel.Token);
                                if (resp.StatusCode != System.Net.HttpStatusCode.OK) return;
                                var contType = resp.Content.Headers.GetValues("Content-Type").First();
                                var filename = Guid.NewGuid().ToString();
                                var outname = filename + "_out";
                                if (contType == "image/jpeg")
                                {
                                    filename += ".jpg";
                                    outname += ".jpg";
                                }
                                else if (contType == "image/png")
                                {
                                    filename += ".png";
                                    outname += ".png";
                                }
                                else if (contType == "image/gif")
                                {
                                    filename += ".gif";
                                    outname += ".gif";
                                }
                                else
                                {
                                    return;
                                }
                                Console.WriteLine("Saving");
                                var file = await resp.Content.ReadAsByteArrayAsync();
                                File.WriteAllBytes(filename, file);
                                var command = "";
                                if (context.Message.Text.Contains("右"))
                                {
                                    command = String.Format(
                                        "-i {0} -vf \"[0:v]split[a][b];[a]crop=x=in_w/2:w=in_w/2[a1];[b]crop=x=in_w/2:w=in_w/2[b1];[b1]hflip[b2];[b2][a1]hstack\" {1}",
                                        filename, outname);
                                }
                                else if (context.Message.Text.Contains("上"))
                                {
                                    command = String.Format(
                                        "-i {0} -vf \"[0:v]split[a][b];[a]crop=y=0:h=in_h/2[a1];[b]crop=y=0:h=in_h/2[b1];[b1]vflip[b2];[a1][b2]vstack\" {1}",
                                        filename, outname);
                                }
                                else if (context.Message.Text.Contains("下"))
                                {
                                    command = String.Format(
                                        "-i {0} -vf \"[0:v]split[a][b];[a]crop=y=in_h/2:h=in_h/2[a1];[b]crop=y=in_h/2:h=in_h/2[b1];[b1]vflip[b2];[b2][a1]vstack\" {1}",
                                        filename, outname);
                                }
                                else
                                {
                                    command = String.Format(
                                        "-i {0} -vf \"[0:v]split[a][b];[a]crop=x=0:w=in_w/2[a1];[b]crop=x=0:w=in_w/2[b1];[b1]hflip[b2];[a1][b2]hstack\" {1}",
                                        filename, outname);
                                }
                                var startInfo = new ProcessStartInfo()
                                {
                                    FileName = "ffmpeg",
                                    Arguments = command,
                                    WorkingDirectory = Environment.CurrentDirectory,
                                };
                                Console.WriteLine("Run ffmpeg");
                                var proc = Process.Start(startInfo);
                                await Task.Run(() => proc.WaitForExit(2000));
                                if (File.Exists(outname))
                                {
                                    Console.WriteLine("Upload");
                                    var outinfo = new FileInfo(outname);
                                    var imgmsg = CqImageMsg.FromFile(outinfo.FullName);
                                    await session.SendGroupMessageAsync(context.GroupId, new CqMessage(imgmsg));
                                    Console.WriteLine("Del image");
                                    File.Delete(outname);
                                    File.Delete(filename);
                                    Console.WriteLine("ok");
                                    isDuicheng = false;
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        await session.SendGroupMessageAsync(context.GroupId, new CqMessage("出错了哟"));
                        isDuicheng = false;
                    }
                }
                //
                await next.Invoke();
            });
            Console.ReadLine();
        }
    }
}