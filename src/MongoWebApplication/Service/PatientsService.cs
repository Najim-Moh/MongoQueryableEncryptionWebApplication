using MongoWebApplication.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Encryption;

namespace MongoWebApplication.Service;

public class PatientsService
{
    private readonly IMongoCollection<Patient> _patientCollection;
    private readonly ILogger<PatientsService> _logger;
    private readonly IOptions<MedicalRecordsStoreDatabaseSettings> _medicalRecordsDatabaseSettings;

    public PatientsService(
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

    public async Task<List<Patient>> GetAsync() =>
        await _patientCollection.Find(_ => true).ToListAsync();

    public async Task<Patient?> GetAsync(string id) =>
        await _patientCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Patient newPatient)
    {
        //For CSFLE with mongocryptd in local docker (MongoDB enterprise in docker as well)
        //Step 1 - Make data key - only once
        //MakeKeyBasicCSFLE();
        //Step - 2 - Insert one document with schema
        await InsertBasicCSFLE(newPatient);
        //await _patientCollection.InsertOneAsync(newBook);       

        _logger.LogInformation($"Patient id: {newPatient.Id}");
    }

    public async Task UpdateAsync(string id, Patient updatedBook) =>
        await _patientCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

    public async Task RemoveAsync(string id) =>
        await _patientCollection.DeleteOneAsync(x => x.Id == id);

    public async Task MakeKeyBasicCSFLE()
    {
        // start-kmsproviders
        var kmsProviders = new Dictionary<string, IReadOnlyDictionary<string, object>>();
        var provider = "local";
        var localMasterKeyBase64Read = "umKvsBgk5vfsQY2qxgAeJY2y/B2CwTK63TdunF2Gv1XjVmjglid8A6JXjblX+UaW3WL59+an5yUuxatEgjJqKmEC6XUciuDdc5dG7k305WkhL7rXvKXzJSRr6J+kTGib";
        var localMasterKeyBytes = Convert.FromBase64String(localMasterKeyBase64Read);
        var localOptions = new Dictionary<string, object>
            {
                { "key", localMasterKeyBytes }
            };
        kmsProviders.Add("local", localOptions);
        // end-kmsproviders

        // start-datakeyopts
        // end-datakeyopts
        // start-create-index
        var connectionString = _medicalRecordsDatabaseSettings.Value.ConnectionString;
        var keyVaultNamespace = CollectionNamespace
            .FromFullName($"{_medicalRecordsDatabaseSettings.Value.KeyVaultDatabaseName}.{_medicalRecordsDatabaseSettings.Value.KeyVaultCollectionName}");
        var keyVaultClient = new MongoClient(connectionString);
        var indexOptions = new CreateIndexOptions<BsonDocument>();
        indexOptions.Unique = true;
        indexOptions.PartialFilterExpression = new BsonDocument { { "keyAltNames", new BsonDocument { { "$exists", new BsonBoolean(true) } } } };
        var builder = Builders<BsonDocument>.IndexKeys;
        var indexKeysDocument = builder.Ascending("keyAltNames");
        var indexModel = new CreateIndexModel<BsonDocument>(indexKeysDocument, indexOptions);
        var keyVaultDatabase = keyVaultClient.GetDatabase(keyVaultNamespace.DatabaseNamespace.ToString());
        // Drop the Key Vault Collection in case you created this collection
        // in a previous run of this application.  
        keyVaultDatabase.DropCollection(keyVaultNamespace.CollectionName);
        // Drop the database storing your encrypted fields as all
        // the DEKs encrypting those fields were deleted in the preceding line.
        keyVaultClient.GetDatabase(_medicalRecordsDatabaseSettings.Value.DatabaseName)
            .DropCollection(_medicalRecordsDatabaseSettings.Value.PatientsCollectionName);
        var keyVaultCollection = keyVaultDatabase.GetCollection<BsonDocument>(keyVaultNamespace.CollectionName.ToString());
        keyVaultCollection.Indexes.CreateOne(indexModel);
        // end-create-index

        // start-create-dek
        var clientEncryptionOptions = new ClientEncryptionOptions(
            keyVaultClient: keyVaultClient,
            keyVaultNamespace: keyVaultNamespace,
            kmsProviders: kmsProviders
            );

        var clientEncryption = new ClientEncryption(clientEncryptionOptions);
        var dataKeyOptions = new DataKeyOptions();
        List<string> keyNames = new List<string>();
        keyNames.Add("demo-data-key");
        var dataKeyId = clientEncryption.CreateDataKey(provider, dataKeyOptions.With(keyNames), CancellationToken.None);
        var dataKeyIdBase64 = Convert.ToBase64String(GuidConverter.ToBytes(dataKeyId, GuidRepresentation.Standard));
        Console.WriteLine($"DataKeyId [base64]: {dataKeyIdBase64}");
        // end-create-dek
    }

