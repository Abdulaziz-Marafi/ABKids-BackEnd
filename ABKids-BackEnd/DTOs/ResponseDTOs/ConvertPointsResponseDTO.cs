namespace ABKids_BackEnd.DTOs.ResponseDTOs
{
    public class ConvertPointsResponseDTO
    {
        public int PointsConverted { get; set; }
        public decimal MoneyReceived { get; set; }
        public int RemainingPoints { get; set; }
        public decimal NewBalance { get; set; }
        public string? Message { get; set; }
    }
}
