namespace TatarSpecialistFinder.Models
{
    public class RequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Contact { get; set; } = "";
        public string WhatNeeded { get; set; } = "";
        public string WhereNeeded { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "";
    }
}
