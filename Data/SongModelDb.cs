using Microsoft.EntityFrameworkCore;

class SongModelDb : DbContext
{
    public SongModelDb(DbContextOptions<SongModelDb> options)
        : base(options) { }

    public DbSet<SongModel> Songs { get; set; }
}