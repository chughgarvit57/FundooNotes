using Microsoft.EntityFrameworkCore;
using ModelLayer.Entity;

namespace RepoLayer.Context
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options) { }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<NoteEntity> Notes { get; set; }
        public DbSet<CollabEntity> Collaborator { get; set; }
        public DbSet<LabelEntity> Labels { get; set; }
        public DbSet<LabelNoteEntity> NoteLabels { get; set; }
    }
}
