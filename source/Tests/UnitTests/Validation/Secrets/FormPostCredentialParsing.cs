﻿/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using FluentAssertions;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Validation;
using Microsoft.Owin;
using System.IO;
using System.Text;
using Xunit;

namespace IdentityServer3.Tests.Validation.Secrets
{
    public class FormPostCredentialExtraction
    {
        const string Category = "Secrets - Form Post Secret Parsing";
        IdentityServerOptions _options;
        PostBodySecretParser _parser;

        public FormPostCredentialExtraction()
        {
            _options = new IdentityServerOptions();
            _parser = new PostBodySecretParser(_options);
        }

        [Fact]
        public async void EmptyOwinEnvironment()
        {
            var context = new OwinContext();
            context.Request.Body = new MemoryStream();

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async void Valid_PostBody()
        {
            var context = new OwinContext();

            var body = "client_id=client&client_secret=secret";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Type.Should().Be(Constants.ParsedSecretTypes.SharedSecret);
            secret.Id.Should().Be("client");
            secret.Credential.Should().Be("secret");
        }

        [Fact]
        public async void ClientId_Too_Long()
        {
            var context = new OwinContext();

            var longClientId = "x".Repeat(_options.InputLengthRestrictions.ClientId + 1);
            var body = string.Format("client_id={0}&client_secret=secret", longClientId);

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async void ClientSecret_Too_Long()
        {
            var context = new OwinContext();

            var longClientSecret = "x".Repeat(_options.InputLengthRestrictions.ClientSecret + 1);
            var body = string.Format("client_id=client&client_secret={0}", longClientSecret);

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async void Missing_ClientId()
        {
            var context = new OwinContext();

            var body = "client_secret=secret";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }

        [Fact]
        public async void Missing_ClientSecret()
        {
            var context = new OwinContext();

            var body = "client_id=client";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().NotBeNull();
            secret.Type.Should().Be(Constants.ParsedSecretTypes.NoSecret);
        }

        [Fact]
        public async void Malformed_PostBody()
        {
            var context = new OwinContext();

            var body = "malformed";

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var secret = await _parser.ParseAsync(context.Environment);

            secret.Should().BeNull();
        }
    }
}