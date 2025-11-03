using EVDealerSales.Business.Interfaces;
using EVDealerSales.BusinessObject.DTOs.VehicleDTOs;
using System.Text.Json;

namespace EVDealerSales.Business.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly IDataAnalyzerService _analyzerService;
        private readonly IGeminiService _geminiService;
        private readonly IVehicleService _vehicleService;

        public ChatbotService(IDataAnalyzerService analyzerService, IGeminiService geminiService, IVehicleService vehicleService)
        {
            _analyzerService = analyzerService;
            _geminiService = geminiService;
            _vehicleService = vehicleService;
        }

        public async Task<string> FreestyleAskAsync(string prompt, string? groupId = null)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt is required.");

        // --- Retrieve analytical data ---
        var vehicles = await _analyzerService.AnalyzeVehiclesAsync();
        var orders = await _analyzerService.AnalyzeSalesAsync();
        var feedbacks = await _analyzerService.AnalyzeFeedbacksAsync();

        // --- Format vehicle data ---
        var vehicleContext = string.Join("\n", vehicles.Select(v => $"""
            Trim: {v.TrimName}
            Model: {v.ModelName}
            Year: {v.ModelYear}
            Price: {v.BasePrice}
            Battery: {v.BatteryCapacity}
            Range: {v.RangeKM}
            Charging Time: {v.ChargingTime}
            Top Speed: {v.TopSpeed}
            Stock: {v.Stock}
            """));

        // --- Format order data (null-safe) ---
        var orderContext = string.Join("\n", orders.Select(o => $"""
            Order ID: {o.Id}
            Customer: {o.Customer?.FullName}
            Total Items: {o.Items?.Count ?? 0} 
            => {string.Join(", ", o.Items?.Select(i => i?.Vehicle?.ModelName) ?? Enumerable.Empty<string>())}
            Total Price: {o.TotalAmount}
            Status: {o.Status}
            Notes: {o.Notes}
            """));

        // --- Format feedback data ---
        var feedbackContext = string.Join("\n", feedbacks.Select(f => $"""
            Feedback ID: {f.Id}
            Customer: {f.Customer?.FullName}
            Order ID: {f.OrderId}
            Content: {f.Content}
            Resolved By: {f.Resolver?.FullName ?? "Unresolved"}
            """));

        // --- Build prompt for Gemini ---
        var contextPrompt = $"""
        [System Instruction]
        You are an advanced EV dealership consultant with market knowledge beyond the local store inventory.
        You can use your general understanding of global EV models, brands, specifications, and market trends
        to answer user questions — even if the vehicle is not present in the provided context.

        When discussing a vehicle not found in the current store:
        - Provide known or estimated information (range, price range, battery, market demand).
        - Mention it is not currently in inventory.
        - Suggest if it should be imported, based on market demand and category fit.
        - Avoid saying "I don't know" or "not in inventory" unless explicitly asked about stock availability.

        If the user asks for comparison, include both in-store and external vehicles in your analysis.

        **PRIORITY FORMATTING RULES:**
        - ALWAYS USE TABLES FOR INFORMATION, DATA, COMPARISONS, AND ANALYSIS
        - For any request involving data, comparisons, vehicle specifications, market analysis, sales data, customer feedback analysis, inventory status, or performance metrics, return a single HTML table
        - Use appropriate column headers based on the request (e.g., "Vehicle", "Price", "Range", "Rating" for comparisons)
        - Include 3-10 rows when possible
        - Only use plain text for simple how-to instructions that don't involve structured data
        [/System Instruction]

        [Vehicle Context]
        {vehicleContext}

        [Order Context]
        {orderContext}

        [Feedback Context]
        {feedbackContext}

        [User Question]
        {prompt}
        """;

        return await _geminiService.GetGeminiResponseAsync(contextPrompt);
    }

        /// <summary>
        /// Use the chatbot to generate a vehicle specification and create it via IVehicleService.
        /// The chatbot is instructed to return a single JSON object matching CreateVehicleRequestDto.
        /// </summary>
        public async Task<VehicleResponseDto> AutomateAddVehicleAsync(string instruction)
        {
            if (string.IsNullOrWhiteSpace(instruction))
                throw new ArgumentException("Instruction is required.");

            // Build a prompt asking the assistant to produce a JSON matching CreateVehicleRequestDto
            var jsonPrompt = $"""
            You are given current vehicle inventory and sales context. Based on the instruction below, suggest one vehicle to add to inventory and return the result as a JSON object matching the following schema exactly (no extra fields):

            [CreateVehicleRequestDto]
              "ModelName": string,
              "TrimName": string,
              "ModelYear": integer or null,
              "BasePrice": number,
              "ImageUrl": string,
              "BatteryCapacity": integer,
              "RangeKM": integer,
              "ChargingTime": integer,
              "TopSpeed": integer,
              "Stock": integer,
              "IsActive": boolean
            [/CreateVehicleRequestDto]

            Use sensible, realistic values. Do not include any explanatory text — only output the JSON object.

            Context: (list recent vehicles and sales trends briefly)
            """ + instruction;

            var assistantOutput = await _geminiService.GetGeminiResponseAsync(jsonPrompt);

            // Try JSON parse first, then fall back to tolerant text parsing
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true
            };

            // Helper local function to attempt JSON extraction
            CreateVehicleRequestDto? TryParseJson(string raw)
            {
                if (string.IsNullOrWhiteSpace(raw)) return null;
                var start = raw.IndexOf('{');
                var end = raw.LastIndexOf('}');
                if (start < 0 || end < 0 || end < start) return null;
                var json = raw[start..(end + 1)];
                try
                {
                    var dto = JsonSerializer.Deserialize<CreateVehicleRequestDto>(json, options);
                    return dto;
                }
                catch
                {
                    return null;
                }
            }

            var createDto = TryParseJson(assistantOutput);
            if (createDto == null)
            {
                // attempt tolerant plain-text parsing (e.g., 'Make: Volvo', 'Price: 49,950.00')
                createDto = ParseSpecTextToDto(assistantOutput);
            }

            if (createDto == null)
            {
                throw new InvalidOperationException("Assistant did not return a valid JSON object.");
            }

            // Validate minimal required fields
            if (string.IsNullOrWhiteSpace(createDto.ModelName) || string.IsNullOrWhiteSpace(createDto.TrimName))
                throw new InvalidOperationException("Generated vehicle must include ModelName and TrimName.");

            // Call the vehicle service to create the vehicle
            var created = await _vehicleService.CreateVehicleAsync(createDto);
            return created;
        }

        /// <summary>
        /// Create a vehicle from a specification string (usually generated by the chatbot).
        /// </summary>
        public async Task<VehicleResponseDto> CreateVehicleFromSpecAsync(string spec)
        {
            if (string.IsNullOrWhiteSpace(spec))
                throw new ArgumentException("Specification is required.");

            // Reusing the JSON parsing logic from AutomateAddVehicleAsync
            try
            {
                var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    PropertyNameCaseInsensitive = true
                };

                CreateVehicleRequestDto? createDto = null;

                // Try JSON first
                var start = spec.IndexOf('{');
                var end = spec.LastIndexOf('}');
                if (start >= 0 && end >= 0 && end >= start)
                {
                    var json = spec[start..(end + 1)];
                    try
                    {
                        createDto = JsonSerializer.Deserialize<CreateVehicleRequestDto>(json, options);
                    }
                    catch { createDto = null; }
                }

                if (createDto == null)
                {
                    // Try JSON parse first, then fall back to tolerant text parsing
                    // fallback to tolerant plain-text parsing
                    createDto = ParseSpecTextToDto(spec);
                }

                if (createDto == null)
                    throw new InvalidOperationException("Specification does not contain a valid JSON object.");

                if (string.IsNullOrWhiteSpace(createDto.ModelName) || string.IsNullOrWhiteSpace(createDto.TrimName))
                    throw new InvalidOperationException("Specification must include ModelName and TrimName.");

                // Call the vehicle service to create the vehicle
                var created = await _vehicleService.CreateVehicleAsync(createDto);
                return created;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse JSON from specification.", ex);
            }
        }

        // Attempt to parse assistant plain-text spec into CreateVehicleRequestDto
        private CreateVehicleRequestDto? ParseSpecTextToDto(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            var lines = raw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                           .Select(l => l.Trim())
                           .Where(l => !string.IsNullOrWhiteSpace(l))
                           .ToList();

            if (!lines.Any()) return null;

            var dto = new CreateVehicleRequestDto();

            // Simple regex helpers
            var kvRegex = new System.Text.RegularExpressions.Regex("^\\s*([A-Za-z0-9 _-]{2,40})\\s*[:\\-\\)]\\s*(.+)$");
            var urlRegex = new System.Text.RegularExpressions.Regex(@"https?://\S+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                // If line contains a URL, treat as imageUrl
                var urlMatch = urlRegex.Match(line);
                if (urlMatch.Success && string.IsNullOrWhiteSpace(dto.ImageUrl))
                {
                    dto.ImageUrl = urlMatch.Value;
                    continue;
                }

                var m = kvRegex.Match(line);
                if (!m.Success) continue;
                var key = m.Groups[1].Value.Trim().ToLowerInvariant();
                var val = m.Groups[2].Value.Trim();

                // Normalize commas in numbers
                var normalizedVal = val.Replace(",", "").Replace("USD", "").Trim();

                try
                {
                    switch (key)
                    {
                        case var k when k.Contains("make") || k.Contains("brand") || k.Contains("model") && !k.Contains("trim"):
                        case "make":
                        case "model":
                            // If there is also a trim line, prefer to set ModelName here
                            if (string.IsNullOrWhiteSpace(dto.ModelName)) dto.ModelName = val;
                            break;
                        case var k when k.Contains("trim"):
                        case "trim":
                        case "trimname":
                            dto.TrimName = val;
                            break;
                        case "year":
                        case "modelyear":
                            if (int.TryParse(new string(normalizedVal.Where(c => char.IsDigit(c)).ToArray()), out var y)) dto.ModelYear = y;
                            break;
                        case var k when k.Contains("price") || k.Contains("price:"):
                        case "price":
                            if (decimal.TryParse(normalizedVal, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var price)) dto.BasePrice = price;
                            break;
                        case var k when k.Contains("battery") || k.Contains("batterycapacity"):
                        case "battery":
                        case "batterycapacity":
                            if (int.TryParse(new string(normalizedVal.Where(c => char.IsDigit(c)).ToArray()), out var bat)) dto.BatteryCapacity = bat;
                            break;
                        case var k when k.Contains("range") || k.Contains("rangekm"):
                        case "range":
                        case "rangekm":
                            if (int.TryParse(new string(normalizedVal.Where(c => char.IsDigit(c)).ToArray()), out var rg)) dto.RangeKM = rg;
                            break;
                        case var k when k.Contains("charging") || k.Contains("chargingtime"):
                        case "charging":
                        case "chargingtime":
                            if (int.TryParse(new string(normalizedVal.Where(c => char.IsDigit(c)).ToArray()), out var ct)) dto.ChargingTime = ct;
                            break;
                        case var k when k.Contains("speed") || k.Contains("top speed"):
                        case "topspeed":
                        case "top speed":
                            if (int.TryParse(new string(normalizedVal.Where(c => char.IsDigit(c)).ToArray()), out var ts)) dto.TopSpeed = ts;
                            break;
                        case "stock":
                        case "initial stock":
                            if (int.TryParse(new string(normalizedVal.Where(c => char.IsDigit(c)).ToArray()), out var st)) dto.Stock = st;
                            break;
                        case var k when k.Contains("active") || k.Contains("isactive"):
                        case "isactive":
                            if (bool.TryParse(val, out var b)) dto.IsActive = b;
                            else dto.IsActive = !(val.ToLowerInvariant().StartsWith("no") || val.ToLowerInvariant().StartsWith("false"));
                            break;
                        default:
                            // ignore unknown keys
                            break;
                    }
                }
                catch { /* ignore parse errors per-line */ }
            }
            // Parse plain-text assistant outputs into CreateVehicleRequestDto. This supports lines like:
            // "Make: Volvo", "Model: XC40 Recharge (Long Range)", "Price: 49,950.00", "Battery: 78 kWh", etc.

            // If ModelName not set but there is a combined 'Make: Model (Trim)' format, try to extract
            if (string.IsNullOrWhiteSpace(dto.TrimName) && !string.IsNullOrWhiteSpace(dto.ModelName))
            {
                // attempt to split by parentheses
                var m = System.Text.RegularExpressions.Regex.Match(dto.ModelName, "^(.*)\\s+\\((.*)\\)$");
                if (m.Success)
                {
                    dto.ModelName = m.Groups[1].Value.Trim();
                    dto.TrimName = m.Groups[2].Value.Trim();
                }
            }

            // Basic defaults if missing
            if (string.IsNullOrWhiteSpace(dto.ModelName) && lines.Any())
            {
                // try first line as model if not parsed
                var first = lines.First();
                if (first.Length > 0 && first.Length < 80 && !first.Contains(":")) dto.ModelName = first;
            }

            // If no imageUrl set, leave null (VehicleService requires it, so creation will error)

            // If dto seems empty, return null
            var anySet = !string.IsNullOrWhiteSpace(dto.ModelName) || !string.IsNullOrWhiteSpace(dto.TrimName) || dto.BasePrice > 0 || dto.BatteryCapacity > 0 || dto.RangeKM > 0;

            // If we didn't find anything useful from per-line parsing, try a more tolerant global-regex pass
            if (!anySet)
            {
                try
                {
                    // global regex helpers that find key:value pairs anywhere in the text
                    string FindValue(string[] keys)
                    {
                        foreach (var k in keys)
                        {
                            // verbatim interpolated string so backslashes are preserved for regex
                            var r = new System.Text.RegularExpressions.Regex(@$"(?im){k}\s*[:\-]\s*(.+?)($|\r?\n|\r)");
                            var m = r.Match(raw);
                            if (m.Success)
                            {
                                var v = m.Groups[1].Value.Trim();
                                if (!string.IsNullOrWhiteSpace(v)) return v;
                            }
                        }

                        return string.Empty;
                    }

                    var modelVal = FindValue(new[] { "model", "make" });
                    var trimVal = FindValue(new[] { "trim", "trimname" });
                    var yearVal = FindValue(new[] { "year", "modelyear" });
                    var priceVal = FindValue(new[] { "price", "cost" });
                    var batteryVal = FindValue(new[] { "battery", "battery capacity", "batterycapacity" });
                    var rangeVal = FindValue(new[] { "range", "rangekm" });
                    var chargingVal = FindValue(new[] { "charging", "charging time", "chargingtime" });
                    var speedVal = FindValue(new[] { "top speed", "topspeed", "speed" });
                    var stockVal = FindValue(new[] { "stock", "initial stock" });
                    var urlMatch = urlRegex.Match(raw);

                    if (!string.IsNullOrWhiteSpace(modelVal) && string.IsNullOrWhiteSpace(dto.ModelName)) dto.ModelName = modelVal;
                    if (!string.IsNullOrWhiteSpace(trimVal) && string.IsNullOrWhiteSpace(dto.TrimName)) dto.TrimName = trimVal;
                    if (!string.IsNullOrWhiteSpace(yearVal) && dto.ModelYear == null)
                    {
                        if (int.TryParse(new string(yearVal.Where(c => char.IsDigit(c)).ToArray()), out var y)) dto.ModelYear = y;
                    }
                    if (!string.IsNullOrWhiteSpace(priceVal) && dto.BasePrice == 0)
                    {
                        var normalized = priceVal.Replace(",", "").Replace("USD", "", StringComparison.InvariantCultureIgnoreCase).Trim();
                        if (decimal.TryParse(normalized, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var p)) dto.BasePrice = p;
                    }
                    if (!string.IsNullOrWhiteSpace(batteryVal) && dto.BatteryCapacity == 0)
                    {
                        if (int.TryParse(new string(batteryVal.Where(c => char.IsDigit(c)).ToArray()), out var b)) dto.BatteryCapacity = b;
                    }
                    if (!string.IsNullOrWhiteSpace(rangeVal) && dto.RangeKM == 0)
                    {
                        if (int.TryParse(new string(rangeVal.Where(c => char.IsDigit(c)).ToArray()), out var rkm)) dto.RangeKM = rkm;
                    }
                    if (!string.IsNullOrWhiteSpace(chargingVal) && dto.ChargingTime == 0)
                    {
                        if (int.TryParse(new string(chargingVal.Where(c => char.IsDigit(c)).ToArray()), out var ct)) dto.ChargingTime = ct;
                    }
                    if (!string.IsNullOrWhiteSpace(speedVal) && dto.TopSpeed == 0)
                    {
                        if (int.TryParse(new string(speedVal.Where(c => char.IsDigit(c)).ToArray()), out var ts)) dto.TopSpeed = ts;
                    }
                    if (!string.IsNullOrWhiteSpace(stockVal) && dto.Stock == 0)
                    {
                        if (int.TryParse(new string(stockVal.Where(c => char.IsDigit(c)).ToArray()), out var st)) dto.Stock = st;
                    }
                    if (urlMatch.Success && string.IsNullOrWhiteSpace(dto.ImageUrl)) dto.ImageUrl = urlMatch.Value;

                    anySet = !string.IsNullOrWhiteSpace(dto.ModelName) || !string.IsNullOrWhiteSpace(dto.TrimName) || dto.BasePrice > 0 || dto.BatteryCapacity > 0 || dto.RangeKM > 0;
                }
                catch
                {
                    // swallow - we'll return null below if still empty
                }
            }

            return anySet ? dto : null;
        }

        public async Task<string> GenerateVehicleSpecAsync(string instruction)
        {
            if (string.IsNullOrWhiteSpace(instruction))
                throw new ArgumentException("Instruction is required.");

            // Gather context from analyzer service
            var vehicles = await _analyzerService.AnalyzeVehiclesAsync();
            var orders = await _analyzerService.AnalyzeSalesAsync();
            var feedbacks = await _analyzerService.AnalyzeFeedbacksAsync();

            // Build a concise context summary to help the assistant
            var vehicleSummary = string.Join("\n", vehicles.Take(20).Select(v =>
                $"{v.TrimName} ({v.ModelName}) - Year:{v.ModelYear} Price:{v.BasePrice} Stock:{v.Stock} Range:{v.RangeKM}km"
            ));

            var salesSummary = string.Join("\n", orders.Take(20).Select(o =>
                $"Order:{o.Id} Items:{o.Items?.Count ?? 0} Total:{o.TotalAmount} Status:{o.Status}"
            ));

            var feedbackSummary = string.Join("\n", feedbacks.Take(20).Select(f =>
                $"FB:{f.Id} Order:{f.OrderId} Resolved:{(f.Resolver != null)}"
            ));

            // Instruct assistant to return a JSON matching CreateVehicleRequestDto
var jsonInstruction = $"""
[System Instruction]
You are an AI Electric Vehicle dealership consultant.
You may recommend vehicles not currently in stock if they fit the customer's demand 
or align with current EV market trends.
[/System Instruction]

Return a single JSON object matching the following schema exactly (no extra fields) to create a new vehicle in inventory:
[CreateVehicleRequestDto]
  "ModelName": string,
  "TrimName": string,
  "ModelYear": integer or null,
  "BasePrice": number,
  "ImageUrl": string,
  "BatteryCapacity": integer,
  "RangeKM": integer,
  "ChargingTime": integer,
  "TopSpeed": integer,
  "Stock": integer,
  "IsActive": boolean
[/CreateVehicleRequestDto]

Context Summary:
Vehicles:\n{vehicleSummary}

Recent Sales:\n{salesSummary}

Feedbacks:\n{feedbackSummary}

Manager Instruction:\n{instruction}

""";

            var response = await _geminiService.GetGeminiResponseAsync(jsonInstruction);
            return response;
        }

        /// <summary>
        /// Helper that generates a vehicle spec from the assistant and immediately attempts
        /// to create the vehicle. Returns the created VehicleResponseDto on success.
        /// </summary>
        public async Task<VehicleResponseDto> GenerateAndCreateVehicleAsync(string instruction)
        {
            if (string.IsNullOrWhiteSpace(instruction))
                throw new ArgumentException("Instruction is required.");

            // Ask the assistant for a spec
            var spec = await GenerateVehicleSpecAsync(instruction);

            // Try to create from spec. CreateVehicleFromSpecAsync already contains JSON extraction
            // and validation logic and will throw informative exceptions if parsing/validation fails.
            var created = await CreateVehicleFromSpecAsync(spec);
            return created;
        }
    }
}
