using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ollama_chat_api.Models;
using System.Data;
using System.Text.Json;
namespace ollama_chat_api.Controllers

{
    [Route("api/[controller]")]
    [ApiController]
    public class APIController : Controller
    {

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<APIController> _logger;
        public APIController(IConfiguration configuration, HttpClient httpClient , ILogger<APIController> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger; 
        }


        [HttpPost("send")]
        public async Task<IActionResult> sendRequest([FromBody] RequestDTO request)
        {

            if (string.IsNullOrWhiteSpace(request.MyPrompt)) return BadRequest();
            var MsgRequest = new
            {
                model = "qwen3:1.7B",
                prompt = request.MyPrompt,
                stream = false
            };
            var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/generate", MsgRequest);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"ollama error:{error}");
            }
            var jsonResult = await response.Content.ReadAsStringAsync();
            var parsedObject = JsonDocument.Parse(jsonResult);
            string Response = parsedObject.RootElement.GetProperty("response").GetString();


            // SAVE TO DATABASE (ADO.NET)
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            
            var sql = "INSERT INTO  msgs (request_msg , response_msg) VALUES (@request_msg , @response_msg)";
            SqlParameter requestParameter = new SqlParameter
            {
                ParameterName = "@request_msg",
                SqlDbType = SqlDbType.Text,
                Direction = ParameterDirection.Input,
                Value = request.MyPrompt

            };
            SqlParameter responseParameter = new SqlParameter
            {
                ParameterName = "@response_msg",
                SqlDbType = SqlDbType.Text,
                Direction = ParameterDirection.Input,
                Value = Response
            };
          
            try
            {

                using (SqlConnection connection = new SqlConnection(connectionString))
                {

                    SqlCommand sqlCommand = new SqlCommand(sql, connection);
                    sqlCommand.Parameters.Add(requestParameter);
                    sqlCommand.Parameters.Add(responseParameter);
                    sqlCommand.CommandType = CommandType.Text;
                    await  connection.OpenAsync ();
                   await sqlCommand.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException e)
            {
                _logger.LogError(e,"error occured while saving the message");
                return StatusCode(500, "database error"); 
            }
            catch (Exception e)
            {
                _logger.LogError(e , "unexpected error");
                return StatusCode(500, "Unexpected error");
            }
        
                return Ok(Response);

        }
    }
}
