using AutoFeed_Backend_DAO.Models;
using AutoFeed_Backend_Repositories.BasicRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFeed_Backend_Repositories.Repositories
{
    public class FeedingSessionRepository : GenericRepository<FeedingSession>
    {
        public FeedingSessionRepository() : base() { }

        public FeedingSessionRepository(AutoFeedDBContext context) : base(context) { }
    }
}
