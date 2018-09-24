// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Http
{
    internal class DefaultTypedHttpClientFactory<TClient> : ITypedHttpClientFactory<TClient>
    {
        private readonly Cache _cache;
        private readonly IServiceProvider _services;

        public DefaultTypedHttpClientFactory(Cache cache, IServiceProvider services)
        {
            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            _cache = cache;
            _services = services;
        }

        public TClient CreateClient(HttpClient httpClient)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            return (TClient)_cache.HttpClientActivator(_services, new object[] { httpClient });
        }

        public TClient CreateClient(HttpMessageHandler httpMessageHandler)
        {
            if (httpMessageHandler == null)
            {
                throw new ArgumentNullException(nameof(httpMessageHandler));
            }

            return (TClient)_cache.HttpMessageHandlerActivator(_services, new object[] { httpMessageHandler });
        }

        // The Cache should be registered as a singleton, so it that it can
        // act as a cache for the Activator. This allows the outer class to be registered
        // as a transient, so that it doesn't close over the application root service provider.
        public class Cache
        {
            private readonly static Func<ObjectFactory> _createHttpClientActivator = () => ActivatorUtilities.CreateFactory(typeof(TClient), new Type[] { typeof(HttpClient) });
            private readonly static Func<ObjectFactory> _createHttpMessageHandlerActivator = () => ActivatorUtilities.CreateFactory(typeof(TClient), new Type[] { typeof(HttpMessageHandler) });

            private ObjectFactory _httpClientActivator;
            private bool _httpClientInitialized;
            private object _httpClientLock;

            public ObjectFactory HttpClientActivator => LazyInitializer.EnsureInitialized(
                ref _httpClientActivator, 
                ref _httpClientInitialized, 
                ref _httpClientLock, 
                _createHttpClientActivator);

            private ObjectFactory _httpMessageHandlerActivator;
            private bool _httpMessageHandlerInitialized;
            private object _httpMessageHandlerLock;

            public ObjectFactory HttpMessageHandlerActivator => LazyInitializer.EnsureInitialized(
                ref _httpMessageHandlerActivator, 
                ref _httpMessageHandlerInitialized, 
                ref _httpMessageHandlerLock, 
                _createHttpMessageHandlerActivator);
        }
    }
}
