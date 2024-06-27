using Microsoft.EntityFrameworkCore;
using prova.models;

namespace prova.data
{
    public class ServicoContext : DbContext
    {
        public ServicoContext(DbContextOptions<ServicoContext> options) : base(options) { }

        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
    }
}
