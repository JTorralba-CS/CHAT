﻿//OK

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Claims;

using Blazored.SessionStorage;

using Standard.Functions;
using Standard.Models;

namespace Portal.Services
{
    public class AuthenticationStateService : AuthenticationStateProvider
    {
        private static IConfigurationRoot? Configuration;

        private readonly string? PassPhrase;

        private readonly ISessionStorageService SessionStorageService;

        private readonly CryptoService CryptoService;

        private readonly StateService StateService;
        
        private readonly ChatService ChatService;

        public AuthenticationStateService(ISessionStorageService sessionStorageService, StateService stateService, ChatService chatService)
        {
            Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            PassPhrase = Configuration["PassPhrase"];

            SessionStorageService = sessionStorageService;

            CryptoService = new CryptoService();

            StateService = stateService;

            ChatService = chatService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsIdentity claimsIdentity;

            ClaimsPrincipal claimsPrinciple;

            string userCache = await SessionStorageService.GetItemAsync<string>(".");

            if (userCache != null)
            {
                try
                {
                    User user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(await CryptoService.Decrypt(CryptoService.StringToByte(userCache), PassPhrase));

                    while (!ChatService._HubConnected && StateService.IsInitialService)
                    {
                    }
                    
                    await ChatService.HubConnection.SendAsync("SendRequestLogin", ChatService.Connection, user);

                    claimsIdentity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name,  user.ID.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.ID.ToString())
                    }, "apiauth_type");
                }
                catch (Exception e)
                {
                    await SessionStorageService.RemoveItemAsync(".");

                    claimsIdentity = new ClaimsIdentity();

                    Core.WriteError($"Portal AuthenticationStateService.cs GetAuthenticationAsync() Exception: {e.Message}");
                }
            }
            else
            {
                claimsIdentity = new ClaimsIdentity();
            }

            claimsPrinciple = new ClaimsPrincipal(claimsIdentity);

            return await Task.FromResult(new AuthenticationState(claimsPrinciple));
        }

        public async Task MarkUserAsAuthenticated(User user)
        {
            byte[]? userCache = await CryptoService.Encrypt(Newtonsoft.Json.JsonConvert.SerializeObject(user), PassPhrase);

            await SessionStorageService.SetItemAsync(".", BitConverter.ToString(userCache));

            ClaimsIdentity claimsIdentity = new (new[]
            {
                new Claim(ClaimTypes.Name, user.ID.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString())
            }, "apiauth_type");

            ClaimsPrincipal claimsPrinciple = new (claimsIdentity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrinciple)));
        }

        public async Task MarkUserAsLoggedOut()
        {
            await SessionStorageService.RemoveItemAsync(".");

            ClaimsIdentity claimsIdentity = new ();

            ClaimsPrincipal claimsPrinciple = new (claimsIdentity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrinciple)));
        }
    }
}