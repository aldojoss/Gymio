namespace Gymio.DTOs
{
    public class TransaccionFinancieraDto
    {
        public DateTime Fecha { get; set; }
        public string Concepto { get; set; } = "";
        public decimal Monto { get; set; }
        public bool EsIngreso { get; set; }
    }
}
