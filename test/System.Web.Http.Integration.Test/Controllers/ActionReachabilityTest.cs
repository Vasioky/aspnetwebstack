﻿// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    public class ActionReachabilityTest
    {
        [Theory]
        [InlineData("GET", "Users", HttpStatusCode.OK, "GetUser")]
        [InlineData("UPDATE", "Users", HttpStatusCode.OK, "PutUser")]
        [InlineData("DELETE", "Users", HttpStatusCode.OK, "Remove")]
        [InlineData("OPTIONS", "Users", HttpStatusCode.OK, "Assist")]
        [InlineData("PUT", "Users", HttpStatusCode.OK, "PutUserWithEmptyName")]
        [InlineData("GET", "ParameterTest", HttpStatusCode.OK, "Get(-1)")]
        [InlineData("GET", "ParameterTest?id=2", HttpStatusCode.OK, "Get(2)")]
        [InlineData("POST", "ParameterTest", HttpStatusCode.OK, "POST(null)")]
        [InlineData("POST", "ParameterTest?id=myId", HttpStatusCode.OK, "POST(myId)")]
        [InlineData("Put", "ParameterTest?id=1&value=myvalue", HttpStatusCode.OK, "Put(1, myvalue)")]
        [InlineData("DELETE", "ParameterTest?id=1", HttpStatusCode.NoContent, "")]
        [InlineData("POST", "Users", HttpStatusCode.InternalServerError, "")] // InternalServerError because of ambiguous match, there're multiple POST actions given that every action is POST by default
        [InlineData("POST", "Users/Approve", HttpStatusCode.NotFound, "")] // NotFound because it doesn't match the route
        [InlineData("DELETE", "Users/Remove", HttpStatusCode.NotFound, "")] // NotFound because it doesn't match the route
        [InlineData("POST", "Users/DefaultActionWithEmptyActionName", HttpStatusCode.NotFound, "")] // NotFound because it doesn't match the route
        [InlineData("DELETE", "ParameterTest", HttpStatusCode.NotFound, "")] // NotFound because Delete requires 'id' as parameter
        [InlineData("Put", "ParameterTest", HttpStatusCode.NotFound, "")] // NotFound because Put requires 'id' and 'value' as parameters
        [InlineData("Put", "ParameterTest?id=1", HttpStatusCode.NotFound, "")] // NotFound because Put requires 'id' and 'value' as parameters
        public void ActionReachability_UsingResourceOrientedRoute(string httpMethod, string requestUrl, HttpStatusCode expectedStatusCode, string expectedActionName)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("REST", "{controller}");
            HttpServer server = new HttpServer(config);
            HttpClient client = new HttpClient(server);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/" + requestUrl);

            HttpResponseMessage response = client.SendAsync(request).Result;

            Assert.Equal(expectedStatusCode, response.StatusCode);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Assert.Equal(expectedActionName, response.Content.ReadAsAsync<string>().Result);
            }
        }

        [Theory]
        [InlineData("GET", "Users/GetUser", HttpStatusCode.OK, "GetUser")]
        [InlineData("POST", "Users/Approve", HttpStatusCode.OK, "Approve")]
        [InlineData("UPDATE", "Users/PutUser", HttpStatusCode.OK, "PutUser")]
        [InlineData("POST", "Users/UpdateUser", HttpStatusCode.OK, "PostUser")]
        [InlineData("PATCH", "Users/ReplaceUser", HttpStatusCode.OK, "DeleteUser")]
        [InlineData("DELETE", "Users/Remove", HttpStatusCode.OK, "Remove")]
        [InlineData("POST", "Users/Reject", HttpStatusCode.OK, "Deny")]
        [InlineData("OPTIONS", "Users/Help", HttpStatusCode.OK, "Assist")]
        [InlineData("GET", "ParameterTest/Get", HttpStatusCode.OK, "Get(-1)")]
        [InlineData("GET", "ParameterTest/Get?id=2", HttpStatusCode.OK, "Get(2)")]
        [InlineData("POST", "ParameterTest/post", HttpStatusCode.OK, "POST(null)")]
        [InlineData("POST", "ParameterTest/post?id=myId", HttpStatusCode.OK, "POST(myId)")]
        [InlineData("Put", "ParameterTest/put?id=1&value=myvalue", HttpStatusCode.OK, "Put(1, myvalue)")]
        [InlineData("DELETE", "ParameterTest/Delete?id=1", HttpStatusCode.NoContent, "")]
        [InlineData("POST", "Users/GetUser", HttpStatusCode.MethodNotAllowed, "")] // MethodNotAllowed because the convention implies it's a GET
        [InlineData("GET", "Users", HttpStatusCode.NotFound, "")] // NotFound because it doesn't match the route
        [InlineData("PUT", "Users", HttpStatusCode.NotFound, "")] // NotFound because it doesn't match the route
        [InlineData("PUT", "Users/PutUserWithEmptyName", HttpStatusCode.NotFound, "")] // NotFound because the action has an empty name and it's not reachable through {action}
        [InlineData("POST", "Users/PostUser", HttpStatusCode.NotFound, "")] // NotFound because the action name is renamed to UpdateUser
        [InlineData("GET", "Users/Approve", HttpStatusCode.MethodNotAllowed, "")] // MethodNotAllowed because only POST is allowed by default for action that has no HttpMethd declared or implied
        [InlineData("POST", "Users/Remove", HttpStatusCode.MethodNotAllowed, "")] // MethodNotAllowed because the action has the attribute HttpDelete
        [InlineData("POST", "Users/DefaultActionWithEmptyActionName", HttpStatusCode.NotFound, "")] // NotFound because the action has an empty name and it's not reachable through {action}
        [InlineData("DELETE", "ParameterTest/Delete", HttpStatusCode.NotFound, "")] // NotFound because Delete requires 'id' as parameter
        [InlineData("Put", "ParameterTest/put", HttpStatusCode.NotFound, "")] // NotFound because Put requires 'id' and 'value' as parameters
        [InlineData("Put", "ParameterTest/put?id=1", HttpStatusCode.NotFound, "")] // NotFound because Put requires 'id' and 'value' as parameters
        public void ActionReachability_UsingRpcStyleRoute(string httpMethod, string requestUrl, HttpStatusCode expectedStatusCode, string expectedActionName)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("RPC", "{controller}/{action}");
            HttpServer server = new HttpServer(config);
            HttpClient client = new HttpClient(server);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/" + requestUrl);

            HttpResponseMessage response = client.SendAsync(request).Result;

            Assert.Equal(expectedStatusCode, response.StatusCode);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Assert.Equal(expectedActionName, response.Content.ReadAsAsync<string>().Result);
            }
        }

        [Theory]
        [InlineData("GET", "Users/GetUser", HttpStatusCode.OK, "GetUser")]
        [InlineData("POST", "Users/Approve", HttpStatusCode.OK, "Approve")]
        [InlineData("UPDATE", "Users/PutUser", HttpStatusCode.OK, "PutUser")]
        [InlineData("POST", "Users/UpdateUser", HttpStatusCode.OK, "PostUser")]
        [InlineData("PATCH", "Users/ReplaceUser", HttpStatusCode.OK, "DeleteUser")]
        [InlineData("DELETE", "Users/Remove", HttpStatusCode.OK, "Remove")]
        [InlineData("POST", "Users/Reject", HttpStatusCode.OK, "Deny")]
        [InlineData("OPTIONS", "Users/Help", HttpStatusCode.OK, "Assist")]
        [InlineData("POST", "Users", HttpStatusCode.OK, "DefaultActionWithEmptyActionName")]
        [InlineData("PUT", "Users", HttpStatusCode.OK, "PutUserWithEmptyName")]
        [InlineData("GET", "Users", HttpStatusCode.MethodNotAllowed, "")] // MethodNotAllowed because GetUser doesn't have the ActionName="" so it's not reachable
        [InlineData("UPDATE", "Users", HttpStatusCode.MethodNotAllowed, "")] // MethodNotAllowed because GetUser doesn't have the ActionName="" so it's not reachable
        [InlineData("DELETE", "Users", HttpStatusCode.MethodNotAllowed, "Remove")] // MethodNotAllowed because GetUser doesn't have the ActionName="" so it's not reachable
        [InlineData("OPTIONS", "Users", HttpStatusCode.MethodNotAllowed, "")] // MethodNotAllowed because GetUser doesn't have the ActionName="" so it's not reachable
        [InlineData("PUT", "Users/PutUserWithEmptyName", HttpStatusCode.NotFound, "")] // NotFound because the action has an empty name and it's not reachable through {action}
        [InlineData("POST", "Users/GetUser", HttpStatusCode.MethodNotAllowed, "")] // MethodNotAllowed because the convention implies it's a GET
        [InlineData("POST", "Users/PostUser", HttpStatusCode.NotFound, "")] // NotFound because the action name is renamed to UpdateUser
        [InlineData("GET", "Users/Approve", HttpStatusCode.MethodNotAllowed, "")] // MethodNotAllowed because only POST is allowed by default for action that has no HttpMethd declared or implied
        [InlineData("POST", "Users/Remove", HttpStatusCode.MethodNotAllowed, "")] // MethodNotAllowed because the action has the attribute HttpDelete
        [InlineData("POST", "Users/DefaultActionWithEmptyActionName", HttpStatusCode.NotFound, "")] // NotFound because the ActionName="" and no HttpMethd is declared or implied
        public void ActionReachability_UsingResourceAndRpcStyleRoutes(string httpMethod, string requestUrl, HttpStatusCode expectedStatusCode, string expectedActionName)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Hybrid", "{controller}/{action}", new { action = "" });
            HttpServer server = new HttpServer(config);
            HttpClient client = new HttpClient(server);
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/" + requestUrl);

            HttpResponseMessage response = client.SendAsync(request).Result;

            Assert.Equal(expectedStatusCode, response.StatusCode);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Assert.Equal(expectedActionName, response.Content.ReadAsAsync<string>().Result);
            }
        }
    }
}
