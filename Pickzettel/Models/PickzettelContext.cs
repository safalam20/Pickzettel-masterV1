using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Pickzettel.Models
{
    public partial class PickzettelContext : DbContext
    {
        public PickzettelContext()
        {
        }

        public PickzettelContext(DbContextOptions<PickzettelContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Pickzettel_Form> PickZettelForms { get; set; }
        public virtual DbSet<Pickzettel_HData> PickzettelHDatas { get; set; }
        public virtual DbSet<Pickzettel_HDataDetail> PickzettelHDataDetails { get; set; }
        public virtual DbSet<Pickzettel_IData> PickzettelIDatas { get; set; }     
        public virtual DbSet<CheckFunction> GetCheckFunction { get; set; }
        public virtual DbSet<UserPrinter> UserPrinters { get; set; }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
