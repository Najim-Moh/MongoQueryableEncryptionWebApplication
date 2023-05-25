using Microsoft.Extensions.Options;
using MongoCSFLEWebApplication.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Encryption;
using MongoWebApplication.Models;
using MongoWebApplication.Service;

namespace MongoCSFLEWebApplication.Service
{
    public class PatientsQuerableService
    {
        private readonly IMongoCollection<Patient> _patientCollection;
        private readonly ILogger<PatientsService> _logger;
        private readonly IOptions<MedicalRecordsStoreDatabaseSettings> _medicalRecordsDatabaseSettings;


        public PatientsQuerableService(
       IOptions<MedicalRecordsStoreDatabaseSettings> medicalRecordsDatabaseSettings, ILogger<PatientsService> logger)
        {
            _medicalRecordsDatabaseSettings = medicalRecordsDatabaseSettings;
            var mongoClient = new MongoClient(
                medicalRecordsDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                medicalRecordsDatabaseSettings.Value.DatabaseName);

            _patientCollection = mongoDatabase.GetCollection<Patient>(
                medicalRecordsDatabaseSettings.Value.PatientsCollectionName);
            _logger = logger;
        }

        public async Task CreateAsync(Patient newPatient)
        {
            await Insert(newPatient);

            _logger.LogInformation($"Patient id: {newPatient.Id}");
        }

        public async Task<List<Patient>> GetAsync() =>
    await _patientCollection.Find(_ => true).ToListAsync();

        public async Task<Patient?> GetAsync(string id) =>
            await _patientCollection.Find(x => x.Id == id).FirstOrDefaultAsync();



        public async Task Insert(Patient newPatient)
        {
            var credentials = new YourCredentials().GetCredentials();
            var connectionString = credentials["MONGODB_URI"];
            // start-key-vault


            var keyVaultNamespace = CollectionNamespace.FromFullName("encryption.__keyVault");
            // end-key-vault

            var coll = _medicalRecordsDatabaseSettings.Value.PatientsCollectionName;
            var db = _medicalRecordsDatabaseSettings.Value.DatabaseName;

            // start-kmsproviders
            var kmsProviders = new Dictionary<string, IReadOnlyDictionary<string, object>>();
            const string provider = "local";
            const string localMasterKeyPath = "master-key.txt";
            var localMasterKeyBase64Read = File.ReadAllText(localMasterKeyPath);
            var localMasterKeyBytes = Convert.FromBase64String(localMasterKeyBase64Read);
            var localOptions = new Dictionary<string, object>
            {
                {"key", localMasterKeyBytes}
            };
            kmsProviders.Add(provider, localOptions);
            // end-kmsproviders



            var regularClient = new MongoClient(connectionString);
            var keyVaultCollection = regularClient.GetDatabase(keyVaultNamespace.DatabaseNamespace.DatabaseName)
                .GetCollection<BsonDocument>(keyVaultNamespace.CollectionName);


            //Get Key from Keyvault
            BsonBinaryData GetKeyId(string altName)
            {
                var filter = Builders<BsonDocument>.Filter.Eq<BsonString>("keyAltNames", altName);
                var document = keyVaultCollection.Find(filter).FirstOrDefault();

                if (document != null)
                {
                    return document["_id"].AsBsonBinaryData;
                }
                else
                {
                    // Handle the case when the document is not found
                    // You can return a default value or throw an exception based on your requirements
                    // For example, return null or throw an InvalidOperationException
                    throw new InvalidOperationException($"Key document with altName '{altName}' not found.");
                }
            }
            var dataKeyId1 = GetKeyId("demo-data-key");

            // start-schema
            var encryptedCollectionNamespace = CollectionNamespace.FromFullName("medicalRecords.patients");


            var encryptedFieldsMap = new Dictionary<string, BsonDocument>
      {
       {
        encryptedCollectionNamespace.FullName, new BsonDocument
        {
            {
                "fields", new BsonArray
                {
                    new BsonDocument
                    {
                        {"keyId", dataKeyId1},
                        {"path", new BsonString("ssn")},
                        {"bsonType", new BsonString("int")},
                        {
                            "queries", new BsonDocument
                            {
                                {"queryType", new BsonString("equality")}
                            }
                        }
                    },
                    new BsonDocument
                    {
                        {"keyId", dataKeyId1},
                        {"path", new BsonString("bloodType")},
                        {"bsonType", new BsonString("string")},
                    },
                    new BsonDocument
                    {
                        {"keyId", dataKeyId1},
                        {"path", new BsonString("medicalRecords")},
                        {"bsonType", new BsonString("object")}
                    },
                    new BsonDocument
                    {
                        {"keyId", dataKeyId1},
                        {"path", new BsonString("insurance.policyNumber")},
                        {"bsonType", new BsonString("string")}
                    },
                    new BsonDocument
                    {
                        {"keyId", dataKeyId1},
                        {"path", new BsonString("insurance.provider")},
                        {"bsonType", new BsonString("string")}
                    }
                }
            }
        }
    }
};
            // end-schema

            // start-extra-options
            var extraOptions = new Dictionary<string, object>()
            {
                {"cryptSharedLibPath", credentials["SHARED_LIB_PATH"]},
            };
            // end-extra-options


            // start-client
            var autoEncryptionOptions = new AutoEncryptionOptions(
                keyVaultNamespace,
                kmsProviders,
                encryptedFieldsMap: encryptedFieldsMap,
                extraOptions: extraOptions
                );


            var clientSettings = MongoClientSettings.FromConnectionString(connectionString);

            clientSettings.AutoEncryptionOptions = autoEncryptionOptions;
            var secureClient = new MongoClient(clientSettings);
            // end-client

            // Construct an auto-encrypting client
             var secureCollection = secureClient.GetDatabase(db).GetCollection<Patient>(coll);


            // Insert a document into the collection
            await secureCollection.InsertOneAsync(newPatient);

            // end-insert

        }

