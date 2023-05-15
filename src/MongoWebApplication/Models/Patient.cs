using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace MongoWebApplication.Models;

public class Patient
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [BsonElement("ssn")]
    [JsonPropertyName("ssn")]
    public int SSN { get; set; }
    
    [BsonElement("bloodType")]
    [JsonPropertyName("bloodType")]
    public string BloodType { get; set; } = null!;

    [BsonElement("medicalRecords")]
    [JsonPropertyName("medicalRecords")]
    public MedicalRecords MedicalRecords { get; set; } = null!;

    [BsonElement("insurance")]
    [JsonPropertyName("insurance")]
    public Insurance Insurance { get; set; } = null!;

}

public class MedicalRecords
{
    [BsonElement("weight")]
    [JsonPropertyName("weight")]
    public decimal Weight { get; set; }

    [BsonElement("bloodPressure")]
    [JsonPropertyName("bloodPressure")]
    public string BloodPressure { get; set; } = null!;

}

public class Insurance
{
    [BsonElement("policyNumber")]
    [JsonPropertyName("policyNumber")]
    public string PolicyNumber { get; set; } = null!;

    [BsonElement("provider")]
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = null!;
}