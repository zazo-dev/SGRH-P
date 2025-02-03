using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SGRH.Web.Models.Entities;

namespace SGRH.Web.Models.Data
{
    public class SgrhContext : IdentityDbContext<User>
    {
        public SgrhContext(DbContextOptions<SgrhContext> options) : base(options)
        {
        }
        
        public DbSet<Absence> Absences { get; set; }
        public DbSet<AbsenceCategory> AbsenceCategories { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Document> Document { get; set; }
        public DbSet<Dossier> Dossiers { get; set; }
        public DbSet<Overtime> Overtimes { get; set; }
        public DbSet<Layoff> Layoffs { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<PayrollPeriod> PayrollPeriod { get; set; }
        public DbSet<PersonalAction> PersonalActions { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Settlement> Settlements { get; set; }
        public DbSet<Vacation> Vacations { get; set; }
        public DbSet<Warning> Warnings { get; set; }
        public DbSet<WorkPeriod> WorkPeriod { get; set; }
    }
}
