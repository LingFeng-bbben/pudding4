using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using rkllm_sharp;

namespace pudding4
{
    internal class LLMsetting
    {
        public string ModelPath = "";
        public string SystemPrompt = "";
        public string PromptTemplate = "<|im_start|>system{0}<|im_end|>\n<|im_start|>user{1}<|im_end|>\n<|im_start|>assistant\n";
        public RkllmParameters RKLLMsetting = new RkllmParameters();
    }
}
