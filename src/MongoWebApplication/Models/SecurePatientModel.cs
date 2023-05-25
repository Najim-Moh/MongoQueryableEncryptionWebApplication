using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace MongoWebApplication.Models;

public class SecurePatientModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [BsonElement("ssn")]
    [JsonPropertyName("ssn")]
    [BsonRepresentation(BsonType.Binary)]
    public object SSN { get; set; } = null!;
    
    [BsonElement("bloodType")]
    [JsonPropertyName("bloodType")]
    [BsonRepresentation(BsonType.Binary)]
    public object BloodType { get; set; } = null!;

    [BsonElement("medicalRecords")]
    [JsonPropertyName("medicalRecords")]
    [BsonRepresentation(BsonType.Binary)]
    public object MedicalRecords { get; set; } = null!;

    [BsonElement("insurance")]
    [JsonPropertyName("insurance")]
    public Insurance InsuranceMongoModel { get; set; } = null!;    
}

public class MedicalRecordsMongoModel
{
    [BsonElement("weight")]
    [JsonPropertyName("weight")]
    public decimal Weight { get; set; }

    [BsonElement("bloodPressure")]
    [JsonPropertyName("bloodPressure")]
    public string BloodPressure { get; set; } = null!;

}

public class InsuranceMongoModel
{
    [BsonElement("policyNumber")]
    [JsonPropertyName("policyNumber")]
    //[BsonRepresentation(BsonType.Binary)]
    public object PolicyNumber { get; set; } = null!;

    [BsonElement("provider")]
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = null!;
}