namespace TatarSpecialistFinder.Models
{
    public class CreateRequestDto
    {
        public string Name { get; set; } = string.Empty;       // имя клиента
        public string Contact { get; set; } = string.Empty;    // контакт
        public string WhatNeeded { get; set; } = string.Empty; // что нужно сделать
        public string WhereNeeded { get; set; } = string.Empty; // где нужно сделать
    }
}

