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
        public APIController(IConfiguration configuration , HttpClient httpClient) {
           _configuration = configuration;
            _httpClient = httpClient;
        }
        

        [HttpPost("send")]
        public async Task<IActionResult> sendRequest([FromBody] RequestDTO request)
        {
            
            if (string.IsNullOrWhiteSpace( request.MyPrompt)) return BadRequest();
            var MsgRequest = new
            {
                model = "qwen3:1.7B",
                prompt = request.MyPrompt,
                stream = false
            };
            var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/generate", MsgRequest);
            if (!response.IsSuccessStatusCode) { 
            var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, $"ollama error:{error}");
            }
            var jsonResult = await response.Content.ReadAsStringAsync();
            var parsedObject = JsonDocument.Parse(jsonResult);
            string Response = parsedObject.RootElement.GetProperty("response").GetString();


            // SAVE TO DATABASE (ADO.NET)
            string connectionString =_configuration.GetConnectionString("DefaultConnection");
            SqlConnection connection = new SqlConnection(connectionString); 
                var sql = "INSERT INTO  msgs (request_msg , response_msg) VALUES " + $"(@request_msg , @response_msg)";
            SqlParameter requestParameter = new SqlParameter {
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
            SqlCommand sqlCommand = new SqlCommand(sql , connection );
            sqlCommand.Parameters.Add( requestParameter );
            sqlCommand.Parameters.Add( responseParameter );
            sqlCommand.CommandType = CommandType.Text;
            connection.Open();
            sqlCommand.ExecuteNonQuery();
            connection.Close();
            // API Returns the response 

            return Ok(Response);
        
        }

    }
}
