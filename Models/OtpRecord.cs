using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AceBackend.Models
{
    public class OtpRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("identifier")]
        public string Identifier { get; set; } = string.Empty; // email

        [BsonElement("otp")]
        public string Otp { get; set; } = string.Empty;

        [BsonElement("purpose")]
        public string Purpose { get; set; } = string.Empty; // registration, passwordReset

        [BsonElement("attempts")]
        public int Attempts { get; set; } = 0;

        [BsonElement("isVerified")]
        public bool IsVerified { get; set; } = false;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        [BsonElement("lastResendAt")]
        public DateTime? LastResendAt { get; set; }
    }
}
