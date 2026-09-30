using Microsoft.EntityFrameworkCore;
using System;
using TaskManager.Models;

namespace TaskManager.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks {get; set; }
}