
using TaxReturns.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace TaxReturns.DAL
{
    public class TaxReturnContext : DbContext
    {

        public TaxReturnContext() : base("kpmgTaxReturnContext")
        {
        }

        public DbSet<AccountTransaction> accountTransactions { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}