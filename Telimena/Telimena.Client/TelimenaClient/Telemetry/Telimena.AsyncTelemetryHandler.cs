﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TelimenaClient
{
    public partial class Telimena
    {
        /// <summary>
        ///     Asynchronous Telimena methods
        /// </summary>
        internal class AsyncTelemetryHandler : IAsyncTelemetryHandler
        {
            /// <summary>
            ///     Asynchronous Telimena methods
            /// </summary>
            public AsyncTelemetryHandler(Telimena telimena, TelemetryProcessingPipeline pipeline)
            {
                this.telimena = telimena;
                this.pipeline = pipeline;
            }

            private readonly Telimena telimena;
            private readonly TelemetryProcessingPipeline pipeline;

            /// <inheritdoc />
            public async Task<TelemetryItem> View(string viewName, Dictionary<string, object> telemetryData = null)
            {
                TelemetryItem item = new TelemetryItem(viewName, TelemetryItemTypes.View, this.telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData, telemetryData);
                await this.pipeline.Process(item).ConfigureAwait(false);
                return item;
            }

            /// <inheritdoc />
            public async Task<TelemetryItem> Event(string eventName, Dictionary<string, object> telemetryData = null)
            {
                TelemetryItem item = new TelemetryItem(eventName, TelemetryItemTypes.Event, this.telimena.Properties.StaticProgramInfo.PrimaryAssembly.VersionData, telemetryData);
                await this.pipeline.Process(item).ConfigureAwait(false);
                return item;
            }

            /// <inheritdoc />
            public async Task<TelemetryInitializeResponse> Initialize(Dictionary<string, object> telemetryData = null)
            {
                TelemetryInitializeRequest request = null;
                try
                {
                    request = new TelemetryInitializeRequest(this.telimena.Properties.TelemetryKey)
                    {
                        ProgramInfo = this.telimena.Properties.StaticProgramInfo
                        , TelimenaVersion = this.telimena.Properties.TelimenaVersion
                        , UserInfo = this.telimena.Properties.UserInfo
                    };
                    TelemetryInitializeResponse response = await this.telimena.Messenger.SendPostRequest<TelemetryInitializeResponse>(ApiRoutes.Initialize, request).ConfigureAwait(false);
                  
                    await this.telimena.LoadLiveData(response).ConfigureAwait(false);

                    if (response != null && response.Exception == null)
                    {
                        this.telimena.IsInitialized = true;
                        this.telimena.initializationResponse = response;
                        return this.telimena.initializationResponse;
                    }
                    else
                    {
                        return response;
                    }
                }

                catch (Exception ex)
                {
                    TelimenaException exception = new TelimenaException("Error occurred while sending registration request", this.telimena.Properties, ex
                        , new KeyValuePair<Type, object>(typeof(TelemetryUpdateRequest), request));
                    if (!this.telimena.Properties.SuppressAllErrors)
                    {
                        throw exception;
                    }

                    return new TelemetryInitializeResponse {Exception = exception};
                }
            }

            
        }
        
    }
}