using DataLinker.Models;
using DataLinker.Services.Identities;
using IdentityModel.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DataLinker.IdsService
{
    public class IdsApiService : IIdsApiService
    {
        private static readonly string BaseUrl = ConfigurationManager.AppSettings["IdentityServerHost"];
        private string _tokenEndpoint = BaseUrl + "/core/connect/token";
        private string _apiEndpoint = BaseUrl + "/api/";

        private static readonly string ClientId = ConfigurationManager.AppSettings["IdentityServerClientID"];
        private static readonly string ClientSecret = ConfigurationManager.AppSettings["IdentityServerClientSecret"];

        async Task<TokenResponse> RequestToken()
        {
            // create the token client
            var client = new TokenClient(_tokenEndpoint, ClientId, ClientSecret); // the client secret... Remove it

            var result = await client.RequestClientCredentialsAsync("server.admin");
            // return to caller
            return result;
        }

        async Task<T> MakeRequest<T>(string uri)
        {
            // get an access token
            var token = await RequestToken();
            // turn url into a uri.
            var apiUri = new Uri(_apiEndpoint);
            // create http client
            var client = new HttpClient
            {
                BaseAddress = apiUri
            };
            // use extension method to set token
            client.SetBearerToken(token.AccessToken);
            // get response as a string
            var response = await client.GetStringAsync(uri);
            // convert back to objects
            var results = JsonConvert.DeserializeObject<T>(response);
            // return to caller
            return results;
        }
        
        async Task<TResponse> MakeRequestPut<TResponse>(string uri, object body)
        {
            // get an access tokenvar token = await RequestToken();
            var token = await RequestToken();
            // create http client
            var client = new HttpClient
            {
                BaseAddress = new Uri(_apiEndpoint)
            };

            // use extension method to set token
            client.SetBearerToken(token.AccessToken);
            // convert object to json
            var json = JsonConvert.SerializeObject(body);
            // create a content object for request
            var content = new StringContent(json, Encoding.Unicode, "application/json");
            // make request
            var response = await client.PutAsync(uri, content);
            // get result
            var result = response;
            var hello = await result.Content.ReadAsStringAsync();
            // convert back to object
            var results = JsonConvert.DeserializeObject<TResponse>(hello);
            // return to caller
            return results;
        }

        async Task<TResponse> MakeRequestPost<TResponse>(string uri, object body)
        {
            // get an access token
            var token = await RequestToken();
            // create http client
            var client = new HttpClient
            {
                BaseAddress = new Uri(_apiEndpoint)
            };
            // use extension method to set token
            client.SetBearerToken(token.AccessToken);
            // convert object to json
            var json = JsonConvert.SerializeObject(body);
            // create a content object for request
            var content = new StringContent(json, Encoding.Unicode, "application/json");
            // make request
            var response = await client.PostAsync(uri, content);
            // get result
            var result = await response.Content.ReadAsStringAsync();
            // throw error if response is not successfull
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(result);
            }
            // convert back to object
            var results = JsonConvert.DeserializeObject<TResponse>(result);
            // return to caller
            return results;
        }

        public async Task<IdentityUser> CreateUser(string username, string plaintextPassword, string firstName, string phone)
        {
            // Setup user details
            var newUser = new IdentityUser
            {
                Username = username,
                PlaintextPassword = plaintextPassword,
                FirstName = username,
                LastName = username,
                PhoneNumber = phone,
                Email = username,
                Active = true,
                Claims = new List<Claim>()
            };

            // Save new user
            return await MakeRequestPost<IdentityUser>("accounts/", newUser);
        }

        public async Task ChangeEmail(string subject, string emailAddress)
        {
            // Change username
            var newUserName = new
            {
                Id = subject,
                NewUsername = emailAddress
            };

            // Update username
            await MakeRequestPost<IdentityUser>($"accounts/{subject}/request/change-username", newUserName);

            // Change email address
            var newEmail = new
            {
                Id = subject,
                NewEmailAddress = emailAddress
            };

            // Update email address
            await MakeRequestPost<bool>($"accounts/{subject}/request/change-email", newEmail);
        }

        public async Task EditUser(string id, string firstName, string phone, bool active)
        {
            // Get user
            var user = await MakeRequest<IdentityUser>("accounts/" + id);

            // Update users details
            user.FirstName = firstName;
            user.PhoneNumber = phone;
            user.Active = active;

            await MakeRequestPut<IdentityUser>("accounts/" + user.Id, user);
        }
    }
}
