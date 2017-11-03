﻿using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using poolranking_players_api.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace poolranking_players_api.Data
{
    public class DataClient
    {
        private DocumentClient _client;

        public DataClient()
        {
            _client = new DocumentClient(new Uri(Constants.EndpointUri), Constants.PrimaryKey);
        }

        public async Task<Player> CreatePlayerIfNotExists(Player player)
        {
            if (player.Id != null)
            {
                try
                {
                    var foundPlayer = await _client.ReadDocumentAsync(
                         UriFactory.CreateDocumentUri(Constants.databaseName, Constants.collectionName, player.Id));

                    return JsonConvert.DeserializeObject<Player>(foundPlayer.Resource.ToString());
                }
                catch (DocumentClientException de)
                {
                    if (de.StatusCode == HttpStatusCode.NotFound)
                    {
                        var createdPlayerWithId = await _client.CreateDocumentAsync(
                              UriFactory.CreateDocumentCollectionUri(Constants.databaseName, Constants.collectionName), player);

                        return JsonConvert.DeserializeObject<Player>(createdPlayerWithId.Resource.ToString());
                    }

                    throw;
                }
            }

            var newPlayer =
                await _client.CreateDocumentAsync(
                    UriFactory.CreateDocumentCollectionUri(Constants.databaseName, Constants.collectionName), player);

            return JsonConvert.DeserializeObject<Player>(newPlayer.Resource.ToString());
        }

        public async Task<Player> GetPlayer(string id)
        {
            return await _client.ReadDocumentAsync<Player>(
                    UriFactory.CreateDocumentUri(Constants.databaseName, Constants.collectionName, id));
        }

        public async Task<List<Player>> GetPlayers()
        {
            IDocumentQuery<Player> query = _client.CreateDocumentQuery<Player>(
                UriFactory.CreateDocumentCollectionUri(Constants.databaseName, Constants.collectionName),
                new FeedOptions { MaxItemCount = -1 })
                .AsDocumentQuery();

            List<Player> results = new List<Player>();

            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<Player>());
            }

            return results;
        }
        public async Task<Player> UpdatePlayer(Player player)
        {
            await _client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(Constants.databaseName, Constants.collectionName, player.Id), player);

            return await _client.ReadDocumentAsync<Player>(UriFactory.CreateDocumentUri(Constants.databaseName, Constants.collectionName, player.Id));
        }
    }
}