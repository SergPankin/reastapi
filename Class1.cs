using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace my_db
{
    public class Bank
    {
        /*
        [JsonIgnore]
        public string? BRANCH { get; set; }
        [JsonIgnore]
        public string? CENTRE { get; set; }
        [JsonIgnore]
        public string? DISTRICT { get; set; }
        [JsonIgnore]
        public string? STATE { get; set; }
        [JsonIgnore]
        public bool? UPI { get; set; }
        [JsonIgnore]
        public string? MICR { get; set; }
        [JsonIgnore]
        public bool? RTGS { get; set; }
        [JsonIgnore]
        public bool? NEFT { get; set; }
        [JsonIgnore]
        public string? SWIFT { get; set; }
        [JsonIgnore]
        public string? CONTACT { get; set; }
        [JsonIgnore]
        public bool? IMPS { get; set; }
        */
        public string? ADDRESS { get; set; }
        public string? CITY { get; set; }
        public string? BANK { get; set; }
        public string? BANKCODE { get; set; }
        [Key]
        public string? IFSC { get; set; }

    }

    public class ApplicationContext : DbContext
    {
        public DbSet<Bank> Banks => Set<Bank>();
        public ApplicationContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=restapi.db");
        }
    }
}