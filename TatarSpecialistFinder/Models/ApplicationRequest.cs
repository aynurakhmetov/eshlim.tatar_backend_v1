namespace TatarSpecialistFinder.Models
{
    public enum RequestStatus
    {
        New,
        SentToAdmin,
        Approved,
        Published,
        Closed
    }

    public class ApplicationRequest
    {
        public int Id { get; set; } // уникальный номер заявки
        public string Name { get; set; } = string.Empty; // имя клиента
        public string Contact { get; set; } = string.Empty; // контактные данные
        public string WhatNeeded { get; set; } = string.Empty; // что нужно сделать
        public string WhereNeeded { get; set; } = string.Empty; // где нужно сделать
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // дата создания
        public RequestStatus Status { get; set; } = RequestStatus.New; // статус заявки
    }
}


