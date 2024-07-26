using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Ollama
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class OllamaSettings
    {
        public string systemMessage { get; set;}
        public string chatAddress { get; set;}
    }

    public partial class OllamaReply
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }

        [JsonProperty("done_reason")]
        public string DoneReason { get; set; }

        [JsonProperty("done")]
        public bool Done { get; set; }

        [JsonProperty("total_duration")]
        public long TotalDuration { get; set; }

        [JsonProperty("load_duration")]
        public long LoadDuration { get; set; }

        [JsonProperty("prompt_eval_count")]
        public long PromptEvalCount { get; set; }

        [JsonProperty("prompt_eval_duration")]
        public long PromptEvalDuration { get; set; }

        [JsonProperty("eval_count")]
        public long EvalCount { get; set; }

        [JsonProperty("eval_duration")]
        public long EvalDuration { get; set; }
    }

    public partial class Message
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        public Message() { }
        public Message(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }

    public partial class OllamaSend
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("messages")]
        public Message[] Messages { get; set; }

        [JsonProperty("options")]
        public Options Options { get; set; }

        [JsonProperty("stream")]
        public bool Stream { get; set; }

        public OllamaSend(string message,string system,string model = "qwen2:1.5b",double temprature = 9999)
        {
            Model = model;
            Messages = new Message[] { new Message("system",system),new Message("user",message)};
            Stream = false;
            Options = new Options() { Temperature = temprature };
        }
    }

    public partial class Options
    {
        [JsonProperty("temperature")]
        public double Temperature { get; set; }
    }
}

