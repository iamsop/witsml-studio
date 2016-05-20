﻿//----------------------------------------------------------------------- 
// PDS.Witsml.Studio, 2016.1
//
// Copyright 2016 Petrotechnical Data Systems
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Energistics;
using Energistics.Protocol.ChannelStreaming;
using Energistics.Protocol.Discovery;
using Energistics.Protocol.Store;
using Energistics.Security;
using PDS.Framework;

namespace PDS.Witsml.Studio.Core.Connections
{
    /// <summary>
    /// Provides a connection test for an Ept Connection instance.
    /// </summary>
    /// <seealso cref="PDS.Witsml.Studio.Core.Connections.IConnectionTest" />
    [Export("Etp", typeof(IConnectionTest))]
    public class EtpConnectionTest : IConnectionTest
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(EtpConnectionTest));

        /// <summary>
        /// Determines whether this Connection instance can connect to the specified connection Uri.
        /// </summary>
        /// <param name="connection">The connection instanace being tested.</param>
        /// <returns>The boolean result from the asynchronous operation.</returns>
        public async Task<bool> CanConnect(Connection connection)
        {
            try
            {
                var applicationName = GetType().Assembly.FullName;
                var applicationVersion = GetType().GetAssemblyVersion();
                var headers = Authorization.Basic(connection.Username, connection.Password);

                using (var client = new EtpClient(connection.Uri, applicationName, applicationVersion, headers))
                {
                    client.Register<IChannelStreamingConsumer, ChannelStreamingConsumerHandler>();
                    client.Register<IDiscoveryCustomer, DiscoveryCustomerHandler>();
                    client.Register<IStoreCustomer, StoreCustomerHandler>();

                    var count = 0;
                    client.Open();

                    while (string.IsNullOrWhiteSpace(client.SessionId) && count < 10)
                    {
                        await Task.Delay(1000);
                        count++;
                    }

                    var result = !string.IsNullOrWhiteSpace(client.SessionId);
                    _log.DebugFormat("Etp connection test {0}", result ? "passed" : "failed");

                    return result;
                }
            }
            catch
            {
                _log.Debug("Etp connection test failed");
                return false;
            }
        }
    }
}