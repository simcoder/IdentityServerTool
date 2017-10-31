using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;

namespace Dev.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AuthModel model)
        {
            // request token
            var tokenClient = new TokenClient($"{model.Authority}/connect/token", model.ClientId, model.ClientSecret);
            var tokenResponse = new TokenResponse(string.Empty);
            switch (model.GrantType)
            {
                case "Resource Owner":
                    tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(model.Username, model.Password, model.Scope);
                    break;
                case "Client Credentials":
                    tokenResponse = await tokenClient.RequestClientCredentialsAsync(model.Scope);
                    break;
            }

          

            if (tokenResponse.IsError)
            {
                return Ok(new ResponseModel
                {
                    Token = tokenResponse.ErrorDescription,
                    IdpStatus = $"{(int)tokenResponse.HttpStatusCode} {tokenResponse.HttpStatusCode}",
                    IdpError = tokenResponse.Error
                });
            }
            if (model.GetTokenOnly)
                return Ok(new ResponseModel
                {
                    IdpStatus = $"{(int) tokenResponse.HttpStatusCode} {tokenResponse.HttpStatusCode}",
                    Token = tokenResponse.AccessToken
                });

            // call api resource
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);
            var response = new HttpResponseMessage();
            switch (model.UrlVerb)
            {
                case "GET":
                    response = await client.GetAsync(model.Url);
                    break;
                case "POST":
                    response = await client.PostAsync(model.Url, new StringContent(model.UrlPayload, Encoding.UTF8, "application/json"));
                    break;
                case "PUT":
                    response = await client.PutAsync(model.Url, new StringContent(model.UrlPayload ?? string.Empty, Encoding.UTF8, "application/json"));
                    break;
                case "DELETE":
                    response = await client.DeleteAsync(model.Url);
                    break;
            }

            if (!response.IsSuccessStatusCode)
            {
                return Ok(new ResponseModel
                {
                    Token = tokenResponse.AccessToken,
                    ApiStatus = $"{(int)response.StatusCode} {response.StatusCode}",
                    IdpStatus = $"{(int)tokenResponse.HttpStatusCode} {tokenResponse.HttpStatusCode}"
                });
            }
            var content = await response.Content.ReadAsStringAsync();
            return Ok(new ResponseModel
            {
                IdpStatus = $"{(int)tokenResponse.HttpStatusCode} {tokenResponse.HttpStatusCode}",
                Token = tokenResponse.AccessToken,
                ApiStatus = $"{(int)response.StatusCode} {response.StatusCode}",
                Response = content
            });
        }

        public class AuthModel
        {
            public string Authority { get; set; }
            public string GrantType { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Scope { get; set; }
            public string ClientSecret { get; set; }
            public string ClientId { get; set; }
            public string Url { get; set; }
            public string UrlVerb { get; set; }
            public string UrlPayload { get; set; }
            public bool GetTokenOnly { get; set; }
        }

        public class ResponseModel
        {
            public string Token { get; set; }
            public string Response { get; set; }
            public string ApiStatus { get; set; }
            public string IdpStatus { get; set; }
            public string IdpError { get; set; }
        }
    }
}