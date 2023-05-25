using MongoCSFLEWebApplication.Models;
using MongoDB.Bson;
using MongoDB.Driver.Encryption;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using MongoWebApplication.Models;
using MongoWebApplication.Service;

namespace MongoCSFLEWebApplication.Service
{
    public class SetupQueryableService
    {

        public async Task CreateAsync()
        {
            await CreateKey();
        }


        public async Task CreateKey()
        {

            var credentials = new YourCredentials().GetCredentials();
            // start-local-cmk
            using (var randomNumberGenerator = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var bytes = new byte[96];
                randomNumberGenerator.GetBytes(bytes);
                var localMasterKeyBase64Write = Convert.ToBase64String(bytes);
                File.WriteAllText("master-key.txt", localMasterKeyBase64Write);
                // end-local-cmk

                // start-kmsproviders
                var kmsProviders = new Dictionary<string, IReadOnlyDictionary<string, object>>();
                const string provider = "local";
                var localMasterKeyBase64Read = File.ReadAllText("master-key.txt");
                var localMasterKeyBytes = Convert.FromBase64String(localMasterKeyBase64Read);
                var localOptions = new Dictionary<string, object>
            {
                {"key", localMasterKeyBytes}
            };
                kmsProviders.Add(provider, localOptions);
                // end-kmsproviders


                // start-create-index
                var connectionString = credentials["MONGODB_URI"];
                var keyVaultNamespace = CollectionNamespace.FromFullName("encryption.__keyVault");
                var keyVaultClient = new MongoClient(connectionString);
                var indexOptions = new CreateIndexOptions<BsonDocument>
                {
                    Unique = true,
                    PartialFilterExpression = new BsonDocument
                    {{"keyAltNames", new BsonDocument {{"$exists", new BsonBoolean(true)}}}}
                };
                var builder = Builders<BsonDocument>.IndexKeys;
                var indexKeysDocument = builder.Ascending("keyAltNames");
                var indexModel = new CreateIndexModel<BsonDocument>(indexKeysDocument, indexOptions);
                var keyVaultDatabase = keyVaultClient.GetDatabase(keyVaultNamespace.DatabaseNamespace.DatabaseName);
                // Drop the Key Vault Collection in case you created this collection
                // in a previous run of this application.
                keyVaultDatabase.DropCollection(keyVaultNamespace.CollectionName);
                var keyVaultCollection = keyVaultDatabase.GetCollection<BsonDocument>(keyVaultNamespace.CollectionName);
                keyVaultCollection.Indexes.CreateOne(indexModel);
                // end-create-index


                // start-create-dek
                var clientEncryptionOptions = new ClientEncryptionOptions(
                    keyVaultClient,
                    keyVaultNamespace,
                    kmsProviders: kmsProviders
                    );
                var clientEncryption = new ClientEncryption(clientEncryptionOptions);
                var dataKeyOptions1 = new DataKeyOptions(alternateKeyNames: new List<string> { "demo-data-key" });

                BsonBinaryData CreateKeyGetID(DataKeyOptions options)
                {
                    var dateKeyGuid = clientEncryption.CreateDataKey(provider, options, CancellationToken.None);
                    return new BsonBinaryData(dateKeyGuid, GuidRepresentation.Standard);
                }

                var dataKeyId1 = CreateKeyGetID(dataKeyOptions1);
                // end-create-dek

                // Convert the value to a valid UUID format
                BsonBinaryData GetKeyId(string altName)
                {
                    var filter = Builders<BsonDocument>.Filter.Eq<BsonString>("keyAltNames", altName);
                    return keyVaultCollection.Find(filter).First<BsonDocument>()["_id"].AsBsonBinaryData;
                }

            }

        }

    }
}