    public async Task InsertBasicCSFLE(Patient newPatient)
    {
        var connectionString = _medicalRecordsDatabaseSettings.Value.ConnectionString;
        // start-key-vault
        var keyVaultNamespace = CollectionNamespace
            .FromFullName($"{_medicalRecordsDatabaseSettings.Value.KeyVaultDatabaseName}.{_medicalRecordsDatabaseSettings.Value.KeyVaultCollectionName}");
        // end-key-vault
        var coll = _medicalRecordsDatabaseSettings.Value.PatientsCollectionName;
        var db = _medicalRecordsDatabaseSettings.Value.DatabaseName;
        var dbNamespace = $"{db}.{coll}";

        // start-kmsproviders
        var kmsProviders = new Dictionary<string, IReadOnlyDictionary<string, object>>();
        var provider = "local";

        var localMasterKeyBytes = Convert.FromBase64String(_medicalRecordsDatabaseSettings.Value.LocalMasterKeyBase64);
        var localOptions = new Dictionary<string, object>
            {
                { "key", localMasterKeyBytes }
            };
        kmsProviders.Add(provider, localOptions);
        // end-kmsproviders       

        var regularKVClient = new MongoClient(connectionString);
        var keyVaultCollection = regularKVClient.GetDatabase(keyVaultNamespace.DatabaseNamespace.DatabaseName)
            .GetCollection<BsonDocument>(keyVaultNamespace.CollectionName);

        BsonBinaryData GetKeyId(string altName)
        {
            var filter = Builders<BsonDocument>.Filter.Eq<BsonString>("keyAltNames", altName);
            return keyVaultCollection.Find(filter).First<BsonDocument>()["_id"].AsBsonBinaryData;
        }

        var demoDataKey = GetKeyId("demo-data-key");

        // start-schema
        var schema = new BsonDocument {
                { "bsonType", "object" },
                //UBK: Following requires the key-id value to be set on the object before saving
                //{ "encryptMetadata", new BsonDocument{
                //        { "keyId", "/key-id"}
                //    }
                //},
                //UBK: THis is a better way to load the GUID from db based on alias of the key
                {
                    "encryptMetadata", new BsonDocument("keyId",
                    new BsonArray(new[] {demoDataKey }))
                },
                { "properties", new BsonDocument {
                        { "ssn", new BsonDocument {
                                { "encrypt", new BsonDocument {
                                   // { "keyId", "/key-id"}, //UBK: Optional, if different keys are needed to encrypt different sections
                                    { "bsonType", "int" },
                                    { "algorithm", "AEAD_AES_256_CBC_HMAC_SHA_512-Deterministic"}
                                    }
                                }
                            }
                        },
                        { "bloodType", new BsonDocument {
                            { "encrypt" ,new BsonDocument {
                                    { "bsonType", "string" },
                                    { "algorithm", "AEAD_AES_256_CBC_HMAC_SHA_512-Random"}
                                    }
                                }
                            }
                        },
                        { "medicalRecords", new BsonDocument {
                                {"encrypt", new BsonDocument {
                                    { "bsonType", "object" },
                                    { "algorithm", "AEAD_AES_256_CBC_HMAC_SHA_512-Random"}
                                    }
                                }
                            }
                        },
                        { "insurance", new BsonDocument {
                                { "bsonType", "object" },
                                { "properties", new BsonDocument {
                                        { "policyNumber", new BsonDocument {
                                                { "encrypt", new BsonDocument {
                                                        { "bsonType", "string" },
                                                        { "algorithm", "AEAD_AES_256_CBC_HMAC_SHA_512-Random"}
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        var schemaMap = new Dictionary<string, BsonDocument>();
        schemaMap.Add(dbNamespace, schema);
        // end-schema

        // start-extra-options - NOT REQUIRED for this POC
        //var extraOptions = new Dictionary<string, object>()
        //    {
        //        { "mongocryptdSpawnPath", credentials["MONGOCRYPTD_PATH"]},
        //    };
        // end-extra-options

        var regularClientSettings = MongoClientSettings.FromConnectionString(connectionString);
        var regularClient = new MongoClient(regularClientSettings);
        var regularCollection = regularClient.GetDatabase(db).GetCollection<BsonDocument>(coll);

        // start-client
        var clientSettings = MongoClientSettings.FromConnectionString(connectionString);
        var autoEncryptionOptions = new AutoEncryptionOptions(
            keyVaultNamespace: keyVaultNamespace,
            kmsProviders: kmsProviders,
            schemaMap: schemaMap //TODO: Check if the schema map could be enforces from server 
            //And if providing the schema at client side could be avoided.
            //,extraOptions: extraOptions //NOT REQUIRED FOR THIS POC
            );
        clientSettings.AutoEncryptionOptions = autoEncryptionOptions;
        var secureClient = new MongoClient(clientSettings);
        // end-client

        //TODO: Create a map between Patient View Model and the BSON Document model
        //So that the encrypted fields could be shown with masks (XXXX) from the view model
       

        // Construct an auto-encrypting client
        var secureCollection = secureClient.GetDatabase(db).GetCollection<Patient>(coll);

        if (null == secureCollection)
        {
            throw new Exception($"{dbNamespace} does not exists!");
        }
       
        // Insert a document into the collection
        await secureCollection.InsertOneAsync(newPatient);
        // end-insert

        // start-find

        /*
        Console.WriteLine("Finding a document with regular (non-encrypted) client.");
        
        //Document should be read as BSON document, else the encrypted properties with Binary data will cause
        //Serialization exception
        var unSecurefilter = Builders<BsonDocument>.Filter.Eq("name", "UB K");
        var unSecureResult = regularCollection.Find(unSecurefilter).Limit(1);
        
        if(null != unSecureResult && unSecureResult.CountDocuments() == 1){
            var regularResultItem = unSecureResult.ToList()[0];
            Console.WriteLine($"\n{regularResultItem}\n");
        }

        Console.WriteLine("Finding a document with encrypted client, searching on an encrypted field");
        //Filter to find an item within encrypted data
        var secureFilter = Builders<Patient>.Filter.Eq("name", "UB K");
        var secureResult = secureCollection.Find(secureFilter).Limit(1);

        if(null != secureResult && secureResult.CountDocuments() == 1){
            var secureResultItem = secureResult.FirstOrDefault();
            Console.WriteLine($"\n{secureResultItem}\n");
        }
        */
        // end-find
    }

}