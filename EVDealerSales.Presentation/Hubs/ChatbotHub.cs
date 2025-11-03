using EVDealerSales.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace EVDealerSales.Presentation.Hubs
{
        [Authorize]
        public class ChatbotHub : Hub
    {
        private readonly IChatbotService _chatbotService;
        private readonly ILogger<ChatbotHub> _logger;

        public ChatbotHub(IChatbotService chatbotService, ILogger<ChatbotHub> logger)
        {
            _chatbotService = chatbotService;
            _logger = logger;
            _logger.LogInformation("ChatbotHub initialized.");
        }

        public async Task AskChatbot(string prompt, string groupId)
        {
            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(prompt))
                {
                    _logger.LogWarning("AskChatbot called with empty prompt from {ConnectionId}", Context.ConnectionId);
                    await Clients.Caller.SendAsync("ReceiveChatbotError", new { GroupId = groupId, Error = "Prompt is required.", Timestamp = DateTime.UtcNow });
                    return;
                }

                if (string.IsNullOrWhiteSpace(groupId))
                {
                    _logger.LogWarning("AskChatbot called with empty groupId from {ConnectionId}", Context.ConnectionId);
                    await Clients.Caller.SendAsync("ReceiveChatbotError", new { GroupId = (string?)null, Error = "GroupId is required.", Timestamp = DateTime.UtcNow });
                    return;
                }

                var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
                _logger.LogInformation("AskChatbot invoked by user {UserId} (conn: {ConnectionId}) for group {GroupId}", userId, Context.ConnectionId, groupId);

                var response = await _chatbotService.FreestyleAskAsync(prompt, groupId);

                // Normalize response: strip code fences, decode HTML entities, and extract a single <table> if present.
                string NormalizeResponse(string raw)
                {
                    if (string.IsNullOrEmpty(raw)) return raw ?? string.Empty;

                    // 1) Remove triple-backtick fences and optional language tag (e.g., ```html\n...```).
                    raw = System.Text.RegularExpressions.Regex.Replace(raw, "```(?:\\w+)?\\r?\\n([\\s\\S]*?)```", "$1", System.Text.RegularExpressions.RegexOptions.Multiline);

                    // 2) HTML-decode escaped entities (e.g., &lt;table&gt; -> <table>)
                    raw = System.Net.WebUtility.HtmlDecode(raw);

                    return raw.Trim();
                }

                string cleaned = NormalizeResponse(response);

                // Try to extract a <table>...</table> block if present
                var tableMatch = System.Text.RegularExpressions.Regex.Match(cleaned, "<table[\\s\\S]*?</table>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                string? tableHtml = tableMatch.Success ? tableMatch.Value : null;

                // Log response length and a short snippet to aid debugging
                var length = response?.Length ?? 0;
                var snippet = string.IsNullOrEmpty(response) ? string.Empty : (length > 200 ? response.Substring(0, 200) + "..." : response);
                _logger.LogInformation("Chatbot response length={Length} for group {GroupId}; snippet={Snippet}", length, groupId, snippet);

                if (string.IsNullOrWhiteSpace(response))
                {
                    _logger.LogWarning("Chatbot returned empty response for group {GroupId}", groupId);
                    await Clients.Caller.SendAsync("ReceiveChatbotError", new { GroupId = groupId, Error = "Assistant returned an empty response.", Timestamp = DateTime.UtcNow });
                    return;
                }

                await Clients.Group(groupId).SendAsync("ReceiveChatbotResponse", new
                {
                    GroupId = groupId,
                    // Plain-text cleaned response (for logging / fallback)
                    Response = cleaned,
                    // If a table was extracted, include it as HTML so the client can render it safely
                    ResponseHtml = tableHtml,
                    FromUser = userId,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AskChatbot failed for connection {ConnectionId} group {GroupId}", Context.ConnectionId, groupId);
                await Clients.Caller.SendAsync("ReceiveChatbotError", new { GroupId = groupId, Error = ex.Message, Timestamp = DateTime.UtcNow });
            }
        }

        public async Task JoinChatGroup(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                await Clients.Caller.SendAsync("ReceiveChatbotError", new { GroupId = (string?)null, Error = "GroupId is required to join a group.", Timestamp = DateTime.UtcNow });
                return;
            }

            _logger.LogInformation("User {ConnectionId} joining group {GroupId}", Context.ConnectionId, groupId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        }

        public async Task LeaveChatGroup(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
            {
                await Clients.Caller.SendAsync("ReceiveChatbotError", new { GroupId = (string?)null, Error = "GroupId is required to leave a group.", Timestamp = DateTime.UtcNow });
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
        }

        /// <summary>
        /// Instruct the chatbot to generate and create a vehicle. The created vehicle response will be broadcast to the group.
        /// </summary>
        public async Task AutomateAddVehicle(string instruction, string groupId)
        {
            _logger.LogInformation("AutomateAddVehicle requested by {ConnectionId} for group {GroupId}", Context.ConnectionId, groupId);

            try
            {
                if (string.IsNullOrWhiteSpace(instruction))
                {
                    await Clients.Caller.SendAsync("ReceiveAutomateAddVehicleError", new { GroupId = groupId, Error = "Instruction is required.", Timestamp = DateTime.UtcNow });
                    return;
                }

                if (string.IsNullOrWhiteSpace(groupId))
                {
                    await Clients.Caller.SendAsync("ReceiveAutomateAddVehicleError", new { GroupId = (string?)null, Error = "GroupId is required.", Timestamp = DateTime.UtcNow });
                    return;
                }

                var vehicle = await _chatbotService.AutomateAddVehicleAsync(instruction);

                await Clients.Group(groupId).SendAsync("ReceiveAutomateAddVehicleResult", new
                {
                    GroupId = groupId,
                    Vehicle = vehicle,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AutomateAddVehicle failed for group {GroupId}", groupId);
                await Clients.Caller.SendAsync("ReceiveAutomateAddVehicleError", new
                {
                    GroupId = groupId,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        // Manager: generate a vehicle spec (JSON) for preview
        public async Task GenerateVehicleSpec(string instruction, string groupId)
        {
            _logger.LogInformation("GenerateVehicleSpec requested by {ConnectionId} for group {GroupId}", Context.ConnectionId, groupId);
            try
            {
                if (string.IsNullOrWhiteSpace(instruction))
                {
                    await Clients.Caller.SendAsync("ReceiveGenerateSpecError", new { GroupId = groupId, Error = "Instruction is required.", Timestamp = DateTime.UtcNow });
                    return;
                }

                if (string.IsNullOrWhiteSpace(groupId))
                {
                    await Clients.Caller.SendAsync("ReceiveGenerateSpecError", new { GroupId = (string?)null, Error = "GroupId is required.", Timestamp = DateTime.UtcNow });
                    return;
                }

                var spec = await _chatbotService.GenerateVehicleSpecAsync(instruction);
                // Send spec back only to caller (preview) and to group as a notification
                await Clients.Caller.SendAsync("ReceiveGeneratedVehicleSpec", new { GroupId = groupId, Spec = spec, Timestamp = DateTime.UtcNow });
                await Clients.Group(groupId).SendAsync("NotifyGroup", new { GroupId = groupId, Message = "Manager requested a suggested vehicle spec.", Timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GenerateVehicleSpec failed for group {GroupId}", groupId);
                await Clients.Caller.SendAsync("ReceiveGenerateSpecError", new { GroupId = groupId, Error = ex.Message, Timestamp = DateTime.UtcNow });
            }
        }

        // Manager: create vehicle from a spec (JSON string)
        public async Task CreateVehicleFromSpec(string spec, string groupId)
        {
            _logger.LogInformation("CreateVehicleFromSpec requested by {ConnectionId} for group {GroupId}", Context.ConnectionId, groupId);
            try
            {
                if (string.IsNullOrWhiteSpace(spec))
                {
                    await Clients.Caller.SendAsync("ReceiveAutomateAddVehicleError", new { GroupId = groupId, Error = "Specification is required.", Timestamp = DateTime.UtcNow });
                    return;
                }

                if (string.IsNullOrWhiteSpace(groupId))
                {
                    await Clients.Caller.SendAsync("ReceiveAutomateAddVehicleError", new { GroupId = (string?)null, Error = "GroupId is required.", Timestamp = DateTime.UtcNow });
                    return;
                }

                var created = await _chatbotService.CreateVehicleFromSpecAsync(spec);
                await Clients.Group(groupId).SendAsync("ReceiveAutomateAddVehicleResult", new
                {
                    GroupId = groupId,
                    Vehicle = created,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateVehicleFromSpec failed for group {GroupId}", groupId);
                await Clients.Caller.SendAsync("ReceiveAutomateAddVehicleError", new { GroupId = groupId, Error = ex.Message, Timestamp = DateTime.UtcNow });
            }
        }
    }
}
