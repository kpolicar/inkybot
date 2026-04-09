using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inkybot.Services
{
    public class CustomizeApiClient
    {
        public async Task<CustomizeResponse> GenerateScript(
            string userMessage, string currentScript, List<ChatMessage> history) {
            // TODO: Replace with real API call to LLM endpoint
            await Task.Delay(500);
            return new CustomizeResponse {
                Code = "using Inkybot.Dofus;\n" +
                       "using Inkybot.Dofus.Contracts;\n" +
                       "using Inkybot.Dofus.Domain;\n\n" +
                       "public class CustomScript : CustomDofusMagingAI\n" +
                       "{\n" +
                       "    protected override IAction Resolve() {\n" +
                       "        // Generated from: " + userMessage + "\n" +
                       "        return ResolveDefault();\n" +
                       "    }\n" +
                       "}",
                Message = "Here's a script based on your request. Click Apply to load it."
            };
        }
    }

    public class CustomizeResponse
    {
        public string Code { get; set; } = "";
        public string Message { get; set; } = "";
    }

    public class ChatMessage
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
    }
}
