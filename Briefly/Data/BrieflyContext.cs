using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Briefly;

namespace Briefly.Data;

public class BrieflyContext : DbContext
{
    public BrieflyContext (DbContextOptions<BrieflyContext> options)
        : base(options)
    {
    }

    public DbSet<Briefly.BlogPost> BlogPost { get; set; } = default!;
    public DbSet<PublishedMessage> PublishedMessages { get; set; } = default!;
}
