namespace prova.models
{
    public class Cliente
    {
        public int Id { get; set; }
        public String Nome { get; set; } = String.Empty;
        public List<Contrato> Contratos { get; set; } = new List<Contrato>();
    }
}