        public async Task GetPatients()
        {
            var connectionString = _medicalRecordsDatabaseSettings.Value.ConnectionString;
            // start-key-vault
            var keyVaultNamespace = CollectionNamespace
                .FromFullName($"{_medicalRecordsDatabaseSettings.Value.KeyVaultDatabaseName}.{_medicalRecordsDatabaseSettings.Value.KeyVaultCollectionName}");

            var coll = _medicalRecordsDatabaseSettings.Value.PatientsCollectionName;
            var db = _medicalRecordsDatabaseSettings.Value.DatabaseName;
            var dbNamespace = $"{db}.{coll}";


            var regularClientSettings = MongoClientSettings.FromConnectionString(connectionString);
            var regularClient = new MongoClient(regularClientSettings);
            var regularCollection = regularClient.GetDatabase(db).GetCollection<BsonDocument>(coll);

            // start-find
            Console.WriteLine("Finding a document with regular (non-encrypted) client.");

            //Document should be read as BSON document, else the encrypted properties with Binary data will cause
            //Serialization exception
            var unSecurefilter = Builders<BsonDocument>.Filter.Eq("name", "UB K");
            var unSecureResult = regularCollection.Find(unSecurefilter).Limit(1);

            if (null != unSecureResult && unSecureResult.CountDocuments() == 1)
            {
                var regularResultItem = unSecureResult.ToList()[0];
                Console.WriteLine($"\n{regularResultItem}\n");
            }



            var clientSettings = MongoClientSettings.FromConnectionString(connectionString);
            
            var secureClient = new MongoClient(clientSettings);
            // end-client

            // Construct an auto-encrypting client
            var secureCollection = secureClient.GetDatabase(db).GetCollection<Patient>(coll);


            Console.WriteLine("Finding a document with encrypted client, searching on an encrypted field");
            //Filter to find an item within encrypted data
            var secureFilter = Builders<Patient>.Filter.Eq("name", "UB K");
            var secureResult = secureCollection.Find(secureFilter).Limit(1);

            if (null != secureResult && secureResult.CountDocuments() == 1)
            {
                var secureResultItem = secureResult.FirstOrDefault();
                Console.WriteLine($"\n{secureResultItem}\n");
            }


        }


    }
}
