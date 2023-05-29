namespace MongoWebApplication.Models;

public class MedicalRecordsStoreDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string PatientsCollectionName { get; set; } = null!;

    public string KeyVaultDatabaseName { get; set; } = null!;

    public string KeyVaultCollectionName { get; set; } = null!;

    public string DataEncryptionKeyAltName { get; set; } = null!;

    public string LocalMasterKeyBase64 { get; set; } = null!;
}