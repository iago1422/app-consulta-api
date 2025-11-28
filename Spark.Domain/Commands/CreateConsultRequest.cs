using System;
using Flunt.Notifications;
using Flunt.Validations;
using Microsoft.AspNetCore.Http;
using Spark.Domain.Commands.Contracts;

namespace Spark.Domain.Commands
{
    public class CreateConsultRequest
    {
        public Guid UserId { get; set; } // paciente
        public Guid DoutorId { get; set; }
    }

    public class CreateConsultResponse
    {
        public Guid ConsultId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
    }

    public class ConsultDto
    {
        public Guid ConsultId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public string Status { get; set; }
    }

    public class SdpRequest
    {
        public Guid UserId { get; set; }
        public string Type { get; set; } // "offer" | "answer"
        public string Sdp { get; set; }
    }

    public class IceCandidateRequest
    {
        public Guid UserId { get; set; }
        public string Candidate { get; set; }
    }

    public class ChatMessageRequest
    {
        public Guid UserId { get; set; }
        public string Message { get; set; }
    }

    public class ChatMessageResponse
    {
        public Guid UserId { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
    }
}