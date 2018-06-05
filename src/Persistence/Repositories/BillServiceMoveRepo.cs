﻿using Phony.Kernel.Repositories;
using Phony.Model;

namespace Phony.Persistence.Repositories
{
    public class BillServiceMoveRepo : Repository<BillServiceMove>, IBillServiceMoveRepo
    {
        public BillServiceMoveRepo(PhonyDbContext context) : base(context)
        {
        }

        public PhonyDbContext PhonyDbContext
        {
            get { return Context as PhonyDbContext; }
        }
    }
}